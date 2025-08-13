from sqlalchemy import create_engine, Table, Column, TEXT, MetaData
from sqlalchemy.dialects.postgresql import insert
from sqlalchemy import select
from pgvector.sqlalchemy import Vector 
import numpy as np

metadata = MetaData()

song_embeddings = Table(
    "sngf_songembeddings",
    metadata,
    Column("id", TEXT, primary_key=True),
    Column("embedding", Vector(17))
)

class SongEmbeddingRepository:
    def __init__(self, DATABASE_URL):
        self.engine = create_engine(DATABASE_URL)

    def insert_vector(self, id: str, vector: list):
      try:
        with self.engine.begin() as conn:
            stmt = insert(song_embeddings).values(
                id=id,
                embedding=vector
            )
            conn.execute(stmt)
      except Exception as e:
          print(e)

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
                    (song_embeddings.c.vector.op("<->")(target_vector)).label("distance")
                )
                .where(song_embeddings.c.id != id) 
                .order_by("distance")
                .limit(num_neighbors)
            )

            results = conn.execute(stmt).fetchall()

            return [{"id": row[0], "vector": np.array(row[1], dtype=np.float32), "distance": row[2]} for row in results]
        
