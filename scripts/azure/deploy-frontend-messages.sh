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
APP_NAME="messages-service"
DOCKERFILE="$REPO_ROOT/frontend/messages-service/Dockerfile"
CONTEXT_DIR="$REPO_ROOT/frontend/messages-service"

# Build args from requested envs
NEXT_PUBLIC_AUTH_USER_SERVICE="${NEXT_PUBLIC_AUTH_USER_SERVICE:-http://localhost:3000}"
NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE="${NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE:-http://localhost:3001}"
NEXT_PUBLIC_MESSAGES_SERVICE="${NEXT_PUBLIC_MESSAGES_SERVICE:-http://localhost:3002}"
NEXT_PUBLIC_ADMIN_SERVICE="${NEXT_PUBLIC_ADMIN_SERVICE:-http://localhost:3003}"
NEXT_PUBLIC_USER_API_URL="${NEXT_PUBLIC_USER_API_URL:-http://localhost:5200}"
NEXT_PUBLIC_PORTFOLIO_API_URL="${NEXT_PUBLIC_PORTFOLIO_API_URL:-http://localhost:5201}"
NEXT_PUBLIC_MESSAGES_API_URL="${NEXT_PUBLIC_MESSAGES_API_URL:-http://localhost:5093}"
NEXT_PUBLIC_API_BASE_URL="${NEXT_PUBLIC_API_BASE_URL:-http://localhost:5201}"
NEXT_PUBLIC_EMAILJS_SERVICE_ID="${NEXT_PUBLIC_EMAILJS_SERVICE_ID:-service_cgaj9lq}"
NEXT_PUBLIC_EMAILJS_TEMPLATE_ID="${NEXT_PUBLIC_EMAILJS_TEMPLATE_ID:-template_2eg78ef}"
NEXT_PUBLIC_EMAILJS_PUBLIC_KEY="${NEXT_PUBLIC_EMAILJS_PUBLIC_KEY:-1u9bREnFdPj6CcXSb}"

FQ_IMAGE="$ACR_LOGIN_SERVER/$APP_NAME:$IMAGE_TAG"

echo "Logging into ACR: $ACR_LOGIN_SERVER"
if [[ -z "${ACR_USERNAME:-}" || -z "${ACR_PASSWORD:-}" ]]; then
  ACR_USERNAME="$(az acr credential show -n "$AZ_ACR_NAME" --query username -o tsv)"
  ACR_PASSWORD="$(az acr credential show -n "$AZ_ACR_NAME" --query passwords[0].value -o tsv)"
fi
if [[ -n "${ACR_PASSWORD:-}" ]]; then
  echo "$ACR_PASSWORD" | docker login "$ACR_LOGIN_SERVER" --username "$ACR_USERNAME" --password-stdin 1>/dev/null || az acr login --name "$AZ_ACR_NAME" 1>/dev/null
else
  az acr login --name "$AZ_ACR_NAME" 1>/dev/null
fi

echo "Building $FQ_IMAGE (local buildx)"
docker buildx build --platform linux/amd64 -f "$DOCKERFILE" \
  --build-arg NEXT_PUBLIC_AUTH_USER_SERVICE="$NEXT_PUBLIC_AUTH_USER_SERVICE" \
  --build-arg NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE="$NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE" \
  --build-arg NEXT_PUBLIC_MESSAGES_SERVICE="$NEXT_PUBLIC_MESSAGES_SERVICE" \
  --build-arg NEXT_PUBLIC_ADMIN_SERVICE="$NEXT_PUBLIC_ADMIN_SERVICE" \
  --build-arg NEXT_PUBLIC_USER_API_URL="$NEXT_PUBLIC_USER_API_URL" \
  --build-arg NEXT_PUBLIC_PORTFOLIO_API_URL="$NEXT_PUBLIC_PORTFOLIO_API_URL" \
  --build-arg NEXT_PUBLIC_MESSAGES_API_URL="$NEXT_PUBLIC_MESSAGES_API_URL" \
  --build-arg NEXT_PUBLIC_API_BASE_URL="$NEXT_PUBLIC_API_BASE_URL" \
  --build-arg NEXT_PUBLIC_EMAILJS_SERVICE_ID="$NEXT_PUBLIC_EMAILJS_SERVICE_ID" \
  --build-arg NEXT_PUBLIC_EMAILJS_TEMPLATE_ID="$NEXT_PUBLIC_EMAILJS_TEMPLATE_ID" \
  --build-arg NEXT_PUBLIC_EMAILJS_PUBLIC_KEY="$NEXT_PUBLIC_EMAILJS_PUBLIC_KEY" \
  -t "$FQ_IMAGE" "$CONTEXT_DIR" --push

echo "Deploying container app: $APP_NAME"
az containerapp up \
  --name "$APP_NAME" \
  --resource-group "${AZ_ENV_RG:-$AZ_RG}" \
  --environment "$AZ_ENV_NAME" \
  --image "$FQ_IMAGE" \
  ${ACR_PASSWORD:+--registry-server "$ACR_LOGIN_SERVER"} \
  ${ACR_PASSWORD:+--registry-username "$ACR_USERNAME"} \
  ${ACR_PASSWORD:+--registry-password "$ACR_PASSWORD"} \
  --ingress external \
  --target-port 3002

echo "Deployed $APP_NAME"


