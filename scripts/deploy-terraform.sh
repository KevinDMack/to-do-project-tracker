#!/usr/bin/env bash
# deploy-terraform.sh — Initialises and applies Terraform for Azure Government.
#
# Usage:
#   ./scripts/deploy-terraform.sh [--plan-only] [--destroy]
#
# Required environment variables:
#   TF_STATE_RG        — Resource group holding the Terraform state storage account
#   TF_STATE_SA        — Terraform state storage account name
#   TF_STATE_CONTAINER — Blob container name for state files
#   TF_VAR_resource_group_name — Target resource group for the application resources
#   TF_VAR_app_name             — Base name for all application resources

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
INFRA_DIR="$(cd "${SCRIPT_DIR}/../infra" && pwd)"

PLAN_ONLY=false
DESTROY=false

while [[ $# -gt 0 ]]; do
  case "$1" in
    --plan-only) PLAN_ONLY=true; shift ;;
    --destroy)   DESTROY=true;   shift ;;
    *) echo "Unknown option: $1"; exit 1 ;;
  esac
done

# Required environment checks
: "${TF_STATE_RG:?Set TF_STATE_RG to the resource group holding the Terraform state storage account}"
: "${TF_STATE_SA:?Set TF_STATE_SA to the Terraform state storage account name}"
: "${TF_STATE_CONTAINER:?Set TF_STATE_CONTAINER to the blob container name}"
: "${TF_VAR_resource_group_name:?Set TF_VAR_resource_group_name to the target resource group}"
: "${TF_VAR_app_name:?Set TF_VAR_app_name to the base name for application resources}"

# Target Azure Government
az cloud set --name AzureUSGovernment

echo "============================================="
echo " Deploying Terraform — Azure Government      "
echo "============================================="
echo "  Infra directory : ${INFRA_DIR}"
echo "  State RG        : ${TF_STATE_RG}"
echo "  State SA        : ${TF_STATE_SA}"
echo "  State Container : ${TF_STATE_CONTAINER}"
echo ""

cd "${INFRA_DIR}"

# Initialise with remote backend
terraform init \
  -backend-config="resource_group_name=${TF_STATE_RG}" \
  -backend-config="storage_account_name=${TF_STATE_SA}" \
  -backend-config="container_name=${TF_STATE_CONTAINER}" \
  -backend-config="key=terraform.tfstate" \
  -reconfigure

if [[ "${DESTROY}" == "true" ]]; then
  echo "⚠️  Destroying infrastructure..."
  terraform destroy -auto-approve
  echo "✅ Infrastructure destroyed."
  exit 0
fi

terraform plan -out=tfplan

if [[ "${PLAN_ONLY}" == "true" ]]; then
  echo "✅ Plan complete. Review tfplan before applying."
  exit 0
fi

terraform apply tfplan
echo "✅ Terraform apply complete."
