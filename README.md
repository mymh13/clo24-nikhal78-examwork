### Readme for my Exam/Thesis project for the CLO24 "Cloud Developer" Program 2025
  
Author: Niklas Häll - https://github.com/mymh13
  
### Repository Structure

```bash
clo24-nikhal78-examwork/
├─ docs/                           # All documentation
│  ├─ README.md                    # Documentation overview
│  ├─ adr/                         # Architecture Decision Records
│  │  ├─ archive/                  # Superseded ADRs
│  │  └─ README.md
│  ├─ glossary.md                  # Terms (zone, trip, booking, etc.)
│  ├─ initial_outtakes/            # Drafts and original ideas
│  │  ├─ architecture.md
│  │  ├─ cosmos-ticketing-considerations.md
│  │  ├─ system_overview.md
│  │  └─ Examensarbete-utkast.pdf
│  ├─ school_related/              # Education-specific documentation
│  │  ├─ teknisk_dokumentation_clo24nikhal.docx
│  │  ├─ verktygspresentation_clo24nikhal.docx
│  └─ journal/                      # Timeline/log per week
│
├─ src/                             # Production code
│  ├─ web/                          # Blazor Server (UI)
│  │  └─ Ticketing.Web/            # .NET 8 Blazor Server project
│  │     ├─ Controllers/           # API Controllers (Auth, Bookings, Users, Health)
│  │     ├─ Pages/                # Razor pages (Login, Admin, User, Inspector, etc.)
│  │     ├─ Services/              # Business logic services
│  │     ├─ Helpers/               # Utility helpers
│  │     ├─ Extensions/            # Startup configuration extensions
│  │     ├─ Authentication/        # Cookie auth and session management
│  │     └─ wwwroot/               # Static files (CSS, favicon)
│  ├─ api/                         # Controller-based Web API
│  │  └─ Ticketing.Api/            # .NET 8 Web API project
│  ├─ functions/                   # Azure Functions (event-driven)
│  │  └─ Ticketing.Functions/      # .NET 8 isolated worker Functions
│  │     ├─ Functions/            # Function implementations
│  │     │  └─ OnBookingCreatedFunction.cs
│  │     ├─ Program.cs            # Function app startup
│  │     ├─ host.json             # Function app configuration (retry, logging)
│  │     └─ local.settings.json  # Local development settings
│  └─ shared/                      # Shared libraries
│     ├─ Ticketing.Contracts/      # DTOs, Events, Contracts
│     │  ├─ Bookings/              # Booking, BookingDto
│     │  ├─ Users/                 # User
│     │  ├─ Events/                # Event base class, BookingCreated, BookingCancelled
│     │  └─ Outbox/                # OutboxEvent, OutboxEventStatus
│     └─ Ticketing.Domain/         # Domain entities (future)
│
├─ infra/                          # Infrastructure as Code (Bicep)
│  ├─ modules/                     # Reusable Bicep modules
│  │  ├─ appconfiguration.bicep
│  │  ├─ applicationinsights.bicep
│  │  ├─ appservice.bicep
│  │  ├─ cosmosdb.bicep
│  │  ├─ functionapp.bicep
│  │  ├─ keyvault.bicep
│  │  └─ servicebus.bicep
│  ├─ env/                         # Environment-specific deployments
│  │  └─ dev/
│  │     ├─ main.bicep
│  │     └─ main.parameters.json
│  └─ README.md
│
├─ .github/                        # GitHub Actions workflows
│  └─ workflows/
│     ├─ ci-build.yaml            # CI: Build and push Docker images
│     ├─ cd-web-dev.yaml          # CD: Deploy Web app to dev
│     └─ cd-functions-dev.yaml    # CD: Deploy Functions to dev
│
├─ letsencrypt/                    # Let's Encrypt SSL certificate management
│  ├─ accounts/                    # ACME account data
│  ├─ archive/                     # Archived certificates
│  ├─ live/                        # Active certificates
│  └─ renewal/                     # Certificate renewal configuration
│
├─ clo24_nikhal78_examwork.sln     # Solution file
├─ .gitignore
├─ LICENSE
└─ README.md                       # This file
```