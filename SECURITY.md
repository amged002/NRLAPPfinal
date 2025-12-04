# Sikkerhetsdokumentasjon

## Oversikt over tiltak

### **Autentisering og autorisasjon**
- ASP.NET Core Identity med roller: **Pilot**, **Approver**, **Admin**.  
- Kritiske operasjoner (godkjenning, avslag, sletting, endring) er beskyttet via `[Authorize(Roles = "...")]`.  
- Pilot kan ikke få admin/approver-tilgang via registrering – roller tildeles manuelt.

### **Ressurskontroll og misbrukshåndtering**
- **Rate limiting** er aktivert på ObstacleController (10 requests per 10 sekunder, med kø).  
- Sensitive GET- og POST-operasjoner krever innlogging og riktig rolle.

### **CSRF-beskyttelse**
- Alle POST-endepunkter bruker:  
  `✔ [ValidateAntiForgeryToken]`  
- Hindrer uautoriserte forespørsler fra eksterne nettsteder.

### **XSS-beskyttelse**
- Razor HTML-encoder alle brukergenererte verdier automatisk.  
- Ingen `Html.Raw()`-bruk for innsendt data.  
- Bilder renderes kun som filer fra vår server, aldri som arbitrary HTML.

### **SQL-injection-beskyttelse**
- Alle queries bruker **parameterisert SQL via Dapper**.  
- Ingen direkte stringmanipulerte SQL-setninger brukes.

### **Inputvalidering**
- Høyde, kategori, beskrivelse og bilde valideres server-side.  
- Tillatte bildeformater: `.jpg`, `.jpeg`, `.png`, `.webp`.  
- Filnavn genereres via GUID for å forhindre path traversal.

### **Filopplasting / filhåndtering**
- Bilder lagres i `wwwroot/uploads` med GUID-navn.  
- Ved sletting av hinder slettes også bildefilen.  
- Ingen mulighet for å overskrive eksisterende filer.

### **Feilpassordhåndtering**
- Feil påloggingsinfo gir kun generisk feilmelding.  
- Identity håndterer lockout hvis aktivert.

---

## Misbruksscenarier og mitigering

### **1. Pilot forsøker privilegieeskalering**
- Roller kan ikke tildeles gjennom registrering.  
- Forsøk på admin/approver-endepunkter → 403 Forbid.

### **2. Spam eller brute force på hinderegistrering**
- Rate limiter stopper overbruk.  
- POST krever innlogging + gyldig anti-CSRF token.

### **3. Manipulert filopplasting**
- Kun godkjente bildeformater.  
- Filnavn overskrives → ingen path traversal-angrep.

### **4. Forsøk på å redigere/slette andres hinder**
- Endringsoperasjoner sjekker både rolle og eierskap.  
- Manuell URL-manipulasjon gir redirect eller forbudt.

### **5. XSS-angrep via tekstfelt**
- Razor encoding stopper scripts i tekstfelt.  

### **6. SQL-injection**
- Parameteriserte queries gjør injeksjon umulig.

### **7. CSRF mot godkjenning/avslag**
- `Approve` og `Reject` krever AntiForgery-token.

### **8. Bildespoofing**
- Brukerens bilde-URL renderes aldri direkte.  
- Kun intern lagrede filer vises.

---

## Operasjonelle rutiner

### **Admin-konto**
- Adminbruker og roller seedes automatisk hvis ikke eksisterende.  
- Admin-passord skal roteres etter innlevering.

### **Database**
- Tilkoblingsstrenger settes via Docker Compose eller appsettings.  

### **Eksterne CDN-er**
- Leaflet og Bootstrap lastes fra CDN → CSP anbefales for produksjon.

### **Filhåndtering**
- `wwwroot/uploads` må være skrivbar.  
- Vurder filstørrelsesbegrensning i produksjon.

---

## Sikkerhetstesting (kortlogg)

| Test | Scenario | Resultat |
|------|----------|----------|
| **Autorisasjon** | Pilot prøver admin-/approver-sider | Avvist |
| **CSRF** | POST uten antiforgery-token | Avvist |
| **Rate limiting** | 15 raske forespørsler | Sperret etter 10 |
| **SQL injection** | `id=1 OR 1=1` i URL | Blokkert |
| **XSS** | `<script>alert(1)</script>` i beskrivelse | Vises som tekst |
| **Filangrep** | Lasting av .exe/.js | Avvist |
| **Rolleeskalering** | Pilot sender POST til Approve/Reject | Avvist |

---

## Oppsummering

Systemet vårt beskytter mot:

- XSS  
- SQL injection  
- CSRF  
- Uautorisert tilgang  
- Filopplastingsangrep  
- Spam/bruteforce gjennom rate limiting  

Kritiske endepunkter er rollebeskyttet, og all data valideres server-side.

Systemet tilfredsstiller solide sikkerhetskrav for et studentprosjekt på ASP.NET Core.

