# Phase 7.4 Testing Guide: Error Scenarios

## Prerequisites

1. **Current State:**
   - Feature flag is enabled (`BookingEvents_Enabled = True`)
   - System is operational in event-driven mode
   - Service Bus queue is empty or has been consumed
   - Outbox has 0 pending events

## Test Scenarios

### Test 1: Service Bus Connection Failure (Event-Driven Mode)

**Objective:** Verify that when Service Bus is unavailable, outbox events remain pending and are retried later.

**Test Steps:**

1. **Simulate Service Bus Connection Failure:**
   - **Option A (Recommended):** Temporarily revoke Service Bus access for App Service managed identity
     ```bash
     # Remove Service Bus Data Owner role from App Service
     az role assignment delete \
       --assignee <app-service-principal-id> \
       --role "Azure Service Bus Data Owner" \
       --scope /subscriptions/<subscription-id>/resourceGroups/<rg>/providers/Microsoft.ServiceBus/namespaces/<namespace>
     ```
   - **Option B:** Stop/disable Service Bus namespace (not recommended - affects other services)
   - **Option C:** Use incorrect queue name in configuration (requires code change)

2. **Create a Booking:**
   - Navigate to `/user` or `/bookings` page
   - Create a booking
   - **Note the booking ID**

3. **Verify Outbox Event Created:**
   - Check Admin Dashboard mini health check - should show 1 pending event
   - Or check Cosmos DB outbox container - event should have `status: "Pending"`

4. **Wait for Processing Attempt:**
   - Wait up to 30 seconds for OutboxProcessorService to attempt processing
   - Check Application Insights logs for Service Bus connection errors

5. **Verify Error Handling:**
   - Outbox event should remain `Pending` (not marked as `Processed`)
   - Application Insights should show Service Bus connection errors
   - Booking should still be created successfully (synchronous mode unaffected)
   - No Service Bus messages should be sent

6. **Restore Service Bus Access:**
   ```bash
   # Restore Service Bus Data Owner role
   az role assignment create \
     --assignee <app-service-principal-id> \
     --role "Azure Service Bus Data Owner" \
     --scope /subscriptions/<subscription-id>/resourceGroups/<rg>/providers/Microsoft.ServiceBus/namespaces/<namespace>
   ```

7. **Verify Retry and Recovery:**
   - Wait up to 30 seconds for OutboxProcessorService to retry
   - Outbox event should now be marked as `Processed`
   - Service Bus message should be sent
   - Function App should process the event

**Expected Results:**
- ✓ Booking created successfully (synchronous mode works)
- ✓ Outbox event created with `Pending` status
- ✓ Service Bus connection errors logged in Application Insights
- ✓ Outbox event remains `Pending` while Service Bus is unavailable
- ✓ After Service Bus access restored, event is processed successfully
- ✓ No data loss - booking and event preserved

---

### Test 2: Function Processing Failure

**Objective:** Verify that when Function App fails to process a message, Service Bus retry mechanism and dead letter queue work correctly.

**Test Steps:**

1. **Create a Booking (Normal Flow):**
   - Create a booking with feature flag enabled
   - Wait for event to be processed (outbox → Service Bus)
   - Verify Function App receives and processes the event successfully

2. **Simulate Function Processing Failure:**
   - **Option A (Recommended):** Temporarily disable Function App
     ```bash
     az functionapp stop --name examwork-functions-dev --resource-group <rg>
     ```
   - **Option B:** Modify Function code to throw exception (requires deployment)
   - **Option C:** Use invalid message format (requires manual message injection)

3. **Create Another Booking:**
   - Create a booking
   - **Note the booking ID**
   - Wait for outbox event to be published to Service Bus

4. **Verify Service Bus Retry Mechanism:**
   - Check Service Bus queue metrics in Azure Portal
   - Message should be retried (delivery count should increase)
   - Check Application Insights for Function execution failures
   - Retry policy: 3 retries with exponential backoff (5 seconds to 5 minutes)

5. **Verify Dead Letter Queue:**
   - After 10 delivery attempts, message should move to dead letter queue
   - Check Service Bus queue → Dead-letter messages count
   - Dead letter queue should contain the failed message

6. **Restore Function App:**
   ```bash
   az functionapp start --name examwork-functions-dev --resource-group <rg>
   ```

7. **Verify Recovery (Optional):**
   - Manually reprocess dead letter queue message (if needed)
   - Or verify that new messages are processed correctly

**Expected Results:**
- ✓ Booking created successfully (synchronous mode works)
- ✓ Outbox event processed and published to Service Bus
- ✓ Function App retries failed messages (3 retries with exponential backoff)
- ✓ After 10 delivery attempts, message moved to dead letter queue
- ✓ Dead letter queue contains failed message
- ✓ No data loss - booking and event preserved
- ✓ New messages processed correctly after Function App restored

---

### Test 3: Dead Letter Queue Handling

**Objective:** Verify dead letter queue configuration and message inspection.

**Test Steps:**

1. **Check Dead Letter Queue Configuration:**
   - Navigate to Azure Portal → Service Bus → `examwork-sb-dev` → Queues → `booking-events`
   - Verify dead letter queue settings:
     - `maxDeliveryCount: 10`
     - `deadLetteringOnMessageExpiration: true`
     - `defaultMessageTimeToLive: P14D` (14 days)

2. **Inspect Dead Letter Queue (if messages exist):**
   - Navigate to Service Bus queue → Dead-letter messages
   - View dead letter messages (if any from previous tests)
   - Check message properties: delivery count, enqueued time, error details

3. **Verify Dead Letter Queue Metrics:**
   - Check Service Bus metrics for dead letter message count
   - Verify messages are not automatically reprocessed

**Expected Results:**
- ✓ Dead letter queue configured correctly
- ✓ Failed messages moved to dead letter queue after max delivery count
- ✓ Dead letter messages can be inspected
- ✓ Dead letter messages require manual intervention to reprocess

---

### Test 4: Outbox Retry Logic

**Objective:** Verify that outbox events are retried when publishing fails.

**Test Steps:**

1. **Note Current Implementation:**
   - `OutboxProcessorService` has `MaxRetryCount = 3` constant
   - However, retry count is not currently incremented in code
   - Events remain `Pending` on failure and are retried on next polling cycle

2. **Create Booking with Service Bus Unavailable:**
   - Follow Test 1 steps to make Service Bus unavailable
   - Create a booking
   - Verify outbox event remains `Pending`

3. **Verify Retry Behavior:**
   - OutboxProcessorService polls every 30 seconds
   - Each polling cycle attempts to process pending events
   - Check Application Insights logs for retry attempts
   - Events should be retried on each polling cycle until successful

4. **Restore Service Bus and Verify:**
   - Restore Service Bus access
   - Wait for next polling cycle (up to 30 seconds)
   - Event should be processed successfully

**Expected Results:**
- ✓ Outbox events retried on each polling cycle (every 30 seconds)
- ✓ Events remain `Pending` until successfully published
- ✓ No events are lost - all events eventually processed
- ✓ Retry attempts logged in Application Insights

**Note:** Current implementation doesn't track retry count in outbox events. This is acceptable for MVP but could be enhanced to track retry attempts and mark events as `Failed` after max retries.

---

### Test 5: Verify Synchronous Mode Unaffected by Event System Failures

**Objective:** Verify that when event system fails, synchronous mode continues to work normally.

**Test Steps:**

1. **Enable Feature Flag:**
   - Ensure feature flag is enabled (`BookingEvents_Enabled = True`)

2. **Simulate Event System Failure:**
   - Make Service Bus unavailable (Test 1) OR
   - Stop Function App (Test 2)

3. **Create Multiple Bookings:**
   - Create 3-5 bookings in synchronous mode (feature flag disabled)
   - **Note all booking IDs**

4. **Verify Synchronous Mode:**
   - All bookings should be created successfully
   - Bookings should be retrievable via API/UI
   - No errors in booking creation
   - Response times should be normal (< 500ms)

5. **Verify Outbox Events:**
   - Outbox events should be created for all bookings
   - Events should have `status: "Pending"`
   - Events should remain pending (not processed due to event system failure)

6. **Restore Event System:**
   - Restore Service Bus access or start Function App

7. **Verify Event System Recovery:**
   - Wait for OutboxProcessorService to process pending events
   - Events should be processed successfully
   - Service Bus messages should be sent
   - Function App should process events

**Expected Results:**
- ✓ All bookings created successfully regardless of event system status
- ✓ Bookings retrievable and functional
- ✓ No impact on synchronous mode performance
- ✓ Outbox events created for audit trail
- ✓ Events processed after event system recovery
- ✓ Zero data loss - all bookings preserved

---

## Validation Checklist

- [ ] Service Bus connection failure handled gracefully
- [ ] Outbox events remain pending during Service Bus failures
- [ ] Events retried and processed after Service Bus recovery
- [ ] Function processing failures trigger Service Bus retry mechanism
- [ ] Dead letter queue receives messages after max delivery count
- [ ] Outbox retry logic works (events retried on polling cycles)
- [ ] Synchronous mode unaffected by event system failures
- [ ] No data loss during error scenarios
- [ ] All errors logged in Application Insights
- [ ] System recovers gracefully after failures resolved

## Expected Results Summary

✓ **Resilient Error Handling** - System handles failures gracefully without data loss  
✓ **Retry Mechanisms** - Both Service Bus and Outbox have retry logic  
✓ **Dead Letter Queue** - Failed messages moved to DLQ after max attempts  
✓ **Synchronous Mode Isolation** - Event system failures don't affect booking creation  
✓ **Audit Trail Maintained** - All bookings create outbox events regardless of event system status  
✓ **Recovery** - System processes pending events after failures resolved  
✓ **Observability** - All errors logged in Application Insights  

## Test Results Template

**Test Date:** [To be filled]

**Test 1: Service Bus Connection Failure**
- Booking ID: `[booking-id]`
- Outbox Event ID: `[event-id]`
- Service Bus error logged: [ ] Yes [ ] No
- Event processed after recovery: [ ] Yes [ ] No
- Processing time after recovery: `[X]` seconds

**Test 2: Function Processing Failure**
- Booking ID: `[booking-id]`
- Service Bus message delivery count: `[X]`
- Message moved to DLQ: [ ] Yes [ ] No
- DLQ message count: `[X]`

**Test 3: Dead Letter Queue**
- DLQ configuration verified: [ ] Yes [ ] No
- DLQ messages inspected: [ ] Yes [ ] No
- DLQ message count: `[X]`

**Test 4: Outbox Retry Logic**
- Booking ID: `[booking-id]`
- Outbox Event ID: `[event-id]`
- Retry attempts observed: `[X]`
- Event processed after retries: [ ] Yes [ ] No

**Test 5: Synchronous Mode Isolation**
- Bookings created during event system failure: `[X]`
- All bookings successful: [ ] Yes [ ] No
- Events processed after recovery: [ ] Yes [ ] No

**Overall Status:** [ ] Pass [ ] Fail [ ] Partial

**Key Findings:**
- [To be documented]

## Troubleshooting

**If Service Bus errors not appearing in logs:**
- Check Application Insights query for "Service Bus" or "ServiceBusException"
- Verify managed identity has correct permissions
- Check Service Bus namespace is accessible

**If dead letter queue not receiving messages:**
- Verify `maxDeliveryCount` is set to 10 in Service Bus queue configuration
- Check Function App is actually failing (not just logging errors)
- Verify retry policy in `host.json` is configured correctly

**If outbox events not retrying:**
- Check OutboxProcessorService is running (Application Insights logs)
- Verify feature flag is enabled
- Check polling interval (30 seconds)

## Document Results

Update `docs/journal/phase7_validation.md` Phase 7.4 section with:
- Test date and time
- Results for each test scenario
- Booking IDs and event IDs
- Error messages and logs
- Recovery times
- Any issues encountered
- Overall test status (Pass/Fail)

