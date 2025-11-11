# Week 1 – Startup and Planning

## Overview
The first week has focused on startup, planning, and documentation structure.  
The goal has been to create a clear foundation for further development, with a focus on organization, modularity, and traceability. 

---

## Completed Activities

- Created **repository base structure** (docs/, infra/, etc.)
- Written **README.md** and added **MIT License**
- Added **.gitignore** adapted for .NET and local environments
- Established solution for the entire project (clo24-nikhal78-examwork.sln)
- Created directories:
  - `docs/initial_outtakes/` → early plans and overviews  
  - `docs/adr/` → Architecture Decision Records  
  - `docs/journal/` → weekly log
  - `src/` → source code (api, web, shared)
- Documented:
  - **system_overview.md** – project description, goals, and tools  
  - **architecture.md** – application and event flows (ASCII diagrams)
  - **cosmos-ticketing-considerations.md** – data and domain considerations for booking and ticket model
- Created six ADRs (001–006) for choice of Cosmos DB, Auth, IaC, Frontend, Azure Services, and Event-driven architecture
- Started code structure for MVP:
  - /src/shared/Ticketing.Contracts/ with project file and BookingDto preparation
  - /src/api/Ticketing.Api/ with project file, reference to Contracts, and minimal Program.cs
  - Established solution file that connects the projects

---

### Reflection
 
The structure is now in place and designed to be scalable, modular, and easy to containerize in the next step. The work has followed the principle of "one thing at a time". 

---

## Next Steps (Week 2)

- Implement BookingsController with BookingDto to enable the first API test.
- Create simple Blazor frontend (landing page) to verify the whole locally.
- Prepare infrastructure folder (infra/) with basic Bicep structure.
- Plan container setup for API, Web, and Functions.
- Start draft of CI/CD pipeline (GitHub Actions) with focus on dev environment.