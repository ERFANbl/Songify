from sqlalchemy import create_engine, Table, Column, TEXT, MetaData
from sqlalchemy.dialects.postgresql import insert
from pgvector.sqlalchemy import Vector
import numpy as np

metadata = MetaData()

user_embeddings = Table(
    "sngf_userembeddings",
    metadata,
    Column("id", TEXT, primary_key=True),
    Column("embedding", Vector(17))
)

class UserEmbeddingRepository:
    def __init__(self, DATABASE_URL):
        self.engine = create_engine(DATABASE_URL)

    def initial_vector(self, id: str):
        with self.engine.begin() as conn:
            vectors = conn.execute(user_embeddings.select())
            rows = vectors.fetchall()

            if rows:
                vector_array = np.array([row.embedding for row in rows])
                mean_vector = vector_array.mean(axis=0)
            else:
                mean_vector = np.zeros(17, dtype=np.float32)

            stmt = insert(user_embeddings).values(
                id=id,
                embedding=mean_vector.tolist()
            )
            conn.execute(stmt)

    def get_vector_by_id(self, id: str) -> np.ndarray | None:
        with self.engine.begin() as conn:
            query = user_embeddings.select().where(user_embeddings.c.id == id)
            result = conn.execute(query).fetchone()

            if result and result["embedding"] is not None:
                return np.array(result["embedding"], dtype=np.float32)
            else:
                return None
            
    def set_vector_by_id(self, id:str, vector:np.ndarray):
        with self.engine.begin as conn:

            stmt = (
            user_embeddings.update()
            .where(user_embeddings.c.id == id)
            .values(embedding=vector.tolist())
            )

            conn.execute(stmt)