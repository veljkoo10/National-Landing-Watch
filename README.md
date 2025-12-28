# **Envizor – National Landing Watch**

The **National Landing Watch** project is developed by the **Envizor** team, with the goal of demonstrating how modern technologies — *machine learning, web development, and databases* — can be combined to address real environmental problems and monitor the impact of different types of landfills on the environment in the Republic of Serbia.

Modern waste management represents one of the greatest challenges of today’s society. Illegal and unregulated landfills have a significant impact on soil, water, and air pollution, as well as on greenhouse gas emissions, especially methane (CH₄).  
The Envizor project aims to provide a **transparent, interactive, and technology-driven insight** into the state of landfills across the country.

*The application enables monitoring, detection, and analysis of landfills using satellite imagery and ML models.*  
Users can explore maps, regional statistics, landfill types, gas emissions, and change trends over time.

---

## **System Architecture**

The system consists of three main components:

- **Backend (ASP.NET Core)** – manages data, business logic, and emission calculations  
- **Frontend (Angular)** – provides an interactive map view and statistical visualizations  
- **ML Models (Python + YOLO)** – automatically detect and segment landfills from satellite images  

The project connects science, technology, and ecology, providing a foundation for analysis, education, and environmentally responsible decision-making at both local and national levels.

---

## **Application Video Demonstration**

This repository includes a video demonstration of the project along with a folder containing model results.

🔗 https://drive.google.com/drive/folders/1AH3_68-3i9BJnJzVnFlepiAliHneEFT7?usp=sharing

---

## **backend/Envizor.Api**

Contains backend logic written in **ASP.NET Core**.  
Uses **Entity Framework Core** to communicate with a **PostgreSQL database**.

Includes models, services, repositories, and controllers responsible for managing landfill data, regions, and calculated gas emissions.  
The backend also contains logic for calculating **methane and CO₂ emissions** and aggregating regional statistics.

---

## **envizor_client/EnvizorFrontend**

The **frontend application** is built using **Angular** and features a modern UI.  
It uses **Mapbox GL** to display an interactive map of Serbia with landfill locations, while charts and statistics are implemented using **Highcharts** and **Angular Material** components.

Users can:
- search by region  
- switch between light and dark mode  
- view detailed information about each landfill  

---

## **models/**

This folder contains **Python scripts and YOLO models** used for landfill detection from satellite images.

Training was performed using datasets from **Roboflow**, supplemented with images from **Google Earth** and other publicly available sources.

### **Model Pipeline**

The system uses **two ML models**:

#### **1. Classification Model**
Determines whether a landfill is present in an image and identifies its type.

Image classes:
- **no landfill** – image without landfill (negative sample)
- **non-illegal landfill** – regulated/legal landfill
- **illegal landfill** – illegal or unregulated landfill

If no landfill is detected, the image is ignored to prevent false positives.

#### **2. Segmentation Model**
Images classified as landfills are passed to the second model, which performs **segmentation** to precisely identify landfill boundaries and surface area.

---

## **Key Features**

- Interactive map of Serbia with all registered and detected landfills  
- Region-based filtering with landfill count, surface area, and emissions  
- Statistical visualization of methane (CH₄) and CO₂ emissions by region and time  
- Overview of the most polluted regions (*Top regions*)  
- Detailed landfill data (name, location, size, type, emissions)  
- Educational blog section  
- *Recenter* button to reset map position  
- *Dark mode* support  

---

## **How the System Works**

1. **Python ML models** analyze satellite images and generate landfill detection results  
2. Results (coordinates, surface area, estimates) are stored in a **PostgreSQL database** via the **.NET API**  
3. The **backend** calculates gas emissions (CH₄, CO₂eq) using formulas from **IPCC guidelines**  
4. The **Angular frontend** fetches data through the API and displays it using **Mapbox** and charts  
5. Users explore regions, view statistics, and assess landfill impact across Serbia  

---

## **Emission Analysis and Formulas**

Gas emissions are calculated using simplified formulas based on **IPCC (Intergovernmental Panel on Climate Change)** guidelines.

Methane (CH₄) is calculated using waste amount, landfill type, and degradation factors.  
CH₄ is converted to CO₂ equivalent using a factor of 25, as methane is 25 times more potent than CO₂.

### **Formulas**

CH₄ = MSW × DOC × DOCf × MCF × F × 16/12
CO₂eq = CH₄ × 25

These calculations help identify high-impact regions and estimate potential emission reductions through improved landfill management.

---

## **Application Demonstration**

The `video/` folder contains a recording demonstrating:
- initial map loading  
- landfill visualization  
- regional statistics and charts  
- dark/light theme switching  
- features such as *Recenter* and *Top regions*  

The demo provides a complete overview of the application’s functionality and design.

---

## **Envizor Team**

- [veljkoo10](https://github.com/veljkoo10)
- [Baric03](https://github.com/Baric03)
- [andjelamrdja](https://github.com/andjelamrdja)
- [milicammm](https://github.com/milicammm)
  
The project was developed by a team of **four students**, with responsibilities divided across:

- **Backend development and API design**
- **Frontend development and UI/UX**
- **AI model training and data analysis**
- **Database design and system integration**

The goal was to demonstrate how collaborative teamwork and combined expertise can produce a complete **full-stack application** that addresses a real-world problem with meaningful social and environmental impact.
