# Vecka 3 – Infrastruktur och CI/CD-grund

## Översikt

Under vecka 3 påbörjades arbetet med att etablera den första versionen av infrastrukturen och koppla samman projektet med CI/CD-pipelines.
Målet var att skapa en minimal men fungerande kedja för provisioning, autentisering och deployment, baserat på den befintliga kodbasen.

---

## Genomförda aktiviteter
 
- Regionval: Bekräftat att Sweden Central används som primär region för miljön, med fokus på hållbarhet och låg latens.
- Infrastruktur: Skapat första resursgruppen rg-examwork-dev och förberett struktur i infra/ för kommande Bicep-filer.
- Azure-integration:
    - Registrerat appen github-oidc-examwork i Entra ID.
    - Konfigurerat Federated credentials (OIDC) för GitHub Actions.
    - Tilldelat rollen Contributor till service principalen i resursgruppen.
- CI/CD:
    - Implementerat workflow OIDC Smoke Test (Azure login).
    - Genomfört lyckad körning – Azure OIDC-inloggning verifierad utan att exponera ID-data.
    - Förberett cd-web-dev.yaml för framtida deployment till App Service.

---

## Reflektion

Arbetet med OIDC gav en djupare förståelse för hur hemlighetsfri autentisering fungerar i en modern GitHub-Actions-pipeline.
Det visade sig viktigt att hålla en strikt separation mellan Tenant ID, Subscription ID och Client ID, särskilt vid användning av student-subskriptioner i Azure.
Nästa steg blir att skapa en minimal App Service och testa den första faktiska deploymenten.

---

## Nästa steg (vecka 4)

*(Planeras i slutet av veckan)*

Exempel:
- Utöka Bicep till att inkludera Cosmos DB och Key Vault.
- Testa första automatiserade deployment till App Service.
- Införa loggning och telemetri (Application Insights).
- Förbereda för API-containerisering.

---
 