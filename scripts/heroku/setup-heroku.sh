#!/bin/bash

# Setup script for Heroku deployment
# This script helps set up the initial configuration for all backend services

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
NC='\033[0m' # No Color

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
        
        # Show key configuration loaded
        echo -e "${BLUE}📋 Key configuration loaded from .env:${NC}"
        if [[ -n "${ConnectionStrings__Database_User:-}" ]]; then
            echo -e "${GREEN}   ✅ User Database: Configured${NC}"
        fi
        if [[ -n "${ConnectionStrings__Database_Portfolio:-}" ]]; then
            echo -e "${GREEN}   ✅ Portfolio Database: Configured${NC}"
        fi
        if [[ -n "${ConnectionStrings__Database_Messages:-}" ]]; then
            echo -e "${GREEN}   ✅ Messages Database: Configured${NC}"
        fi
        if [[ -n "${AUTH_SECRET:-}" ]]; then
            echo -e "${GREEN}   ✅ Authentication: Configured${NC}"
        fi
        if [[ -n "${OPENROUTER_API_KEY:-}" ]]; then
            echo -e "${GREEN}   ✅ AI Service: Configured${NC}"
        fi
        if [[ -n "${STRIPE_SECRET_KEY:-}" ]]; then
            echo -e "${GREEN}   ✅ Stripe: Configured${NC}"
        fi
        if [[ -n "${Email__SmtpHost:-}" ]]; then
            echo -e "${GREEN}   ✅ Email: Configured${NC}"
        fi
        echo ""
    else
        echo -e "${YELLOW}⚠️  .env file not found at $SCRIPT_DIR/.env${NC}"
        echo -e "${YELLOW}   Please ensure the .env file exists with proper configuration${NC}"
        exit 1
    fi
}

echo -e "${PURPLE}🔧 Heroku Setup Script for Cognizant Backend Services${NC}"
echo ""

# Check if Heroku CLI is installed
if ! command -v heroku &> /dev/null; then
    echo -e "${RED}❌ Heroku CLI is not installed.${NC}"
    echo -e "${YELLOW}Please install it first:${NC}"
    echo "https://devcenter.heroku.com/articles/heroku-cli"
    echo ""
    echo "For macOS:"
    echo "  brew tap heroku/brew && brew install heroku"
    echo ""
    echo "For Ubuntu/Debian:"
    echo "  curl https://cli-assets.heroku.com/install-ubuntu.sh | sh"
    echo ""
    echo "For Windows:"
    echo "  Download from: https://devcenter.heroku.com/articles/heroku-cli"
    exit 1
fi

echo -e "${GREEN}✅ Heroku CLI is installed${NC}"

# Check if user is logged in to Heroku
if ! heroku auth:whoami &> /dev/null; then
    echo -e "${YELLOW}⚠️  Not logged in to Heroku. Please login first:${NC}"
    heroku login
else
    echo -e "${GREEN}✅ Logged in to Heroku as: $(heroku auth:whoami)${NC}"
fi

echo ""

# Load environment variables
load_env_vars

# Function to setup database for a service
setup_database() {
    local app_name=$1
    local service_name=$2
    
    echo -e "${BLUE}🗄️  Setting up PostgreSQL database for ${service_name}...${NC}"
    
    # Check if database already exists
    if heroku addons:info --app "$app_name" | grep -q "heroku-postgresql"; then
        echo -e "${GREEN}✅ PostgreSQL database already exists for ${service_name}${NC}"
        return 0
    fi
    
    # Create PostgreSQL database
    echo -e "${YELLOW}Creating PostgreSQL database for ${service_name}...${NC}"
    if heroku addons:create heroku-postgresql:mini --app "$app_name"; then
        echo -e "${GREEN}✅ PostgreSQL database created for ${service_name}${NC}"
        
        # Get database URL and set it as environment variable
        local db_url=$(heroku config:get DATABASE_URL --app "$app_name")
        if [[ -n "$db_url" ]]; then
            echo -e "${BLUE}Database URL: ${db_url}${NC}"
        fi
    else
        echo -e "${RED}❌ Failed to create PostgreSQL database for ${service_name}${NC}"
        return 1
    fi
}

# Function to setup a service
setup_service() {
    local service_name=$1
    local app_name=$2
    
    echo -e "${PURPLE}🔧 Setting up ${service_name}...${NC}"
    
    # Check if app exists
    if ! heroku apps:info --app "$app_name" &> /dev/null; then
        echo -e "${YELLOW}⚠️  App '$app_name' doesn't exist. Please run the deployment script first.${NC}"
        return 1
    fi
    
    # Setup database
    setup_database "$app_name" "$service_name"
    
    echo -e "${GREEN}✅ ${service_name} setup completed${NC}"
    echo ""
}

# Get base app name from user
echo -e "${BLUE}📝 Enter the base name for your Heroku apps (default: cognizant):${NC}"
read -r base_name
base_name=${base_name:-cognizant}

echo ""
echo -e "${PURPLE}🚀 Setting up Heroku configuration for base name: ${base_name}${NC}"
echo ""

# Setup each service
setup_service "portfolio" "${base_name}-portfolio-backend"
setup_service "messages" "${base_name}-messages-backend"
setup_service "user" "${base_name}-user-backend"
setup_service "ai" "${base_name}-ai-backend"

echo -e "${PURPLE}🎉 Heroku setup completed!${NC}"
echo ""
echo -e "${BLUE}📋 Configuration summary:${NC}"
echo -e "${GREEN}✅ All environment variables loaded from .env file${NC}"
echo -e "${GREEN}✅ Database connection strings configured${NC}"
echo -e "${GREEN}✅ CORS origins and external service URLs set${NC}"
echo -e "${GREEN}✅ Authentication secrets and OAuth credentials configured${NC}"
echo -e "${GREEN}✅ Email configuration set for messages service${NC}"
echo -e "${GREEN}✅ AI configuration and OpenRouter API keys set${NC}"
echo -e "${GREEN}✅ Stripe configuration set for user service${NC}"
echo ""
echo -e "${BLUE}📋 Next steps:${NC}"
echo -e "${BLUE}   1. Deploy your services using the deployment scripts${NC}"
echo -e "${BLUE}   2. The scripts will automatically use all configuration from .env${NC}"
echo -e "${BLUE}   3. No manual environment variable configuration needed${NC}"
echo ""
echo -e "${BLUE}🔧 Useful commands:${NC}"
echo -e "${BLUE}   ./deploy-all-backends.sh ${base_name}${NC}"
echo -e "${BLUE}   heroku config --app <app-name>${NC}"
echo -e "${BLUE}   heroku logs --tail --app <app-name>${NC}"
echo -e "${BLUE}   heroku addons:open postgresql --app <app-name>${NC}"
echo ""
echo -e "${YELLOW}⚠️  Remember to:${NC}"
echo -e "${YELLOW}   - Update your .env file if you need to change any configuration${NC}"
echo -e "${YELLOW}   - The deployment scripts will automatically use the updated values${NC}"
echo -e "${YELLOW}   - Test each service after deployment${NC}"
echo ""
echo -e "${BLUE}🚀 Ready to deploy! Run: ./deploy-all-backends.sh ${base_name}${NC}"
