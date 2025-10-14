import os
import csv
from pathlib import Path
import torch
from ultralytics import YOLO

# ==============================
# 1) Osnovna podešavanja
# ==============================
DATA_YAML     = "../dataset_seg/data.yaml"     # isti YAML kao za trening
BEST_WEIGHTS  = "../outputs/runs/seg_best.pt"  # kopiran u train_seg.py
IMG_SIZE      = 640
PROJECT_OUT   = "../outputs/preds"
RUN_NAME      = "seg_test_infer"               # podfolder za rezultate
SAVE_VISUALS  = True                           # sačuvaj renderovane slike sa poligonima

device = 0 if torch.cuda.is_available() else "cpu"
print(f"✅ Uređaj: {'CUDA' if device == 0 else 'CPU'}")

# ==============================
# 2) Provere
# ==============================
if not os.path.exists(BEST_WEIGHTS):
    raise FileNotFoundError(
        f"❌ Nije pronađen model na {BEST_WEIGHTS}. Pokreni train_seg.py da napraviš seg_best.pt."
    )
if not os.path.exists(DATA_YAML):
    raise FileNotFoundError(
        f"❌ Nije pronađen {DATA_YAML}. Proveri data.yaml u dataset_seg/."
    )

# ==============================
# 3) Učitavanje modela
# ==============================
model = YOLO(BEST_WEIGHTS)

# ==============================
# 4) Evaluacija na TEST skupu (mAP metrike)
# ==============================
print("▶️ Evaluacija na TEST skupu (ako je definisan u data.yaml)...")
try:
    val_results = model.val(
        data=DATA_YAML,
        split="test",        # koristi test split iz data.yaml
        imgsz=IMG_SIZE,
        device=device,
        project=PROJECT_OUT,
        name=f"{RUN_NAME}_metrics",
        save_json=False,     # može i True ako želiš COCO-style json
        verbose=True
    )
    print("📊 Test metrike:", getattr(val_results, "results_dict", {}))
except Exception as e:
    print(f"ℹ️ Preskačem test evaluaciju (verovatno nema 'test' u data.yaml): {e}")

# ==============================
# 5) Generisanje predikcija nad TEST slikama i snimanje u CSV
# ==============================
# YOLO 'predict' će proći kroz test split (ako ga zadamo kao 'source' iz YAML-a)
# Jednostavnije: prosledi putanju do test images foldera.
# U data.yaml obično stoji npr:
# test: ../dataset_seg/test/images
# Pokušaćemo da je pročitamo.
import yaml
with open(DATA_YAML, "r", encoding="utf-8") as f:
    cfg = yaml.safe_load(f)

test_images_dir = cfg.get("test", None)
if test_images_dir is None:
    print("ℹ️ Upozorenje: 'test' nije definisan u data.yaml. Preskačem predikcije na test slikama.")
    raise SystemExit(0)

# izlazi
pred_dir = Path(PROJECT_OUT) / RUN_NAME
pred_dir.mkdir(parents=True, exist_ok=True)
csv_path = pred_dir / "test_predictions_polygons.csv"

print(f"▶️ Generišem predikcije nad: {test_images_dir}")
results = model.predict(
    source=test_images_dir,
    imgsz=IMG_SIZE,
    device=device,
    project=PROJECT_OUT,
    name=RUN_NAME,
    save=SAVE_VISUALS,     # čuva renderovane slike sa poligonima
    save_txt=False,        # možete i True (YOLO txt); ovde pravimo svoj CSV
    conf=0.25,
    iou=0.5,
    verbose=True
)

# ==============================
# 6) Parsiranje rezultata: poligoni (px) i confidence → CSV
# ==============================
# Svaki 'r' u results je BatchResults za jednu sliku.
# r.path → putanja slike
# r.masks → poligoni (ako ih ima), u normalizovanom ili px obliku; u v8 dobijamo xy u px u r.masks.xy
# r.boxes.conf → confidence za svaku detekciju

rows = []
for r in results:
    img_path = r.path
    img_name = os.path.basename(img_path)

    # Ako nema maski (nema detekcija), preskoči
    if r.masks is None:
        # Beležimo i "no detections" ako želiš:
        # rows.append([img_name, "", "", 0.0])
        continue

    # r.masks.xy je lista poligona; svaki je np.ndarray oblika (N, 2) u pikselima
    xy_list = r.masks.xy
    confs = r.boxes.conf.cpu().tolist() if r.boxes is not None else [None] * len(xy_list)

    for poly_xy, conf in zip(xy_list, confs):
        # pretvaramo poligon u ravan niz "x1,y1,x2,y2,..."
        flat = []
        for x, y in poly_xy:
            flat.extend([float(x), float(y)])

        rows.append([
            img_name,
            conf if conf is not None else 0.0,
            ";".join(map(lambda v: f"{v:.2f}", flat))  # string svih xy sa 2 decimale, ;-separirano
        ])

# snimi CSV
with open(csv_path, "w", newline="", encoding="utf-8") as f:
    writer = csv.writer(f)
    writer.writerow(["image_name", "confidence", "polygon_px"])  # polygon_px: "x1,y1; x2,y2; ..."
    writer.writerows(rows)

print(f"💾 CSV sa poligonima (px) sačuvan: {csv_path}")

# Ako je SAVE_VISUALS=True, renderovane slike su u:
# ../outputs/preds/seg_test_infer/  (folder 'labels' i 'images' u YOLO formatu)
print(f"🖼️ Renderi i prateći fajlovi su u: {pred_dir}")
