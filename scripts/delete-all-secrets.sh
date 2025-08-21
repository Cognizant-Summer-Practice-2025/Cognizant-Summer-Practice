#!/bin/bash

# Delete GitHub secrets for the current repo
# Usage:
#   # Delete all repository-level secrets
#   ./scripts/delete-all-secrets.sh                 # interactive confirmation
#   ./scripts/delete-all-secrets.sh --yes           # non-interactive, skip confirmation
#   ./scripts/delete-all-secrets.sh -y              # alias for --yes
#
#   # Delete all secrets in a specific GitHub Environment (repo env secrets)
#   ./scripts/delete-all-secrets.sh --env production
#   ./scripts/delete-all-secrets.sh --env staging --yes

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

# Extract owner and repo from supported URL formats (with or without .git)
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

# Remove .git suffix if present
REPO="${REPO%.git}"

echo "Detected repository: $OWNER/$REPO"

# Parse flags (supports --yes/-y and --env <name>)
YES_FLAG=false
ENV_NAME=""
while [[ $# -gt 0 ]]; do
    case "$1" in
        -y|--yes)
            YES_FLAG=true
            shift
            ;;
        --env)
            if [[ -z "$2" ]]; then
                echo "Error: --env requires a value (environment name)."
                exit 1
            fi
            ENV_NAME="$2"
            shift 2
            ;;
        *)
            # ignore unknown args for now
            shift
            ;;
    esac
done

# Authenticate with GitHub if not already authenticated
if ! gh auth status &>/dev/null; then
    echo "You need to authenticate with GitHub CLI."
    gh auth login || { echo "Error: GitHub authentication failed."; exit 1; }
fi

# Fetch all secret names using GitHub API (handles pagination)
SECRET_SCOPE_DESC="repository"
if [[ -n "$ENV_NAME" ]]; then
    SECRET_SCOPE_DESC="environment '$ENV_NAME'"
    SECRET_NAMES=($(gh api \
      -H "Accept: application/vnd.github+json" \
      "/repos/$OWNER/$REPO/environments/$ENV_NAME/secrets" \
      --paginate \
      --jq '.secrets[].name' 2>/dev/null))
else
    SECRET_NAMES=($(gh api \
      -H "Accept: application/vnd.github+json" \
      "/repos/$OWNER/$REPO/actions/secrets" \
      --paginate \
      --jq '.secrets[].name' 2>/dev/null))
fi

if [ ${#SECRET_NAMES[@]} -eq 0 ]; then
    echo "No $SECRET_SCOPE_DESC secrets found for $OWNER/$REPO. Nothing to delete."
    exit 0
fi

echo "Found ${#SECRET_NAMES[@]} $SECRET_SCOPE_DESC secrets in $OWNER/$REPO:"
for name in "${SECRET_NAMES[@]}"; do
    echo "  - $name"
done

if [ "$YES_FLAG" != true ]; then
    echo
    echo "This will permanently delete ALL ${#SECRET_NAMES[@]} $SECRET_SCOPE_DESC secrets listed above."
    read -p "Type DELETE to confirm: " CONFIRM
    if [ "$CONFIRM" != "DELETE" ]; then
        echo "Aborted. No secrets were deleted."
        exit 1
    fi
fi

# Delete each secret
FAILED=0
for name in "${SECRET_NAMES[@]}"; do
    echo "Deleting secret: $name"
    if [[ -n "$ENV_NAME" ]]; then
        # Delete environment-level secret
        if gh secret delete "$name" --env "$ENV_NAME" --repo "$OWNER/$REPO" -y; then
            echo "Deleted '$name'"
        else
            echo "Error: Failed to delete '$name'"
            FAILED=$((FAILED+1))
        fi
    else
        # Delete repo-level secret
        if gh secret delete "$name" --repo "$OWNER/$REPO"; then
            echo "Deleted '$name'"
        else
            echo "Error: Failed to delete '$name'"
            FAILED=$((FAILED+1))
        fi
    fi
done

if [ $FAILED -gt 0 ]; then
    echo "Completed with $FAILED failure(s)."
    exit 1
fi

if [[ -n "$ENV_NAME" ]]; then
    echo "All secrets deleted successfully from environment '$ENV_NAME' in $OWNER/$REPO."
else
    echo "All repository secrets deleted successfully from $OWNER/$REPO."
fi
