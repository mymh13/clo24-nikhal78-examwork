# Week 3 – Infrastructure and CI/CD Foundation

## Overview

During week 3, work began on establishing the first version of the infrastructure and connecting the project with CI/CD pipelines.
The goal was to create a minimal but functional chain for provisioning, authentication, and deployment, based on the existing codebase.

---

## Completed Activities
 
- Region choice: Confirmed that Sweden Central is used as the primary region for the environment, with focus on sustainability and low latency.
- Infrastructure:
    - Created the first resource group rg-examwork-dev.
    - Established Bicep structure following planned architecture (modules/, env/dev/, env/prod/).
    - Created App Service module (infra/modules/appservice.bicep) and dev environment deployment (infra/env/dev/main.bicep).
    - Successfully deployed App Service `examwork-web-dev` (Linux, .NET 8, Basic B1 tier) in Sweden Central.
    - Upgraded App Service from Free tier to Basic B1 to enable SSL certificate bindings.
    - Configured custom domain `ticket.mymh.dev` with CNAME pointing to App Service.
    - Created ADR-007 documenting SSL certificate decision (manual Let's Encrypt via Docker).
    - Completed SSL certificate setup:
      - Added DNS TXT record `_acme-challenge.ticket.mymh.dev` for Let's Encrypt DNS-01 validation.
      - Generated Let's Encrypt certificate using Certbot (Docker).
      - Converted certificate to PFX format.
      - Uploaded certificate to Azure App Service.
      - Bound SSL certificate to custom domain via Azure Portal (Basic tier required).
    - Custom domain now accessible via HTTPS: `https://ticket.mymh.dev` with valid SSL certificate.
- Azure integration:
    - Registered the app github-oidc-examwork in Entra ID.
    - Configured Federated credentials (OIDC) for GitHub Actions.
    - Assigned the Contributor role to the service principal in the resource group.
- CI/CD:
    - Implemented OIDC Smoke Test workflow (Azure login).
    - Completed successful run – Azure OIDC login verified without exposing ID data.
    - Enhanced CI workflow with path filters to reduce unnecessary runs (only triggers on code changes, not docs/infra).
    - Added concurrency control to cancel duplicate workflow runs and save GitHub Actions minutes.
    - Implemented Git SHA-based versioning for all builds (short SHA embedded in assembly version).
    - Fixed CD workflow to skip deployment gracefully when App Service doesn't exist yet.
    - Resolved .NET SDK version mismatch and assembly versioning issues.
    - **Deployment Strategy Change:**
      - Initially attempted zip-deploy with Oryx build system.
      - Encountered persistent issue: Oryx auto-detected PHP instead of .NET despite explicit configuration.
      - Multiple troubleshooting attempts (app settings, manifest files, container restarts) were unsuccessful.
      - Switched to Docker container deployment via GitHub Container Registry (GHCR).
      - Created Dockerfile with multi-stage build for Blazor Server application.
      - Updated CI workflow to build and push Docker images to GHCR (~43 seconds).
      - Updated CD workflow to deploy container images to App Service (~44 seconds).
      - Successfully deployed Blazor landing page to `https://ticket.mymh.dev`.
      - Created ADR-008 documenting the deployment strategy decision and implementation.
- Documentation:
    - Translated all project documentation from Swedish to English (automated translation via LLMs).
    - Updated all ADR files, journal entries, glossary, and README files to English.
    - Maintained formatting and structure throughout the translation process.
    - Created ADR-007 for SSL certificate decision (manual Let's Encrypt via Docker).
    - Created ADR-008 for deployment strategy decision (Docker containers via GHCR, resolving Oryx issues).

---

## Reflection

The work with OIDC provided a deeper understanding of how secretless authentication works in a modern GitHub Actions pipeline.
It proved important to maintain a strict separation between Tenant ID, Subscription ID, and Client ID, especially when using student subscriptions in Azure.

Optimizing the CI/CD workflows to reduce GitHub Actions minutes consumption was crucial given the 2000-minute monthly limit.
Path filters and concurrency controls proved effective in preventing unnecessary workflow runs while maintaining full functionality.

The Git SHA-based versioning approach provides automatic traceability without manual version management, which aligns well with the incremental development approach.

Translating all documentation to English improves accessibility and aligns with professional development practices, while automated translation via LLMs made this really swift to do. Originally I thought all documentation had to be in Swedish given that this is a Swedish exam/thesis course, but I got updated information that English was fine. I see multiple benefits by documentation being accessible to everyone, so shifted to English then.

Establishing the Bicep structure with modules and environment separation provides a solid foundation for infrastructure automation. The modular approach makes it easy to add new resources (Cosmos DB, Key Vault, etc.) as the project progresses.

The App Service is now deployed and accessible via both the default Azure URL and the custom domain. DNS propagation was faster than expected, allowing immediate configuration of the custom domain binding.

SSL certificate configuration was completed using the Docker-based Let's Encrypt approach documented in ADR-007. The process involved adding a DNS TXT record at Loopia, generating the certificate via Certbot, converting to PFX format, and uploading to Azure. A key learning was that the Free tier does not support SSL certificate bindings - the Azure CLI commands failed silently without clear error messages, making it difficult to diagnose. The issue was only discovered when attempting to bind the certificate through the Azure Portal, which explicitly stated that Basic tier or higher is required. Upgrading to Basic B1 enabled the SSL binding functionality, and the certificate was successfully bound via the portal. The custom domain is now fully functional with HTTPS at `https://ticket.mymh.dev`.

**Deployment Challenges and Resolution:**  
The initial deployment approach using zip-deploy with Oryx encountered a critical issue: despite explicit configuration (`linuxFxVersion: 'DOTNET|8.0'`, `appCommandLine: 'dotnet Ticketing.Web.dll'`), Oryx consistently auto-detected PHP as the runtime. This persisted across multiple troubleshooting attempts including recreating the App Service, adding manifest files, and configuring app settings. The root cause appears to be Oryx's auto-detection logic running before files are fully deployed, or cached container image choices overriding explicit configuration.

After extensive troubleshooting, the decision was made to switch to Docker container deployment via GHCR. This approach bypasses Oryx entirely, provides full control over the runtime environment, and ensures reproducible deployments. The implementation was straightforward: created a multi-stage Dockerfile, updated CI workflow to build and push images to GHCR, and updated CD workflow to deploy containers to App Service. The first successful deployment completed in ~43 seconds for CI and ~44 seconds for CD, and the Blazor landing page is now live at `https://ticket.mymh.dev` with proper .NET runtime. This experience highlighted the importance of having a reliable, predictable deployment strategy, and Docker containers provide exactly that. The decision and implementation are documented in ADR-008.

---

## Next Steps (Week 4)

- Introduce Application Insights for basic telemetry and monitoring.
- Begin adding Cosmos DB module to Bicep infrastructure.
- Prepare API containerization (extend Docker approach to API project).
- Continue development of Blazor landing page features.

---
 