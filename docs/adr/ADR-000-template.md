# ADR-000 – [Kort titel på beslutet]
 
**Status:** Proposed  
**Datum:** YYYY-MM-DD  
**Författare:** [Namn eller team]
 
---
 
## Sammanhang (Context)
Beskriv kort vilket problem eller behov som ledde fram till detta beslut.  
Max 3–5 meningar.
 
---
 
## Beslut (Decision)
Sammanfatta vilket beslut som togs och varför detta alternativ valdes.  
Exempel: “Vi använder Azure Cosmos DB (Serverless) för att minimera kostnader och hantera dynamisk last.”
 
---
 
## Konsekvenser (Consequences)
Lista de huvudsakliga följderna av beslutet, både positiva och eventuella nackdelar.  
- Fördel:  
- Nackdel:  
 
---
 
## Risker / Åtgärder 
Identifiera eventuella risker kopplade till beslutet och hur de kan hanteras.  
- Risk:  
- Åtgärd:  
 
Exempel:  
- **Risk:** Databasen kan exponeras externt vid felaktig konfiguration.  
  **Åtgärd:** Begränsa åtkomst till privata nätverk och använd Managed Identity för autentisering.  
- **Risk:** För hög kostnad vid belastningstoppar.  
  **Åtgärd:** Inför requestbegränsning (rate limiting) och telemetriövervakning. 
 
---
 
## Alternativ (Alternatives)
Ange vilka alternativ som övervägdes men avfärdades, och varför.  
- Alternativ 1 – kort kommentar  
- Alternativ 2 – kort kommentar  
 
---
 
## Referenser (References)
Länka till relaterade dokument, PR:er, diskussioner eller externa källor.  
Exempel: [System Overview](../system_overview.md)
 