# Phase 7.2 Testing Guide: Event-Driven Flow (Feature Flag Enabled)

## Prerequisites

1. **Current State:**
   - Feature flag is currently disabled (`BookingEvents_Enabled = False`)
   - Service Bus queue is empty (0 messages)
   - Outbox has 2 pending events from previous test

## Test Steps

### Step 1: Enable Feature Flag

**Option A: Via Azure Portal**
1. Navigate to Azure Portal → App Configuration → `examwork-appconfig-dev`
2. Go to **Feature Manager** → **Feature flags**
3. Find or create feature flag: `BookingEvents_Enabled`
4. Set value to **`true`** (enabled)
5. **Update sentinel key** to trigger hot-reload:
   - Go to **Configuration explorer** → **Create** (or edit existing)
   - Key: `Settings:Sentinel`
   - Set value to increment (e.g., if current is "1", set to "2")
   - This triggers automatic refresh of all configuration within 1 minute

**Option B: Via Azure CLI**
```bash
# Enable feature flag
az appconfig feature enable \
  --name examwork-appconfig-dev \
  --feature BookingEvents_Enabled \
  --yes

# Update sentinel key to trigger refresh (PowerShell)
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

### Step 2: Verify Feature Flag Enabled

1. **Wait 1 minute** for hot-reload (sentinel key pattern)
2. **Check health endpoint:**
   - Navigate to https://ticket.mymh.dev/health
   - Verify: `Feature Manager: ✓ Available - BookingEvents_Enabled = True`
   - Or check `/api/health` endpoint directly

### Step 3: Create a Test Booking

1. **Log in as a User** (or Admin/Inspector)
2. **Create a booking** via UI:
   - Navigate to User landing page
   - Select zones (e.g., Zone A, Zone C)
   - Create booking
   - **Note the booking ID** from success message

3. **Or create via API:**
   ```bash
   curl -X POST https://ticket.mymh.dev/api/bookings \
     -H "Content-Type: application/json" \
     -H "Cookie: [your-auth-cookie]" \
     -d '{
       "zone": "Zone A, Zone C",
       "region": ""
     }'
   ```

### Step 4: Verify Outbox Event Created

**Check Cosmos DB Outbox Container:**
- Navigate to Cosmos DB → Data Explorer
- Select `ticketing` database → `outbox` container
- Query: `SELECT * FROM c WHERE c.status = 'Pending' ORDER BY c.createdAt DESC`
- Verify:
  - New outbox event exists with `eventType: "BookingCreated"`
  - `status: "Pending"`
  - `eventData` contains booking JSON
  - `createdAt` timestamp matches booking creation time

**Or check health endpoint:**
- `/api/health` should show increased pending events count

### Step 5: Wait for OutboxProcessorService to Process

**Important:** The `OutboxProcessorService` polls every 30 seconds, so:
- Wait up to 30 seconds after creating the booking
- The service will check the feature flag and publish to Service Bus if enabled

### Step 6: Verify Service Bus Message Sent

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
- Check "Active message count" - should be > 0 (or increased)
- **Note:** Message may be consumed quickly by Function, so check immediately

**Check Outbox Event Status:**
- Query outbox container: `SELECT * FROM c WHERE c.status = 'Processed' ORDER BY c.processedAt DESC`
- Verify the outbox event status changed from `Pending` to `Processed`
- Verify `processedAt` timestamp is set

### Step 7: Verify Function Receives and Processes Event

**Check Application Insights:**
- **Note:** Query editor is in dropdown menu (top right) - switch from "Simple" to "KQL" mode
- Navigate to Application Insights → Logs
- Query for Function execution:
  ```kusto
  traces
  | where message contains "OnBookingCreated" or message contains "Received BookingCreated"
  | where timestamp > ago(10m)
  | project timestamp, message
  | order by timestamp desc
  ```

**Expected Log Messages:**
- `"Received BookingCreated event from Service Bus"`
- `"Processing BookingCreated event: BookingId=..., CustomerId=..., CustomerEmail=..."`
- `"BookingCreated event processed successfully: BookingId=..."`

**Check Function App Logs:**
- Navigate to Function App → Functions → `OnBookingCreated` → Monitor
- Check for recent executions
- Verify execution succeeded (green status)

### Step 8: Verify Application Insights Event Flow

**Check Complete Event Flow:**
```kusto
traces
| where timestamp > ago(10m)
| where message contains "Booking" or message contains "Event" or message contains "Service Bus"
| project timestamp, message
| order by timestamp desc
```

**Expected Log Sequence:**
1. `"Booking created: ... Architecture: Event-Driven"`
2. `"Outbox event created: ..."`
3. `"Event published to Service Bus: ..."`
4. `"Outbox event marked as processed: ..."`
5. `"Received BookingCreated event from Service Bus"` (Function)
6. `"Processing BookingCreated event: ..."` (Function)
7. `"BookingCreated event processed successfully: ..."` (Function)

### Step 9: Verify No Duplicate Processing

- Check that outbox event is marked as `Processed` (not still `Pending`)
- Verify only one Service Bus message was sent per booking
- Verify Function executed only once per event

## Validation Checklist

- [ ] Feature flag enabled: `BookingEvents_Enabled = True`
- [ ] Health endpoint confirms feature flag is enabled
- [ ] Booking created successfully
- [ ] Outbox event created with `status: "Pending"`
- [ ] OutboxProcessorService processed event (within 30 seconds)
- [ ] Service Bus message sent (queue count increased, or message consumed)
- [ ] Outbox event status changed to `Processed`
- [ ] Function `OnBookingCreated` executed
- [ ] Function logs show event processing with booking details
- [ ] Application Insights shows complete event flow
- [ ] Logs show "Architecture: Event-Driven"
- [ ] No duplicate processing

## Expected Results Summary

✓ **Feature flag enabled** - System switches to event-driven mode  
✓ **Outbox event created** - Event stored for processing  
✓ **Service Bus message sent** - Event published to queue  
✓ **Function processes event** - Function receives and processes event  
✓ **Complete event flow** - End-to-end event-driven architecture operational  
✓ **Logging confirms event-driven mode** - All logs indicate event-driven architecture path

## Troubleshooting

**If Service Bus message count doesn't increase:**
- Check OutboxProcessorService is running (should poll every 30 seconds)
- Verify feature flag is actually enabled (check health endpoint)
- Check Application Insights for OutboxProcessorService logs
- Verify Service Bus connection is working (check health endpoint)

**If Function doesn't execute:**
- Check Function App is deployed and running
- Verify Service Bus trigger binding is correct
- Check Function App logs for errors
- Verify Function App has correct RBAC roles (Service Bus Data Receiver)

**If outbox event doesn't change to Processed:**
- Check OutboxProcessorService logs for errors
- Verify Service Bus publishing succeeded
- Check for exceptions in Application Insights

## Document Results

Update `docs/journal/phase7_validation.md` Phase 7.2 section with:
- Test date and time
- Booking ID(s) created
- Outbox event ID(s)
- Service Bus message count
- Function execution details
- Any issues encountered
- Overall test status (Pass/Fail)

