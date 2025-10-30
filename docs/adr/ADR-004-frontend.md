# ADR-004 – Val av frontend-teknik: .NET 8 Blazor Server

**Status:** Accepted  
**Datum:** 2025-10-30  
**Författare:** Niklas Häll

---

## Sammanhang (Context)
Projektet ska efterlikna en befintlig lösning där Microsoft-/Azure-stack används fullt ut.  
Frontend behöver:
- kunna integrera mot .NET-baserade API:er utan extra lager,
- stödja inloggning för både interna roller (admin/inspektör via Entra ID) och vanliga användare,
- kunna köras lokalt för att hålla nere kostnader under utveckling,
- kunna deployas enkelt till Azure App Service.

Alternativ som React, Angular och Vue skulle fungera tekniskt, men skulle kräva separat byggkedja (npm) och ge onödig komplexitet för ett 8-veckors MVP.  
HTMX hade däremot kunnat vara ett attraktivt alternativ tack vare sin filosofi kring **Hypermedia-Driven Applications (HDA)**, där interaktionen styrs av servern och inte av en tung frontend.  
Det hade passat väl för ett biljettbokningssystem där logiken redan finns i backend, och där låg komplexitet, snabb utveckling och tydlig server-state är viktiga mål.  
HTMX ger ett modernt, responsivt gränssnitt utan att bygga en SPA, men saknar den djupa integration som Blazor erbjuder i en ren .NET-miljö.

---

## Beslut (Decision)
Vi använder **.NET 8 Blazor Server** som frontendramverk för applikationen.  
Det ger en hel-C# lösning där UI, affärslogik och autentisering kan ligga nära varandra och där realtidsuppdateringar (SignalR) hanteras av plattformen.  
Blazor Server kan även köras lokalt under utveckling för att minska användningen av CI/CD-minuter och Azure-resurser.

---

## Konsekvenser (Consequences)
**Fördelar:**
- Samma teknikstack (.NET) i hela projektet → enklare onboarding och färre verktyg.
- Enkel integration med ASP.NET Identity och Entra ID.
- Serverdrivet UI → lättare att skydda adminvyer bakom inloggning.
- Lämpligt för snabb MVP-utveckling (ingen separat SPA-byggkedja).
- Lätt att hosta i Azure App Service.

**Nackdelar:**
- Kräver fungerande och relativt stabil server-anslutning (SignalR).
- Kan bli dyrare i längden än statisk Blazor WASM om man har många samtidiga användare.
- UI-prestanda påverkas mer av servern än i en ren klientapp.

---

## Risker / Åtgärder
- **Risk:** App Service på Free/F1 kan “somna”, vilket påverkar SignalR-anslutningen.  
  **Åtgärd:** Skala upp till B1 inför demo eller när systemet visas för extern part.

- **Risk:** För tung logik i Blazor-komponenter kan göra UI långsamt.  
  **Åtgärd:** Flytta affärslogik till API/domänlager och låt Blazor endast anropa tjänster.

- **Risk:** Framtida behov av publik, helt anonym trafik (t.ex. öppen biljettvisning) kan passa sämre i Server-modellen.  
  **Åtgärd:** Öppna för kompletterande Razor Pages / statiska vyer eller framtida Blazor WASM-modul.

---

## Alternativ (Alternatives)
- **Blazor WebAssembly:** Bra för statisk hosting, men mer jobb för auth och API-säkerhet i MVP.
- **React/Angular:** Väletablerat, men kräver egen byggkedja och bryter “ren .NET-stack”. Ökad komplexitet och stort underhållsbehov.
- **Razor Pages / MVC Views:** Enklare, men sämre för framtida interaktioner och realtid.
- **HTMX:** Modernt och resurssnålt, ger snabb och serverdriven interaktion utan tung frontend-stack. Passar väl för applikationer med stark backend-logik, men är ännu inte lika etablerat i .NET-ekosystemet och har svagare stöd i Azure-verktygskedjan. 

---

## Referenser (References)
- [Systemöversikt](../system_overview.md)
- [Microsoft Docs – Host and deploy Blazor Server](https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/server)

