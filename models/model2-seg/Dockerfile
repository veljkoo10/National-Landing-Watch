# ============================
# 🔹 Baza: NVIDIA CUDA 12.1 + cuDNN8 (Ubuntu 22.04)
# ============================
FROM nvidia/cuda:12.1.0-cudnn8-runtime-ubuntu22.04

# Onemogući interaktivne promptove (za apt)
ENV DEBIAN_FRONTEND=noninteractive

# ============================
# 🔹 OS paketi i sistemske biblioteke
# ============================
RUN apt-get update && apt-get install -y \
    python3 python3-pip git wget nano unzip \
    libgl1 libglib2.0-0 libsm6 libxext6 libxrender1 \
    && rm -rf /var/lib/apt/lists/*

# ============================
# 🔹 Radni direktorijum
# ============================
WORKDIR /app

# ============================
# 🔹 Kopiranje requirements fajla (ako postoji)
# ============================
COPY requirements.txt .

# ============================
# 🔹 Instalacija Pythona i zavisnosti
# ============================
RUN pip install --upgrade pip

# PyTorch sa CUDA 12.1 buildom
RUN pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu121

# YOLOv8 + alati
RUN pip install ultralytics opencv-python-headless matplotlib pandas seaborn tqdm

# Ako koristiš još nešto u requirements.txt
RUN if [ -f requirements.txt ]; then pip install -r requirements.txt; fi

# ============================
# 🔹 Kopiranje tvog koda u kontejner
# ============================
COPY . .

# ============================
# 🔹 Default CMD (možeš menjati prilikom compose up)
# ============================
CMD ["python3", "src/train_seg.py"]
