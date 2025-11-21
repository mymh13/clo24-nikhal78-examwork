# Week 3 – Infrastructure and CI/CD Foundation

## Overview

During week 3, work began on establishing the first version of the infrastructure and connecting the project with CI/CD pipelines.
The goal was to create a minimal but functional chain for provisioning, authentication, and deployment, based on the existing codebase.

---

## Completed Activities

### Infrastructure Setup
- **Region & Resource Group:** Confirmed Sweden Central as primary region, created `rg-examwork-dev`. Established modular Bicep architecture (`modules/`, `env/dev/`, `env/prod/`).
- **Modules:** Created App Service, Cosmos DB (Serverless mode), and Application Insights Bicep modules following the modular pattern.

### App Service Deployment
- **Deployment:** Successfully deployed App Service `examwork-web-dev` (Linux, .NET 8, Basic B1 tier). Upgraded from Free tier to enable SSL certificate bindings.
- **Custom Domain & SSL:** Configured `ticket.mymh.dev` with CNAME, generated Let's Encrypt certificate via Docker (Certbot), converted to PFX, and bound to custom domain. Documented in ADR-007. Custom domain accessible via HTTPS.

### Cosmos DB Setup
- **Deployment:** Deployed Cosmos DB account `examwork-cosmos-dev` in Serverless mode. Created `ticketing` database and `bookings` container with partition key `/customerId`. Verified pay-per-request pricing with no fixed costs.

### Application Insights Setup
- **Deployment:** Deployed Application Insights `examwork-insights-dev`, created Bicep module, and integrated with App Service via connection string. Added Application Insights SDK to application code.
- **Deployment Challenge:** Initial deployment failed due to empty `WorkspaceResourceId` property in Bicep template. Resolved by removing the property, deployment completed successfully.

### Azure Authentication (OIDC)
- **Configuration:** Registered `github-oidc-examwork` in Entra ID, configured OIDC federated credentials for GitHub Actions, and assigned Contributor role to service principal on resource group.

### CI/CD Pipelines
- **OIDC Verification:** Implemented and verified OIDC Smoke Test workflow for secretless Azure authentication.
- **CI Workflow Optimizations:** Added path filters and concurrency control to reduce unnecessary runs, implemented Git SHA-based versioning, resolved .NET SDK version mismatch issues.
- **Deployment Strategy (ADR-008):** Initially attempted zip-deploy with Oryx but encountered persistent PHP auto-detection issue. Switched to Docker container deployment via GHCR with multi-stage Dockerfile. CI builds and pushes images (~43 seconds), CD deploys to App Service (~44 seconds). Successfully deployed Blazor landing page to `https://ticket.mymh.dev`.

### Frontend & UI
- **Landing Page Enhancement:** Added tech-overview section to landing page with custom CSS styling, dark theme with cyan accent color (#4ec9b0). Displays four categories (Infrastructure & Deployment, Application Stack, Data & Storage, Security & Authentication) reflecting current state and planned features.

### Documentation
- **Translation:** Translated all project documentation from Swedish to English (automated via LLMs), updating ADR files, journal entries, glossary, and README files while maintaining formatting.
- **Architecture Decision Records:** Created ADR-007 (SSL Certificate) and ADR-008 (Deployment Strategy).

---

## Reflection

OIDC work provided deeper understanding of secretless authentication in GitHub Actions. Maintaining strict separation between Tenant ID, Subscription ID, and Client ID proved important, especially with student subscriptions.

CI/CD workflow optimizations (path filters, concurrency controls) effectively reduced GitHub Actions minutes consumption. Git SHA-based versioning provides automatic traceability without manual version management.

Documentation translation to English improved accessibility. Originally thought Swedish was required, but English is acceptable and provides broader accessibility.

Bicep modular structure with environment separation provides solid foundation for infrastructure automation. Cosmos DB and Application Insights successfully added using same pattern. Serverless mode ensures zero cost when idle, aligning with MVP cost optimization goals.

App Service deployed and accessible via custom domain. SSL certificate configuration completed via Docker-based Let's Encrypt (ADR-007). Key learning: Free tier doesn't support SSL bindings - Azure CLI failed silently, issue only discovered via Azure Portal requiring Basic tier or higher. Upgraded to Basic B1, certificate successfully bound. Custom domain fully functional with HTTPS.

**Deployment Challenges and Resolution:**  
The initial deployment approach using zip-deploy with Oryx encountered a critical issue: despite explicit configuration (`linuxFxVersion: 'DOTNET|8.0'`, `appCommandLine: 'dotnet Ticketing.Web.dll'`), Oryx consistently auto-detected PHP as the runtime. This persisted across multiple troubleshooting attempts including recreating the App Service, adding manifest files, and configuring app settings. The root cause appears to be Oryx's auto-detection logic running before files are fully deployed, or cached container image choices overriding explicit configuration.

After extensive troubleshooting, the decision was made to switch to Docker container deployment via GHCR. This approach bypasses Oryx entirely, provides full control over the runtime environment, and ensures reproducible deployments. The implementation was straightforward: created a multi-stage Dockerfile, updated CI workflow to build and push images to GHCR, and updated CD workflow to deploy containers to App Service. The first successful deployment completed in ~43 seconds for CI and ~44 seconds for CD, and the Blazor landing page is now live at `https://ticket.mymh.dev` with proper .NET runtime. This experience highlighted the importance of having a reliable, predictable deployment strategy, and Docker containers provide exactly that. The decision and implementation are documented in ADR-008.

---

## Next Steps (Week 4)

- Add Azure Key Vault for secure secret management (Cosmos DB connection strings, etc.).
- Prepare API containerization (extend Docker approach to API project).
- Begin connecting application code to Cosmos DB for data persistence.
- Continue development of Blazor landing page features.
- Verify Application Insights telemetry collection after next application deployment.

---
 