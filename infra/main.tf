locals {
  base_name = lower(replace(var.app_name, " ", "-"))
}

# ── Resource Group ────────────────────────────────────────────────────────────
data "azurerm_resource_group" "rg" {
  name = var.resource_group_name
}

# ── Container Registry ────────────────────────────────────────────────────────
resource "azurerm_container_registry" "acr" {
  name                = "${replace(local.base_name, "-", "")}acr"
  resource_group_name = data.azurerm_resource_group.rg.name
  location            = data.azurerm_resource_group.rg.location
  sku                 = "Basic"
  admin_enabled       = false

  identity {
    type = "SystemAssigned"
  }

  tags = var.tags
}

# ── Storage Account ───────────────────────────────────────────────────────────
resource "azurerm_storage_account" "sa" {
  name                     = "${replace(local.base_name, "-", "")}sa"
  resource_group_name      = data.azurerm_resource_group.rg.name
  location                 = data.azurerm_resource_group.rg.location
  account_tier             = "Standard"
  account_replication_type = "LRS"

  identity {
    type = "SystemAssigned"
  }

  tags = var.tags
}

# ── App Service Plan ──────────────────────────────────────────────────────────
resource "azurerm_service_plan" "plan" {
  name                = "${local.base_name}-plan"
  resource_group_name = data.azurerm_resource_group.rg.name
  location            = data.azurerm_resource_group.rg.location
  os_type             = "Linux"
  sku_name            = var.sku_name

  tags = var.tags
}

# ── Web App ───────────────────────────────────────────────────────────────────
resource "azurerm_linux_web_app" "app" {
  name                = "${local.base_name}-app"
  resource_group_name = data.azurerm_resource_group.rg.name
  location            = data.azurerm_resource_group.rg.location
  service_plan_id     = azurerm_service_plan.plan.id

  identity {
    type = "SystemAssigned"
  }

  site_config {
    application_stack {
      docker_image_name        = "${azurerm_container_registry.acr.login_server}/todotracker:${var.container_image_tag}"
      docker_registry_url      = "https://${azurerm_container_registry.acr.login_server}"
    }
  }

  app_settings = {
    WEBSITES_PORT                       = "8080"
    DOCKER_REGISTRY_SERVER_URL          = "https://${azurerm_container_registry.acr.login_server}"
    DOCKER_ENABLE_CI                    = "true"
    AZURE_STORAGE_ACCOUNT_NAME          = azurerm_storage_account.sa.name
    ASPNETCORE_ENVIRONMENT              = "Production"
  }

  tags = var.tags
}

# ── RBAC: Web App → Container Registry (AcrPull) ─────────────────────────────
resource "azurerm_role_assignment" "app_acr_pull" {
  scope                = azurerm_container_registry.acr.id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_linux_web_app.app.identity[0].principal_id
}

# ── RBAC: Web App → Storage Account (Storage Blob Data Contributor) ───────────
resource "azurerm_role_assignment" "app_storage_blob" {
  scope                = azurerm_storage_account.sa.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_linux_web_app.app.identity[0].principal_id
}
