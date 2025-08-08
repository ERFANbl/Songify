from sqlalchemy import create_engine, Table, Column, TEXT, MetaData
from sqlalchemy.dialects.postgresql import insert, select
from pgvector.sqlalchemy import Vector, CosineDistance
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

    def insert_vector(self, id: str, vector: np.ndarray):
        with self.engine.begin() as conn:
            stmt = insert(song_embeddings).values(
                id=id,
                vector=vector.tolist()
            )
            conn.execute(stmt)

    def find_similar_songs(self, id: str, num_neighbors: int) -> list:
        with self.engine.begin() as conn:
            result = conn.execute(
                select(song_embeddings.c.vector).where(song_embeddings.c.id == id)
            ).first()

            if not result or not result[0]:
                return []  

            target_vector = result[0]

            stmt = (
                select(
                    song_embeddings.c.id,
                    song_embeddings.c.vector,
                    CosineDistance(song_embeddings.c.vector, target_vector).label("distance")
                )
                .where(song_embeddings.c.id != id) 
                .order_by("distance")
                .limit(num_neighbors)
            )

            results = conn.execute(stmt).fetchall()

            return [{"id": row[0], "vector": np.array(row[1], dtype=np.float32), "distance": row[2]} for row in results]