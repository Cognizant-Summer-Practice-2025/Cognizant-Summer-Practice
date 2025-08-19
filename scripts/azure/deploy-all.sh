#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

TAG="${1:-latest}"

#"$SCRIPT_DIR/az-setup.sh"
#"$SCRIPT_DIR/setup-postgres-flex.sh" || echo "Postgres Flex setup failed; continuing"

set +e
# Skip containerized DBs when using Azure Flex Servers
"$SCRIPT_DIR/deploy-backend-user.sh" "$TAG" || echo "backend-user failed; continuing"
"$SCRIPT_DIR/deploy-backend-portfolio.sh" "$TAG" || echo "backend-portfolio failed; continuing"
"$SCRIPT_DIR/deploy-backend-messages.sh" "$TAG" || echo "backend-messages failed; continuing"
"$SCRIPT_DIR/deploy-backend-ai.sh" "$TAG" || echo "backend-ai failed; continuing"
"$SCRIPT_DIR/deploy-frontend-auth-user.sh" "$TAG" || echo "auth-user-service failed; continuing"
"$SCRIPT_DIR/deploy-frontend-home-portfolio.sh" "$TAG" || echo "home-portfolio-service failed; continuing"
"$SCRIPT_DIR/deploy-frontend-messages.sh" "$TAG" || echo "messages-service failed; continuing"
"$SCRIPT_DIR/deploy-frontend-admin.sh" "$TAG" || echo "admin-service failed; continuing"
set -e

echo "Done."


