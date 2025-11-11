# ADR-000 – [Short decision title]
 
**Status:** Proposed  
**Date:** YYYY-MM-DD  
**Author:** [Name or team]
 
---
 
## Context
Briefly describe what problem or need led to this decision.  
Max 3–5 sentences.
 
---
 
## Decision
Summarize what decision was made and why this option was chosen.  
Example: "We use Azure Cosmos DB (Serverless) to minimize costs and handle dynamic load."
 
---
 
## Consequences
List the main consequences of the decision, both positive and potential drawbacks.  
- Advantage:  
- Disadvantage:  
 
---
 
## Risks / Mitigations 
Identify any risks associated with the decision and how they can be handled.  
- Risk:  
- Mitigation:  
 
Example:  
- **Risk:** The database may be exposed externally due to incorrect configuration.  
  **Mitigation:** Restrict access to private networks and use Managed Identity for authentication.  
- **Risk:** Too high cost during load peaks.  
  **Mitigation:** Implement request throttling (rate limiting) and telemetry monitoring. 
 
---
 
## Alternatives
List which alternatives were considered but rejected, and why.  
- Alternative 1 – brief comment  
- Alternative 2 – brief comment  
 
---
 
## References
Link to related documents, PRs, discussions, or external sources.  
Example: [System Overview](../system_overview.md)
 