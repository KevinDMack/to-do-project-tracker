output "web_app_url" {
  description = "Default hostname of the deployed Web App."
  value       = "https://${azurerm_linux_web_app.app.default_hostname}"
}

output "acr_login_server" {
  description = "Container Registry login server URL."
  value       = azurerm_container_registry.acr.login_server
}

output "storage_account_name" {
  description = "Storage account name."
  value       = azurerm_storage_account.sa.name
}

output "web_app_principal_id" {
  description = "System-assigned managed identity principal ID for the Web App."
  value       = azurerm_linux_web_app.app.identity[0].principal_id
}
