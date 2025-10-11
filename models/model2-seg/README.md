# 🛰️ Model 2 – Segmentacija deponija (YOLOv8-seg)

Ovaj modul je druga faza sistema: **precizna lokacija deponije** na slici pomoću **segmentacije poligonom** (YOLOv8-seg).  
Ulaz: satelitska slika.  
Izlaz: poligon(i) deponije u **piksel koordinatama** + confidence.

---

## 📂 Struktura

model2-seg/
├─ dataset_seg/
│ ├─ train/
│ │ ├─ images/
│ │ └─ labels/ # YOLOv8-seg .txt sa poligonima (normalized [0,1])
│ ├─ val/
│ │ ├─ images/
│ │ └─ labels/
│ └─ test/
│ ├─ images/
│ └─ labels/
│
├─ src/
│ ├─ train_seg.py # treniranje (YOLOv8-seg)
│ ├─ infer_seg.py # evaluacija + CSV poligona (TEST)
│ └─ infer_seg_folder.py # inference nad realnim slikama (produkcija)
│
├─ configs/
│ └─ seg_config.yaml
│
├─ outputs/
│ ├─ runs/ # modeli i treninzi
│ ├─ preds/ # CSV + renderi iz inference-a
│ └─ metrics/ # (opciono) dodatne metrike
│
├─ requirements.txt
└─ README.md

---

## 📝 `dataset_seg/data.yaml` (primer)

```yaml
path: ../dataset_seg

train: ../dataset_seg/train/images
val:   ../dataset_seg/val/images
test:  ../dataset_seg/test/images

names:
  0: landfill

Uslov: Za svaku sliku images/xxx.jpg mora postojati labels/xxx.txt.
Format jedne linije: class_id x1 y1 x2 y2 ... xN yN (sve normalizovano na [0,1]).

🚀 Pokretanje

Instalacija zavisnosti:

pip install -r requirements.txt


Trening:

python src/train_seg.py


Evaluacija (TEST) + CSV poligona:

python src/infer_seg.py


Inference nad realnim slikama (npr. iz Google Earth-a):

Stavi slike u model2-seg/real_images/

python src/infer_seg_folder.py


Rezultati (CSV sa poligonima u pikselima) nalaze se u:

outputs/preds/seg_test_infer/test_predictions_polygons.csv
outputs/preds/seg_folder_infer/folder_predictions_polygons.csv

🔗 Šta backend koristi

Backend čita CSV sa kolonom polygon_px (format: "x1,y1; x2,y2; ..."),
zatim piksel tačke pretvara u geokoordinate prema BBOX-u slike, računa površinu i čuva u bazu.


✅ Saveti

Počni sa yolov8n-seg.pt (brz i mali), pa kasnije probaj yolov8s-seg.pt za veću tačnost.
Standardizuj rezolucije slika u produkciji (npr. 1024×1024) da bi px→lon/lat bilo stabilno.
Proveri da su labele pravilno normalizovane (sve vrednosti u [0,1]).
```
