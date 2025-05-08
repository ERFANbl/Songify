Thought for a second


# Song Representation & Multi-Label Regression System

This repository implements a **multimodal** system that fuses **song lyrics**, **audio features**, and **metadata** into a single joint embedding and predicts 18 continuous song‐level labels. The architecture is end‑to‑end trainable (including the LLM) and may evolve over time.

---

## 📖 Architecture Overview

```
[Start]
┌──────────────┐   ┌───────────────┐   ┌───────────────┐
│ Song Lyrics  │   │  Audio File   │   │   Metadata    │
└──────┬───────┘   └──────┬────────┘   └──────┬────────┘
       │                 │                   │
       ▼                 ▼                   ▼
[LLM Embedding]   [Extract Audio Features]   [Process Metadata]
   (1024‑d)             (22‑d)                 (~8‑d)
       │                 │                   │
       └──────────┬──────┴───────┬───────────┘
                  ▼
   [Fusion Layer]  (concatenate all three vectors)
                  │
                  ▼
   [MLP – Feature Projection] (compact shared embedding)
                  │
                  ▼
[Encoder Network (Fully Connected Layers)]  
                  │
                  ▼
[Regression Head → 18 continuous targets]
                  │
                [End]
```

---

## 🔍 Component Details

1. **Lyrics Encoder**

   * **Model**: `BAAI/bge-large-en-v1.5` (OpenAI‑compatible embedding LLM)
   * **Input**: Raw song lyrics
   * **Output**: 1024‑dim semantic embedding (CLS token)

2. **Audio Feature Encoder**

   * **Input features (22‑dim)**:

     ```
     zcr_mean, rms_mean, centroid_mean, bw_mean, flat_mean,
     mfcc1_mean, mfcc2_mean, chroma_mean,
     zcr_std,  rms_std,  centroid_std,  bw_std,  flat_std,
     mfcc1_std, mfcc2_std, chroma_std,
     mfcc1_dmean, mfcc2_dmean,
     tempo, tempo_conf, beat_int_mean, beat_int_std
     ```
   * **Processing**: Small MLP (e.g., Linear→ReLU→Dropout)
   * **Output**: 22‑dim → projected to match fusion space

3. **Metadata Encoder**

   * **Fields**:

     * `genre` (7 categories: country, jazz, blues, hip hop, pop, reggae, rock)
     * `release_date` (year, MinMax‑normalized)
   * **Processing**: Ordinal or one‑hot encode genre + scale date → small MLP
   * **Output**: \~8‑dim vector

4. **Fusion Layer**

   * **Operation**: Concatenate `[lyrics_vector ‖ audio_vector ‖ metadata_vector]` → combined dim \~ (1024 + 22 + 8)
   * **Role**: Merge modalities into a single feature vector

5. **MLP – Feature Projection**

   * **Purpose**: Project the high‑dim fused vector to a compact embedding (e.g., 512‑d)
   * **Structure**: One or two Dense layers with non‑linearities and dropout

6. **Encoder Network**

   * **Purpose**: Further transform the pooled embedding via fully connected layers
   * **Output**: Final shared representation used by both regression head and similarity search

7. **Regression Head**

   * **Output neurons**: 18 continuous targets

     ```
     dating, violence, world/life, night/time, shake the audience,
     family/gospel, romantic, communication, obscene, music,
     movement/places, light/visual perceptions, family/spiritual,
     like/girls, sadness, feelings, danceability
     ```
   * **Loss**: Mean Squared Error (MSE) or Huber loss

---

## ⚙️ Data Flow & Training

1. **Precompute** audio features for each song and store alongside lyrics and metadata.
2. **During each training step**:

   * Tokenize & encode lyrics via `bge-large-en-v1.5`.
   * Normalize & project audio and metadata through their MLP encoders.
   * Fuse, project, encode, and predict all 18 labels.
3. **End‑to‑end fine‑tuning** includes the LLM weights (via LoRA/adapters for efficiency).

---

## 🚧 Future Extensions

* **Architecture tweaks**: depth/width of MLPs, alternative fusion strategies.
* **Additional modalities**: cover art embeddings, user interaction signals.
* **Multi‑task learning**: auxiliary genre or era classification tasks.

---

> *This documentation may be updated as the system evolves.*
