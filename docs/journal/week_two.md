# Week 2 – Code Structure and First Runnable Version

## Overview

During week 2, the focus has been on moving from the planning phase to a runnable codebase.
The goal was to establish the technical foundation for both backend and frontend and verify that the application works locally before infrastructure and CI/CD begin.

---

## Completed Activities

* **Solution and Project Structure**

  * Created main solution `clo24-nikhal78-examwork.sln` in the project root.
  * Added projects `Ticketing.Api` and `Ticketing.Contracts` to the solution.
  * Secured references between projects (`API → Contracts`).
  * Established permanent repo structure:

    ```
    src/
    ├─ api/
    ├─ web/
    ├─ shared/
    ├─ functions/
    └─ tests/
    ```

* **Shared Project**
  * Created `Ticketing.Contracts.csproj` (class library, net8.0).
  * Prepared `BookingDto.cs` according to documented domain fields in `cosmos-ticketing-considerations.md`.

* **API Project**
  * Created `Ticketing.Api.csproj` with Web SDK and reference to `Contracts`.
  * Implemented minimal `Program.cs` for runnable Web API base.
  * Confirmed successful compilation and local execution.

* **Web Project (Blazor Server)**
  * Created `Ticketing.Web.csproj` (Blazor Server, net8.0).
  * Added minimal `Program.cs`, `_Host.cshtml`, `App.razor`, `_Imports.razor`, `MainLayout.razor`, and `Index.razor`.
  * Ensured the application runs locally and renders a working **landing page**.
  * Handled initial errors around `_Host` and `PageTitle` by completing the project structure.
  
* **Other**
  * Considered design choices around **Entra ID** (for admin roles) and **container strategy** (for API, Web, and Functions).
  * Decided that containerization happens in conjunction with CI/CD and not during the MVP phase.
  * Established region choice:
    * **Sweden Central (`swedencentral`)** as primary region (sustainability + low latency)
    * **West Europe (`westeurope`)** as fallback region

---

## Reflection

The foundation for the application is now runnable and follows a clean, modular architecture.
The work has continued according to the principle *"one file at a time"*, which has minimized errors and made it possible to verify each step locally before the next was introduced.
The Blazor Server frontend and API are ready to be extended with functionality for bookings and data storage.

---

## Next Steps (Week 3)

* Create **infra/** structure with Bicep and define Azure resources (App Service, Cosmos DB, Key Vault, App Config).
* Prepare **GitHub Actions workflows** for CI (build) and CD (deploy to dev).
* Evaluate **Dockerfile** for API and Web in preparation for upcoming containerization.
* Confirm successful provisioning in **Sweden Central** before expanding the infrastructure.

---
