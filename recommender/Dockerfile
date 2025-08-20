# syntax=docker/dockerfile:1
FROM python:3.11-slim AS base

ENV PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1 \
    PIP_NO_CACHE_DIR=1

# System deps: build essentials and audio libs for librosa
RUN apt-get update && apt-get install -y --no-install-recommends \
    build-essential \
    ffmpeg \
    libsndfile1 \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY requirements.txt ./
RUN pip install --upgrade pip && pip install -r requirements.txt

# Copy project
COPY . .

# Build arg to choose service
ARG SERVICE=
ENV SERVICE=${SERVICE}

# Default command expects SERVICE to be set to one of: encoder, uservec, madeforuser
# We wire the proper uvicorn entry in docker-compose
CMD ["bash", "-lc", "echo Please set SERVICE build arg and override CMD in compose"] 