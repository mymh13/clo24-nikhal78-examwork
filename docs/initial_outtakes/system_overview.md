## Systemöversikt
### Syfte
  
Detta projekt syftar till att utveckla ett kostnadseffektivt och händelsestyrt biljettsystem för kollektivtrafik. Applikationen gör det möjligt för kunder att se tillgängliga resor, köpa och avboka biljetter samt för administratörer att hantera rutter och zoner.  
  
Systemet byggs stegvis: först som ett enkelt, synkront och resurseffektivt system, med möjlighet att senare aktivera händelsestyrda flöden utan att ändra grundarkitekturen.  
  
### Teknisk översikt
  
Systemet använder .NET 8 Blazor Server som frontend och ett Controller-baserat API för affärslogik och datalagring. All data lagras i Azure Cosmos DB (Serverless) för att minimera kostnader vid låg belastning.
Funktioner som funktionsflaggor, hemlighetshantering och telemetri är integrerade redan från början för att möjliggöra kontroll och övervakning.  
  
#### Azure-tjänster
| Tjänst                     | Syfte                                                                        | 
| -------------------------- | ---------------------------------------------------------------------------- | 
| **App Service**            | Kör Blazor Server och API-applikationen.                                     | 
| **Cosmos DB (Serverless)** | Primär databas för resor, bokningar och zoner.                               | 
| **Azure Function**         | Används för bakgrundsjobb som triggas av händelser (t.ex. `BookingCreated`). | 
| **Service Bus**            | Kö för asynkrona händelser (inaktiverad i MVP).                              | 
| **App Configuration**      | Lagrar feature-flags och miljökonfiguration.                                 | 
| **Key Vault**              | Hanterar secrets och anslutningssträngar.                                    | 
| **Application Insights**   | Samlar loggar, telemetri och prestandadata.                                  | 
| **API Management (APIM)**  | Gateway för publika GET-endpoints (läs-only-data).                           | 
  
#### .NET-komponenter och verktyg
| Komponent                       | Syfte                                                              | 
| ------------------------------- | ------------------------------------------------------------------ | 
| **Blazor Server**               | Användargränssnitt för kunder och administratörer.                 | 
| **ASP.NET Controller API**      | Hanterar affärslogik och dataåtkomst.                              | 
| **xUnit + NSubstitute**         | Enhetstestning.                                                    | 
| **Entity / Repository Pattern** | Abstraktion för datalagring i Cosmos DB.                           | 
| **Outbox Pattern**              | Förbereder systemet för händelsestyrd publicering via Service Bus. | 
   
#### DevOps och infrastruktur  
 
| Område | Beskrivning  |
|-----------------------------------|-----------------------------------------------------------------------------------------------------------|
| **CI/CD**                         | Hanteras via Azure DevOps eller GitHub Actions med YAML-pipelines för automatiska byggen och deployment.  |
| **Infrastructure as Code (IaC)**  | Infrastruktur kan hanteras med **Bicep**, **ARM-mallar** eller **Terraform**.
    Exakt verktyg beslutas senare, men fokus ligger på att skapa en reproducerbar och kostnadseffektiv miljö.                                   |
| **Miljöer**                       | Två miljöer planeras: `dev` och `prod`, separerade i egna Resource Groups i Azure.                        |
| **Loggning och övervakning**      | Implementeras med **Application Insights** och **KQL-frågor** 
    för att visualisera nyckelvärden i dashboards eller workbooks.                                                                              |
  
### MVP-omfattning
  
- Book and cancel trips (Bus, Train).
- Display available routes and zones.
- Manage trips via admin view.
- Track performance and usage via telemetry.
- Prepare (but not enable) event flow through Service Bus and Functions.
  
### Möjliga framtida utökningar
  
- Enable Service Bus + Function pipeline for asynchronous events.
- Add ticket validation module (Inspector role).
- Expand zone-based pricing and peak-hour rules.
- Real-time metrics dashboard in Grafana.