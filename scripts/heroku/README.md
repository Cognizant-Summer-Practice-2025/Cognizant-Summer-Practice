# Heroku Deployment Scripts for Cognizant Backend Services

This directory contains scripts to deploy all backend services to Heroku using the Heroku CLI. The scripts are designed to work exactly like the Azure deployment scripts, automatically loading all configuration from the `.env` file.

## 📋 Prerequisites

1. **Heroku CLI installed** - [Installation Guide](https://devcenter.heroku.com/articles/heroku-cli)
2. **Heroku account** - [Sign up here](https://signup.heroku.com/)
3. **Git repository** - Your project should be a Git repository
4. **.NET 9.0** - Your backend services use .NET 9.0
5. **`.env` file** - Must contain all necessary configuration (see Configuration section)

## 🚀 Quick Start

### 1. Initial Setup
```bash
# Make scripts executable
chmod +x scripts/heroku/*.sh

# Run the setup script (creates databases, validates .env)
./scripts/heroku/setup-heroku.sh
```

### 2. Deploy All Services
```bash
# Deploy all backend services at once (automatically uses .env config)
./scripts/heroku/deploy-all-backends.sh

# Or deploy with custom base name
./scripts/heroku/deploy-all-backends.sh mycompany
```

### 3. Deploy Individual Services
```bash
# Deploy specific services (automatically uses .env config)
./scripts/heroku/deploy-backend-portfolio.sh
./scripts/heroku/deploy-backend-messages.sh
./scripts/heroku/deploy-backend-user.sh
./scripts/heroku/deploy-backend-ai.sh

# Or with custom app names
./scripts/heroku/deploy-backend-portfolio.sh my-portfolio-app
```

## 📁 Scripts Overview

| Script | Purpose | Default App Name | Configuration Source |
|--------|---------|------------------|---------------------|
| `setup-heroku.sh` | Initial Heroku configuration and database setup | N/A | `.env` file |
| `deploy-all-backends.sh` | Deploy all 4 backend services | `cognizant-{service}-backend` | `.env` file |
| `deploy-backend-portfolio.sh` | Deploy portfolio service only | `cognizant-portfolio-backend` | `.env` file |
| `deploy-backend-messages.sh` | Deploy messages service only | `cognizant-messages-backend` | `.env` file |
| `deploy-backend-user.sh` | Deploy user service only | `cognizant-user-backend` | `.env` file |
| `deploy-backend-ai.sh` | Deploy AI service only | `cognizant-ai-backend` | `.env` file |

## 🔧 Configuration

### Environment Variables
All scripts automatically load configuration from the `.env` file, just like the Azure deployment scripts. Each service will have these environment variables set:

#### Basic Configuration (All Services)
- `ASPNETCORE_ENVIRONMENT=Production`
- `ASPNETCORE_URLS=http://0.0.0.0:$PORT`
- `DOTNET_RUNNING_IN_CONTAINER=true`
- `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false`

#### Database Configuration
- `ConnectionStrings__Database_User` - User service database
- `ConnectionStrings__Database_Portfolio` - Portfolio service database  
- `ConnectionStrings__Database_Messages` - Messages service database

#### CORS and Frontend URLs
- `ALLOWED_ORIGINS` - CORS allowed origins
- `NEXT_PUBLIC_AUTH_USER_SERVICE` - Auth service URL
- `NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE` - Portfolio service URL
- `NEXT_PUBLIC_MESSAGES_SERVICE` - Messages service URL
- `NEXT_PUBLIC_ADMIN_SERVICE` - Admin service URL

#### Service-Specific Variables

**Portfolio Service:**
- External service URLs (User service)
- Frontend URLs
- HTTP client configuration
- Airflow secret

**Messages Service:**
- Email configuration (SMTP settings)
- External service URLs (User, Portfolio services)
- Frontend URLs
- HTTP client configuration

**User Service:**
- Authentication secrets (`AUTH_SECRET`, `NEXTAUTH_SECRET`)
- OAuth credentials (GitHub, Google, LinkedIn, Facebook)
- Stripe configuration
- External service URLs (Portfolio, Messages services)
- Frontend URLs
- HTTP client configuration

**AI Service:**
- OpenRouter AI configuration (API key, model, base URL)
- Portfolio prompts
- External service URLs (User, Portfolio, Messages services)
- Frontend URLs
- AI-specific logging levels
- HTTP client configuration

### Database Configuration
Each service will need a PostgreSQL database. The setup script can create these automatically using:
```bash
heroku addons:create heroku-postgresql:mini --app <app-name>
```

## 🔄 How It Works (Same as Azure Scripts)

The Heroku deployment scripts work exactly like the Azure deployment scripts:

1. **Load `.env` file** - All environment variables are loaded automatically
2. **Validate configuration** - Scripts check for required variables
3. **Set Heroku config** - Environment variables are set using `heroku config:set`
4. **Deploy application** - Code is pushed to Heroku
5. **Verify deployment** - App is opened and information displayed

### Key Differences from Azure
- Uses Heroku CLI instead of Azure CLI
- Deploys to Heroku Container Apps instead of Azure Container Apps
- Uses Heroku PostgreSQL addons instead of Azure PostgreSQL
- Same configuration structure and environment variable handling

## 📝 Required .env Configuration

Your `.env` file must contain all the configuration that the Azure scripts use. Here's what's required:

### Database Connections
```bash
ConnectionStrings__Database_User="Host=your-host;Port=5432;Database=user_db;Username=postgres;Password=your-password"
ConnectionStrings__Database_Portfolio="Host=your-host;Port=5434;Database=portfolio_db;Username=postgres;Password=your-password"
ConnectionStrings__Database_Messages="Host=your-host;Port=5433;Database=messages_db;Username=postgres;Password=your-password"
```

### Authentication
```bash
AUTH_SECRET=your-auth-secret
NEXTAUTH_SECRET=your-nextauth-secret
AUTH_GITHUB_ID=your-github-client-id
AUTH_GITHUB_SECRET=your-github-client-secret
# ... other OAuth providers
```

### External Services
```bash
USER_SERVICE_URL=https://your-user-service-url
PORTFOLIO_SERVICE_URL=https://your-portfolio-service-url
NEXT_PUBLIC_MESSAGES_API_URL=https://your-messages-service-url
```

### AI Configuration
```bash
OPENROUTER_API_KEY=your-openrouter-api-key
OPENROUTER_MODEL=your-openrouter-model
BEST_PORTFOLIO_PROMPT=your-portfolio-prompt
```

### Email Configuration
```bash
Email__SmtpHost=smtp.gmail.com
Email__SmtpPort=587
Email__SmtpUsername=your-email@gmail.com
Email__SmtpPassword=your-app-password
# ... other email settings
```

### Stripe Configuration
```bash
STRIPE_SECRET_KEY=your-stripe-secret-key
STRIPE_WEBHOOK_SECRET=your-stripe-webhook-secret
STRIPE_PRICE_ID=your-stripe-price-id
```

## 🔍 Monitoring and Management

### View App Information
```bash
heroku apps:info --app <app-name>
```

### View Logs
```bash
# Real-time logs
heroku logs --tail --app <app-name>

# Recent logs
heroku logs --app <app-name>
```

### View Configuration
```bash
# All config vars
heroku config --app <app-name>

# Filter specific config vars
heroku config --app <app-name> | grep -E '(ConnectionStrings|ALLOWED_ORIGINS|AUTH_|STRIPE_|EMAIL_|OPENROUTER_)'
```

### Open Database Dashboard
```bash
heroku addons:open postgresql --app <app-name>
```

### Scale Your Apps
```bash
# Scale to 1 dyno (free tier)
heroku ps:scale web=1 --app <app-name>

# Scale to multiple dynos (paid tier)
heroku ps:scale web=2 --app <app-name>
```

## 🚨 Troubleshooting

### Common Issues

1. **Buildpack Issues**
   ```bash
   # Clear and reset buildpacks
   heroku buildpacks:clear --app <app-name>
   heroku buildpacks:set jincod/dotnetcore --app <app-name>
   ```

2. **Database Connection Issues**
   ```bash
   # Check database status
   heroku addons:info postgresql --app <app-name>
   
   # Reset database
   heroku pg:reset DATABASE_URL --app <app-name>
   ```

3. **Environment Variable Issues**
   ```bash
   # View all config vars
   heroku config --app <app-name>
   
   # Remove specific config var
   heroku config:unset VARIABLE_NAME --app <app-name>
   ```

### Log Analysis
```bash
# View recent errors
heroku logs --app <app-name> | grep -i error

# View recent warnings
heroku logs --app <app-name> | grep -i warning
```

## 📚 Additional Resources

- [Heroku .NET Buildpack](https://github.com/jincod/dotnet-buildpack)
- [Heroku PostgreSQL Addon](https://devcenter.heroku.com/articles/heroku-postgresql)
- [Heroku CLI Commands](https://devcenter.heroku.com/articles/heroku-cli-commands)
- [.NET Core on Heroku](https://devcenter.heroku.com/articles/deploying-net-core)

## 🔄 Update Process

To update deployed services:

1. Make your code changes
2. Update your `.env` file if needed
3. Commit changes to Git
4. Run the deployment script again
5. The script will automatically use updated values from `.env`

## 💡 Tips

- **Same as Azure**: The Heroku scripts work exactly like your Azure scripts - they load the same `.env` file and use the same configuration
- **No Manual Setup**: All environment variables are automatically loaded and configured
- **Easy Updates**: Just update your `.env` file and redeploy
- **Consistent**: Same configuration structure across Azure and Heroku deployments
- Use the `--app` flag when running Heroku commands to specify which app you're working with
- Monitor your app's performance using `heroku ps --app <app-name>`
- Set up monitoring and alerting for production apps
- Use environment-specific configuration for different deployment stages
- Regularly backup your databases using `heroku pg:backups:capture --app <app-name>`

## 🔐 Security Notes

- All sensitive configuration is loaded from your `.env` file
- The `.env` file should be kept secure and not committed to version control
- Heroku config vars are encrypted at rest
- Use Heroku's built-in secret management for production deployments
