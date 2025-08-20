import os
import sys
import io
import numpy as np
import pandas as pd
import torch
import torch.nn as nn
import torch.optim as optim
from fastapi import FastAPI, APIRouter, Form
import json
import math

sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..')))
from Repositories.UserRepository import UserEmbeddingRepository
from Repositories.SongRepository import SongEmbeddingRepository

DATABASE_URL = os.getenv("DATABASE_URL", "postgresql://postgres:P%40ssw0rd%212025%23Strong@localhost:5433/AppDb")

import json
import math

def score(weekly_log: str, latest_log: str) -> list:
    weekly = json.loads(weekly_log)
    latest = json.loads(latest_log)
    songs_with_scores = []
    weekly_weights = [1, 0.75, 0.5, 0.25]
    alpha = 1.5  
    beta = 1
    
    for song_key, song_info in latest.items():
        song_id = song_key.split('#')[1]
        score = {"id": song_id}
        result = 0
        
        for i, week_data in enumerate(weekly.values()):
            song_weekly_info = week_data.get(song_key)
            
            if not song_weekly_info:
                continue
                
            try:
                duration_plays = song_info["song_duration"] * song_weekly_info["play_count"]
                if duration_plays == 0:
                    ratio = 0
                else:
                    ratio = song_weekly_info["time_listened"] / duration_plays
                    
                play_count = math.log10(song_weekly_info["play_count"] + 1)
                result += weekly_weights[i] * (play_count + beta * ratio)
                
                score["vectorId"] = song_weekly_info["vectorId"]
                
            except KeyError as e:
                print(f"Missing key {e} in song data")
                continue
                
        score["score"] = result + (alpha * song_info.get("is_liked", 0))
        songs_with_scores.append(score)
        
    return songs_with_scores

def online_update(model, optimizer, criterion, input, labels):

    outputs = model(input)
    loss = criterion(outputs, labels)

    optimizer.zero_grad()
    loss.backward()
    optimizer.step()

    updated_weights = model.weight.data.numpy()
    return updated_weights


app = FastAPI()
router = APIRouter()

class UserVectorController:
    def __init__(self):
        self._userRepo = UserEmbeddingRepository(DATABASE_URL)
        self._songRepo = SongEmbeddingRepository(DATABASE_URL)

    def InitialUserVector(self, vectorId: str):
        self._userRepo.initial_vector(vectorId)

    def UpdateUserVector(
        self, userVectorId: str,
        WeeklyUserInteractions: str = Form(title="WeeklyUserInteractions"),
        latestUserInteractions: str = Form(title="latestUserInteractions")):
            
            songs_with_scores = score(WeeklyUserInteractions, latestUserInteractions)

            songs_with_scores = pd.DataFrame(songs_with_scores)

            song_vectors = self._songRepo.get_vectors_by_ids(songs_with_scores["vectorId"].tolist())

            userVector = self._userRepo.get_vector_by_id(userVectorId)

            weights = torch.from_numpy(userVector.reshape((1,17))).float()

            model = nn.Linear(in_features=17,out_features=1,bias=False).float()

            with torch.no_grad():
                model.weight.data = weights

            criterion = nn.MSELoss()  
            optimizer = optim.Adam(model.parameters(), lr=0.001)

            input = torch.from_numpy(song_vectors).float()
            labels = torch.from_numpy(songs_with_scores["score"].to_numpy()).float()

            updated_vector = online_update(model=model,optimizer=optimizer,criterion=criterion,input=input,labels=labels)

            self._userRepo.set_vector_by_id(id=userVectorId,vector=updated_vector.reshape(17))




controller = UserVectorController()
router.add_api_route("/InitialUserVector/{vectorId}", controller.InitialUserVector, methods=["POST"])
router.add_api_route("/UpdateUserVector/{userVectorId}", controller.UpdateUserVector, methods=["POST"])
app.include_router(router)

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="127.0.0.1", port=8001)