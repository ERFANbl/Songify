import os
import sys
import io
import numpy as np
import pandas as pd
import torch
import torch.nn as nn
from fastapi import FastAPI, APIRouter

sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..')))
from Repositories.UserRepository import UserEmbeddingRepository
DATABASE_URL = "postgresql://postgres:P%40ssw0rd%212025%23Strong@localhost:5433/AppDb"

app = FastAPI()
router = APIRouter()

class UserVectorController:
    def __init__(self):
        self._repo = UserEmbeddingRepository(DATABASE_URL)

    def InitialUserVector(self, vectorId: str):
        self._repo.initial_vector(vectorId)
    

controller = UserVectorController()
router.add_api_route("/InitialUserVector/{vectorId}", controller.InitialUserVector, methods=["POST"])
app.include_router(router)

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="127.0.0.1", port=8001)