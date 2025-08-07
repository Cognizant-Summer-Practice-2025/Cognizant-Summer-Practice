# 🔐 Complete Centralized SSO Authentication & OAuth 2.0 Secure Endpoints

This documentation covers the comprehensive Single Sign-On (SSO) authentication system with automatic user injection, cross-service logout capabilities, and OAuth 2.0 secure endpoints implemented across all microservices.

## ✨ System Overview

**What This System Provides:**
- ✅ **Centralized Authentication** - Only `auth-user-service` handles OAuth login
- ✅ **OAuth 2.0 Secure Endpoints** - Backend APIs protected with OAuth 2.0 Bearer tokens
- ✅ **Automatic User Injection** - User data automatically pushed to all services on login
- ✅ **Real-time Cross-Service Logout** - Logout from any service affects all services
- ✅ **Persistent Sessions** - Users stay logged in across service visits
- ✅ **Zero CORS Issues** - JWT token-based communication
- ✅ **Fault Tolerance** - Works even if some services are down

## 🏗️ Architecture

### Authentication Flow Components

```
┌─────────────────┐    ┌───────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│  Auth Service   │    │  Other Services   │    │   All Services   │    │  Backend APIs   │
│   (Port 3000)   │    │ (Ports 3001-3003) │    │   (In-Memory)    │    │ (Ports 5200+)   │
├─────────────────┤    ├───────────────────┤    ├──────────────────┤    ├─────────────────┤
│ NextAuth Setup  │───▶│ Custom AuthContext│───▶│ User Data Storage│───▶│ OAuth 2.0 Auth  │
│ OAuth Providers │    │ JWT Verification  │    │ Cross-Tab Sync   │    │ Bearer Tokens   │
│ User Injection  │    │ SSO Token Handling│    │ Logout Detection │    │ Secure Endpoints│
│ Session Management│  │ Local Sessions    │    │ Real-time Updates│    │ Token Validation│
└─────────────────┘    └───────────────────┘    └──────────────────┘    └─────────────────┘
```

## 🔄 Complete Login Flow

### Step-by-Step Process

```mermaid
sequenceDiagram
    participant User
    participant AdminService as Admin Service (3003)
    participant AuthService as Auth Service (3000)
    participant AllServices as All Services

    User->>AdminService: 1. Visit admin-service
    AdminService->>AdminService: 2. Check local session
    AdminService->>AdminService: 3. Check injected user data
    AdminService->>AuthService: 4. Redirect to SSO callback
    AuthService->>AuthService: 5. Check NextAuth session
    AuthService->>User: 6. Redirect to OAuth login
    User->>AuthService: 7. Complete OAuth login
    AuthService->>AllServices: 8. Inject user data to ALL services
    AuthService->>AllServices: 9. Create JWT token
    AuthService->>AdminService: 10. Redirect with JWT token
    AdminService->>AdminService: 11. Verify JWT & create local session
    AdminService->>User: 12. User logged in successfully
```

### 1. Initial Visit
```javascript
// User visits any service (e.g., admin-service on port 3003)
// AuthContext checks authentication state
const checkAuthentication = async () => {
  // 1. Check for SSO token in URL
  // 2. Check local session storage
  // 3. Check injected user data from auth service
  // 4. If none found, redirect to auth service
}
```

### 2. Auth Service Redirect
```javascript
// Redirect to auth service SSO callback
const redirectToAuth = () => {
  const currentUrl = window.location.href;
  window.location.href = `${authServiceUrl}/api/sso/callback?callbackUrl=${currentUrl}`;
}
```

### 3. OAuth Authentication
```javascript
// In auth-user-service/lib/auth/auth-options.ts
async signIn({ user, account }) {
  // Verify OAuth provider
  // Check if user exists
  // On successful login:
  const userData = await getUserByEmail(user.email);
  await UserInjectionService.injectUser(userData); // 🔥 NEW: Immediate injection
  return true;
}
```

### 4. User Injection Process
```javascript
// auth-user-service/lib/services/user-injection-service.ts
static async injectUser(user: User) {
  const services = [
    'ADMIN_SERVICE',
    'HOME_PORTFOLIO_SERVICE', 
    'MESSAGES_SERVICE'
  ];
  
  // Inject user data to all services simultaneously
  await Promise.allSettled(
    services.map(service => 
      fetch(`${service.url}/api/user/inject`, {
        method: 'POST',
        headers: { 'X-Service-Auth': SERVICE_AUTH_SECRET },
        body: JSON.stringify(userData)
      })
    )
  );
}
```

### 5. JWT Token Creation & Redirect
```javascript
// auth-user-service/app/api/sso/callback/route.ts
// Create JWT token and redirect back to originating service
const token = await new SignJWT({ 
  email: userData.email,
  userId: userData.id,
  timestamp: Date.now()
}).setExpirationTime('5m').sign(secret);

return NextResponse.redirect(`${callbackUrl}?ssoToken=${token}`);
```

### 6. Local Session Creation
```javascript
// other-services/lib/contexts/auth-context.tsx
if (ssoToken) {
  const tokenPayload = await verifySSOToken(ssoToken);
  await createLocalSession(tokenPayload);
  setIsAuthenticated(true);
  
  // Also check for injected data
  const response = await fetch('/api/user/get');
  if (response.ok) {
    const userData = await response.json();
    setUser(userData);
  }
}
```

## 🚪 Complete Logout Flow

### Universal Logout System

```mermaid
sequenceDiagram
    participant User
    participant AnyService as Any Service
    participant AuthService as Auth Service
    participant AllServices as All Services
    participant LocalStorage as Browser Storage

    User->>AnyService: 1. Click logout
    AnyService->>LocalStorage: 2. Clear local session
    AnyService->>AuthService: 3. Call signout-all API
    AuthService->>AllServices: 4. Remove user from ALL services
    AuthService->>AuthService: 5. NextAuth signOut()
    LocalStorage->>AllServices: 6. Broadcast logout event
    AllServices->>AllServices: 7. Detect & sync logout
    AllServices->>User: 8. All services logged out
```

### 1. Logout Initiation (Any Service)
```javascript
// other-services/lib/contexts/auth-context.tsx
const logout = async () => {
  // Clear local state immediately
  setIsAuthenticated(false);
  setUserEmail(null);
  setUserId(null);
  
  // Call comprehensive logout
  await logoutFromAllServices();
};
```

### 2. Cross-Service Logout Call
```javascript
// other-services/lib/auth/sso-auth.ts
export async function logoutFromAllServices() {
  try {
    // Call auth service to remove user from all services
    const response = await fetch(`${authServiceUrl}/api/auth/signout-all`, {
      method: 'POST',
      credentials: 'include',
    });
    
    // Trigger cross-service logout signal
    localStorage.removeItem('sso_session');
    
    // Redirect to auth service for complete logout
    window.location.href = `${authServiceUrl}/api/auth/signout`;
  } catch (error) {
    console.error('Logout error:', error);
  }
}
```

### 3. Auth Service Comprehensive Logout
```javascript
// auth-user-service/app/api/auth/auto-signout/route.ts
export async function POST(request: NextRequest) {
  // 1. Get current session
  const session = await getServerSession(authOptions);
  
  // 2. Remove user data from all services
  await UserInjectionService.removeUser(session.user.email);
  
  // 3. Automatically clear NextAuth session cookies
  const response = NextResponse.json({ success: true });
  clearNextAuthSession(response); // No redirects, just clear cookies
  
  return response;
}
```

### 4. Automatic Session Clearing
```javascript
// auth-user-service/lib/auth/session-clearer.ts
export function clearNextAuthSession(response: NextResponse) {
  // Clear all NextAuth session cookies automatically
  response.cookies.set('next-auth.session-token', '', { maxAge: 0 });
  response.cookies.set('next-auth.csrf-token', '', { maxAge: 0 });
  // ... clear all session cookies
  
  // No redirects needed - session is cleared server-side
}
```

### 5. User Data Removal
```javascript
// auth-user-service/app/api/services/user-injection/route.ts
async function removeUserFromServices(userEmail) {
  const services = [
    'AUTH_USER_SERVICE',    // 🔥 NEW: Include auth service
    'ADMIN_SERVICE',
    'HOME_PORTFOLIO_SERVICE',
    'MESSAGES_SERVICE'
  ];
  
  // Remove user from all services
  await Promise.allSettled(
    services.map(service => 
      fetch(`${service.url}/api/user/remove`, {
        method: 'DELETE',
        headers: { 'X-Service-Auth': SERVICE_AUTH_SECRET },
        body: JSON.stringify({ email: userEmail })
      })
    )
  );
}
```

### 6. Cross-Tab Logout Synchronization
```javascript
// All services monitor localStorage changes
useEffect(() => {
  const handleStorageChange = (event) => {
    if (event.key === 'sso_session' && event.newValue === null) {
      // Another tab/service logged out, sync immediately
      setIsAuthenticated(false);
      setUserEmail(null);
      setUserId(null);
    }
  };
  
  window.addEventListener('storage', handleStorageChange);
  return () => window.removeEventListener('storage', handleStorageChange);
}, []);
```

### 7. NextAuth Session Cleanup
```javascript
// auth-user-service/components/auth-signout-monitor.tsx
// Monitors for logout signals and triggers NextAuth signOut
useEffect(() => {
  const checkSignoutSignal = async () => {
    const response = await fetch('/api/auth/check-signout-signal', {
      method: 'POST',
      body: JSON.stringify({ email: session.user.email })
    });
    
    if (response.ok) {
      const data = await response.json();
      if (data.shouldSignOut) {
        await signOut({ callbackUrl: '/', redirect: true });
      }
    }
  };
  
  // Check every 5 seconds
  const interval = setInterval(checkSignoutSignal, 5000);
  return () => clearInterval(interval);
}, []);
```

## 🏃‍♂️ Automatic User Detection

### Periodic User Injection Detection
```javascript
// All services automatically detect injected users
useEffect(() => {
  // Check for injected user data every 15 seconds
  const intervalId = setInterval(() => {
    if (!isAuthenticated) {
      checkAuthentication(); // Checks /api/user/get endpoint
    }
  }, 15000);
  
  return () => clearInterval(intervalId);
}, [isAuthenticated]);
```

### User Data Retrieval
```javascript
// other-services/app/api/user/get/route.ts
export async function GET() {
  // Check if user data was injected by auth service
  if (global.serviceUserStorage.size === 0) {
    return NextResponse.json({ error: 'No user data found' }, { status: 404 });
  }
  
  // Return injected user data
  const userData = Array.from(global.serviceUserStorage.values())[0];
  return NextResponse.json(userData);
}
```

## 🔒 OAuth 2.0 Secure Endpoints Implementation

### Backend Security Architecture

The backend APIs are now secured using OAuth 2.0 Bearer tokens with the following components:

#### 1. OAuth 2.0 Middleware
```csharp
// backend/backend-user/Middleware/OAuth2Middleware.cs
public async Task InvokeAsync(HttpContext context, IOAuth2Service oauth2Service)
{
    // Extract Bearer token from Authorization header
    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
    if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Unauthorized: No valid token provided");
        return;
    }

    var token = authHeader.Substring("Bearer ".Length);
    
    // Validate token with OAuth2Service
    var oauthProvider = await oauth2Service.ValidateAccessTokenAsync(token);
    if (oauthProvider == null)
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Unauthorized: Invalid token");
        return;
    }

    // Add user context to request
    context.Items["User"] = await oauth2Service.GetUserByAccessTokenAsync(token);
    await _next(context);
}
```

#### 2. OAuth 2.0 Service Implementation
```csharp
// backend/backend-user/Services/OAuth2Service.cs
public class OAuth2Service : IOAuth2Service
{
    public async Task<OAuthProvider?> ValidateAccessTokenAsync(string token)
    {
        // Find OAuth provider by access token
        return await _oauthProviderRepository.GetByAccessTokenAsync(token);
    }

    public async Task<User?> GetUserByAccessTokenAsync(string token)
    {
        var oauthProvider = await ValidateAccessTokenAsync(token);
        if (oauthProvider == null) return null;
        
        return await _userRepository.GetByIdAsync(oauthProvider.UserId);
    }

    public async Task<bool> RefreshAccessTokenAsync(string refreshToken)
    {
        // Implement token refresh logic
        var oauthProvider = await _oauthProviderRepository.GetByRefreshTokenAsync(refreshToken);
        if (oauthProvider == null) return false;
        
        // Refresh token logic here
        return true;
    }
}
```

#### 3. Frontend Authenticated API Client
```typescript
// frontend/auth-user-service/lib/api/authenticated-client.ts
export class AuthenticatedApiClient {
  static async request<T>(
    endpoint: string,
    options: ApiClientOptions = {}
  ): Promise<T> {
    const { method = 'GET', body, headers, requireAuth = true } = options

    const requestHeaders: Record<string, string> = {
      'Content-Type': 'application/json',
      ...headers,
    }

    // Add OAuth 2.0 Bearer token if required
    if (requireAuth) {
      const session = await getSession()
      if (session?.accessToken) {
        requestHeaders['Authorization'] = `Bearer ${session.accessToken}`
      } else {
        throw new Error('No access token available. Please sign in.')
      }
    }

    const response = await fetch(`${AuthenticatedApiClient.baseUrl}${endpoint}`, config)
    
    if (!response.ok) {
      if (response.status === 401) {
        throw new Error('Unauthorized: Please sign in again')
      }
      // Handle other errors...
    }

    return await response.json();
  }
}
```

#### 4. NextAuth Integration with OAuth 2.0
```typescript
// frontend/auth-user-service/lib/auth/auth-options.ts
async jwt({ token, account, user }) {
  if (account && user) {
    try {
      const userData = await getUserByEmail(user.email!);
      if (userData) {
        // Fetch OAuth provider access token from backend
        const backendUrl = process.env.NEXT_PUBLIC_USER_API_URL || 'http://localhost:5200';
        const url = `${backendUrl}/api/users/${userData.id}/oauth-providers/${getProviderNumber(account.provider)}`;
        
        const response = await fetch(url, {
          method: 'GET',
          headers: { 'Content-Type': 'application/json' },
        });
        
        if (response.ok) {
          const oauthData = await response.json();
          if (oauthData.exists && oauthData.provider) {
            token.accessToken = oauthData.provider.accessToken;
            token.userId = userData.id;
          }
        }
      }
    } catch (error) {
      console.error('Error fetching OAuth access token:', error);
    }
  }
  return token;
}
```

### Protected Endpoints

The following endpoints are now secured with OAuth 2.0 Bearer tokens:

#### Backend User Service (Port 5200)
- ✅ `GET /api/users/{id}` - Get user profile
- ✅ `PUT /api/users/{id}` - Update user profile
- ✅ `GET /api/users/{id}/oauth-providers` - Get user OAuth providers
- ✅ `POST /api/users/{id}/oauth-providers` - Add OAuth provider
- ✅ `DELETE /api/users/{id}/oauth-providers/{providerId}` - Remove OAuth provider
- ✅ `PUT /api/users/{id}/oauth-providers/{providerId}` - Update OAuth provider

#### Unauthenticated Endpoints (During Auth Flow)
- ✅ `POST /api/users/login` - User login
- ✅ `POST /api/users/register` - User registration
- ✅ `GET /api/users/check-email` - Check email availability
- ✅ `GET /api/users/oauth-providers/check` - Check OAuth provider
- ✅ `GET /api/users/email/{email}` - Get user by email (during auth)
- ✅ `GET /api/users/{userId}/oauth-providers/{providerId}` - Get OAuth provider (during auth)

### OAuth 2.0 Token Flow

```mermaid
sequenceDiagram
    participant User
    participant Frontend as NextAuth Frontend
    participant Backend as .NET Backend
    participant Database as PostgreSQL

    User->>Frontend: 1. Sign in with OAuth
    Frontend->>Backend: 2. Create/update OAuth provider
    Backend->>Database: 3. Store access token
    Frontend->>Backend: 4. Fetch OAuth provider data
    Backend->>Frontend: 5. Return access token
    Frontend->>Frontend: 6. Store in NextAuth session
    User->>Frontend: 7. Make authenticated request
    Frontend->>Backend: 8. Include Bearer token
    Backend->>Backend: 9. Validate token
    Backend->>Frontend: 10. Return protected data
```

### Database Schema

The OAuth 2.0 implementation uses the existing `oauth_providers` table:

```sql
-- Existing oauth_providers table structure
CREATE TABLE oauth_providers (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    provider OAuthProviderType NOT NULL, -- 0=Google, 1=GitHub, 2=LinkedIn, 3=Facebook
    provider_id VARCHAR(255) NOT NULL,
    provider_email VARCHAR(255) NOT NULL,
    access_token TEXT NOT NULL,           -- 🔥 OAuth 2.0 access token
    refresh_token TEXT,                   -- 🔥 OAuth 2.0 refresh token
    token_expires_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);
```

### Security Features

- ✅ **Bearer Token Authentication** - All protected endpoints require valid OAuth 2.0 tokens
- ✅ **Token Validation** - Backend validates tokens against database
- ✅ **Automatic Token Injection** - Frontend automatically includes tokens in requests
- ✅ **Unauthorized Handling** - Proper 401 responses for invalid tokens
- ✅ **Session Integration** - Tokens stored in NextAuth session
- ✅ **Cross-Service Security** - All backend APIs can be secured with same pattern

## 📁 Complete File Structure

### Auth-User-Service (Primary Authentication)
```
auth-user-service/
├── lib/auth/
│   ├── auth-options.ts              # Enhanced NextAuth with OAuth 2.0
│   ├── custom-signout.ts            # Comprehensive logout
│   └── sso-auth.ts                  # Cross-service utilities
├── lib/api/
│   ├── authenticated-client.ts       # 🔥 OAuth 2.0 API client
│   └── user/api.ts                  # Updated with OAuth 2.0
├── lib/services/
│   └── user-injection-service.ts    # Service communication
├── lib/hooks/
│   ├── use-cross-service-auth.ts    # Cross-service detection
│   └── use-oauth-session.ts         # 🔥 OAuth session hook
├── app/api/
│   ├── services/user-injection/route.ts    # Central injection API
│   ├── auth/
│   │   ├── signout-all/route.ts            # Universal logout
│   │   └── check-signout-signal/route.ts   # Signal monitoring
│   ├── sso/callback/route.ts               # SSO handler
│   └── user/remove/route.ts                # Self cleanup
└── components/
    ├── providers.tsx                       # All providers
    ├── cross-service-auth-provider.tsx     # Cross-service detection
    └── auth-signout-monitor.tsx            # NextAuth cleanup
```

### Backend User Service (OAuth 2.0 Security)
```
backend-user/
├── Services/
│   ├── OAuth2Service.cs             # 🔥 OAuth 2.0 token validation
│   ├── Abstractions/
│   │   └── IOAuth2Service.cs        # 🔥 OAuth 2.0 interface
│   └── Mappers/
│       └── OAuthProviderMapper.cs   # 🔥 Updated with AccessToken
├── Middleware/
│   └── OAuth2Middleware.cs          # 🔥 OAuth 2.0 Bearer auth
├── Controllers/
│   └── OAuth2Controller.cs          # 🔥 OAuth 2.0 endpoints
├── DTO/
│   └── OAuthProvider/Response/
│       └── OAuthProviderResponseDto.cs # 🔥 Updated with tokens
├── Repositories/
│   ├── IOAuthProviderRepository.cs  # 🔥 Updated with token methods
│   └── OAuthProviderRepository.cs   # 🔥 Token lookup methods
└── Data/
    └── UserDbContext.cs             # Entity Framework setup
```

### Other Services (Admin, Home-Portfolio, Messages)
```
service/
├── lib/
│   ├── auth/sso-auth.ts             # JWT verification & sessions
│   └── contexts/
│       ├── auth-context.tsx         # Custom authentication
│       └── user-context.tsx         # User data management
├── app/api/user/
│   ├── inject/route.ts              # Receives user data
│   ├── remove/route.ts              # Removes user data
│   ├── get/route.ts                 # Local user access
│   └── auth/verify-sso/route.ts     # Server-side JWT verification
├── components/
│   ├── providers.tsx                # AuthProvider setup
│   └── header.tsx                   # Login/logout UI
└── middleware.ts                    # Request handling
```

## 🔒 Security Implementation

### JWT Token Security
```javascript
// 5-minute expiration for security
const token = await new SignJWT(payload)
  .setProtectedHeader({ alg: 'HS256' })
  .setExpirationTime('5m')
  .sign(secret);
```

### Service-to-Service Authentication
```javascript
// All inter-service calls use authentication header
headers: {
  'X-Service-Auth': process.env.SERVICE_AUTH_SECRET,
  'Content-Type': 'application/json'
}
```

### Session Management
```javascript
// Local sessions with 24-hour expiration
const sessionData = {
  email: user.email,
  userId: user.id,
  timestamp: Date.now(),
  expires: Date.now() + (24 * 60 * 60 * 1000) // 24 hours
};
```

## 🔧 Environment Configuration

### Required Variables (All Services)
```env
# JWT & Service Authentication
AUTH_SECRET=your-jwt-secret-key-here
SERVICE_AUTH_SECRET=your-service-auth-secret-here

# Service URLs
NEXT_PUBLIC_AUTH_USER_SERVICE=http://localhost:3000
NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE=http://localhost:3001
NEXT_PUBLIC_MESSAGES_SERVICE=http://localhost:3002
NEXT_PUBLIC_ADMIN_SERVICE=http://localhost:3003

# Backend API URLs (OAuth 2.0 Secure Endpoints)
NEXT_PUBLIC_USER_API_URL=http://localhost:5200
NEXT_PUBLIC_PORTFOLIO_API_URL=http://localhost:5201
NEXT_PUBLIC_MESSAGES_API_URL=http://localhost:5093

# OAuth Credentials (Auth Service Only)
AUTH_GITHUB_ID=your-github-oauth-id
AUTH_GITHUB_SECRET=your-github-oauth-secret
AUTH_GOOGLE_ID=your-google-oauth-id
AUTH_GOOGLE_SECRET=your-google-oauth-secret
AUTH_FACEBOOK_ID=your-facebook-oauth-id
AUTH_FACEBOOK_SECRET=your-facebook-oauth-secret
AUTH_LINKEDIN_ID=your-linkedin-oauth-id
AUTH_LINKEDIN_SECRET=your-linkedin-oauth-secret

# NextAuth Configuration (Auth Service Only)
NEXTAUTH_URL=http://localhost:3000
NEXTAUTH_SECRET=your-nextauth-secret-key-here
```

