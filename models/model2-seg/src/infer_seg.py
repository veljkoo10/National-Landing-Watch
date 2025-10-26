import os
import csv
from pathlib import Path
import torch
from ultralytics import YOLO
import yaml


# 1️) General settings
DATA_YAML     = "/app/dataset/segmentation/data.yaml"
BEST_WEIGHTS  = "/app/outputs/runs/seg_best.pt"
IMG_SIZE      = 1024
PROJECT_OUT   = "/app/outputs/preds"
RUN_NAME      = "seg_test_infer"
SAVE_VISUALS  = True  
CONF_THRES    = 0.25
IOU_THRES     = 0.5
CUSTOM_TEST_DIR = "/app/dataset/test/images"

device = 0 if torch.cuda.is_available() else "cpu"
print(f" Using device: {'CUDA' if device == 0 else 'CPU'}")


# 2️) Checks
if not os.path.exists(BEST_WEIGHTS):
    raise FileNotFoundError(f" Model not found at {BEST_WEIGHTS}. Run train_seg.py to create seg_best.pt.")
if not os.path.exists(DATA_YAML):
    raise FileNotFoundError(f" {DATA_YAML} not found. Check data.yaml in dataset/segmentation.")


# 3️) Loading the model
print(f" Loading model from: {BEST_WEIGHTS}")
model = YOLO(BEST_WEIGHTS)


# 4️) Evaluation
print(" Evaluating on TEST set from data.yaml...")
try:
    val_results = model.val(
        data=DATA_YAML,
        split="test",
        imgsz=IMG_SIZE,
        device=device,
        project=PROJECT_OUT,
        name=f"{RUN_NAME}_metrics",
        save_json=False,
        verbose=True
    )
    print("Test metrics:", getattr(val_results, "results_dict", {}))
except Exception as e:
    print(f" Skipping test evaluation (likely no 'test' in data.yaml): {e}")


# 5️) Selecting the folder with test images
if os.path.exists(CUSTOM_TEST_DIR) and len(os.listdir(CUSTOM_TEST_DIR)) > 0:
    test_images_dir = CUSTOM_TEST_DIR
    print(f" Using custom test directory: {test_images_dir}")
else:
    with open(DATA_YAML, "r", encoding="utf-8") as f:
        cfg = yaml.safe_load(f)
    test_images_dir = cfg.get("test", None)
    if not test_images_dir:
        print(" No 'test' defined in either data.yaml or /app/dataset/test. Exiting.")
        raise SystemExit(0)


# 6️) Generating predictions
pred_dir = Path(PROJECT_OUT) / RUN_NAME
pred_dir.mkdir(parents=True, exist_ok=True)
csv_path = pred_dir / "test_predictions_polygons.csv"

print(f" Generating predictions on: {test_images_dir}")
results = model.predict(
    source=test_images_dir,
    imgsz=IMG_SIZE,
    device=device,
    project=PROJECT_OUT,
    name=RUN_NAME,
    save=SAVE_VISUALS,
    save_txt=False,
    conf=CONF_THRES,
    iou=IOU_THRES,
    verbose=True
)


# 7️) Parsing results to CSV
rows = []
for r in results:
    img_name = os.path.basename(r.path)
    if r.masks is None:
        continue 

    xy_list = r.masks.xy
    confs = r.boxes.conf.cpu().tolist() if r.boxes is not None else [None] * len(xy_list)

    for poly_xy, conf in zip(xy_list, confs):
        flat = [f"{float(x):.2f},{float(y):.2f}" for x, y in poly_xy]
        polygon_str = ";".join(flat)
        rows.append([img_name, conf if conf else 0.0, polygon_str])

# updating CSV
with open(csv_path, "w", newline="", encoding="utf-8") as f:
    writer = csv.writer(f)
    writer.writerow(["image_name", "confidence", "polygon_px"])
    writer.writerows(rows)

print(f" CSV saved: {csv_path}")
print(f" Rendered images and masks are in: {pred_dir}")
