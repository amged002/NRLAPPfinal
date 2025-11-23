# NRL Hindermeldingssystem  
Et komplett system for registrering, behandling og godkjenning av luftfartshindre.  


---

# Innholdsfortegnelse
1. Oversikt  
2. Systemarkitektur  
3. Hvordan kjøre systemet (Docker)  
4. Brukerroller og tilgangskontroll  
5. Funksjonalitet  
6. Datamodell (MariaDB)  
7. WMS / Permalenker / Kartlag (Avansert)  
8. Organisasjoner (Avansert)  
9. Sikkerhetstiltak  
10. Testing  
11. Dokumentasjonsstruktur  
12. Videre arbeid  

---

# 1. Oversikt

NRL Hindermeldingssystem lar piloter og crew registrere hindringer direkte i kart, samt lar registerfører (Approver) behandle og godkjenne disse.

Systemet tilbyr:

✔ ASP.NET Core MVC  
✔ ASP.NET Identity (brukere + roller)  
✔ Leaflet kartløsning  
✔ MariaDB via Docker  
✔ Dapper for spørringer  
✔ Pilot, Crew, Approver og Admin-roller  
✔ Mobiltilpasset frontend  

---

# 2. Systemarkitektur

```
┌──────────────────────────┐
│     Nettleser / Klient   │
│  Pilot / Crew / Approver │
│    Leaflet + Bootstrap   │
└───────────┬──────────────┘
            │ HTTP (MVC)
┌───────────▼──────────────┐
│   ASP.NET Core Backend   │
│ Kontrollere:             │
│  - Account               │
│  - Admin                 │
│  - Obstacle              │
│ Identity / Dapper        │
└───────────┬──────────────┘
            │ SQL
┌───────────▼──────────────┐
│      MariaDB (Docker)    │
│  obstacles + aspnetusers │
└──────────────────────────┘
```

---

# 3. Hvordan kjøre systemet (Docker)

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

# 4. Brukerroller og tilgangskontroll

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
- Kan slette brukere
- Har ikke tilgang til hindersystemet
- Landingsside → **Admin/Users**

---

# 5. Funksjonalitet

## A) Registrering av hinder
Brukeren tegner i kartet via Leaflet:
- Punkt
- Linje
- Polygon

GeoJSON lagres direkte i MariaDB.

## B) Metadata-utfylling
- Kategori
- Høyde (meter eller fot)
- Beskrivelse
- Lagre som utkast

## C) Hindertabell
Pilot/Crew → kun egne  
Approver → alle

Filtrering på:
- ID
- Navn
- Høydeintervall
- Status
- Dato

## D) Godkjenning / Avvisning
Approver kan:
- Godkjenne
- Avvise
- Legge inn kommentar
- Tildeles som “saksbehandler”

---

# 6. Datamodell (MariaDB)

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

# 7. WMS / Permalenker / Kartlag (Avansert)

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

# 8. Organisasjoner (Avansert krav)

Systemet støtter organisasjoner gjennom:
- `organization_id` i obstacles-tabellen
- Kan utvides så Approver kun ser hindere fra egen organisasjon
- Identity kan utvides med organisasjonsfelt

---

# 9. Sikkerhetstiltak

✔ ASP.NET Identity – sikrede passord  
✔ Rollebasert tilgang – `[Authorize(Roles="...")]`  
✔ Anti-forfalskningsbeskyttelse via `@Html.AntiForgeryToken()`  
✔ Server-side validering  
✔ Klientvalidering via jQuery Validate  
✔ Ingen SQL-injeksjon (parameteriserte spørringer via Dapper)  
✔ Pilot/Crew isoleres til egne hindere  
✔ Admin kan ikke utføre hindermelding  
✔ Passord lagres som salted hash  

---

# 10. Testing

## A) Enhetstesting (manuelle)
- Konvertering ft → meter
- Dato-normalisering til UTC
- Roller → riktig redirect etter login
- Pilot får ikke tilgang til Approver/Admin-sider

## B) Systemtesting
- Registrere hinder
- Redigere / slette hinder
- Filterfunksjoner
- Godkjenning / avvisning
- Endre rolle
- Opprette ny bruker

## C) Sikkerhetstesting
- SQL-injeksjon: blokkeres av Dapper-parametere  
- XSS-forsøk i felt  
- CSRF: tester POST uten token → avvist  
- Forsøk på tilgang til /Admin → avvist for ikke-admin  

## D) Brukervennlighet
- Testet på mobil via Chrome DevTools  
- Kart fungerer med touch  
- Større knapper etter brukertesting  

---

# 11. Dokumentasjonsstruktur

Repo inneholder:
- `README.md` (denne filen)
- `docker-compose.yml`
- MVC-projektstruktur
- Kommentarer i kontrollerne
- Databasediagram i markdown

---

# 12. Videre arbeid

Forslag til neste steg:
- Integrasjon mot Kartverket WMS
- Eksponering av eget API
- Dashboard / bedre rapportfunksjon
- Push-varsler ved endret status
- GIS-export av hendelser
- Multi-organisasjonsfiltrering

---

# Oppsummering
Systemet oppfyller:

✔ Docker-miljø  
✔ MariaDB-tilkobling  
✔ Identitet og registrering  
✔ Autentisering / Autorisering  
✔ Datastruktur for hinder  
✔ Karttegning (punkt/linje/område)  
✔ Pilot og Approver-rolle  
✔ Mobiltilpasset frontend  
✔ Sikkerhetstiltak  
✔ Testing  
✔ Full dokumentasjon  

### Start systemet:
```
docker compose up --build
```

### Admin-innlogging:
```
admin@nrl.local
Admin!123!
```
