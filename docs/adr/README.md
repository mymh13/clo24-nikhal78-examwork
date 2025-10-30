# Architecture Decision Records (ADR)
 
Denna katalog innehåller dokumentation av viktiga tekniska beslut i projektet.  
Syftet är att skapa spårbarhet mellan beslut, motiv och eventuella framtida ändringar. 
 
Varje ADR beskriver **ett beslut**, dess **bakgrund (context)**, **alternativ** och **konsekvenser**.  
Status anger var i livscykeln beslutet befinner sig: `Proposed`, `Accepted`, `Rejected` eller `Superseded`. 
 
---
 
## Index över ADR:er 
 
#### OBS! Nedanstående är ett exempel tills dess jag fyllt den med korrekt innehåll!
 
|    Nr   |                       Titel                       | Status   | Datum      | Kommentar  |
|---------|---------------------------------------------------|----------|------------|------------|
| ADR-001 | Val av databas: Azure Cosmos DB (Serverless)      | Accepted | 2025-10-30 | Kostnads- och driftoptimering
| ADR-002 | Autentisering: ASP.NET Identity + Entra ID        | Proposed | 2025-10-30 | Delad modell för kund/admin
| ADR-003 | Infrastruktur som kod (IaC) – verktygsval         | Proposed | 2025-10-30 | Bicep, ARM eller Terraform
| ADR-004 | Eventdriven arkitektur via Service Bus + Function | Planned  | 2025-XX-XX | Aktiveras efter MVP
 
---
 
### Namngivning och format
- Filnamn: `ADR-###-example.md` (stigande nummer, tre siffror).  
- Rubriker: `Context`, `Decision`, `Consequences`, `Status`, `Alternatives`, `References`.  
- Statusvärden: `Proposed`, `Accepted`, `Rejected`, `Superseded`.  
- När ett beslut ersätts flyttas den gamla ADR:n till `_archive/`.
 
---
 
### Syfte
ADR:erna fungerar som en **beslutslogg** för systemarkitekturen.  
De hjälper framtida utvecklare att förstå **varför** ett beslut togs, inte bara **vad** som gjordes.
 
---
 
### Disclaimer
 
ADR-000-template.md har tagits fram av en LLM-modell. 
Jag har beskrivit syftet och vad jag vill ha, den har genererat en template som jag sedan reviewat och justerat. 
 
Alla följande ADR-dokument har jag bett den fylla i efter mallen och efter vad jag vill bygga. Jag har reviewat varje modell och justerat där det känns relevant.