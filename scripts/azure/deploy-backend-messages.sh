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
APP_NAME="backend-messages"
DOCKERFILE="$REPO_ROOT/backend/backend-messages/Dockerfile"
CONTEXT_DIR="$REPO_ROOT/backend/backend-messages"

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
docker buildx build --platform linux/amd64 -f "$DOCKERFILE" -t "$FQ_IMAGE" "$CONTEXT_DIR" --push

echo "Deploying container app: $APP_NAME"

MESSAGES_DB_HOST="${MESSAGES_DB_HOST:-}"
if [[ -z "$MESSAGES_DB_HOST" ]]; then
  MESSAGES_DB_HOST="$(az containerapp show -g "$AZ_RG" -n messages-db --query properties.configuration.ingress.fqdn -o tsv 2>/dev/null || true)"
fi
MESSAGES_DB_HOST="${MESSAGES_DB_HOST:-messages-db}"

# Ensure messages_db exists and is initialized when connecting to Azure PG Flexible Server
if [[ "$MESSAGES_DB_HOST" == *".postgres.database.azure.com"* ]]; then
  echo "Ensuring database 'messages_db' exists on $MESSAGES_DB_HOST with SSL"
  docker run --rm -e PGPASSWORD="$POSTGRES_PASSWORD" postgres:15 \
    psql "host=$MESSAGES_DB_HOST port=5433 dbname=postgres user=$POSTGRES_USER sslmode=require" -tc \
    "SELECT 1 FROM pg_database WHERE datname='messages_db'" | grep -q 1 || \
  docker run --rm -e PGPASSWORD="$POSTGRES_PASSWORD" postgres:15 \
    psql "host=$MESSAGES_DB_HOST port=5433 dbname=postgres user=$POSTGRES_USER sslmode=require" -c \
    "CREATE DATABASE messages_db;"

  # Initialize schema if core table is missing
  echo "Checking if schema is initialized in 'messages_db'"
  if ! docker run --rm -e PGPASSWORD="$POSTGRES_PASSWORD" postgres:15 \
    psql "host=$MESSAGES_DB_HOST port=5433 dbname=messages_db user=$POSTGRES_USER sslmode=require" -tAc \
    "SELECT to_regclass('public.messages')" | grep -q messages; then
    echo "Initializing schema from database/messages-db/messages_db_init.sql"
    SQL_FILE="$SCRIPT_DIR/../../database/messages-db/messages_db_init.sql"
    if [[ -f "$SQL_FILE" ]]; then
      docker run --rm -e PGPASSWORD="$POSTGRES_PASSWORD" -v "$REPO_ROOT:/repo" postgres:15 \
        psql "host=$MESSAGES_DB_HOST port=5433 dbname=messages_db user=$POSTGRES_USER sslmode=require" -f \
        "/repo/database/messages-db/messages_db_init.sql"
    else
      echo "Warning: SQL init file not found at $SQL_FILE; skipping schema init"
    fi
  fi
fi

USER_SVC_URL="${USER_SVC_URL:-}"
if [[ -z "$USER_SVC_URL" ]]; then
  USER_SVC_URL="$(az containerapp show -g "${AZ_ENV_RG:-$AZ_RG}" -n backend-user --query properties.configuration.ingress.fqdn -o tsv 2>/dev/null || true)"
  [[ -n "$USER_SVC_URL" ]] && USER_SVC_URL="https://$USER_SVC_URL"
fi
USER_SVC_URL="${USER_SVC_URL:-http://backend-user:5200}"

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
echo "🔍 Backend Messages Environment Configuration:"
echo "  ALLOWED_ORIGINS: $ALLOWED_ORIGINS"
echo "  USER_SERVICE_URL: $USER_SERVICE_URL"
echo "  ConnectionStrings__Database_Messages: ${ConnectionStrings__Database_Messages:0:50}..."
echo ""

echo "Using ConnectionStrings__Database_Messages from .env (with validation)"

# Normalize and validate database connection string
# 1) Strip surrounding quotes if present
# 2) Fallback to constructed string if missing
CLEAN_DB_CONN_STR="${ConnectionStrings__Database_Messages}"
# Strip surrounding single/double quotes if present
if [[ "$CLEAN_DB_CONN_STR" =~ ^\".*\"$ ]]; then
  CLEAN_DB_CONN_STR=${CLEAN_DB_CONN_STR:1:${#CLEAN_DB_CONN_STR}-2}
elif [[ "$CLEAN_DB_CONN_STR" =~ ^\'.*\'$ ]]; then
  CLEAN_DB_CONN_STR=${CLEAN_DB_CONN_STR:1:${#CLEAN_DB_CONN_STR}-2}
fi

if [[ -z "$CLEAN_DB_CONN_STR" ]]; then
  echo "ConnectionStrings__Database_Messages not set; constructing from host/user/password variables"
  : "${MESSAGES_DB_HOST:=messages-db}"
  : "${POSTGRES_USER:=postgres}"
  : "${POSTGRES_PASSWORD:=postgres}"
  CLEAN_DB_CONN_STR="Host=${MESSAGES_DB_HOST};Port=5433;Database=messages_db;Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}"
fi

echo "Resolved DB conn length: ${#CLEAN_DB_CONN_STR}"

if az containerapp show -g "${AZ_ENV_RG:-$AZ_RG}" -n "$APP_NAME" 1>/dev/null 2>&1; then
  echo "Updating existing Container App: $APP_NAME"
  az containerapp update \
    --name "$APP_NAME" \
    --resource-group "${AZ_ENV_RG:-$AZ_RG}" \
    --image "$FQ_IMAGE" \
    --set-env-vars \
  ConnectionStrings__Database_Messages="$CLEAN_DB_CONN_STR" \
      UserService__BaseUrl="$USER_SVC_URL" \
      UserServiceUrl="$USER_SVC_URL" \
      USER_SERVICE_URL="$USER_SVC_URL" \
      ALLOWED_ORIGINS="$ALLOWED_ORIGINS" \
      Email__SmtpHost="$Email__SmtpHost" \
      Email__SmtpPort="$Email__SmtpPort" \
      Email__SmtpUsername="$Email__SmtpUsername" \
      Email__SmtpPassword="$Email__SmtpPassword" \
      Email__FromAddress="$Email__FromAddress" \
      Email__FromName="$Email__FromName" \
      Email__UseSSL="$Email__UseSSL" \
      Email__EnableContactNotifications="$Email__EnableContactNotifications" \
      Email__TimeoutSeconds="$Email__TimeoutSeconds" \
      Email__MaxRetryAttempts="$Email__MaxRetryAttempts" \
      Email__RetryDelaySeconds="$Email__RetryDelaySeconds" \
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
    --target-port 5093 \
    --env-vars \
      ConnectionStrings__Database_Messages="$CLEAN_DB_CONN_STR" \
      UserService__BaseUrl="$USER_SVC_URL" \
      UserServiceUrl="$USER_SVC_URL" \
      USER_SERVICE_URL="$USER_SVC_URL" \
      ALLOWED_ORIGINS="$ALLOWED_ORIGINS" \
  Email__SmtpHost="$Email__SmtpHost" \
  Email__SmtpPort="$Email__SmtpPort" \
  Email__SmtpUsername="$Email__SmtpUsername" \
  Email__SmtpPassword="$Email__SmtpPassword" \
  Email__FromAddress="$Email__FromAddress" \
  Email__FromName="$Email__FromName" \
  Email__UseSSL="$Email__UseSSL" \
  Email__EnableContactNotifications="$Email__EnableContactNotifications" \
  Email__TimeoutSeconds="$Email__TimeoutSeconds" \
  Email__MaxRetryAttempts="$Email__MaxRetryAttempts" \
  Email__RetryDelaySeconds="$Email__RetryDelaySeconds" \
      LOGGING_LOGLEVEL_DEFAULT=Information \
      LOGGING_LOGLEVEL_MICROSOFT_ASPNETCORE=Warning
fi

echo "Deployed $APP_NAME"


