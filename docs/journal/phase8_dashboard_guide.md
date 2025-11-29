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

1. Navigate to Azure Portal → Application Insights → `examwork-appinsights-dev`
2. Click **"Dashboards"** in the left menu
3. Click **"+ New dashboard"**
4. Name it: **"Event-Driven Architecture Demo"**

### Step 2: Add Architecture Mode Comparison Chart

**Chart 1: Bookings by Architecture Mode (Last 1 Hour)**

1. Click **"+ Add"** → **"Query"**
2. **Title:** "Bookings by Architecture Mode"
3. **Query:**
```kusto
customEvents
| where name == "BookingCreated"
| where timestamp > ago(1h)
| summarize Count = count() by SystemType = tostring(customDimensions.SystemType)
| render piechart
```
4. **Visualization:** Pie chart
5. **Time range:** Last 1 hour

**Chart 2: Bookings Over Time (Last 1 Hour)**

1. Click **"+ Add"** → **"Query"**
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

### Step 3: Add Event-Driven Flow Visualization

**Chart 3: Event-Driven Flow Events**

1. Click **"+ Add"** → **"Query"**
2. **Title:** "Event-Driven Flow: Outbox → Service Bus → Function"
3. **Query:**
```kusto
customEvents
| where name in ("OutboxEventProcessed", "ServiceBusEventPublished", "FunctionBookingCreatedProcessed")
| where timestamp > ago(1h)
| summarize Count = count() by EventName = name, bin(timestamp, 1m)
| render timechart
```
4. **Visualization:** Time chart
5. **Time range:** Last 1 hour

**Chart 4: Event Processing Time**

1. Click **"+ Add"** → **"Query"**
2. **Title:** "Event Processing Time (Event-Driven Mode)"
3. **Query:**
```kusto
customEvents
| where name == "OutboxEventProcessed"
| where timestamp > ago(1h)
| extend ProcessingTime = todouble(customMetrics.ProcessingTimeMs)
| summarize AvgProcessingTime = avg(ProcessingTime), MaxProcessingTime = max(ProcessingTime), MinProcessingTime = min(ProcessingTime) by bin(timestamp, 5m)
| render timechart
```
4. **Visualization:** Time chart
5. **Time range:** Last 1 hour

### Step 4: Add Mode Switch Tracking

**Chart 5: Mode Switches**

1. Click **"+ Add"** → **"Query"**
2. **Title:** "Architecture Mode Switches"
3. **Query:**
```kusto
customEvents
| where name == "ModeSwitch"
| where timestamp > ago(24h)
| project timestamp, FromMode = tostring(customDimensions.FromMode), ToMode = tostring(customDimensions.ToMode), UserId = tostring(customDimensions.UserId)
| order by timestamp desc
```
4. **Visualization:** Table
5. **Time range:** Last 24 hours

### Step 5: Add Synchronous Mode Indicator

**Chart 6: Synchronous Mode Bookings (No Event Processing)**

1. Click **"+ Add"** → **"Query"**
2. **Title:** "Synchronous Mode: Bookings (No Event Processing)"
3. **Query:**
```kusto
customEvents
| where name == "BookingCreated"
| where timestamp > ago(1h)
| where tostring(customDimensions.SystemType) == "Synchronous"
| summarize Count = count() by bin(timestamp, 1m)
| render timechart
```
4. **Visualization:** Time chart
5. **Time range:** Last 1 hour

**Chart 7: Event-Driven Mode: Complete Flow**

1. Click **"+ Add"** → **"Query"**
2. **Title:** "Event-Driven Mode: Complete Flow"
3. **Query:**
```kusto
customEvents
| where name in ("BookingCreated", "OutboxEventCreated", "OutboxEventProcessed", "ServiceBusEventPublished", "FunctionBookingCreatedProcessed")
| where timestamp > ago(1h)
| where tostring(customDimensions.SystemType) == "Event-Driven" or name == "OutboxEventCreated"
| summarize Count = count() by EventName = name, bin(timestamp, 1m)
| render timechart
```
4. **Visualization:** Time chart
5. **Time range:** Last 1 hour

### Step 6: Add Real-Time Comparison Table

**Chart 8: Recent Activity Comparison**

1. Click **"+ Add"** → **"Query"**
2. **Title:** "Recent Activity: Synchronous vs Event-Driven"
3. **Query:**
```kusto
customEvents
| where name == "BookingCreated"
| where timestamp > ago(30m)
| project timestamp, BookingId = tostring(customDimensions.BookingId), CustomerEmail = tostring(customDimensions.CustomerEmail), SystemType = tostring(customDimensions.SystemType), ArchitectureMode = tostring(customDimensions.ArchitectureMode)
| order by timestamp desc
| take 20
```
4. **Visualization:** Table
5. **Time range:** Last 30 minutes

## Demonstration Workflow

### Before Demo:
1. Open Application Insights dashboard in one browser tab
2. Open Admin Dashboard (`/admin`) in another tab
3. Position windows side-by-side

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
| where timestamp > ago(10m)
| summarize 
    SynchronousBookings = countif(tostring(customDimensions.SystemType) == "Synchronous"),
    EventDrivenBookings = countif(tostring(customDimensions.SystemType) == "Event-Driven"),
    EventsProcessed = countif(name == "OutboxEventProcessed"),
    ServiceBusPublished = countif(name == "ServiceBusEventPublished"),
    FunctionsProcessed = countif(name == "FunctionBookingCreatedProcessed")
| project SynchronousBookings, EventDrivenBookings, EventsProcessed, ServiceBusPublished, FunctionsProcessed
```

### Real-Time Event Flow:
```kusto
customEvents
| where timestamp > ago(5m)
| where name in ("BookingCreated", "OutboxEventCreated", "OutboxEventProcessed", "ServiceBusEventPublished", "FunctionBookingCreatedProcessed", "ModeSwitch")
| project timestamp, EventName = name, SystemType = tostring(customDimensions.SystemType), Details = strcat(tostring(customDimensions))
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

**If events don't appear:**
- Check Application Insights connection string is configured
- Verify `ITelemetryService` is registered in DI
- Check time range in queries (may need to adjust)
- Verify events are being tracked (check Application Insights → Logs → customEvents)

**If SystemType is missing:**
- Verify custom events include `SystemType` property
- Check event properties in Application Insights → Logs

