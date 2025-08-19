#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/.state/azure.env"

# Load user overrides
if [[ -f "$SCRIPT_DIR/.env" ]]; then
  while IFS= read -r line || [ -n "$line" ]; do
    case "$line" in ''|\#*) continue ;; esac
    if [[ "$line" =~ ^[A-Za-z_][A-Za-z0-9_]*= ]]; then export "$line"; fi
  done < "$SCRIPT_DIR/.env"
fi

: "${POSTGRES_USER:=postgres}"
: "${POSTGRES_PASSWORD:=postgres}"

# Server names (override via env if you want custom names)
: "${AZ_PG_USER_SERVER:=csp-user-${RANDOM}}"
: "${AZ_PG_PORTFOLIO_SERVER:=csp-portfolio-${RANDOM}}"
: "${AZ_PG_MESSAGES_SERVER:=csp-messages-${RANDOM}}"

create_server_and_db() {
  local server_name="$1"
  local db_name="$2"
  local PG_LOCATION="${AZ_PG_LOCATION:-polandcentral}"
  echo "Creating Azure PostgreSQL Flexible Server: $server_name in $PG_LOCATION" >&2
  az postgres flexible-server create \
    --name "$server_name" \
    --resource-group "${AZ_ENV_RG:-$AZ_RG}" \
    --location "$PG_LOCATION" \
    --admin-user "$POSTGRES_USER" \
    --admin-password "$POSTGRES_PASSWORD" \
    --tier Burstable \
    --sku-name Standard_B1ms \
    --version 15 \
    --storage-size 32 \
    --high-availability Disabled \
    --public-access 0.0.0.0-255.255.255.255 1>/dev/null

  echo "Creating database: $db_name on $server_name" >&2
  az postgres flexible-server db create \
    --resource-group "${AZ_ENV_RG:-$AZ_RG}" \
    --server-name "$server_name" \
    --database-name "$db_name" 1>/dev/null

  local fqdn
  fqdn="$(az postgres flexible-server show -g "${AZ_ENV_RG:-$AZ_RG}" -n "$server_name" --query fullyQualifiedDomainName -o tsv)"
  echo "$server_name FQDN: $fqdn" >&2

  echo "Initializing schema for $db_name from local SQL if available" >&2
  local sql_file
  case "$db_name" in
    user_db) sql_file="$SCRIPT_DIR/../../database/user-db/user_db_init.sql" ;;
    portfolio_db) sql_file="$SCRIPT_DIR/../../database/portfolio-db/portfolio_db_init.sql" ;;
    messages_db) sql_file="$SCRIPT_DIR/../../database/messages-db/messages_db_init.sql" ;;
    *) sql_file="" ;;
  esac
  if [[ -f "$sql_file" ]]; then
    echo "Running init SQL for $db_name via dockerized psql" >&2
    docker run --rm -i postgres:15 \
      psql "host=$fqdn port=5432 dbname=$db_name user=$POSTGRES_USER password=$POSTGRES_PASSWORD sslmode=require" < "$sql_file"
  else
    echo "No SQL file found for $db_name, skipping." >&2
  fi

  echo "$fqdn"
}

USER_FQDN=$(create_server_and_db "$AZ_PG_USER_SERVER" user_db)
PORTFOLIO_FQDN=$(create_server_and_db "$AZ_PG_PORTFOLIO_SERVER" portfolio_db)
MESSAGES_FQDN=$(create_server_and_db "$AZ_PG_MESSAGES_SERVER" messages_db)

mkdir -p "$SCRIPT_DIR/.state"
{ 
  echo "USER_DB_HOST=$USER_FQDN"
  echo "PORTFOLIO_DB_HOST=$PORTFOLIO_FQDN"
  echo "MESSAGES_DB_HOST=$MESSAGES_FQDN"
} >> "$SCRIPT_DIR/.state/azure.env"

echo "Azure PostgreSQL Flexible Servers ready. Hosts saved to scripts/azure/.state/azure.env"


