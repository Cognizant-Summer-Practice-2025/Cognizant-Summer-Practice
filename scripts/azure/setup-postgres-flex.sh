#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Load previous state if present (server names, FQDNs, etc.)
if [[ -f "$SCRIPT_DIR/.state/azure.env" ]]; then
	# shellcheck disable=SC1091
	source "$SCRIPT_DIR/.state/azure.env"
fi

# Load user overrides
if [[ -f "$SCRIPT_DIR/.env" ]]; then
	while IFS= read -r line || [ -n "$line" ]; do
		case "$line" in ''|\#*) continue ;; esac
		if [[ "$line" =~ ^[A-Za-z_][A-Za-z0-9_]*= ]]; then export "$line"; fi
	done < "$SCRIPT_DIR/.env"
fi

: "${POSTGRES_USER:=postgres}"
: "${POSTGRES_PASSWORD:=postgres}"

# Stable default server names (can be overridden by env)
: "${AZ_PG_USER_SERVER:=csp-user}"
: "${AZ_PG_PORTFOLIO_SERVER:=csp-portfolio}"
: "${AZ_PG_MESSAGES_SERVER:=csp-messages}"

PG_LOCATION="${AZ_PG_LOCATION:-polandcentral}"
RG_NAME="${AZ_ENV_RG:-${AZ_RG:-}}"

ensure_server() {
	local server_name="$1"
	if az postgres flexible-server show -g "$RG_NAME" -n "$server_name" 1>/dev/null 2>&1; then
		echo "Server exists: $server_name" >&2
		return 0
	fi
	echo "Creating Azure PostgreSQL Flexible Server: $server_name in $PG_LOCATION" >&2
	az postgres flexible-server create \
		--name "$server_name" \
		--resource-group "$RG_NAME" \
		--location "$PG_LOCATION" \
		--admin-user "$POSTGRES_USER" \
		--admin-password "$POSTGRES_PASSWORD" \
		--tier Burstable \
		--sku-name Standard_B1ms \
		--version 15 \
		--storage-size 32 \
		--high-availability Disabled \
		--public-access 0.0.0.0-255.255.255.255 1>/dev/null
}

database_exists() {
	local server_name="$1" db_name="$2"
	local count
	count="$(az postgres flexible-server db list --resource-group "$RG_NAME" --server-name "$server_name" --query "[?name=='$db_name'] | length(@)" -o tsv)"
	[[ "$count" == "1" ]]
}

ensure_database() {
	local server_name="$1" db_name="$2"
	if database_exists "$server_name" "$db_name"; then
		echo "Database exists: $db_name on $server_name" >&2
		return 0
	fi
	az postgres flexible-server db create \
		--resource-group "$RG_NAME" \
		--server-name "$server_name" \
		--database-name "$db_name" 1>/dev/null
}

get_fqdn() {
	local server_name="$1"
	az postgres flexible-server show -g "$RG_NAME" -n "$server_name" --query fullyQualifiedDomainName -o tsv
}

schema_initialized() {
	local fqdn="$1" db_name="$2" sentinel_table="$3"
	# Returns 0 if sentinel table exists
	docker run --rm -e PGPASSWORD="$POSTGRES_PASSWORD" postgres:15 \
		psql "host=$fqdn port=5432 dbname=$db_name user=$POSTGRES_USER sslmode=require" -tAc \
		"SELECT to_regclass('public.$sentinel_table')" | grep -q "$sentinel_table"
}

initialize_schema_if_needed() {
	local fqdn="$1" db_name="$2" sql_file="$3" sentinel_table="$4"
	if [[ ! -f "$sql_file" ]]; then
		echo "No SQL file for $db_name, skipping." >&2
		return 0
	fi
	if schema_initialized "$fqdn" "$db_name" "$sentinel_table"; then
		echo "Schema already initialized for $db_name (found table: $sentinel_table)" >&2
		return 0
	fi
	echo "Initializing schema for $db_name from $sql_file" >&2
	docker run --rm -i postgres:15 \
		psql "host=$fqdn port=5432 dbname=$db_name user=$POSTGRES_USER password=$POSTGRES_PASSWORD sslmode=require" < "$sql_file"
}

update_env_var() {
	local file="$1" key="$2" value="$3"
	touch "$file"
	if grep -q "^$key=" "$file"; then
		# macOS-compatible sed inline
		sed -i '' -E "s|^$key=.*|$key=$value|" "$file"
	else
		echo "$key=$value" >> "$file"
	fi
}

# Ensure servers and databases
ensure_server "$AZ_PG_USER_SERVER"
ensure_server "$AZ_PG_PORTFOLIO_SERVER"
ensure_server "$AZ_PG_MESSAGES_SERVER"

ensure_database "$AZ_PG_USER_SERVER" user_db
ensure_database "$AZ_PG_PORTFOLIO_SERVER" portfolio_db
ensure_database "$AZ_PG_MESSAGES_SERVER" messages_db

# Resolve FQDNs
USER_FQDN="$(get_fqdn "$AZ_PG_USER_SERVER")"
PORTFOLIO_FQDN="$(get_fqdn "$AZ_PG_PORTFOLIO_SERVER")"
MESSAGES_FQDN="$(get_fqdn "$AZ_PG_MESSAGES_SERVER")"

# Initialize schemas only if missing
initialize_schema_if_needed "$USER_FQDN" user_db "$SCRIPT_DIR/../../database/user-db/user_db_init.sql" users
initialize_schema_if_needed "$PORTFOLIO_FQDN" portfolio_db "$SCRIPT_DIR/../../database/portfolio-db/portfolio_db_init.sql" portfolio_templates
initialize_schema_if_needed "$MESSAGES_FQDN" messages_db "$SCRIPT_DIR/../../database/messages-db/messages_db_init.sql" conversations

# Persist state
mkdir -p "$SCRIPT_DIR/.state"
STATE_FILE="$SCRIPT_DIR/.state/azure.env"
update_env_var "$STATE_FILE" USER_DB_SERVER "$AZ_PG_USER_SERVER"
update_env_var "$STATE_FILE" PORTFOLIO_DB_SERVER "$AZ_PG_PORTFOLIO_SERVER"
update_env_var "$STATE_FILE" MESSAGES_DB_SERVER "$AZ_PG_MESSAGES_SERVER"
update_env_var "$STATE_FILE" USER_DB_HOST "$USER_FQDN"
update_env_var "$STATE_FILE" PORTFOLIO_DB_HOST "$PORTFOLIO_FQDN"
update_env_var "$STATE_FILE" MESSAGES_DB_HOST "$MESSAGES_FQDN"

echo "Azure PostgreSQL Flexible Servers ready. Hosts saved to scripts/azure/.state/azure.env"


