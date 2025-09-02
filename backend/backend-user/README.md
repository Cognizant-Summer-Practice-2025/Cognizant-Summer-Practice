# Backend User Service

## Table of Contents

### [1. Overview](#1-overview)
### [2. Architecture](#2-architecture)
- [2.1 Technology Stack](#21-technology-stack)
- [2.2 Project Structure](#22-project-structure)

### [3. Core Components](#3-core-components)
- [3.1 Data Models](#31-data-models)
  - [3.1.1 User Entity](#311-user-entity)
  - [3.1.2 OAuth Provider Entity](#312-oauth-provider-entity)
  - [3.1.3 Supporting Entities](#313-supporting-entities)
- [3.2 Database Layer](#32-database-layer)
  - [3.2.1 UserDbContext](#321-userdbcontext)
  - [3.2.2 Repository Pattern](#322-repository-pattern)
- [3.3 Service Layer](#33-service-layer)
  - [3.3.1 Core Services](#331-core-services)
  - [3.3.2 Service Abstractions](#332-service-abstractions)
- [3.4 API Controllers](#34-api-controllers)
  - [3.4.1 UsersController](#341-userscontroller)
  - [3.4.2 OAuth2Controller](#342-oauth2controller)

### [4. Data Flow Architecture](#4-data-flow-architecture)
- [4.1 User Registration Flow](#41-user-registration-flow)
- [4.2 OAuth Authentication Flow](#42-oauth-authentication-flow)
- [4.3 User Search Flow](#43-user-search-flow)
- [4.4 Bookmark Management Flow](#44-bookmark-management-flow)

### [5. Configuration](#5-configuration)
- [5.1 Database Configuration](#51-database-configuration)
- [5.2 Service Registration](#52-service-registration)
- [5.3 CORS Configuration](#53-cors-configuration)

### [6. Implementation Patterns & Code Examples](#6-implementation-patterns--code-examples)
- [6.1 Validation Framework](#61-validation-framework)
  - [6.1.1 UserValidator](#611-uservalidator)
  - [6.1.2 ValidationResult Pattern](#612-validationresult-pattern)
- [6.2 Mapping System](#62-mapping-system)
  - [6.2.1 UserMapper](#621-usermapper)
- [6.3 Error Handling Patterns](#63-error-handling-patterns)
  - [6.3.1 Controller-Level Error Handling](#631-controller-level-error-handling)
  - [6.3.2 Service-Level Error Handling](#632-service-level-error-handling)
  - [6.3.3 Repository-Level Error Handling](#633-repository-level-error-handling)
- [6.4 Async/Await Patterns](#64-asyncawait-patterns)
  - [6.4.1 Proper Async Implementation](#641-proper-async-implementation)
- [6.5 Dependency Injection Patterns](#65-dependency-injection-patterns)
  - [6.5.1 Constructor Injection](#651-constructor-injection)
  - [6.5.2 Service Registration](#652-service-registration)

### [7. OAuth 2.0 Implementation & Security](#7-oauth-20-implementation--security)
- [7.1 Supported Providers](#71-supported-providers)
- [7.2 OAuth Flow Implementation](#72-oauth-flow-implementation)
  - [7.2.1 Token Validation Process](#721-token-validation-process)
  - [7.2.2 Automatic Token Refresh](#722-automatic-token-refresh)
  - [7.2.3 Provider-Specific Refresh Logic](#723-provider-specific-refresh-logic)
  - [7.2.4 Google Token Refresh Implementation](#724-google-token-refresh-implementation)
- [7.3 Security Considerations](#73-security-considerations)
  - [7.3.1 Token Storage Security](#731-token-storage-security)
  - [7.3.2 Environment Variable Management](#732-environment-variable-management)
  - [7.3.3 Token Validation Security](#733-token-validation-security)
  - [7.3.4 OAuth Provider Security](#734-oauth-provider-security)
- [7.4 Token Management](#74-token-management)

### [8. Performance Optimizations & Database Design](#8-performance-optimizations--database-design)
- [8.1 Database Indexes & Query Optimization](#81-database-indexes--query-optimization)
  - [8.1.1 Strategic Index Design](#811-strategic-index-design)
  - [8.1.2 Query Optimization Strategies](#812-query-optimization-strategies)
- [8.2 Entity Framework Core Optimization](#82-entity-framework-core-optimization)
  - [8.2.1 DbContext Configuration](#821-dbcontext-configuration)
  - [8.2.2 Query Performance Best Practices](#822-query-performance-best-practices)
- [8.3 Connection Pooling & Resource Management](#83-connection-pooling--resource-management)
  - [8.3.1 Database Connection Optimization](#831-database-connection-optimization)
  - [8.3.2 Memory Management](#832-memory-management)
- [8.4 Query Optimization](#84-query-optimization)

### [9. Testing Strategy & Development Best Practices](#9-testing-strategy--development-best-practices)
- [9.1 Comprehensive Testing Approach](#91-comprehensive-testing-approach)
  - [9.1.1 Test Structure & Organization](#911-test-structure--organization)
  - [9.1.2 Unit Testing Strategy](#912-unit-testing-strategy)
  - [9.1.3 Integration Testing](#913-integration-testing)
  - [9.1.4 Test Configuration & Setup](#914-test-configuration--setup)
- [9.2 Development Best Practices](#92-development-best-practices)
  - [9.2.1 Code Organization & Standards](#921-code-organization--standards)
  - [9.2.2 Error Handling Best Practices](#922-error-handling-best-practices)
  - [9.2.3 Performance Best Practices](#923-performance-best-practices)
  - [9.2.4 Security Best Practices](#924-security-best-practices)

### [10. Deployment](#10-deployment)
- [10.1 Environment Configuration](#101-environment-configuration)
- [10.2 Docker Support](#102-docker-support)

### [11. Stripe Payments & Subscriptions](#11-stripe-payments--subscriptions)

### [12. API Endpoints Summary](#12-api-endpoints-summary)
- [11.1 User Management](#111-user-management)
- [11.2 OAuth Operations](#112-oauth-operations)
- [11.3 Bookmark Management](#113-bookmark-management)
- [11.4 User Reporting](#114-user-reporting)

### [13. Future Enhancements](#13-future-enhancements)
- [12.1 Planned Features](#121-planned-features)
- [12.2 Scalability Improvements](#122-scalability-improvements)

### [14. Contributing](#14-contributing)
- [13.1 Development Guidelines](#131-development-guidelines)
- [13.2 Code Review Checklist](#132-code-review-checklist)

### [15. Support](#15-support)

---

## 1. Overview

The Backend User Service is a comprehensive .NET 9.0 microservice responsible for managing user accounts, authentication, OAuth providers, bookmarks, user reports, and analytics. It provides a robust foundation for user management in a portfolio showcase platform.

## 2. Architecture

### 2.1 Technology Stack
- **Framework**: .NET 9.0
- **Database**: PostgreSQL with Entity Framework Core
- **Authentication**: OAuth 2.0 with multiple providers (Google, GitHub, LinkedIn, Facebook)
- **API Documentation**: Swagger/OpenAPI
- **Validation**: Custom validation framework
- **Logging**: Built-in .NET logging with ILogger

### 2.2 Project Structure
```
backend-user/
├── Config/                          # Configuration classes
├── Controllers/                     # API endpoints
├── Data/                           # Database context and configuration
├── DTO/                            # Data Transfer Objects
├── Extensions/                      # Extension methods
├── Middleware/                      # Custom middleware
├── Models/                          # Entity models
├── Repositories/                    # Data access layer
├── Services/                        # Business logic layer
│   ├── Abstractions/               # Service interfaces
│   ├── Mappers/                    # Entity-DTO mappers
│   └── Validators/                 # Validation logic
└── Tests/                          # Unit tests
```

## 3. Core Components

### 3.1 Data Models

#### 3.1.1 User Entity
The central entity representing user accounts with the following properties:

**Identity Properties:**
- `Id` (Guid): Primary key, auto-generated using `Guid.NewGuid()`
- `Email` (string, 255 chars): Unique email address with validation attributes
- `Username` (string, 100 chars): Auto-generated from email (before @ symbol)

**Profile Properties:**
- `FirstName` (string, 100 chars): User's first name (required)
- `LastName` (string, 100 chars): User's last name (required)
- `ProfessionalTitle` (string, 200 chars): Job title or professional designation
- `Bio` (string): Unlimited text for user biography
- `Location` (string, 100 chars): Geographic location
- `AvatarUrl` (string): Profile image URL

**Status Properties:**
- `IsActive` (bool): Account status, defaults to true
- `IsAdmin` (bool): Administrative privileges, defaults to false
- `LastLoginAt` (DateTime?): Timestamp of last successful login

**Timestamps:**
- `CreatedAt` (DateTime): Account creation time, auto-set to UTC
- `UpdatedAt` (DateTime): Last modification time, auto-updated

**Navigation Properties (EF Core Relationships):**
- `OAuthProviders`: One-to-many relationship with OAuth providers
- `Newsletters`: One-to-many relationship with newsletter subscriptions
- `UserAnalytics`: One-to-many relationship with user behavior data
- `UserReports`: One-to-many relationship with reports about this user
- `Bookmarks`: One-to-many relationship with portfolio bookmarks

**Database Constraints:**
- Email uniqueness enforced at database level with unique index
- Username uniqueness enforced at database level with unique index
- Snake case naming convention for database columns
- Cascade delete for related entities

#### 3.1.2 OAuth Provider Entity
Manages external authentication providers with comprehensive token management:

**Provider Types (OAuthProviderType enum):**
- `Google`: Full OAuth 2.0 support with refresh tokens
- `GitHub`: OAuth app flow (limited refresh token support)
- `LinkedIn`: OAuth 2.0 with refresh token support
- `Facebook`: Long-lived access token approach

**Core Properties:**
- `Id` (Guid): Primary key for the OAuth provider record
- `UserId` (Guid): Foreign key to User entity (required)
- `Provider` (OAuthProviderType): Enum value for provider type
- `ProviderId` (string, 255 chars): Unique identifier from the OAuth provider
- `ProviderEmail` (string, 255 chars): Email associated with the OAuth account

**Token Management:**
- `AccessToken` (string): Current access token (required)
- `RefreshToken` (string?): Refresh token for token renewal (nullable)
- `TokenExpiresAt` (DateTime?): Token expiration timestamp
- `CreatedAt` (DateTime): Record creation time
- `UpdatedAt` (DateTime): Last token update time

**Database Relationships:**
- One-to-many relationship with User entity
- Cascade delete when user is removed
- Unique constraint on (Provider, ProviderId) combination
- Index on UserId for efficient lookups

**Token Refresh Strategy:**
- Google: Full refresh token support with automatic renewal
- LinkedIn: Refresh token support with automatic renewal
- GitHub: Limited refresh support (OAuth app flow)
- Facebook: Long-lived token approach (no refresh)

#### 3.1.3 Supporting Entities

**Bookmark Entity:**
- **Purpose**: Store user portfolio bookmarks for quick access
- **Properties**: `UserId`, `PortfolioId`, `PortfolioTitle`, `PortfolioOwnerName`
- **Constraints**: Unique constraint on (UserId, PortfolioId) combination
- **Use Case**: Users can bookmark portfolios they want to revisit later

**UserReport Entity:**
- **Purpose**: Enable users to report inappropriate behavior or content
- **Properties**: `UserId` (reported user), `ReportedByUserId` (reporter), `Reason`
- **Constraints**: Unique constraint on (UserId, ReportedByUserId) to prevent duplicate reports
- **Moderation**: Foundation for content moderation and user safety

**UserAnalytics Entity:**
- **Purpose**: Track user behavior and interaction patterns
- **Properties**: `SessionId`, `EventType`, `EventData` (JSON), `IpAddress`, `UserAgent`
- **Data Storage**: JSONB column for flexible event data storage
- **Use Cases**: User engagement analysis, feature usage tracking, security monitoring

**Newsletter Entity:**
- **Purpose**: Manage user subscription preferences for different communication types
- **Properties**: `Type` (newsletter category), `IsActive` (subscription status)
- **Constraints**: Unique constraint on (UserId, Type) for subscription management
- **Use Cases**: Marketing communications, feature announcements, user engagement

### 3.2 Database Layer

#### 3.2.1 UserDbContext
Entity Framework Core DbContext with comprehensive configuration:

**Entity Configuration:**
- **Snake Case Naming**: Uses `UseSnakeCaseNamingConvention()` for database columns
- **Enum Mapping**: Custom enum types mapped to PostgreSQL enum columns
- **Relationship Configuration**: Proper foreign key relationships with cascade deletes
- **Index Strategy**: Strategic indexes for performance optimization

**Database Indexes:**
```csharp
// User entity indexes
entity.HasIndex(u => u.Email).IsUnique();           // Email uniqueness
entity.HasIndex(u => u.Username).IsUnique();        // Username uniqueness

// OAuth provider indexes
entity.HasIndex(o => new { o.Provider, o.ProviderId }).IsUnique();  // Provider uniqueness
entity.HasIndex(o => o.UserId);                     // User lookup optimization

// Newsletter indexes
entity.HasIndex(n => new { n.UserId, n.Type }).IsUnique();  // Subscription uniqueness

// Analytics indexes
entity.HasIndex(ua => ua.SessionId);                // Session tracking
entity.HasIndex(ua => ua.CreatedAt);                // Time-based queries

// Report indexes
entity.HasIndex(ur => new { ur.UserId, ur.ReportedByUserId }).IsUnique();  // Report uniqueness
entity.HasIndex(ur => ur.CreatedAt);                // Time-based reporting

// Bookmark indexes
entity.HasIndex(b => new { b.UserId, b.PortfolioId }).IsUnique();  // Bookmark uniqueness
entity.HasIndex(b => b.CreatedAt);                   // Bookmark history
```

**Enum Type Mapping:**
```csharp
// Custom enum mapping for OAuth providers
dataSourceBuilder.MapEnum<OAuthProviderType>("oauth_provider_type");
```

**Cascade Delete Strategy:**
- User deletion triggers cascade delete of all related entities
- Ensures data consistency and prevents orphaned records
- Proper cleanup of OAuth providers, bookmarks, reports, and analytics

#### 3.2.2 Repository Pattern
Data access layer implementing the Repository pattern for clean separation of concerns:

**IUserRepository:**
```csharp
public interface IUserRepository
{
    Task<User?> GetUserById(Guid id);
    Task<User?> GetUserByEmail(string email);
    Task<User?> GetUserByUsername(string username);
    Task<List<User>> SearchUsers(string searchTerm);
    Task<List<User>> GetAllUsers();
    Task<User> CreateUser(User user);
    Task<User?> UpdateUser(Guid id, UpdateUserRequest request);
    Task<bool> UpdateLastLoginAsync(Guid userId, DateTime lastLoginAt);
}
```

**IOAuthProviderRepository:**
```csharp
public interface IOAuthProviderRepository
{
    Task<OAuthProvider?> GetByIdAsync(Guid id);
    Task<List<OAuthProvider>> GetByUserIdAsync(Guid userId);
    Task<OAuthProvider?> GetByProviderAndProviderIdAsync(OAuthProviderType provider, string providerId);
    Task<OAuthProvider?> GetByAccessTokenAsync(string accessToken);
    Task<OAuthProvider?> GetByRefreshTokenAsync(string refreshToken);
    Task<OAuthProvider> CreateAsync(OAuthProviderCreateRequestDto request);
    Task<OAuthProvider?> UpdateAsync(Guid id, OAuthProviderUpdateRequestDto request);
    Task<bool> DeleteAsync(Guid id);
}
```

**IBookmarkRepository:**
```csharp
public interface IBookmarkRepository
{
    Task<Bookmark> AddBookmark(Bookmark bookmark);
    Task<bool> RemoveBookmark(Guid userId, string portfolioId);
    Task<List<Bookmark>> GetUserBookmarks(Guid userId);
    Task<bool> IsBookmarked(Guid userId, string portfolioId);
}
```

**IUserReportRepository:**
```csharp
public interface IUserReportRepository
{
    Task<UserReport> CreateUserReportAsync(UserReport userReport);
    Task<List<UserReport>> GetUserReportsAsync(Guid userId);
    Task<List<UserReport>> GetReportsByReporterAsync(Guid reporterId);
    Task<bool> HasUserReportedUserAsync(Guid userId, Guid reportedByUserId);
}
```

**IUserAnalyticsRepository:**
```csharp
public interface IUserAnalyticsRepository
{
    Task<UserAnalytics> CreateUserAnalyticsAsync(UserAnalytics userAnalytics);
    Task<List<UserAnalytics>> GetUserAnalyticsAsync(Guid userId);
    Task<List<UserAnalytics>> GetUserAnalyticsBySessionAsync(string sessionId);
    Task<List<UserAnalytics>> GetUserAnalyticsByEventTypeAsync(string eventType);
    Task<int> GetUserAnalyticsCountAsync(Guid userId);
    Task<bool> DeleteUserAnalyticsAsync(Guid id);
}
```

**Repository Implementation Benefits:**
- **Testability**: Easy to mock for unit testing
- **Dependency Injection**: Scoped services for proper lifecycle management
- **Interface Segregation**: Each repository handles specific entity types
- **Data Access Abstraction**: Business logic doesn't depend on EF Core directly

### 3.3 Service Layer

#### 3.3.1 Core Services
Business logic layer implementing clean architecture principles:

**UserService:**
```csharp
public class UserService : IUserService
{
    // Core user operations with validation
    public async Task<User?> GetUserByIdAsync(Guid id)
    public async Task<User?> GetUserByEmailAsync(string email)
    public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm)
    public async Task<User> CreateUserAsync(RegisterUserRequest request)
    public async Task<User?> UpdateUserAsync(Guid id, UpdateUserRequest request)
    public async Task<object?> GetUserPortfolioInfoAsync(Guid id)
}
```
**Responsibilities**: User CRUD operations, search functionality, portfolio info retrieval

**AuthenticationService:**
```csharp
public class AuthenticationService : IAuthenticationService
{
    // OAuth authentication and token management
    public async Task<User?> AuthenticateOAuthUserAsync(OAuthProviderType provider, string providerId, string providerEmail)
    public async Task<bool> UpdateLastLoginAsync(Guid userId)
    public async Task<bool> IsOAuthProviderLinkedAsync(OAuthProviderType provider, string providerId)
}
```
**Responsibilities**: OAuth user authentication, login tracking, provider validation

**UserRegistrationService:**
```csharp
public class UserRegistrationService : IUserRegistrationService
{
    // User registration for both regular and OAuth users
    public async Task<User> RegisterUserAsync(RegisterUserRequest request)
    public async Task<object> RegisterOAuthUserAsync(RegisterOAuthUserRequest request)
    public async Task<bool> CanRegisterUserAsync(string email)
    public async Task<bool> CanRegisterOAuthUserAsync(string email, OAuthProviderType provider, string providerId)
}
```
**Responsibilities**: User registration logic, duplicate checking, OAuth user creation

**LoginService:**
```csharp
public class LoginService : ILoginService
{
    // OAuth login and token management
    public async Task<LoginResponseDto> LoginWithOAuthAsync(OAuthLoginRequestDto request)
    public async Task<bool> ValidateOAuthCredentialsAsync(OAuthProviderType provider, string providerId, string providerEmail)
    public async Task<bool> UpdateOAuthTokenAsync(OAuthProviderType provider, string providerId, string accessToken, string? refreshToken, DateTime? tokenExpiresAt)
}
```
**Responsibilities**: OAuth login flow, credential validation, token updates

**BookmarkService:**
```csharp
public class BookmarkService : IBookmarkService
{
    // Portfolio bookmark management
    public async Task<BookmarkResponse> AddBookmarkAsync(Guid userId, AddBookmarkRequest request)
    public async Task<bool> RemoveBookmarkAsync(Guid userId, string portfolioId)
    public async Task<IEnumerable<BookmarkResponse>> GetUserBookmarksAsync(Guid userId)
    public async Task<IsBookmarkedResponse> GetBookmarkStatusAsync(Guid userId, string portfolioId)
}
```
**Responsibilities**: Bookmark CRUD operations, duplicate prevention, user validation

**UserReportService:**
```csharp
public class UserReportService : IUserReportService
{
    // User reporting and moderation
    public async Task<UserReportResponseDto> CreateUserReportAsync(Guid userId, UserReportCreateRequestDto request)
    public async Task<List<UserReportResponseDto>> GetUserReportsAsync(Guid userId)
    public async Task<List<UserReportResponseDto>> GetReportsByReporterAsync(Guid reporterId)
}
```
**Responsibilities**: User reporting, duplicate report prevention, moderation data access

**UserAnalyticsService:**
```csharp
public class UserAnalyticsService : IUserAnalyticsService
{
    // User behavior analytics
    public async Task<UserAnalyticsResponseDto> CreateUserAnalyticsAsync(UserAnalyticsCreateRequestDto request)
    public async Task<List<UserAnalyticsResponseDto>> GetUserAnalyticsAsync(Guid userId)
    public async Task<List<UserAnalyticsResponseDto>> GetUserAnalyticsBySessionAsync(string sessionId)
    public async Task<List<UserAnalyticsSummaryDto>> GetUserAnalyticsSummaryAsync(Guid userId)
}
```
**Responsibilities**: Analytics data management, session tracking, user behavior analysis

**OAuth2Service:**
```csharp
public class OAuth2Service : IOAuth2Service
{
    // OAuth 2.0 token management and refresh
    public async Task<OAuthProvider?> ValidateAccessTokenAsync(string token)
    public async Task<User?> GetUserByAccessTokenAsync(string token)
    public async Task<OAuthProvider?> RefreshAccessTokenAsync(string refreshToken)
}
```
**Responsibilities**: Token validation, automatic refresh, provider-specific OAuth logic

#### 3.3.2 Service Abstractions
All services implement interfaces for dependency injection and testability:

**Interface Design Principles:**
- **Single Responsibility**: Each interface focuses on one domain area
- **Interface Segregation**: Clients only depend on methods they use
- **Dependency Inversion**: High-level modules depend on abstractions
- **Testability**: Easy to mock for unit testing

**Complete Interface List:**
```csharp
// Core user management
public interface IUserService
public interface IAuthenticationService
public interface IUserRegistrationService
public interface ILoginService

// Feature-specific services
public interface IBookmarkService
public interface IUserReportService
public interface IUserAnalyticsService
public interface IOAuth2Service
public interface IOAuthProviderService

// Repository interfaces
public interface IUserRepository
public interface IOAuthProviderRepository
public interface IBookmarkRepository
public interface IUserReportRepository
public interface IUserAnalyticsRepository

// Utility interfaces
public interface IUserAnalyticsMapper
public interface IUserReportMapper
```

**Dependency Injection Benefits:**
- **Loose Coupling**: Services depend on interfaces, not concrete implementations
- **Testability**: Easy to substitute real services with mocks in tests
- **Flexibility**: Can swap implementations without changing consuming code
- **Lifecycle Management**: Proper scoping (Scoped, Singleton) for different service types

### 3.4 API Controllers

#### 3.4.1 UsersController
Comprehensive user management endpoints with detailed implementation:

**User CRUD Operations:**
```csharp
[HttpGet]                           // GET /api/users
public async Task<IActionResult> GetAllUsers()

[HttpGet("{id}")]                   // GET /api/users/{id}
public async Task<IActionResult> GetUserById(Guid id)

[HttpPost]                          // POST /api/users
public async Task<IActionResult> CreateUser(User user)

[HttpPut("{id}")]                   // PUT /api/users/{id}
public async Task<IActionResult> UpdateUser(Guid id, UpdateUserRequest request)
```

**Search & Discovery Endpoints:**
```csharp
[HttpGet("search")]                 // GET /api/users/search?q={term}
public async Task<IActionResult> SearchUsers([FromQuery] string q)

[HttpGet("email/{email}")]          // GET /api/users/email/{email}
public async Task<IActionResult> GetUserByEmail(string email)

[HttpGet("check-email/{email}")]    // GET /api/users/check-email/{email}
public async Task<IActionResult> CheckUserExistsByEmail(string email)

[HttpGet("{id}/portfolio-info")]    // GET /api/users/{id}/portfolio-info
public async Task<IActionResult> GetUserPortfolioInfo(Guid id)
```

**OAuth Management Endpoints:**
```csharp
[HttpGet("{userId}/oauth-providers")]           // GET /api/users/{userId}/oauth-providers
public async Task<IActionResult> GetUserOAuthProviders(Guid userId)

[HttpPost("oauth-providers")]                   // POST /api/users/oauth-providers
public async Task<IActionResult> CreateOAuthProvider(OAuthProviderCreateRequestDto request)

[HttpPut("oauth-providers/{id}")]               // PUT /api/users/oauth-providers/{id}
public async Task<IActionResult> UpdateOAuthProvider(Guid id, OAuthProviderUpdateRequestDto request)

[HttpDelete("oauth-providers/{id}")]            // DELETE /api/users/oauth-providers/{id}
public async Task<IActionResult> DeleteOAuthProvider(Guid id)

[HttpGet("oauth-providers/check")]              // GET /api/users/oauth-providers/check?provider={type}&providerId={id}
public async Task<IActionResult> CheckOAuthProvider(OAuthProviderType provider, string providerId)

[HttpPost("register-oauth")]                   // POST /api/users/register-oauth
public async Task<IActionResult> RegisterOAuthUser(RegisterOAuthUserRequest request)
```

**Bookmark Operations:**
```csharp
[HttpPost("{userId}/bookmarks")]               // POST /api/users/{userId}/bookmarks
public async Task<IActionResult> AddBookmark(Guid userId, AddBookmarkRequest request)

[HttpGet("{userId}/bookmarks")]                // GET /api/users/{userId}/bookmarks
public async Task<IActionResult> GetUserBookmarks(Guid userId)

[HttpDelete("{userId}/bookmarks/{portfolioId}")] // DELETE /api/users/{userId}/bookmarks/{portfolioId}
public async Task<IActionResult> RemoveBookmark(Guid userId, string portfolioId)

[HttpGet("{userId}/bookmarks/{portfolioId}/status")] // GET /api/users/{userId}/bookmarks/{portfolioId}/status
public async Task<IActionResult> GetBookmarkStatus(Guid userId, string portfolioId)
```

**User Reporting Endpoints:**
```csharp
[HttpPost("{userId}/report")]                  // POST /api/users/{userId}/report
public async Task<IActionResult> ReportUser(Guid userId, UserReportCreateRequestDto request)

[HttpGet("{userId}/reports")]                   // GET /api/users/{userId}/reports
public async Task<IActionResult> GetUserReports(Guid userId)

[HttpGet("reports/by-reporter/{reporterId}")]   // GET /api/users/reports/by-reporter/{reporterId}
public async Task<IActionResult> GetReportsByReporter(Guid reporterId)
```

**Controller Implementation Details:**
- **Dependency Injection**: All services injected via constructor
- **Error Handling**: Comprehensive try-catch blocks with proper HTTP status codes
- **Validation**: Input validation before service calls
- **Response Mapping**: Consistent response format across all endpoints
- **Async Operations**: All operations are async for better performance

#### 3.4.2 OAuth2Controller
OAuth 2.0 specific endpoints for token management and user authentication:

**Token Validation Endpoint:**
```csharp
[HttpPost("validate")]              // POST /api/oauth/validate
public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequest request)
```
**Purpose**: Validates access tokens and returns user information
**Request Body**: `{ "accessToken": "string" }`
**Response**: User profile data or 401 Unauthorized for invalid tokens

**Token Refresh Endpoint:**
```csharp
[HttpPost("refresh")]               // POST /api/oauth/refresh
public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
```
**Purpose**: Refreshes expired access tokens using refresh tokens
**Request Body**: `{ "refreshToken": "string" }`
**Response**: New access token information or error details
**Supported Providers**: Google (full support), LinkedIn (full support), GitHub (limited), Facebook (no refresh)

**User Profile Endpoint:**
```csharp
[HttpGet("me")]                     // GET /api/oauth/me
public async Task<IActionResult> GetCurrentUser()
```
**Purpose**: Retrieves current user profile using Authorization header
**Authentication**: Bearer token in Authorization header
**Response**: Complete user profile information
**Use Case**: Frontend user profile display and authentication verification

**Token Status Endpoint:**
```csharp
[HttpGet("token-status")]           // GET /api/oauth/token-status
public async Task<IActionResult> GetTokenStatus()
```
**Purpose**: Provides debugging information about user's OAuth tokens
**Authentication**: Bearer token in Authorization header
**Response**: Token status for all linked OAuth providers
**Debugging Info**: Expiration status, refresh token availability, provider support

**Controller Implementation Details:**
- **Dual Routing**: Supports both `/api/oauth` and `/api/oauth2` routes
- **Token Extraction**: Automatically extracts tokens from Authorization headers
- **Provider-Specific Logic**: Handles different OAuth provider capabilities
- **Comprehensive Logging**: Detailed logging for debugging OAuth issues
- **Error Handling**: Provider-specific error messages and status codes

## 4. Data Flow Architecture

### 4.1 User Registration Flow

```
Client Request → UsersController → UserRegistrationService → UserValidator → UserMapper → UserRepository → Database
```

**Detailed Flow:**
1. **Request Validation**: `UserValidator.ValidateRegisterRequest()` checks email format, required fields
2. **Duplicate Check**: Repository checks for existing users by email
3. **Entity Creation**: `UserMapper.ToEntity()` converts DTO to User entity
4. **Database Persistence**: Repository creates user record
5. **Response Mapping**: Service returns mapped response DTO

### 4.2 OAuth Authentication Flow

```
OAuth Provider → OAuth2Controller → OAuth2Service → OAuthProviderRepository → UserRepository → Database
```

**Detailed Flow:**
1. **Token Validation**: `OAuth2Service.ValidateAccessTokenAsync()` checks token validity
2. **User Lookup**: Repository finds user by OAuth provider ID
3. **Token Refresh**: Automatic refresh if token expired (Google, LinkedIn)
4. **User Authentication**: Returns user profile if valid

### 4.3 User Search Flow

```
Search Request → UsersController → UserService → UserRepository → Database → Mapped Response
```

**Detailed Flow:**
1. **Query Processing**: Controller receives search term
2. **Business Logic**: Service validates and processes search
3. **Database Query**: Repository executes optimized search with indexes
4. **Result Mapping**: Service maps entities to response DTOs
5. **Response**: Returns filtered, paginated results

### 4.4 Bookmark Management Flow

```
Bookmark Request → UsersController → BookmarkService → BookmarkRepository → Database
```

**Detailed Flow:**
1. **User Validation**: Service checks if user exists
2. **Duplicate Check**: Repository verifies bookmark doesn't exist
3. **Entity Creation**: Creates bookmark with portfolio metadata
4. **Persistence**: Saves to database with proper relationships

## 5. Configuration

### 5.1 Database Configuration
The DatabaseConfiguration class implements comprehensive database setup that optimizes performance and maintains consistency. The AddDbContext method configures Entity Framework Core with the Npgsql provider for PostgreSQL, enabling the use of advanced PostgreSQL features. The UseSnakeCaseNamingConvention method ensures that database columns follow PostgreSQL naming conventions, while the custom data source provides enum type mapping support for OAuth provider types. The connection string is retrieved from configuration, supporting different environments and deployment scenarios.

**Features:**
The database configuration provides several key features that enhance the development experience and system performance. PostgreSQL with Npgsql provider offers robust database capabilities including advanced indexing, JSON support, and efficient query execution. Snake case naming convention ensures consistency between C# property names and database column names, following PostgreSQL best practices. Custom enum type mappings enable the use of PostgreSQL enum columns for OAuth provider types, providing type safety and performance benefits. Connection string configuration from external sources supports flexible deployment scenarios and environment-specific settings.

### 5.2 Service Registration
The ServiceRegistrationConfiguration class implements a clean and organized approach to dependency injection setup. The AddRepositoryServices method registers all data access layer components, while AddMapperServices registers entity-DTO transformation services. AddBusinessServices registers core business logic services, and AddAuthenticationServices registers OAuth and authentication-related services. This modular approach makes the service registration process readable and maintainable, ensuring that all application dependencies are properly configured.

**Dependency Injection:**
The system implements a comprehensive dependency injection strategy that optimizes resource usage and promotes testability. Scoped services including repositories, services, and mappers are created once per HTTP request, ensuring proper isolation between requests while maintaining performance through connection pooling. Singleton services like the database data source are created once and shared across all requests, optimizing resource usage. Interface-based registration ensures that all services depend on abstractions rather than concrete implementations, promoting loose coupling and enabling easy mocking for unit testing.

### 5.3 CORS Configuration
The CorsConfiguration class implements comprehensive Cross-Origin Resource Sharing setup that enables secure communication between the backend service and frontend applications. The AddCors method configures CORS policies with the "AllowFrontend" policy that specifies allowed origins, HTTP methods, headers, and credential support. The policy uses the frontendUrls configuration to dynamically determine which origins are allowed, supporting multiple frontend applications during development. The AllowCredentials option enables the transmission of authentication cookies and headers, which is essential for OAuth authentication flows.

**Frontend URLs**: The system supports multiple frontend applications running on different localhost ports (3000-3003) during development, allowing developers to work on different frontend services simultaneously. This configuration approach supports the microservices architecture where multiple frontend applications may need to communicate with the same backend service, while maintaining security by restricting access to only the specified origins.

## Implementation Patterns & Code Examples

### 6.1 Validation Framework

#### 6.1.1 UserValidator
Comprehensive validation for all user operations with detailed implementation:

**Email Validation:**
The system implements robust email validation using the .NET MailAddress class. This approach validates both the email format and ensures the email address is properly structured. The validation checks for null or empty strings first, then attempts to parse the email using the MailAddress constructor. If parsing fails, it catches the exception and returns false, ensuring only valid email formats are accepted.

**Field Length Validation:**
Professional title and location fields have specific length constraints to maintain database efficiency and user experience consistency. The professional title is limited to 200 characters to accommodate various job titles while preventing excessive storage usage. Location fields are capped at 100 characters, providing sufficient space for city names and general locations without overwhelming the database.

**Required Fields Validation:**
The system enforces mandatory fields for user registration, including email, first name, and last name. Email validation goes beyond simple null checking to ensure proper email format. First and last names are required to maintain user identity integrity and provide meaningful user profiles. The validation system collects all errors before returning them, allowing users to see all validation issues at once rather than one at a time.

**OAuth Validation:**
OAuth provider credentials undergo thorough validation to ensure data integrity and security. Provider ID validation prevents empty or null identifiers that could cause authentication failures. Provider email validation ensures the email associated with the OAuth account is properly formatted and valid. This validation is crucial for maintaining the connection between external OAuth accounts and internal user records.

#### 6.1.2 ValidationResult Pattern
The ValidationResult class implements a robust pattern for handling validation outcomes throughout the system. This class encapsulates both the validation status and any error messages that occur during the validation process. The class uses a private constructor pattern to ensure that ValidationResult instances can only be created through the static factory methods, maintaining data integrity and preventing invalid states.

The Success() method creates a ValidationResult instance indicating successful validation with an empty error list. The Failure() methods create instances with specific error messages, supporting both single and multiple error scenarios. This pattern allows the system to collect all validation errors before reporting them to the user, providing a better user experience by showing all issues at once rather than one at a time.

**Usage Pattern:**
The validation pattern follows a consistent approach where validation methods return a ValidationResult object. The calling code then checks the IsValid property to determine if validation passed. If validation fails, the Errors collection contains all the validation error messages, which are typically joined into a single string and thrown as an ArgumentException. This approach ensures that validation logic is centralized and reusable across different parts of the system.

### 6.2 Mapping System

#### 6.2.1 UserMapper
Centralized entity-DTO conversion with comprehensive mapping:

**Entity to Response DTO:**
The ToResponseDto method transforms User entities into UserResponseDto objects for API responses. This method includes comprehensive null checking to prevent runtime errors and ensure data integrity. The mapping covers all user properties including identity information, profile details, status flags, and timestamps. The method maintains consistency between the internal entity structure and the external API contract, ensuring that all necessary user information is available to clients while protecting sensitive internal data.

**Request DTO to Entity:**
The ToEntity method converts RegisterUserRequest DTOs into User entities for database persistence. This method implements intelligent username generation by extracting the local part of the email address before the @ symbol. The mapping handles all user profile fields including personal information, professional details, and optional profile elements. The method ensures that all required fields are properly mapped and that the resulting entity is ready for database storage.

**Portfolio Information Mapping:**
The ToPortfolioInfo method creates specialized portfolio information objects from User entities. This method handles null values gracefully by providing default values for missing information, ensuring that portfolio displays always have meaningful content. The mapping creates a clean, frontend-friendly structure that includes user identification, display names, professional information, and contact details. This approach separates portfolio-specific data presentation from general user information.

### 6.3 Error Handling Patterns

#### 6.3.1 Controller-Level Error Handling
The controller layer implements comprehensive error handling that provides meaningful HTTP responses while maintaining security. The try-catch blocks capture different types of exceptions and map them to appropriate HTTP status codes. ArgumentException errors are returned as 400 Bad Request responses, indicating client-side input issues. Unexpected exceptions are logged for debugging purposes but return generic 500 Internal Server Error responses to prevent information leakage. This approach ensures that users receive helpful error messages while maintaining system security.

#### 6.3.2 Service-Level Error Handling
Service layer error handling focuses on business logic validation and data integrity. The validation process occurs before any business operations, ensuring that invalid data is rejected early in the process. Business rule violations throw InvalidOperationException, which are then caught and handled appropriately by the calling controller. This separation of concerns allows the service layer to focus on business logic while the controller handles HTTP-specific error responses. The approach ensures consistent error handling across all service operations.

#### 6.3.3 Repository-Level Error Handling
Repository layer error handling emphasizes graceful degradation and data consistency. When entities are not found, the repository returns null rather than throwing exceptions, allowing the service layer to decide how to handle missing data. The partial update approach only modifies fields that are provided in the request, maintaining existing data for unchanged fields. This pattern supports flexible update operations while ensuring data integrity. The repository layer focuses on data access concerns, leaving business logic decisions to the service layer.

### 6.4 Async/Await Patterns

#### 6.4.1 Proper Async Implementation
The system implements proper async/await patterns throughout all layers to ensure optimal performance and scalability. The GetUserByIdAsync method demonstrates the correct pattern where validation occurs synchronously before the async database operation. This approach prevents unnecessary async overhead for operations that don't require it while maintaining the performance benefits for I/O-bound operations like database queries.

The SearchUsers method showcases advanced async patterns with Entity Framework Core. The method uses async database operations with complex LINQ queries that include filtering, ordering, and result limiting. The async approach ensures that database connections are not blocked during query execution, allowing the system to handle multiple concurrent requests efficiently. The method also demonstrates proper result set limiting to prevent memory issues with large datasets.

### 6.5 Dependency Injection Patterns

#### 6.5.1 Constructor Injection
The UsersController demonstrates comprehensive constructor injection that follows dependency inversion principles. The controller accepts all required services through its constructor, ensuring that dependencies are explicitly declared and easily testable. Each service parameter is validated with null checks using the null-coalescing operator, which throws ArgumentNullException if any required service is not provided. This approach prevents runtime errors and makes debugging easier by failing fast during object construction.

The readonly fields ensure that service references cannot be modified after construction, maintaining the immutability principle and preventing accidental service replacement during the controller's lifetime. This pattern supports unit testing by allowing test code to inject mock services, and it makes the controller's dependencies explicit and manageable.

#### 6.5.2 Service Registration
The AddBusinessServices method implements a clean service registration pattern that centralizes all business service dependencies. Each service is registered with the Scoped lifetime, meaning a new instance is created for each HTTP request. This approach ensures proper isolation between requests while maintaining performance through connection pooling and resource sharing within the same request context.

The method returns the service collection to support method chaining, allowing for fluent configuration in the startup code. This pattern makes the service registration process readable and maintainable, and it ensures that all business services are properly configured before the application starts. The scoped lifetime is appropriate for most business services as it provides the right balance between performance and request isolation.

## 6.6 Validation Framework

### 6.6.1 UserValidator
Comprehensive validation for all user operations:
- **Email Validation**: Format and uniqueness checks
- **Field Length**: Professional title, location constraints
- **Required Fields**: First name, last name validation
- **OAuth Validation**: Provider-specific credential checks

### 6.6.2 ValidationResult
The ValidationResult class implements a robust pattern for handling validation outcomes throughout the system. This class encapsulates both the validation status and any error messages that occur during the validation process. The class uses private setters to ensure that ValidationResult instances can only be created through the static factory methods, maintaining data integrity and preventing invalid states. The IsValid property provides a simple boolean check for validation success, while the Errors collection contains detailed error messages for failed validations.

**Usage Pattern:**
The validation pattern follows a consistent approach where validation methods return a ValidationResult object. The calling code then checks the IsValid property to determine if validation passed. If validation fails, the Errors collection contains all the validation error messages, which are typically joined into a single string and thrown as an ArgumentException. This approach ensures that validation logic is centralized and reusable across different parts of the system, while providing comprehensive error information to clients.

## Mapping System

### UserMapper
Centralized entity-DTO conversion:
- **ToResponseDto**: User entity to response DTO
- **ToEntity**: Request DTO to User entity
- **ToSummaryDto**: User entity to summary DTO
- **ToPortfolioInfo**: User entity to portfolio information

**Benefits:**
The mapping system provides several key benefits that improve code quality and maintainability. Consistent data transformation ensures that all entity-to-DTO conversions follow the same patterns, reducing bugs and improving reliability. Null safety with argument validation prevents runtime errors by checking input parameters before processing, while centralized mapping logic makes it easy to update data transformation rules in one location. Easy maintenance and updates are facilitated by the centralized approach, allowing developers to modify mapping logic without searching through multiple files.

## 7. OAuth 2.0 Implementation & Security

### 7.1 Supported Providers
1. **Google**: Full refresh token support with automatic renewal
2. **GitHub**: OAuth app flow (limited refresh token support)
3. **LinkedIn**: Refresh token support with automatic renewal
4. **Facebook**: Long-lived access token approach (no refresh)

### 7.2 OAuth Flow Implementation

#### Token Validation Process
The GetUserByAccessTokenAsync method implements a two-step validation process that ensures both token validity and user existence. First, it validates the access token by checking if it exists in the database and whether it has expired. The method uses the TokenExpiresAt field to determine if the token is still valid, comparing it against the current UTC time. If the token is valid, the method then retrieves the associated user from the repository using the UserId stored in the OAuth provider record.

The ValidateAccessTokenAsync method performs the core token validation logic. It searches the database for an OAuth provider record that matches the provided access token. The method includes comprehensive expiration checking that handles nullable expiration timestamps gracefully. This approach ensures that tokens without expiration dates are treated as valid, while expired tokens are automatically rejected. The validation process maintains security by preventing the use of stale or invalid tokens.

#### Automatic Token Refresh
The RefreshAccessTokenAsync method implements a comprehensive token refresh process that handles multiple failure scenarios gracefully. The method begins by locating the OAuth provider record using the refresh token, logging warnings if the token is not found or if the provider has an empty refresh token. This early validation prevents unnecessary API calls to external OAuth providers and provides clear debugging information.

The refresh process uses a try-catch block to handle external API failures gracefully. The method calls the appropriate OAuth provider's refresh endpoint through the RefreshTokenWithProvider method, which handles provider-specific refresh logic. Upon successful refresh, the method updates the local OAuth provider record with the new access token, expiration time, and potentially a new refresh token. The updated record is then persisted to the database, ensuring that subsequent requests use the fresh token.

The method includes comprehensive logging at various stages, making it easier to diagnose issues in production environments. Error handling ensures that refresh failures don't crash the application, and the method returns null to indicate that the refresh operation failed, allowing calling code to handle the failure appropriately.

#### Provider-Specific Refresh Logic
The RefreshTokenWithProvider method implements a strategy pattern that routes refresh requests to the appropriate provider-specific implementation. The method creates a new HttpClient instance for each refresh operation, ensuring proper resource management and preventing connection pooling issues. The switch statement handles the different OAuth provider types, calling the appropriate refresh method for each supported provider.

The method includes special handling for GitHub OAuth apps, which don't support refresh tokens in the traditional OAuth 2.0 flow. For GitHub, the method returns null immediately, indicating that refresh is not supported. This approach allows the system to gracefully handle providers with different capabilities while maintaining a consistent interface for the calling code.

The strategy pattern makes it easy to add support for new OAuth providers by simply adding new cases to the switch statement and implementing the corresponding refresh method. This design ensures that the refresh logic remains maintainable and extensible as the system grows to support additional authentication providers.

#### Google Token Refresh Implementation
The RefreshGoogleToken method implements the complete Google OAuth 2.0 token refresh flow using the official Google token endpoint. The method retrieves Google OAuth credentials from environment variables, ensuring that sensitive client information is not hardcoded in the source code. This approach follows security best practices and allows for different credentials in different environments.

The method constructs a form-encoded request body containing the client ID, client secret, grant type, and refresh token. The grant type is set to "refresh_token" to indicate that this is a token refresh operation rather than an initial authorization. The method uses the non-null assertion operator for the refresh token since the calling code has already validated its existence.

The HTTP request is sent to Google's token endpoint, and the response is thoroughly validated. The method checks the HTTP status code and logs detailed error information if the request fails. Upon successful response, the method deserializes the JSON response into a GoogleTokenResponse object and validates that the access token is present.

The method calculates the new expiration time by adding the expires_in value (in seconds) to the current UTC time, with a fallback to 3600 seconds (1 hour) if the expiration is not provided. This ensures that the token expiration is properly tracked for future validation operations.

### Security Considerations

#### Token Storage Security
- **Access Tokens**: Stored in database with proper encryption
- **Refresh Tokens**: Securely stored for automatic renewal
- **Token Expiration**: Automatic cleanup of expired tokens
- **Provider Isolation**: Separate credentials per OAuth provider

#### Environment Variable Management
The system uses environment variables to store sensitive OAuth credentials, following security best practices for configuration management. The AUTH_GOOGLE_ID and AUTH_GOOGLE_SECRET variables store Google OAuth application credentials, while AUTH_LINKEDIN_ID and AUTH_LINKEDIN_SECRET store LinkedIn OAuth credentials. This approach ensures that sensitive information is not hardcoded in the source code and can be easily changed between different environments (development, staging, production).

The environment variable approach supports different deployment scenarios, including containerized environments where credentials can be injected at runtime. This method also facilitates credential rotation and management without requiring code changes or redeployment. The system validates that these credentials are present during startup, logging errors if they are missing to prevent runtime failures.

#### Token Validation Security
- **Token Verification**: Validate tokens against stored provider records
- **Expiration Checking**: Automatic expiration validation
- **Provider Verification**: Ensure token belongs to correct provider
- **User Association**: Verify token-user relationship

#### OAuth Provider Security
- **Client ID/Secret**: Secure storage in environment variables
- **HTTPS Only**: All OAuth endpoints use HTTPS
- **Token Encryption**: Sensitive data encrypted in database
- **Access Control**: User-specific token isolation

### Token Management
The OAuth2Service implements comprehensive token management that handles the complete lifecycle of OAuth access tokens. The RefreshAccessTokenAsync method orchestrates the token refresh process by first locating the OAuth provider record using the refresh token, then calling the appropriate provider-specific refresh implementation. This centralized approach ensures consistent token handling across all supported OAuth providers.

**Features:**
- **Automatic token refresh**: The system automatically detects expired tokens and refreshes them using stored refresh tokens, ensuring continuous user authentication without manual intervention.
- **Provider-specific refresh logic**: Each OAuth provider has customized refresh implementation that handles their specific API requirements and response formats.
- **Environment-based credentials**: OAuth client credentials are stored in environment variables, supporting different deployment environments and secure credential management.
- **Comprehensive logging**: All token operations are logged with appropriate detail levels, facilitating debugging and monitoring of OAuth authentication flows.

## Security Features

### Input Validation
- **DTO Validation**: Data annotation attributes
- **Business Validation**: Custom validation rules
- **SQL Injection Prevention**: Parameterized queries via EF Core

### OAuth Security
- **Token Validation**: Access token verification
- **Refresh Token Security**: Secure token refresh flow
- **Provider Isolation**: Separate credentials per provider

### Data Protection
- **Sensitive Data**: OAuth tokens stored securely
- **User Privacy**: Personal information protection
- **Access Control**: User-specific data isolation

## 8. Performance Optimizations & Database Design

### 8.1 Database Indexes & Query Optimization

#### Strategic Index Design
The database employs a comprehensive indexing strategy that optimizes performance for the most common query patterns. User entity indexes include unique constraints on email and username fields, ensuring data integrity while providing fast lookups for authentication and user identification. The composite search index on first name, last name, and username with an active user filter significantly improves user discovery performance by allowing the database to quickly locate users based on partial name matches.

OAuth provider indexes are designed to support fast token validation and user lookup operations. The composite unique index on provider and provider ID ensures that each OAuth account can only be linked to one user, while separate indexes on user ID, access token, and refresh token optimize the various lookup patterns used by the authentication system.

Analytics indexes support efficient data retrieval for user behavior analysis and reporting. The user ID index enables fast retrieval of all analytics data for a specific user, while the session ID index supports session-based analysis. Time-based indexes on created_at fields facilitate efficient date range queries and time-series analysis.

Reporting and moderation indexes ensure fast access to user report data, supporting both user-specific report retrieval and time-based reporting analysis. Bookmark indexes optimize portfolio bookmark operations, while newsletter subscription indexes support efficient subscription management and filtering.

#### Query Optimization Strategies

**Efficient User Search Implementation:**
The SearchUsers method implements a sophisticated search algorithm that balances performance with user experience. The method begins with early validation to return empty results for null or empty search terms, preventing unnecessary database queries. The search logic uses case-insensitive matching by converting both the search term and database values to lowercase, ensuring consistent results regardless of input case.

The search criteria cover multiple user fields including username, first name, last name, and full name combinations, providing comprehensive search coverage. The ordering logic prioritizes exact matches by checking if usernames start with the search term, then falls back to alphabetical ordering for remaining results. The Take(20) method limits results to prevent performance issues with large datasets while maintaining responsive user experience.

**OAuth Provider Lookup Optimization:**
The OAuth provider lookup methods leverage the strategic index design to achieve optimal performance. GetByProviderAndProviderIdAsync uses the composite index on provider and provider ID, enabling the database to quickly locate specific OAuth accounts. GetByAccessTokenAsync utilizes the dedicated access token index for fast token validation during authentication requests.

These methods use FirstOrDefaultAsync to retrieve single records efficiently, avoiding the overhead of retrieving multiple records when only one is needed. The index-based approach ensures that lookup operations remain fast even as the number of OAuth providers grows, maintaining consistent authentication performance.

**Analytics Data Retrieval Optimization:**
The GetUserAnalyticsByDateRangeAsync method demonstrates efficient time-based data retrieval using composite indexes. The method leverages the index on user ID and created date to quickly filter analytics data for specific users within date ranges. The OrderByDescending on created date ensures that the most recent analytics data appears first, which is typically the most relevant for user behavior analysis.

The method uses precise date range filtering to minimize the data set before applying ordering, optimizing both memory usage and query performance. This approach supports efficient analytics reporting and user behavior tracking without impacting overall system performance.

### Entity Framework Core Optimization

#### DbContext Configuration
The OnModelCreating method implements comprehensive Entity Framework Core configuration that optimizes both performance and data integrity. The User entity configuration establishes unique indexes on email and username fields, ensuring that no duplicate accounts can be created while providing fast lookup performance for authentication operations. String length constraints are explicitly defined to prevent excessive storage usage and maintain consistent data quality across all user records.

The OAuthProvider entity configuration implements proper relationship mapping with cascade delete behavior, ensuring that when a user is removed, all associated OAuth provider records are automatically cleaned up. This prevents orphaned OAuth records and maintains database consistency. The composite unique index on provider and provider ID ensures that each OAuth account can only be linked to one user, while separate indexes on user ID, access token, and refresh token optimize the various lookup patterns used by the authentication system.

The configuration approach uses fluent API methods that provide compile-time safety and clear, readable configuration code. This method of configuration is preferred over data annotations as it centralizes all database schema configuration in one location, making it easier to maintain and modify as the system evolves.

#### Query Performance Best Practices

**Avoiding N+1 Query Problems:**
The GetUserWithOAuthProvidersAsync method demonstrates the proper use of Entity Framework Core's Include method to prevent N+1 query problems. By including related entities (OAuth providers and bookmarks) in a single query, the method retrieves all necessary data in one database round trip rather than making separate queries for each related entity. This approach significantly improves performance by reducing the number of database connections and queries executed.

The GetUserSummariesAsync method showcases the use of projection to optimize read-only operations. Instead of retrieving complete User entities and then mapping them to DTOs, the method uses the Select method to project directly to UserSummaryDto objects. This approach reduces memory usage and network transfer time by only retrieving the fields that are actually needed for the summary display.

**Batch Operations for Performance:**
The UpdateLastLoginAsync method demonstrates the use of raw SQL for simple, single-field updates that don't require Entity Framework's change tracking capabilities. Using ExecuteSqlRawAsync is more efficient than loading the entity, modifying it, and saving changes when only a few fields need to be updated. This approach reduces memory usage and eliminates the overhead of change tracking for simple operations.

The method uses parameterized queries to prevent SQL injection attacks while maintaining the performance benefits of raw SQL. The return value indicates whether the update was successful, allowing calling code to handle cases where the user might not exist or the update fails for other reasons.

### Connection Pooling & Resource Management

#### Database Connection Optimization
The AddDatabaseServices method implements comprehensive database configuration that optimizes connection management and performance. The method retrieves the database connection string from configuration and validates its presence, throwing a descriptive exception if the connection string is missing. This approach ensures that configuration errors are caught early during application startup rather than at runtime.

The method creates a custom data source with enum mapping support, enabling the use of PostgreSQL enum types for OAuth provider types. The Npgsql configuration includes retry logic that automatically attempts to reconnect up to three times with exponential backoff delays, improving application resilience during temporary database connectivity issues.

The command timeout is set to 30 seconds, providing a reasonable balance between allowing complex queries to complete and preventing indefinite blocking. The maximum batch size of 100 optimizes bulk operations while preventing memory issues with extremely large datasets. The snake case naming convention ensures consistency between C# property names and database column names, following PostgreSQL best practices.

#### Memory Management
The UserRepository class implements the IDisposable pattern to ensure proper resource cleanup and prevent memory leaks. The repository maintains a reference to the UserDbContext and implements the standard dispose pattern with a protected virtual Dispose method that can be overridden by derived classes if needed.

The dispose pattern includes a boolean flag to prevent multiple disposal attempts, ensuring that resources are only cleaned up once. The protected Dispose method checks if disposal has already occurred before proceeding, preventing exceptions from multiple disposal calls. The method also checks if the context is null before attempting disposal, providing additional safety.

The public Dispose method calls the protected Dispose method and then suppresses finalization by calling GC.SuppressFinalize. This optimization prevents the garbage collector from calling the finalizer on an object that has already been properly disposed, improving performance. The pattern ensures that database connections and other resources are properly released when the repository is no longer needed.

### Query Optimization
- **Eager Loading**: Navigation properties loaded efficiently
- **Pagination**: Search results limited to 20 users
- **Indexed Searches**: Optimized user search queries

## Error Handling

### Exception Strategy
The system implements a comprehensive exception handling strategy that provides meaningful error responses while maintaining security and debugging capabilities. The try-catch blocks capture different types of exceptions and map them to appropriate HTTP status codes, ensuring that clients receive helpful error messages. The NotFound response is returned when requested resources don't exist, while BadRequest responses are returned for other types of exceptions. This approach ensures consistent error handling across all API endpoints while preventing information leakage.

**Error Types:**
The system categorizes errors into distinct types to provide appropriate responses and facilitate debugging. ArgumentException represents invalid input parameters that need to be corrected by the client. InvalidOperationException indicates business rule violations where the requested operation cannot be performed in the current system state. NotFoundException represents missing resources that the client is trying to access. ConflictException indicates duplicate resource conflicts where the client is attempting to create resources that already exist.

## 9. Testing Strategy & Development Best Practices

### 9.1 Comprehensive Testing Approach

#### Test Structure & Organization
The test project follows a comprehensive structure that mirrors the main application architecture, ensuring thorough coverage of all components. The Controllers directory contains tests for API endpoints, validating HTTP responses, status codes, and request handling. Services tests focus on business logic validation, ensuring that business rules are properly enforced and edge cases are handled correctly.

Repository tests validate data access patterns and database operations, including both unit tests with mocked contexts and integration tests with in-memory databases. Model tests ensure that entity validation, relationships, and constraints work as expected. Middleware tests validate custom authentication and authorization logic, while configuration tests ensure that application settings are properly loaded and validated.

The Helpers directory contains test utilities, data factories, and common test setup code that promotes test code reuse and consistency. TestConfiguration.cs centralizes test environment setup, including in-memory database configuration and mock service registration. This structure ensures that all aspects of the application are thoroughly tested and that tests are maintainable and easy to understand.

#### Unit Testing Strategy

**Service Layer Testing:**
The UserServiceTests class demonstrates comprehensive unit testing of the service layer using the Arrange-Act-Assert pattern. The test class uses MSTest attributes for test organization and includes a TestInitialize method that sets up fresh mocks and service instances for each test, ensuring test isolation and preventing state leakage between tests.

The GetUserByIdAsync_ValidId_ReturnsUser test validates the happy path scenario where a valid user ID returns the expected user. The test arranges the mock repository to return a specific user, executes the service method, and then asserts that the returned user matches the expected values. This approach ensures that the service correctly retrieves and returns user data from the repository.

The GetUserByIdAsync_InvalidId_ThrowsArgumentException test validates error handling by testing the scenario where an invalid user ID is provided. The test uses Assert.ThrowsExceptionAsync to verify that the service throws the correct exception type when invalid input is received. This ensures that input validation is working correctly and that the service fails gracefully with appropriate error messages.

**Controller Testing:**
The UsersControllerTests class demonstrates comprehensive testing of the API controller layer using MSTest and Moq. The test class includes a TestInitialize method that creates fresh mock instances for each test, ensuring test isolation and preventing state leakage between test methods. The controller is instantiated with all required dependencies, using Mock.Of for services that aren't the primary focus of the test.

The GetUserById_ValidId_ReturnsOkResult test validates the successful user retrieval scenario. The test arranges the mock user service to return a specific user, executes the controller action, and then asserts that the result is an OkObjectResult with the expected user data. This approach ensures that the controller correctly handles successful user lookups and returns appropriate HTTP responses.

The GetUserById_UserNotFound_ReturnsNotFound test validates the error handling scenario where a user is not found. The test arranges the mock service to return null, executes the controller action, and then asserts that the result is a NotFoundObjectResult. This ensures that the controller properly handles missing users and returns the correct HTTP status code for not found scenarios.

#### Integration Testing

**Repository Testing with In-Memory Database:**
The UserRepositoryIntegrationTests class demonstrates integration testing of the repository layer using Entity Framework Core's in-memory database provider. The test class uses a TestInitialize method that creates a fresh in-memory database for each test, ensuring complete test isolation and preventing data contamination between tests. Each test gets a unique database name using Guid.NewGuid(), ensuring that no test can interfere with another.

The SearchUsers_ValidTerm_ReturnsMatchingUsers test validates the repository's search functionality with real database operations. The test seeds the in-memory database with test data, executes the search method, and then asserts that the results contain the expected users. The assertion logic validates that all returned users match the search criteria across multiple fields (first name, last name, and username), ensuring comprehensive search coverage.

The SeedTestData method demonstrates proper test data setup by creating realistic user records with appropriate email addresses, usernames, and names. This approach ensures that the tests validate real-world scenarios rather than artificial edge cases. The method uses AddRange and SaveChanges to efficiently populate the test database, demonstrating proper Entity Framework Core usage patterns.

#### Test Configuration & Setup

**TestConfiguration.cs:**
The TestConfiguration class provides centralized test service configuration that promotes consistency and reusability across all test classes. The AddTestServices extension method configures the dependency injection container with test-specific services, including an in-memory database with a unique name for each test run. This approach ensures that tests have access to real repository implementations while using a lightweight, fast database provider.

The CreateTestServiceProvider method demonstrates how to create a complete service provider for integration tests that require the full dependency injection container. This method builds a service collection, adds test services, and returns a configured service provider that can be used to resolve dependencies in integration test scenarios. This approach supports testing complex service interactions and dependency chains.

**Test Data Factories:**
The TestDataFactory class implements the factory pattern for creating test data, ensuring consistency and reducing duplication across test methods. The CreateTestUser method generates realistic user entities with proper GUIDs, timestamps, and default values. The method uses the email parameter to generate a username by extracting the local part, demonstrating the same logic used in the actual application.

The CreateTestOAuthProvider method creates test OAuth provider records with realistic data including proper relationships to users, provider types, and token information. The method uses default parameters to simplify test setup while allowing customization when needed. Both factory methods use DateTime.UtcNow for timestamps, ensuring that test data reflects real-world usage patterns and maintains temporal consistency.

### Development Best Practices

#### Code Organization & Standards

**Naming Conventions:**
The project follows consistent naming conventions that promote code readability and maintainability. Interfaces use the "I" prefix to clearly identify them as contracts, while concrete implementations use descriptive suffixes like "Service" and "Repository" to indicate their purpose and role in the system. DTOs use clear suffixes like "Request" and "Response" to distinguish between input and output data structures, making API contracts self-documenting.

**File Organization:**
The project structure follows a logical organization that separates concerns and promotes maintainability. The Services directory contains all business logic components organized by functionality, with Abstractions holding interface definitions, Mappers handling data transformation, and Validators containing input validation logic. The DTOs directory organizes data transfer objects by domain area, with separate Request and Response subdirectories for each domain, and a Common directory for shared DTOs used across multiple domains.

#### Error Handling Best Practices

**Consistent Exception Types:**
The system uses a consistent set of exception types to represent different categories of errors, making error handling predictable and maintainable. ArgumentException is used for input validation errors, indicating that client-provided data is invalid or malformed. InvalidOperationException represents business rule violations, such as attempting to create duplicate resources or perform operations that aren't allowed in the current state. KeyNotFoundException indicates that requested resources don't exist, while other InvalidOperationException instances handle duplicate resource conflicts.

**HTTP Status Code Mapping:**
The controller layer implements consistent HTTP status code mapping that provides meaningful responses to clients while maintaining security. 400 Bad Request responses are returned for ArgumentException errors, indicating client-side input issues that need to be corrected. 404 Not Found responses are returned for KeyNotFoundException errors, indicating that requested resources don't exist. 409 Conflict responses are returned for business rule violations, indicating that the requested operation conflicts with current system state. 500 Internal Server Error responses are returned for unexpected exceptions, with generic messages to prevent information leakage while logging detailed error information for debugging.

#### Performance Best Practices

**Async/Await Usage:**
The system implements proper async/await patterns throughout all layers to ensure optimal performance and scalability. The GetUserAsync method demonstrates the correct pattern where the async operation is properly awaited and the result is returned directly. The GetUsersAsync method showcases parallel execution using Task.WhenAll, which allows multiple database queries to execute concurrently rather than sequentially, significantly improving performance for bulk operations. The GetUserAsync method with cancellation token support demonstrates proper resource management by allowing long-running operations to be cancelled when needed.

**Memory Management:**
The system implements several memory management strategies to prevent memory leaks and optimize resource usage. Using statements ensure that disposable resources like HttpClient instances are properly cleaned up, preventing connection pool exhaustion. Result set limiting prevents memory issues with large datasets by restricting the number of records returned, while projection techniques reduce memory usage by only retrieving the fields that are actually needed for the operation. These approaches ensure that the application remains responsive and stable even under high load conditions.

#### Security Best Practices

**Input Validation:**
The system implements comprehensive input validation at service boundaries to ensure data integrity and security. The CreateUserAsync method demonstrates the validation-first approach where all input is validated before any business logic is executed. The method uses the centralized UserValidator to check input validity, throwing descriptive exceptions if validation fails. Input sanitization is performed after validation, including trimming whitespace and normalizing email addresses to lowercase, ensuring consistent data storage and preventing common input issues.

**SQL Injection Prevention:**
The system implements multiple layers of SQL injection prevention to ensure database security. Entity Framework Core automatically parameterizes all LINQ queries, preventing SQL injection attacks through user input. When raw SQL is necessary for performance reasons, the system uses parameterized queries with ExecuteSqlRawAsync, ensuring that user input is properly escaped and cannot be used to construct malicious SQL statements. This multi-layered approach ensures that the application remains secure even when using different data access patterns.

**Authentication & Authorization:**
The system implements proper authorization checks to ensure users can only access their own data. The GetUserBookmarkAsync method demonstrates the ownership verification pattern where the service checks that the requested resource belongs to the authenticated user before returning it. This approach prevents unauthorized access to other users' data and maintains proper data isolation. The method returns null for unauthorized access attempts, allowing the calling code to handle the security violation appropriately.

## Deployment

### Environment Configuration
The system uses JSON configuration files to manage environment-specific settings including database connection strings and frontend URLs for CORS configuration. The ConnectionStrings section contains the PostgreSQL database connection information with host, port, database name, username, and password. The FrontendUrls array specifies the allowed origins for CORS requests, supporting multiple frontend applications running on different localhost ports during development. This configuration approach allows for easy environment switching between development, staging, and production without code changes.

### Docker Support
- **Database**: PostgreSQL container with initialization scripts
- **Service**: .NET 9.0 container with optimized runtime
- **Environment Variables**: Secure credential management

## Stripe Payments & Subscriptions

### Overview
This service implements Stripe-based premium subscriptions using `StripeService` and a `PremiumSubscription` repository for persistence.

### Architecture
- Checkout session creation via `POST /api/PremiumSubscription/create-checkout-session`
- Stripe Checkout redirects user, then sends webhooks to `POST /api/PremiumSubscription/webhook`
- Webhooks handle: `checkout.session.completed`, `customer.subscription.created|updated|deleted`
- Subscription state persisted in `PremiumSubscription` with period tracking

### Endpoints
- `GET /api/PremiumSubscription/status` — returns `{ IsPremium: boolean }`
- `POST /api/PremiumSubscription/create-checkout-session` — returns `{ CheckoutUrl }`
- `POST /api/PremiumSubscription/webhook` — Stripe webhook receiver (unauthenticated)
- `POST /api/PremiumSubscription/cancel` — cancels at period end

### Environment Variables
- `STRIPE_SECRET_KEY`: Stripe API secret key
- `STRIPE_PRICE_ID`: Price ID for the subscription product
- `STRIPE_WEBHOOK_SECRET`: Webhook signing secret

### Data Model Summary
`PremiumSubscription` stores: `UserId`, `StripeSubscriptionId`, `StripeCustomerId`, `Status`, `CurrentPeriodStart`, `CurrentPeriodEnd`, `CancelAtPeriodEnd`.

## API Endpoints Summary

### User Management
- `GET /api/users` - List all users
- `GET /api/users/{id}` - Get user by ID
- `GET /api/users/search?q={term}` - Search users
- `POST /api/users/register` - Register new user
- `PUT /api/users/{id}` - Update user profile

### OAuth Operations
- `POST /api/oauth/validate` - Validate access token
- `POST /api/oauth/refresh` - Refresh access token
- `GET /api/oauth/me` - Get current user profile
- `GET /api/oauth/token-status` - Check token status

### Bookmark Management
- `POST /api/users/{userId}/bookmarks` - Add bookmark
- `GET /api/users/{userId}/bookmarks` - List user bookmarks
- `DELETE /api/users/{userId}/bookmarks/{portfolioId}` - Remove bookmark

### User Reporting
- `POST /api/users/{userId}/report` - Report a user
- `GET /api/users/{userId}/reports` - Get user reports
- `GET /api/users/reports/by-reporter/{reporterId}` - Get reports by reporter

## Future Enhancements

### Planned Features
1. **Rate Limiting**: API request throttling
2. **Caching**: Redis-based response caching
3. **Audit Logging**: Comprehensive user action tracking
4. **Multi-factor Authentication**: Enhanced security
5. **User Roles**: Role-based access control
6. **API Versioning**: Backward compatibility support

### Scalability Improvements
1. **Database Sharding**: Horizontal scaling
2. **Microservice Communication**: Event-driven architecture
3. **Load Balancing**: Multiple service instances
4. **Monitoring**: Health checks and metrics

## Contributing

### Development Guidelines
1. **Code Style**: Follow C# coding conventions
2. **Testing**: Maintain high test coverage
3. **Documentation**: Update this README for changes
4. **Validation**: Use existing validation framework
5. **Error Handling**: Implement proper exception handling

### Code Review Checklist
- [ ] Interface implementation
- [ ] Validation logic
- [ ] Error handling
- [ ] Unit tests
- [ ] Documentation updates
- [ ] Performance considerations

## Support

For technical support or questions about the Backend User Service:
- **Repository**: Check the main project repository
- **Issues**: Report bugs or feature requests
- **Documentation**: Refer to this README and inline code comments
- **Testing**: Run the test suite for validation

---

*This documentation covers the complete architecture and implementation details of the Backend User Service. For specific implementation questions, refer to the inline code comments and unit tests.*
