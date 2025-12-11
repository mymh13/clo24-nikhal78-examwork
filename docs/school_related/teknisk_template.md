# Teknisk Dokumentation - Struktur och Innehåll

## 1. Introduktion

### 1.1 Projektpresentation
- Dual-System Coexistence: Runtime-Switching mellan Synkront och Eventdrivet System
- Biljettbokningssystem för kollektivtrafik
- Toggle-switch för att växla mellan systemarkitekturer
- .NET 8 + Azure-molntjänster

### 1.2 Problemet det skulle lösa
- Kunden arbetar i två system parallellt (äldre + nyare)
- Behov av ett gränssnitt för att hantera båda systemen
- Successiv migration från gammalt till nytt system
- Runtime-växling utan service-restart

### 1.3 Hur vi löste det / Systemöversikt
- Permanent dual-system coexistence
- Feature flags styr vilket system som används
- Synkront system: Direkt API → Cosmos DB
- Eventdrivet system: API → Outbox → Service Bus → Azure Functions → Cosmos DB
- Toggle-knappen växlar mellan systemen via Azure App Configuration

### 1.4 Fil och mappstruktur
- docs/ - Dokumentation (ADR, journal, glossary)
- src/web/ - Blazor Server (UI + API)
- src/functions/ - Azure Functions
- src/shared/ - Delade kontrakt
- src/tests/ - Testprojekt
- infra/ - Bicep-templates
- .github/workflows/ - CI/CD pipelines

### 1.5 Applikationens komponenter
- Controllers: BookingsController, UsersController, AuthController, HealthController, FeatureFlagController
- Services: BookingService, UserService, OutboxService, FeatureFlagService, TelemetryService, EventPublisher
- Helpers: ErrorHandlerHelper, HttpErrorHelper, PriceCalculationHelper, NavigationHelper, TicketActivationHelper, QrCodeHelper
- Pages: Login, AdminLandingPage, UserLandingPage, Bookings, Users, Health, Demo
- Components: BookingTable, BookingManagement
- Extensions: ServiceCollectionExtensions, WebApplicationExtensions, ConfigurationExtensions

### 1.6 Flöden - Flödesschema
- Synkront flöde: Användare → Blazor Server → API → Cosmos DB
- Eventdrivet flöde: Användare → Blazor Server → API → Outbox → Service Bus → Azure Functions → Cosmos DB
- Toggle-switch: Feature flag i App Configuration styr vilket flöde som används
- Outbox Pattern: Säkerställer transaktionssäkerhet vid dual writes

---

## 2. Arbetsmetodik

### 2.1 Iterativt arbete i små steg
- Arbete i faser med delmoment
- Varje fas bröts ner i små, hanterbara delar
- Simulerar agilt arbete utan formella sprints
- Kontinuerlig refaktorering och förbättring
- Testning integrerad i varje fas

### 2.2 ADR (Architecture Decision Records)
- Dokumentation av tekniska beslut
- Motivering till varje val
- Konsekvenser och alternativ
- 20 ADR-dokument skapade
- Viktiga ADRer: ADR-001 (Cosmos DB), ADR-004 (Blazor Server), ADR-005 (Azure Services), ADR-013 (Outbox Pattern), ADR-014 (Sentinel Key)

### 2.3 Journal
- Veckovis loggning av framsteg
- Dokumentation av aktiviteter, utmaningar och lärdomar
- Timeline för projektet
- Veckor 1-6 dokumenterade
- Event-driven roadmap för detaljerad implementation

### 2.4 Övrig dokumentation
- Glossary - Förklarar begrepp och termer
- Systemöversikt - Övergripande bild av systemet
- Architecture.md - Arkitekturdiagram och flöden
- Bug backlog - Kända buggar och lösningar
- Future improvements - Framtida förbättringar

---

## 3. .NET

### 3.1 .NET 8
- .NET 8 LTS
- C# som programmeringsspråk
- ASP.NET Core för backend
- Samma version som kunden använder

### 3.2 Blazor Server
- UI-framework för interaktiva webbgränssnitt
- Server-driven med SignalR för real-time updates
- Razor Components för komponentbaserad UI
- Ingen JavaScript-beroende
- Snabb och responsiv

### 3.3 ASP.NET Core Identity
- Användarhantering och autentisering
- Cookie Authentication för vanliga användare
- BCrypt för lösenordshashning
- Rollbaserad åtkomst (Admin, Inspector, User)

### 3.4 Controller-based REST API
- RESTful API-design
- Controllers: BookingsController, UsersController, AuthController, HealthController, FeatureFlagController
- Standardiserade HTTP-metoder (GET, POST, PUT, DELETE)
- Konsistent felhantering via ErrorHandlerHelper

### 3.5 Dependency Injection
- Inbyggd IoC-container
- Service-registrering via extension methods
- Scoped, Singleton, Transient lifetimes
- Interface-baserad design

### 3.6 Extension Methods Pattern
Flyttad till 4.5

### 3.7 Repository/Service Pattern
Flyttad till avsnitt 4

### 3.8 Error Handling Helpers
- ErrorHandlerHelper - Centraliserad felhantering för API controllers
- HttpErrorHelper - Felhantering för Razor pages
- Konsistenta felmeddelanden
- Användarvänliga meddelanden
- Strukturerad logging

### 3.9 Navigation Helper
- Rollbaserad navigering
- GetLandingPageUrl() - Bestämmer korrekt landningssida baserat på roll
- Används i Login.razor och andra sidor

### 3.10 Price Calculation Helper
- Beräkning av biljettpriser
- Zonbaserad prissättning
- Åldersrabatter (barn, vuxen, pensionär)
- Studentrabatt
- Konfigurerbar baspris per zon

### 3.11 Ticket Activation Helper
- Validering av biljettaktivering
- Beräkning av giltighetsperiod (90 minuter)
- Statuskontroll (Created, Activated, Valid, Expired)
- KanActivate() och ValidateActivation() metoder

### 3.12 QR Code Helper
- QR-kodgenerering med QRCoder-bibliotek
- Base64-kodad PNG-lagring i Cosmos DB
- JSON-kodad data (Booking ID, Customer ID, timestamps, status)
- GetQrCodeDataUrl() för visning i UI

### 3.13 Cosmos JSON Serializer
- Anpassad serializer för Cosmos DB
- Hanterar Cosmos DB-specifika serialiseringsproblem
- System.Text.Json som bas

### 3.14 Bibliotek och NuGet-paket
- QRCoder - QR-kodgenerering
- System.Text.Json - JSON-serialisering
- Microsoft.Extensions.* - Konfiguration, dependency injection, logging
- Azure.* - Azure SDK-paket för Cosmos DB, Service Bus, App Configuration, Key Vault

---

## 4. Patterns

### 4.1 Outbox Pattern
- Lösning för dual write-problem (Cosmos DB + Service Bus)
- Transaktionssäkerhet - booking och event skapas atomiskt
- Outbox container i Cosmos DB med status (Pending/Processed/Failed)
- Background service (OutboxProcessorService) pollar och publicerar events
- Retry logic med exponential backoff
- Audit trail - alla events lagras oavsett publiceringsstatus
- Synkront: Event skapas men publiceras INTE till Service Bus
- Eventdrivet: Event publiceras till Service Bus → Azure Functions bearbetar

### 4.2 Sentinel Key Pattern
- Hot-reload av konfiguration utan service-restart
- Trigger-nyckel: Settings:Sentinel i Azure App Configuration
- När sentinel ändras → alla konfigurationsvärden uppdateras automatiskt
- Refresh interval: 30 sekunder
- Middleware kallar TryRefreshAsync() på varje HTTP-request
- Zero-downtime switching
- Möjliggör live-demos med runtime-växling

---
Repository och Service Patterns generellt:
- Separation of concerns
- IBookingService, IUserService, IOutboxService interfaces
- Abstraktion för dataåtkomst
- Enklare testning och underhåll

### 4.3 Repository Pattern
- Abstraktion för dataåtkomst
- IBookingService, IUserService, IOutboxService
- Enklare testning med mock-implementationer
- Separation of concerns

### 4.4 Service Layer Pattern
- Affärslogik separerad från controllers
- Services hanterar komplex logik
- Controllers fokuserar på HTTP-hantering
- Tydlig ansvarsfördelning

### 4.5 Extension Methods Pattern
- Startup-konfiguration organiserad i extension methods
- ServiceCollectionExtensions, WebApplicationExtensions, ConfigurationExtensions
- Tydligare och mer läsbar Program.cs
- Återanvändbar konfiguration

### 4.6 Dependency Injection Pattern
- Interface-baserad design
- Constructor injection
- Scoped, Singleton, Transient lifetimes
- Enklare testning och underhåll

### 4.7 Controller-based REST Pattern
- RESTful API-design
- Standardiserade HTTP-metoder
- Konsistent URL-struktur
- Proper HTTP status codes

### 4.8 Error Handling Pattern
- Centraliserad felhantering
- ErrorHandlerHelper för controllers
- HttpErrorHelper för Razor pages
- Konsistenta felmeddelanden
- Strukturerad logging

### 4.9 Feature Flag Pattern
- Runtime-konfiguration via Azure App Configuration
- BookingEvents_Enabled styr vilket system som används
- Hot-reload via Sentinel Key Pattern
- Toggle-knapp i Admin Dashboard

### 4.10 Dual-System Coexistence Pattern
- Båda systemen körs parallellt
- Feature flag styr vilket system som används
- Outbox alltid aktiv för audit
- Zero breaking changes - synkront system fungerar som tidigare
- Permanent architecture - inte en temporär migration

---

## 5. Azure

### 5.1 Azure App Service
- Hosting för Blazor Server och API
- Basic B1 plan (dev)
- Docker-containers från GHCR
- Linux, .NET 8
- Managed Identity för säker åtkomst

### 5.2 Azure Cosmos DB
- Serverless NoSQL-databas
- Containers: bookings, users, outbox
- Partition keys för effektiv querying
- Transactional consistency
- Serverless-läge - betala per användning

### 5.3 Azure Service Bus
- Meddelandekö för event-driven architecture
- booking-events queue
- Dead letter queue support
- Basic tier (dev)
- Managed Identity access

### 5.4 Azure Functions
- Serverless event processing
- Processar Service Bus-meddelanden
- Basic B1 plan (dev)
- .NET 8 isolated worker
- Managed Identity för Cosmos DB och Service Bus

### 5.5 Azure Application Insights
- Logging, monitoring, performance measurement
- KQL-queries för analys
- Custom events för dual-system architecture
- Telemetry abstraction layer (ITelemetryService interface)
- Custom events: BookingCreated, OutboxEventCreated, OutboxEventProcessed, ServiceBusEventPublished, FeatureFlagToggled, ModeSwitch
- Custom dimensions: SystemType, ToMode, ArchitectureMode för att skilja synkront/eventdrivet
- Workbooks för visualisering (Current Mode Indicator, Latest Booking Flow Timeline, Events by Type)
- Pay-as-you-go pricing
- Används av både App Service och Function App

### 5.6 Azure App Configuration
- Centraliserad konfigurationshantering
- Feature flags (BookingEvents_Enabled)
- Hot-reload via Sentinel Key Pattern
- Free tier (dev)
- Managed Identity access

### 5.7 Azure Key Vault
- Säker lagring av secrets och connection strings
- Standard tier
- RBAC-baserad åtkomst via Managed Identity
- Inga secrets i kod eller konfigurationsfiler

### 5.8 Azure Storage Account
- Krävs för Azure Functions runtime
- State management för Functions
- Standard LRS
- Används indirekt av Function App



### 5.10 Managed Identity och RBAC
- Alla tjänster använder Managed Identity
- RBAC-baserad autentisering
- App Service: "App Configuration Data Reader", "Azure Service Bus Data Owner"
- Function App: "Azure Service Bus Data Receiver", "DocumentDB Account Contributor"
- Inga connection strings i kod

### 5.11 Azure Entra ID (tidigare Azure AD)
- Autentisering för Admin/Inspector-roller
- Microsoft-konto integration
- OAuth 2.0 / OpenID Connect

---

## 6. CI/CD

### 6.1 GitHub Actions
- CI/CD-plattform
- YAML-baserade pipelines
- Separated CI/CD (build och deploy i separata workflows)
- OIDC-autentisering för Azure
- Environment protection (dev/prod)

### 6.2 Docker
- Containerisering av applikationen
- Multi-stage builds för optimering
- Löste Oryx auto-detection problem
- Dockerfile för både web och functions

### 6.3 GitHub Container Registry (GHCR)
- Container-lagring
- Public images för dev
- Integrerat med GitHub Actions
- Pull images i Azure App Service

### 6.4 Bicep (Infrastructure as Code)
- Azure-infrastruktur som kod
- Modulär struktur - återanvändbara moduler
- Resource Groups - separata för dev/prod
- Managed Identity konfiguration
- RBAC-roller definierade i Bicep

### 6.5 Authentication och Secrets
- Hybrid-autentisering: OIDC permissions (id-token: write) för säker åtkomst
- GitHub Secrets för Azure-autentisering: AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_SUBSCRIPTION_ID
- Application Insights connection string lagras som secret
- GITHUB_TOKEN för GHCR-autentisering (automatiskt tillgänglig)
- Environment-scoped secrets för säker separation mellan dev/prod

### 6.6 Multi-stage Docker Builds
- Optimering av container-storlek
- Build stage och runtime stage
- Separerade dependencies
- Snabbare deployments

### 6.7 Deployment Strategies
- Web App: Docker-containers via GHCR med versionering (SHA-baserade tags)
- Function App: Zip-deploy (rekommenderat för .NET isolated mode)
- Trunk-based development: Push till main efter lokal validering
- Idempotent deployments - pipelines kan köras flera gånger säkert
- Rollback-möjligheter via versionerade container-images

### 6.8 Environment Management
- GitHub Environments för deployment approval och secret management
- Environment protection rules för säker deployment
- Dev environment: `rg-examwork-dev` Resource Group
- Struktur förberedd för prod (infra/env/prod planerad men ej implementerad)
- Environment-scoped secrets och variables för isolering

---

## 7. Säkerhet

### 7.1 Testning
- xUnit - Testramverk
- NSubstitute - Mocking
- Unit tests - Affärslogik (price calculations)
- Integration tests - Fullstack med WebApplicationFactory
- In-memory mocks - InMemoryUserService, InMemoryBookingService, InMemoryOutboxService
- InMemoryStorage - Singleton för delad data i tester


### 7.2 Autentisering
- Azure Entra ID för Admin/Inspector
- Cookie Authentication för vanliga användare
- ASP.NET Core Identity-principer
- BCrypt för lösenordshashning
- Rollbaserad åtkomst (RBAC)

### 7.3 Säkerhet i Azure
- Managed Identity överallt
- RBAC-baserad åtkomst
- Inga connection strings i kod
- Key Vault för secrets
- Network security (kan utökas med private endpoints)

### 7.4 GDPR och Session Management
- Server-side session storage
- Cookie-baserad autentisering
- Säker hantering av användardata
- ADR-010 dokumenterar GDPR-kompatibel session management

### 7.5 Error Handling och Logging
- Centraliserad felhantering
- Inga känsliga detaljer exponeras
- Strukturerad logging med Application Insights
- User-friendly error messages

---

## 8. Lärdomar och Utveckling

### 8.1 Lärdomar av projektet
- Docker löste Oryx-problem - Bypassade auto-detection
- Sentinel Key möjliggjorde live-demos - Runtime-switching kritiskt
- Outbox Pattern säkrar data - Dual write-problem kräver transaktionssäkerhet
- In-memory mocks för tester - Snabbare, isolerade tester
- Extension Methods förbättrar läsbarhet - Startup-kod tydligare
- Centraliserad error handling - Konsistent UX och logging
- Component extraction - Eliminerade kodduplicering
- Configuration as code - Runtime-anpassningar möjliga
- MVP-filosofi - Fokus på kärnfunktionalitet, undvik over-engineering
- Right tool for the job - Azure Portal för Application Insights visualisering

### 8.2 Hur produkten kan utvecklas
- API Management (APIM) - Gateway för publika endpoints
- Cosmos DB Change Feed - Push-baserad event-processing
- Event-driven ticket expiration - Automatisk expirering
- Shopping Cart - Lägg till flera biljetter innan betalning
- User Registration - Användarregistrering i UI
- QR Code Scanning - Fysisk scanning för Inspectors
- Ticket Search - Sök och filtrera i admin-bokningar
- A/B testing - Feature flags för gradual rollouts
- Multi-environment - Staging och production
- Rate Limiting - Skydd för API-endpoints
- Swagger/OpenAPI - API-dokumentation
- Domain Separation - Separera domäner för bättre arkitektur

---

## 9. Referenser

### 9.1 Viktiga ADRer
- ADR-001: Cosmos DB
- ADR-002: Authentication
- ADR-003: Infrastructure as Code (Bicep)
- ADR-004: Blazor Server
- ADR-005: Azure Services
- ADR-006: Event-Driven Architecture
- ADR-008: Docker och GitHub Actions
- ADR-012: Azure App Configuration
- ADR-013: Outbox Pattern
- ADR-014: Sentinel Key Pattern
- ADR-015: Application Insights Telemetry
- ADR-016: Managed Identity & RBAC
- ADR-017: Service Component Organization
- ADR-018: Error Handling Strategy
- ADR-019: API Design Pattern
- ADR-020: QR Code Implementation

### 9.2 Dokumentation
- GitHub Repository: https://github.com/mymh13/clo24-nikhal78-examwork
- ADR-dokumentation: /docs/adr/
- Journal: /docs/journal/
- Glossary: /docs/glossary.md
- Systemöversikt: /docs/initial_outtakes/system_overview.md

### 9.3 Externa Referenser
- Microsoft Docs - Azure Services
- Microsoft Docs - .NET 8
- Microsoft Docs - Blazor Server
- Microsoft Docs - Application Insights
- Microsoft Docs - Cosmos DB
- Microsoft Docs - Service Bus
- Microsoft Docs - Azure Functions
- Microsoft Docs - Bicep

