import os
import csv
from pathlib import Path
import torch
from ultralytics import YOLO
from PIL import Image

# 1) General settings
IMAGES_DIR   = "/app/real_images"                  
BEST_WEIGHTS = "/app/outputs/runs/seg_best.pt"     
IMG_SIZE     = 1024
CONF_THRES   = 0.25
IOU_THRES    = 0.50
SAVE_VISUALS = True                                

PROJECT_OUT  = "/app/outputs/preds"
RUN_NAME     = "seg_infer_real"
CSV_NAME     = "predictions_real.csv"

device = 0 if torch.cuda.is_available() else "cpu"
print(f" Using device: {'CUDA' if device == 0 else 'CPU'}")


# 2) Checks
if not os.path.exists(BEST_WEIGHTS):
    raise FileNotFoundError(f" Model not found at: {BEST_WEIGHTS}")

if not os.path.isdir(IMAGES_DIR):
    raise FileNotFoundError(f" Images directory not found: {IMAGES_DIR}")

# Check if there are any images
has_image = any(f.lower().endswith((".png", ".jpg", ".jpeg", ".bmp", ".tif", ".tiff"))
                for f in os.listdir(IMAGES_DIR))
if not has_image:
    raise RuntimeError(f" No images found in: {IMAGES_DIR}")


# 3) Load model
model = YOLO(BEST_WEIGHTS)


# 4) Inference
pred_dir = Path(PROJECT_OUT) / RUN_NAME
pred_dir.mkdir(parents=True, exist_ok=True)
csv_path = pred_dir / CSV_NAME

print(f" Running inference on: {IMAGES_DIR}")
results = model.predict(
    source=IMAGES_DIR,
    imgsz=IMG_SIZE,
    device=device,
    conf=CONF_THRES,
    iou=IOU_THRES,
    save=SAVE_VISUALS,
    project=PROJECT_OUT,
    name=RUN_NAME,
    save_txt=False,
    verbose=True
)


# 5) Saving results in a CSV file
rows = []
for r in results:
    img_name = os.path.basename(r.path)
    if r.masks is None:
        continue

    xy_list = r.masks.xy
    confs = r.boxes.conf.cpu().tolist() if r.boxes is not None else [0.0] * len(xy_list)

    for poly_xy, conf in zip(xy_list, confs):
        flat = [f"{float(x):.2f},{float(y):.2f}" for x, y in poly_xy]
        rows.append([img_name, conf, ";".join(flat)])

with open(csv_path, "w", newline="", encoding="utf-8") as f:
    writer = csv.writer(f)
    writer.writerow(["image_name", "confidence", "polygon_px"])
    writer.writerows(rows)

print(f" CSV with polygons saved: {csv_path}")
print(f" Renderings and visuals are in: {pred_dir}")
