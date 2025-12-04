# 🔎 Testing og testresultater

Dette dokumentet beskriver manuelle tester utført på NRLApp.

---

# ✔ Funksjonell testing

## Pilot
- Registrering av hinder → vises i liste som "Avventer"
- Utkast → lagres og kan fullføres senere

## Approver/Admin
- Godkjenning/avvisning oppdaterer status og kommentar
- Liste filtrerer på status, dato, organisasjon, høyde, kategori

---

# ✔ Sikkerhetstesting

## IDOR
Pilot2 forsøker å åpne Pilot1 sine hindre:  
→ *tilgang nektet (404/Forbid)*

## Rollebeskyttelse
Pilot prøver å åpne Admin/Approve-endepunkter:  
→ *nektes tilgang*

## CSRF
POST uten anti-forgery token:  
→ *avvist (400)*

## Innlogging
Feil passord flere ganger:  
→ *lockout aktivert*

## Filopplasting
Feil filformat:  
→ *avvist med norsk valideringsfeil*  
Slettet hinder:  
→ *tilhørende bildefil slettes*

---

# 🧪 Testlogg (kort)

Dato | Scenario | Rolle | Resultat
-----|----------|-------|---------
2025-01 | Registrering av hinder | Pilot | OK
2025-01 | Godkjenning/avvisning | Approver | OK
2025-01 | Listefiltrering | Alle | OK
2025-01 | IDOR/URL-manipulasjon | Pilot | OK
2025-01 | CSRF-testing | System | OK
2025-01 | Lockout ved feil passord | Pilot | OK
