# Vecka 1 – Uppstart och planering

## Översikt
Första veckan har fokuserat på uppstart, planering och dokumentationsstruktur.  
Målet har varit att skapa en tydlig grund för vidare utveckling, med fokus på ordning, modularitet och spårbarhet. 

---

## Genomförda aktiviteter

- Skapat **grundstruktur för repository** (docs/, infra/, etc.)
- Skrivit **README.md** och lagt till **MIT License**
- Lagt till **.gitignore** anpassad för .NET och lokala miljöer
- Etablerat solution för hela projektet (clo24-nikhal78-examwork.sln)
- Skapat katalogerna:
  - `docs/initial_outtakes/` → tidiga planer och översikter  
  - `docs/adr/` → Architecture Decision Records  
  - `docs/journal/` → veckologg
  - `src/` → källkod (api, web, shared)
- Dokumenterat:
  - **system_overview.md** – projektbeskrivning, mål och verktyg  
  - **architecture.md** – applikations- och eventflöden (ASCII-diagram)
  - **cosmos-ticketing-considerations.md** – data- och domänöverväganden för boknings- och biljettmodellen
- Skapat sex ADR:er (001–006) för val av Cosmos DB, Auth, IaC, Frontend, Azure Services och Eventdriven arkitektur
- Påbörjat kodstruktur för MVP:
  - /src/shared/Ticketing.Contracts/ med projektfil och BookingDto-förberedelse
  - /src/api/Ticketing.Api/ med projektfil, referens till Contracts och minimal Program.cs
  - Etablerad solution-fil som kopplar samman projekten

---

### Reflektion
 
Strukturen är nu på plats och utformad för att vara skalbar, modulär och enkel att containerisera i nästa steg. Arbetet har följt principen ”en sak i taget”. 

---

## Nästa steg (vecka 2)

- Implementera BookingsController med BookingDto för att möjliggöra första API-testet.
- Skapa enkel Blazor-frontend (landing page) för att bekräfta helheten lokalt.
- Förbereda infrastrukturmapp (infra/) med grundläggande Bicep-struktur.
- Planera container-upplägg för API, Web och Functions.
- Påbörja utkast till CI/CD-pipeline (GitHub Actions) med fokus på dev-miljön.