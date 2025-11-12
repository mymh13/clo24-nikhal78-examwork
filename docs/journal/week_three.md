# Week 3 – Infrastructure and CI/CD Foundation

## Overview

During week 3, work began on establishing the first version of the infrastructure and connecting the project with CI/CD pipelines.
The goal was to create a minimal but functional chain for provisioning, authentication, and deployment, based on the existing codebase.

---

## Completed Activities
 
- Region choice: Confirmed that Sweden Central is used as the primary region for the environment, with focus on sustainability and low latency.
- Infrastructure: Created the first resource group rg-examwork-dev and prepared structure in infra/ for upcoming Bicep files.
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

---

## Reflection

The work with OIDC provided a deeper understanding of how secretless authentication works in a modern GitHub Actions pipeline.
It proved important to maintain a strict separation between Tenant ID, Subscription ID, and Client ID, especially when using student subscriptions in Azure.

Optimizing the CI/CD workflows to reduce GitHub Actions minutes consumption was crucial given the 2000-minute monthly limit.
Path filters and concurrency controls proved effective in preventing unnecessary workflow runs while maintaining full functionality.

The Git SHA-based versioning approach provides automatic traceability without manual version management, which aligns well with the incremental development approach.

Translating all documentation to English improves accessibility and aligns with professional development practices, while automated translation via LLMs made this really swift to do. Originally I thought all documentation had to be in Swedish given that this is a Swedish exam/thesis course, but I got updated information that English was fine. I see multiple benefits by documentation being accessible to everyone, so shifted to English then.

The next step will be to create a minimal App Service and test the first actual deployment.

---

## Next Steps (Week 4)

Examples:
- Create App Service in swedencentral and test first deployment.
- Introduce Application Insights for basic telemetry.
- Begin Bicep automation for resource group and App Service.
- Prepare pipeline for API containerization.

---
 