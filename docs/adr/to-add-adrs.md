add the following adrs:

1. Application Insights / Telemetry Strategy - **complete**

2. Managed Identity & RBAC Strategy - **complete**
Why needed: All services use managed identity with RBAC (no connection strings)
What to document: Decision to use managed identity everywhere, RBAC role assignments, security benefits, operational considerations
Current state: Briefly mentioned in ADR-005 but not a dedicated decision

3. CI/CD Pipeline Strategy - **complete** (covered in ADR-003 and ADR-008)

4. Service/Component Organization Pattern - **complete**

5. Error Handling & Logging Strategy - **complete**

6. API Design Pattern (Controller-based REST) - **complete**
Lower Priority (Nice to Have)

7. Testing Strategy - Testing was always considered but was not added (yet), thus I think we should wait with this until we actually add testing
Why needed: If testing approach is defined (unit, integration, etc.)
What to document: Testing philosophy, what is/isn't tested, testing tools
Current state: Not documented

8. Data Model / Domain Design - Was domain modelling decisions taken re: the Function or Azure implementations? Then it would warrant an ADR but otherwise I think this might be left hanging until actual implementation, like the Testing Strategy
Why needed: If there are specific domain modeling decisions
What to document: Entity design, relationships, data access patterns
Current state: Partially covered in ADR-001 (Cosmos DB) but not domain modeling