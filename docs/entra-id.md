# Entra ID Authentication — Deployed Configuration

## Overview

The application uses **Microsoft Entra ID** (formerly Azure AD) with OpenID Connect via `Microsoft.Identity.Web`.  
In production the authentication settings are supplied through **Azure App Service Application Settings**, which ASP.NET Core automatically maps to configuration using the `__` (double-underscore) separator convention.

Because the infrastructure targets **Azure Government**, the Entra ID instance endpoint differs from the commercial endpoint.

---

## 1. Create the App Registration

> Use the **Azure Government** portal: <https://portal.azure.us>

1. Go to **Microsoft Entra ID → App registrations → New registration**.
2. Give the app a descriptive name (e.g. `TodoTracker-Prod`).
3. Under **Supported account types**, select **Accounts in this organizational directory only**.
4. Leave the Redirect URI blank for now; you will add it after the App Service is deployed.
5. Click **Register**.
6. Note the **Application (client) ID** and **Directory (tenant) ID** from the Overview page.

---

## 2. Add Redirect URIs

1. In the app registration, go to **Authentication → Add a platform → Web**.
2. Add the following Redirect URI (replace `<app-hostname>` with the actual App Service hostname, e.g. `mytodotracker-app.azurewebsites.us`):

   ```
   https://<app-hostname>/signin-oidc
   ```

3. Under **Front-channel logout URL**, add:

   ```
   https://<app-hostname>/signout-callback-oidc
   ```

4. Tick **ID tokens** under **Implicit grant and hybrid flows**.
5. Click **Save**.

---

## 3. Add API Permissions

1. In the app registration, go to **API permissions → Add a permission → Microsoft Graph → Delegated permissions**.
2. Add both of the following permissions:
   - `Tasks.ReadWrite`
   - `User.Read`
3. Click **Add permissions**.
4. Click **Grant admin consent for `<your tenant>`** and confirm.

---

## 4. Create a Client Secret

1. In the app registration, go to **Certificates & secrets → New client secret**.
2. Give it a description and choose an expiry period.
3. Click **Add** and immediately copy the **Value** — it is only shown once.

---

## 5. Configure App Service Application Settings

The application reads `AzureAd` configuration from environment variables at startup.  
Set the following **Application Settings** in the App Service (Azure Portal → App Service → Configuration → Application settings, or via the Azure CLI):

| Setting name | Value |
|---|---|
| `AzureAd__Instance` | `https://login.microsoftonline.us/` |
| `AzureAd__TenantId` | Directory (tenant) ID from the app registration |
| `AzureAd__ClientId` | Application (client) ID from the app registration |
| `AzureAd__ClientSecret` | Client secret value created in step 4 |
| `AzureAd__Domain` | Tenant domain, e.g. `contoso.onmicrosoft.us` |
| `AzureAd__CallbackPath` | `/signin-oidc` |
| `AzureAd__SignedOutCallbackPath` | `/signout-callback-oidc` |

> **Azure Government note:** The `AzureAd__Instance` value must be `https://login.microsoftonline.us/` (`.us`, not `.com`). The default value in `appsettings.json` points to the commercial endpoint and must be overridden here.

### Setting values with the Azure CLI

```bash
az webapp config appsettings set \
  --resource-group <resource-group> \
  --name <app-name> \
  --settings \
    "AzureAd__Instance=https://login.microsoftonline.us/" \
    "AzureAd__TenantId=<tenant-id>" \
    "AzureAd__ClientId=<client-id>" \
    "AzureAd__ClientSecret=<client-secret>" \
    "AzureAd__Domain=<domain>.onmicrosoft.us" \
    "AzureAd__CallbackPath=/signin-oidc" \
    "AzureAd__SignedOutCallbackPath=/signout-callback-oidc"
```

---

## 6. GitHub Secrets (CI/CD)

If your workflow or deployment scripts inject these values into the App Service, add the following secrets to your GitHub repository (**Settings → Secrets and variables → Actions**):

| Secret | Description |
|---|---|
| `AzureAd__TenantId` | Directory (tenant) ID |
| `AzureAd__ClientId` | Application (client) ID |
| `AzureAd__ClientSecret` | Client secret value |
| `AzureAd__Domain` | Tenant domain |

The `AzureAd__Instance` value (`https://login.microsoftonline.us/`) is not sensitive and can be hard-coded in the deployment workflow or Terraform variables.

---

## 7. (Optional) Pass Settings via Terraform

To manage the Entra ID settings as Terraform-controlled App Service settings, add variables to `infra/variables.tf` and extend the `app_settings` block in `infra/main.tf`:

```hcl
# infra/variables.tf (additions)
variable "entra_tenant_id"     { type = string; sensitive = true }
variable "entra_client_id"     { type = string; sensitive = true }
variable "entra_client_secret" { type = string; sensitive = true }
variable "entra_domain"        { type = string }
```

```hcl
# infra/main.tf — app_settings block (additions)
app_settings = {
  # ... existing settings ...
  "AzureAd__Instance"              = "https://login.microsoftonline.us/"
  "AzureAd__TenantId"              = var.entra_tenant_id
  "AzureAd__ClientId"              = var.entra_client_id
  "AzureAd__ClientSecret"          = var.entra_client_secret
  "AzureAd__Domain"                = var.entra_domain
  "AzureAd__CallbackPath"          = "/signin-oidc"
  "AzureAd__SignedOutCallbackPath" = "/signout-callback-oidc"
}
```

Supply the values at apply time using `-var` flags or a `terraform.tfvars` file (never commit secrets to source control).

---

## Troubleshooting

| Symptom | Likely cause | Fix |
|---|---|---|
| `AADSTS50011: The redirect URI … does not match` | Redirect URI in the app registration does not match the App Service URL | Re-check step 2; ensure the URI is exact including the path `/signin-oidc` |
| `AADSTS700016: Application not found in directory` | `AzureAd__ClientId` is wrong or the app registration was created in a different tenant | Verify the client ID and tenant ID |
| `IDX20803: Unable to obtain configuration from …login.microsoftonline.com` | `AzureAd__Instance` is pointing to the commercial endpoint | Set `AzureAd__Instance` to `https://login.microsoftonline.us/` (see step 5) |
| `401 Unauthorized` after sign-in | `Tasks.ReadWrite` or `User.Read` consent not granted | Re-check step 3 and grant admin consent |
| Sign-in loop / `correlation failed` | Cookie/session issue or mismatched callback path | Confirm `AzureAd__CallbackPath` and `AzureAd__SignedOutCallbackPath` match the registered URIs |
