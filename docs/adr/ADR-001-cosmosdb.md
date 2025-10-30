# ADR-001 – Val av databas: Azure Cosmos DB (Serverless) 
 
**Status:** Accepted  
**Datum:** 2025-10-30  
**Författare:** Niklas Häll 
 
---
 
## Sammanhang (Context) 
Projektet kräver en databaslösning som hanterar dynamiska datamängder med låg driftskostnad.  
Systemet ska kunna köras kontinuerligt utan att generera kostnader vid inaktivitet och ska enkelt kunna skalas upp vid behov.  
Eftersom lösningen körs i Azure-ekosystemet är kompatibilitet och enkel integration viktiga faktorer.
 
---

## Beslut (Decision) 
Vi använder **Azure Cosmos DB i Serverless-läge** som primär datalagring för resor, bokningar och zoner.  
Databasen erbjuder en kostnadseffektiv modell där endast faktiska förfrågningar debiteras, vilket passar projektets MVP-fas.  
Cosmos DB integreras direkt med .NET SDK och kan användas utan extern databasdrift. 
 
---
 
## Konsekvenser (Consequences) 
**Fördelar:** 
- Ingen fast kostnad – betala per förfrågan.  
- Skalbar och fullt hanterad lösning inom Azure.  
- Enkelt att integrera med .NET och Azure-tjänster (App Service, Functions, Key Vault).  
- Serverless-läget gör det lätt att hålla utvecklingsmiljön igång till låg kostnad. 
 
**Nackdelar:** 
- Begränsat stöd för avancerade joins och relationer.  
- Pris per request kan öka vid hög last.   
- Låst till Azure-ekosystemet. 
 
---
 
**Risker / Åtgärder:** 
- Begränsa externa anrop till databasen via privata nätverk och autentisering. 
- Säkerställ att endast API:t har rättigheter mot Cosmos via Managed Identity. 
 
---
 
## Alternativ (Alternatives) 
- **Azure SQL Database** – mer traditionell modell men högre grundkostnad.  
- **PostgreSQL (Azure Flexible Server)** – kraftfullt men kräver mer konfiguration och underhåll.  
- **MongoDB Atlas** – liknande dokumentmodell men utanför Azure-ekosystemet. 
 
---
 
## Referenser (References) 
- [Systemöversikt](../system_overview.md)  
- [Azure Cosmos DB – Serverless best practices](https://learn.microsoft.com/en-us/azure/cosmos-db/serverless) 
