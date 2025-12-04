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

## Videre arbeid
- Legg til integrasjonstester med en in-memory webserver (`WebApplicationFactory`) for å dekke routing, modellbinding og autentisering.
- Utvid modell- og valideringstestene etter hvert som nye felter legges til hinder, kontoer eller andre domenemodeller.
- Kjør testene automatisk i CI (GitHub Actions/Azure DevOps) med `dotnet test` for å fange regresjoner tidlig.