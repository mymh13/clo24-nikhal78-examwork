# Demo Page Setup Guide

## Overview

The demo page (`/demo`) provides a **one-stop shop** for demonstrating the event-driven architecture. It combines three key components on a single page:

1. **Application Insights Query Results** (Top Left) - Live KQL query results showing custom events
2. **Event-Driven Architecture Status** (Top Right) - Feature flag toggle with health status
3. **Booking Management** (Bottom, Full Width) - Full booking creation and management interface with full-width table

## Access

- **URL:** `https://ticket.mymh.dev/demo`
- **Authentication:** Admin role required
- **Link from Admin Dashboard:** Click "🚀 Go to Demo Page" button

## Features

### 1. Application Insights Section (Top)

- **Auto-refresh:** Toggle to automatically refresh every 10 seconds
- **Manual refresh:** "Refresh Events" button
- **Default query:** Shows all custom events from the last hour, grouped by event name
- **Displays:** Event counts in a table format

**Query executed:**
```kusto
customEvents
| where timestamp > ago(1h)
| summarize Count = count() by name
| order by Count desc
```

### 2. Event-Driven Architecture Status (Top Right)

- **Mini health check:** Shows App Configuration, Feature Manager, and Outbox Service status
- **Feature flag toggle:** Button to enable/disable event-driven mode
- **Propagation tracking:** Visual feedback when toggling (waiting → success)
- **Cooldown period:** 5-second cooldown after toggle to prevent rapid clicks

### 3. Booking Management (Bottom, Full Width)

- **Create booking:** Full form with email, name, and zone selection
- **Get bookings:** Search by customer ID or list all bookings
- **Delete bookings:** Delete button for each booking (Admin only)
- **Auto-refresh insights:** After creating a booking, insights automatically refresh (with 2-second delay for ingestion)
- **Full-width layout:** Booking table uses full page width for better visibility

## Technical Implementation

### API Endpoint: `/api/applicationinsights/query`

**Method:** POST  
**Authorization:** Admin role required  
**Request Body:**
```json
{
  "query": "customEvents | where timestamp > ago(1h) | summarize Count = count() by name",
  "timespan": "PT1H"
}
```

**Response:**
```json
{
  "success": true,
  "tables": [
    {
      "name": "PrimaryResult",
      "columns": [
        { "name": "name", "type": "string" },
        { "name": "Count", "type": "long" }
      ],
      "rows": [
        ["BookingCreated", 5],
        ["OutboxEventCreated", 5],
        ...
      ]
    }
  ]
}
```

### Authentication

The API uses **Azure Managed Identity** to authenticate with Application Insights:
- Uses `DefaultAzureCredential` for token acquisition
- Requires `https://management.azure.com/.default` scope
- Works automatically in Azure App Service (managed identity enabled)

### Subscription ID Configuration

The controller tries multiple sources for subscription ID:
1. Environment variable: `AZURE_SUBSCRIPTION_ID`
2. Configuration: `AZURE_SUBSCRIPTION_ID` or `Azure:SubscriptionId`
3. Azure Metadata Service (automatic in Azure App Service)

**For local development:** Set `AZURE_SUBSCRIPTION_ID` in `appsettings.Development.local.json`:
```json
{
  "AZURE_SUBSCRIPTION_ID": "68bf6cf1-dc03-413f-89d7-9828f182b09d"
}
```

**For Azure deployment:** The metadata service will automatically provide the subscription ID.

## Customization

### Change the Default Query

Edit `Demo.razor`, find the `RefreshInsights()` method, and modify the `queryRequest`:

```csharp
var queryRequest = new
{
    Query = @"
customEvents
| where timestamp > ago(1h)
| where name == 'BookingCreated'
| project timestamp, BookingId = tostring(customDimensions['BookingId']), SystemType = tostring(customDimensions['SystemType'])
| order by timestamp desc
| take 10",
    Timespan = "PT1H"
};
```

### Add More Queries

You can add multiple query sections by:
1. Adding more query state variables
2. Creating additional refresh methods
3. Adding more sections to the UI

### Change Auto-Refresh Interval

Modify the delay in `StartAutoRefresh()`:
```csharp
await Task.Delay(10000, cancellationToken); // Change 10000 to desired milliseconds
```

## Troubleshooting

### "Azure subscription ID not configured" Error

**Solution:** Add subscription ID to configuration:
- Local: `appsettings.Development.local.json`
- Azure: App Service Configuration → Application Settings → `AZURE_SUBSCRIPTION_ID`

### "Query failed: 403 Forbidden" Error

**Solution:** The App Service managed identity needs "Reader" role on Application Insights:

1. Get the App Service principal ID:
```bash
az webapp identity show \
  --name examwork-web-dev \
  --resource-group rg-examwork-dev \
  --query principalId -o tsv
```

2. Grant the Reader role:
```bash
az role assignment create \
  --assignee <app-service-principal-id> \
  --role "Reader" \
  --scope /subscriptions/68bf6cf1-dc03-413f-89d7-9828f182b09d/resourceGroups/rg-examwork-dev/providers/Microsoft.Insights/components/examwork-insights-dev
```

**Note:** After granting permissions, wait 1-2 minutes for propagation. If the error persists, restart the App Service to refresh the managed identity token:
```bash
az webapp restart --name examwork-web-dev --resource-group rg-examwork-dev
```

### No Events Showing

- **Wait 2-5 minutes:** Application Insights has ingestion delay
- **Check time range:** Default is `ago(1h)` - try `ago(24h)` for older events
- **Verify events exist:** Check Application Insights Logs directly in Azure Portal

### Auto-Refresh Not Working

- Check browser console for errors
- Verify the page hasn't been disposed
- Check network tab for API call failures

## Demo Workflow

1. **Open demo page:** Navigate to `/demo` (or click button from Admin Dashboard)
2. **View current events:** Application Insights section shows recent custom events
3. **Toggle feature flag:** Use toggle button to switch between Synchronous and Event-Driven modes
4. **Create booking:** Use booking form to create a test booking
5. **Watch events update:** 
   - Insights auto-refresh after booking creation
   - New events appear within 2-5 minutes
   - Event counts update in real-time (if auto-refresh enabled)
6. **Compare modes:** Toggle feature flag, create bookings in each mode, see different event flows

## Benefits

✅ **Single page** for complete demo  
✅ **Live data** from Application Insights  
✅ **Interactive** - toggle feature flags and create bookings  
✅ **Real-time updates** with auto-refresh  
✅ **No context switching** - everything in one view  
✅ **Perfect for presentations** - show architecture changes live

