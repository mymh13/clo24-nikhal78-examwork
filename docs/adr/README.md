# Architecture Decision Records (ADR)
 
This directory contains documentation of important technical decisions in the project.  
The purpose is to create traceability between decisions, motivations, and any future changes. 
 
Each ADR describes **a decision**, its **background (context)**, **alternatives**, and **consequences**.  
Status indicates where in the lifecycle the decision is: `Proposed`, `Accepted`, `Rejected`, or `Superseded`. 
 
---
 
## ADR Index 

Last updated: 2025-12-03  

|    Nr   |                       Title                                     | Status   | Date      | Comment  |
|---------|-----------------------------------------------------------------|----------|------------|------------|
| ADR-001 | Database choice: Azure Cosmos DB (Serverless)                   | Accepted | 2025-10-30 | Cost and operations optimization |
| ADR-002 | Authentication: ASP.NET Identity + Entra ID                     | Accepted | 2025-10-30 | Shared model for customer/admin |
| ADR-003 | Infrastructure as Code (IaC) – tool choice: Bicep               | Accepted | 2025-10-30 | Bicep for IaC with GitHub Actions CI/CD integration |
| ADR-004 | Frontend choice: .NET 8 Blazor Server                           | Accepted | 2025-10-30 | Full .NET stack and easy hosting |
| ADR-005 | Cloud services choice: App Service, App Config, Key Vault, App Insights and APIM | Accepted | 2025-10-30 | Core Azure components for operations |
| ADR-006 | Event-driven architecture: Service Bus + Function + Outbox Pattern | Accepted | 2025-10-30 | Dual-system implementation complete, Service Bus integration in Phase 5 |
| ADR-007 | SSL Certificate: Manual Let's Encrypt on Free Tier | Accepted | 2025-11-12 | Cost-optimized SSL for custom domain |
| ADR-008 | Deployment Strategy: Docker Containers via GHCR & CI/CD Pipeline | Accepted | 2025-11-13 | Docker deployment via GHCR, GitHub Actions CI/CD pipeline architecture, environment management |
| ADR-009 | Code Organization: Extension Methods Pattern | Accepted | 2025-11-18 | Improved readability and maintainability of startup configuration |
| ADR-010 | GDPR-Compliant Session Management: Server-Side Session Storage | Accepted | 2025-11-21 | GDPR compliance with ITicketStore for enhanced security |
| ADR-011 | Price Calculation System: Age-Based and Student Discounts with Zone Pricing | Accepted | 2025-11-24 | Flexible pricing model with price modifiers for discounts and zone-based pricing |
| ADR-012 | Azure App Configuration for Feature Flags: Dual-System Coexistence | Accepted | 2025-11-25 | Runtime feature flag management for permanent dual-system architecture |
| ADR-013 | Outbox Pattern: Securing Data Integrity for Dual Write Operations | Accepted | 2025-11-26 | Transactional consistency for atomic booking creation and event publishing |
| ADR-014 | Sentinel Key Pattern: Hot-Reload Configuration Without Service Restart | Accepted | 2025-11-27 | Zero-downtime configuration updates for live demonstrations and operational flexibility |
| ADR-015 | Application Insights Telemetry Strategy: Custom Events for Dual-System Architecture | Accepted | 2025-12-01 | Custom telemetry events to differentiate and visualize Synchronous vs Event-Driven architecture modes |
| ADR-016 | Managed Identity & RBAC Strategy: Secure Azure Service Authentication | Accepted | 2025-12-01 | Eliminate connection strings and access keys using Azure Managed Identity with RBAC for all Azure service authentication |
| ADR-017 | Service/Component Organization Pattern: Code Structure and Separation of Concerns | Accepted | 2025-12-01 | Directory structure (Services, Controllers, Helpers, Components, Extensions), separation of concerns, dependency injection patterns |
| ADR-018 | Error Handling & Logging Strategy: Exception Management and Observability | Accepted | 2025-12-01 | Layered error handling (global, controller, service), structured logging with ILogger, user-friendly error messages, Application Insights integration |
| ADR-019 | API Design Pattern: Controller-Based REST | Accepted | 2025-12-01 | Controller-based REST API with [ApiController], RESTful routing conventions, HTTP verb attributes, authorization integration, no API versioning |
| ADR-020 | QR Code Implementation: Activation-Time Generation with Cosmos DB Storage | Accepted | 2025-12-03 | QR code generation at activation time, JSON-encoded data with validity period, base64 storage in Cosmos DB, instant retrieval for display |
 
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