import os
import numpy as np
import pandas as pd
import librosa
import joblib
import torch
from fastapi import  UploadFile
import tempfile

def extract_audio_features(data: UploadFile) -> torch.Tensor:

    try:
        with tempfile.NamedTemporaryFile(suffix=".mp3", delete=False) as tmp:
            data.file.seek(0)
            tmp.write(data.file.read())
            tmp_path = tmp.name

        y, sr = librosa.load(tmp_path, sr=None, mono=True)

        if np.allclose(y, 0):
                raise ValueError("Decoded audio is silent — file may be corrupted or ffmpeg missing.")

        zcr = librosa.feature.zero_crossing_rate(y=y)
        rms = librosa.feature.rms(y=y)
        centroid  = librosa.feature.spectral_centroid(y=y, sr=sr)
        bw = librosa.feature.spectral_bandwidth(y=y, sr=sr)
        flatness  = librosa.feature.spectral_flatness(y=y)
        mfccs   = librosa.feature.mfcc(y=y, sr=sr, n_mfcc=13)
        chroma        = librosa.feature.chroma_stft(y=y, sr=sr)
        tempo, beats = librosa.beat.beat_track(y=y, sr=sr)
        onset_env        = librosa.onset.onset_strength(y=y, sr=sr)
        tempo_conf   = float(np.max(onset_env) / (np.mean(onset_env) + 1e-8))
        beat_times   = librosa.frames_to_time(beats, sr=sr)
        intervals    = np.diff(beat_times) if len(beat_times) > 1 else np.array([0.0])

        raw_feats = pd.DataFrame({
               "zcr_mean":          zcr.mean(),
               "rms_mean":          rms.mean(),
               "centroid_mean": centroid.mean(),
               "bw_mean":           bw.mean(),
               "flat_mean":         flatness.mean(),
               "mfcc1_mean":        mfccs[0].mean(),
               "mfcc2_mean":        mfccs[1].mean(),
               "chroma_mean":   chroma.mean(),
               "zcr_std":           zcr.std(),
               "rms_std":           rms.std(),
               "centroid_std":  centroid.std(),
               "bw_std":                bw.std(),
               "flat_std":          flatness.std(),
               "mfcc1_std":         mfccs[0].std(),
               "mfcc2_std":         mfccs[1].std(),
               "chroma_std":        chroma.std(),
               "mfcc1_dmean":   np.diff(mfccs[0]).mean() if mfccs.shape[1] > 1 else 0.0,
               "mfcc2_dmean":   np.diff(mfccs[1]).mean() if mfccs.shape[1] > 1 else 0.0,
               "tempo":                 tempo,
               "tempo_conf":        tempo_conf,
               "beat_int_mean": intervals.mean(),
               "beat_int_std":  intervals.std(),
        }, index=[0])

        BASE_DIR = os.path.dirname(os.path.abspath(__file__))
        SCALER_PATH = os.path.join(
        BASE_DIR, "scalers/audio_scaler.pkl"
        )
        scaler = joblib.load(SCALER_PATH)
        scaled = scaler.transform(raw_feats)
    except Exception as e:
        print(e)
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
    
    BASE_DIR = os.path.dirname(os.path.abspath(__file__))
    SCALER_PATH = os.path.join(
    BASE_DIR, "scalers/release_date_scaler.pkl"
                                        )
    scaler = joblib.load(SCALER_PATH)
    onehot[0] = scaler.transform([[year,0]])[0][0]
    return torch.tensor(onehot.reshape(1, -1), dtype=torch.float)