import os
import io
import numpy as np
import pandas as pd
import torch
import torch.nn as nn
from transformers import AutoTokenizer, AutoModel
from peft import LoraConfig, get_peft_model, PeftModel

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


