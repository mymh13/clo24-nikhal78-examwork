# Architecture Decision Records (ADR)
 
This directory contains documentation of important technical decisions in the project.  
The purpose is to create traceability between decisions, motivations, and any future changes. 
 
Each ADR describes **a decision**, its **background (context)**, **alternatives**, and **consequences**.  
Status indicates where in the lifecycle the decision is: `Proposed`, `Accepted`, `Rejected`, or `Superseded`. 
 
---
 
## ADR Index 

Last updated: 2025-11-24  

|    Nr   |                       Title                                     | Status   | Date      | Comment  |
|---------|-----------------------------------------------------------------|----------|------------|------------|
| ADR-001 | Database choice: Azure Cosmos DB (Serverless)                   | Accepted | 2025-10-30 | Cost and operations optimization |
| ADR-002 | Authentication: ASP.NET Identity + Entra ID                     | Accepted | 2025-10-30 | Shared model for customer/admin |
| ADR-003 | Infrastructure as Code (IaC) – tool choice: Bicep               | Accepted | 2025-10-30 | Easy integration in Azure DevOps |
| ADR-004 | Frontend choice: .NET 8 Blazor Server                           | Accepted | 2025-10-30 | Full .NET stack and easy hosting |
| ADR-005 | Cloud services choice: App Service, App Config, Key Vault, App Insights and APIM | Accepted | 2025-10-30 | Core Azure components for operations |
| ADR-006 | Event-driven architecture: Service Bus + Function + Outbox Pattern | Planned  | 2025-10-30 | Activated after MVP |
| ADR-007 | SSL Certificate: Manual Let's Encrypt on Free Tier | Accepted | 2025-11-12 | Cost-optimized SSL for custom domain |
| ADR-008 | Deployment Strategy: Docker Containers via GHCR | Accepted | 2025-11-13 | Resolves Oryx auto-detection issues, reliable container deployment |
| ADR-009 | Code Organization: Extension Methods Pattern | Accepted | 2025-11-18 | Improved readability and maintainability of startup configuration |
| ADR-010 | GDPR-Compliant Session Management: Server-Side Session Storage | Accepted | 2025-11-21 | GDPR compliance with ITicketStore for enhanced security |
| ADR-011 | Price Calculation System: Age-Based and Student Discounts with Zone Pricing | Accepted | 2025-11-24 | Flexible pricing model with price modifiers for discounts and zone-based pricing |
 
---
 
### Naming and format
- Filename: `ADR-###-example.md` (ascending number, three digits).  
- Headers: `Context`, `Decision`, `Consequences`, `Status`, `Alternatives`, `References`.  
- Status values: `Proposed`, `Accepted`, `Rejected`, `Superseded`.  
- When a decision is replaced, the old ADR is moved to `_archive/`.
 
---
 
### Purpose
The ADRs serve as a **decision log** for the system architecture.  
They help future developers understand **why** a decision was made, not just **what** was done.
 
---
 
### Disclaimer
 
ADR-000-template.md was created by an LLM model. 
I described the purpose and what I wanted, it generated a template which I then reviewed and adjusted. 
 
All following ADR documents I asked it to fill in according to the template and what I want to build. I have reviewed each one and adjusted where it felt relevant.