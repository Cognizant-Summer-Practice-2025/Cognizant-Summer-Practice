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
APP_NAME="backend-user"
DOCKERFILE="$REPO_ROOT/backend/backend-user/Dockerfile"
CONTEXT_DIR="$REPO_ROOT/backend/backend-user"

FQ_IMAGE="$ACR_LOGIN_SERVER/$APP_NAME:$IMAGE_TAG"

echo "Logging into ACR: $ACR_LOGIN_SERVER"
if [[ -z "${ACR_USERNAME:-}" || -z "${ACR_PASSWORD:-}" ]]; then
  # Refresh ACR admin creds if not present
  ACR_USERNAME="$(az acr credential show -n "$AZ_ACR_NAME" --query username -o tsv)"
  ACR_PASSWORD="$(az acr credential show -n "$AZ_ACR_NAME" --query passwords[0].value -o tsv)"
fi
if [[ -n "${ACR_PASSWORD:-}" ]]; then
  echo "$ACR_PASSWORD" | docker login "$ACR_LOGIN_SERVER" --username "$ACR_USERNAME" --password-stdin 1>/dev/null || az acr login --name "$AZ_ACR_NAME" 1>/dev/null
else
  az acr login --name "$AZ_ACR_NAME" 1>/dev/null
fi

echo "Building $FQ_IMAGE (local buildx)"
docker buildx build --platform linux/amd64 -f "$DOCKERFILE" -t "$FQ_IMAGE" "$CONTEXT_DIR" --push

echo "Deploying container app: $APP_NAME"

# Resolve DB host (FQDN if available)
USER_DB_HOST="${USER_DB_HOST:-}"
if [[ -z "$USER_DB_HOST" ]]; then
  USER_DB_HOST="$(az containerapp show -g "$AZ_RG" -n user-db --query properties.configuration.ingress.fqdn -o tsv 2>/dev/null || true)"
fi
USER_DB_HOST="${USER_DB_HOST:-user-db}"

# Build ALLOWED_ORIGINS from deployed frontend FQDNs when available
FRONT_ORIGINS=()
for APP in auth-user-service home-portfolio-service messages-service admin-service; do
  FQDN="$(az containerapp show -g "${AZ_ENV_RG:-$AZ_RG}" -n "$APP" --query properties.configuration.ingress.fqdn -o tsv 2>/dev/null || true)"
  if [[ -n "$FQDN" ]]; then FRONT_ORIGINS+=("https://$FQDN"); fi
done
if [[ ${#FRONT_ORIGINS[@]} -gt 0 ]]; then
  ALLOWED_ORIGINS="$(IFS=,; echo "${FRONT_ORIGINS[*]}")"
else
  ALLOWED_ORIGINS="${ALLOWED_ORIGINS:-http://localhost:3000,http://localhost:3001,http://localhost:3002,http://localhost:3003}"
fi

# Debug: Show CORS and connection string configuration
echo "🔍 Backend User Environment Configuration:"
echo "  ALLOWED_ORIGINS: $ALLOWED_ORIGINS"
echo "  ConnectionStrings__Database_User: ${ConnectionStrings__Database_User:0:50}..."
echo ""

if az containerapp show -g "${AZ_ENV_RG:-$AZ_RG}" -n "$APP_NAME" 1>/dev/null 2>&1; then
  echo "Updating existing Container App: $APP_NAME"
  az containerapp update \
    --name "$APP_NAME" \
    --resource-group "${AZ_ENV_RG:-$AZ_RG}" \
    --image "$FQ_IMAGE" \
    --set-env-vars \
      ConnectionStrings__Database_User="Host=$USER_DB_HOST;Port=5432;Database=user_db;Username=$POSTGRES_USER;Password=$POSTGRES_PASSWORD;Ssl Mode=Require;Trust Server Certificate=true" \
      ALLOWED_ORIGINS="$ALLOWED_ORIGINS" \
      AUTH_GOOGLE_ID="${AUTH_GOOGLE_ID:-}" \
      AUTH_GOOGLE_SECRET="${AUTH_GOOGLE_SECRET:-}" \
      AUTH_GITHUB_ID="${AUTH_GITHUB_ID:-}" \
      AUTH_GITHUB_SECRET="${AUTH_GITHUB_SECRET:-}" \
      AUTH_LINKEDIN_ID="${AUTH_LINKEDIN_ID:-}" \
      AUTH_LINKEDIN_SECRET="${AUTH_LINKEDIN_SECRET:-}" \
      AUTH_FACEBOOK_ID="${AUTH_FACEBOOK_ID:-}" \
      AUTH_FACEBOOK_SECRET="${AUTH_FACEBOOK_SECRET:-}" \
      LOGGING_LOGLEVEL_DEFAULT=Information \
      LOGGING_LOGLEVEL_MICROSOFT_ASPNETCORE=Warning
else
  az containerapp up \
    --name "$APP_NAME" \
    --resource-group "${AZ_ENV_RG:-$AZ_RG}" \
    --environment "$AZ_ENV_NAME" \
    --image "$FQ_IMAGE" \
    ${ACR_PASSWORD:+--registry-server "$ACR_LOGIN_SERVER"} \
    ${ACR_PASSWORD:+--registry-username "$ACR_USERNAME"} \
    ${ACR_PASSWORD:+--registry-password "$ACR_PASSWORD"} \
    --ingress external \
    --target-port 5200 \
    --env-vars \
      ConnectionStrings__Database_User="Host=$USER_DB_HOST;Port=5432;Database=user_db;Username=$POSTGRES_USER;Password=$POSTGRES_PASSWORD;Ssl Mode=Require;Trust Server Certificate=true" \
      ALLOWED_ORIGINS="$ALLOWED_ORIGINS" \
      AUTH_GOOGLE_ID="${AUTH_GOOGLE_ID:-}" \
      AUTH_GOOGLE_SECRET="${AUTH_GOOGLE_SECRET:-}" \
      AUTH_GITHUB_ID="${AUTH_GITHUB_ID:-}" \
      AUTH_GITHUB_SECRET="${AUTH_GITHUB_SECRET:-}" \
      AUTH_LINKEDIN_ID="${AUTH_LINKEDIN_ID:-}" \
      AUTH_LINKEDIN_SECRET="${AUTH_LINKEDIN_SECRET:-}" \
      AUTH_FACEBOOK_ID="${AUTH_FACEBOOK_ID:-}" \
      AUTH_FACEBOOK_SECRET="${AUTH_FACEBOOK_SECRET:-}" \
      LOGGING_LOGLEVEL_DEFAULT=Information \
      LOGGING_LOGLEVEL_MICROSOFT_ASPNETCORE=Warning
fi

echo "Deployed $APP_NAME"


