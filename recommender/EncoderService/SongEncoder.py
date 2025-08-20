import os
import sys
import torch
from transformers import AutoTokenizer
from fastapi import FastAPI, HTTPException, UploadFile, File, Form
from .EncoderModel import SongModel
from .Preprocess import extract_audio_features, preprocess_metadata

sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..')))
from Repositories.SongRepository import SongEmbeddingRepository

DATABASE_URL = os.getenv("DATABASE_URL", "postgresql://postgres:P%40ssw0rd%212025%23Strong@localhost:5433/AppDb")

app = FastAPI()
BASE_DIR = os.path.dirname(os.path.abspath(__file__))
MODEL_CHECKPOINT = os.path.join(
    BASE_DIR, "check_point", "Final_checkpoint"
)

TOKENIZER_CHECKPOINT = os.path.join(
    BASE_DIR, "check_point", "tokenizer"
)
device = torch.device('cuda' if torch.cuda.is_available() else 'cpu')
model = SongModel(MODEL_CHECKPOINT).to(device)
tokenizer = AutoTokenizer.from_pretrained(TOKENIZER_CHECKPOINT)

@app.post("/EncodeSong")
async def EncodeSong(
    audio_file: UploadFile = File(title="audio_file"),
    genre: str = Form(title="genre"),
    releaseDate: str = Form(title="releaseDate"),
    lyric: str = Form(title="lyric"),
    Id: str = Form(title="Id")
):
    try:
        _repo = SongEmbeddingRepository(DATABASE_URL=DATABASE_URL)
        audio_feats = extract_audio_features(audio_file)
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
            print(out, out.size())
        vector = out.squeeze(0).cpu().tolist()

        _repo.insert_vector(id=Id, vector=vector)

        return {"status": "200", "message": "Vector saved successfully"}

    except Exception as e:
        print(e)
        raise HTTPException(status_code=500, detail=str(e))

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="127.0.0.1", port=8000)