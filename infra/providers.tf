terraform {
  required_version = ">= 1.5.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.110"
    }
  }

  backend "azurerm" {
    # Populated by create-tf-state-rig.sh / deploy-terraform.sh via -backend-config flags
  }
}

provider "azurerm" {
  environment = "usgovernment"

  features {}

  # Uses the default logged-in Azure CLI user — no service principal needed.
  use_cli = true
}
