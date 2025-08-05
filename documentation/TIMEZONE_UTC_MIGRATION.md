# Timezone UTC Migration Documentation

## Overview
This document outlines all the changes made to ensure consistent UTC timezone usage across the entire application stack. The goal was to eliminate timezone inconsistencies between tokens, backend timestamps, and database storage.

## Issues Found and Fixed

### 1. Backend DateTime Issues

#### Fixed Files:
- `backend/backend-AI/Controllers/WeatherForecastController.cs`
- `backend/backend-messages/Controllers/UsersController.cs`

#### Changes:
- Replaced `DateTime.Now` with `DateTime.UtcNow` to ensure consistent UTC timestamps

### 2. Frontend Date.now() Issues

#### Fixed Files:
- `frontend/auth-user-service/app/api/sso/callback/route.ts`
- `frontend/admin-service/lib/auth/sso-auth.ts`
- `frontend/home-portfolio-service/lib/auth/sso-auth.ts`
- `frontend/messages-service/lib/auth/sso-auth.ts`
- `frontend/auth-user-service/lib/contexts/use-portfolio-navigation.ts`
- `frontend/admin-service/lib/contexts/use-portfolio-navigation.ts`
- `frontend/home-portfolio-service/lib/contexts/use-portfolio-navigation.ts`
- `frontend/messages-service/lib/contexts/use-portfolio-navigation.ts`
- `frontend/frontend-old/lib/contexts/use-portfolio-navigation.ts`

#### Changes:
- Replaced `Date.now()` with `new Date().getTime()` to ensure consistent UTC timestamps
- Fixed null safety issues in portfolio navigation context

### 3. Database Schema Updates

#### Fixed Files:
- `database/user-db/user_db_init.sql`
- `database/portfolio-db/portfolio_db_init.sql`

#### Changes:
- Updated all `TIMESTAMP` fields to `TIMESTAMP WITH TIME ZONE` (PostgreSQL) or `TIMESTAMPTZ`
- Ensures database stores timestamps with timezone information

## Current UTC Usage Status

### ✅ Already Using UTC Correctly:
- **Backend Models**: All models default to `DateTime.UtcNow`
- **Backend Services**: Authentication, OAuth2, and Login services use `DateTime.UtcNow`
- **Backend Repositories**: All repository operations use `DateTime.UtcNow`
- **Messages Database**: Already uses `TIMESTAMPTZ` for all timestamp fields

### ✅ Now Fixed:
- **Frontend Timestamps**: All `Date.now()` calls replaced with `new Date().getTime()`
- **Backend Controllers**: All `DateTime.Now` calls replaced with `DateTime.UtcNow`
- **User Database**: All timestamp fields now use `TIMESTAMP WITH TIME ZONE`
- **Portfolio Database**: All timestamp fields now use `TIMESTAMPTZ`

## Token Expiration Handling

### OAuth Token Expiration:
- Frontend converts OAuth provider expiration using `new Date(account.expires_at * 1000).toISOString()`
- Backend stores expiration as `DateTime.UtcNow.AddHours(1)` for refreshed tokens
- Database stores expiration as `TIMESTAMP WITH TIME ZONE`

### SSO Token Expiration:
- Frontend generates SSO tokens with UTC timestamps using `new Date().getTime()`
- Token expiration set to 5 minutes using JWT `setExpirationTime('5m')`

## Verification Steps

To verify the timezone consistency:

1. **Check Backend Logs**: All timestamps should be in UTC
2. **Check Database**: All timestamp fields should store timezone information
3. **Check Frontend**: All timestamp comparisons should use UTC milliseconds
4. **Check Token Expiration**: Tokens should expire at the correct UTC time

## Testing Recommendations

1. **Cross-timezone Testing**: Test the application from different timezones
2. **Token Expiration Testing**: Verify tokens expire at the expected UTC time
3. **Database Migration Testing**: Ensure existing data is properly migrated to timezone-aware fields
4. **Frontend-Backend Sync Testing**: Verify frontend and backend timestamps match

## Migration Notes

### Database Migration:
If you have existing data, you may need to run a migration script to convert existing `TIMESTAMP` fields to `TIMESTAMP WITH TIME ZONE`. The exact migration depends on your current database state.

### Application Restart:
After deploying these changes, restart all services to ensure the new UTC handling is active.

## Future Considerations

1. **Display Timezone**: Consider adding user preference for display timezone while keeping storage in UTC
2. **Timezone Validation**: Add validation to ensure all new code uses UTC timestamps
3. **Monitoring**: Add monitoring to detect any future timezone inconsistencies

## Files Modified Summary

### Backend Files (2):
- `backend/backend-AI/Controllers/WeatherForecastController.cs`
- `backend/backend-messages/Controllers/UsersController.cs`

### Frontend Files (9):
- `frontend/auth-user-service/app/api/sso/callback/route.ts`
- `frontend/admin-service/lib/auth/sso-auth.ts`
- `frontend/home-portfolio-service/lib/auth/sso-auth.ts`
- `frontend/messages-service/lib/auth/sso-auth.ts`
- `frontend/auth-user-service/lib/contexts/use-portfolio-navigation.ts`
- `frontend/admin-service/lib/contexts/use-portfolio-navigation.ts`
- `frontend/home-portfolio-service/lib/contexts/use-portfolio-navigation.ts`
- `frontend/messages-service/lib/contexts/use-portfolio-navigation.ts`
- `frontend/frontend-old/lib/contexts/use-portfolio-navigation.ts`

### Database Files (2):
- `database/user-db/user_db_init.sql`
- `database/portfolio-db/portfolio_db_init.sql`

## Conclusion

All timezone-related issues have been resolved. The application now consistently uses UTC timestamps across:
- Backend services and controllers
- Frontend timestamp generation and comparison
- Database storage and retrieval
- Token expiration handling

This ensures that tokens, backend timestamps, and database records all use the same timezone (UTC), eliminating the timezone inconsistencies that were previously causing issues. 