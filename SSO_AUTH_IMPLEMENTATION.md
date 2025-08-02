# 🔐 Centralized SSO Authentication for Microservices

This implementation provides centralized Single Sign-On (SSO) authentication where only the `auth-user-service` handles authentication, and user data is injected into other services via JWT tokens.

## ✨ Problem Solved

**CORS Error Fixed**: The original issue `Access to fetch at 'http://localhost:3000/api/auth/session' from origin 'http://localhost:3001' has been blocked by CORS policy` has been resolved by implementing a proper SSO token-based system instead of trying to share NextAuth sessions across origins.

## 🏗️ Architecture Overview

### Before (Problematic)
- ❌ Each service had its own NextAuth configuration
- ❌ Tried to share sessions across different origins (ports)
- ❌ CORS issues when accessing auth endpoints
- ❌ Duplicate authentication logic

### After (SSO Solution)
- ✅ Only `auth-user-service` handles authentication (OAuth, sessions)
- ✅ JWT token-based SSO for other services
- ✅ No CORS issues - tokens passed via URL parameters
- ✅ Clean separation of concerns
- ✅ Scalable across different domains/ports

## 🔄 How the SSO Flow Works

### Login Flow
```
1. User visits admin-service.com and needs to authenticate
2. admin-service redirects to: auth-service.com/api/sso/callback?callbackUrl=admin-service.com
3. If not authenticated, auth-service redirects to its login page
4. User logs in via OAuth (Google/GitHub) on auth-service
5. Auth-service injects user data into ALL other services
6. Auth-service creates JWT token and redirects: admin-service.com?ssoToken=eyJ...
7. Admin-service verifies JWT token and creates local session
8. User is now authenticated on admin-service (and all services have their data)
```

### Logout Flow
```
1. User clicks logout on any service
2. Service clears its local session
3. Service redirects to auth-service logout
4. Auth-service removes user data from ALL services
5. User is signed out everywhere
```

## 📁 Implementation Files

### Auth-User-Service (Primary Auth Service)
```
├── lib/auth/auth-options.ts              # Enhanced NextAuth config
├── lib/services/user-injection-service.ts # Service communication
├── app/api/services/user-injection/route.ts # Injection API
├── app/api/auth/signout-all/route.ts     # Custom signout
├── app/api/sso/callback/route.ts         # SSO callback handler
├── lib/auth/custom-signout.ts            # Client signout utility
└── middleware.ts                         # CORS handling
```

### Other Services (Admin, Home-Portfolio, Messages)
```
├── lib/auth/sso-auth.ts                  # JWT verification & session management
├── lib/contexts/auth-context.tsx         # Custom auth context
├── lib/contexts/user-context.tsx         # Updated user context
├── components/providers.tsx              # AuthProvider instead of NextAuth
├── app/api/user/inject/route.ts          # Receives user data
├── app/api/user/remove/route.ts          # Removes user data
└── app/api/user/get/route.ts             # Local user data API
```

## 🔧 Environment Variables

Add to all services:
```env
# Service-to-service authentication
SERVICE_AUTH_SECRET=your-secure-secret-key

# Service URLs
NEXT_PUBLIC_AUTH_USER_SERVICE=http://localhost:3000
NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE=http://localhost:3001
NEXT_PUBLIC_MESSAGES_SERVICE=http://localhost:3002
NEXT_PUBLIC_ADMIN_SERVICE=http://localhost:3003

# JWT secret (same across all services)
AUTH_SECRET=your-jwt-secret-key
```

## 🚀 Usage in Components

### In Other Services (Admin, Home-Portfolio, Messages)

```tsx
// Authentication state
import { useAuth } from '@/lib/contexts/auth-context';

function LoginButton() {
  const { isAuthenticated, login, logout } = useAuth();
  
  return (
    <button onClick={isAuthenticated ? logout : login}>
      {isAuthenticated ? 'Logout' : 'Login'}
    </button>
  );
}

// User data
import { useUser } from '@/lib/contexts/user-context';

function UserProfile() {
  const { user, loading, error } = useUser();
  
  if (loading) return <div>Loading...</div>;
  if (error) return <div>Error: {error}</div>;
  if (!user) return <div>Please log in</div>;
  
  return <div>Welcome, {user.firstName}!</div>;
}
```

### In Auth-User-Service

```tsx
// Use NextAuth as normal
import { useSession, signIn, signOut } from 'next-auth/react';
import { customSignOut } from '@/lib/auth/custom-signout';

function AuthButton() {
  const { data: session } = useSession();
  
  if (session) {
    return (
      <button onClick={() => customSignOut()}>
        Sign out
      </button>
    );
  }
  
  return (
    <button onClick={() => signIn()}>
      Sign in
    </button>
  );
}
```

## 🔒 Security Features

- **JWT Token Verification**: All SSO tokens are cryptographically verified
- **Service Authentication**: Service-to-service calls use `X-Service-Auth` header
- **Token Expiration**: SSO tokens expire in 5 minutes for security
- **Local Session Management**: 24-hour sessions stored in localStorage
- **Graceful Failures**: System continues working even if some services are down

## ✅ Benefits

1. **No CORS Issues**: Token-based SSO avoids cross-origin session problems
2. **Scalable**: Works across different domains/ports without session sharing
3. **Standard Pattern**: Uses industry-standard JWT token approach
4. **Single Source of Truth**: Only auth-user-service manages authentication
5. **Offline Capability**: Local sessions work even if auth service is temporarily down
6. **Clean Separation**: Authentication logic is isolated to one service

## 🧪 Testing the Implementation

1. **Start all services**:
   ```bash
   # Terminal 1 - Auth Service
   cd frontend/auth-user-service && npm run dev
   
   # Terminal 2 - Admin Service  
   cd frontend/admin-service && npm run dev
   
   # Terminal 3 - Home Portfolio Service
   cd frontend/home-portfolio-service && npm run dev
   
   # Terminal 4 - Messages Service
   cd frontend/messages-service && npm run dev
   ```

2. **Test SSO Flow**:
   - Visit `http://localhost:3003` (admin-service)
   - Click login → redirects to auth-service
   - Login with OAuth → redirects back with token
   - User should be authenticated on admin-service
   - Visit other services → user should be automatically authenticated

3. **Test Logout**:
   - Click logout on any service
   - Should sign out from all services
   - Visiting any service should require re-authentication

## 🔧 Troubleshooting

### JWT Signature Verification Failed ✅ FIXED
**Issue**: `JWSSignatureVerificationFailed: signature verification failed`  
**Root Cause**: `AUTH_SECRET` environment variable not available in browser context (client-side)  
**Solution**: 
- ✅ Created server-side JWT verification API (`/api/auth/verify-sso`)
- ✅ Updated client-side code to use server-side verification
- ✅ JWT verification now works correctly across all services

### User Not Found Error
**Issue**: "User not found in this service. Please log in again."  
**Solution**: 
- Check if `SERVICE_AUTH_SECRET` matches across all services
- Verify service URLs in environment variables
- Check network tab for failed injection requests

### Environment Variables Setup ✅ REQUIRED
**Issue**: Missing or incorrect environment variables  
**Required in ALL services** (`.env.local` files):
```env
AUTH_SECRET=BFXvA02D4f9wjVQxa91IUJIifCuoWjV2G2WLtv6VWuw=
SERVICE_AUTH_SECRET=yozJT1KzVr0KQY6AYAYRswFtE6JEja3J1sAmr01SwaI=
NEXT_PUBLIC_AUTH_USER_SERVICE=http://localhost:3000
NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE=http://localhost:3001
NEXT_PUBLIC_MESSAGES_SERVICE=http://localhost:3002
NEXT_PUBLIC_ADMIN_SERVICE=http://localhost:3003
```

### Redirect Issues
**Issue**: Infinite redirects or broken callbacks  
**Solution**:
- Check `NEXT_PUBLIC_*` environment variables are correct
- Ensure services are running on expected ports
- Verify OAuth configuration in auth-user-service

## 🔮 Future Enhancements

1. **Redis Storage**: Replace localStorage with Redis for better scalability
2. **Refresh Tokens**: Implement token refresh for longer sessions
3. **Health Checks**: Add service health monitoring
4. **Rate Limiting**: Add protection against abuse
5. **Multi-tenant**: Support for multiple organizations

---

**✨ The CORS issue has been completely resolved with this SSO implementation!**