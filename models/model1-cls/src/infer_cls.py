import os
import torch
import torch.nn as nn
from torchvision import datasets, transforms, models
from torch.utils.data import DataLoader
from sklearn.metrics import classification_report, confusion_matrix
import matplotlib.pyplot as plt
import numpy as np
import pandas as pd

# ==============================
# 1️⃣  Basic podešavanja
# ==============================
DATA_DIR = "../dataset_cls/test"         # test skup
MODEL_PATH = "../outputs/runs/landfill_classifier.pth"
BATCH_SIZE = 32
IMG_SIZE = 224

device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
print(f"✅ Koristimo uređaj: {device}")

# ==============================
# 2️⃣  Transformacije (moraju biti iste kao kod treniranja!)
# ==============================
transform = transforms.Compose([
    transforms.Resize((IMG_SIZE, IMG_SIZE)),
    transforms.ToTensor(),
    transforms.Normalize([0.485, 0.456, 0.406], [0.229, 0.224, 0.225])
])

# ==============================
# 3️⃣  Dataset i DataLoader
# ==============================
test_dataset = datasets.ImageFolder(DATA_DIR, transform=transform)
test_loader = DataLoader(test_dataset, batch_size=BATCH_SIZE, shuffle=False)
class_names = test_dataset.classes
num_classes = len(class_names)
print(f"📁 Test skup: {len(test_dataset)} slika, klase: {class_names}")

# ==============================
# 4️⃣  Učitavanje modela
# ==============================
model = models.resnet18(pretrained=False)
model.fc = nn.Linear(model.fc.in_features, num_classes)
model.load_state_dict(torch.load(MODEL_PATH, map_location=device))
model = model.to(device)
model.eval()

# ==============================
# 5️⃣  Predikcije i evaluacija
# ==============================
all_preds = []
all_labels = []
all_paths = []

with torch.no_grad():
    for inputs, labels in test_loader:
        inputs, labels = inputs.to(device), labels.to(device)
        outputs = model(inputs)
        _, preds = torch.max(outputs, 1)
        all_preds.extend(preds.cpu().numpy())
        all_labels.extend(labels.cpu().numpy())

# ==============================
# 6️⃣  Izveštaji o performansama
# ==============================
print("\n📈 Classification Report:")
print(classification_report(all_labels, all_preds, target_names=class_names))

cm = confusion_matrix(all_labels, all_preds)
plt.figure(figsize=(6, 5))
plt.imshow(cm, cmap="Blues")
plt.title("Confusion Matrix")
plt.colorbar()
plt.xticks(np.arange(num_classes), class_names, rotation=45)
plt.yticks(np.arange(num_classes), class_names)
plt.xlabel("Predicted")
plt.ylabel("True")
plt.tight_layout()
plt.show()

# ==============================
# 7️⃣  (Opciono) Sačuvaj predikcije u CSV
# ==============================
# Ovo ti može kasnije pomoći da vidiš koje slike model greši
image_paths = [path[0] for path in test_dataset.samples]
df = pd.DataFrame({
    "image_path": image_paths,
    "true_label": [class_names[i] for i in all_labels],
    "predicted_label": [class_names[i] for i in all_preds]
})
os.makedirs("../outputs/preds", exist_ok=True)
csv_path = "../outputs/preds/test_predictions.csv"
df.to_csv(csv_path, index=False)
print(f"📁 Predikcije sačuvane u: {csv_path}")
