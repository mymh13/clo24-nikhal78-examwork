# ADR-002 – Autentisering: ASP.NET Identity + Entra ID

**Status:** Accepted  
**Datum:** 2025-10-30  
**Författare:** Niklas Häll

---

## Sammanhang (Context)
Systemet behöver stöd för två typer av användare: kunder som bokar biljetter och administratörer/inspektörer som hanterar resor och zoner.  
Lösningen måste vara kostnadseffektiv, säker och fungera både lokalt och i molnet utan att kräva licenser eller komplex konfiguration.

---

## Beslut (Decision)
Vi använder en **delad autentiseringsmodell**:  
- **Kunder** loggar in via **ASP.NET Core Identity**, där konton och lösenord hanteras lokalt i systemets databas.  
- **Administratörer och inspektörer** loggar in via **Azure Entra ID**, vilket möjliggör säker åtkomst till administrationsgränssnittet utan att exponera interna funktioner externt.  

Modellen kan enkelt utökas till federerad inloggning (t.ex. Entra External ID) vid behov, men hålls enkel i MVP-fasen.

---

## Konsekvenser (Consequences)
**Fördelar:**  
- Kostnadseffektivt: endast intern personal använder Entra ID.  
- Enkel för kunder: standardinloggning via e-post och lösenord.  
- Säker åtkomstkontroll för admin och inspektör.  
- Lätt att integrera med befintliga Azure-resurser och RBAC.  

**Nackdelar:**  
- Två autentiseringsvägar kräver tydlig rollhantering i koden.  
- Lokal Identity-hantering innebär ansvar för lösenordspolicy och säker lagring.  
- SSO-funktionalitet begränsas i MVP-fasen eftersom kunder autentiseras lokalt (ASP.NET Identity) och inte via en gemensam identitetsleverantör. Full SSO (Single Sign-On) kan införas senare genom att även kundflödet flyttas till Entra ID eller annan extern IdP.

---

## Risker / Åtgärder
- **Risk:** Felaktig hantering av rollbaserad åtkomst kan exponera adminfunktioner.  
  **Åtgärd:** Implementera rollkontroll i controllers och Razor-komponenter samt verifiera via testfall.  

- **Risk:** Lokala kundkonton kan utsättas för brute force-attacker.  
  **Åtgärd:** Aktivera inloggningsbegränsning (lockout policy) och kräva starka lösenord.  

- **Risk:** Entra ID-beroende kan skapa problem i offline-miljöer.  
  **Åtgärd:** Behåll fallback-läge lokalt för utveckling utan molnkoppling.

---

## Alternativ (Alternatives)
- **Endast Entra ID:** Säker men dyr och överkomplicerad för kundkonton.  
- **Endast lokal Identity:** Enkel men sämre säkerhet för adminfunktioner.  
- **OAuth2 med extern leverantör (t.ex. Google):** OAuth2/OIDC mot extern IdP (Google/Microsoft) bedöms som överdrivet för MVP eftersom målet främst är att visa arkitekturen, inte identity-federering.  

---

## Referenser (References)
- [Systemöversikt](../system_overview.md)  
- [Microsoft Docs – ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)  
- [Microsoft Docs – Entra ID integration](https://learn.microsoft.com/en-us/azure/active-directory/develop/)
