#!/bin/bash

# Enable extended globbing for pattern-based trimming
shopt -s extglob

# Ensure gh CLI is installed
if ! command -v gh &>/dev/null; then
    echo "Error: GitHub CLI (gh) is not installed."
    exit 1
fi

# Get current repository information from git
if ! git rev-parse --git-dir > /dev/null 2>&1; then
    echo "Error: Not a git repository. Please run this script from within a git repository."
    exit 1
fi

# Get remote origin URL and extract owner/repo
REMOTE_URL=$(git config --get remote.origin.url)
if [ -z "$REMOTE_URL" ]; then
    echo "Error: Could not find remote origin URL. Please ensure the repository has a remote origin."
    exit 1
fi

# Extract owner and repo from different URL formats (with or without .git)
if [[ "$REMOTE_URL" =~ ^https://github\.com/([^/]+)/([^/]+)(\.git)?$ ]]; then
    OWNER="${BASH_REMATCH[1]}"
    REPO="${BASH_REMATCH[2]}"
elif [[ "$REMOTE_URL" =~ ^git@github\.com:([^/]+)/([^/]+)(\.git)?$ ]]; then
    OWNER="${BASH_REMATCH[1]}"
    REPO="${BASH_REMATCH[2]}"
else
    echo "Error: Could not parse GitHub repository URL: $REMOTE_URL"
    exit 1
fi

# Remove .git suffix if accidentally captured
REPO="${REPO%.git}"

echo "Detected repository: $OWNER/$REPO"

# Check if .env file exists in azure folder
AZURE_ENV_FILE="scripts/azure/.env"

if [ ! -f "$AZURE_ENV_FILE" ]; then
    echo "Error: Could not find .env file at $AZURE_ENV_FILE"
    exit 1
fi

# Check if azure.env file exists in .state folder
AZURE_STATE_ENV_FILE="scripts/azure/.state/azure.env"

if [ ! -f "$AZURE_STATE_ENV_FILE" ]; then
    echo "Error: Could not find azure.env file at $AZURE_STATE_ENV_FILE"
    exit 1
fi

# Authenticate with GitHub if not already authenticated
if ! gh auth status &>/dev/null; then
    echo "You need to authenticate with GitHub CLI."
    gh auth login
fi

# Function to process environment file and upload secrets
process_env_file() {
    local env_file="$1"

    echo "Processing environment file: $env_file"

    # Use POSIX-friendly parsing with trimming, CRLF stripping, inline comment handling
    while IFS= read -r line || [[ -n "$line" ]]; do
        # Strip trailing carriage return (CRLF compatibility)
        line=${line%$'\r'}

        # Trim leading/trailing whitespace
        line="${line##+([[:space:]])}"
        line="${line%%+([[:space:]])}"

        # Skip empty or full-line comment
        if [[ -z "$line" ]] || [[ "$line" =~ ^# ]]; then
            continue
        fi

        # Split on first '=' only
        if [[ "$line" == *"="* ]]; then
            local key=${line%%=*}
            local value=${line#*=}

            # Trim whitespace around key and value
            key="${key##+([[:space:]])}"
            key="${key%%+([[:space:]])}"
            value="${value##+([[:space:]])}"
            value="${value%%+([[:space:]])}"

            # If value is unquoted, strip inline comment starting with #
            if [[ ! "$value" =~ ^\".*\"$ ]] && [[ ! "$value" =~ ^\'.*\'$ ]]; then
                value=${value%%#*}
                value="${value%%+([[:space:]])}"
            fi

            # Strip surrounding single or double quotes if present
            if [[ "$value" =~ ^\".*\"$ ]]; then
                value=${value:1:${#value}-2}
            elif [[ "$value" =~ ^\'.*\'$ ]]; then
                value=${value:1:${#value}-2}
            fi

            # Skip if key empty after trimming
            if [[ -z "$key" ]]; then
                continue
            fi

            echo "Processing secret: $key"

            # Create/update secret
            if echo -n "$value" | gh secret set "$key" --repo "$OWNER/$REPO" -b -; then
                echo "Secret '$key' created/updated successfully."
            else
                echo "Error: Failed to create/update secret '$key'"
            fi
        fi
    done < "$env_file"
}

# Process azure/.env file
echo "=== Processing azure/.env file ==="
process_env_file "$AZURE_ENV_FILE"

# Process azure/.state/azure.env file
echo "=== Processing azure/.state/azure.env file ==="
process_env_file "$AZURE_STATE_ENV_FILE"

echo "Environment secrets upload completed!"
