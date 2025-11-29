# Phase 7.3 Testing Guide: Switching Between Modes

## Prerequisites

1. **Current State:**
   - Feature flag is currently enabled (`BookingEvents_Enabled = True`)
   - Outbox has 0 pending events (all processed)
   - Service Bus queue is empty or has been consumed
   - System is operational in event-driven mode

## Test Steps

### Step 1: Create Booking with Feature Flag Enabled (Event-Driven Mode)

1. **Verify Feature Flag State:**
   - Navigate to https://ticket.mymh.dev/health
   - Confirm: `Feature Manager: ✓ Available - BookingEvents_Enabled = True`
   - Or check `/api/health` endpoint directly

2. **Create a Booking:**
   - Log in as a User (or Admin/Inspector)
   - Navigate to User landing page or `/bookings` page
   - Create a booking with one or more zones
   - **Note the booking ID** from success message (e.g., `booking-1`)

3. **Verify Event-Driven Behavior:**
   - Check outbox container in Cosmos DB - should have 1 pending event
   - Wait up to 30 seconds for OutboxProcessorService to process
   - Check health endpoint - pending events should decrease to 0
   - Verify Service Bus queue received message (or was consumed by Function)
   - Verify outbox event status changed to `Processed`

### Step 2: Disable Feature Flag (Switch to Synchronous Mode)

**Via Azure CLI:**
```bash
# Disable feature flag
az appconfig feature disable \
  --name examwork-appconfig-dev \
  --feature BookingEvents_Enabled \
  --yes

# Update sentinel key to trigger refresh (PowerShell)
az appconfig kv set \
  --name examwork-appconfig-dev \
  --key Settings:Sentinel \
  --value "$([DateTimeOffset]::UtcNow.ToUnixTimeSeconds())" \
  --yes

# Alternative for bash/Git Bash (Windows):
# Note: Git Bash on Windows may interpret 'date' as PowerShell Get-Date
# Use Python instead for reliable Unix timestamp:
az appconfig kv set \
  --name examwork-appconfig-dev \
  --key Settings:Sentinel \
  --value $(python -c "import time; print(int(time.time()))") \
  --yes
```

**Via Azure Portal:**
1. Navigate to Azure Portal → App Configuration → `examwork-appconfig-dev`
2. Go to **Feature Manager** → **Feature flags**
3. Find feature flag: `BookingEvents_Enabled`
4. Set value to **`false`** (disabled)
5. **Update sentinel key** to trigger hot-reload:
   - Go to **Configuration explorer** → Edit `Settings:Sentinel`
   - Increment value (e.g., if current is "1764336187", set to "1764336188")
   - Save

### Step 3: Wait for Hot-Reload

1. **Wait 30 seconds** for hot-reload (sentinel key pattern, refresh interval is 30 seconds)
2. **Check health endpoint:**
   - Navigate to https://ticket.mymh.dev/health
   - Verify: `Feature Manager: ✓ Available - BookingEvents_Enabled = False`
   - Verify: Sentinel value updated to the new value
   - Or check `/api/health` endpoint directly

### Step 4: Create Booking with Feature Flag Disabled (Synchronous Mode)

1. **Create a Booking:**
   - Log in as a User (or Admin/Inspector)
   - Navigate to User landing page or `/bookings` page
   - Create a booking with one or more zones
   - **Note the booking ID** from success message (e.g., `booking-2`)

2. **Verify Synchronous Behavior:**
   - Check outbox container in Cosmos DB - should have 1 new pending event
   - **Wait 1-2 minutes** - outbox event should remain `Pending` (not processed)
   - Check health endpoint - pending events count should increase
   - Verify Service Bus queue remains empty (no new messages)
   - Verify outbox event status remains `Pending` (not `Processed`)

### Step 5: Re-Enable Feature Flag (Switch Back to Event-Driven Mode)

**Via Admin Dashboard (Recommended - Fastest Method):**
1. Navigate to https://ticket.mymh.dev/admin (must be logged in as Admin)
2. Scroll to **"Event-Driven Architecture Status"** section
3. Verify current state:
   - Feature flag shows: `BookingEvents_Enabled = False`
   - Outbox shows: `X pending events` (should be 1 from Step 4)
4. Click **"Toggle Feature Flag"** button
5. Wait for 5-second cooldown (button shows countdown)
6. Watch for **"Waiting for change..."** status (yellow box)
   - Shows elapsed time and check count
   - Polls every 3 seconds to detect when change takes effect
7. When you see **"Change applied!"** (green box), the flag is enabled
8. Verify in mini health check:
   - Feature flag now shows: `BookingEvents_Enabled = True`
   - Sentinel value has updated

**Via Azure CLI (Alternative):**
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

# Alternative for bash/Git Bash (Windows):
# Note: Git Bash on Windows may interpret 'date' as PowerShell Get-Date
# Use Python instead for reliable Unix timestamp:
az appconfig kv set \
  --name examwork-appconfig-dev \
  --key Settings:Sentinel \
  --value $(python -c "import time; print(int(time.time()))") \
  --yes
```

**Via Azure Portal (Alternative):**
1. Navigate to Azure Portal → App Configuration → `examwork-appconfig-dev`
2. Go to **Feature Manager** → **Feature flags**
3. Find feature flag: `BookingEvents_Enabled`
4. Set value to **`true`** (enabled)
5. **Update sentinel key** to trigger hot-reload (increment value)

### Step 6: Wait for Hot-Reload and Verify Processing

**If using Admin Dashboard toggle:**
- The propagation polling UX automatically detects when the change takes effect
- You'll see "Change applied!" message when ready
- No manual waiting needed - the UI handles it

**If using Azure CLI/Portal:**
1. **Wait 30 seconds** for hot-reload (refresh interval is 30 seconds)
2. **Check health endpoint:**
   - Verify: `Feature Manager: ✓ Available - BookingEvents_Enabled = True`
   - Verify: Sentinel value updated to the new value

**Verify Backlog Processing (All Methods):**
3. **Wait up to 30 seconds** for OutboxProcessorService to process pending events
4. **Check Admin Dashboard or Health Endpoint:**
   - Pending events count should decrease from 1 to 0 (or decrease by number of pending events)
   - Refresh the mini health check section to see updated count
5. **Verify Processing:**
   - Check health endpoint - pending events should decrease (all processed)
   - Verify outbox events from Step 4 are now marked as `Processed` (check Cosmos DB)
   - Verify Service Bus queue received messages (or were consumed by Function)
   - Verify Function App processed the events (check Application Insights or Azure Portal)

### Step 7: Create Another Booking with Feature Flag Enabled

1. **Create a Booking:**
   - Create another booking
   - **Note the booking ID** from success message (e.g., `booking-3`)

2. **Verify Event-Driven Behavior Again:**
   - Check outbox - should have 1 new pending event
   - Wait up to 30 seconds - event should be processed
   - Verify Service Bus and Function App activity

## Validation Checklist

- [ ] Feature flag can be toggled at runtime (enabled → disabled → enabled)
- [ ] Hot-reload works correctly (flag updates within 30 seconds without restart)
- [ ] Booking created in event-driven mode: outbox event processed, Service Bus message sent
- [ ] Booking created in synchronous mode: outbox event created but not processed, no Service Bus message
- [ ] All bookings created successfully regardless of feature flag state
- [ ] Outbox events exist for all bookings (audit trail maintained)
- [ ] Service Bus messages only for bookings created when flag was enabled
- [ ] Pending events from synchronous mode are processed when flag is re-enabled
- [ ] No data loss or corruption during mode switches
- [ ] System handles mode switch gracefully (no errors, no downtime)

## Expected Results Summary

✓ **Runtime switching works** - Feature flag can be toggled without restart  
✓ **Hot-reload functional** - Configuration updates within 30 seconds  
✓ **Dual-system coexistence** - Both modes work correctly  
✓ **Audit trail maintained** - All bookings create outbox events  
✓ **Selective processing** - Only events from enabled periods are published  
✓ **Backlog processing** - Pending events processed when flag re-enabled  
✓ **No data loss** - All bookings saved correctly regardless of mode  
✓ **Zero downtime** - Mode switches happen without service restart  

## Test Results Template

**Test Date:** [To be filled]

**Booking IDs:**
- Booking 1 (event-driven): `[booking-id-1]`
- Booking 2 (synchronous): `[booking-id-2]`
- Booking 3 (event-driven): `[booking-id-3]`

**Outbox Events:**
- Event 1 (from booking 1): `[event-id-1]` - Status: `Processed`
- Event 2 (from booking 2): `[event-id-2]` - Status: `Processed` (after re-enable)
- Event 3 (from booking 3): `[event-id-3]` - Status: `Processed`

**Service Bus Messages:**
- Messages for booking 1: ✓ Sent
- Messages for booking 2: ✗ Not sent (created during synchronous mode)
- Messages for booking 3: ✓ Sent

**Hot-Reload Timing:**
- Disable flag → Refresh time: `[X]` seconds
- Enable flag → Refresh time: `[X]` seconds

**Overall Status:** [ ] Pass [ ] Fail [ ] Partial

**Key Findings:**
- [To be documented]

## Troubleshooting

**If hot-reload doesn't work:**
- Verify sentinel key was updated (check last modified timestamp in Azure Portal)
- Wait up to 30 seconds for refresh interval (refresh happens on each HTTP request)
- Refresh the health page to trigger the middleware
- Check health endpoint for current feature flag value and sentinel value
- Verify middleware is in place (`WebApplicationExtensions.cs`)
- Check Application Insights logs for refresh attempts and errors
- **Note:** If `IConfigurationRefresherProvider` is not found, the refresher is accessed via static variable (`ConfigurationExtensions.GetConfigurationRefresher()`)

**If outbox events aren't processed:**
- Check feature flag is actually enabled (health endpoint)
- Verify OutboxProcessorService is running (check Application Insights logs)
- Check for errors in OutboxProcessorService logs
- Verify Service Bus connection is working (health endpoint)

**If bookings fail during mode switch:**
- Check Application Insights for errors
- Verify all services are registered in DI
- Check Cosmos DB connection status
- Verify user authentication is working

## Document Results

Update `docs/journal/phase7_validation.md` Phase 7.3 section with:
- Test date and time
- Booking IDs created in each mode
- Outbox event IDs and statuses
- Service Bus message status
- Hot-reload timing
- Any issues encountered
- Overall test status (Pass/Fail)

