#!/bin/bash

# Deploy backend-user to Heroku
# Usage: ./deploy-backend-user.sh [app-name]

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Default app name
DEFAULT_APP_NAME="cognizant-user-backend"
APP_NAME=${1:-$DEFAULT_APP_NAME}

# Get the directory where this script is located
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Load environment variables from .env file
load_env_vars() {
    if [[ -f "$SCRIPT_DIR/.env" ]]; then
        echo -e "${BLUE}📁 Loading environment variables from .env file...${NC}"
        while IFS= read -r line || [ -n "$line" ]; do
            case "$line" in
                ''|\#*) continue ;;
            esac
            if [[ "$line" =~ ^[A-Za-z_][A-Za-z0-9_]*= ]]; then
                export "$line"
            fi
        done < "$SCRIPT_DIR/.env"
        echo -e "${GREEN}✅ Environment variables loaded${NC}"
    else
        echo -e "${YELLOW}⚠️  .env file not found at $SCRIPT_DIR/.env${NC}"
        echo -e "${YELLOW}   Please ensure the .env file exists with proper configuration${NC}"
        exit 1
    fi
}

echo -e "${BLUE}🚀 Deploying backend-user to Heroku...${NC}"
echo -e "${BLUE}App name: ${APP_NAME}${NC}"

# Check if Heroku CLI is installed
if ! command -v heroku &> /dev/null; then
    echo -e "${RED}❌ Heroku CLI is not installed. Please install it first:${NC}"
    echo "https://devcenter.heroku.com/articles/heroku-cli"
    exit 1
fi

# Check if user is logged in to Heroku
if ! heroku auth:whoami &> /dev/null; then
    echo -e "${YELLOW}⚠️  Not logged in to Heroku. Please login first:${NC}"
    heroku login
fi

# Load environment variables
load_env_vars

# Navigate to backend-user directory
cd "$(dirname "$0")/../../backend/backend-user"

echo -e "${BLUE}📁 Working directory: $(pwd)${NC}"

# Check if app exists, create if not
if ! heroku apps:info --app "$APP_NAME" &> /dev/null; then
    echo -e "${YELLOW}⚠️  App '$APP_NAME' doesn't exist. Creating new app...${NC}"
    heroku create "$APP_NAME" --region us
    echo -e "${GREEN}✅ Created new Heroku app: $APP_NAME${NC}"
else
    echo -e "${GREEN}✅ App '$APP_NAME' already exists${NC}"
fi

# Set buildpack for .NET
echo -e "${BLUE}🔧 Setting .NET buildpack...${NC}"
heroku buildpacks:set --app "$APP_NAME" jincod/dotnetcore

# Set environment variables
echo -e "${BLUE}🔐 Setting environment variables...${NC}"

# Database connection (you'll need to set these manually or use Heroku Postgres addon)
echo -e "${YELLOW}⚠️  Please ensure you have a PostgreSQL database configured.${NC}"
echo -e "${YELLOW}   You can add one with: heroku addons:create heroku-postgresql:mini --app $APP_NAME${NC}"

# Set basic environment variables
heroku config:set --app "$APP_NAME" \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://0.0.0.0:\$PORT \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Set database connection string from .env
if [[ -n "${ConnectionStrings__Database_User:-}" ]]; then
    echo -e "${BLUE}🗄️  Setting database connection string...${NC}"
    heroku config:set --app "$APP_NAME" \
        ConnectionStrings__Database_User="$ConnectionStrings__Database_User"
else
    echo -e "${YELLOW}⚠️  ConnectionStrings__Database_User not found in .env${NC}"
fi

# Set CORS origins from .env
if [[ -n "${ALLOWED_ORIGINS:-}" ]]; then
    echo -e "${BLUE}🌐 Setting CORS origins...${NC}"
    heroku config:set --app "$APP_NAME" \
        ALLOWED_ORIGINS="$ALLOWED_ORIGINS"
else
    echo -e "${YELLOW}⚠️  ALLOWED_ORIGINS not found in .env, using defaults${NC}"
    heroku config:set --app "$APP_NAME" \
        ALLOWED_ORIGINS="https://your-frontend-domain.com,https://your-admin-domain.com"
fi

# Set logging from .env
if [[ -n "${LOGGING_LOGLEVEL_DEFAULT:-}" ]]; then
    heroku config:set --app "$APP_NAME" \
        LOGGING__LOGLEVEL_DEFAULT="$LOGGING_LOGLEVEL_DEFAULT"
else
    heroku config:set --app "$APP_NAME" \
        LOGGING__LOGLEVEL_DEFAULT=Information
fi

if [[ -n "${LOGGING_LOGLEVEL_MICROSOFT_ASPNETCORE:-}" ]]; then
    heroku config:set --app "$APP_NAME" \
        LOGGING__LOGLEVEL_MICROSOFT_ASPNETCORE="$LOGGING_LOGLEVEL_MICROSOFT_ASPNETCORE"
else
    heroku config:set --app "$APP_NAME" \
        LOGGING__LOGLEVEL_MICROSOFT_ASPNETCORE=Warning
fi

# Set authentication secrets from .env
if [[ -n "${AUTH_SECRET:-}" ]]; then
    echo -e "${BLUE}🔐 Setting authentication secrets...${NC}"
    heroku config:set --app "$APP_NAME" \
        AUTH_SECRET="$AUTH_SECRET"
fi

if [[ -n "${NEXTAUTH_SECRET:-}" ]]; then
    heroku config:set --app "$APP_NAME" \
        NEXTAUTH_SECRET="$NEXTAUTH_SECRET"
fi

if [[ -n "${NEXTAUTH_URL:-}" ]]; then
    heroku config:set --app "$APP_NAME" \
        NEXTAUTH_URL="$NEXTAUTH_URL"
else
    heroku config:set --app "$APP_NAME" \
        NEXTAUTH_URL="https://$APP_NAME.herokuapp.com"
fi

# Set OAuth provider credentials from .env
if [[ -n "${AUTH_GITHUB_ID:-}" ]]; then
    echo -e "${BLUE}🔑 Setting OAuth credentials...${NC}"
    heroku config:set --app "$APP_NAME" \
        AUTH_GITHUB_ID="$AUTH_GITHUB_ID" \
        AUTH_GITHUB_SECRET="$AUTH_GITHUB_SECRET"
fi

if [[ -n "${AUTH_GOOGLE_ID:-}" ]]; then
    heroku config:set --app "$APP_NAME" \
        AUTH_GOOGLE_ID="$AUTH_GOOGLE_ID" \
        AUTH_GOOGLE_SECRET="$AUTH_GOOGLE_SECRET"
fi

if [[ -n "${AUTH_LINKEDIN_ID:-}" ]]; then
    heroku config:set --app "$APP_NAME" \
        AUTH_LINKEDIN_ID="$AUTH_LINKEDIN_ID" \
        AUTH_LINKEDIN_SECRET="$AUTH_LINKEDIN_SECRET"
fi

if [[ -n "${AUTH_FACEBOOK_ID:-}" ]]; then
    heroku config:set --app "$APP_NAME" \
        AUTH_FACEBOOK_ID="$AUTH_FACEBOOK_ID" \
        AUTH_FACEBOOK_SECRET="$AUTH_FACEBOOK_SECRET"
fi

# Set Stripe configuration from .env
if [[ -n "${STRIPE_SECRET_KEY:-}" ]]; then
    echo -e "${BLUE}💳 Setting Stripe configuration...${NC}"
    heroku config:set --app "$APP_NAME" \
        STRIPE_SECRET_KEY="$STRIPE_SECRET_KEY" \
        STRIPE_WEBHOOK_SECRET="$STRIPE_WEBHOOK_SECRET" \
        STRIPE_PRICE_ID="$STRIPE_PRICE_ID"
fi

# Set external service URLs from .env
if [[ -n "${PORTFOLIO_SERVICE_URL:-}" ]]; then
    echo -e "${BLUE}🔗 Setting external service URLs...${NC}"
    heroku config:set --app "$APP_NAME" \
        EXTERNALSERVICES__PORTFOLIOSERVICE__BASEURL="$PORTFOLIO_SERVICE_URL"
fi

if [[ -n "${NEXT_PUBLIC_MESSAGES_API_URL:-}" ]]; then
    heroku config:set --app "$APP_NAME" \
        EXTERNALSERVICES__MESSAGESSERVICE__BASEURL="$NEXT_PUBLIC_MESSAGES_API_URL"
fi

# Set frontend URLs from .env
if [[ -n "${NEXT_PUBLIC_AUTH_USER_SERVICE:-}" ]]; then
    echo -e "${BLUE}🌐 Setting frontend URLs...${NC}"
    heroku config:set --app "$APP_NAME" \
        FRONTENDURLS__0="$NEXT_PUBLIC_AUTH_USER_SERVICE" \
        FRONTENDURLS__1="$NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE" \
        FRONTENDURLS__2="$NEXT_PUBLIC_MESSAGES_SERVICE" \
        FRONTENDURLS__3="$NEXT_PUBLIC_ADMIN_SERVICE"
fi

# Set HTTP client configuration
heroku config:set --app "$APP_NAME" \
    HTTPCLIENT__TIMEOUT="00:00:30" \
    HTTPCLIENT__USERAGENT="UserService/1.0" \
    HTTPCLIENT__MAXCONNECTIONSPERSERVER="10" \
    HTTPCLIENT__USECOOKIES="false" \
    HTTPCLIENT__HANDLERLIFETIME="00:05:00"

echo -e "${GREEN}✅ Environment variables set${NC}"

# Deploy to Heroku
echo -e "${BLUE}📤 Deploying to Heroku...${NC}"
git add .
git commit -m "Deploy to Heroku - $(date)" || true
git push heroku main

# Open the app
echo -e "${GREEN}✅ Deployment complete!${NC}"
echo -e "${BLUE}🌐 Opening app in browser...${NC}"
heroku open --app "$APP_NAME"

# Show app info
echo -e "${BLUE}📊 App information:${NC}"
heroku apps:info --app "$APP_NAME"

echo -e "${GREEN}🎉 backend-user successfully deployed to Heroku!${NC}"
echo -e "${BLUE}App URL: https://$APP_NAME.herokuapp.com${NC}"
