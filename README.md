### Readme for my Exam project for the CLO24 "Cloud Developer" Program 2025
  
Author: Niklas Häll - https://github.com/mymh13
  
### English / Swedish
  
The Exam assignment clearly state that the documentation has to be in Swedish. I would argue that English is more suitable in a professional setting, but since this is an education situated in Sweden, it makes somewhat sense. Either case, we follow the rules. Because of this, from this point on all the documentation will be in Swedish. (Obviously, class names, method names et c. will be in English.)
  
### Grundläggande struktur för repot, OBS! Detta är ett utkast, kan justeras!

```bash
clo24-nikhal78-examwork/
├─ docs/                      # All dokumentation
│  ├─ README.md               # Projektöversikt för dokumentationen (länkar vidare?)
│  ├─ adr/                    # Architecture Decision Records (ADR-00x.md)
│  ├─ architecture/           # Arkitektur & diagram
│  ├─ glossary.md             # Begrepp (zon, trip, booking, etc.)
│  ├─ initial_outtakes/       # Utkast och ursprungsidéer
│  ├─ journal/                # Timeline/logg per vecka (markdown)
│  ├─ operations/             # Driftmanual, runbooks, incident checklist
│  └─ telemetry/              # App Insights & KQL-exempel, dashboard/workbook-notes
│
├─ src/                       # Produktionskod
│  ├─ web/                    # Blazor Server (UI)
│  │  └─ Ticketing.Web/       # .NET 8 projekt
│  ├─ api/                    # Controller-baserat Web API
│  │  └─ Ticketing.Api/
│  ├─ functions/              # Azure Functions (eventpåslag)
│  │  └─ Ticketing.Functions/
│  └─ shared/                 # Delade bibliotek (DTO, Contracts, Domain)
│     ├─ Ticketing.Domain/    # Entities, ValueObjects, Domain Services
│     └─ Ticketing.Contracts/ # Request/Response/Events (ex. BookingCreated)
│
├─ tests/                     # Tester (xUnit + NSubstitute)
│  ├─ Ticketing.Domain.Tests/
│  ├─ Ticketing.Api.Tests/
│  └─ Ticketing.Functions.Tests/
│
├─ infra/                     # IaC (Bicep/ARM? eller Terraform (helst))
│  ├─ modules/                # Återanvändbara moduler (appservice, cosmos, apim, insights?)
│  ├─ env/                    # Miljöspecifika deployment-paket
│  │  ├─ dev/
│  │  │  ├─ main.bicep
│  │  │  └─ main.parameters.json
│  │  └─ prod/
│  │     ├─ main.bicep
│  │     └─ main.parameters.json
│  └─ policies/               # APIM policies (XML) & config (endast GET i MVP)
│
├─ pipelines/                 # Pipeline-definitioner (om Azure DevOps)
│  ├─ build.yaml              # CI: restore/build/test + artefakter
│  ├─ deploy_infra.yaml       # CD: infra (dev→prod, manual approval)
│  └─ deploy_app.yaml         # CD: web/api/functions zip deploy (stegvis)
│
├─ .github/
│  └─ workflows/              # GitHub Actions
│     ├─ ci-build.yaml
│     ├─ cd-infra.yaml
│     └─ cd-app.yaml
│
├─ .gitignore
├─ LICENSE
└─ README.md                  # Huvud-README: pitch, snabbstart, mappar, länkar
```