#!/usr/bin/env bash
# build-docker-images.sh — Build and optionally push the TodoTracker Docker image.
#
# Usage:
#   ./scripts/build-docker-images.sh [--push] [--registry <acr-login-server>] [--tag <tag>]
#
# Environment variables (override flags):
#   REGISTRY   — ACR login server (e.g. myacr.azurecr.us)
#   IMAGE_TAG  — Docker image tag (default: latest)

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

PUSH=false
REGISTRY="${REGISTRY:-}"
IMAGE_TAG="${IMAGE_TAG:-latest}"

# Parse CLI flags
while [[ $# -gt 0 ]]; do
  case "$1" in
    --push)      PUSH=true; shift ;;
    --registry)  REGISTRY="$2"; shift 2 ;;
    --tag)       IMAGE_TAG="$2"; shift 2 ;;
    *) echo "Unknown option: $1"; exit 1 ;;
  esac
done

IMAGE_NAME="todotracker"
FULL_IMAGE="${IMAGE_NAME}:${IMAGE_TAG}"

if [[ -n "${REGISTRY}" ]]; then
  FULL_IMAGE="${REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG}"
fi

echo "Building Docker image: ${FULL_IMAGE}"

docker build \
  -t "${FULL_IMAGE}" \
  -f "${REPO_ROOT}/web-apps/Dockerfile" \
  "${REPO_ROOT}/web-apps"

echo "✅ Image built: ${FULL_IMAGE}"

if [[ "${PUSH}" == "true" ]]; then
  if [[ -z "${REGISTRY}" ]]; then
    echo "❌ --registry must be specified when using --push" >&2
    exit 1
  fi
  echo "Pushing image to ${REGISTRY}..."
  docker push "${FULL_IMAGE}"
  echo "✅ Image pushed: ${FULL_IMAGE}"
fi
