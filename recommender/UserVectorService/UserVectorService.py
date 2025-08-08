import os
import io
import numpy as np
import pandas as pd
import torch
import torch.nn as nn
from fastapi import FastAPI, APIRouter, HTTPException, UploadFile, Form
from Repositories.UserRepository import UserEmbeddingRepository

DATABASE_URL = "postgresql+psycopg2://postgres:P@ssw0rd!2025#Strong@localhost:5432/AppDb"

app = FastAPI()
router = APIRouter()

class UserVectorController:
    def __init__(self):
        self._repo = UserEmbeddingRepository(DATABASE_URL)

    @router.post("/InitialUserVector/{vectorId}")
    async def InitialUserVector(self, vectorId: int):
        self._repo.initial_vector(vectorId)
    

controller = UserVectorController()
router.add_api_route("/InitialUserVector/{vectorId}", controller.InitialUserVector, methods=["POST"])
app.include_router(router)

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="1.1.1.1", port=8001)