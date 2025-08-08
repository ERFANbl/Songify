import os
import io
import numpy as np
import pandas as pd
import torch
import torch.nn as nn
from Repositories.UserRepository import UserEmbeddingRepository
from Repositories.SongRepository import SongEmbeddingRepository

DATABASE_URL = "postgresql+psycopg2://postgres:P@ssw0rd!2025#Strong@localhost:5432/AppDb"

_userRepo = UserEmbeddingRepository(DATABASE_URL)
_songRepo = SongEmbeddingRepository(DATABASE_URL)

def RecommendSong(UserVectorId : str, songs : dict, num_recommend : int) -> list:

    recommendedSong = []

    for song in songs :
        recommendedSongVectors = _songRepo.find_similar_songs(song["vectorId"], num_neighbors=num_recommend)
        recommendedSong.extend(recommendedSongVectors)

    recommendedSong = pd.DataFrame(recommendedSong)

    userVector = torch.tensor(_userRepo.get_vector_by_id(UserVectorId), dtype=torch.float32)

    songVectors = torch.tensor(recommendedSong["vector"].tolist(), dtype=torch.float32)

    scores = userVector @ songVectors

    songsWithScores = pd.DataFrame({"id": recommendedSong["id"].tolist(), "scores": scores.tolist()})

    sorted_songs_ids = songsWithScores.sort_values("scores", ascending=False)

    return sorted_songs_ids["id"].tolist()
    
