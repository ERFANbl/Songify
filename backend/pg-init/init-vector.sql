--Arash Ashrafi

--install PgVector
CREATE EXTENSION IF NOT EXISTS vector;

-- Erfan Balaee
CREATE TABLE SNGF_SongEmbeddings(
  id TEXT PRIMARY KEY,
  embedding vector(17)
);