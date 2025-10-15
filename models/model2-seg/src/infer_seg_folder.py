import os
import csv
from pathlib import Path
import torch
from ultralytics import YOLO
from PIL import Image

# ==============================
# 1) Osnovna podešavanja
# ==============================
IMAGES_DIR   = "../real_images"            # 📂 folder sa realnim GE/produkcionim slikama
BEST_WEIGHTS = "../outputs/runs/seg_best.pt"
IMG_SIZE     = 640
CONF_THRES   = 0.25
IOU_THRES    = 0.50
SAVE_VISUALS = True                        # sačuvaj rendere sa iscrtanim poligonima

PROJECT_OUT  = "../outputs/preds"
RUN_NAME     = "seg_folder_infer"          # podfolder sa rezultatima
CSV_NAME     = "folder_predictions_polygons.csv"

device = 0 if torch.cuda.is_available() else "cpu"
print(f"✅ Uređaj: {'CUDA' if device == 0 else 'CPU'}")

# ==============================
# 2) Provere
# ==============================
if not os.path.exists(BEST_WEIGHTS):
    raise FileNotFoundError(
        f"❌ Nije pronađen model na {BEST_WEIGHTS}. Pokreni train_seg.py da napraviš seg_best.pt."
    )

if not os.path.isdir(IMAGES_DIR):
    raise FileNotFoundError(
        f"❌ Nije pronađen folder sa slikama: {IMAGES_DIR}"
    )

# (opciono) proveri da folder sadrži barem jednu sliku
has_image = any(
    f.lower().endswith((".png", ".jpg", ".jpeg", ".bmp", ".tif", ".tiff"))
    for f in os.listdir(IMAGES_DIR)
)
if not has_image:
    raise RuntimeError(f"⚠️ U {IMAGES_DIR} nema slika (.png/.jpg/.jpeg/.bmp/.tif)")

# ==============================
# 3) Učitavanje modela
# ==============================
model = YOLO(BEST_WEIGHTS)

# ==============================
# 4) Inference nad folderom
# ==============================
pred_dir = Path(PROJECT_OUT) / RUN_NAME
pred_dir.mkdir(parents=True, exist_ok=True)
csv_path = pred_dir / CSV_NAME

print(f"▶️ Pokrećem predikcije nad: {IMAGES_DIR}")
results = model.predict(
    source=IMAGES_DIR,
    imgsz=IMG_SIZE,
    device=device,
    conf=CONF_THRES,
    iou=IOU_THRES,
    save=SAVE_VISUALS,          # čuva rendere sa iscrtanim maskama/poligonima
    project=PROJECT_OUT,
    name=RUN_NAME,
    save_txt=False,
    verbose=True
)

# ==============================
# 5) Parsiranje rezultata → CSV
# ==============================
rows = []  # image_name, confidence, polygon_px

for r in results:
    img_path = r.path
    img_name = os.path.basename(img_path)

    # Ako nema maski, zabeleži red bez poligona (po potrebi)
    if r.masks is None:
        # rows.append([img_name, 0.0, ""])  # otkomentariši ako želiš "no detections" u CSV
        continue

    # r.masks.xy → lista poligona (svaki je ndarray Nx2 u pikselima)
    xy_list = r.masks.xy
    confs = r.boxes.conf.cpu().tolist() if r.boxes is not None else [None] * len(xy_list)

    for poly_xy, conf in zip(xy_list, confs):
        flat = []
        for x, y in poly_xy:
            flat.extend([float(x), float(y)])

        # čuvamo poligon kao "x1,y1; x2,y2; ..."
        rows.append([
            img_name,
            float(conf) if conf is not None else 0.0,
            ";".join(f"{v:.2f}" for v in flat)
        ])

# upis u CSV
with open(csv_path, "w", newline="", encoding="utf-8") as f:
    writer = csv.writer(f)
    writer.writerow(["image_name", "confidence", "polygon_px"])
    writer.writerows(rows)

print(f"💾 CSV sa poligonima (px) sačuvan: {csv_path}")
print(f"🖼️ Renderi (ako je SAVE_VISUALS=True) su u: {pred_dir}")
