#!/bin/bash

# Deploy backend-portfolio to Heroku
# Usage: ./deploy-backend-portfolio.sh [app-name]

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Default app name
DEFAULT_APP_NAME="cognizant-portfolio-backend"
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

echo -e "${BLUE}🚀 Deploying backend-portfolio to Heroku...${NC}"
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

# Navigate to backend-portfolio directory
cd "$(dirname "$0")/../../backend/backend-portfolio"

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
if [[ -n "${ConnectionStrings__Database_Portfolio:-}" ]]; then
    echo -e "${BLUE}🗄️  Setting database connection string...${NC}"
    heroku config:set --app "$APP_NAME" \
        ConnectionStrings__Database_Portfolio="$ConnectionStrings__Database_Portfolio"
else
    echo -e "${YELLOW}⚠️  ConnectionStrings__Database_Portfolio not found in .env${NC}"
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

# Set external service URLs from .env
if [[ -n "${USER_SERVICE_URL:-}" ]]; then
    echo -e "${BLUE}🔗 Setting external service URLs...${NC}"
    heroku config:set --app "$APP_NAME" \
        EXTERNALSERVICES__USERSERVICE__BASEURL="$USER_SERVICE_URL" \
        UserServiceUrl="$USER_SERVICE_URL" \
        USER_SERVICE_URL="$USER_SERVICE_URL"
else
    echo -e "${YELLOW}⚠️  USER_SERVICE_URL not found in .env${NC}"
    heroku config:set --app "$APP_NAME" \
        EXTERNALSERVICES__USERSERVICE__BASEURL="https://your-user-service-app.herokuapp.com"
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
    HTTPCLIENT__USERAGENT="PortfolioService/1.0" \
    HTTPCLIENT__MAXCONNECTIONSPERSERVER="10" \
    HTTPCLIENT__USECOOKIES="false" \
    HTTPCLIENT__HANDLERLIFETIME="00:05:00"

# Set Airflow secret if available
if [[ -n "${AIRFLOW_SECRET:-}" ]]; then
    echo -e "${BLUE}🔐 Setting Airflow secret...${NC}"
    heroku config:set --app "$APP_NAME" \
        AIRFLOW_SECRET="$AIRFLOW_SECRET"
fi

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

echo -e "${GREEN}🎉 backend-portfolio successfully deployed to Heroku!${NC}"
echo -e "${BLUE}App URL: https://$APP_NAME.herokuapp.com${NC}"
