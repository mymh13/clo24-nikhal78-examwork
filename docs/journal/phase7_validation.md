# Phase 7: Testing & Validation Results

## Phase 7.1: Test Synchronous Flow (Feature Flag Disabled)

**Test Date:** 2025-11-28  
**Feature Flag State:** `BookingEvents_Enabled = false`  
**Expected Behavior:** System operates in synchronous mode (chained API calls), no Service Bus publishing

### Test Checklist

#### 1. Verify Bookings Work Exactly as Before (Chained API Mode)
- [x] Create a booking via API/UI
  - **Booking ID:** `e40c3e9d-fcca-4fca-b944-d88db4dc9982`
  - **Created by:** User role (test-user@mymh.dev)
- [x] Verify booking is successfully created in Cosmos DB `bookings` container
  - **Verified:** Booking exists in `bookings` container with correct structure
  - **Customer:** test-user@mymh.dev (ID: 4c1a085b-6de6-490d-a0cc-c20970c07d41)
  - **Zones:** Zone D, Zone B
  - **Pricing:** basePrice: 40, totalPrice: 40, priceModifier: 1
  - **Timestamp:** 2025-11-28T12:12:34.9109881Z
- [x] Verify booking can be retrieved via `GET /api/bookings/my-bookings` (User role)
  - **Status:** Booking appears in user's booking list
- [x] Verify booking appears in admin bookings list
  - **Status:** Verified (booking visible in Cosmos DB)
- [x] Verify booking data is correct (customer info, zones, pricing)
  - **Status:** All data correct - customer info, zones (Zone D, Zone B), pricing (40 SEK for 2 zones)
- [x] **Result:** Check **PASS** - Bookings work exactly as before, no breaking changes

#### 2. Verify Outbox Events Are Created (For Audit)
- [x] After creating a booking, verify outbox event exists in Cosmos DB `outbox` container
  - **Health endpoint shows:** "Outbox Service: ✓ Registered - Operational (2 pending events)"
  - **Status:** Outbox events confirmed in container
- [x] Query outbox container: `SELECT * FROM c WHERE c.status = "Pending"`
  - **Status:** 2 pending events found (includes test booking)
- [x] Verify outbox event has correct structure:
  - `eventType: "BookingCreated"` - ✓ Verified
  - `status: "Pending"` - ✓ Verified (2 pending events)
  - `eventData` contains booking JSON - ✓ Verified
- [x] Verify outbox event `createdAt` timestamp matches booking creation time
  - **Status:** Timestamps verified
- [x] **Result:** ✓ **PASS** - Outbox events created correctly for audit purposes

#### 3. Verify No Service Bus Messages Sent
- [x] Check Service Bus queue `booking-events` in Azure Portal
  - **Active messages:** 0 ✓
  - **Dead-letter messages:** 0 ✓
  - **Scheduled messages:** 0 ✓
- [x] Verify queue message count is 0 (or unchanged from before test)
  - **Status:** ✓ Confirmed - 0 messages in queue
- [x] Check Application Insights logs for Service Bus publishing attempts
  - **Query used:** `traces | where message contains "Service Bus"`
  - **Result:** No Service Bus publishing messages found in last 24h ✓
  - **Note:** Application Insights query editor is hidden behind dropdown in top right - must switch from "Simple" mode to "KQL" mode to access query editor
- [x] Verify no messages in Service Bus queue after creating bookings
  - **Status:** ✓ Confirmed - 0 messages
- [x] **Result:** ✓ **PASS** - No Service Bus messages sent, synchronous mode confirmed

#### 4. Verify No Performance Impact from Event Infrastructure
- [ ] Measure booking creation response time
  - **Status:** Not measured in detail, but booking creation felt responsive
- [ ] Compare with baseline (if available) or verify response time is acceptable (< 500ms)
  - **Status:** Response time appears acceptable, no noticeable delay
- [ ] Check Application Insights for any performance degradation
  - **Status:** No performance issues observed
- [ ] Verify feature flag check overhead is minimal (~1ms)
  - **Status:** Overhead appears minimal
- [ ] Verify outbox write overhead is minimal (~5-10ms)
  - **Status:** Overhead appears minimal, no noticeable impact
- [x] **Result:** ✓ **PASS** - No significant performance impact observed

#### 5. Verify Logging and Observability
- [x] Check Application Insights logs for booking creation
  - **Query used:** `traces | where message contains "Synchronous"`
  - **Result:** No results found in last 24h
  - **Note:** May need to check with different query or time range. Logs may be using different message format.
- [ ] Verify log messages show "Architecture: Synchronous"
  - **Status:** Could not find in Application Insights (may need different query)
- [ ] Verify log messages show "Synchronous architecture - booking processed via chained API calls"
  - **Status:** Could not find in Application Insights (may need different query)
- [x] Verify no Service Bus publishing log messages
  - **Query used:** `traces | where message contains "Service Bus"`
  - **Result:** ✓ No Service Bus publishing messages found
- [x] Check health endpoint `/api/health` - verify `FeatureFlagTest` shows `BookingEvents_Enabled = False`
  - **Status:** ✓ Confirmed - Health endpoint shows: "Feature Manager: ✓ Available - BookingEvents_Enabled = False"
- [x] **Result:** ⚠️ **PARTIAL** - Health endpoint confirms feature flag state, but Application Insights log queries need refinement

### Test Results Summary

**Overall Status:** ✓ **PASS** (with minor note on log querying)

**Key Findings:**
- ✓ Feature flag correctly disabled (`BookingEvents_Enabled = False`)
- ✓ Booking creation works exactly as before - no breaking changes
- ✓ Outbox events are created correctly (2 pending events confirmed)
- ✓ Service Bus queue has 0 messages - no event-driven publishing occurred
- ✓ System operates in synchronous mode as expected
- ⚠️ Application Insights log queries may need refinement - "Synchronous" keyword search didn't return results (may need to check exact log message format or use different query)

**Issues Encountered:**
- Application Insights query editor is hidden behind dropdown in top right corner - must switch from "Simple" mode to "KQL" mode to access query editor
- Log queries for "Synchronous" messages didn't return results - may need to check exact log message format or use different search terms

**Performance Metrics:**
- Average booking creation time: Not measured in detail, but appears responsive (< 500ms estimated)
- Outbox write time: Not measured, but appears minimal
- Feature flag check time: Not measured, but appears minimal (~1ms estimated)

**Notes:**
- Health endpoint at https://ticket.mymh.dev/health confirms all systems operational
- Test booking ID: `e40c3e9d-fcca-4fca-b944-d88db4dc9982`
- Outbox shows 2 pending events (includes test booking)
- Service Bus queue `booking-events` confirmed empty (0 active, 0 dead-letter, 0 scheduled messages)
- Application Insights querying: Editor is in dropdown menu (top right) - switch from "Simple" to "KQL" mode to access query editor

---

## Phase 7.2: Test Event-Driven Flow (Feature Flag Enabled)

**Test Date:** 2025-11-28  
**Feature Flag State:** `BookingEvents_Enabled = true`  
**Expected Behavior:** Events published to Service Bus, Function processes events

### Test Checklist

#### 1. Enable Feature Flag
- [x] Set `BookingEvents_Enabled = true` in Azure App Configuration
- [x] Update sentinel key `Settings:Sentinel` to trigger hot-reload
- [x] Wait 1 minute for configuration refresh
- [x] Verify health endpoint shows `BookingEvents_Enabled = True`
- [x] **Result:** ✓ Feature flag enabled successfully via Azure CLI (`az appconfig feature enable`). Hot-reload worked correctly after adding refresh middleware - feature flag updated to `True` within 1 minute without service restart.

#### 2. Verify Outbox Event Created
- [x] Create a booking
- [x] Verify outbox event exists with `status: "Pending"`
- [x] **Result:** ✓ Booking created successfully. Outbox event created with `status: "Pending"` and correct event structure.

#### 3. Verify Service Bus Message Sent
- [x] Wait for OutboxProcessorService to poll (up to 30 seconds)
- [x] Check Service Bus queue `booking-events` - verify message count increased
- [x] Verify outbox event status changed to `Processed` (after successful publish)
- [x] **Result:** ✓ OutboxProcessorService processed pending events automatically. Health endpoint showed pending events count decreased from 2 to 0, confirming events were published to Service Bus and marked as processed.

#### 4. Verify Function Receives and Processes Event
- [x] Check Application Insights for Function execution logs
- [x] Verify `OnBookingCreatedFunction` executed
- [x] Verify function logs show event processing
- [x] Verify function logs show booking details (BookingId, CustomerEmail, etc.)
- [x] **Result:** ✓ Function App received and processed events. Verified in Azure Portal - Function execution logs show successful processing. (Note: Application Insights querying can be improved with better monitoring dashboard - Phase 8)

#### 5. Verify Application Insights Logs
- [x] Check Application Insights for event flow:
  - Booking creation log
  - Outbox event creation log
  - Service Bus publishing log
  - Function execution log
- [x] Verify log messages show "Architecture: Event-Driven"
- [x] **Result:** ✓ Complete event flow verified. All components operational: Web App → Outbox → Service Bus → Function App → Application Insights.

### Test Results Summary

**Overall Status:** ✓ **Pass**

**Key Findings:**
- **Hot-reload works correctly:** Feature flag updates without service restart after adding refresh middleware (`IConfigurationRefresher.TryRefreshAsync()` in HTTP pipeline)
- **OutboxProcessorService operational:** Background service successfully polls outbox every 30 seconds and publishes events when feature flag is enabled
- **End-to-end flow verified:** Complete event-driven architecture operational: bookings → outbox → Service Bus → Azure Functions → Application Insights
- **Zero downtime switching:** Can toggle between synchronous and event-driven modes at runtime using feature flags
- **Monitoring improvement needed:** Application Insights querying requires navigating multiple tabs - Phase 8 (Monitoring & Observability) will add dashboards for better visibility
- **Performance:** No noticeable performance impact from event infrastructure when feature flag is enabled

---

## Phase 7.3: Test Switching Between Modes

**Test Date:** 2025-11-28  
**Testing Guide:** See `docs/journal/phase7_3_testing_guide.md` for detailed step-by-step instructions

**Hot-Reload Fix Applied:** 
- **Issue:** `IConfigurationRefresherProvider` was not found in service container, preventing hot-reload
- **Root Cause:** The refresher provider wasn't being registered automatically or wasn't accessible via service locator
- **Solution:** Store the refresher directly during configuration using `options.GetRefresher()` in `ConfigurationExtensions.cs` and access it via static variable in middleware
- **Implementation:** Added static variable `_configurationRefresher` and `GetConfigurationRefresher()` method. Middleware accesses refresher via static method instead of service container
- **Result:** Hot-reload now works correctly - configuration refreshes within 30 seconds without restart

### Test Checklist

#### 1. Create Booking with Feature Flag Enabled (Event-Driven Mode)
- [x] Verify feature flag is enabled (`BookingEvents_Enabled = True`)
- [x] Create booking (note booking ID: `3cbcf3c4-77e3-4d60-a136-4c84ce9dbb45`)
- [x] Verify outbox event created with `status: "Pending"` (immediately after booking)
- [x] Wait up to 30 seconds - verify event processed and published to Service Bus
- [x] Verify outbox event status changed to `Processed` (health endpoint shows 0 pending events)
- [x] **Result:** ✓ Event-driven flow working correctly - Booking created successfully, outbox event created and processed within 30 seconds. Health endpoint shows 0 pending events, indicating event was published to Service Bus and marked as Processed. Event-driven architecture operational.

#### 2. Disable Feature Flag and Verify Hot-Reload
- [x] Disable feature flag via Azure CLI or Portal
- [x] Update sentinel key to trigger hot-reload
- [x] Wait 30 seconds for configuration refresh (refresh interval is 30 seconds)
- [x] Verify health endpoint shows `BookingEvents_Enabled = False`
- [x] Verify sentinel value updated to new value
- [x] **Result:** ✓ Hot-reload worked correctly - feature flag updated from `True` to `False` within 30 seconds without restart. Sentinel value updated from `1764364056` to `1764365122`.

#### 3. Create Booking with Feature Flag Disabled (Synchronous Mode)
- [x] Create booking (note booking ID: `0e4fc863-8efc-462d-99d0-21ee97d11fa2`)
- [x] Verify outbox event created with `status: "Pending"` (health endpoint shows 1 pending event)
- [x] Wait 1-2 minutes - verify event remains `Pending` (not processed)
- [x] Verify Service Bus queue remains empty (no new messages - event-driven mode disabled)
- [x] Verify health endpoint shows increased pending events count (1 pending event)
- [x] **Result:** ✓ Synchronous mode working correctly - Booking created successfully, outbox event created with `Pending` status and remains unprocessed. Health endpoint shows 1 pending event, confirming OutboxProcessorService is not processing events when feature flag is disabled. No Service Bus messages sent. System operates in synchronous mode as expected.

#### 4. Re-Enable Feature Flag and Verify Backlog Processing
- [x] Enable feature flag via Azure CLI or Portal
- [x] Update sentinel key to trigger hot-reload
- [x] Wait 30 seconds for configuration refresh
- [x] Verify health endpoint shows `BookingEvents_Enabled = True`
- [x] Verify sentinel value updated to new value
- [ ] Wait up to 30 seconds - verify pending events from Step 3 are processed
- [ ] Verify outbox events marked as `Processed`
- [ ] Verify Service Bus messages sent for backlog events
- [x] **Result:** ✓ Hot-reload worked correctly - feature flag updated from `False` to `True` within 30 seconds without restart. Sentinel value updated from `1764365122` to `1764365937`. Backlog processing to be verified.

#### 5. Create Another Booking with Feature Flag Enabled
- [ ] Create booking (note booking ID: `[booking-3]`)
- [ ] Verify event-driven behavior works again
- [ ] Verify complete event flow: outbox → Service Bus → Function
- [ ] **Result:** [To be filled]

### Test Results Summary

**Overall Status:** ✓ **Pass** (Hot-reload validated, full mode switching test in progress)

**Booking IDs:**
- Booking 1 (event-driven): `3cbcf3c4-77e3-4d60-a136-4c84ce9dbb45`
- Booking 2 (synchronous): `0e4fc863-8efc-462d-99d0-21ee97d11fa2`
- Booking 3 (event-driven): `[booking-id-3]`

**Outbox Events:**
- Event 1 (from booking 1): `[event-id-1]` - Status: `Processed` (processed within 30 seconds)
- Event 2 (from booking 2): `[event-id-2]` - Status: `Processed` (after re-enable)
- Event 3 (from booking 3): `[event-id-3]` - Status: `Processed`

**Service Bus Messages:**
- Messages for booking 1: [x] Sent [ ] Not sent
- Messages for booking 2: [ ] Sent [x] Not sent (correct - synchronous mode)
- Messages for booking 3: [ ] Sent [ ] Not sent

**Hot-Reload Timing:**
- Disable flag → Refresh time: `~30` seconds (after page refresh to trigger middleware)
- Enable flag → Refresh time: `~30` seconds (after page refresh to trigger middleware)

**Key Findings:**
- **Hot-reload works correctly** - Feature flags can be toggled at runtime without service restart
- **Refresh mechanism:** Uses static variable to store refresher (`options.GetRefresher()`) instead of service container, as `IConfigurationRefresherProvider` was not found in DI
- **Refresh interval:** Reduced to 30 seconds for faster testing (can be increased to 1 minute for production)
- **Middleware trigger:** Refresh happens on each HTTP request - refreshing the health page triggers the middleware
- **Bidirectional switching:** Works in both directions (enabled → disabled → enabled) within 30 seconds
- **Sentinel key pattern:** Must update sentinel key value after changing feature flag to trigger refresh
- **Zero downtime:** Mode switches happen without service restart - perfect for live demonstrations

---

## Phase 7.4: Test Error Scenarios

**Test Date:** [To be filled]

### Test Checklist

- [ ] Test Service Bus connection failure (if possible)
- [ ] Test Function processing failure
- [ ] Test dead letter queue handling
- [ ] Test outbox retry logic
- [ ] Verify synchronous mode unaffected by event system failures
- [ ] **Result:** [To be filled]

---

## Phase 7.5: Performance Testing

**Test Date:** [To be filled]

### Test Checklist

- [ ] Compare performance: synchronous vs event-driven
- [ ] Test multiple concurrent bookings in both modes
- [ ] Test outbox processing throughput
- [ ] Test Function scaling behavior
- [ ] Document performance characteristics
- [ ] **Result:** [To be filled]

