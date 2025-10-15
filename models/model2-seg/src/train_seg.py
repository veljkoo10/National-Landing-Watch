import os
import shutil
import torch

# YOLOv8 (segmentacija) – instaliraj: pip install ultralytics
from ultralytics import YOLO

# ==============================
# 1) Osnovna podešavanja
# ==============================
DATA_YAML   = "../dataset_seg/data.yaml"   # mora da postoji i sadrži train/val/test i names
PRETRAINED  = "yolov8n-seg.pt"             # mali, brz početni model (može: yolov8s-seg.pt ...)
IMG_SIZE    = 640
EPOCHS      = 50
BATCH_SIZE  = 8
WORKERS     = 2

# gde YOLO smešta rezultate (čuvamo u našem outputs/runs da ostanemo konzistentni)
PROJECT_OUT = "../outputs/runs"
RUN_NAME    = "seg_v1"

# ==============================
# 2) Provere
# ==============================
if not os.path.exists(DATA_YAML):
    raise FileNotFoundError(
        f"❌ Nije pronađen {DATA_YAML}. Napravi YOLO data.yaml u dataset_seg/ "
        f"(sa train/val/test putanjama i listom 'names')."
    )

device = 0 if torch.cuda.is_available() else "cpu"
print(f"✅ Uređaj: {'CUDA' if device == 0 else 'CPU'}")

# ==============================
# 3) Učitavanje modela (pretrained) i trening
# ==============================
model = YOLO(PRETRAINED)

results = model.train(
    data=DATA_YAML,
    imgsz=IMG_SIZE,
    epochs=EPOCHS,
    batch=BATCH_SIZE,
    workers=WORKERS,
    device=device,
    project=PROJECT_OUT,
    name=RUN_NAME,
    exist_ok=True,        # ne pucaj ako folder postoji
    verbose=True
)

# YOLO kreira folder npr: ../outputs/runs/seg_v1
run_dir = getattr(results, "save_dir", os.path.join(PROJECT_OUT, RUN_NAME))
print(f"📁 Rezultati treninga: {run_dir}")

# ==============================
# 4) (Opcionalno) Evaluacija na TEST skupu
# ==============================
# Ako u data.yaml postoji 'test', YOLO će evaluirati; inače preskačemo.
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
    # Rezultati (AP, mAP50-95, itd.) kao rečnik:
    print("📊 Test metrike:", getattr(test_results, "results_dict", {}))
except Exception as e:
    print(f"ℹ️ Preskačem test evaluaciju (verovatno nema 'test' u data.yaml): {e}")

# ==============================
# 5) Kopiraj best.pt na predvidivu lokaciju
# ==============================
best_src = os.path.join(run_dir, "weights", "best.pt")
best_dst = os.path.join(PROJECT_OUT, "seg_best.pt")
if os.path.exists(best_src):
    os.makedirs(PROJECT_OUT, exist_ok=True)
    shutil.copy2(best_src, best_dst)
    print(f"💾 Sačuvane najbolje težine: {best_dst}")
else:
    print("⚠️ Nije pronađen best.pt. Proveri run direktorijum i trening logove.")
