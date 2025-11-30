# Phase 8: Application Insights Dashboard Setup Guide

## Overview

This guide helps you create an Application Insights dashboard that clearly visualizes the difference between **Synchronous** and **Event-Driven** architectures when toggling the feature flag. Perfect for live demonstrations side-by-side with the Admin Dashboard.

## Custom Events Tracked

The application now tracks the following custom events with clear `SystemType` and `ArchitectureMode` properties:

### Web App Events:
1. **`BookingCreated`** - When a booking is created
   - Properties: `BookingId`, `CustomerEmail`, `ArchitectureMode`, `EventDrivenEnabled`, `SystemType`
   - `SystemType`: "Synchronous" or "Event-Driven"

2. **`OutboxEventCreated`** - When an outbox event is created
   - Properties: `OutboxEventId`, `BookingId`, `EventType`, `ArchitectureMode`, `Status`
   - Always created (for audit), regardless of mode

3. **`OutboxEventProcessed`** - When an outbox event is processed
   - Properties: `OutboxEventId`, `EventType`, `Status`, `SystemType`
   - Metric: `ProcessingTimeMs`
   - Only occurs in Event-Driven mode

4. **`ServiceBusEventPublished`** - When an event is published to Service Bus
   - Properties: `EventId`, `EventType`, `QueueName`, `SystemType`
   - Only occurs in Event-Driven mode

5. **`FeatureFlagToggled`** - When feature flag is toggled
   - Properties: `FeatureFlag`, `PreviousValue`, `NewValue`, `FromMode`, `ToMode`, `UserId`

6. **`ModeSwitch`** - When architecture mode switches
   - Properties: `FromMode`, `ToMode`, `UserId`, `SystemType`

### Function App Events:
7. **`FunctionBookingCreatedProcessed`** - When Function App processes a booking event
   - Properties: `BookingId`, `EventId`, `CustomerEmail`, `EventType`, `SystemType`, `FunctionName`, `DeliveryCount`
   - Metric: `ProcessingTimeMs`
   - Only occurs in Event-Driven mode

## Dashboard Setup Steps

### Step 1: Create New Dashboard

1. Navigate to Azure Portal → Application Insights → `examwork-insights-dev`
2. Click the **"Dashboards"** tab at the top (in the navigation tabs, next to "Application Dashboard", "Getting started", "Search", "Logs", etc.)
3. Click the **"Create"** button (top right of the dashboard view, next to "Upload", "Refresh", etc.)
4. You'll see a grid of dashboard templates. Select **"Custom"** template:
   - **Location:** First tile in the top-left (shows a grid with a blue plus sign)
   - **Description:** "Create a custom dashboard"
   - **Why Custom:** Gives you a blank canvas to add only the charts you need for the demo
   - **Alternative:** You can use "Application Insights" template if you want a pre-configured dashboard, but you'll need to remove/modify existing charts
5. The dashboard editor will open with an empty grid
6. **Name the dashboard:** Click on "Untitled dashboard" at the top and rename it to **"Event-Driven Architecture Demo"**

### Step 2: Verify Events Are Being Tracked (Diagnostic Query)

**First, let's verify that custom events are being tracked:**

1. **Navigate to Logs:** In Application Insights (`examwork-insights-dev`), click the **"Logs"** tab at the top
2. **Switch to KQL mode:** In the query editor, look for a dropdown in the top-right corner that says "Simple" - change it to **"KQL"** mode
3. **Run this diagnostic query to see all custom events:**
```kusto
customEvents
| where timestamp > ago(24h)
| summarize Count = count() by name
| order by Count desc
```
4. **Expected results:** You should see events like:
   - `BookingCreated`
   - `OutboxEventCreated`
   - `OutboxEventProcessed` (if event-driven mode was used)
   - `ServiceBusEventPublished` (if event-driven mode was used)
   - `FeatureFlagToggled`
   - `ModeSwitch`

**If you see no results:**
- Events might not have been tracked yet - create a test booking first
- Check if Application Insights connection string is configured correctly
- Verify `ITelemetryService` is being called in `BookingsController`

**If you see events but queries below fail:**
- Try increasing the time range from `ago(1h)` to `ago(24h)` or `ago(7d)`
- Check the exact property names in `customDimensions` by running: `customEvents | where name == "BookingCreated" | take 1 | project customDimensions`

### Step 3: Add Architecture Mode Comparison Chart

**Chart 1: Bookings by Architecture Mode**

1. **In the Logs tab**, write the query:
```kusto
customEvents
| where name == "BookingCreated"
| where timestamp > ago(24h)
| summarize Count = count() by SystemType = tostring(customDimensions["SystemType"])
| render piechart
```
2. **Run the query:** Click **"Run"** button
3. **If you get results, pin to dashboard:**
   - Click the **"Pin"** icon at the top of the query results
   - Select your dashboard: **"Event-Driven Architecture Demo"**
   - **Title:** "Bookings by Architecture Mode"
   - **Visualization:** Select "Pie chart" from the pin dialog
   - Click **"Pin"** to confirm
4. The chart will appear on your dashboard - you can resize and reposition it by dragging

**Note:** If no results, try `ago(7d)` instead of `ago(24h)` to see older events

**Chart 2: Bookings Over Time (Last 1 Hour)**

1. Click **"+ Add"** → **"Query"** (or **"Logs query"**)
2. **Title:** "Bookings Over Time - Synchronous vs Event-Driven"
3. **Query:**
```kusto
customEvents
| where name == "BookingCreated"
| where timestamp > ago(1h)
| summarize Count = count() by bin(timestamp, 1m), SystemType = tostring(customDimensions.SystemType)
| render timechart
```
4. **Visualization:** Time chart
5. **Time range:** Last 1 hour

### Step 4: Add Event-Driven Flow Visualization

**Chart 3: Event-Driven Flow Events**

1. **In the Logs tab**, write the query:
```kusto
customEvents
| where name in ("OutboxEventProcessed", "ServiceBusEventPublished", "FunctionBookingCreatedProcessed")
| where timestamp > ago(24h)
| summarize Count = count() by EventName = name, bin(timestamp, 1m)
| render timechart
```
2. **Run the query** and click **"Pin"** icon
3. **Pin settings:**
   - Dashboard: "Event-Driven Architecture Demo"
   - **Title:** "Event-Driven Flow: Outbox → Service Bus → Function"
   - **Visualization:** "Time chart"
   - Click **"Pin"**

**Note:** This will only show data if event-driven mode was used. If no results, ensure feature flag was enabled and bookings were created in event-driven mode.

**Chart 4: Event Processing Time**

1. **In the Logs tab**, write the query:
```kusto
customEvents
| where name == "OutboxEventProcessed"
| where timestamp > ago(24h)
| extend ProcessingTime = todouble(customMetrics["ProcessingTimeMs"])
| where isnotnull(ProcessingTime)
| summarize AvgProcessingTime = avg(ProcessingTime), MaxProcessingTime = max(ProcessingTime), MinProcessingTime = min(ProcessingTime) by bin(timestamp, 5m)
| render timechart
```
2. **Run the query** and click **"Pin"** icon
3. **Pin settings:**
   - Dashboard: "Event-Driven Architecture Demo"
   - **Title:** "Event Processing Time (Event-Driven Mode)"
   - **Visualization:** "Time chart"
   - Click **"Pin"**

**Note:** If you get an error about `customMetrics`, try this alternative query that uses the property from customDimensions:
```kusto
customEvents
| where name == "OutboxEventProcessed"
| where timestamp > ago(24h)
| summarize Count = count() by bin(timestamp, 5m)
| render timechart
```

### Step 5: Add Mode Switch Tracking

**Chart 5: Mode Switches**

1. **In the Logs tab**, write the query:
```kusto
customEvents
| where name == "ModeSwitch"
| where timestamp > ago(7d)
| project timestamp, FromMode = tostring(customDimensions["FromMode"]), ToMode = tostring(customDimensions["ToMode"]), UserId = tostring(customDimensions["UserId"])
| order by timestamp desc
```
2. **Run the query** and click **"Pin"** icon
3. **Pin settings:**
   - Dashboard: "Event-Driven Architecture Demo"
   - **Title:** "Architecture Mode Switches"
   - **Visualization:** "Table"
   - Click **"Pin"**

**Note:** Mode switches only occur when the feature flag is toggled. If no results, try `ago(30d)` or toggle the feature flag to generate a test event.

### Step 6: Add Synchronous Mode Indicator

**Chart 6: Synchronous Mode Bookings (No Event Processing)**

1. **In the Logs tab**, write the query:
```kusto
customEvents
| where name == "BookingCreated"
| where timestamp > ago(24h)
| where tostring(customDimensions["SystemType"]) == "Synchronous"
| summarize Count = count() by bin(timestamp, 1m)
| render timechart
```
2. **Run the query** and click **"Pin"** icon
3. **Pin settings:**
   - Dashboard: "Event-Driven Architecture Demo"
   - **Title:** "Synchronous Mode: Bookings (No Event Processing)"
   - **Visualization:** "Time chart"
   - Click **"Pin"**

**Chart 7: Event-Driven Mode: Complete Flow**

1. **In the Logs tab**, write the query:
```kusto
customEvents
| where name in ("BookingCreated", "OutboxEventCreated", "OutboxEventProcessed", "ServiceBusEventPublished", "FunctionBookingCreatedProcessed")
| where timestamp > ago(24h)
| where tostring(customDimensions["SystemType"]) == "Event-Driven" or name == "OutboxEventCreated"
| summarize Count = count() by EventName = name, bin(timestamp, 1m)
| render timechart
```
2. **Run the query** and click **"Pin"** icon
3. **Pin settings:**
   - Dashboard: "Event-Driven Architecture Demo"
   - **Title:** "Event-Driven Mode: Complete Flow"
   - **Visualization:** "Time chart"
   - Click **"Pin"**

### Step 7: Add Real-Time Comparison Table

**Chart 8: Recent Activity Comparison**

1. **In the Logs tab**, write the query:
```kusto
customEvents
| where name == "BookingCreated"
| where timestamp > ago(24h)
| project timestamp, BookingId = tostring(customDimensions["BookingId"]), CustomerEmail = tostring(customDimensions["CustomerEmail"]), SystemType = tostring(customDimensions["SystemType"]), ArchitectureMode = tostring(customDimensions["ArchitectureMode"])
| order by timestamp desc
| take 20
```
2. **Run the query** and click **"Pin"** icon
3. **Pin settings:**
   - Dashboard: "Event-Driven Architecture Demo"
   - **Title:** "Recent Activity: Synchronous vs Event-Driven"
   - **Visualization:** "Table"
   - Click **"Pin"**

### Step 8: Alternative Method - Add Queries Directly to Dashboard

**If the Pin button is not available in Logs, you can add queries directly to the dashboard:**

1. **Navigate to your dashboard:** Click the **"Dashboards"** tab → Open **"Event-Driven Architecture Demo"**
2. **Click "Edit"** button (top right of the dashboard)
3. **Click "+ Add"** button
4. **Look for these tile types:**
   - **"Logs query"** or **"Query"** - This allows you to enter KQL queries directly
   - **"Application Insights query"** - Similar option
   - **"Markdown"** - For text/labels between charts
5. **If you find "Logs query" or "Query" tile:**
   - Select it
   - **Title:** "Bookings by Architecture Mode"
   - **Query:** Paste your KQL query:
     ```kusto
     customEvents
     | where name == "BookingCreated"
     | where timestamp > ago(24h)
     | summarize Count = count() by SystemType = tostring(customDimensions["SystemType"])
     | render piechart
     ```
   - **Resource:** Select `examwork-insights-dev` Application Insights resource
   - **Visualization:** Select "Pie chart"
   - Click **"Done"** or **"Apply"**

**If "Logs query" tile is NOT available either, use Workbooks (see Step 9):**

### Step 9: Alternative Method - Use Workbooks

**If dashboard tiles don't support queries, use Application Insights Workbooks:**

1. **Navigate to Application Insights** → `examwork-insights-dev`
2. Click **"Workbooks"** in the left menu (or search for it)
3. Click **"+ New"** or **"Create"**
4. **Add a query step:**
   - Click **"+ Add"** → **"Add query"**
   - Paste your KQL query
   - Select visualization type
5. **Save the workbook** for future use
6. **Pin the workbook** to your dashboard (if pin option is available)

### Step 10: (Optional) Add Application Map

**Note:** The user mentioned seeing "Application map" in the tile options - this is a great visual for showing the architecture flow!

1. **In the dashboard editor**, click **"+ Add"** button
2. Select **"Application map"** from the tile options
3. **Configure:**
   - Select Application Insights resource: `examwork-insights-dev`
   - This will show a visual map of your application components and their dependencies
   - Great for demonstrating the event-driven flow visually!
4. Click **"Done"** to add it to the dashboard

### Step 11: Save and View Dashboard

1. **Navigate to your dashboard:** Click the **"Dashboards"** tab → Open **"Event-Driven Architecture Demo"**
2. **Arrange charts:** Drag and resize charts to organize them as needed
3. **Save:** Click **"Done editing"** or **"Save"** button (if in edit mode)
4. **Optional:** Click **"Share"** or **"Manage sharing"** to share the dashboard with others

## Alternative: Use Workbooks Instead of Dashboard

**If pinning queries to dashboards is not working, use Application Insights Workbooks:**

1. **Navigate to Application Insights** → `examwork-insights-dev`
2. Click **"Workbooks"** in the left menu
3. Click **"+ New"** or **"Create"**
4. **Add queries:**
   - Click **"+ Add"** → **"Add query"** for each chart you want
   - Paste your KQL queries
   - Configure visualizations
5. **Save the workbook** as "Event-Driven Architecture Demo"
6. **Access it anytime** from Workbooks section

**Workbooks are often more reliable for Application Insights queries than dashboard pinning.**

See `docs/journal/dashboard_pin_alternatives.md` for detailed alternatives.

## Demonstration Workflow

### Before Demo:
1. Navigate to Azure Portal → Application Insights → `examwork-insights-dev`
2. Click the **"Dashboards"** tab
3. Open your **"Event-Driven Architecture Demo"** dashboard
4. Open Admin Dashboard (`https://ticket.mymh.dev/admin`) in another browser tab
5. Position windows side-by-side for live comparison
6. **Note:** Pinned queries from Logs will use the time range set when you pinned them. To update:
   - Click on a chart tile
   - It will open the query in Logs
   - Adjust time range and re-pin if needed

### During Demo:

**Step 1: Show Synchronous Mode**
1. Ensure feature flag is **disabled** (Synchronous mode)
2. Create a booking
3. **Dashboard shows:**
   - `BookingCreated` event with `SystemType: "Synchronous"`
   - `OutboxEventCreated` event (for audit)
   - **NO** `OutboxEventProcessed`, `ServiceBusEventPublished`, or `FunctionBookingCreatedProcessed` events

**Step 2: Toggle to Event-Driven Mode**
1. Click "Toggle Feature Flag" in Admin Dashboard
2. Wait for "Change applied!" message
3. **Dashboard shows:**
   - `FeatureFlagToggled` event
   - `ModeSwitch` event: `FromMode: "Synchronous"`, `ToMode: "Event-Driven"`

**Step 3: Show Event-Driven Mode**
1. Create a booking with feature flag **enabled**
2. **Dashboard shows:**
   - `BookingCreated` event with `SystemType: "Event-Driven"`
   - `OutboxEventCreated` event
   - **Within 30 seconds:**
     - `OutboxEventProcessed` event
     - `ServiceBusEventPublished` event
     - `FunctionBookingCreatedProcessed` event
   - Complete event flow visible in real-time

**Step 4: Compare Side-by-Side**
- **Synchronous mode:** Only `BookingCreated` and `OutboxEventCreated` (no processing)
- **Event-Driven mode:** Full flow: `BookingCreated` → `OutboxEventCreated` → `OutboxEventProcessed` → `ServiceBusEventPublished` → `FunctionBookingCreatedProcessed`

## Key Queries for Live Demo

### Quick Comparison Query:
```kusto
customEvents
| where name in ("BookingCreated", "OutboxEventProcessed", "ServiceBusEventPublished", "FunctionBookingCreatedProcessed")
| where timestamp > ago(24h)
| summarize 
    SynchronousBookings = countif(tostring(customDimensions["SystemType"]) == "Synchronous"),
    EventDrivenBookings = countif(tostring(customDimensions["SystemType"]) == "Event-Driven"),
    EventsProcessed = countif(name == "OutboxEventProcessed"),
    ServiceBusPublished = countif(name == "ServiceBusEventPublished"),
    FunctionsProcessed = countif(name == "FunctionBookingCreatedProcessed")
| project SynchronousBookings, EventDrivenBookings, EventsProcessed, ServiceBusPublished, FunctionsProcessed
```

### Real-Time Event Flow:
```kusto
customEvents
| where timestamp > ago(24h)
| where name in ("BookingCreated", "OutboxEventCreated", "OutboxEventProcessed", "ServiceBusEventPublished", "FunctionBookingCreatedProcessed", "ModeSwitch")
| project timestamp, EventName = name, SystemType = tostring(customDimensions["SystemType"]), Details = strcat(tostring(customDimensions))
| order by timestamp desc
```

## Tips for Live Demo

1. **Refresh Dashboard:** Click refresh button in Application Insights to see latest events
2. **Time Range:** Use "Last 10 minutes" or "Last 1 hour" for live demos
3. **Auto-Refresh:** Enable auto-refresh in dashboard settings (every 30 seconds)
4. **Highlight Differences:** 
   - Synchronous: Only 2 events (BookingCreated, OutboxEventCreated)
   - Event-Driven: 5 events (complete flow)
5. **Show Processing Time:** Use `ProcessingTimeMs` metric to show event processing latency

## Expected Results

**Synchronous Mode:**
- ✓ `BookingCreated` with `SystemType: "Synchronous"`
- ✓ `OutboxEventCreated` (audit trail)
- ✗ No `OutboxEventProcessed`
- ✗ No `ServiceBusEventPublished`
- ✗ No `FunctionBookingCreatedProcessed`

**Event-Driven Mode:**
- ✓ `BookingCreated` with `SystemType: "Event-Driven"`
- ✓ `OutboxEventCreated` (audit trail)
- ✓ `OutboxEventProcessed` (within 30 seconds)
- ✓ `ServiceBusEventPublished` (immediately after outbox processed)
- ✓ `FunctionBookingCreatedProcessed` (within seconds of Service Bus)

## Troubleshooting

### No Results Found

**If queries return "No results found":**

1. **First, verify events exist:**
   ```kusto
   customEvents
   | where timestamp > ago(7d)
   | summarize Count = count() by name
   | order by Count desc
   ```
   - If this returns no results, events are not being tracked
   - Check Application Insights connection string is configured in `appsettings.json` or App Configuration
   - Verify `ITelemetryService` is registered in `ServiceCollectionExtensions.cs`
   - Create a test booking to generate events

2. **If events exist but queries fail:**
   - **Increase time range:** Change `ago(1h)` to `ago(24h)` or `ago(7d)`
   - **Check property names:** Run this to see actual property names:
     ```kusto
     customEvents
     | where name == "BookingCreated"
     | take 1
     | project customDimensions
     ```
   - **Use bracket notation:** Use `customDimensions["SystemType"]` instead of `customDimensions.SystemType`

3. **If `customMetrics` fails:**
   - Metrics might not be stored as expected
   - Use the alternative query provided in Chart 4 that counts events instead
   - Or check if metrics are stored differently:
     ```kusto
     customEvents
     | where name == "OutboxEventProcessed"
     | take 1
     | project customMetrics
     ```

### Property Access Issues

**If you get errors accessing properties:**
- Use bracket notation: `customDimensions["PropertyName"]` instead of `customDimensions.PropertyName`
- Check if property exists first: `where isnotnull(customDimensions["SystemType"])`
- Use `tostring()` to convert values: `tostring(customDimensions["SystemType"])`

### Time Range Issues

**If events are old:**
- Increase time range: `ago(7d)` or `ago(30d)`
- Check when last booking was created
- Create a new test booking to generate fresh events

### Verification Steps

1. **Check if telemetry is configured:**
   - Verify `ApplicationInsights:ConnectionString` in configuration
   - Check `AddApplicationInsightsTelemetry()` is called in `Program.cs` or `ServiceCollectionExtensions.cs`

2. **Verify events are being tracked:**
   - Check `BookingsController.cs` - ensure `_telemetryService.TrackBookingCreated()` is called
   - Check logs for telemetry errors
   - Create a test booking and immediately check Application Insights

3. **Test with a simple query:**
   ```kusto
   customEvents
   | where timestamp > ago(1h)
   | take 10
   ```
   If this returns results, events are being tracked - the issue is with specific queries.

