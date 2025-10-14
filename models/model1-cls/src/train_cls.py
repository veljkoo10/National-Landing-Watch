import os
import torch
import torch.nn as nn
import torch.optim as optim
from torchvision import datasets, transforms, models
from torch.utils.data import DataLoader
from sklearn.metrics import classification_report, confusion_matrix
import matplotlib.pyplot as plt
import numpy as np

# ==============================
# 1️⃣  Basic podešavanja
# ==============================
DATA_DIR = "../dataset_cls"      # putanja do dataset-a
BATCH_SIZE = 32
NUM_EPOCHS = 15
LEARNING_RATE = 0.001
IMG_SIZE = 224                   # standardna veličina za većinu CNN-a
OUTPUT_DIR = "../outputs/runs"  # gde ćemo sačuvati model

device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
print(f"✅ Koristimo uređaj: {device}")

# ==============================
# 2️⃣  Transformacije za slike
# ==============================
transform = transforms.Compose([
    transforms.Resize((IMG_SIZE, IMG_SIZE)),
    transforms.ToTensor(),
    transforms.Normalize([0.485, 0.456, 0.406], [0.229, 0.224, 0.225])  # standard za pretreniran model
])

# ==============================
# 3️⃣  Dataset i DataLoader
# ==============================
train_dataset = datasets.ImageFolder(os.path.join(DATA_DIR, "train"), transform=transform)
val_dataset = datasets.ImageFolder(os.path.join(DATA_DIR, "val"), transform=transform)

train_loader = DataLoader(train_dataset, batch_size=BATCH_SIZE, shuffle=True)
val_loader = DataLoader(val_dataset, batch_size=BATCH_SIZE, shuffle=False)

class_names = train_dataset.classes
num_classes = len(class_names)
print(f"📁 Klasifikacija {num_classes} klasa: {class_names}")

# ==============================
# 4️⃣  CNN model (transfer learning – ResNet18)
# ==============================
model = models.resnet18(pretrained=True)         # koristimo gotov ResNet18
for param in model.parameters():
    param.requires_grad = False                  # zamrzni bazne slojeve

# Zamenjujemo izlazni sloj sa našim brojem klasa
model.fc = nn.Linear(model.fc.in_features, num_classes)
model = model.to(device)

# ==============================
# 5️⃣  Gubitak i optimizator
# ==============================
criterion = nn.CrossEntropyLoss()
optimizer = optim.Adam(model.fc.parameters(), lr=LEARNING_RATE)

# ==============================
# 6️⃣  Trening petlja
# ==============================
train_losses, val_losses = [], []

for epoch in range(NUM_EPOCHS):
    # ---- Trening ----
    model.train()
    running_loss = 0.0
    correct = 0
    total = 0

    for inputs, labels in train_loader:
        inputs, labels = inputs.to(device), labels.to(device)

        optimizer.zero_grad()
        outputs = model(inputs)
        loss = criterion(outputs, labels)
        loss.backward()
        optimizer.step()

        running_loss += loss.item()
        _, preds = torch.max(outputs, 1)
        correct += torch.sum(preds == labels).item()
        total += labels.size(0)

    epoch_train_loss = running_loss / len(train_loader)
    epoch_train_acc = correct / total
    train_losses.append(epoch_train_loss)

    # ---- Validacija ----
    model.eval()
    val_loss = 0.0
    correct_val = 0
    total_val = 0

    with torch.no_grad():
        for inputs, labels in val_loader:
            inputs, labels = inputs.to(device), labels.to(device)
            outputs = model(inputs)
            loss = criterion(outputs, labels)

            val_loss += loss.item()
            _, preds = torch.max(outputs, 1)
            correct_val += torch.sum(preds == labels).item()
            total_val += labels.size(0)

    epoch_val_loss = val_loss / len(val_loader)
    epoch_val_acc = correct_val / total_val
    val_losses.append(epoch_val_loss)

    print(f"📊 Epoch [{epoch+1}/{NUM_EPOCHS}] "
          f"Train Loss: {epoch_train_loss:.4f} Acc: {epoch_train_acc:.4f} "
          f"Val Loss: {epoch_val_loss:.4f} Acc: {epoch_val_acc:.4f}")

# ==============================
# 7️⃣  Čuvanje modela
# ==============================
os.makedirs(OUTPUT_DIR, exist_ok=True)
model_path = os.path.join(OUTPUT_DIR, "landfill_classifier.pth")
torch.save(model.state_dict(), model_path)
print(f"✅ Model sačuvan: {model_path}")

# ==============================
# 8️⃣  Evaluacija na validacionom skupu
# ==============================
model.eval()
all_preds = []
all_labels = []

with torch.no_grad():
    for inputs, labels in val_loader:
        inputs, labels = inputs.to(device), labels.to(device)
        outputs = model(inputs)
        _, preds = torch.max(outputs, 1)
        all_preds.extend(preds.cpu().numpy())
        all_labels.extend(labels.cpu().numpy())

# 📊 Izveštaj klasifikacije
print("\n📈 Classification Report:")
print(classification_report(all_labels, all_preds, target_names=class_names))

# 📉 Konfuziona matrica
cm = confusion_matrix(all_labels, all_preds)
plt.imshow(cm, cmap="Blues")
plt.title("Confusion Matrix")
plt.colorbar()
plt.xticks(np.arange(num_classes), class_names, rotation=45)
plt.yticks(np.arange(num_classes), class_names)
plt.xlabel("Predicted")
plt.ylabel("True")
plt.show()
