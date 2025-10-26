# Base
FROM nvidia/cuda:12.1.0-cudnn8-runtime-ubuntu22.04

# OS settings 
ENV DEBIAN_FRONTEND=noninteractive

# Python general alats
RUN apt-get update && apt-get install -y \
    python3 python3-pip git wget nano && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Requiments 
COPY requirements.txt .

# Python i zavisnosti
RUN pip install --upgrade pip && pip install -r requirements.txt

# Kopiranje koda
COPY . .

# Default komande
CMD ["python3", "src/train_cls.py"]
