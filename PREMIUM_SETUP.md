# Premium Subscription System Setup Guide

This guide will help you set up the premium subscription system with Stripe integration for the Cognizant Summer Practice project.

## Overview

The premium subscription system includes:
- Database table for storing premium subscription data
- Backend API endpoints for managing subscriptions
- Stripe integration for payment processing
- Frontend components for premium access control
- Premium guard for protecting AI features

## Prerequisites

1. **Stripe Account**: You need a Stripe account with API keys
2. **PostgreSQL Database**: The system uses PostgreSQL for data storage
3. **Environment Variables**: Configure Stripe and database credentials

## Backend Setup

### 1. Install Dependencies

The backend already includes the Stripe.net package. If you need to install it manually:

```bash
cd backend/backend-user
dotnet add package Stripe.net
```

### 2. Database Migration

The premium subscription table is already included in the database init script. Run the database initialization:

```bash
# If using Docker
docker-compose up -d user-db

# The table will be created automatically when the container starts
```

### 3. Configure Stripe

Update the `backend/backend-user/appsettings.json` file with your Stripe credentials:

```json
{
  "Stripe": {
    "SecretKey": "sk_test_your_actual_stripe_secret_key",
    "WebhookSecret": "whsec_your_actual_webhook_secret",
    "PriceId": "price_your_actual_price_id"
  }
}
```

**To get these values:**

1. **Secret Key**: Go to Stripe Dashboard → Developers → API Keys
2. **Webhook Secret**: Go to Stripe Dashboard → Developers → Webhooks → Add endpoint
   - URL: `https://your-domain.com/api/premiumsubscription/webhook`
   - Events: `customer.subscription.created`, `customer.subscription.updated`, `customer.subscription.deleted`
3. **Price ID**: Go to Stripe Dashboard → Products → Create/Select Product → Pricing

### 4. Build and Run Backend

```bash
cd backend/backend-user
dotnet build
dotnet run
```

## Frontend Setup

### 1. Install Dependencies

```bash
cd frontend/auth-user-service
npm install
```

### 2. Configure Environment Variables

Create a `.env.local` file in the frontend directory:

```env
NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY=pk_test_your_actual_stripe_publishable_key
NEXT_PUBLIC_USER_API_URL=http://localhost:5200
AUTH_SECRET=your_auth_secret_here
```

### 3. Build and Run Frontend

```bash
npm run build
npm run dev
```

## Stripe Product Setup

### 1. Create a Product in Stripe

1. Go to Stripe Dashboard → Products
2. Click "Add Product"
3. Set product name (e.g., "Premium Subscription")
4. Set pricing:
   - Price: $9.99/month (or your desired price)
   - Billing: Recurring
   - Billing period: Monthly
5. Save the product and note the Price ID

### 2. Configure Webhook Endpoint

1. Go to Stripe Dashboard → Developers → Webhooks
2. Click "Add endpoint"
3. Set endpoint URL: `https://your-domain.com/api/premiumsubscription/webhook`
4. Select events:
   - `customer.subscription.created`
   - `customer.subscription.updated`
   - `customer.subscription.deleted`
5. Save and note the webhook secret

## Testing the System

### 1. Test Database Connection

Ensure the backend can connect to the database and the premium_subscriptions table exists.

### 2. Test Stripe Integration

1. Use Stripe test cards for testing:
   - Success: `4242 4242 4242 4242`
   - Decline: `4000 0000 0000 0002`
2. Navigate to `/ai` page
3. If not premium, you should see the upgrade prompt
4. Click "Upgrade to Premium" to test Stripe checkout

### 3. Test Webhook Processing

1. Complete a test payment
2. Check the backend logs for webhook processing
3. Verify the premium subscription is created in the database

## API Endpoints

### Backend Endpoints

- `GET /api/premiumsubscription/status` - Check user's premium status
- `POST /api/premiumsubscription/create-checkout-session` - Create Stripe checkout session
- `POST /api/premiumsubscription/webhook` - Handle Stripe webhooks
- `POST /api/premiumsubscription/cancel` - Cancel subscription

### Frontend API Routes

- `GET /api/premium/status` - Check premium status (proxies to backend)
- `POST /api/premium/create-checkout-session` - Create checkout session (proxies to backend)
- `POST /api/premium/cancel` - Cancel subscription (proxies to backend)

## Components

### PremiumGuard
Protects premium-only routes and shows upgrade prompt for non-premium users.

### PremiumStatusIndicator
Shows user's premium status with crown icon.

### PaymentSuccess
Handles post-payment success flow and redirects users appropriately.

## Security Considerations

1. **Webhook Verification**: Stripe webhooks are verified using the webhook secret
2. **Authentication**: All premium endpoints require valid JWT tokens
3. **Database Constraints**: User can only have one premium subscription
4. **Stripe Security**: Uses official Stripe SDK with proper error handling

## Troubleshooting

### Common Issues

1. **Stripe Keys Not Working**
   - Ensure you're using the correct environment (test/live)
   - Verify API keys are correct
   - Check Stripe account status

2. **Webhook Not Receiving Events**
   - Verify webhook endpoint URL is accessible
   - Check webhook secret configuration
   - Ensure proper event selection

3. **Premium Status Not Updating**
   - Check webhook processing in backend logs
   - Verify database connection
   - Check Stripe subscription status

4. **Frontend Not Loading**
   - Verify environment variables are set
   - Check API endpoint URLs
   - Ensure backend is running

### Debug Mode

Enable debug logging in the backend by setting log level to "Debug" in `appsettings.json`.

## Production Deployment

### 1. Update Configuration
- Use live Stripe keys instead of test keys
- Set proper webhook URLs for production domain
- Configure production database connection

### 2. SSL/TLS
- Ensure all endpoints use HTTPS
- Configure proper SSL certificates
- Update webhook URLs to use HTTPS

### 3. Monitoring
- Set up Stripe webhook monitoring
- Monitor database performance
- Set up application logging and monitoring

## Support

For issues with:
- **Stripe Integration**: Check Stripe documentation and dashboard
- **Backend Issues**: Check application logs and database connectivity
- **Frontend Issues**: Check browser console and network requests
- **Database Issues**: Verify connection strings and table structure

## Additional Features

The system can be extended with:
- Multiple subscription tiers
- Usage-based billing
- Discount codes and promotions
- Subscription management dashboard
- Email notifications for subscription events
