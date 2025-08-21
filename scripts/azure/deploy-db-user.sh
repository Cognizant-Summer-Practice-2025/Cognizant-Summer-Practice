#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/.state/azure.env"

# Optionally load user env overrides
if [[ -f "$SCRIPT_DIR/.env" ]]; then
  while IFS= read -r line || [ -n "$line" ]; do
    case "$line" in
      ''|\#*) continue ;;
    esac
    if [[ "$line" =~ ^[A-Za-z_][A-Za-z0-9_]*= ]]; then
      export "$line"
    fi
  done < "$SCRIPT_DIR/.env"
fi

IMAGE_TAG="${1:-latest}"
APP_NAME="user-db"
DOCKERFILE="$REPO_ROOT/database/user-db/Dockerfile"
CONTEXT_DIR="$REPO_ROOT/database/user-db"

echo "Building and pushing $APP_NAME:$IMAGE_TAG"
az acr build \
  --registry "$AZ_ACR_NAME" \
  --image "$APP_NAME:$IMAGE_TAG" \
  --file "$DOCKERFILE" \
  "$CONTEXT_DIR"

FQ_IMAGE="$ACR_LOGIN_SERVER/$APP_NAME:$IMAGE_TAG"

echo "Deploying container app: $APP_NAME"
az containerapp up \
  --name "$APP_NAME" \
  --resource-group "${AZ_ENV_RG:-$AZ_RG}" \
  --environment "$AZ_ENV_NAME" \
  --image "$FQ_IMAGE" \
  --ingress external \
  --target-port 5432 \
  --registry-server "$ACR_LOGIN_SERVER" \
  --registry-username "$ACR_USERNAME" \
  --registry-password "$ACR_PASSWORD" \
  --env-vars \
    POSTGRES_PASSWORD=postgres \
    POSTGRES_USER=postgres

# Ensure TCP ingress on 5432
az containerapp ingress set \
  --name "$APP_NAME" \
  --resource-group "$AZ_RG" \
  --type external \
  --target-port 5432 \
  --transport tcp \
  --exposed-port 5432

FQDN="$(az containerapp show -g "$AZ_RG" -n "$APP_NAME" --query properties.configuration.ingress.fqdn -o tsv)"
echo "$APP_NAME FQDN: $FQDN"

echo "Deployed $APP_NAME"


