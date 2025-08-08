import os
import io
import numpy as np
import pandas as pd
import torch
import torch.nn as nn
from transformers import AutoTokenizer, AutoModel
from fastapi import FastAPI, HTTPException, UploadFile, File, Form
from Repositories.SongRepository import SongEmbeddingRepository
from EncoderModel import SongModel
from Preprocess import extract_audio_features, preprocess_metadata

DATABASE_URL = "postgresql+psycopg2://postgres:P@ssw0rd!2025#Strong@localhost:5432/AppDb"


def load_model(checkpoint_path: str, device: torch.device = None) -> nn.Module:
    if device is None:
        device = torch.device('cuda' if torch.cuda.is_available() else 'cpu')
    model = SongModel().to(device)
    if not os.path.isfile(checkpoint_path):
        raise FileNotFoundError(f"Checkpoint not found: {checkpoint_path}")
    state = torch.load(checkpoint_path, map_location=device)
    model.load_state_dict(state)
    model.eval()
    return model

app = FastAPI()
MODEL_CHECKPOINT = os.getenv('MODEL_CHECKPOINT', 'step_4475_train0.0025_val0.0057.pt')
model = load_model(MODEL_CHECKPOINT)
tokenizer = AutoTokenizer.from_pretrained("google/bigbird-roberta-base")

@app.post("/EncodeSong")
async def EncodeSong(
    audio_file: UploadFile = File(...),
    genre: str = Form(...),
    releaseDate: str = Form(...),
    lyric: str = Form(...),
    Id: str = Form(None)
):
    try:
        _repo = SongEmbeddingRepository(DATABASE_URL=DATABASE_URL)
        raw = await audio_file.read()
        audio_feats = extract_audio_features(raw)
        meta_feats  = preprocess_metadata(genre, releaseDate)

        tokens = tokenizer(lyric, return_tensors='pt', padding='max_length', truncation=True)
        input_ids = tokens['input_ids']
        attention_mask = tokens['attention_mask']

        device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
        input_ids = input_ids.to(device)
        attention_mask = attention_mask.to(device)
        audio_feats = audio_feats.to(device)
        meta_feats  = meta_feats.to(device)

        with torch.no_grad():
            out = model(input_ids, attention_mask, audio_feats, meta_feats)
        vector = out.squeeze(0).cpu().tolist()

        _repo.insert_vector(id=Id, vector=vector)

        return {"status": "200", "message": "Vector saved successfully"}

    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)

