variable "resource_group_name" {
  description = "Name of the Azure resource group."
  type        = string
}

variable "location" {
  description = "Azure Government region (e.g. usgovvirginia)."
  type        = string
  default     = "usgovvirginia"
}

variable "app_name" {
  description = "Base name used for all resources (must be globally unique)."
  type        = string
}

variable "container_image_tag" {
  description = "Docker image tag to deploy (e.g. latest or a Git SHA)."
  type        = string
  default     = "latest"
}

variable "sku_name" {
  description = "App Service Plan SKU (e.g. B1, P1v3)."
  type        = string
  default     = "B1"
}

variable "tags" {
  description = "Tags to apply to all resources."
  type        = map(string)
  default     = {}
}
