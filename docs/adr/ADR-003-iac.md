# ADR-003 – Infrastructure as Code (IaC) – verktygsval: Bicep

**Status:** Accepted  
**Datum:** 2025-10-30  
**Författare:** Niklas Häll

---

## Sammanhang (Context)
Projektet kräver ett verktyg för att definiera och reproducera infrastruktur i Azure på ett kontrollerat och versionshanterat sätt.  
Valet av IaC-verktyg påverkar både utvecklingshastighet, läsbarhet och hur enkelt lösningen kan integreras i CI/CD-flöden.  
Alternativen som övervägts är **Bicep**, **ARM-mallar** och **Terraform**.

---

## Beslut (Decision)
Vi använder **Bicep** som verktyg för Infrastructure as Code.  
Bicep ger en deklarativ syntax med hög läsbarhet och har **inbyggt stöd i Azure CLI och Azure DevOps**, vilket gör integrationen med befintliga pipelines och resurser smidig.  
Eftersom projektet redan utnyttjar flera Azure-tjänster (App Service, Cosmos DB, Key Vault, m.fl.) ger Bicep en naturlig passform och kräver ingen extra runtime eller externa beroenden.

---

## Konsekvenser (Consequences)
**Fördelar:**  
- Förstklassigt stöd i Azure CLI, utvecklarens IDE och DevOps.  
- Tydlig och deklarativ syntax som förenklar underhåll och kodgranskning.  
- Ingen extern konfiguration eller backend krävs (till skillnad från Terraform).  
- Lätt att bygga vidare på vid framtida drift i Azure.  

**Nackdelar:**  
- Mindre portabelt – svårt att flytta till andra molnplattformar. 
- Begränsat stöd för multi-cloud-scenarier.  
- ARM-mallar genereras i bakgrunden, vilket kan göra felsökning mer teknisk. (Bicep blir en overlay, ett "lager ovanpå")  

---

## Risker / Åtgärder
- **Risk:** Felaktiga Bicep-parametrar kan orsaka oönskade resursändringar.  
  **Åtgärd:** Inför validering via `what-if` (det görs lokalt via CLI innan deployment) i CI/CD-pipelines.  

- **Risk:** Begränsat stöd för icke-Azure-resurser.  
  **Åtgärd:** Behåll Terraform som potentiellt verktyg för framtida multi-cloud-expansion. Hade vi inte haft så extensivt användande av Azure-resurser i övrigt så hade vi valt Terraform över Bicep. 

---

## Alternativ (Alternatives)
- **Terraform:** Portabelt och väletablerat, men kräver backend (state-fil behöver lagras, men blir stor i stora projekt så det brukar hanteras via en remote backend) och extra konfiguration.  
- **ARM-mallar:** Direktstödda av Azure men svårare att läsa och underhålla.  
- **Pulumi:** Kraftfullt men onödigt komplext för detta projekt.  

---

## Referenser (References)
- [Systemöversikt](../system_overview.md)  
- [Microsoft Learn – Bicep documentation](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/)
