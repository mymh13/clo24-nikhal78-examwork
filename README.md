### Readme for my Exam/Thesis project for the CLO24 "Cloud Developer" Program 2025
  
Author: Niklas Häll - https://github.com/mymh13
  
### Basic repository structure, NOTE: This is a draft and may change!

```bash
clo24-nikhal78-examwork/
├─ docs/                      # All documentation
│  ├─ README.md               # Documentation overview (links onward?)
│  ├─ adr/                    # Architecture Decision Records (ADR-00x.md)
│  ├─ glossary.md             # Terms (zone, trip, booking, etc.)
│  ├─ initial_outtakes/       # Drafts and original ideas
│  ├─ journal/                # Timeline/log per week (markdown)
│  ├─ operations/             # Operations manual, runbooks, incident checklist
│  └─ telemetry/              # App Insights & KQL examples, dashboard/workbook notes
│
├─ src/                       # Production code
│  ├─ web/                    # Blazor Server (UI)
│  │  └─ Ticketing.Web/       # .NET 8 project
│  ├─ api/                    # Controller-based Web API
│  │  └─ Ticketing.Api/
│  ├─ functions/              # Azure Functions (event-driven)
│  │  └─ Ticketing.Functions/
│  └─ shared/                 # Shared libraries (DTO, Contracts, Domain)
│     ├─ Ticketing.Domain/    # Entities, ValueObjects, Domain Services
│     └─ Ticketing.Contracts/ # Request/Response/Events (e.g., BookingCreated)
│
├─ tests/                     # Tests (xUnit + NSubstitute)
│  ├─ Ticketing.Domain.Tests/
│  ├─ Ticketing.Api.Tests/
│  └─ Ticketing.Functions.Tests/
│
├─ infra/                     # IaC (Bicep)
│  ├─ modules/                # Reusable modules (appservice, cosmos, apim, insights?)
│  ├─ env/                    # Environment-specific deployment packages
│  │  ├─ dev/
│  │  │  ├─ main.bicep
│  │  │  └─ main.parameters.json
│  │  └─ prod/
│  │     ├─ main.bicep
│  │     └─ main.parameters.json
│  └─ policies/               # APIM policies (XML) & config (GET-only in MVP)
│
├─ pipelines/                 # Pipeline definitions (if Azure DevOps)
│  ├─ build.yaml              # CI: restore/build/test + artifacts
│  ├─ deploy_infra.yaml       # CD: infra (dev→prod, manual approval)
│  └─ deploy_app.yaml         # CD: web/api/functions zip deploy (staged)
│
├─ .github/
│  └─ workflows/              # GitHub Actions
│     ├─ ci-build.yaml
│     ├─ cd-infra.yaml
│     └─ cd-app.yaml
│
├─ .gitignore
├─ LICENSE
└─ README.md                  # Main README: pitch, quickstart, folders, links
```