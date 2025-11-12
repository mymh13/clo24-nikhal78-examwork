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
    - Successfully deployed App Service `examwork-web-dev` (Linux, .NET 8, Free tier) in Sweden Central.
    - Configured custom domain `ticket.mymh.dev` with CNAME pointing to App Service.
    - Created ADR-007 documenting SSL certificate decision (manual Let's Encrypt via Docker).
- Azure integration:
    - Registered the app github-oidc-examwork in Entra ID.
    - Configured Federated credentials (OIDC) for GitHub Actions.
    - Assigned the Contributor role to the service principal in the resource group.
- CI/CD:
    - Implemented OIDC Smoke Test workflow (Azure login).
    - Completed successful run – Azure OIDC login verified without exposing ID data.
    - Prepared cd-web-dev.yaml for future deployment to App Service.
    - Enhanced CI workflow with path filters to reduce unnecessary runs (only triggers on code changes, not docs/infra).
    - Added concurrency control to cancel duplicate workflow runs and save GitHub Actions minutes.
    - Implemented Git SHA-based versioning for all builds (short SHA embedded in assembly version).
    - Fixed CD workflow to skip deployment gracefully when App Service doesn't exist yet.
    - Resolved .NET SDK version mismatch and assembly versioning issues.
- Documentation:
    - Translated all project documentation from Swedish to English (automated translation via LLMs).
    - Updated all ADR files, journal entries, glossary, and README files to English.
    - Maintained formatting and structure throughout the translation process.
    - Created ADR-007 for SSL certificate decision (manual Let's Encrypt on Free tier).

---

## Reflection

The work with OIDC provided a deeper understanding of how secretless authentication works in a modern GitHub Actions pipeline.
It proved important to maintain a strict separation between Tenant ID, Subscription ID, and Client ID, especially when using student subscriptions in Azure.

Optimizing the CI/CD workflows to reduce GitHub Actions minutes consumption was crucial given the 2000-minute monthly limit.
Path filters and concurrency controls proved effective in preventing unnecessary workflow runs while maintaining full functionality.

The Git SHA-based versioning approach provides automatic traceability without manual version management, which aligns well with the incremental development approach.

Translating all documentation to English improves accessibility and aligns with professional development practices, while automated translation via LLMs made this really swift to do. Originally I thought all documentation had to be in Swedish given that this is a Swedish exam/thesis course, but I got updated information that English was fine. I see multiple benefits by documentation being accessible to everyone, so shifted to English then.

Establishing the Bicep structure with modules and environment separation provides a solid foundation for infrastructure automation. The modular approach makes it easy to add new resources (Cosmos DB, Key Vault, etc.) as the project progresses.

The App Service is now deployed and accessible via both the default Azure URL and the custom domain. DNS propagation was faster than expected, allowing immediate configuration of the custom domain binding. The next step is to configure the SSL certificate using the Docker-based Let's Encrypt approach documented in ADR-007.

---

## Next Steps (Week 4)

Examples:
- Configure SSL certificate for `ticket.mymh.dev` using Let's Encrypt (Docker approach from ADR-007).
- Test first deployment via CD workflow to App Service.
- Introduce Application Insights for basic telemetry.
- Begin adding Cosmos DB module to Bicep infrastructure.
- Prepare pipeline for API containerization.

---
 