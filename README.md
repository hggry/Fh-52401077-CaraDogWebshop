# CaraDog Webshop – Web Technologien MVP

## Projektübersicht
Ein Webshop für das Business **Cara♡Dog – Handgemachte Hundemode** im Rahmen der Lehrveranstaltung *Web Technologien* an der FH Salzburg.

Ziel ist es eine nachhaltige und erweiterbare E-Commerce-Website mit **HTML, CSS, JavaScript & C# Backend** umzusetzen.

---

## Features

### Allgemein
- Mehrseitige Website mit Navigation
- Responsives Layout
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

# Installation / Lokale Entwicklung
Voraussetzungen
- Docker
- .NET 8.0
- Entity Framework CLI

(Achten auf korrekte Pfadverweise)
1. **Falls nicht vorhanden Entity Framwork CLI installieren**
```bash
dotnet new tool-manifest
dotnet tool install dotnet-ef --version 8.0.22
dotnet tool restore
```

2. **Docker Container starten**
```bash
docker compose up --build -d
```

3. **Db mit Tabellen befüllen**
```bash
dotnet ef database update -p .\Backend\CaraDog.Db\CaraDog.Db.csproj -s .\Backend\CaraDog.Api\CaraDog.Api.csproj
```

4. **Db mit initialen Daten befüllen**
```bash
dotnet run --project .\Backend\CaraDog.CLI -- seed --file .\Backend\CaraDog.CLI\seed-example.json --upsert
```

5. **Webshop aufrufen**
-> http://localhost:8081