#!/usr/bin/env bash
set -euo pipefail

# Minimal Azure setup for ACR and Container Apps Environment (no analytics)

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

# Load env if present (export only KEY=VALUE lines)
if [[ -f "$SCRIPT_DIR/.env" ]]; then
  echo "Loading environment variables from .env file..."
  while IFS= read -r line || [ -n "$line" ]; do
    case "$line" in
      ''|\#*) continue ;;
    esac
    if [[ "$line" =~ ^[A-Za-z_][A-Za-z0-9_]*= ]]; then
      export "$line"
    fi
  done < "$SCRIPT_DIR/.env"
fi

# Defaults (override via scripts/azure/.env)
: "${AZ_SUBSCRIPTION:=-}"
: "${AZ_RG:=csp-rg}"
: "${AZ_LOCATION:=westeurope}"
: "${AZ_ACR_NAME:=cspacr$RANDOM}"
: "${AZ_ENV_NAME:=csp-aca-env}"

# Debug: Show key environment variables used
echo "🔍 Key environment variables in use:"
echo "  AZ_RG: $AZ_RG"
echo "  AZ_LOCATION: $AZ_LOCATION"
echo "  AZ_ACR_NAME: $AZ_ACR_NAME"
echo "  NEXT_PUBLIC_PORTFOLIO_API_URL: ${NEXT_PUBLIC_PORTFOLIO_API_URL-}"
echo "  NEXT_PUBLIC_USER_API_URL: ${NEXT_PUBLIC_USER_API_URL-}"
echo "  ALLOWED_ORIGINS: ${ALLOWED_ORIGINS-}"
echo ""

if [[ -n "${AZ_SUBSCRIPTION}" ]]; then
  az account set --subscription "$AZ_SUBSCRIPTION" 1>/dev/null
fi

echo "Ensuring Azure Container Apps extension is installed"
az extension add --name containerapp --upgrade -y 1>/dev/null || true

echo "Creating resource group: $AZ_RG ($AZ_LOCATION)"
az group create \
  --name "$AZ_RG" \
  --location "$AZ_LOCATION" 1>/dev/null

echo "Creating ACR: $AZ_ACR_NAME"
az acr create \
  --resource-group "$AZ_RG" \
  --name "$AZ_ACR_NAME" \
  --sku Basic \
  --admin-enabled true 1>/dev/null

ACR_LOGIN_SERVER="$(az acr show -n "$AZ_ACR_NAME" --query loginServer -o tsv)"
read -r ACR_USERNAME ACR_PASSWORD < <(az acr credential show -n "$AZ_ACR_NAME" --query "[username,passwords[0].value]" -o tsv)

echo "Checking for existing Container Apps Environment named $AZ_ENV_NAME (any region)"
SUB_ID="$(az account show --query id -o tsv)"
ALL_ENVS_JSON="$(az rest --method get --url "https://management.azure.com/subscriptions/$SUB_ID/providers/Microsoft.App/managedEnvironments?api-version=2024-03-01" -o json 2>/dev/null || echo '{}')"

# Prefer exact name match anywhere in subscription
MATCH_NAME_JSON="$(jq -r --arg NAME "$AZ_ENV_NAME" '.value[] | select(.name==$NAME) | {name:.name, rg:(.id|split("/")[4]), location:.location}' <<<"$ALL_ENVS_JSON" 2>/dev/null || true)"
if [[ -n "$MATCH_NAME_JSON" ]]; then
  AZ_ENV_RG="$(jq -r '.rg' <<<"$MATCH_NAME_JSON")"
  FOUND_LOC="$(jq -r '.location' <<<"$MATCH_NAME_JSON")"
  # Align AZ_LOCATION with existing env to avoid region conflicts
  AZ_LOCATION="$FOUND_LOC"
  echo "Reusing existing env by name: $AZ_ENV_NAME in RG $AZ_ENV_RG (location: $AZ_LOCATION)"
else
  echo "Checking for any env in region $AZ_LOCATION"
  EXISTING_ENV_NAME="$(jq -r '.value[] | select(.location==env.AZ_LOCATION) | .name' <<<"$ALL_ENVS_JSON" | head -n1 || true)"
  EXISTING_ENV_RG="$(jq -r '.value[] | select(.location==env.AZ_LOCATION) | .id | split("/")[4]' <<<"$ALL_ENVS_JSON" | head -n1 || true)"
  if [[ -n "$EXISTING_ENV_NAME" ]]; then
    echo "Reusing existing env in region: $EXISTING_ENV_NAME (RG: $EXISTING_ENV_RG)"
    AZ_ENV_NAME="$EXISTING_ENV_NAME"
    AZ_ENV_RG="$EXISTING_ENV_RG"
  else
    echo "Creating Container Apps Environment: $AZ_ENV_NAME (no analytics) in $AZ_LOCATION"
    set +e
    az containerapp env create \
      --name "$AZ_ENV_NAME" \
      --resource-group "$AZ_RG" \
      --location "$AZ_LOCATION" \
      --logs-destination none 1>/dev/null
    CREATE_RC=$?
    set -e
    if [[ $CREATE_RC -ne 0 ]]; then
      echo "Create failed; attempting to detect env by name after wait (operation in progress?)"
      sleep 10
      ALL_ENVS_JSON="$(az rest --method get --url "https://management.azure.com/subscriptions/$SUB_ID/providers/Microsoft.App/managedEnvironments?api-version=2024-03-01" -o json 2>/dev/null || echo '{}')"
      MATCH_NAME_JSON="$(jq -r --arg NAME "$AZ_ENV_NAME" '.value[] | select(.name==$NAME) | {name:.name, rg:(.id|split("/")[4]), location:.location}' <<<"$ALL_ENVS_JSON" 2>/dev/null || true)"
      if [[ -n "$MATCH_NAME_JSON" ]]; then
        AZ_ENV_RG="$(jq -r '.rg' <<<"$MATCH_NAME_JSON")"
        FOUND_LOC="$(jq -r '.location' <<<"$MATCH_NAME_JSON")"
        AZ_LOCATION="$FOUND_LOC"
        echo "Reusing env after retry: $AZ_ENV_NAME in RG $AZ_ENV_RG (location: $AZ_LOCATION)"
      else
        echo "No existing environment found; please remove old environments or choose another region." >&2
        exit 1
      fi
    else
      AZ_ENV_RG="$AZ_RG"
    fi
  fi
fi

mkdir -p "$SCRIPT_DIR/.state"
cat >"$SCRIPT_DIR/.state/azure.env" <<EOF
AZ_RG=$AZ_RG
AZ_LOCATION=$AZ_LOCATION
AZ_ACR_NAME=$AZ_ACR_NAME
AZ_ENV_NAME=$AZ_ENV_NAME
AZ_ENV_RG=${AZ_ENV_RG:-$AZ_RG}
ACR_LOGIN_SERVER=$ACR_LOGIN_SERVER
ACR_USERNAME=$ACR_USERNAME
ACR_PASSWORD=$ACR_PASSWORD
REPO_ROOT=$REPO_ROOT
EOF

echo "Setup complete. Saved state to scripts/azure/.state/azure.env"


