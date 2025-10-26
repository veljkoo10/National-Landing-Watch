import os, shutil, torch, random, numpy as np
from ultralytics import YOLO


# 1️) General settings
DATA_YAML = "/app/dataset/segmentation/data.yaml"
PRETRAINED = "yolo11m-seg.pt"  
IMG_SIZE = 1024
EPOCHS = 80
BATCH_SIZE = 8
WORKERS = 2
PROJECT_OUT = "../outputs/runs"
RUN_NAME = "seg_v11_1024_final"

os.makedirs(PROJECT_OUT, exist_ok=True)
device = 0 if torch.cuda.is_available() else "cpu"
print(f"Using device: {'CUDA' if device == 0 else 'CPU'}")

# Fixing seeds for reproducibility
torch.manual_seed(42)
np.random.seed(42)
random.seed(42)
torch.backends.cudnn.deterministic = True
torch.backends.cudnn.benchmark = False


# 2️) Loading model
model = YOLO(PRETRAINED)


# 3️) Training
results = model.train(
    data=DATA_YAML,
    imgsz=IMG_SIZE,
    epochs=EPOCHS,
    batch=BATCH_SIZE,
    workers=WORKERS,
    device=device,
    project=PROJECT_OUT,
    name=RUN_NAME,
    optimizer="AdamW",
    lr0=0.001,
    lrf=0.01,
    weight_decay=0.0005,
    momentum=0.937,
    patience=30,
    cos_lr=True,
    warmup_epochs=3,
    hsv_h=0.015, hsv_s=0.6, hsv_v=0.4,
    scale=0.4,
    translate=0.1,
    degrees=10,
    mosaic=0.6,
    erasing=0.2,
    mixup=0.1,
    dropout=0.05,
    verbose=True
)


# 4️) Validacija
run_dir = getattr(results, "save_dir", os.path.join(PROJECT_OUT, RUN_NAME))
print(f" Training results: {run_dir}")

try:
    test_results = model.val(
        data=DATA_YAML,
        split="test",
        imgsz=IMG_SIZE,
        device=device,
        project=PROJECT_OUT,
        name=f"{RUN_NAME}_test",
        workers=WORKERS,
        exist_ok=True
    )
    print("Metrics test:", getattr(test_results, "results_dict", {}))
except Exception as e:
    print(f" I'm skipping the test evaluation: {e}")


# 5️) Saving the best weights
best_src = os.path.join(run_dir, "weights", "best.pt")
best_dst = os.path.join(PROJECT_OUT, "seg_best.pt")

if os.path.exists(best_src):
    shutil.copy2(best_src, best_dst)
    print(f" Keeping the best weights: {best_dst}")
else:
    print(" Best.pt not found.")
