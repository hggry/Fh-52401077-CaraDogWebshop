# CaraDog Webshop – Web Technologien MVP

## Projektübersicht
Ein Webshop für das Business **Cara♡Dog – Handgemachte Hundemode** im Rahmen der Lehrveranstaltung *Web Technologien* an der FH Salzburg.

Ziel ist es eine nachhaltige und erweiterbare E-Commerce-Website mit **HTML, CSS, JavaScript & C# Backend** umzusetzen.

---

## Features

### Allgemein
- Mehrseitige Website mit Navigation
- Responsives Layout
- Hash-Routing ohne Framework
- Zentrale Styles über CSS-Variablen
- Produktdatenverwaltung via SQL DB

### Shop
- Produktkategorieseite
- Produktdetailseiten
- Warenkorb (gespeichert im LocalStorage)
- Checkout-Demoformular mit Validierung

### Individueller Konfigurator
- Konfigurator für:
  - Halsbänder
  - Leinen  
- Optionen:
  - Material
  - Hardware
  - Breite
  - Größe / Maßangaben
  - Personalisierung
- Dynamische Preisberechnung
- Hinzufügen als Custom-Produkt in den Warenkorb

---

## Tech Stack
- Frontend:  
  - HTML5  
  - CSS3  
  - Vanilla JavaScript

- Backend:
  - C# ASP.NET Core API
  - SQL

## Installation / Lokale Entwicklung
Zur ausführung wird ein lokaler Server benötigt.

### Option 1 – Python
1. Webserver starten mit python befehl
```bash
python -m http.server 5500
```
2. Dann öffnen: http://localhost:5500

### Option 2 – VS Code
1. „Live Server“ Extension installieren
2. index.html mit Live Server starten

