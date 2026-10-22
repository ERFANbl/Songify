# Songify

A music streaming platform: users upload songs, the backend encodes each track into a
17-dimensional embedding, and a personalised *"Made for you"* playlist is generated from
those embeddings and the user's listening history.

The system is split into three deployables that live in this repository:

| Part | Stack | Purpose |
|---|---|---|
| `backend/` | ASP.NET Core 8, EF Core, Hangfire, Redis | REST API, auth, metadata, S3 audio streaming |
| `frontend/` | React 19, TypeScript, Vite, Tailwind v4 | SPA client |
| `recommender/` | Python 3.11, FastAPI, PyTorch, transformers | Song encoder, user vector, recommendations |

---

## Features

- **JWT authentication** — signup / login / logout. Tokens are persisted per user so logout
  actually revokes access; Redis caches the token → user-id lookup.
- **Song upload** — audio + metadata (lyrics, genre, release date) is stored in S3-compatible
  object storage and encoded into a 17-dim vector by the encoder service.
- **Listening history & user vectors** — weekly interaction logs are aggregated into a
  per-user vector through an online linear-regression update.
- **Made for you** — a weekly Hangfire job asks the recommender for the songs closest to
  each user's vector and stores the result on the user.
- **Range-request audio streaming** — `api/Stream/{fileKey}` supports HTTP `Range` requests
  for progressive playback.

---

## Architecture

```
┌──────────────┐        ┌───────────────────────────────┐
│   frontend   │  HTTP  │           backend (.NET 8)     │
│  React SPA   │ ─────► │  Controllers → Services →      │
└──────────────┘        │  Repositories → EF Core        │
                        └───────┬───────────────┬────────┘
                                │               │
                     multipart │               │ SQL
                                ▼               ▼
        ┌───────────────────────────────┐   ┌────────────────────┐
        │  recommender (Python/FastAPI) │   │ Postgres + pgvector│
        │  :8000 encoder                │   │  + Redis           │
        │  :8001 user vector            │   └────────────────────┘
        │  :8002 made-for-user          │
        └───────────────────────────────┘
```

The .NET side owns identity, metadata and storage; the Python side owns all ML logic.
The API talks to Python over multipart form posts, and to Postgres/Redis/S3 directly.

### Recommender pipeline

1. `POST /EncodeSong` (audio + genre + releaseDate + lyric) → 22 audio features
   (librosa) + 8 metadata features + BigBird lyrics embedding → fused → **17-dim** vector,
   stored in `SNGF_SongEmbeddings` (pgvector).
2. `POST /UpdateUserVector/{userVectorId}` → weekly logs are scored
   (`log10(play_count)` + listen-ratio, decayed across 4 weeks, + like bonus) and the user's
   17-dim vector is updated with an online gradient step.
3. `POST /MadeForUser` → nearest neighbours of the user's songs are re-ranked by the dot
   product with the user vector.

> The lyrics encoder is `google/bigbird-roberta-base` (chosen over `bge-large-en-v1.5`
> because many lyrics exceed 512 tokens). See `docs/recommender/`.

---

## Repository layout

```
backend/
  backend.sln
  src/
    Domain/              entities, DTOs, mappers, interfaces
    Application/         service implementations (auth, song, stream, vectors)
    Infrastructure/      repositories, Redis, token helpers, service interfaces
    EntityFrameworkCore/ DbContext, model configuration, EF migrations
    WebAPI/              Program.cs, controllers, DI, auth filter, appsettings
  pg-init/               pgvector extension + embedding tables (runs on first init)
  tests/                 WebAPI.Tests (integration), Application.Tests (unit, not in the sln)
  docker-compose.yml     webapi + postgres(pgvector) + redis
frontend/
  src/                   App.tsx, pages/, components/, context/AuthProvider.tsx, services/api.ts
recommender/
  EncoderService/        audio/metadata preprocessing + song encoder (needs a checkpoint)
  UserVectorService/     user vector init + online update
  MadeForUserService/    recommendation endpoint
  SongRecommender/       scoring / nearest-neighbour logic
  Repositories/          SQLAlchemy + pgvector repositories
docs/                    dated development notes (backend + recommender)
```

---

## Prerequisites

- **.NET 8 SDK** (the projects target `net8.0`; a newer SDK can build them, but running
  the tests needs the .NET 8 runtime or `DOTNET_ROLL_FORWARD=Major`)
- **Node.js 18+** and npm
- **Docker** (for Postgres/pgvector, Redis, and the Python services)
- **Python 3.10+** if you run the recommender outside Docker (the Dockerfile pins 3.11)

---

## Getting started

### 1. Infrastructure

```bash
cd backend
docker compose up -d database redis
```

This starts:

| Service | Container port | Host port |
|---|---|---|
| Postgres 17 + pgvector | 5432 | **5433** |
| Redis 7 | 6379 | **6380** |

`backend/pg-init/init-vector.sql` runs on first container initialisation and creates the
`vector` extension plus the `SNGF_SongEmbeddings` / `SNGF_UserEmbeddings` tables
(`vector(17)`).

### 2. Database schema for the .NET entities

The EF migrations are not applied automatically — apply them once:

```bash
cd backend
dotnet ef database update --project src/EntityFrameworkCore --startup-project src/WebAPI
```

This creates `SNGF_User`, `SNGF_Song` and `SNGF_LikedSongs`.

### 3. Backend

```bash
cd backend
dotnet run --project src/WebAPI
```

The API binds to `http://127.0.0.1:4000` (hardcoded via `UseUrls` in `Program.cs`, which
overrides `launchSettings.json` and `ASPNETCORE_HTTP_PORTS`). Swagger UI is served at
`/swagger` in Development, and the Hangfire dashboard at `/hangfire`.

> The frontend currently points at `http://127.0.0.1:5143` (`frontend/src/services/api.ts`).
> Change either side so they match, or the SPA will not reach the API.

### 4. Recommender services (optional, required for upload/recommendation flows)

```bash
cd recommender
docker compose up --build
```

| Service | Port | Entry point |
|---|---|---|
| encoder | 8000 | `EncoderService.SongEncoder:app` |
| user_vector | 8001 | `UserVectorService.UserVectorService:app` |
| made_for_user | 8002 | `MadeForUserService.MadeForUserService:app` |

The encoder loads a fine-tuned checkpoint from `EncoderService/check_point/` (ignored by
git — it must be downloaded separately); the audio and release-date scalers in
`EncoderService/scalers/` are committed.

### 5. Frontend

```bash
cd frontend
npm ci
npm run dev
```

The JWT is kept in `localStorage.token` and attached to every request by an axios
interceptor.

---

## Configuration

`backend/src/WebAPI/appsettings.json` holds the local defaults:

| Key | Value | Notes |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | `Host=localhost;Port=5433;Database=AppDb;...` | Host port of the compose-mapped Postgres |
| `Redis:Configuration` | `redis:6380` | Host port of the compose-mapped Redis |
| `JwtSettings:SecretKey` / `Issuer` / `Audience` | committed dev values | issuer/audience are `https://localhost:5143` |
| `JwtSettings:ExpirationMinutes` | `1440` | one day |
| `S3:AccessKey` / `SecretKey` / `Endpoint` / `BucketName` | ArvanCloud object storage | audio files are stored here |
| `RecommenderServices:*` | host + port of each Python service | `localhost:8000/8001/8002` |

Docker Compose overrides the connection string and Redis host through environment
variables (`ConnectionStrings__DefaultConnection`, `Redis__Configuration`).

The Python services read `DATABASE_URL` (default
`postgresql://postgres:P%40ssw0rd%212025%23Strong@localhost:5433/AppDb`); Compose points
them at `host.docker.internal:5433`.

> **Note:** credentials, the JWT signing key and the S3 keys are committed in
> `appsettings.json`. Replace them before any deployment.

---

## API reference

All routes are prefixed with `api/`.

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `api/Auth/signup` | – | Register; creates the user's vector in the recommender |
| POST | `api/Auth/login` | – | Returns a JWT |
| POST | `api/Auth/logout` | `[Authorize]` | Clears the stored token |
| GET | `api/Auth/profile` | `[Authenticate]` | Current user id |
| POST | `api/Song/Upload/{userId}` | – | Multipart upload (audio + metadata) |
| PATCH | `api/Song/Delete/{userId}/{songId}` | – | Soft delete (`is_deleted`) |
| GET | `api/Song/GetUploadedSongs/{userId}` | – | Metadata of the user's songs |
| GET | `api/Song/GetSong/{userId}/{songId}` | – | Single song metadata |
| PUT | `api/Song/LikeSong/{userId}/{songId}` | – | Like a song |
| GET | `api/Stream/{fileKey}` | – | Audio stream with range support |
| GET | `api/MadeForUser/GetAllRecomendedSongs/{userId}` | – | Weekly recommendations |
| PUT | `api/UserVector/UpdateUserVector/{userId}` | – | Submit weekly listening logs |

Two authorization styles coexist: the standard `[Authorize]` (JWT signature validation)
and a custom `[Authenticate]` action filter (`backend/src/WebAPI/AuthLayer/`) that resolves
the token against the database, so revoked tokens are rejected. It exposes the id as
`HttpContext.Items["UserId"]`.

---

## Testing

```bash
dotnet test backend/backend.sln
```

Covers `backend/tests/WebAPI.Tests` (integration tests through `WebApplicationFactory` with
an in-memory database).

Two caveats:

- The integration tests need a reachable Postgres, because Hangfire resolves its PostgreSQL
  storage during application startup — the in-memory database swap does not cover it.
- `backend/tests/Application.Tests` is not part of `backend.sln` and currently does not
  compile; run it explicitly with
  `dotnet test backend/tests/Application.Tests/Application.Tests.csproj`.

CI (`.github/workflows/ci-build.yml`) restores and builds the solution on .NET 8; the test
step is commented out.

---

## Notes and known gaps

- Passwords are hashed with **SHA-256** without a salt — replace with bcrypt/Argon2 before
  production.
- `MadeForUserService.CallApiMadeForUser` posts to an empty URL, so the weekly Hangfire job
  (`Made-for-user`, Sundays 23:00) cannot reach the recommender service yet.
- Most controllers take `userId` from the route and are not protected by `[Authorize]`.
- `backend/src/API/Program.cs` is leftover scaffolding: it has no project file and is not
  part of the solution.
- The vector width is **17** everywhere (pgvector columns, regression head, user vector).
  The notes in `docs/recommender/` that mention 18 labels predate that change.

## License

MIT — see [LICENSE](LICENSE).
