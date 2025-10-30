# ADR-005 – Val av molntjänster: Azure App Service, App Configuration, Key Vault, Application Insights och API Management

**Status:** Accepted  
**Datum:** 2025-10-30  
**Författare:** Niklas Häll

---

## Sammanhang (Context)
Systemet byggs på Azure-plattformen och behöver flera kompletterande tjänster för att hantera drift, säkerhet, konfiguration och övervakning.  
Målet är att skapa en lösning som:
- är **billig att köra** under utveckling (Free eller Serverless-planer),
- **enkel att skala upp** inför demonstration eller produktion, och
- **förberedd för CI/CD** och framtida DevOps-integration.

Eftersom organisationen redan använder Azure som huvudplattform är det naturligt att även nyttja Microsofts egna molntjänster för infrastruktur, konfiguration och loggning.

---

## Beslut (Decision)
Följande Azure-tjänster används i MVP och framtida utbyggnad:

|          Tjänst           |               Syfte                                                          |
|---------------------------|------------------------------------------------------------------------------|
| **App Service**           | Kör Blazor Server och API-applikationen i samma App Service-plan (Free/B1).  |
| **App Configuration**     | Centraliserad hantering av inställningar och feature-flaggor mellan miljöer. |
| **Key Vault**             | Säker lagring av secrets och anslutningssträngar.                            |
| **Application Insights**  | Loggning, övervakning och prestandamätning via KQL.                          |
| **API Management (APIM)** | Gateway för publika GET-endpoints, caching och rate limiting.                |

---

## Konsekvenser (Consequences)
**Fördelar:**
- Full integration med övriga Azure-tjänster och DevOps-flöden.  
- Låg instegskostnad (F1/Serverless).  
- Centraliserad och säker hantering av konfiguration och secrets.  
- Enkel övergång till produktion via skalning i App Service-planen.  
- APIM ger möjlighet att framtidssäkra API:erna med versionering och åtkomstkontroll.

**Nackdelar:**
- Låst till Azure-ekosystemet.  
- Begränsade resurser i F1-planer (kan “somna” vid inaktivitet).  
- APIM i Consumption-läge kan bli långsammare än direkt API-access.  

---

## Risker / Åtgärder
- **Risk:** App Service i F1-läge stängs av vid inaktivitet.  
  **Åtgärd:** Uppgradera till B1 inför demo eller högre last.  

- **Risk:** Felaktig hantering av secrets kan leda till exponering.  
  **Åtgärd:** Lagra alla anslutningssträngar i Key Vault och använd Managed Identity för åtkomst.  

- **Risk:** Onödiga kostnader vid test av flera tjänster samtidigt.  
  **Åtgärd:** Aktivera endast nödvändiga resurser under MVP, stäng av Service Bus/Function tills eventflödet ska demonstreras.  

---

## Alternativ (Alternatives)
- **Container-baserad drift (Azure Container Apps eller Azure Kubernetes Service):** Kraftfullt men överdimensionerat för MVP.  
- **Statisk hosting (t.ex. Blazor WASM + Blob Storage):** Billigt, men saknar stöd för serverbaserad inloggning och realtid. 
- **Extern loggning (t.ex. Grafana Cloud, Elastic):** Flexibelt men ökar komplexitet och driftkostnad.  

---

## Referenser (References)
- [Systemöversikt](../system_overview.md)  
- [Microsoft Docs – Azure App Service](https://learn.microsoft.com/en-us/azure/app-service/overview)  
- [Microsoft Docs – Azure App Configuration](https://learn.microsoft.com/en-us/azure/azure-app-configuration/overview)  
- [Microsoft Docs – Azure Key Vault](https://learn.microsoft.com/en-us/azure/key-vault/general/basic-concepts)  
- [Microsoft Docs – Application Insights](https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)  
- [Microsoft Docs – API Management](https://learn.microsoft.com/en-us/azure/api-management/api-management-key-concepts)
