# Pull Request: Refactor Admin Authentication & Improve Sign-Out UX

## Overview

This PR introduces a major refactor of the admin authentication system, moving from a legacy SSO/context-based approach to a modern, JWT-based authentication context. It also improves the sign-out experience and loading overlays, and removes obsolete code and API routes.

---

## Commits

- **24a33d3**: fixed is_admin and loading overlay on signing out (Theo3883, 2025-08-21)
- **6cea2e9**: fixed login into admin (Theo3883, 2025-08-21)

---

## Detailed Code Changes

### 1. Authentication Refactor

- **Removed legacy SSO and custom sign-out logic:**
  - Deleted `lib/auth/sso-auth.ts` and `lib/auth/custom-signout.ts`.
  - Deleted `lib/contexts/auth-context.tsx` and all usage.
  - Removed related API routes:  
    - `app/api/auth/local-logout/route.ts`
    - `app/api/auth/verify-sso/route.ts`
    - `app/api/user/get/route.ts`
    - `app/api/user/inject/route.ts`
    - `app/api/user/remove/route.ts`

- **Introduced JWT-based authentication:**
  - Added `lib/contexts/jwt-auth-context.tsx`:
    - Provides a `JWTAuthProvider` and `useJWTAuth` hook.
    - Handles JWT storage, verification, auto-login, and user state.
    - Supports login via SSO token in URL, auto-login via cookie, and token refresh.
    - Handles sign-out by clearing tokens and redirecting to a cascade sign-out endpoint.
  - Added `lib/hooks/use-auth.ts`:
    - Exposes a simplified hook for authentication state and actions, wrapping `useJWTAuth`.

- **Centralized service URLs:**
  - Added `lib/config/services.ts`:
    - Exports a `SERVICES` object for all service base URLs.
    - Provides helpers for building service URLs and redirection.

- **Refactored authenticated API client:**
  - Updated `lib/authenticated-client.ts`:
    - Uses JWT from local/session storage.
    - Decodes JWT payload to extract access tokens.
    - Handles 401 responses by attempting token refresh and fallback to login.
    - Uses new `SERVICES` config for service URLs.

### 2. User Context Refactor

- **Updated user context to use new auth:**
  - Refactored `lib/contexts/user-context.tsx`:
    - Now uses `useAuth` from the new JWT context.
    - Maps JWT user data to the local `User` interface.
    - Handles user state updates and profile redirection via new service config.

### 3. UI/UX Improvements

- **Sign-out handler:**
  - Added `components/signout-handler.tsx`:
    - Cleans up JWT tokens from storage when `?signout=1` is present in the URL.

- **Loading overlay improvements:**
  - Updated `auth-user-service/components/loader/loading-overlay.tsx`:
    - Added `backgroundColor` prop for customizable overlay backgrounds.
  - Updated `auth-user-service/app/signing-out/page.tsx`:
    - Uses the new `backgroundColor` prop for a consistent sign-out experience.

- **Admin components:**
  - Updated admin header, guard, and modal components to use new authentication and user context.

### 4. Code Cleanup

- **Removed obsolete files and code:**
  - Deleted unused API routes and legacy authentication files.
  - Removed over 600 lines of code, reducing complexity and maintenance burden.

- **Updated imports and usage:**
  - All authentication and user state now flow through the new JWT context and hooks.
  - All service URLs are now managed centrally.

---

## File Change Summary

- **22 files changed**
- **367 insertions**
- **692 deletions**

Key files added/removed/modified:
- Added:  
  - `frontend/admin-service/lib/contexts/jwt-auth-context.tsx`
  - `frontend/admin-service/lib/hooks/use-auth.ts`
  - `frontend/admin-service/lib/config/services.ts`
  - `frontend/admin-service/components/signout-handler.tsx`
- Removed:  
  - `frontend/admin-service/lib/auth/sso-auth.ts`
  - `frontend/admin-service/lib/auth/custom-signout.ts`
  - `frontend/admin-service/lib/contexts/auth-context.tsx`
  - Multiple API route files under `frontend/admin-service/app/api/`
- Modified:  
  - `frontend/admin-service/lib/authenticated-client.ts`
  - `frontend/admin-service/lib/contexts/user-context.tsx`
  - `frontend/auth-user-service/app/signing-out/page.tsx`
  - `frontend/auth-user-service/components/loader/loading-overlay.tsx`
  - Admin UI components

---

## Technical Highlights

- **JWT Auth Context**:  
  - Handles all authentication logic, including login, logout, token verification, and auto-login.
  - Exposes user state and actions via React context and hooks.
  - Ensures secure token storage and cleanup.

- **Service URL Management**:  
  - All service endpoints are now managed in a single config, reducing risk of misconfiguration.

- **Sign-Out Flow**:  
  - Sign-out now cascades across all services, ensuring complete session termination.
  - UI feedback during sign-out is improved and visually consistent.

- **User Context**:  
  - User state is now derived from JWT, ensuring consistency and reducing reliance on injected user data.

---

## Breaking Changes

- All authentication and user state must now use the new JWT context and hooks.
- Legacy SSO and custom sign-out logic are fully removed.
- API routes for user injection and SSO verification are deleted.

---

## Testing

- Manual testing of login, sign-out, and admin access flows.
- Verified UI for loading overlays and sign-out transitions.
- Confirmed token cleanup and session invalidation across tabs and services.

---

## Motivation

This refactor modernizes authentication, improves security, and lays a scalable foundation for future admin features. It also reduces technical debt and simplifies the codebase.

---

**Please review the code, test authentication and sign-out flows, and ensure all admin features work as expected before merging.**
