# Phase 7.1 Testing Guide: Synchronous Flow (Feature Flag Disabled)

## Prerequisites

1. **Verify Feature Flag State:**
   - Navigate to `/health` page or `/api/health` endpoint
   - Confirm `FeatureFlagTest` shows: `BookingEvents_Enabled = False`
   - If flag is enabled, disable it in Azure App Configuration:
     ```bash
     az appconfig feature disable \
       --name examwork-appconfig-dev \
       --feature BookingEvents_Enabled \
       --yes
     
     # Update sentinel to trigger refresh (PowerShell)
     az appconfig kv set \
       --name examwork-appconfig-dev \
       --key Settings:Sentinel \
       --value "$([DateTimeOffset]::UtcNow.ToUnixTimeSeconds())" \
       --yes
     
     # Alternative for bash/Git Bash:
     # az appconfig kv set \
     #   --name examwork-appconfig-dev \
     #   --key Settings:Sentinel \
     #   --value "$(date +%s)" \
     #   --yes
     ```
   - Wait 1 minute for hot-reload

2. **Clear any existing test data (optional):**
   - Note current booking count
   - Note current outbox event count

## Test Steps

### Step 1: Create a Test Booking

1. **Via UI:**
   - Log in as a User
   - Navigate to User landing page
   - Create a booking with one or more zones
   - Note the booking ID from the success message

2. **Via API (alternative):**
   ```bash
   # Get auth token first, then:
   curl -X POST https://ticket.mymh.dev/api/bookings \
     -H "Content-Type: application/json" \
     -H "Cookie: [your-auth-cookie]" \
     -d '{
       "zone": "Zone A",
       "region": ""
     }'
   ```

### Step 2: Verify Booking Created Successfully

**Check Cosmos DB:**
```bash
# Query bookings container
az cosmosdb sql container query \
  --account-name examwork-cosmos-dev \
  --database-name ticketing \
  --name bookings \
  --query-text "SELECT * FROM c WHERE c.id = '<BOOKING_ID>' ORDER BY c._ts DESC"
```

**Or via Azure Portal:**
- Navigate to Cosmos DB → Data Explorer
- Select `ticketing` database → `bookings` container
- Query: `SELECT * FROM c WHERE c.id = '<BOOKING_ID>'`
- Verify booking exists with correct data

**Check via API:**
- Navigate to `/user` page (if logged in as User)
- Verify booking appears in "My Tickets" table
- Or call: `GET /api/bookings/my-bookings`

### Step 3: Verify Outbox Event Created

**Check Cosmos DB Outbox Container:**
```bash
# Query pending outbox events
az cosmosdb sql container query \
  --account-name examwork-cosmos-dev \
  --database-name ticketing \
  --name outbox \
  --query-text "SELECT * FROM c WHERE c.status = 'Pending' ORDER BY c.createdAt DESC"
```

**Or via Azure Portal:**
- Navigate to Cosmos DB → Data Explorer
- Select `ticketing` database → `outbox` container
- Query: `SELECT * FROM c WHERE c.status = 'Pending' ORDER BY c.createdAt DESC`
- Verify:
  - Event exists with `eventType: "BookingCreated"`
  - `status: "Pending"`
  - `eventData` contains booking JSON
  - `createdAt` timestamp matches booking creation time

**Verify Event Data Structure:**
- Open the outbox event document
- Check `eventData` field contains valid JSON
- Verify `eventData` includes: `bookingId`, `customerId`, `customerEmail`, `totalPrice`, etc.

### Step 4: Verify No Service Bus Messages

**Check Service Bus Queue:**
```bash
# Check queue message count
az servicebus queue show \
  --namespace-name examwork-sb-dev \
  --resource-group rg-examwork-dev \
  --name booking-events \
  --query "countDetails.activeMessageCount"
```

**Or via Azure Portal:**
- Navigate to Service Bus → Queues → `booking-events`
- Check "Active message count" - should be 0 (or unchanged)
- Check "Dead letter message count" - should be 0

**Check Application Insights:**
- Navigate to Application Insights → Logs
- **Note:** Query editor is in dropdown menu (top right) - switch from "Simple" to "KQL" mode
- Query:
  ```kusto
  traces
  | where message contains "Service Bus" or message contains "Event published"
  | where timestamp > ago(10m)
  | order by timestamp desc
  ```
- Should show NO Service Bus publishing messages

### Step 5: Verify Logging Shows Synchronous Mode

**Check Application Insights Logs:**
- **Note:** Query editor is in dropdown menu (top right) - switch from "Simple" to "KQL" mode
- **Important:** Application Insights query editor is hidden behind a dropdown in the top right corner of the query window
- Switch from "Simple" mode to "KQL" mode to access the query editor
- Query:
  ```kusto
  traces
  | where message contains "Booking created" or message contains "Architecture"
  | where timestamp > ago(10m)
  | project timestamp, message
  | order by timestamp desc
  ```

**Expected Log Messages:**
- `"Architecture: Synchronous"`
- `"Synchronous architecture - booking processed via chained API calls"`
- `"Outbox event created: ..."`
- **Should NOT see:** `"Event published to Service Bus"` or `"Event-driven architecture enabled"`

### Step 6: Verify Performance

**Check Response Times:**
- Note the time when booking creation request was sent
- Note the time when booking creation response was received
- Calculate response time (should be < 500ms typically)

**Check Application Insights Performance:**
- **Note:** Query editor is in dropdown menu (top right) - switch from "Simple" to "KQL" mode
```kusto
requests
| where name contains "POST" and name contains "bookings"
| where timestamp > ago(10m)
| project timestamp, duration, success
| order by timestamp desc
```

**Expected:**
- `duration` should be reasonable (< 500ms)
- `success` should be `true`
- No significant performance degradation compared to baseline

### Step 7: Verify OutboxProcessorService Behavior

**Check Application Insights:**
- **Note:** Query editor is in dropdown menu (top right) - switch from "Simple" to "KQL" mode
- Query:
  ```kusto
  traces
  | where message contains "Outbox Processor" or message contains "Event-driven mode disabled"
  | where timestamp > ago(10m)
  | project timestamp, message
  | order by timestamp desc
  ```

**Expected:**
- Should see: `"Event-driven mode disabled, skipping outbox processing"`
- Should NOT see: `"Successfully published and processed outbox event"`

## Validation Checklist

- [ ] Booking created successfully in Cosmos DB
- [ ] Booking retrievable via API/UI
- [ ] Outbox event created with `status: "Pending"`
- [ ] Outbox event contains correct booking data
- [ ] Service Bus queue message count is 0 (or unchanged)
- [ ] No Service Bus publishing log messages
- [ ] Logs show "Architecture: Synchronous"
- [ ] Logs show "Synchronous architecture - booking processed via chained API calls"
- [ ] Health endpoint shows `BookingEvents_Enabled = False`
- [ ] Response time is acceptable (< 500ms)
- [ ] OutboxProcessorService logs show "Event-driven mode disabled"

## Expected Results Summary

✓ **Bookings work exactly as before** - No breaking changes, all functionality intact  
✓ **Outbox events are created** - Events stored for audit and future activation  
✓ **No Service Bus messages sent** - Event-driven path not executed  
✓ **No performance impact** - Minimal overhead from feature flag check and outbox write  
✓ **Logging confirms synchronous mode** - All logs indicate synchronous architecture path

## Document Results

Update `docs/journal/phase7_validation.md` with:
- Test date and time
- Booking ID(s) created
- Outbox event ID(s)
- Response times
- Any issues encountered
- Overall test status (Pass/Fail)

