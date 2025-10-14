<!-- | Folder / fajl         |                   Uloga                                                                                                |
| --------------------- | ---------------------------------------------------------------------------------------------------- |
| `dataset_cls/`        | Glavni dataset podeljen u train / val / test sa tri klase (illegal, non_illegal, no_landfill).       |
| `train_cls.py`        | Skripta za treniranje modela – ovde učitavamo slike, treniramo CNN i čuvamo model.                   |
| `infer_cls.py`        | Evaluacija modela – ovde proveravamo preciznost, recall, F1, konfuzionu matricu…                     |
| `infer_cls_folder.py` | Batch predikcija – koristi se kasnije da testiraš novi set slika (npr. slike iz Srbije).             |
| `cls_config.yaml`     | Parametri (epohe, veličina slike, batch size, learning rate itd.) – menjaju se lako bez izmene koda. |
| `outputs/`            | Sve što model proizvede – težine (`.pt`), predikcije i metrički rezultati.                           |


📌 Napomena: Ova struktura je potpuno spremna da kasnije dodamo:

-> model2-seg/ za segmentaciju,
-> inference_api/ za servis koji poziva model iz backenda. -->

# 🧠 Model 1 – Klasifikacija deponija (Landfill Classification)

Ovaj projekat predstavlja **prvu fazu sistema za automatsko prepoznavanje deponija** na satelitskim snimcima. Cilj Modela 1 je da na osnovu slike **klasifikuje** da li na njoj postoji deponija, i ako postoji – o kojoj vrsti je reč (_divlja / sanitarna_).

📌 Ovaj model čini prvi korak kompletnog sistema i koristi se **pre detekcije i segmentacije (Model 2)**.

---

## 📊 Funkcionalnosti

✅ Klasifikacija slika u tri klase:

- `illegal` – divlja deponija
- `non_illegal` – sanitarna deponija
- `no_landfill` – bez deponije

✅ Evaluacija performansi modela pomoću:

- Precision, Recall, F1-score
- Confusion Matrix

✅ Inferencija nad stvarnim slikama:

- Korišćenjem istreniranog modela, sistem može da klasifikuje **nove slike iz realnog sveta** bez dodatnih informacija.

---

## 📂 Struktura projekta

model1-cls/
│
├─ dataset_cls/ # 📊 Dataset za treniranje, validaciju i testiranje
│ ├─ train/
│ ├─ val/
│ └─ test/
│
├─ src/ # 📁 Python skripte projekta
│ ├─ train_cls.py # Trenira klasifikacioni model
│ ├─ infer_cls.py # Evaluira model na test skupu
│ └─ infer_cls_folder.py # Pokreće predikciju nad realnim slikama
│
├─ configs/
│ └─ cls_config.yaml # Parametri za treniranje modela
│
├─ outputs/
│ ├─ runs/ # Sačuvani modeli (.pth)
│ ├─ preds/ # CSV fajlovi sa predikcijama
│ └─ metrics/ # Izveštaji i grafici performansi
│
├─ requirements.txt # Potrebne biblioteke
└─ README.md # Ovaj dokument

---

## ⚙️ Instalacija i pokretanje

1️⃣ Kloniraj projekat ili ga preuzmi kao ZIP:

```bash
git clone <repo-url>
cd model1-cls

2️⃣ Instaliraj potrebne biblioteke:

pip install -r requirements.txt


3️⃣ Pokreni treniranje modela:

python src/train_cls.py


4️⃣ Evaluiraj model na test skupu:

python src/infer_cls.py

5️⃣ Pokreni predikciju nad realnim slikama:

python src/infer_cls_folder.py
```
