import os
import io
import numpy as np
import pandas as pd
import librosa
import joblib
import torch
import torch.nn as nn
from transformers import AutoTokenizer, AutoModel
from peft import LoraConfig, get_peft_model, PeftModel
from fastapi import FastAPI, HTTPException, UploadFile, File, Form
from pydantic import BaseModel
from repository import SongEmbeddingRepository

DATABASE_URL = "postgresql+psycopg2://postgres:P@ssw0rd!2025#Strong@localhost:5432/AppDb"

def extract_audio_features(data: bytes) -> torch.Tensor:
    y, sr = librosa.load(io.BytesIO(data), sr=None)

    zcr       = librosa.feature.zero_crossing_rate(y=y)
    rms       = librosa.feature.rms(y=y)
    centroid  = librosa.feature.spectral_centroid(y=y, sr=sr)
    bw        = librosa.feature.spectral_bandwidth(y=y, sr=sr)
    flatness  = librosa.feature.spectral_flatness(y=y)
    mfccs     = librosa.feature.mfcc(y=y, sr=sr, n_mfcc=13)
    chroma    = librosa.feature.chroma_stft(y=y, sr=sr)
    tempo, beats = librosa.beat.beat_track(y=y, sr=sr)
    onset_env    = librosa.onset.onset_strength(y=y, sr=sr)
    tempo_conf   = float(np.max(onset_env) / (np.mean(onset_env) + 1e-8))
    beat_times   = librosa.frames_to_time(beats, sr=sr)
    intervals    = np.diff(beat_times) if len(beat_times) > 1 else np.array([0.0])

    raw_feats = pd.DataFrame({
        "zcr_mean":      zcr.mean(),
        "rms_mean":      rms.mean(),
        "centroid_mean": centroid.mean(),
        "bw_mean":       bw.mean(),
        "flat_mean":     flatness.mean(),
        "mfcc1_mean":    mfccs[0].mean(),
        "mfcc2_mean":    mfccs[1].mean(),
        "chroma_mean":   chroma.mean(),
        "zcr_std":       zcr.std(),
        "rms_std":       rms.std(),
        "centroid_std":  centroid.std(),
        "bw_std":        bw.std(),
        "flat_std":      flatness.std(),
        "mfcc1_std":     mfccs[0].std(),
        "mfcc2_std":     mfccs[1].std(),
        "chroma_std":    chroma.std(),
        "mfcc1_dmean":   np.diff(mfccs[0]).mean() if mfccs.shape[1] > 1 else 0.0,
        "mfcc2_dmean":   np.diff(mfccs[1]).mean() if mfccs.shape[1] > 1 else 0.0,
        "tempo":         tempo,
        "tempo_conf":    tempo_conf,
        "beat_int_mean": intervals.mean(),
        "beat_int_std":  intervals.std(),
    }, index=[0])

    scaler = joblib.load("audio_scaler.pkl")
    scaled = scaler.transform(raw_feats)
    return torch.tensor(scaled, dtype=torch.float)

def preprocess_metadata(genre: str, release_date: str) -> torch.Tensor:
    genres = ['blues','country','hip hop','jazz','pop','reggae','rock']
    g = genre.lower().strip()
    if g not in genres:
        raise ValueError(f"Unknown genre: {genre}")
    onehot = np.zeros(len(genres) + 1)
    onehot[genres.index(g) + 1] = 1

    try:
        year = int(release_date)
    except ValueError:
        raise ValueError(f"Invalid release_date: {release_date}")

    scaler = joblib.load("release_date_scaler.pkl")
    onehot[0] = scaler.transform([[year,0]])[0][0]
    return torch.tensor(onehot.reshape(1, -1), dtype=torch.float)


class LyricsEncoder(nn.Module):
    _tokenizer_cache = {}
    _base_model_cache = {}

    def __init__(
        self,
        base_path="/content/drive/MyDrive/models/BigBird/",
        lora_path=None,  
        lora_r=16
    ):
        super().__init__()

        self.base_path = base_path
        self.lora_path = lora_path

        if not os.path.isdir(base_path):
            tokenizer = AutoTokenizer.from_pretrained("google/bigbird-roberta-base")
            model     = AutoModel.from_pretrained("google/bigbird-roberta-base")
            os.makedirs(base_path, exist_ok=True)
            tokenizer.save_pretrained(base_path)
            model.save_pretrained(base_path)

        if base_path in LyricsEncoder._tokenizer_cache:
            self.tokenizer = LyricsEncoder._tokenizer_cache[base_path]
        else:
            tok = AutoTokenizer.from_pretrained(base_path)
            LyricsEncoder._tokenizer_cache[base_path] = tok
            self.tokenizer = tok

        if base_path in LyricsEncoder._base_model_cache:
            base_model = LyricsEncoder._base_model_cache[base_path]
        else:
            mdl = AutoModel.from_pretrained(base_path)
            LyricsEncoder._base_model_cache[base_path] = mdl
            base_model = mdl

        if self.lora_path and os.path.isdir(self.lora_path):
            self.model = PeftModel.from_pretrained(base_model, self.lora_path)
        else:
            peft_conf = LoraConfig(
                task_type="FEATURE_EXTRACTION",
                r=lora_r, lora_alpha=32,
                target_modules=["query", "value"]
            )
            self.model = get_peft_model(base_model, peft_conf)

    def forward(self, input_ids, attention_mask):
        out = self.model(input_ids=input_ids,
                         attention_mask=attention_mask,
                         return_dict=True)

        return out.last_hidden_state[:, 0]


class AudioEncoder(nn.Module):
    def __init__(self, in_dim=22, hidden_dim=64):
        super().__init__()
        self.net = nn.Sequential(
            nn.Linear(in_dim, hidden_dim),
            nn.ReLU(),
            nn.Dropout(0.1)
        )
    def forward(self, x): return self.net(x)

class MetadataEncoder(nn.Module):
    def __init__(self, in_dim=8, hidden_dim=32):
        super().__init__()
        self.net = nn.Sequential(
            nn.Linear(in_dim, hidden_dim),
            nn.ReLU(),
            nn.Dropout(0.1),
        )
    def forward(self, x): return self.net(x)


class FusionMLP(nn.Module):
    def __init__(self, dims=(768,64,32), proj_dim=512):
        super().__init__()
        total = sum(dims)
        mid_dim = 700
        self.net = nn.Sequential(
            nn.Linear(total, mid_dim),
            nn.LeakyReLU(),
            nn.Dropout(0.1),

            nn.Linear(mid_dim, proj_dim),
            nn.LeakyReLU(),
        )
    def forward(self, *inputs):
        x = torch.cat(inputs, dim=1)
        return self.net(x)


class EncoderNetwork(nn.Module):
    def __init__(self, in_dim=512, hidden_dim=128):
        super().__init__()
        mid_dim = 256
        self.net = nn.Sequential(
            nn.Linear(in_dim, mid_dim),
            nn.LeakyReLU(),
            nn.Dropout(0.1),

            nn.Linear(mid_dim, hidden_dim),
            nn.LeakyReLU(),
        )
    def forward(self, x): return self.net(x)


class RegressionHead(nn.Module):
    def __init__(self, in_dim=128, out_dim=17):
        super().__init__()
        self.fc = nn.Linear(in_dim, out_dim)
    def forward(self, x): return self.fc(x)


class SongModel(nn.Module):
    def __init__(self):
        super().__init__()
        self.lyrics_enc = LyricsEncoder()
        self.audio_enc  = AudioEncoder()
        self.meta_enc   = MetadataEncoder()
        self.fusion     = FusionMLP()
        self.encoder    = EncoderNetwork()
        self.reg_head   = RegressionHead()

    def forward(self, input_ids, attention_mask, audio_feats, meta_feats):
        L = self.lyrics_enc(input_ids, attention_mask)
        A = self.audio_enc(audio_feats)
        M = self.meta_enc(meta_feats)
        F = self.fusion(L, A, M)
        E = self.encoder(F)
        return self.reg_head(E)


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

@app.post("/process-audio")
async def process_audio(
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

