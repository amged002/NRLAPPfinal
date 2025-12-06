# NRLApp – IS-202 Programmeringsprosjekt (Høst 2025)

NRLApp er en ASP.NET Core 9 MVC-applikasjon som lar piloter registrere luftfartshindre via karttegning og skjema, og gjør det mulig for Approvere og Admin å behandle og godkjenne innsendelser.  
Løsningen kjører i Visual Studio gjennom Docker Compose, som starter både webapplikasjonen og MariaDB-databasen.

Dette prosjektet ble utviklet av **Gruppe 15** som del av IS-200 Programmeringsprosjekt ved Universitetet i Agder.

---

# Teknologi og nøkkelfunksjoner

- **ASP.NET Core 9 MVC**
- **Identity med roller:** Pilot, Approver, Admin
- **MariaDB** (via Dapper + EF Core)
- **Leaflet-kart** (punkt og linje)
- **Sikker bildeopplasting** med validering og automatisk sletting
- **Filtrering** (kategori, status, organisasjon, høyde, dato)
- **Rolle- og ID-basert tilgangskontroll**

# Hvordan kjøre systemet (Docker)

### ▶ Start systemet
```
docker compose up --build
```

Systemet starter på:  
🔗 **http://localhost:8080**

---

### ⏹ Stopp systemet
```
docker compose down
```

### 🗑 Slett databasevolumer
```
docker compose down -v
```

---

### 👤 Standard admin-bruker
Brukes for å gi roller til nye brukere.

**E-post:** `admin@nrl.local`  
**Passord:** `Admin!123!`

---

# Brukerroller og tilgangskontroll

## Pilot / Crew
- Kan registrere hinder (punkt / linje / område)
- Ser **kun sine egne hindere**
- Kan endre/slette egne
- Landingsside etter innlogging → **Obstacle/Area**

## Approver (Registerfører)
- Ser **alle** hindere
- Kan godkjenne / avvise
- Kan skrive vurderingskommentar
- Landingsside → **Obstacle/List**

## Admin
- Kan tildele roller
- Kan tildele Orgnisasjon 
- Kan slette brukere
- Har ikke tilgang til hindersystemet
- Landingsside → **Admin/Users**

---

# Funksjonalitet

## A) Registrering av hinder
Brukeren tegner i kartet via Leaflet:
- Punkt
- Linje

GeoJSON lagres direkte i MariaDB.

## B) Metadata-utfylling
- Opplasting av bilde (Valgfritt)
- Kategori
- Høyde (meter eller fot)
- Beskrivelse
- Lagre som utkast

## C) Hindertabell
Pilot/Crew → kun egne  
Approver → alle

Filtrering på:
- ID
- Kategori
- Høydeintervall
- Status
- Dato
- Organisasjon

## D) Godkjenning / Avvisning
Approver kan:
- Godkjenne
- Avvise
- Legge inn kommentar
- Tildeles som “saksbehandler”

---

# Datamodell (MariaDB)

### Tabell: `obstacles`

| Felt | Type | Beskrivelse |
|------|------|-------------|
| id | int | Primærnøkkel |
| geojson | longtext | hindergeometri |
| obstacle_category | varchar | kategori |
| obstacle_name | varchar | navn |
| height_m | int | høyde i meter |
| description | text | beskrivelse |
| is_draft | tinyint | utkast |
| created_by_user_id | varchar | FK til AspNetUsers |
| assigned_to_user_id | varchar | saksbehandler |
| review_status | varchar | Approved/Rejected/Pending |
| review_comment | text | vurdering |
| created_utc | datetime | tidsstempel |

---

# WMS / Permalenker / Kartlag (Avansert)

Dette er gjort klart i arkitekturen og kan bygges ut videre.

## WMS (Kartverket)
- Støtte for WMS-lag via Leaflet:
  ```
  L.tileLayer.wms(url, { layers: '...', format: 'image/png' })
  ```
- Kan brukes for offisielle bakgrunnskart.

## Permalenker
- Kartposisjon og zoom kan deles som URL-parametere.
- Geometri kan inkluderes i URL eller hentes fra DB.

## GeoJSON 
- All geometri lagres som standard GeoJSON.
- Enkelt å eksportere til GIS-verktøy.

---
## Dokumentasjon

- [Systemarkitektur](./Systemarkitektur.md)
- [Mobiltilpasning](./Mobiltilpasning.md)
- [Testing](./Testing.md)

Team
Dette prosjektet ble utviklet av Gruppe 15:
- Amgad
- Yousef
- Storm
- Joachim
- Filip
- Marius

