# Projektöversikt för dokumentationen

Denna katalog innehåller all dokumentation kopplad till projektet – från tidiga planer till beslutade arkitekturval och veckovisa loggar.

---

### ./rot-mappen
|       Fil                     |  Beskrivning  |
|-------------------------------|---------------|
| **glossary.md**               | (Valfritt) Ordlista över centrala begrepp och akronymer som används i projektet. |

---

### ./adr/                      ADR - Achitecture Design Record. Dokumenterar design- och arkitekturella beslut.
|       Fil                     |  Beskrivning  |
|-------------------------------|---------------|
| **./archive/**                | Arkiv för äldre eller ersatta ADR-dokument. |
| **ADR-000-template.md**       | Mall för nya Architecture Decision Records. |
| **ADR-001-cosmosdb.md**       | Beslut: Databasval (Azure Cosmos DB, Serverless). |
| **ADR-002-authentication.md** | Beslut: Autentisering via ASP.NET Identity + Entra ID. |
| **ADR-003-iac.md**            | Beslut: Infrastructure as Code med Bicep. |
| **ADR-004-frontend.md**       | Beslut: Frontend i .NET 8 Blazor Server. |
| **ADR-005-azureservices.md**  | Beslut: Centrala Azure-tjänster för drift och övervakning. |
| **ADR-006-eventdriven.md**    | Planerat beslut: Eventdriven arkitektur (Service Bus + Function). |

---

### ./initial_outtakes/         Tidiga utkast och planer som definierade projektets riktning.

|       Fil                     |  Beskrivning  |
|-------------------------------|---------------|
| **system_overview.md**        | Systemöversikt med projektmål, teknisk plan och tjänsteval. |
| **architecture.md**           | Arkitekturell översikt med ASCII-diagram och flödesbeskrivning. |

---

### ./journal/                  Veckovisa loggar för projektets framsteg.

|       Fil                     |  Beskrivning  |
|-------------------------------|---------------|
| **week_one.md**               | Vecka 1 – uppstart, planering och dokumentationsstruktur. |

---

> **Notering:**  
> Dokumentationen uppdateras iterativt under projektets gång. Nya ADR:er och journalposter läggs till successivt när beslut fattas eller milstolpar nås.
