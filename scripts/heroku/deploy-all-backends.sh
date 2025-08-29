#!/bin/bash

# Deploy all backend services to Heroku
# Usage: ./deploy-all-backends.sh [base-app-name]

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
NC='\033[0m' # No Color

# Default base app name
DEFAULT_BASE_NAME="cognizant"
BASE_NAME=${1:-$DEFAULT_BASE_NAME}

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
        echo ""
    else
        echo -e "${YELLOW}⚠️  .env file not found at $SCRIPT_DIR/.env${NC}"
        echo -e "${YELLOW}   Please ensure the .env file exists with proper configuration${NC}"
        exit 1
    fi
}

echo -e "${PURPLE}🚀 Deploying all backend services to Heroku...${NC}"
echo -e "${PURPLE}Base app name: ${BASE_NAME}${NC}"

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

# Function to deploy a service
deploy_service() {
    local service_name=$1
    local script_name=$2
    local app_name="${BASE_NAME}-${service_name}-backend"
    
    echo -e "${BLUE}📦 Deploying ${service_name}...${NC}"
    echo -e "${BLUE}App name: ${app_name}${NC}"
    
    # Run the deployment script
    if bash "${SCRIPT_DIR}/${script_name}" "$app_name"; then
        echo -e "${GREEN}✅ ${service_name} deployed successfully!${NC}"
        echo -e "${BLUE}   App URL: https://${app_name}.herokuapp.com${NC}"
        echo ""
    else
        echo -e "${RED}❌ Failed to deploy ${service_name}${NC}"
        return 1
    fi
}

# Deploy all services
echo -e "${PURPLE}🔄 Starting deployment of all backend services...${NC}"
echo ""

# Deploy backend-portfolio
deploy_service "portfolio" "deploy-backend-portfolio.sh"

# Deploy backend-messages
deploy_service "messages" "deploy-backend-messages.sh"

# Deploy backend-user
deploy_service "user" "deploy-backend-user.sh"

# Deploy backend-AI
deploy_service "ai" "deploy-backend-ai.sh"

echo -e "${PURPLE}🎉 All backend services deployment completed!${NC}"
echo ""
echo -e "${BLUE}📋 Summary of deployed services:${NC}"
echo -e "${GREEN}✅ Portfolio Service: https://${BASE_NAME}-portfolio-backend.herokuapp.com${NC}"
echo -e "${GREEN}✅ Messages Service: https://${BASE_NAME}-messages-backend.herokuapp.com${NC}"
echo -e "${GREEN}✅ User Service: https://${BASE_NAME}-user-backend.herokuapp.com${NC}"
echo -e "${GREEN}✅ AI Service: https://${BASE_NAME}-ai-backend.herokuapp.com${NC}"
echo ""
echo -e "${YELLOW}⚠️  Important notes:${NC}"
echo -e "${YELLOW}   1. All environment variables have been loaded from .env file${NC}"
echo -e "${YELLOW}   2. Database connection strings are configured from .env${NC}"
echo -e "${YELLOW}   3. CORS origins and external service URLs are set from .env${NC}"
echo -e "${YELLOW}   4. Authentication secrets and OAuth credentials are configured${NC}"
echo -e "${YELLOW}   5. Email configuration is set for the messages service${NC}"
echo -e "${YELLOW}   6. AI configuration and OpenRouter API keys are set${NC}"
echo -e "${YELLOW}   7. Stripe configuration is set for the user service${NC}"
echo ""
echo -e "${BLUE}🔧 To manage your apps, use:${NC}"
echo -e "${BLUE}   heroku apps:info --app <app-name>${NC}"
echo -e "${BLUE}   heroku logs --tail --app <app-name>${NC}"
echo -e "${BLUE}   heroku config --app <app-name>${NC}"
echo ""
echo -e "${BLUE}📊 To view all environment variables for an app:${NC}"
echo -e "${BLUE}   heroku config --app <app-name> | grep -E '(ConnectionStrings|ALLOWED_ORIGINS|AUTH_|STRIPE_|EMAIL_|OPENROUTER_)'${NC}"
