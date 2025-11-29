# How to Get Application Insights Connection String

## Quick Command

```bash
az monitor app-insights component show \
  --app examwork-insights-dev \
  --resource-group rg-examwork-dev \
  --query connectionString \
  -o tsv
```

## Alternative: Get from Azure Portal

1. Navigate to Azure Portal → Application Insights → `examwork-insights-dev`
2. Click **"Overview"** in the left menu
3. Find **"Connection String"** in the Essentials section
4. Click the **copy icon** next to the connection string

## Add to GitHub Secrets

1. Go to your GitHub repository
2. Navigate to **Settings** → **Secrets and variables** → **Actions**
3. Click **"New repository secret"**
4. **Name:** `APPLICATIONINSIGHTS_CONNECTION_STRING`
5. **Value:** Paste the connection string from the command above
6. Click **"Add secret"**

## Verify Connection String Format

The connection string should look like:
```
InstrumentationKey=xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx;IngestionEndpoint=https://swedencentral-0.in.applicationinsights.azure.com/;LiveEndpoint=https://swedencentral.livediagnostics.monitor.azure.com/
```

## Note

The connection string is already configured in the App Service via Bicep deployment, but if you need it for GitHub Actions or local development, use the command above.

