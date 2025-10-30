# Cosmos-baserad biljettmodell – överväganden

## Syfte
Syftet med detta dokument är att samla de tidiga tankarna kring hur bokningar och biljetter ska lagras i Cosmos DB för MVP:t. Det är inte ett slutgiltigt beslut (ingen ADR) utan ett underlag för kommande designbeslut.

## Problem / utmaningar
- Vi vill kunna **boka nu men använda senare** (aktivering vs giltighet).
- Vi vill **hålla allt i ett dokument i början** (billigt, enkelt, snabb MVP).
- Vi vill **behålla fält för framtida funktioner** (på-/avstigning, flera zoner) för att slippa migrera tidigt.
- Vi måste **logga källa och status** för att kunna bygga eventdrivna flöden senare.
- Vi vill **inte låsa oss vid rutter** nu, utan börja med **zon**.

## Domänantaganden (MVP)
- _Booking_ = kunden har beställt en biljett.
- _Ticket_ = det som visas/valideras, kan vara samma dokument i MVP.
- _Customer/User_ = den som äger bokningen (kan vara separat modell senare).

## Fältförslag (MVP)
**Identitet & kund**
- `id` – guid
- `customerId` – referens till användare (kan vara email i början)
- `customerName`
- `email`
- `phoneNumber` (valfritt men reserverat fält)

**Biljett**
- `ticketType` – `standard | student | senior`
- `zone` – en zon i MVP, men fältet finns
- `routeOn` / `routeOff` – hållplats in/ut, för framtida behov
- `validFrom` / `validTo` – när biljetten gäller
- `ticketDuration` – hur länge biljetten gäller
- `activatedAt` - för framtiden, see Aktiverings-avsnittet längre ner

**Meta & drift**
- `bookingDate` – när bokningen skapades
- `status` – `pending | confirmed | cancelled`
- `source` – `web | mobile | inspector | backoffice`
- `price` / `currency`
- `validationCode` – kan senare göras om till QR

## Exempel
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

## Aktivering (framtida)

MVP använder fördefinierad giltighet: klienten skickar `validFrom`, servern räknar ut `validTo` baserat på biljettens längd.

För framtida stöd av “aktivera vid påstigning” läggs följande fält till:
- `activatedAt` – tidpunkt då biljetten faktiskt börjar gälla
- `ticketDurationMinutes` – t.ex. 60, 1440, 43200

Vid aktivering:  
`validFrom = activatedAt`  
`validTo = activatedAt + ticketDurationMinutes`