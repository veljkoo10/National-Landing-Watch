import os
import torch
import torch.nn as nn
from torchvision import transforms, models
from PIL import Image
import pandas as pd

# ==============================
# 1️⃣  Podešavanja
# ==============================
IMAGES_DIR = "../real_images"           # 📂 Folder sa stvarnim slikama (npr. iz GE)
MODEL_PATH = "../outputs/runs/landfill_classifier.pth"
OUTPUT_CSV = "../outputs/preds/real_predictions.csv"
IMG_SIZE = 224

device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
print(f"✅ Koristimo uređaj: {device}")

# ==============================
# 2️⃣  Transformacije (MORAJU biti iste kao kod treniranja!)
# ==============================
transform = transforms.Compose([
    transforms.Resize((IMG_SIZE, IMG_SIZE)),
    transforms.ToTensor(),
    transforms.Normalize([0.485, 0.456, 0.406], [0.229, 0.224, 0.225])
])

# ==============================
# 3️⃣  Učitavanje modela
# ==============================
# Klase moraju biti u istom redosledu kao tokom treniranja!
class_names = ["illegal", "non_illegal", "no_landfill"]
num_classes = len(class_names)

model = models.resnet18(pretrained=False)
model.fc = nn.Linear(model.fc.in_features, num_classes)
model.load_state_dict(torch.load(MODEL_PATH, map_location=device))
model = model.to(device)
model.eval()

# ==============================
# 4️⃣  Predikcija nad folderom slika
# ==============================
results = []

for filename in os.listdir(IMAGES_DIR):
    if not filename.lower().endswith((".png", ".jpg", ".jpeg")):
        continue

    img_path = os.path.join(IMAGES_DIR, filename)
    image = Image.open(img_path).convert("RGB")
    image = transform(image).unsqueeze(0).to(device)

    with torch.no_grad():
        outputs = model(image)
        probs = torch.softmax(outputs, dim=1)
        confidence, pred_class = torch.max(probs, 1)

    predicted_label = class_names[pred_class.item()]
    confidence = confidence.item()

    print(f"🖼️ {filename} → {predicted_label} ({confidence*100:.2f}%)")

    results.append({
        "image_name": filename,
        "predicted_label": predicted_label,
        "confidence": confidence
    })

# ==============================
# 5️⃣  Sačuvaj rezultate u CSV
# ==============================
os.makedirs(os.path.dirname(OUTPUT_CSV), exist_ok=True)
df = pd.DataFrame(results)
df.to_csv(OUTPUT_CSV, index=False)
print(f"📁 Rezultati sačuvani u: {OUTPUT_CSV}")
