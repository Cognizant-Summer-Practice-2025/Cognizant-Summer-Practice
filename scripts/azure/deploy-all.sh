#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

TAG="${1:-latest}"

#"$SCRIPT_DIR/az-setup.sh"
#"$SCRIPT_DIR/setup-postgres-flex.sh" || echo "Postgres Flex setup failed; continuing"

set +e

# Run independent deploys in parallel to speed up
jobs=()

# Backends (these can build/push in parallel)
"$SCRIPT_DIR/deploy-backend-user.sh" "$TAG" & jobs+=($!)
"$SCRIPT_DIR/deploy-backend-portfolio.sh" "$TAG" & jobs+=($!)
"$SCRIPT_DIR/deploy-backend-messages.sh" "$TAG" & jobs+=($!)
"$SCRIPT_DIR/deploy-backend-ai.sh" "$TAG" & jobs+=($!)

# Frontends in parallel
"$SCRIPT_DIR/deploy-frontend-auth-user.sh" "$TAG" & jobs+=($!)
"$SCRIPT_DIR/deploy-frontend-home-portfolio.sh" "$TAG" & jobs+=($!)
"$SCRIPT_DIR/deploy-frontend-messages.sh" "$TAG" & jobs+=($!)
"$SCRIPT_DIR/deploy-frontend-admin.sh" "$TAG" & jobs+=($!)

# Wait for all
fail=0
for pid in "${jobs[@]}"; do
  if ! wait "$pid"; then
    fail=1
  fi
done

set -e

if [ "$fail" -ne 0 ]; then
  echo "Some deploys failed." >&2
  exit 1
fi

echo "Done."


