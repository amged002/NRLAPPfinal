# Testing

Denne guiden beskriver hvordan du kjører og vedlikeholder testene for NRL Hindermeldingssystem.

## Krav
- .NET SDK 9.0 installert lokalt.
- Ingen database- eller Docker-avhengigheter er nødvendige for enhetstestene.

## Testoversikt
| Type | Prosjekt/sti | Dekker |
| ---- | ------------- | ------ |
| Enhetstester (xUnit) | `NRLApp.Tests` | Validator- og kontrolllogikk (bl.a. `ObstacleMetaValidator`, `AccountController`, `HomeController`, `ContactController`) samt viewmodell-oppførsel (`ErrorViewModel`). |
| Manuell funksjonell verifikasjon | Produksjons- og QA-miljøer | Registrering av hinder, roller og tilgangsstyring som beskrevet i README. |

## Prosjektstruktur
- `NRLApp.Tests/NRLApp.Tests.csproj` – testprosjekt (.NET 9).
- `NRLApp.Tests/Controllers/AccountControllerTests.cs` – pålogging, registrering og utlogging.
- `NRLApp.Tests/Controllers/SimpleControllersTests.cs` – `HomeController` og `ContactController` returnerer forventede views.
- `NRLApp.Tests/Models/ErrorViewModelTests.cs` – `ShowRequestId`-logikk.
- `NRLApp.Tests/Models/Obstacles/ObstacleMetaValidatorTests.cs` – validering av hinder-metadata og høydekonvertering.

## Kjøre tester
Kjør kommandoene fra repo-roten.

### Alle tester
```bash
dotnet test NRLApp.sln
```

### Kun testprosjektet
```bash
dotnet test NRLApp.Tests/NRLApp.Tests.csproj
```

### Filtrere på navnerom/klasse/testnavn
```bash
dotnet test NRLApp.Tests/NRLApp.Tests.csproj --filter FullyQualifiedName~AccountControllerTests
```

### Rask validering av én fil/klasse
```bash
dotnet test NRLApp.Tests/NRLApp.Tests.csproj --filter ObstacleMetaValidator
```

## Feilsøking
- **SDK mangler**: Installer .NET 9.0 SDK fra Microsofts nedlastinger og verifiser med `dotnet --version` før du kjører testene.
- **Feil referanser**: Kjør `dotnet restore` i rotmappen hvis NuGet-pakker mangler.
- **Uventet testoppførsel**: Sørg for at miljøvariabler og appsettings for lokale miljøer ikke påvirker controller-logikken når du kjører testene.

### Mål
Verifisere funksjonell flyt for:
- Innlogging
- Registrering av hinder med bilde
- Saksbehandling (godkjenning/avslag)
- Kartvisning av godkjente/ventende hinder
- Rollebasert tilgangskontroll
- Sikkerhetsmekanismer (rate limiting, CSRF, filvalidering)

### Testmiljø
- Kjøring via **Visual Studio + Docker Compose** (ASP.NET Core 9 + MariaDB).
- Dapper + parameteriserte SQL-queries.
- Browser: Edge/Chrome.
- Kultur: `nb-NO`.

### Roller testet
- **Pilot**
- **Approver**
- **Admin**

### Testdata
- Seedet admin-bruker.
- Manuelt opprettede Pilot- og Approver-brukere.
- Flere hinder med og uten bilde.

---

## Testscenarier

### Pilot registrerer hinder (inkl. bilde)
1. Pilot logger inn.
2. Fyller ut hinderdata → velger bilde → sender inn.
3. Systemet lagrer:
   - GeoJSON
   - Metadata
   - Bilde i `/wwwroot/uploads`
   - Status = *Pending*
4. Kvitteringsside vises.

**Forventet resultat:** Hinder lagret i DB, bilde ligger lagret, vis/skjul-knapp fungerer på detaljsiden.

---

### Approver / Admin oppdaterer status
1. Bruker med rolle *Approver* eller *Admin* åpner detaljer.
2. Legger inn kommentar og velger **Godkjenn** eller **Avvis**.
3. POST er beskyttet med:
   - `[Authorize(Roles="Admin,Approver")]`
   - `[ValidateAntiForgeryToken]`
4. Status og kommentar lagres i databasen.

**Forventet resultat:** Status endres korrekt og vises på kart/oversikt.

---

### Pilot ser hinder på kart
1. Pilot åpner kartvisningen.
2. API-et returnerer *Approved + Pending* hinder i GeoJSON.
3. Leaflet viser punkter med korrekt høyde, plassering og ikon.

**Forventet resultat:** Kartet viser alle relevante hinder.

---

### Server-side validering
Testet:
- Manglende høyde
- Negativ høyde
- Ugyldige bildeformater
- For store filer
- Feil enhetsvalg

**Forventet resultat:** Feilmeldinger vises, ingenting lagres.

---

### Rollebeskyttede ruter
Forsøk utført av Pilot:

| Rute forsøkt | Forventet | Resultat |
|--------------|-----------|----------|
| `/Obstacle/Delete/ID` | Forbudt | Avvist |
| `/Obstacle/Approve/ID` | Forbudt | Avvist |
| `/Admin/*` | Forbudt | Avvist |

**Forventet resultat:** 403 eller redirect → oppfylt.

---

### Sikkerhetsbelastning – rate limiting
Kjørt 15 raske forespørsler mot `/Obstacle/Meta`.

**Forventet resultat:**  
- Første 10 = OK  
- 11–15 = blokkert av rate limiter  

**Resultat:** Bestått.

---

### CSRF-beskyttelse
- POST uten token → Avvist
- Manipulert token → Avvist

**Resultat:** Bestått.

---

### Bildehåndtering
Testet:
- Last opp bilde → Bekreftet lagret
- Se bilde → vis/skjul fungerer
- Slette hinder → bildefilen slettes automatisk

**Resultat:** Bestått.

---

### URL-manipulasjon / direkte ID-tilgang
- Pilot forsøker å endre hinder som tilhører andre
- Pilot prøver å redigere slettet hinder
- Approver prøver å slette noe uten tilgang

**Forventet resultat:** Redirect til liste eller 403.  
**Resultat:** Bestått.

---

### Unit-testing (egen testmappe)
Prosjektet inneholder en mappe for **unit-tester**, blant annet:

- Validering av høydekonvertering
- Parsing/lagring av GeoJSON
- Test av vis/skjul-bilde-logikk
- Kontroll av at feil ID returnerer null/404

**Resultat:** Alle enhetstester passerte.
<img width="615" height="1019" alt="image" src="https://github.com/user-attachments/assets/ea71d09b-3f64-43e7-bcdd-541fb75fd555" />


---

## Testlogg og resultater

| Dato | Scenario | Rolle | Resultat |
|------|----------|-------|----------|
| 01/12/2025 | Pilot registrerer hinder med bilde | Pilot | OK |
| 01/12/2025 | Bilde vises + vis/skjul fungerer | Pilot | OK |
| 01/12/2025 | Approver godkjenner hinder | Approver | OK |
| 01/12/2025 | Avvis hinder med kommentar | Approver | OK |
| 01/12/2025 | Pilot ser Approved + Pending i kart | Pilot | OK |
| 01/12/2025 | Ugyldig skjema gir valideringsfeil | Pilot | OK |
| 01/12/2025 | Pilot prøver Approver-endepunkt | Pilot | Avvist |
| 01/12/2025 | Rate limiting etter 10 forespørsler | Pilot | OK |
| 01/12/2025 | POST uten CSRF-token | Pilot | Avvist |
| 02/12/2025 | Bilde slettes når hinder slettes | Admin | OK |
| 02/12/2025 | URL-manipulasjon for å endre andres hinder | Pilot | Avvist |
| 02/12/2025 | Unit-test: GeoJSON parsing | System | OK |

---

## Oppsummering

Alle funksjonelle og sikkerhetsrelaterte testscenarier ble gjennomført og bestått.  
Systemet er stabilt, rollebasert tilgang kontrolleres korrekt, og sikkerhetsmekanismer fungerer som forventet.

