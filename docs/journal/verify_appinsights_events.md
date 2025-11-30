# Verify Application Insights Custom Events

## Your Booking Details
- **Booking ID:** `07577a62-cb42-41b4-9740-be708cd8bf41`
- **Created:** 2025-11-29 23:03:54 (23:04:12 UTC)
- **Mode:** Event-Driven
- **Outbox Event ID:** `968cde2b-e8da-4c0d-9110-eaced8addcd5`

## Step 1: Wait for Ingestion (IMPORTANT!)

**Application Insights has a 2-5 minute ingestion delay.** Since your booking was created at 23:04:12 UTC, wait until at least **23:09 UTC** (5 minutes later) before querying.

## Step 2: Verify Events Exist (Simple Query)

**In Application Insights Logs, run this query:**
```kusto
customEvents
| where timestamp > ago(1h)
| summarize Count = count() by name
| order by Count desc
```

**Expected Results:**
- `BookingCreated` - should show at least 1
- `OutboxEventCreated` - should show at least 1
- `FeatureFlagToggled` - should show multiple (from your toggles)
- `ModeSwitch` - should show multiple (from your toggles)

**If you see these events:** Great! Events are being tracked. Proceed to Step 3.

**If you see NO events:** Wait another 5 minutes and try again. If still nothing after 10 minutes, check the troubleshooting guide.

## Step 3: Find Your Specific Booking Event

**Query to find your booking:**
```kusto
customEvents
| where name == "BookingCreated"
| where timestamp > ago(1h)
| where tostring(customDimensions["BookingId"]) == "07577a62-cb42-41b4-9740-be708cd8bf41"
| project timestamp, name, BookingId = tostring(customDimensions["BookingId"]), SystemType = tostring(customDimensions["SystemType"]), ArchitectureMode = tostring(customDimensions["ArchitectureMode"])
```

**Expected Result:**
- Should show 1 event with:
  - `BookingId: 07577a62-cb42-41b4-9740-be708cd8bf41`
  - `SystemType: Event-Driven`
  - `ArchitectureMode: Event-Driven`

## Step 4: Verify All Event Types

**Query to see all events from your session:**
```kusto
customEvents
| where timestamp > ago(1h)
| where tostring(customDimensions["BookingId"]) == "07577a62-cb42-41b4-9740-be708cd8bf41"
   or tostring(customDimensions["OutboxEventId"]) == "968cde2b-e8da-4c0d-9110-eaced8addcd5"
| project timestamp, EventName = name, BookingId = tostring(customDimensions["BookingId"]), OutboxEventId = tostring(customDimensions["OutboxEventId"]), SystemType = tostring(customDimensions["SystemType"])
| order by timestamp asc
```

**Expected Results:**
1. `BookingCreated` - with BookingId
2. `OutboxEventCreated` - with OutboxEventId and BookingId
3. Possibly `OutboxEventProcessed` - if it was processed (should be, since status is "Processed")
4. Possibly `ServiceBusEventPublished` - if event-driven mode published to Service Bus

## Step 5: Check Feature Flag Toggle Events

**Query to see your feature flag toggles:**
```kusto
customEvents
| where name in ("FeatureFlagToggled", "ModeSwitch")
| where timestamp > ago(1h)
| project timestamp, EventName = name, FromMode = tostring(customDimensions["FromMode"]), ToMode = tostring(customDimensions["ToMode"]), UserId = tostring(customDimensions["UserId"])
| order by timestamp desc
```

**Expected Results:**
- Should show your toggles from around 23:03 UTC
- `FromMode: Event-Driven → ToMode: Synchronous` (first toggle)
- `FromMode: Synchronous → ToMode: Event-Driven` (second toggle)

## Troubleshooting

**If events still don't appear after 10 minutes:**

1. **Check if ANY custom events exist:**
   ```kusto
   customEvents
   | where timestamp > ago(24h)
   | take 10
   ```
   If this returns nothing, Application Insights might not be receiving events.

2. **Check for errors:**
   ```kusto
   exceptions
   | where timestamp > ago(1h)
   | where outerMessage contains "telemetry" or outerMessage contains "Application Insights"
   | project timestamp, outerMessage
   ```

3. **Verify connection string is correct:**
   - Check App Service configuration
   - Verify health endpoint shows `ApplicationInsightsConfigured: true`

4. **Check Live Metrics:**
   - Go to Application Insights → Live Metrics
   - Create a new booking
   - See if events appear in real-time (they should, even if queries are delayed)

## Time Ranges to Try

If `ago(1h)` doesn't work, try:
- `ago(2h)` - to catch events from an hour ago
- `ago(24h)` - to see all recent events
- Specific time range: `where timestamp > datetime(2025-11-29T23:00:00Z)`

## Expected Timeline

Based on your logs:
- **23:03:05 UTC** - Feature flag toggled (Event-Driven → Synchronous)
- **23:03:33 UTC** - Feature flag toggled (Synchronous → Event-Driven)
- **23:04:12 UTC** - Booking created
- **23:04:12 UTC** - Outbox event created
- **23:04:20 UTC** - Outbox event processed

All these should appear in Application Insights within 5-10 minutes of their timestamps.

