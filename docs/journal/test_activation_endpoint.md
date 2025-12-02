# Testing the Activation Endpoint

## Prerequisites

1. **Application Running**: The application should be running locally or in Azure
2. **Authentication**: You need to be authenticated (Admin, Inspector, or User role)
3. **Existing Booking**: You need a booking with status `Created` to activate

## Step 1: Get Authentication Token/Cookie

Since the application uses cookie-based authentication for local users and OpenID Connect for Azure AD, you have two options:

### Option A: Use Browser (Easiest)
1. Navigate to `http://localhost:5000` (or your local URL)
2. Log in via `/login` page
3. Use browser DevTools to copy the authentication cookie
4. Use the cookie in your API requests

### Option B: Use Swagger/Postman
If Swagger is enabled, use the browser's authenticated session.

## Step 2: Create a Test Booking (if needed)

If you don't have a booking with status `Created`, create one first:

```http
POST /api/bookings
Authorization: Bearer {token} (or Cookie: {cookie})
Content-Type: application/json

{
  "customerEmail": "test@example.com",
  "customerName": "Test User",
  "zone": "Zone A",
  "region": ""
}
```

**Note the `bookingId` from the response** - you'll need it for activation.

## Step 3: Test Activation Endpoint

### Test Case 1: Basic Activation (Default: Now, 90 minutes)

```http
POST /api/bookings/{bookingId}/activate
Authorization: Bearer {token} (or Cookie: {cookie})
Content-Type: application/json

{}
```

**Expected Response (200 OK):**
```json
{
  "id": "booking-id",
  "customerId": "user-id",
  "customerEmail": "test@example.com",
  "customerName": "Test User",
  "zone": "Zone A",
  "status": "Activated",
  "activatedAt": "2025-12-02T10:00:00Z",
  "validFrom": "2025-12-02T10:00:00Z",
  "validTo": "2025-12-02T11:30:00Z",
  "bookingDate": "2025-12-02T09:00:00Z",
  ...
}
```

### Test Case 2: Custom Activation Time and Validity

```http
POST /api/bookings/{bookingId}/activate
Authorization: Bearer {token} (or Cookie: {cookie})
Content-Type: application/json

{
  "activationTime": "2025-12-02T14:00:00Z",
  "validityMinutes": 120
}
```

**Expected Response (200 OK):**
- `activatedAt`: "2025-12-02T14:00:00Z"
- `validFrom`: "2025-12-02T14:00:00Z"
- `validTo`: "2025-12-02T16:00:00Z" (120 minutes later)
- `status`: "Activated"

### Test Case 3: Admin/Inspector Activating for Another User

```http
POST /api/bookings/{bookingId}/activate
Authorization: Bearer {admin-token} (or Cookie: {admin-cookie})
Content-Type: application/json

{
  "customerId": "other-user-id",
  "activationTime": "2025-12-02T15:00:00Z",
  "validityMinutes": 90
}
```

## Test Cases for Error Handling

### Test Case 4: Booking Not Found (404)

```http
POST /api/bookings/non-existent-id/activate
Authorization: Bearer {token}
Content-Type: application/json

{}
```

**Expected Response (404 Not Found):**
```json
{
  "error": "Booking not found."
}
```

### Test Case 5: Booking Already Activated (400)

Try to activate the same booking twice:

```http
POST /api/bookings/{already-activated-booking-id}/activate
Authorization: Bearer {token}
Content-Type: application/json

{}
```

**Expected Response (400 Bad Request):**
```json
{
  "error": "Ticket cannot be activated. Current status: Activated. Only tickets with status 'Created' can be activated."
}
```

### Test Case 6: User Trying to Activate Another User's Ticket (400)

As a User role, try to activate a booking that belongs to a different user:

```http
POST /api/bookings/{other-user-booking-id}/activate
Authorization: Bearer {user-token}
Content-Type: application/json

{}
```

**Expected Response (400 Bad Request):**
```json
{
  "error": "You can only activate your own tickets."
}
```

### Test Case 7: Missing Customer ID (Admin/Inspector) (400)

As Admin/Inspector, try to activate without providing `customerId`:

```http
POST /api/bookings/{bookingId}/activate
Authorization: Bearer {admin-token}
Content-Type: application/json

{}
```

**Expected Response (400 Bad Request):**
```json
{
  "error": "Customer ID is required."
}
```

## Using cURL (Command Line)

### Basic Activation
```bash
curl -X POST "http://localhost:5000/api/bookings/{bookingId}/activate" \
  -H "Content-Type: application/json" \
  -H "Cookie: {your-auth-cookie}" \
  -d '{}'
```

### Custom Activation
```bash
curl -X POST "http://localhost:5000/api/bookings/{bookingId}/activate" \
  -H "Content-Type: application/json" \
  -H "Cookie: {your-auth-cookie}" \
  -d '{
    "activationTime": "2025-12-02T14:00:00Z",
    "validityMinutes": 120
  }'
```

## Verification Checklist

After activation, verify:

- [ ] `status` is set to `"Activated"`
- [ ] `activatedAt` is set to the activation time (or current time if not specified)
- [ ] `validFrom` equals `activatedAt`
- [ ] `validTo` is `activatedAt + validityMinutes` (default 90 minutes)
- [ ] Booking can be retrieved and shows updated fields
- [ ] Trying to activate again returns appropriate error
- [ ] User role can only activate their own bookings
- [ ] Admin/Inspector can activate any booking (with customerId)

## Next Steps

Once manual testing is complete:
- [ ] Proceed to Step 3: Add Activation UI to User Landing Page
- [ ] Integration tests can be added later (Priority 2.2)

