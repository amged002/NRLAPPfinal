# 🧩 Systemarkitektur

NRLApp består av to hovedkomponenter:

---

## 1. Webapplikasjon (ASP.NET Core MVC)

- Presentasjon via Razor Views
- Identity for autentisering
- Roller: Pilot, Approver, Admin
- Leaflet-kart med tegning av punkter/linjer
- Dapper + EF Core for datahåndtering
- Sikker bildeopplasting

---

## 2. MariaDB-database

Lagrer:

- Hindre
- Geometri (GeoJSON)
- Brukere, roller og organisasjoner
- Høyde, kategori, status, tidspunkt
- Saksbehandlingsinformasjon og kommentarer

---

## Kontrollflyt

### Pilot:
1. Velger kategori  
2. Tegner på kart  
3. Fyller inn detaljer  
4. Sender inn hinder  

### Approver/Admin:
1. Ser innsendte hindre  
2. Godkjenner eller avviser  
3. Kommentar lagres med tidsstempel  

---

## Distribusjon

- Kjøres via **Docker Compose**  
- Web + database starter i egne containere  
- Filsystemet for bilder (`wwwroot/uploads`) er lokalt persistent

---
