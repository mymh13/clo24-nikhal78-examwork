# Infra (dev) – minimal start

Syfte: provisionera minsta möjliga Azure-resurs för att hosta Blazor Server landing page.

## Förkrav
- Azure CLI (`az`) inloggad mot rätt tenant/subscription
- Resursgrupp skapad (Bicep körs på RG-nivå)

## 1) Skapa resursgrupp (en gång)
```bash
az group create -n rg-examwork-dev -l swedencentral
