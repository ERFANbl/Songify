# 🔄 Model Architecture Update: Transition to BigBird + LoRA

## 📌 Reason for Switching from `bge-large-en-v1.5` to BigBird

In our initial design, the lyrics encoder was based on `bge-large-en-v1.5`, a dense transformer model optimized for short-to-medium text embedding and semantic retrieval. However, this architecture became limiting when dealing with **long-form song lyrics**, many of which exceed 512 tokens.

### 🧠 Why BigBird?

We switched to **`google/bigbird-roberta-base`** for its ability to handle long input sequences efficiently. BigBird uses **sparse attention** (random, global, and windowed) to scale to sequences up to **4096+ tokens** while maintaining transformer-style performance.

This was essential because:
- Many lyrics contain **line-by-line semantic variation**
- Positional encoding matters more across long sequences
- CLS pooling from shorter models ignored late tokens entirely

In contrast, BigBird provides:
- **Stable representations** of long lyrics
- Compatibility with LoRA tuning (for efficient fine-tuning)
- Strong performance on long-sequence NLP tasks

> ✅ As a result, we gained better semantic coverage of lyrics without truncation.

---

## 🎯 Output Labels: Final Configuration

The final regression head predicts **17 continuous emotion/theme scores**, not 18 as initially planned.

### ✅ Output Heads:
```text
'dating', 'violence', 'world/life', 'night/time', 'shake the audience',
'family/gospel', 'romantic', 'communication', 'obscene', 'music',
'movement/places', 'light/visual perceptions', 'family/spiritual',
'like/girls', 'sadness', 'feelings', 'danceability'
