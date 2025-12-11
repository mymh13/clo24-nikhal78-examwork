**Högsta prio**:
- Presentationen av problemet
- ..och lösningen

**Tekniska val**:
- Presentera dem
- Motivera dem

**Begränsningar**:
- Ca 15 min presentation OCH demo
- Ca 10 min frågor samt opposition
- Publiken är "supercoola kodare, så presentera hellre teknik än övergripande"

**Tankar kring demot**:
- Demot skall fokusera på Insight och Toggle-feature, resterande funktionalitet kan kort nämnas. Ca 4 min?
- Presentationen av de tekniska lösningarna är det tunga, ca 11 min?

---

**Introduktionen**:
- Vad har byggts, hur löser det problemet, vad är kärnfunktionalitet? punktlista
- Ex: "Kunden har idag en applikation där ett äldre och ett nyare system fungerar sida vid sida. Vissa verktyg har refaktorerats och integrerats i det nya verktyget, medan andra lever kvar i det äldre. På grund av det så arbetar kunden i dagsläget i två system, samtidigt.

Målet med detta projekt är att skapa ett gränssnitt för att kunna hantera båda systemen inom samma applikation, och att kunna växla mellan dem med en enkel knapptryckning. Detta möjliggör en smidig övergång från det gamla systemet till det nya, utan att behöva stänga av det gamla systemet helt.

För att uppnå detta har vi valt att använda Microsofts .NET-ekosystem tillsammans med Azure-molntjänster. Detta val motiveras av att kunden redan använder Microsoft-teknologier i stor utsträckning, vilket gör integrationen enklare och mer kostnadseffektiv."
- Exemplet ovan måste kortas ner, men den kärnan.

---

**Förslag på presentations-ordning**:
1. Introduktionen
2. Visa huvudflöde (återanvänd från verktygspresentationen!) (grov överblick av trafik, inte UML)
3. Gå över till demot och visa hur lösningen fungerar
- Visa övergripande funktionalitet på bokningen, någon minut, lägg nästan all tid på toggle-switchen
- Berätta vad som skall visas / vad som kommer
- Trigga funktioner som ex toggle switch medan jag pratar, låt den arbeta medan jag jobbar
- Berätta vad vi förväntas se, presentera det sedan på Insights
4. Nämn arbetsmetodik, jobbat i faser som brutits ner i delmoment, simulerar agilt utan att jobba med sprint, iterativt
5. Via repo-struktur
6. Presentera de olika delarna i var sin slide:
- 6a: Infrastruktur, IaC och CICD
- 6b: .NET, Blazor Server, Tester
- 6c: Azure och CosmosDB serverless
- 6d: Sentinel Key, Outbox pattern centrala för hela projektet
7. Lärdomar från projektet
- Några stora lärdomar
- Hur kan projektet utvecklas
8. Avslutning, tack för mig plus QR-kod till repot

---

**Slide 1** - Introduktion
- Kunden arbetar idag i två system parallellt (äldre + nyare)
- Mål: Ett gränssnitt för att hantera båda systemen
- Kärnfunktionalitet: Toggle-knapp för att växla mellan synkront och eventdrivet system
- Teknologistack: .NET 8 + Azure-molntjänster
- Motivering: Kunden använder redan Microsoft-teknologier
  
**Slide 2** - Huvudflöde
**Synkront system:**
Användare → Blazor Server → API → Cosmos DB

**Eventdrivet system:**
Användare → Blazor Server → API → Outbox → Service Bus → Azure Functions → Cosmos DB

**Toggle-knappen:** Växlar mellan systemen via Azure App Configuration feature flags

---

**DEMO**

---
  
**Slide 3** - Demo (4 min)
**Övergripande funktionalitet (1 min):**
- Visa bokningsflöde: skapa, aktivera, visa QR-kod
- Visa admin-dashboard: användare, bokningar, statistik

**Toggle-switchen (3 min):**
- Visa toggle-knappen i admin-dashboard
- Förklara vad som ska hända
- Aktivera eventdrivet system
- Visa i Application Insights: Service Bus-meddelanden, Functions-aktivering
- Växla tillbaka till synkront system
- Visa skillnaden i Insights

---

**SLUT PÅ DEMO**

---
  
**Slide 4** - Arbetsmetodik
- Iterativt arbete i faser
- Varje fas bröts ner i delmoment
- Simulerar agilt arbete utan formella sprints
- Dokumentation via ADR (Architecture Decision Records)
- Kontinuerlig refaktorering och förbättring
- Testning integrerad i varje fas
  
**Slide 5** - Repo-struktur
```
clo24-nikhal78-examwork/
├─ docs/                    # Dokumentation
│  ├─ adr/                  # Architecture Decision Records
│  ├─ journal/              # Veckovis logg
│  └─ bugs_and_improvements/
├─ src/
│  ├─ web/                  # Blazor Server (UI + API)
│  ├─ functions/            # Azure Functions
│  └─ shared/                # Delade kontrakt
├─ infra/                   # Bicep-templates
│  └─ modules/              # Återanvändbara moduler
└─ .github/workflows/       # CI/CD pipelines
```
  
**Slide 6a** - Infrastruktur, IaC och CI/CD
**Infrastructure as Code:**
- **Bicep** - Azure-infrastruktur som kod
- **Modulär struktur** - Återanvändbara Bicep-moduler
- **Resource Groups** - Separata för dev/prod
- **Managed Identity** - RBAC-baserad autentisering

**CI/CD Pipeline:**
- **GitHub Actions** - CI/CD-plattform
- **Docker** - Containerisering av applikation
- **GitHub Container Registry (GHCR)** - Container-lagring
- **Multi-stage builds** - Optimering av container-storlek
- **Separated CI/CD** - CI bygger, CD deployar
- **OIDC-autentisering** - Säker Azure-autentisering utan credentials
- **Environment protection** - GitHub Environments med secrets
- **Workflow triggers** - Push, PR, manual, workflow_run

**Deployment:**
- **Web App:** Docker-containrar via GHCR
- **Function App:** Zip-deploy (snabbare för Functions)
- **Idempotent deployments** - Säker att köra flera gånger
  
**Slide 6b** - .NET, Blazor Server, Testning
**.NET 8 Stack:**
- **.NET 8** - Senaste LTS-versionen
- **C#** - Ett språk för frontend och backend
- **ASP.NET Core** - Web framework
- **Blazor Server** - UI-framework med SignalR
- **Razor Components** - Komponentbaserad UI
- **Dependency Injection** - Inbyggd IoC-container

**Arkitektur och mönster:**
- **Controller-based JSON API** - RESTful endpoints
- **Repository Pattern** - Dataåtkomst-abstraktion
- **Service Layer** - Affärslogik-separation
- **Extension Methods Pattern** - Startup-konfiguration (ADR-009)
- **Error Handling Helpers** - Centraliserad felhantering
  - `ErrorHandlerHelper` - API-controllers
  - `HttpErrorHelper` - Razor pages
- **Navigation Helper** - Rollbaserad navigering

**Testning:**
- **xUnit** - Testramverk
- **NSubstitute** - Mocking-bibliotek
- **Unit tests** - Affärslogik och priskalkylering
- **Integration tests** - Fullstack-testning med in-memory mocks
- **InMemoryStorage** - Mock-implementationer för tester
  - `InMemoryUserService`
  - `InMemoryBookingService`
  - `InMemoryOutboxService`

**Bibliotek:**
- **QRCoder** - QR-kodgenerering
- **System.Text.Json** - JSON-serialisering
- **Microsoft.Extensions.*** - Konfiguration, logging, dependency injection
  
**Slide 6c** - Azure och Cosmos DB Serverless
**Azure-tjänster (implementerade):**
- **App Service (Basic B1)** - Hosting för Blazor Server och API
- **Cosmos DB (Serverless)** - NoSQL-databas för bookings, users, outbox
- **Service Bus (Basic)** - Meddelandekö för eventdriven arkitektur
- **Azure Functions (Basic B1)** - Serverless event-bearbetning
- **Application Insights** - Logging, monitoring, telemetry, KQL-queries
- **App Configuration (Free/Standard)** - Feature flags och konfiguration
- **Key Vault (Standard)** - Säker lagring av secrets
- **Storage Account (Standard LRS)** - Functions runtime-storage

**Cosmos DB:**
- **Serverless-läge** - Betala per användning
- **Partition keys** - Effektiv querying
- **Containers:** bookings, users, outbox
- **Transactional consistency** - Atomiska operationer

**Konfiguration och säkerhet:**
- **Managed Identity** - RBAC-baserad autentisering
- **Key Vault integration** - Secrets via managed identity
- **App Configuration labels** - Miljöspecifik konfiguration
- **Feature flags** - Runtime-konfiguration utan deployment
  
**Slide 6d** - Sentinel Key och Outbox Pattern
**Sentinel Key Pattern (ADR-014):**
- **Hot-reload** - Konfigurationsuppdatering utan restart
- **Zero-downtime switching** - Växla mellan system i runtime
- **Sentinel key:** `Settings:Sentinel` - Trigger för refresh
- **Refresh interval:** 30 sekunder (polling)
- **RefreshAll:** Alla konfigurationsvärden uppdateras atomiskt
- **Middleware:** Automatisk refresh på varje HTTP-request
- **Admin Dashboard toggle** - UI för att växla feature flags
- **Propagation polling** - Real-time feedback när ändringar träder i kraft

**Outbox Pattern (ADR-013):**
- **Transactional consistency** - Atomiska writes till Cosmos DB
- **Dual write problem** - Lösning för Cosmos DB + Service Bus
- **Outbox container** - Lagrar events med status (Pending/Processed/Failed)
- **Background service** - Pollar outbox och publicerar till Service Bus
- **Retry logic** - Exponential backoff för misslyckade events
- **Audit trail** - Alla events lagras oavsett publiceringsstatus
- **Feature flag integration** - Outbox alltid skrivs, publishing kontrolleras via flag

**Dual-System Coexistence:**
- **Permanent architecture** - Båda systemen lever parallellt
- **Feature flag control** - `BookingEvents_Enabled` styr vilket system som används
- **Outbox alltid aktiv** - Events skrivs för audit även när flag är av
- **Service Bus publishing** - Endast när feature flag är aktiv
- **Zero breaking changes** - Synkront system fungerar som tidigare
  
**Slide 7** - Lärdomar från projektet
- **Docker löste Oryx-problem** - Bypassade auto-detection genom containerisering
- **Sentinel Key möjliggjorde live-demos** - Runtime-switching är kritiskt för presentationer
- **Outbox Pattern säkrar data** - Dual write-problem kräver transaktionssäkerhet
- **In-memory mocks för tester** - Snabbare, isolerade tester utan externa dependencies
- **Extension Methods förbättrar läsbarhet** - Startup-kod blir mycket tydligare
- **Centraliserad error handling** - Konsistent användarupplevelse och logging

**Hur projektet kan utvecklas:**
- **API Management (APIM)** - Gateway för publika endpoints
- **Cosmos DB Change Feed** - Push-baserad event-processing (istället för polling)
- **Shopping cart** - Multi-biljett-bokning (flyttat till future improvements)
- **Event-driven ticket expiration** - Automatisk expirering via Service Bus
- **A/B testing** - Feature flags för gradual rollouts
- **Multi-environment** - Staging och production-miljöer
  
**Slide 8** - Avslutning
**Sammanfattning:**
- Dual-system coexistence med runtime-switching
- Full .NET-stack med Azure-tjänster
- Eventdriven arkitektur med Outbox Pattern
- Hot-reload via Sentinel Key Pattern
- CI/CD med Docker och GitHub Actions

**Repo och dokumentation:**
- GitHub: https://github.com/mymh13/clo24-nikhal78-examwork
- ADR-dokumentation: `/docs/adr/`
- Journal: `/docs/journal/`
- QR-kod till repot: [QR-kod här]

**Tack för mig!**

---
