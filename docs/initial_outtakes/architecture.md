## Arkitekturöversikt
### Syfte
  
Detta dokument beskriver hur applikationen är uppbyggd och hur dataflödet ser ut både ur användarens perspektiv och ur ett tekniskt perspektiv. Systemet är designat för att vara modulärt, händelseförberett och kostnadseffektivt med fokus på enkel drift i Azure. 
  
1. Användarflöde
```java
[ Kund ] 
   │
   │ 1. Loggar in med kundkonto (ASP.NET Identity) (Admin/Inspektör loggar in via Entra ID)
   │
   ▼
[ Blazor Server UI ]
   │ 2. Visar tillgängliga resor (kanske även zoner)
   │ 3. Kund väljer resa och bokar biljett
   │
   ▼
[ Ticketing API ]
   │ 4. Tar emot bokningsförfrågan
   │ 5. Skapar bokning i databasen
   │
   ▼
[ Cosmos DB (Serverless) ]
   │ 6. Lagrar bokning, resa, zon och referens till användare:
            - kund: userAccountId (GUID)
            - admin/inspektör: oid (Entra ID)
   │
   ▼
[ Application Insights ]
   │ 7. Loggar användarflödet för övervakning och statistik
   │
   ▼
[ Blazor Server UI ]
   │ 8. Visar bekräftelse och "Mina bokningar"
```
 
2. Tekniskt flöde (intern arkitektur)
```java
                      ┌─────────────────────────────────────┐
                      │            Användargränssnitt       │
                      │─────────────────────────────────────│
                      │  Blazor Server (.NET 8)             │
                      │  Hanterar UI, inloggning och session│
                      │  Auth: ASP.NET Identity + Entra ID  │
                      └─────────────────────────────────────┘
                                      │
                                      ▼
                      ┌─────────────────────────────────────┐
                      │           Applikationslager         │
                      │─────────────────────────────────────│
                      │  ASP.NET Controller API             │
                      │  • Validerar request                │
                      │  • Anropar domäntjänster            │
                      │  • Skapar outbox-event (valfritt)   │
                      └─────────────────────────────────────┘
                                      │
                                      ▼
                      ┌─────────────────────────────────────┐
                      │           Databaslager              │
                      │─────────────────────────────────────│
                      │  Azure Cosmos DB (Serverless)       │
                      │  • Trip-, Booking-, Zone-data       │
                      │  • Minimal kostnad vid idle         │
                      └─────────────────────────────────────┘
                                      │
                                      ▼
           ┌────────────────────────────────────────────────────────┐
           │       Händelse- och integrationslager (påslagsbart)    │
           │────────────────────────────────────────────────────────│
           │  • Azure Service Bus  – mellanlagrar händelser         │
           │  • Azure Function     – behandlar t.ex. BookingCreated │
           │  • Outbox Pattern     – säkerställer leverans          │
           └────────────────────────────────────────────────────────┘
                                      │
                                      ▼
                      ┌─────────────────────────────────────┐
                      │   Konfiguration och säkerhet        │
                      │─────────────────────────────────────│
                      │  Azure App Configuration            │
                      │  Azure Key Vault                    │
                      │  Application Insights (telemetri)   │
                      └─────────────────────────────────────┘
```

### Sammanfattning
Applikationen följer en tydlig lagerindelning: 
- Blazor Server hanterar interaktion med användaren.
- Ticketing API implementerar affärslogiken och kommunicerar med databasen.
- Cosmos DB fungerar som central lagring.
- Service Bus och Functions kan aktiveras via feature flag för att övergå till ett händelsestyrt arbetssätt.
- App Configuration, Key Vault och Application Insights används tvärs över hela systemet för konfiguration, säkerhet och övervakning.
 
### Bilaga - Utkast till Eventflöde (framtida modul)
```java
[ Ticketing API ]
    │
    │ 1. Kund skapar bokning via Blazor-gränssnittet
    │
    ├─► Skapar bokning i Cosmos DB
    │
    ├─► Lägger till post i Outbox (typ: BookingCreated)
    │
    └─► Om feature-flaggan "BookingEvents.Enabled" = true:
            ▼
            [ Azure Service Bus ]
                │
                │ 2. Tar emot meddelandet "BookingCreated"
                │
                ▼
            [ Azure Function: OnBookingCreated ]
                │
                │ 3. Behandlar händelsen:
                │     - uppdaterar status / notifierar / skriver logg
                │     - kan utlösa nya events (t.ex. BookingConfirmed)
                │
                ▼
            [ Application Insights ]
                │
                │ 4. Loggar hela händelsekedjan för spårning och analys
                ▼
            [ Cosmos DB ]
                │
                │ 5. Eventuella uppdateringar i datalagringen
                ▼
            [ Blazor UI ]
                │
                │ 6. Kunden får uppdaterad status (t.ex. "Bekräftad bokning")
```
### Sammanfattning av eventflödet
Detta flöde illustrerar hur systemet kan utökas till ett händelsestyrt arbetssätt utan att förändra den befintliga kärnlogiken. I den synkrona versionen skrivs bokningen direkt till databasen via API:t. 
 
När eventflödet aktiveras används Outbox-mönstret för att skapa en händelsepost i samma transaktion som datalagringen. Denna händelse skickas sedan till Azure Service Bus, där en Azure Function reagerar på meddelandet och utför efterbearbetning, till exempel loggning, notifiering eller statusuppdatering. 
 
Fördelarna med detta tillvägagångssätt är:
- Systemet blir lösare kopplat och kan byggas ut stegvis.
- Nya funktioner kan läggas till som separata prenumeranter (t.ex. e-post, statistik, rapportering).
- Eventflödet kan aktiveras eller pausas via feature-flaggor utan påverkan på grundsystemet.
- Det möjliggör skalig parallell behandling utan att API:et belastas av långvariga operationer. 
 
Detta gör arkitekturen framtidssäker och redo för en mer distribuerad, händelsedriven infrastruktur när behovet uppstår. Det innebär även att vi kan simulera en miljö där ett befintligt system, byggt på kedjade API-anrop (API chaining), kan refaktoreras och ersättas med en modernare och mer händelsedriven arkitektur.
 
### Disclaimer
Jag har bett en LLM att rita ASCII-flödena utefter min beskrivning. 