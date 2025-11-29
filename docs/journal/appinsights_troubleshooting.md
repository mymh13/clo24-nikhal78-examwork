# Application Insights Custom Events Troubleshooting

## Issue: Custom events not appearing in Application Insights

### Step 1: Verify Connection String is Set

**Check App Service Configuration:**
```bash
az webapp config appsettings list \
  --name examwork-web-dev \
  --resource-group rg-examwork-dev \
  --query "[?name=='APPLICATIONINSIGHTS_CONNECTION_STRING']" \
  -o table
```

**Expected:** Should show the connection string value.

**If missing:** The connection string needs to be set. It's configured via Bicep deployment, but can also be set manually:
```bash
az webapp config appsettings set \
  --name examwork-web-dev \
  --resource-group rg-examwork-dev \
  --settings APPLICATIONINSIGHTS_CONNECTION_STRING="<connection-string>"
```

### Step 2: Verify Events Are Being Tracked (Check Application Logs)

**Check App Service Logs:**
```bash
az webapp log tail \
  --name examwork-web-dev \
  --resource-group rg-examwork-dev
```

**Look for:**
- `"Tracked BookingCreated event: {BookingId}, Mode: {ArchitectureMode}"`
- `"Tracked OutboxEventCreated: {OutboxEventId}, Booking: {BookingId}"`

**If you see these logs:** Events are being tracked, but might not be reaching Application Insights.

**If you DON'T see these logs:** Events aren't being tracked - check if `ITelemetryService` is being called.

### Step 3: Check Application Insights Ingestion Delay

**Application Insights has ingestion delays:**
- **Normal delay:** 2-5 minutes
- **Peak times:** Up to 10 minutes
- **First events:** Can take longer on cold start

**Wait at least 5 minutes** after creating a booking before querying Application Insights.

### Step 4: Verify TelemetryClient is Initialized

**Check Health Endpoint:**
Navigate to: `https://ticket.mymh.dev/health`

**Look for:**
- `ApplicationInsightsConfigured: true`

**If false:** Connection string is not being read correctly.

### Step 5: Test with Simple Query

**In Application Insights Logs, try this query:**
```kusto
traces
| where timestamp > ago(1h)
| where message contains "Tracked BookingCreated"
| project timestamp, message
| order by timestamp desc
```

**If this returns results:** Events are being tracked, but custom events might not be showing up. Try:
```kusto
customEvents
| where timestamp > ago(1h)
| take 10
```

### Step 6: Check for Errors

**Query Application Insights for errors:**
```kusto
exceptions
| where timestamp > ago(1h)
| where outerMessage contains "telemetry" or outerMessage contains "Application Insights"
| project timestamp, outerMessage, type
| order by timestamp desc
```

### Step 7: Verify TelemetryClient Configuration

**Check if TelemetryClient is properly configured:**
- Connection string should be set in `APPLICATIONINSIGHTS_CONNECTION_STRING` environment variable
- `AddApplicationInsightsTelemetry()` should be called in `ServiceCollectionExtensions.cs`
- `ITelemetryService` should be registered and injected

### Step 8: Force Flush (Already Implemented)

The code now calls `_telemetryClient.Flush()` after tracking events to ensure they're sent immediately. This is especially important for demos.

### Step 9: Check Time Range in Queries

**Common mistake:** Using too short time range.

**Try:**
- `ago(1h)` instead of `ago(10m)`
- `ago(24h)` to see older events
- `ago(7d)` to see all recent events

### Step 10: Verify Event Names

**Custom events are tracked with these names:**
- `BookingCreated`
- `OutboxEventCreated`
- `OutboxEventProcessed`
- `ServiceBusEventPublished`
- `FeatureFlagToggled`
- `ModeSwitch`
- `FunctionBookingCreatedProcessed` (from Function App)

**Query to see all custom events:**
```kusto
customEvents
| where timestamp > ago(24h)
| summarize Count = count() by name
| order by Count desc
```

## Quick Diagnostic Checklist

- [ ] Connection string is set in App Service configuration
- [ ] Health endpoint shows `ApplicationInsightsConfigured: true`
- [ ] Application logs show "Tracked BookingCreated event" messages
- [ ] Waited at least 5 minutes after creating booking
- [ ] Using time range `ago(1h)` or longer in queries
- [ ] Querying `customEvents` table (not `traces`)
- [ ] No errors in Application Insights exceptions table

## Still Not Working?

1. **Restart App Service:**
   ```bash
   az webapp restart --name examwork-web-dev --resource-group rg-examwork-dev
   ```

2. **Create a test booking** and wait 5-10 minutes

3. **Check Application Insights → Live Metrics** to see if events are being received in real-time

4. **Verify the connection string format** is correct (should start with `InstrumentationKey=`)

5. **Check if Application Insights resource is active** and not in a disabled state

