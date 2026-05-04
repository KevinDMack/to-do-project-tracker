# Deployment Guide

## Overview

The application is deployed as a Docker container to Azure App Service in Azure Government.  
The GitHub workflow handles automated build and deployment on every merge to `main`.

## First-Time Infrastructure Setup

### 1. Create the Terraform State Storage

```bash
./scripts/create-tf-state-rig.sh \
  <resource-group> \
  <storage-account-name> \
  <container-name> \
  usgovvirginia
```

### 2. Deploy Infrastructure

Set the required environment variables:

```bash
export TF_STATE_RG=<resource-group-for-tfstate>
export TF_STATE_SA=<storage-account-name>
export TF_STATE_CONTAINER=<container-name>
export TF_VAR_resource_group_name=<target-resource-group>
export TF_VAR_app_name=<unique-base-name>
```

Then run:

```bash
./scripts/deploy-terraform.sh
```

### 3. Configure Entra ID Authentication

Before setting GitHub secrets, complete the Entra ID app registration and obtain the required values.  
See [docs/entra-id.md](entra-id.md) for the full step-by-step guide.

### 4. Configure GitHub Secrets

Add the following secrets to your GitHub repository:

| Secret | Description |
|--------|-------------|
| `ACR_LOGIN_SERVER` | ACR login server (from Terraform output `acr_login_server`) |
| `AZURE_CLIENT_ID` | Managed identity or service principal client ID |
| `AZURE_TENANT_ID` | Entra ID tenant ID |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription ID |
| `AzureAd__TenantId` | Entra ID tenant ID for the app |
| `AzureAd__ClientId` | App registration client ID |
| `AzureAd__ClientSecret` | App registration client secret |
| `AzureAd__Domain` | Tenant domain (e.g. `contoso.onmicrosoft.com`) |

## Automated Deployment (GitHub Actions)

On every merge to `main`, the `deploy.yml` workflow:

1. Runs all unit tests.
2. Builds the Docker image and tags it with the Git SHA.
3. Pushes the image to GitHub Container Registry (ghcr.io).
4. Publishes the artifact with a package label connecting it to this repository.

## Updating Infrastructure

```bash
./scripts/deploy-terraform.sh --plan-only   # preview changes
./scripts/deploy-terraform.sh               # apply changes
```

## Destroying Infrastructure

```bash
./scripts/deploy-terraform.sh --destroy
```

> ⚠️ This will permanently delete all application resources. Terraform state is stored separately and is not affected.

## Terraform Outputs

After a successful `apply`, Terraform prints:

| Output | Description |
|--------|-------------|
| `web_app_url` | HTTPS URL of the deployed web application |
| `acr_login_server` | Container Registry login server |
| `storage_account_name` | Storage account name |
| `web_app_principal_id` | Managed identity principal ID |
