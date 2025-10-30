# Ordlista (Glossary)

Denna ordlista samlar centrala begrepp, förkortningar och termer som används i projektet.  
Syftet är att underlätta förståelsen av tekniska dokument och beslut, särskilt för läsare som inte är insatta i Azure- eller .NET-ekosystemet.

---

## Azure-tjänster

| Begrepp / Akronym         | Förklaring                                            | Kommentar / Användning i projektet |
|---------------------------|-------------------------------------------------------|------------------------------------|
| **App Service**           | Azure-tjänst för att köra webbappar och API:er.       | Kör både Blazor Server och API-delen i MVP. |
| **App Configuration**     | Central lagring för inställningar och feature-flaggor.| Styr aktivering av funktioner och miljökonfiguration. |
| **Key Vault**             | Säker lagring av hemligheter (secrets).               | Används för anslutningssträngar och API-nycklar. |
| **Cosmos DB**             | Distribuerad NoSQL-databas från Azure.                | Primär databas i MVP (Serverless-läge). |
| **Service Bus**           | Meddelandekö för asynkron kommunikation.              | Används i framtida eventdrivet flöde. |
| **Azure Function**        | Serverless-funktion för händelsestyrd logik.          | Konsumerar event (t.ex. `BookingCreated`). |
| **Application Insights**  | Tjänst för loggning, övervakning och telemetri.       | Spårar händelser och prestanda i applikationen. |
| **API Management (APIM)** | Gateway för publika endpoints.                        | Hanterar GET-anrop, caching och rate limiting. |

---

## .NET-komponenter

| Begrepp / Akronym      | Förklaring                                            | Kommentar / Användning i projektet |
|------------------------|-------------------------------------------------------|------------------------------------|
| **Blazor Server**      | Webbramverk i .NET för serverdrivna gränssnitt.       | Bygger frontend och hanterar inloggning. |
| **ASP.NET Identity**   | Ramverk för användarhantering och autentisering.      | Används för kundinloggning. |
| **Entra ID**           | Tidigare Azure Active Directory (AAD).                | Hanterar autentisering för admin/inspektör. |
| **xUnit / NSubstitute**| Testverktyg för .NET-projekt.                         | Används för enhetstestning. |

---

## Infrastruktur och DevOps

| Begrepp / Akronym      | Förklaring                                            | Kommentar / Användning i projektet |
|------------------------|-------------------------------------------------------|------------------------------------|
| **IaC**                | *Infrastructure as Code.*                             | Princip för att definiera infrastruktur i kod (Bicep). |
| **Bicep**              | Microsofts IaC-språk för Azure.                       | Används för att deklarativt skapa resurser. |
| **CI/CD**              | *Continuous Integration / Continuous Deployment.*     | Automatiska byggen och leveranser via YAML-pipelines. |
| **YAML**               | Konfigurationsspråk.                                  | Används för pipelines i DevOps. |

---

## Arkitekturmönster

| Begrepp / Akronym          | Förklaring                                               | Kommentar / Användning i projektet |
|----------------------------|----------------------------------------------------------|------------------------------------|
| **Outbox Pattern**         | Designmönster för tillförlitlig publicering av händelser.| Förbereds i API:t för eventflöden. |
| **Eventdriven arkitektur** | Arkitekturmönster där komponenter reagerar på händelser. | Införs efter MVP via Service Bus + Function. |

---

## Övriga begrepp

| Begrepp / Akronym      | Förklaring                                             | Kommentar / Användning i projektet |
|------------------------|--------------------------------------------------------|------------------------------------|
| **ADR**                | *Architecture Decision Record.*                        | Dokument som motiverar och loggar tekniska beslut. |
| **HDA**                | *Hypermedia-Driven Application.*                       | Arkitekturstil som HTMX bygger på. |
| **HTMX**               | JavaScript-bibliotek för serverdriven interaktion.     | Alternativ till SPA-ramverk (ej valt i detta projekt). |
| **MVP**                | *Minimum Viable Product.*                              | Första fungerande versionen av systemet. |

---

> **Notering:**  
> Ordlistan uppdateras löpande i takt med att nya tekniska begrepp och designmönster introduceras i projektet.
