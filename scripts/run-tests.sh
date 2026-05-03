#!/usr/bin/env bash
# run-tests.sh — Run all unit tests for the web application.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

echo "============================="
echo " Running TodoTracker Tests   "
echo "============================="

dotnet test \
  "${REPO_ROOT}/web-app-tests/TodoTracker.Tests/TodoTracker.Tests.csproj" \
  --configuration Release \
  --logger "console;verbosity=normal" \
  --results-directory "${REPO_ROOT}/test-results"

echo ""
echo "✅ All tests passed."
