#!/usr/bin/env bash
# create-tf-state-rig.sh — Creates the Azure storage account and container used for Terraform remote state.
#
# Usage:
#   ./scripts/create-tf-state-rig.sh <resource-group> <storage-account-name> <container-name> [location]
#
# Defaults:
#   location = usgovvirginia

set -euo pipefail

RESOURCE_GROUP="${1:?Resource group name required as first argument}"
STORAGE_ACCOUNT="${2:?Storage account name required as second argument}"
CONTAINER_NAME="${3:?Container name required as third argument}"
LOCATION="${4:-usgovvirginia}"

echo "========================================"
echo " Creating Terraform State Storage Rig   "
echo "========================================"
echo "  Resource Group   : ${RESOURCE_GROUP}"
echo "  Storage Account  : ${STORAGE_ACCOUNT}"
echo "  Container        : ${CONTAINER_NAME}"
echo "  Location         : ${LOCATION}"
echo ""

# Ensure we target Azure Government
az cloud set --name AzureUSGovernment

# Create resource group if it doesn't exist
az group create \
  --name "${RESOURCE_GROUP}" \
  --location "${LOCATION}" \
  --output none

echo "✅ Resource group ready: ${RESOURCE_GROUP}"

# Create storage account
az storage account create \
  --name "${STORAGE_ACCOUNT}" \
  --resource-group "${RESOURCE_GROUP}" \
  --location "${LOCATION}" \
  --sku Standard_LRS \
  --encryption-services blob \
  --output none

echo "✅ Storage account ready: ${STORAGE_ACCOUNT}"

# Create blob container
az storage container create \
  --name "${CONTAINER_NAME}" \
  --account-name "${STORAGE_ACCOUNT}" \
  --auth-mode login \
  --output none

echo "✅ Container ready: ${CONTAINER_NAME}"
echo ""
echo "Use these backend-config values in deploy-terraform.sh:"
echo "  resource_group_name  = ${RESOURCE_GROUP}"
echo "  storage_account_name = ${STORAGE_ACCOUNT}"
echo "  container_name       = ${CONTAINER_NAME}"
echo "  key                  = terraform.tfstate"
