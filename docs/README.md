# Project Overview for Documentation

This directory contains all documentation related to the project – from early plans to decided architectural choices and weekly logs.

---

## Documentation Structure

```bash
docs/
├─ README.md                                    # This file - documentation overview
├─ glossary.md                                  # Glossary of central concepts and acronyms
├─ statistics.md                                # Project statistics and metrics
│
├─ adr/                                         # Architecture Decision Records
│  ├─ README.md                                 # ADR index and overview
│  ├─ ADR-000-template.md                       # Template for new Architecture Decision Records
│  ├─ ADR-001-cosmosdb.md                       # Decision: Database choice (Azure Cosmos DB, Serverless)
│  ├─ ADR-002-authentication.md                 # Decision: Authentication via ASP.NET Identity + Entra ID
│  ├─ ADR-003-iac.md                            # Decision: Infrastructure as Code with Bicep
│  ├─ ADR-004-frontend.md                       # Decision: Frontend in .NET 8 Blazor Server
│  ├─ ADR-005-azureservices.md                  # Decision: Core Azure services for operations and monitoring
│  ├─ ADR-006-eventdriven.md                    # Planned decision: Event-driven architecture (Service Bus + Function)
│  ├─ ADR-007-ssl-certificate.md                # Decision: SSL certificate strategy (manual Let's Encrypt via Docker)
│  ├─ ADR-008-docker-deployment.md              # Decision: Deployment strategy (Docker containers via GHCR)
│  ├─ ADR-009-extension-methods-pattern.md      # Decision: Code organization using extension methods pattern
│  ├─ ADR-010-gdpr-session-management.md        # Decision: GDPR-compliant session management
│  ├─ ADR-011-price-calculation-system.md       # Decision: Price calculation system with discounts
│  ├─ ADR-012-azure-app-configuration.md        # Decision: Azure App Configuration for feature flags
│  ├─ ADR-013-outbox-pattern.md                 # Decision: Outbox Pattern for data integrity in dual writes
│  ├─ ADR-014-sentinel-key-pattern.md           # Decision: Sentinel Key Pattern for hot-reload configuration
│  ├─ ADR-015-application-insights-telemetry-strategy.md # Decision: Application Insights custom events for dual-system architecture
│  ├─ ADR-016-managed-identity-rbac-strategy.md # Decision: Managed Identity & RBAC for secure Azure service access
│  ├─ ADR-017-service-component-organization-pattern.md # Decision: Code organization and separation of concerns
│  ├─ ADR-018-error-handling-logging-strategy.md # Decision: Error handling and structured logging strategy
│  ├─ ADR-019-api-design-pattern-controller-based-rest.md # Decision: Controller-based REST API design pattern
│  ├─ ADR-020-qr-code-implementation.md         # Decision: QR code generation and storage strategy
│  └─ archive/                                  # Archive for older or replaced ADR documents (currently empty)
│
├─ initial_outtakes/                            # Early drafts and plans that defined the project's direction
│  ├─ architecture.md                           # Architectural overview with ASCII diagrams and flow descriptions
│  ├─ cosmos-ticketing-considerations.md        # Data and domain considerations for booking and ticket model
│  ├─ system_overview.md                        # System overview with project goals, technical plan, and service choices
│  └─ Examensarbete-utkast.pdf                  # Initial thesis draft (PDF)
│
├─ journal/                                     # Weekly logs and project documentation
│  ├─ week_one.md                               # Week 1 – startup, planning, and documentation structure
│  ├─ week_two.md                               # Week 2 – code structure and first runnable version
│  ├─ week_three.md                             # Week 3 – infrastructure and CI/CD foundation
│  ├─ week_four.md                              # Week 4 – infrastructure expansion and application integration
│  ├─ week_five.md                              # Week 5 – feature development and ticket management
│  ├─ week_six.md                               # Week 6 – event-driven architecture refinement & demo enhancements
│  ├─ week_seven.md                             # Week 7 – documentation, presentation and finalization
│  ├─ eventdriven_roadmap.md                    # Detailed roadmap for event-driven architecture refactoring (phases 1-10)
│  ├─ phase7_testing_guide.md                  # Testing guide for Phase 7.1 (synchronous flow validation)
│  ├─ phase7_2_testing_guide.md                 # Testing guide for Phase 7.2 (event-driven flow validation)
│  ├─ phase7_3_testing_guide.md                 # Testing guide for Phase 7.3 (switching between modes)
│  ├─ phase7_4_testing_guide.md                 # Testing guide for Phase 7.4 (additional validation)
│  ├─ phase7_validation.md                     # Test results and validation documentation for Phase 7
│  ├─ phase8_dashboard_guide.md                # Application Insights Workbook setup guide
│  ├─ demo_page_setup.md                       # Demo page setup and configuration
│  ├─ appinsights_troubleshooting.md            # Application Insights troubleshooting guide
│  ├─ get_appinsights_connection_string.md     # Guide for retrieving Application Insights connection string
│  ├─ test_activation_endpoint.md              # Testing guide for ticket activation endpoint
│  └─ app_insights_refresh_query.kql           # KQL query for App Configuration refresh activity
│
└─ school_related/                              # School-related documents (thesis, presentations)
   ├─ teknisk_dokumentation_clo24nikhal.docx    # Technical documentation (Word)
   ├─ teknisk_dokumentation_clo24nikhal.pdf     # Technical documentation (PDF)
   ├─ teknisk_template.md                      # Technical documentation template/structure
   ├─ verktygspresentation_clo24nikhal.docx     # Tool presentation (Word)
   ├─ verktygspresentation_clo24nikhal.pdf     # Tool presentation (PDF)
   ├─ template_presentation.md                 # Presentation template/structure
   ├─ presentation_clo24nikhal.pptx             # PowerPoint presentation
   ├─ copilot_presentation_guide.md             # Copilot in PowerPoint design brief (compressed)
   ├─ copilot_powerpoint_instructions.md       # Copilot in PowerPoint instructions
   └─ powerpoint_design_brief.md                # PowerPoint design brief
```

---

## Directory Descriptions

### Root Directory

| File                     | Description  |
|--------------------------|--------------|
| **README.md**            | This file - documentation overview and structure |
| **glossary.md**          | Glossary of central concepts and acronyms used in the project |
| **statistics.md**        | Project statistics and metrics |

---

### ./adr/ - Architecture Decision Records

Documents design and architectural decisions following the ADR format.

| File                     | Description  |
|--------------------------|--------------|
| **README.md**            | ADR index and overview with status tracking |
| **ADR-000-template.md**  | Template for new Architecture Decision Records |
| **ADR-001-cosmosdb.md**  | Decision: Database choice (Azure Cosmos DB, Serverless) |
| **ADR-002-authentication.md** | Decision: Authentication via ASP.NET Identity + Entra ID |
| **ADR-003-iac.md**       | Decision: Infrastructure as Code with Bicep |
| **ADR-004-frontend.md**  | Decision: Frontend in .NET 8 Blazor Server |
| **ADR-005-azureservices.md** | Decision: Core Azure services for operations and monitoring |
| **ADR-006-eventdriven.md** | Planned decision: Event-driven architecture (Service Bus + Function) |
| **ADR-007-ssl-certificate.md** | Decision: SSL certificate strategy (manual Let's Encrypt via Docker) |
| **ADR-008-docker-deployment.md** | Decision: Deployment strategy (Docker containers via GHCR) |
| **ADR-009-extension-methods-pattern.md** | Decision: Code organization using extension methods pattern for startup configuration |
| **ADR-010-gdpr-session-management.md** | Decision: GDPR-compliant session management using server-side storage |
| **ADR-011-price-calculation-system.md** | Decision: Price calculation system with age-based and student discounts, zone pricing |
| **ADR-012-azure-app-configuration.md** | Decision: Azure App Configuration for feature flags to support permanent dual-system coexistence |
| **ADR-013-outbox-pattern.md** | Decision: Outbox Pattern for securing data integrity in dual write operations (Cosmos DB + Service Bus) |
| **ADR-014-sentinel-key-pattern.md** | Decision: Sentinel Key Pattern for hot-reloading configuration without service restart |
| **ADR-015-application-insights-telemetry-strategy.md** | Decision: Application Insights custom events for dual-system architecture visualization |
| **ADR-016-managed-identity-rbac-strategy.md** | Decision: Managed Identity & RBAC for secure Azure service access without connection strings |
| **ADR-017-service-component-organization-pattern.md** | Decision: Code organization pattern with clear separation of concerns (Services, Controllers, Helpers, Components, Pages, Extensions) |
| **ADR-018-error-handling-logging-strategy.md** | Decision: Layered error handling and structured logging strategy with Application Insights integration |
| **ADR-019-api-design-pattern-controller-based-rest.md** | Decision: Controller-based REST API design pattern with RESTful conventions |
| **ADR-020-qr-code-implementation.md** | Decision: QR code generation and storage strategy using QRCoder library with Cosmos DB storage |
| **archive/**             | Archive for older or replaced ADR documents (currently empty) |

---

### ./initial_outtakes/ - Early Drafts and Plans

Early drafts and plans that defined the project's direction.

| File                     | Description  |
|--------------------------|--------------|
| **architecture.md**      | Architectural overview with ASCII diagrams and flow descriptions |
| **cosmos-ticketing-considerations.md** | Data and domain considerations for booking and ticket model |
| **system_overview.md**   | System overview with project goals, technical plan, and service choices |
| **Examensarbete-utkast.pdf** | Initial thesis draft (PDF) |

---

### ./journal/ - Weekly Logs

Weekly logs for project progress and timeline tracking.

| File                     | Description  |
|--------------------------|--------------|
| **week_one.md**          | Week 1 – startup, planning, and documentation structure |
| **week_two.md**          | Week 2 – code structure and first runnable version |
| **week_three.md**        | Week 3 – infrastructure and CI/CD foundation |
| **week_four.md**         | Week 4 – infrastructure expansion and application integration |
| **week_five.md**         | Week 5 – feature development and ticket management |
| **week_six.md**          | Week 6 – event-driven architecture refinement & demo enhancements |
| **week_seven.md**        | Week 7 – documentation, presentation and finalization |
| **eventdriven_roadmap.md** | Detailed roadmap for event-driven architecture refactoring (phases 1-10) |
| **phase7_testing_guide.md** | Testing guide for Phase 7.1 (synchronous flow validation) |
| **phase7_2_testing_guide.md** | Testing guide for Phase 7.2 (event-driven flow validation) |
| **phase7_3_testing_guide.md** | Testing guide for Phase 7.3 (switching between modes) |
| **phase7_4_testing_guide.md** | Testing guide for Phase 7.4 (additional validation) |
| **phase7_validation.md** | Test results and validation documentation for Phase 7 |
| **phase8_dashboard_guide.md** | Application Insights Workbook setup guide |
| **demo_page_setup.md**   | Demo page setup and configuration |
| **appinsights_troubleshooting.md** | Application Insights troubleshooting guide |
| **get_appinsights_connection_string.md** | Guide for retrieving Application Insights connection string |
| **test_activation_endpoint.md** | Testing guide for ticket activation endpoint |
| **app_insights_refresh_query.kql** | KQL query for Application Insights to check App Configuration refresh activity |

---

### ./school_related/ - School-Related Documents

Documents related to the academic thesis and presentations.

| File                     | Description  |
|--------------------------|--------------|
| **teknisk_dokumentation_clo24nikhal.docx** | Technical documentation (Word document) |
| **teknisk_dokumentation_clo24nikhal.pdf** | Technical documentation (PDF) |
| **teknisk_template.md** | Technical documentation template/structure with headings and bullet points |
| **verktygspresentation_clo24nikhal.docx** | Tool presentation (Word document) |
| **verktygspresentation_clo24nikhal.pdf** | Tool presentation (PDF) |
| **template_presentation.md** | Presentation template/structure for PowerPoint slides |
| **presentation_clo24nikhal.pptx** | PowerPoint presentation file |
| **copilot_presentation_guide.md** | Compressed design brief for Copilot in PowerPoint (under 2000 characters) |
| **copilot_powerpoint_instructions.md** | Detailed instructions for Copilot in PowerPoint |
| **powerpoint_design_brief.md** | PowerPoint design brief with design and content guidelines |

---

## Project Structure

The project has a clear separation between production code, tests, and infrastructure:

```bash
clo24-nikhal78-examwork/
├─ src/                             # Production code
│  ├─ web/                          # Blazor Server (UI + API)
│  │  └─ Ticketing.Web/            # .NET 8 Blazor Server project
│  │     ├─ Controllers/           # API Controllers (Auth, Bookings, Users, Health, FeatureFlag)
│  │     ├─ Pages/                 # Razor pages (Login, Admin, User, Inspector, Demo, Health)
│  │     ├─ Components/            # Reusable Razor components
│  │     ├─ Services/              # Business logic services
│  │     ├─ Helpers/               # Utility helpers
│  │     ├─ Extensions/            # Startup configuration extensions
│  │     └─ Authentication/        # Cookie auth and session management
│  ├─ functions/                    # Azure Functions (event-driven)
│  │  └─ Ticketing.Functions/      # .NET 8 isolated worker Functions
│  │     └─ Functions/             # Function implementations
│  ├─ shared/                       # Shared libraries
│  │  ├─ Ticketing.Contracts/      # DTOs, Events, Contracts
│  │  └─ Ticketing.Domain/         # Domain entities (future/placeholder)
│  └─ tests/                        # Test projects
│     └─ Ticketing.Web.Tests/      # xUnit test project for Ticketing.Web
│        ├─ Helpers/                # Unit tests
│        └─ Integration/            # Integration tests with mocks
├─ infra/                          # Infrastructure as Code (Bicep)
├─ docs/                           # Documentation (this directory)
└─ letsencrypt/                    # SSL certificate management
```

**Test Project Structure:**
- **Location:** `src/tests/Ticketing.Web.Tests/`
- **Framework:** xUnit
- **Mocking:** NSubstitute
- **Organization:** Test classes mirror source directory structure (e.g., `Helpers/PriceCalculationHelperTests.cs` for `Helpers/PriceCalculationHelper.cs`)
- **Coverage:** Unit tests for price calculations

---

> **Note:**  
> The documentation is updated iteratively during the project. New ADRs and journal entries are added progressively as decisions are made or milestones are reached.
>
> **Excluded Files:**
> - `notes.md` - Personal notes (gitignored)
> - Temporary files (e.g., `~$*.docx` Word temp files)
