# Week 3 – Infrastructure and CI/CD Foundation

## Overview

During week 3, work began on establishing the first version of the infrastructure and connecting the project with CI/CD pipelines.
The goal was to create a minimal but functional chain for provisioning, authentication, and deployment, based on the existing codebase.

---

## Completed Activities

### Infrastructure Setup
- **Region:** Confirmed Sweden Central as primary region (sustainability and low latency focus).
- **Resource Group:** Created `rg-examwork-dev` in Sweden Central.
- **Bicep Structure:** Established modular architecture (`modules/`, `env/dev/`, `env/prod/`).
- **App Service Module:** Created `infra/modules/appservice.bicep` and dev environment deployment.
- **Cosmos DB Module:** Created `infra/modules/cosmosdb.bicep` with Serverless mode configuration.

### App Service Deployment
- **Deployment:** Successfully deployed App Service `examwork-web-dev` (Linux, .NET 8, Basic B1 tier).
- **Tier Upgrade:** Upgraded from Free tier to Basic B1 to enable SSL certificate bindings.
- **Custom Domain:** Configured `ticket.mymh.dev` with CNAME pointing to App Service.
- **SSL Certificate:**
  - Created ADR-007 documenting SSL certificate decision (manual Let's Encrypt via Docker).
  - Added DNS TXT record `_acme-challenge.ticket.mymh.dev` for Let's Encrypt DNS-01 validation.
  - Generated Let's Encrypt certificate using Certbot (Docker).
  - Converted certificate to PFX format and uploaded to Azure App Service.
  - Bound SSL certificate to custom domain via Azure Portal.
  - Custom domain now accessible via HTTPS: `https://ticket.mymh.dev`.

### Cosmos DB Setup
- **Account:** Deployed Cosmos DB account `examwork-cosmos-dev` in Sweden Central (Serverless mode).
- **Database:** Created database `ticketing`.
- **Container:** Created container `bookings` with partition key `/customerId`.
- **Configuration:** Verified Serverless mode (pay-per-request, no fixed costs).
- **Note:** Confirmed Free Tier is not applicable with Serverless mode (expected behavior).

### Azure Authentication (OIDC)
- **App Registration:** Registered `github-oidc-examwork` in Entra ID.
- **Federated Credentials:** Configured OIDC for GitHub Actions.
- **Permissions:** Assigned Contributor role to service principal on resource group.

### CI/CD Pipelines
- **OIDC Verification:** Implemented and verified OIDC Smoke Test workflow (secretless Azure login).
- **CI Workflow Optimizations:**
  - Added path filters to reduce unnecessary runs (only triggers on code changes).
  - Added concurrency control to cancel duplicate workflow runs.
  - Implemented Git SHA-based versioning for all builds.
  - Resolved .NET SDK version mismatch and assembly versioning issues.
- **Deployment Strategy (ADR-008):**
  - Initially attempted zip-deploy with Oryx build system.
  - Encountered persistent issue: Oryx auto-detected PHP instead of .NET.
  - Multiple troubleshooting attempts (app settings, manifest files, restarts) unsuccessful.
  - Switched to Docker container deployment via GitHub Container Registry (GHCR).
  - Created Dockerfile with multi-stage build for Blazor Server application.
  - Updated CI workflow to build and push Docker images to GHCR (~43 seconds).
  - Updated CD workflow to deploy container images to App Service (~44 seconds).
  - Successfully deployed Blazor landing page to `https://ticket.mymh.dev`.
- **CD Workflow:** Fixed to skip deployment gracefully when App Service doesn't exist yet.

### Frontend & UI
- **Landing Page Enhancement:**
  - Added tech-overview section to landing page (`Index.razor`).
  - Created custom CSS styling (`wwwroot/css/custom.css`) matching other project designs.
  - Implemented dark theme with cyan accent color (#4ec9b0) for consistency.
  - Tech-overview displays four categories: Infrastructure & Deployment, Application Stack, Data & Storage, Security & Authentication.
  - Content reflects current deployed state and planned features (marked as "planned").

### Documentation
- **Translation:** Translated all project documentation from Swedish to English (automated via LLMs).
  - Updated all ADR files, journal entries, glossary, and README files.
  - Maintained formatting and structure throughout.
- **Architecture Decision Records:**
  - Created ADR-007: SSL Certificate (manual Let's Encrypt via Docker).
  - Created ADR-008: Deployment Strategy (Docker containers via GHCR).

---

## Reflection

The work with OIDC provided a deeper understanding of how secretless authentication works in a modern GitHub Actions pipeline.
It proved important to maintain a strict separation between Tenant ID, Subscription ID, and Client ID, especially when using student subscriptions in Azure.

Optimizing the CI/CD workflows to reduce GitHub Actions minutes consumption was crucial given the 2000-minute monthly limit.
Path filters and concurrency controls proved effective in preventing unnecessary workflow runs while maintaining full functionality.

The Git SHA-based versioning approach provides automatic traceability without manual version management, which aligns well with the incremental development approach.

Translating all documentation to English improves accessibility and aligns with professional development practices, while automated translation via LLMs made this really swift to do. Originally I thought all documentation had to be in Swedish given that this is a Swedish exam/thesis course, but I got updated information that English was fine. I see multiple benefits by documentation being accessible to everyone, so shifted to English then.

Establishing the Bicep structure with modules and environment separation provides a solid foundation for infrastructure automation. The modular approach makes it easy to add new resources (Cosmos DB, Key Vault, etc.) as the project progresses. Cosmos DB was successfully added using the same modular pattern, demonstrating the flexibility of the infrastructure setup. The Serverless mode configuration ensures zero cost when idle, which aligns perfectly with the MVP phase and cost optimization goals.

The App Service is now deployed and accessible via both the default Azure URL and the custom domain. DNS propagation was faster than expected, allowing immediate configuration of the custom domain binding.

SSL certificate configuration was completed using the Docker-based Let's Encrypt approach documented in ADR-007. The process involved adding a DNS TXT record at Loopia, generating the certificate via Certbot, converting to PFX format, and uploading to Azure. A key learning was that the Free tier does not support SSL certificate bindings - the Azure CLI commands failed silently without clear error messages, making it difficult to diagnose. The issue was only discovered when attempting to bind the certificate through the Azure Portal, which explicitly stated that Basic tier or higher is required. Upgrading to Basic B1 enabled the SSL binding functionality, and the certificate was successfully bound via the portal. The custom domain is now fully functional with HTTPS at `https://ticket.mymh.dev`.

**Deployment Challenges and Resolution:**  
The initial deployment approach using zip-deploy with Oryx encountered a critical issue: despite explicit configuration (`linuxFxVersion: 'DOTNET|8.0'`, `appCommandLine: 'dotnet Ticketing.Web.dll'`), Oryx consistently auto-detected PHP as the runtime. This persisted across multiple troubleshooting attempts including recreating the App Service, adding manifest files, and configuring app settings. The root cause appears to be Oryx's auto-detection logic running before files are fully deployed, or cached container image choices overriding explicit configuration.

After extensive troubleshooting, the decision was made to switch to Docker container deployment via GHCR. This approach bypasses Oryx entirely, provides full control over the runtime environment, and ensures reproducible deployments. The implementation was straightforward: created a multi-stage Dockerfile, updated CI workflow to build and push images to GHCR, and updated CD workflow to deploy containers to App Service. The first successful deployment completed in ~43 seconds for CI and ~44 seconds for CD, and the Blazor landing page is now live at `https://ticket.mymh.dev` with proper .NET runtime. This experience highlighted the importance of having a reliable, predictable deployment strategy, and Docker containers provide exactly that. The decision and implementation are documented in ADR-008.

---

## Next Steps (Week 4)

- Introduce Application Insights for basic telemetry and monitoring.
- Add Azure Key Vault for secure secret management (Cosmos DB connection strings, etc.).
- Prepare API containerization (extend Docker approach to API project).
- Begin connecting application code to Cosmos DB for data persistence.
- Continue development of Blazor landing page features.

---
 