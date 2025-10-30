# Architecture Decision Records (ADR)
 
Denna katalog innehåller dokumentation av viktiga tekniska beslut i projektet.  
Syftet är att skapa spårbarhet mellan beslut, motiv och eventuella framtida ändringar. 
 
Varje ADR beskriver **ett beslut**, dess **bakgrund (context)**, **alternativ** och **konsekvenser**.  
Status anger var i livscykeln beslutet befinner sig: `Proposed`, `Accepted`, `Rejected` eller `Superseded`. 
 
---
 
## Index över ADR:er 
 
Senast uppdaterad: 2025-10-30  
 
|    Nr   |                       Titel                                     | Status   | Datum      | Kommentar  |
|---------|-----------------------------------------------------------------|----------|------------|------------|
| ADR-001 | Val av databas: Azure Cosmos DB (Serverless)                    | Accepted | 2025-10-30 | Kostnads- och driftoptimering |
| ADR-002 | Autentisering: ASP.NET Identity + Entra ID                      | Accepted | 2025-10-30 | Delad modell för kund/admin |
| ADR-003 | Infrastructure as Code (IaC) – verktygsval: Bicep               | Accepted | 2025-10-30 | Enkel integration i Azure DevOps |
| ADR-004 | Val av frontend: .NET 8 Blazor Server                           | Accepted | 2025-10-30 | Hel .NET-stack och enkel hosting |
| ADR-005 | Val av molntjänster: App Service, App Config, Key Vault, App Insights och APIM | Accepted | 2025-10-30 | Centrala Azure-komponenter för drift |
| ADR-006 | Eventdriven arkitektur: Service Bus + Function + Outbox Pattern | Planned  | 2025-10-30 | Aktiveras efter MVP |
 
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