from ultralytics import YOLO
import yaml
from pathlib import Path

CFG = Path(__file__).resolve().parents[1] / "configs" / "clf.yaml"

def main():
    cfg = yaml.safe_load(open(CFG, "r", encoding="utf-8"))

    # 1) Provera dataset strukture
    ds_root = Path(cfg["dataset_root"])
    assert (ds_root/"train").exists() and (ds_root/"val").exists(), \
        f"Dataset folderi train/ i val/ ne postoje u {ds_root}"

    # 2) Kreiraj model iz baznih težina (pretrained)
    model = YOLO(cfg["base_model"])   # npr. yolov8n-cls.pt

    # 3) Trening (NE moraš sada pokretati)
    model.train(
        task="classify",
        data=str(ds_root),       # YOLO-CLS: root sa train/ i val/
        imgsz=cfg["imgsz"],
        epochs=cfg["epochs"],
        batch=cfg["batch"],
        workers=cfg["workers"],
        device=cfg["device"],
        project=cfg["project"],
        name=cfg["name"],
        seed=cfg["seed"],
        verbose=True
    )

    print("✅ Trening završen / ili spreman za pokretanje kada odlučiš.")

if __name__ == "__main__":
    main()



# 📌 Napomena: Mi sada nećemo pokretati ovaj fajl — on samo stoji spreman.
#  Kada budeš povezala projekat sa Dockerom ili serverom, samo klikneš 
# Run ▶️ i model će početi da se trenira.