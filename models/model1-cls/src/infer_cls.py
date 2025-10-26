import os
import torch
import torch.nn as nn
from torchvision import transforms, models
from PIL import Image
import pandas as pd
import numpy as np


# 1️) General settings
DATA_DIR = "/app/dataset/test" 
MODEL_PATH = "/app/outputs/runs/landfill_classifier.pth"
OUTPUT_CSV = "/app/outputs/preds/test_predictions.csv"
IMG_SIZE = 224

class_names = ["illegal", "no_landfill", "non_illegal"]
num_classes = len(class_names)

device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
print(f" Using device: {device}")


# 2️) Transformations
transform = transforms.Compose([
    transforms.Resize((IMG_SIZE, IMG_SIZE)),
    transforms.ToTensor(),
    transforms.Normalize([0.485, 0.456, 0.406],
                         [0.229, 0.224, 0.225])
])

# 3️) Model
model = models.resnet18(pretrained=False)
model.fc = nn.Linear(model.fc.in_features, num_classes)
model.load_state_dict(torch.load(MODEL_PATH, map_location=device))
model = model.to(device)
model.eval()

# 4️) Inference
results = []

image_files = [f for f in os.listdir(DATA_DIR) if f.lower().endswith((".jpg", ".jpeg", ".png"))]
if not image_files:
    print(f" Nema slika u folderu: {DATA_DIR}")
    exit(1)

for filename in sorted(image_files):
    img_path = os.path.join(DATA_DIR, filename)
    image = Image.open(img_path).convert("RGB")
    image = transform(image).unsqueeze(0).to(device)

    with torch.no_grad():
        outputs = model(image)
        probs = torch.softmax(outputs, dim=1)
        confidence, pred_class = torch.max(probs, 1)

    predicted_label = class_names[pred_class.item()]
    confidence = confidence.item()
    print(f" {filename} → {predicted_label} ({confidence*100:.2f}%)")

    results.append({
        "image_name": filename,
        "predicted_label": predicted_label,
        "confidence": confidence
    })

# 5️) Saving results in a CSV file
os.makedirs(os.path.dirname(OUTPUT_CSV), exist_ok=True)
df = pd.DataFrame(results)
df.to_csv(OUTPUT_CSV, index=False)
print(f" Results saved to: {OUTPUT_CSV}")
