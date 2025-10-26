### **Link ka video demonstraciji aplikacije**
Unutar ovog foldera se nalazi video demonstracija projekta i folder sa rezultatima modela.
https://drive.google.com/drive/folders/1AH3_68-3i9BJnJzVnFlepiAliHneEFT7?usp=sharing
---

### **backend/Enzivor.Api**

Sadrži backend logiku napisanu u **ASP.NET Core-u**.  
Koristi **Entity Framework Core** za komunikaciju sa **PostgreSQL bazom**.  

Ovde se nalaze modeli, servisi, repozitorijumi i kontroleri koji upravljaju podacima o deponijama, regionima i izračunatim emisijama gasova.  
Backend takođe ima sloj za izračunavanje **metanskih i CO₂ emisija**, kao i agregaciju statistika po regionima.  

---

### **envizor_client/EnvizorFrontend**

Ovo je **frontend aplikacija** napravljena u **Angular-u**, sa modernim korisničkim interfejsom.  
Koristi **Mapbox GL** za prikaz interaktivne mape Srbije na kojoj su prikazane lokacije deponija, dok su grafikoni i statistike urađeni pomoću **Highcharts-a** i **Angular Material** komponenti.  

Korisnik može da pretražuje po regionima, menja režim prikaza (svetli/tamni) i pregleda dodatne podatke o svakoj deponiji.  

---

### **models/**

Ovaj folder sadrži **Python skripte i YOLO modele** koji su korišćeni za prepoznavanje deponija na satelitskim slikama.  
Treniranje je rađeno na datasetovima sa platforme **Roboflow**, uz dodatne slike preuzete iz **Google Earth-a** i drugih javno dostupnih izvora.  

Sistem koristi **dva modela**, od kojih **prvi model radi klasifikaciju**.  
Njegov zadatak je da, na osnovu nepoznate slike, **odredi kojoj klasi pripada posmatrana lokacija** — odnosno da li se na slici nalazi deponija i koje je ona vrste.  
Ako model zaključi da deponija **nije prisutna**, takva slika se tretira kao **negativna slika** (oznaka *no landfill*), čime se sprečava pogrešno označavanje površina koje zapravo nisu deponije.  

Modeli su podešeni da klasifikuju slike u tri kategorije:  
- **no landfill** – slika bez prisustva deponije (negativna slika)  
- **non-illegal landfill** – uređena ili legalna deponija  
- **illegal landfill** – nelegalna ili neuređena deponija  

Ovaj prvi korak klasifikacije omogućava da se filtriraju slike koje zaista sadrže deponije, dok se ostale ignorišu.  
Na osnovu rezultata klasifikacije, slike koje su označene kao deponije dalje se prosleđuju **drugom modelu** koji obavlja **segmentaciju** – odnosno precizno označava granice i površinu deponije na slici.  

---

## **Glavne funkcionalnosti**

- Mapa Srbije sa svim registrovanim i detektovanim deponijama  
- Filtriranje po regionima i prikaz broja deponija, površine i emisija  
- Statistički prikazi emisija metana (CH₄) i CO₂ po regionima i periodima  
- Pregled najzagađenijih regiona (Top regions)  
- Detalji o svakoj deponiji – ime, lokacija, veličina, tip, emisije  
- Blog sekcija sa edukativnim sadržajem  
- *Recenter* dugme – vraća mapu u početni prikaz  
- *Dark mode* – tamni vizuelni režim  

---

## **Kako sistem funkcioniše**

1. **Python model** analizira satelitske slike i generiše rezultate o detektovanim deponijama.  
2. Ti rezultati (koordinate, površina, procene) se učitavaju u **PostgreSQL bazu** putem **.NET API-ja**.  
3. **Backend** računa emisije gasova (CH₄, CO₂eq) koristeći međunarodne faktore i formule iz **IPCC vodiča**.  
4. **Angular frontend** preuzima podatke preko API-ja i prikazuje ih na **Mapbox mapi** i kroz grafikone.  
5. Korisnik može da izabere region, pregleda statistike i vizuelno sagleda stanje deponija u Srbiji.  

---

## **Analiza podataka i formule**

Za procenu emisije gasova koriste se uprošćene verzije formula iz **IPCC (Intergovernmental Panel on Climate Change)** smernica.  
Metan (CH₄) se računa prema količini otpada, tipu deponije i faktorima degradacije.  
Zatim se CH₄ pretvara u CO₂-ekvivalent pomoću faktora 25 (jer je metan 25 puta jači gas staklene bašte od CO₂).  

**Formule:**  
CH₄ = MSW × DOC × DOCf × MCF × F × 16/12
CO₂eq = CH₄ × 25


Ovi podaci omogućavaju da se vidi koji regioni imaju najveći uticaj na zagađenje i koliko bi se emisije smanjile boljim upravljanjem deponijama.  

---

## **Demonstracija aplikacije**

U folderu `video/` nalazi se snimak rada aplikacije koji prikazuje:  
- učitavanje početne mape  
- označavanje deponija  
- prikaz detalja i grafikona po regionima  
- promenu teme (dark/light)  
- način rada funkcija poput *Recenter* i *Top regions*  

Na osnovu snimka može se steći kompletan utisak o izgledu i funkcionalnosti projekta.  

---

## **Tim Envizor**

Projekat je nastao kroz timski rad **četiri studenta**, koji su podelili zadatke po oblastima:  
- razvoj **backend logike i API endpointa**  
- razvoj **frontend aplikacije i UI dizajna**  
- **treniranje AI modela** i analiza podataka  
- priprema **baze podataka** i integracija komponenti  

Cilj tima je bio da prikaže kako se kroz zajednički rad i kombinovanje znanja može realizovati kompletna **full-stack aplikacija** koja rešava konkretan problem i ima društveni značaj.  



