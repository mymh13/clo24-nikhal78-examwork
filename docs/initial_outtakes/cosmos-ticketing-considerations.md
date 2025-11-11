# Cosmos-based Ticket Model – Considerations

## Purpose
The purpose of this document is to gather early thoughts on how bookings and tickets should be stored in Cosmos DB for the MVP. It is not a final decision (no ADR) but a basis for upcoming design decisions.

## Problems / Challenges
- We want to be able to **book now but use later** (activation vs validity).
- We want to **keep everything in one document initially** (cheap, simple, fast MVP).
- We want to **retain fields for future features** (boarding/alighting, multiple zones) to avoid early migration.
- We must **log source and status** to be able to build event-driven flows later.
- We want to **not lock ourselves to routes** now, but start with **zone**.

## Domain Assumptions (MVP)
- _Booking_ = the customer has ordered a ticket.
- _Ticket_ = what is displayed/validated, can be the same document in MVP.
- _Customer/User_ = the one who owns the booking (can be a separate model later).

## Field Proposal (MVP)
**Identity & Customer**
- `id` – guid
- `customerId` – reference to user (can be email initially)
- `customerName`
- `email`
- `phoneNumber` (optional but reserved field)

**Ticket**
- `ticketType` – `standard | student | senior`
- `zone` – one zone in MVP, but the field exists
- `routeOn` / `routeOff` – boarding/alighting stop, for future needs
- `validFrom` / `validTo` – when the ticket is valid
- `ticketDuration` – how long the ticket is valid
- `activatedAt` - for the future, see Activation section below

**Meta & Operations**
- `bookingDate` – when the booking was created
- `status` – `pending | confirmed | cancelled`
- `source` – `web | mobile | inspector | backoffice`
- `price` / `currency`
- `validationCode` – can later be converted to QR

## Example
```json
{
  "id": "guid-här",
  "customerId": "guid-eller-email",
  "customerName": "Anna Andersson",
  "email": "anna@example.com",
  "phoneNumber": "+46700000000",

  "ticketType": "student",
  "zone": "Malmö",
  "routeOn": "Triangeln",
  "routeOff": "Hyllie",

  "validFrom": "2025-10-31T08:00:00Z",
  "validTo": "2025-10-31T10:00:00Z",

  "bookingDate": "2025-10-30T19:22:00Z",
  "status": "confirmed",
  "source": "web",
  "ticketDuration": "PT1H",

  "price": 0,
  "currency": "SEK",
  "validationCode": "A7K3FD"
}
```

## Activation (Future)

MVP uses predefined validity: the client sends `validFrom`, the server calculates `validTo` based on the ticket's length.

For future support of "activate on boarding", the following fields are added:
- `activatedAt` – point in time when the ticket actually starts to be valid
- `ticketDurationMinutes` – e.g. 60, 1440, 43200

On activation:  
`validFrom = activatedAt`  
`validTo = activatedAt + ticketDurationMinutes`