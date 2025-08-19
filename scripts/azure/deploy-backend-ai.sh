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
APP_NAME="backend-ai"
DOCKERFILE="$REPO_ROOT/backend/backend-AI/Dockerfile"
CONTEXT_DIR="$REPO_ROOT/backend/backend-AI"

echo "Building and pushing $APP_NAME:$IMAGE_TAG (local buildx)"
FQ_IMAGE="$ACR_LOGIN_SERVER/$APP_NAME:$IMAGE_TAG"
# Ensure ACR creds
if [[ -z "${ACR_USERNAME:-}" || -z "${ACR_PASSWORD:-}" ]]; then
  ACR_USERNAME="$(az acr credential show -n "$AZ_ACR_NAME" --query username -o tsv)"
  ACR_PASSWORD="$(az acr credential show -n "$AZ_ACR_NAME" --query passwords[0].value -o tsv)"
fi
if [[ -n "${ACR_PASSWORD:-}" ]]; then
  echo "$ACR_PASSWORD" | docker login "$ACR_LOGIN_SERVER" --username "$ACR_USERNAME" --password-stdin 1>/dev/null || az acr login --name "$AZ_ACR_NAME" 1>/dev/null
else
  az acr login --name "$AZ_ACR_NAME" 1>/dev/null || true
fi

docker buildx build --platform linux/amd64 -f "$DOCKERFILE" -t "$FQ_IMAGE" "$CONTEXT_DIR" --push

echo "Deploying container app: $APP_NAME"

USER_SVC_URL="${USER_SVC_URL:-}"
if [[ -z "$USER_SVC_URL" ]]; then
  USER_SVC_URL="$(az containerapp show -g "${AZ_ENV_RG:-$AZ_RG}" -n backend-user --query properties.configuration.ingress.fqdn -o tsv 2>/dev/null || true)"
  [[ -n "$USER_SVC_URL" ]] && USER_SVC_URL="https://$USER_SVC_URL"
fi
USER_SVC_URL="${USER_SVC_URL:-http://backend-user:5200}"

PORTFOLIO_SVC_URL="${PORTFOLIO_SVC_URL:-}"
if [[ -z "$PORTFOLIO_SVC_URL" ]]; then
  PORTFOLIO_SVC_URL="$(az containerapp show -g "${AZ_ENV_RG:-$AZ_RG}" -n backend-portfolio --query properties.configuration.ingress.fqdn -o tsv 2>/dev/null || true)"
  [[ -n "$PORTFOLIO_SVC_URL" ]] && PORTFOLIO_SVC_URL="https://$PORTFOLIO_SVC_URL"
fi
PORTFOLIO_SVC_URL="${PORTFOLIO_SVC_URL:-http://backend-portfolio:5201}"

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

echo "🔍 Backend AI Environment Configuration:"
echo "  USER_SERVICE_URL: $USER_SVC_URL"
echo "  PORTFOLIO_SERVICE_URL: $PORTFOLIO_SVC_URL"
echo "  ALLOWED_ORIGINS: $ALLOWED_ORIGINS"

az containerapp up \
  --name "$APP_NAME" \
  --resource-group "${AZ_ENV_RG:-$AZ_RG}" \
  --environment "$AZ_ENV_NAME" \
  --image "$FQ_IMAGE" \
  ${ACR_PASSWORD:+--registry-server "$ACR_LOGIN_SERVER"} \
  ${ACR_PASSWORD:+--registry-username "$ACR_USERNAME"} \
  ${ACR_PASSWORD:+--registry-password "$ACR_PASSWORD"} \
  --ingress external \
  --target-port 5134 \
  --env-vars \
    USER_SERVICE_URL="$USER_SVC_URL" \
    PORTFOLIO_SERVICE_URL="$PORTFOLIO_SVC_URL" \
    ALLOWED_ORIGINS="$ALLOWED_ORIGINS" \
    OPENROUTER_API_KEY="${OPENROUTER_API_KEY:-}" \
    OPENROUTER_MODEL="${OPENROUTER_MODEL:-openai/gpt-oss-20b:free}" \
    OPENROUTER_BASE_URL="${OPENROUTER_BASE_URL:-https://openrouter.ai/api/v1/chat/completions}" \
    OPENROUTER_PROMPT="${OPENROUTER_PROMPT:-What is the meaning of life?}" \
    BEST_PORTFOLIO_PROMPT="${BEST_PORTFOLIO_PROMPT:-}" \
    RANKING_LOG_LEVEL="${RANKING_LOG_LEVEL:-Information}" \
    LOG_LEVEL="${LOG_LEVEL:-Information}" \
    AI_LOG_LEVEL="${AI_LOG_LEVEL:-Information}"

echo "Deployed $APP_NAME"


