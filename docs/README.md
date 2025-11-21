# Project Overview for Documentation

This directory contains all documentation related to the project – from early plans to decided architectural choices and weekly logs.

---

### ./root directory

| File                     | Description  |
|--------------------------|--------------|
| **glossary.md**          | (Optional) Glossary of central concepts and acronyms used in the project. |

---

### ./adr/                      ADR - Architecture Decision Record. Documents design and architectural decisions.

| File                     | Description  |
|--------------------------|--------------|
| **./archive/**           | Archive for older or replaced ADR documents. |
| **ADR-000-template.md**  | Template for new Architecture Decision Records. |
| **ADR-001-cosmosdb.md**  | Decision: Database choice (Azure Cosmos DB, Serverless). |
| **ADR-002-authentication.md** | Decision: Authentication via ASP.NET Identity + Entra ID. |
| **ADR-003-iac.md**       | Decision: Infrastructure as Code with Bicep. |
| **ADR-004-frontend.md**  | Decision: Frontend in .NET 8 Blazor Server. |
| **ADR-005-azureservices.md** | Decision: Core Azure services for operations and monitoring. |
| **ADR-006-eventdriven.md** | Planned decision: Event-driven architecture (Service Bus + Function). |
| **ADR-007-ssl-certificate.md** | Decision: SSL certificate strategy (manual Let's Encrypt via Docker). |
| **ADR-008-docker-deployment.md** | Decision: Deployment strategy (Docker containers via GHCR). |
| **ADR-009-extension-methods-pattern.md** | Decision: Code organization using extension methods pattern for startup configuration. |
| **ADR-010-gdpr-session-management.md** | Decision: GDPR-compliant session management using server-side storage. |

---

### ./initial_outtakes/         Early drafts and plans that defined the project's direction.

| File                     | Description  |
|--------------------------|--------------|
| **system_overview.md**   | System overview with project goals, technical plan, and service choices. |
| **architecture.md**       | Architectural overview with ASCII diagrams and flow descriptions. |
| **cosmos-ticketing-considerations.md** | Data and domain considerations for booking and ticket model. |

---

### ./journal/                  Weekly logs for project progress.

| File                     | Description  |
|--------------------------|--------------|
| **week_one.md**          | Week 1 – startup, planning, and documentation structure. |
| **week_two.md**          | Week 2 – code structure and first runnable version. |
| **week_three.md**        | Week 3 – infrastructure and CI/CD foundation. |
| **week_four.md**         | Week 4 – infrastructure expansion and application integration. |

---

> **Note:**  
> The documentation is updated iteratively during the project. New ADRs and journal entries are added progressively as decisions are made or milestones are reached.
