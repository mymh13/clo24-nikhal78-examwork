# Infra (dev) – minimal start

Purpose: provision the smallest possible Azure resource to host the Blazor Server landing page.

## Prerequisites
- Azure CLI (`az`) logged in to the correct tenant/subscription
- Resource group created (Bicep runs at RG level)

## 1) Create resource group (once)
```bash
az group create -n rg-examwork-dev -l swedencentral
```