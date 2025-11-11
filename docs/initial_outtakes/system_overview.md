## System Overview
### Purpose
  
This project aims to develop a cost-effective and event-driven ticketing system for public transportation. The application enables customers to view available trips, purchase and cancel tickets, and administrators to manage routes and zones.  
  
The system is built incrementally: first as a simple, synchronous, and resource-efficient system, with the possibility to later activate event-driven flows without changing the core architecture.  
  
### Technical Overview
  
The system uses .NET 8 Blazor Server as the frontend and a Controller-based API for business logic and data storage. All data is stored in Azure Cosmos DB (Serverless) to minimize costs at low load.
Features such as feature flags, secret management, and telemetry are integrated from the start to enable control and monitoring.  
  
#### Azure Services
| Service                     | Purpose                                                                        | 
| -------------------------- | ---------------------------------------------------------------------------- | 
| **App Service**            | Runs Blazor Server and API application.                                     | 
| **Cosmos DB (Serverless)** | Primary database for trips, bookings, and zones.                               | 
| **Azure Function**         | Used for background jobs triggered by events (e.g. `BookingCreated`). | 
| **Service Bus**            | Queue for asynchronous events (disabled in MVP).                              | 
| **App Configuration**      | Stores feature flags and environment configuration.                                 | 
| **Key Vault**              | Manages secrets and connection strings.                                    | 
| **Application Insights**   | Collects logs, telemetry, and performance data.                                  | 
| **API Management (APIM)**  | Gateway for public GET endpoints (read-only data).                           | 
  
#### .NET Components and Tools
| Component                       | Purpose                                                              | 
| ------------------------------- | ------------------------------------------------------------------ | 
| **Blazor Server**               | User interface for customers and administrators.                 | 
| **ASP.NET Controller API**      | Handles business logic and data access.                              | 
| **xUnit + NSubstitute**         | Unit testing.                                                    | 
| **Entity / Repository Pattern** | Abstraction for data storage in Cosmos DB.                           | 
| **Outbox Pattern**              | Prepares the system for event-driven publishing via Service Bus. | 
   
#### DevOps and Infrastructure  
 
| Area | Description  |
|-----------------------------------|-----------------------------------------------------------------------------------------------------------|
| **CI/CD**                         | Handled via Azure DevOps or GitHub Actions with YAML pipelines for automatic builds and deployment.  |
| **Infrastructure as Code (IaC)**  | Infrastructure can be managed with **Bicep**, **ARM templates**, or **Terraform**.
   Exact tool is decided later, but the focus is on creating a reproducible and cost-effective environment.                                   |
| **Environments**                       | Two environments are planned: `dev` and `prod`, separated in their own Resource Groups in Azure.                        |
| **Logging and Monitoring**      | Implemented with **Application Insights** and **KQL queries** 
   to visualize key values in dashboards or workbooks.                                                                              |
  
### MVP Scope
  
- Book and cancel trips (Bus, Train).
- Display available routes and zones.
- Manage trips via admin view.
- Track performance and usage via telemetry.
- Prepare (but not enable) event flow through Service Bus and Functions.
  
### Possible Future Extensions
  
- Enable Service Bus + Function pipeline for asynchronous events.
- Add ticket validation module (Inspector role).
- Expand zone-based pricing and peak-hour rules.
- Real-time metrics dashboard in Grafana.