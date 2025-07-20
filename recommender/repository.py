from sqlalchemy import create_engine, Table, Column, TEXT, MetaData
from sqlalchemy.dialects.postgresql import insert
from pgvector.sqlalchemy import Vector
import numpy as np

metadata = MetaData()

song_embeddings = Table(
    "SNGF_SongEmbeddings",
    metadata,
    Column("id", TEXT, primary_key=True),
    Column("vector", Vector(17))
)

class SongEmbeddingRepository:
    def __init__(self, DATABASE_URL):
        self.engine = create_engine(DATABASE_URL)

    def insert_vector(self, id: int, vector: np.ndarray):
        with self.engine.begin() as conn:
            stmt = insert(song_embeddings).values(
                id=id,
                vector=vector.tolist()
            )
            conn.execute(stmt)
