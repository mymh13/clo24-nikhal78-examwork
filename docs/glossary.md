# Glossary

This glossary collects central concepts, abbreviations, and terms used in the project.  
The purpose is to facilitate understanding of technical documents and decisions, especially for readers who are not familiar with the Azure or .NET ecosystem.

---

## Azure Services

| Concept / Acronym         | Explanation                                            | Comment / Usage in Project |
|---------------------------|--------------------------------------------------------|----------------------------|
| **App Service**           | Azure service for running web apps and APIs.           | Runs both Blazor Server and API part in MVP. |
| **App Configuration**     | Central storage for settings and feature flags.        | Controls activation of features and environment configuration. |
| **Key Vault**             | Secure storage of secrets.                             | Used for connection strings and API keys. |
| **Cosmos DB**             | Distributed NoSQL database from Azure.                 | Primary database in MVP (Serverless mode). |
| **Service Bus**           | Message queue for asynchronous communication.          | Used in future event-driven flow. |
| **Azure Function**        | Serverless function for event-driven logic.            | Consumes events (e.g. `BookingCreated`). |
| **Application Insights**  | Service for logging, monitoring, and telemetry.        | Tracks events and performance in the application. |
| **API Management (APIM)** | Gateway for public endpoints.                          | Handles GET requests, caching, and rate limiting. |

---

## .NET Components

| Concept / Acronym      | Explanation                                            | Comment / Usage in Project |
|------------------------|--------------------------------------------------------|----------------------------|
| **Blazor Server**      | Web framework in .NET for server-driven interfaces.    | Builds frontend and handles login. |
| **ASP.NET Identity**   | Framework for user management and authentication.      | Used for customer login. |
| **Entra ID**           | Previously Azure Active Directory (AAD).               | Handles authentication for admin/inspector. |
| **xUnit / NSubstitute**| Testing tools for .NET projects.                       | Used for unit testing. |

---

## Infrastructure and DevOps

| Concept / Acronym      | Explanation                                            | Comment / Usage in Project |
|------------------------|--------------------------------------------------------|----------------------------|
| **IaC**                | *Infrastructure as Code.*                              | Principle for defining infrastructure in code (Bicep). |
| **Bicep**              | Microsoft's IaC language for Azure.                    | Used to declaratively create resources. |
| **CI/CD**              | *Continuous Integration / Continuous Deployment.*      | Automatic builds and deliveries via YAML pipelines. |
| **YAML**               | Configuration language.                                | Used for pipelines in DevOps. |

---

## Architecture Patterns

| Concept / Acronym          | Explanation                                               | Comment / Usage in Project |
|----------------------------|-----------------------------------------------------------|----------------------------|
| **Outbox Pattern**         | Design pattern for reliable publishing of events.         | Prepared in the API for event flows. |
| **Event-driven architecture** | Architecture pattern where components react to events. | Introduced after MVP via Service Bus + Function. |

---

## Other Concepts

| Concept / Acronym      | Explanation                                             | Comment / Usage in Project |
|------------------------|--------------------------------------------------------|----------------------------|
| **ADR**                | *Architecture Decision Record.*                        | Document that motivates and logs technical decisions. |
| **HDA**                | *Hypermedia-Driven Application.*                       | Architecture style that HTMX builds on. |
| **HTMX**               | JavaScript library for server-driven interaction.      | Alternative to SPA frameworks (not chosen in this project). |
| **MVP**                | *Minimum Viable Product.*                              | First working version of the system. |

---

> **Note:**  
> The glossary is updated continuously as new technical concepts and design patterns are introduced in the project.
