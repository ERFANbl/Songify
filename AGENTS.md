# AGENTS.md — Songify

Music streaming app split across three deployables in one repo (no root-level build; each part is built separately):

- `backend/` — ASP.NET Core 8 API, Clean Architecture (`backend/backend.sln`)
- `frontend/` — React 19 + TypeScript + Vite SPA
- `recommender/` — Python 3.11 FastAPI services (song encoder, user vector, made-for-you)
- `docs/` — dated dev notes (backend + recommender). Two of the backend notes are UTF‑16 encoded; decode before diffing.

## Commands

### Backend
```bash
dotnet build backend/backend.sln                     # from repo root
dotnet test  backend/backend.sln                     # only runs backend/tests/WebAPI.Tests
dotnet test  backend/tests/Application.Tests/Application.Tests.csproj   # separate, see gotchas
```
Verified gotchas:
- `backend/tests/Application.Tests` is **not** in `backend.sln` **and does not compile** (missing `using Infrastructure.Interfaces;` / `Infrastructure.Interfaces.Services;`). `dotnet test backend/backend.sln` silently skips it.
- `backend/tests/WebAPI.Tests` (integration, `WebApplicationFactory<Program>`) **fails at host startup unless Postgres is reachable** on `localhost:5433`: Hangfire registers PostgreSQL storage from `ConnectionStrings:DefaultConnection` and `UseHangfireDashboard` resolves that storage during startup. The test factory's in-memory DbContext swap does not cover Hangfire.
- The test host targets `net8.0`; it aborts with "You must install or update .NET" on a machine that only has a .NET 10 runtime. `DOTNET_ROLL_FORWARD=Major dotnet test backend/backend.sln` works around it.
- CI (`.github/workflows/ci-build.yml`) only does restore + build on .NET 8; the test step is commented out.
- EF Core 8. DbContext lives in `src/EntityFrameworkCore`, startup project is `src/WebAPI`:
  ```bash
  cd backend && dotnet ef migrations add <Name> \
      --project src/EntityFrameworkCore --startup-project src/WebAPI
  ```
  Migrations are **not** applied automatically (no `Database.Migrate()` in `Program.cs`); the pgvector tables come from `backend/pg-init/*.sql` instead.

### Frontend
```bash
cd frontend && npm ci     # node_modules is not committed
npm run dev | build | lint      # no test script; `build` = `tsc -b && vite build`
```
- Tailwind v4 is wired through the `@tailwindcss/vite` plugin; theme tokens live in `src/App.css` under `@theme`. No `tailwind.config.js`.
- `tsconfig.app.json` is strict (`noUnusedLocals`, `noUnusedParameters`) — unused imports break `npm run build`.

### Recommender
```bash
cd recommender && docker compose up --build    # encoder:8000, user_vector:8001, made_for_user:8002
```
- Needs **Python 3.10+** (`np.ndarray | None` in `Repositories/UserRepository.py`); the Dockerfile pins `python:3.11-slim`, macOS system python3.9 will fail to import.
- The encoder **cannot boot without** `recommender/EncoderService/check_point/Final_checkpoint` and `check_point/tokenizer` — that path is gitignored and not in the repo. The scalers (`EncoderService/scalers/*.pkl`) *are* committed.
- Entry points differ in import style: `EncoderService/SongEncoder.py` uses relative imports (run as `uvicorn EncoderService.SongEncoder:app` from the repo root), while `MadeForUserService` imports `SongRecommender.SongRecommenderService` absolutely (needs cwd on `sys.path`).
- DB access is via `DATABASE_URL` (default `postgresql://postgres:P%40ssw0rd%212025%23Strong@localhost:5433/AppDb`); compose overrides it to `host.docker.internal:5433`.

## Architecture facts that are not obvious from the file names

**Backend**
- Layering is `Domain → Application → Infrastructure → WebAPI`, but namespaces were deliberately left inconsistent:
  - DTOs sit in `src/Domain/DTOs/**` yet declare `namespace Application.DTOs.Auth` / `Application.DTOs.Song`. Imports depend on that mismatch — don't "fix" the folder/namespace mismatch casually.
  - `IRedisService` is in the Domain project but namespaced `Songify.Domain.Interfaces`.
- DI registration for every repository and service lives in `backend/src/WebAPI/DependencyInjection.cs` (file in WebAPI, `namespace Infrastructure`). Add new services there.
- Repository *and* service interfaces live under `src/Infrastructure/Interfaces/` (`Services/` subfolder); service *implementations* are in `src/Application/Services/`, repository implementations in `src/Infrastructure/Repositories/`.
- `backend/src/API/Program.cs` is orphaned: no `.csproj`, not in the solution. Ignore it.
- Two auth mechanisms coexist:
  - `[Authorize]` → JWT bearer validation (secret/issuer/audience in `appsettings.json` → `JwtSettings`).
  - Custom `[Authenticate]` filter (`WebAPI/AuthLayer/AuthenticateAttribute.cs`) → looks the token up in the DB so revoked tokens fail, and exposes the id via `HttpContext.Items["UserId"]`.
  - The `Song`, `Stream`, `MadeForUser` and `UserVector` controllers take `userId` from the **route** and carry no `[Authorize]`.
- The ML logic lives in the Python services; .NET only proxies multipart form posts to them. Hosts/ports come from `RecommenderServices:{EncoderService,UserVectorService,MadeForUserService}` in `appsettings.json`.
- `MadeForUserService.CallApiMadeForUser` posts to an empty URL — no endpoint is configured, so the weekly Hangfire job (`Cron.Weekly(Sunday, 23)`, registered in `Program.cs`, dashboard at `/hangfire`) cannot reach the recommender today.
- Hangfire uses PostgreSQL storage in schema `hangfire`.

**Vector dimensionality is 17 everywhere** — pgvector columns `vector(17)`, the regression head `out_dim=17`, user-vector `reshape((1,17))`. Changing it touches the .NET DTOs, the Python repositories and `backend/pg-init/init-vector.sql` together. The `docs/recommender/*` notes that mention 18 labels are outdated.

**Frontend**
- API base URL is hardcoded to `http://127.0.0.1:5143` in `src/services/api.ts`, but the API binds `http://127.0.0.1:4000` — `Program.cs` calls `builder.WebHost.UseUrls(...)`, which overrides both `launchSettings.json` and `ASPNETCORE_HTTP_PORTS`. Align one of them or the SPA cannot reach the API.
- The JWT is kept in `localStorage.token`; the axios request interceptor attaches `Authorization: Bearer <token>`.

## Local environment / ops

- Ports (docker compose maps): Postgres `5433→5432`, Redis `6380→6379`, API `8080/8081`.
- `appsettings.json` expects the host-run form (`localhost:5433`, `redis:6380`); compose overrides them with env vars (`ConnectionStrings__DefaultConnection`, `Redis__Configuration`).
- Postgres is `pgvector/pgvector:pg17`; `backend/pg-init/init-vector.sql` creates the `vector` extension and the two embedding tables, and runs only on first container init.
- Secrets (DB password, JWT signing key, S3 access keys) are committed in `appsettings.json` — local runs depend on them being present, not on user-secrets.
- Song audio is stored in ArvanCloud S3 (`S3:*` config, `AWSSDK.S3` v4 with a custom `Endpoint`) and served by `api/Stream/{fileKey}` with HTTP range requests.
- Serilog writes to `logs/log.txt` (rolling daily, relative to the working directory).
