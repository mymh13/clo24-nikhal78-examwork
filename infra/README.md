# Infra (dev) – minimal start

Purpose: provision the smallest possible Azure resource to host the Blazor Server landing page.

## Prerequisites
- Azure CLI (`az`) logged in to the correct tenant/subscription
- Resource group created (Bicep runs at RG level)

## 1) Create resource group (once)
```bash
az group create -n rg-examwork-dev -l swedencentral
```

## 2) Deploy App Service (Linux, .NET 8)
```bash
# Deploy using Bicep (dev environment)
az deployment group create \
  --resource-group rg-examwork-dev \
  --template-file infra/env/dev/main.bicep \
  --parameters @infra/env/dev/main.parameters.json

# Get the App Service URL
az webapp show -n examwork-web-dev -g rg-examwork-dev --query defaultHostName -o tsv
```

**Note:** The App Service name must be globally unique. If `examwork-web-dev` is taken, update `infra/env/dev/main.parameters.json` with a different name.

After deployment, configure the GitHub Actions variables:
- `WEBAPP_NAME`: The App Service name (e.g., `examwork-web-dev`)
- `WEBAPP_RG`: `rg-examwork-dev`

## Structure

- `modules/` - Reusable Bicep modules (appservice, cosmos, appconfiguration, servicebus, etc.)
- `env/dev/` - Development environment deployments
- `env/prod/` - Production environment deployments (future)
- `policies/` - APIM policies (future)

## Resources Deployed

The infrastructure includes:
- **App Service** - Hosts the Blazor Server application
- **Cosmos DB** - Serverless NoSQL database
- **Application Insights** - Monitoring and telemetry
- **Key Vault** - Secrets management (RBAC, managed identity)
- **App Configuration** - Feature flags and configuration (managed identity)
- **Service Bus** - Message queue for event-driven architecture (managed identity)