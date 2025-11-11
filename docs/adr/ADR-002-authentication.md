# ADR-002 – Authentication: ASP.NET Identity + Entra ID

**Status:** Accepted  
**Date:** 2025-10-30  
**Author:** Niklas Häll

---

## Context
The system needs to support two types of users: customers who book tickets and administrators/inspectors who manage trips and zones.  
The solution must be cost-effective, secure, and work both locally and in the cloud without requiring licenses or complex configuration.

---

## Decision
We use a **shared authentication model**:  
- **Customers** log in via **ASP.NET Core Identity**, where accounts and passwords are managed locally in the system's database.  
- **Administrators and inspectors** log in via **Azure Entra ID**, which enables secure access to the administration interface without exposing internal functions externally.  

The model can easily be extended to federated login (e.g., Entra External ID) when needed, but is kept simple in the MVP phase.

---

## Consequences
**Advantages:**  
- Cost-effective: only internal staff use Entra ID.  
- Simple for customers: standard login via email and password.  
- Secure access control for admin and inspector.  
- Easy to integrate with existing Azure resources and RBAC.  

**Disadvantages:**  
- Two authentication paths require clear role management in the code.  
- Local Identity management entails responsibility for password policy and secure storage.  
- SSO functionality is limited in the MVP phase since customers authenticate locally (ASP.NET Identity) and not via a common identity provider. Full SSO (Single Sign-On) can be introduced later by also moving the customer flow to Entra ID or another external IdP.

---

## Risks / Mitigations
- **Risk:** Incorrect handling of role-based access can expose admin functions.  
  **Mitigation:** Implement role control in controllers and Razor components and verify via test cases.  

- **Risk:** Local customer accounts can be exposed to brute force attacks.  
  **Mitigation:** Enable login throttling (lockout policy) and require strong passwords.  

- **Risk:** Entra ID dependency can create problems in offline environments.  
  **Mitigation:** Maintain fallback mode locally for development without cloud connectivity.

---

## Alternatives
- **Entra ID only:** Secure but expensive and overcomplicated for customer accounts.  
- **Local Identity only:** Simple but poorer security for admin functions.  
- **OAuth2 with external provider (e.g., Google):** OAuth2/OIDC against external IdP (Google/Microsoft) is considered excessive for MVP since the goal is primarily to demonstrate the architecture, not identity federation.  

---

## References
- [System overview](../system_overview.md)  
- [Microsoft Docs – ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)  
- [Microsoft Docs – Entra ID integration](https://learn.microsoft.com/en-us/azure/active-directory/develop/)
