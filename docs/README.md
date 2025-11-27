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
│  └─ archive/                                  # Archive for older or replaced ADR documents (currently empty)
│
├─ initial_outtakes/                            # Early drafts and plans that defined the project's direction
│  ├─ architecture.md                           # Architectural overview with ASCII diagrams and flow descriptions
│  ├─ cosmos-ticketing-considerations.md        # Data and domain considerations for booking and ticket model
│  ├─ system_overview.md                        # System overview with project goals, technical plan, and service choices
│  └─ Examensarbete-utkast.pdf                  # Initial thesis draft (PDF)
│
├─ journal/                                     # Weekly logs for project progress
│  ├─ week_one.md                               # Week 1 – startup, planning, and documentation structure
│  ├─ week_two.md                               # Week 2 – code structure and first runnable version
│  ├─ week_three.md                             # Week 3 – infrastructure and CI/CD foundation
│  ├─ week_four.md                              # Week 4 – infrastructure expansion and application integration
│  ├─ week_five.md                              # Week 5 – feature development and ticket management
│  └─ eventdriven_roadmap.md                    # Detailed roadmap for event-driven architecture refactoring
│
└─ school_related/                              # School-related documents (thesis, presentations)
   ├─ teknisk_dokumentation_clo24nikhal.docx    # Technical documentation (Word)
   └─ verktygspresentation_clo24nikhal.docx     # Tool presentation (Word)
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
| **eventdriven_roadmap.md** | Detailed roadmap for event-driven architecture refactoring (phases 1-10) |

---

### ./school_related/ - School-Related Documents

Documents related to the academic thesis and presentations.

| File                     | Description  |
|--------------------------|--------------|
| **teknisk_dokumentation_clo24nikhal.docx** | Technical documentation (Word document) |
| **verktygspresentation_clo24nikhal.docx** | Tool presentation (Word document) |

---

> **Note:**  
> The documentation is updated iteratively during the project. New ADRs and journal entries are added progressively as decisions are made or milestones are reached.
>
> **Excluded Files:**
> - `notes.md` - Personal notes (gitignored)
> - Temporary files (e.g., `~$*.docx` Word temp files)
