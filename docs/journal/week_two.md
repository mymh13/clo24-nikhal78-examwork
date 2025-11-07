# Vecka 2 – Kodstruktur och första körbara version

## Översikt

Under vecka 2 har fokus legat på att gå från planeringsfas till körbar kodbas.
Målet var att etablera den tekniska grunden för både backend och frontend samt verifiera att applikationen fungerar lokalt innan infrastruktur och CI/CD påbörjas.

---

## Genomförda aktiviteter

* **Solution och projektstruktur**

  * Skapat huvudlösning `clo24-nikhal78-examwork.sln` i projektroten.
  * Lagt till projekten `Ticketing.Api` och `Ticketing.Contracts` i solution.
  * Säkrat referenser mellan projekten (`API → Contracts`).
  * Fastställt permanent repo-struktur:

    ```
    src/
    ├─ api/
    ├─ web/
    ├─ shared/
    ├─ functions/
    └─ tests/
    ```

* **Shared-projekt**
  * Skapat `Ticketing.Contracts.csproj` (class library, net8.0).
  * Förberett `BookingDto.cs` enligt dokumenterade domänfält i `cosmos-ticketing-considerations.md`.

* **API-projekt**
  * Skapat `Ticketing.Api.csproj` med Web SDK och referens till `Contracts`.
  * Implementerat minimal `Program.cs` för körbar Web API-bas.
  * Bekräftat lyckad kompilering och lokal körning.

* **Web-projekt (Blazor Server)**
  * Skapat `Ticketing.Web.csproj` (Blazor Server, net8.0).
  * Lagt till minimal `Program.cs`, `_Host.cshtml`, `App.razor`, `_Imports.razor`, `MainLayout.razor` och `Index.razor`.
  * Säkrat att applikationen körs lokalt och renderar en fungerande **landing page**.
  * Hanterat initiala fel kring `_Host` och `PageTitle` genom att komplettera projektstrukturen.
  
* **Övrigt**
  * Övervägt designval kring **Entra ID** (för adminroller) och **containerstrategi** (för API, Web och Functions).
  * Beslutat att containerisering sker i samband med CI/CD och inte under MVP-fasen.
  * Fastställt regionval:
    * **Sweden Central (`swedencentral`)** som primär region (hållbarhet + låg latens)
    * **West Europe (`westeurope`)** som fallback-region

---

## Reflektion

Grunden för applikationen är nu körbar och följer en ren, modulär arkitektur.
Arbetet har fortsatt enligt principen *“en fil i taget”*, vilket minimerat fel och gjort det möjligt att verifiera varje steg lokalt innan nästa introducerats.
Blazor Server-fronten och API:t ligger redo att byggas ut med funktionalitet för bokningar och datalagring.

---

## Nästa steg (vecka 3)

* Skapa **infra/**-struktur med Bicep och definiera Azure-resurser (App Service, Cosmos DB, Key Vault, App Config).
* Förbereda **GitHub Actions-workflows** för CI (build) och CD (deploy till dev).
* Utvärdera **Dockerfile** för API och Web inför kommande containerisering.
* Bekräfta lyckad provisioning i **Sweden Central** innan utökning av infrastrukturen.

---
