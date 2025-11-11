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

---

## Reflection

The work with OIDC provided a deeper understanding of how secretless authentication works in a modern GitHub Actions pipeline.
It proved important to maintain a strict separation between Tenant ID, Subscription ID, and Client ID, especially when using student subscriptions in Azure.
The next step will be to create a minimal App Service and test the first actual deployment.

---

## Next Steps (Week 4)

Examples:
- Create App Service in swedencentral and test first deployment.
- Introduce Application Insights for basic telemetry.
- Begin Bicep automation for resource group and App Service.
- Prepare pipeline for API containerization.

---
 