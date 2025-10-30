# ADR-006 – Eventdriven arkitektur: Azure Service Bus + Azure Function + Outbox Pattern

**Status:** Planned  
**Datum:** 2025-10-30  
**Författare:** Niklas Häll

---

## Sammanhang (Context)
Det befintliga systemet som projektet inspireras av är uppbyggt med sekventiella och kedjade API-anrop (API chaining).  
Detta leder till hög koppling mellan tjänster och svårigheter med felhantering och skalning.  
För att modernisera arkitekturen planeras en övergång till **händelsestyrd kommunikation**, där systemet reagerar på publicerade event snarare än synkrona API-anrop.

---

## Beslut (Decision)
Systemet förbereds för **eventdriven arkitektur** genom att:
- införa **Outbox Pattern** i API:t (bokningshändelser loggas samtidigt som datan skrivs till databasen),  
- skapa en **Azure Service Bus** för publicering och prenumeration av händelser,  
- låta en **Azure Function** reagera på utvalda event (exempelvis `BookingCreated`) och utföra efterbearbetning, notifiering eller audit-loggning.  

Hela flödet kan aktiveras eller stängas av via **feature-flaggor** i App Configuration, vilket gör att MVP kan köras helt utan dessa komponenter under utveckling.

---

## Konsekvenser (Consequences)
**Fördelar:**  
- Lösare koppling mellan komponenter.  
- Lätt att bygga ut nya funktioner som prenumererar på befintliga händelser.  
- Förbättrad robusthet och skalbarhet vid hög last.  
- En realistisk modell för att simulera modernisering av äldre API-baserade system.

**Nackdelar:**  
- Ökad komplexitet vid felsökning (flödet blir asynkront).  
- Kräver fler Azure-resurser och kan öka kostnaderna.  
- Funktionerna måste designas idempotenta för att undvika dubbla händelser.  

---

## Risker / Åtgärder
- **Risk:** Händelser kan tappas bort vid fel i Service Bus eller Function.  
  **Åtgärd:** Använd Dead Letter Queue och övervakning via Application Insights.  

- **Risk:** För tidig aktivering av eventflödet kan öka kostnader i MVP-fasen.  
  **Åtgärd:** Håll Service Bus och Functions provisionerade men inaktiva tills test eller demo.  

- **Risk:** Asynkronitet gör systembeteende svårare att förutsäga.  
  **Åtgärd:** Behåll kärnflödet synkront och logga event separat i Outbox tills mognad uppnåtts.  

---

## Alternativ (Alternatives)
- **Ren API-baserad arkitektur:** Enklare men svår att skala och felhantera.  
- **Azure Event Grid:** Mer avancerat och bra service i sig, men onödigt för MVP och riskerar att öka kostnaderna när flera händelsetyper införs.   
- **Service Bus Topics + flera Functions:** Kan införas i senare version för bredare eventdistribution.  
  
Notera: varje extra meddelandetjänst (Service Bus, Event Grid, Event Hubs) innebär ytterligare kostnader över tid. Därför hålls MVP:n till Service Bus + Function som minsta eventdrivna kärna.  

---

## Referenser (References)
- [Arkitekturöversikt](../architecture.md)  
- [Systemöversikt](../system_overview.md)  
- [Microsoft Docs – Azure Service Bus](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview)  
- [Microsoft Docs – Azure Functions](https://learn.microsoft.com/en-us/azure/azure-functions/functions-overview)  
- [Transaktionellt outbox-pattern med Azure Cosmos DB](https://learn.microsoft.com/sv-se/azure/architecture/databases/guide/transactional-outbox-cosmos)  
  