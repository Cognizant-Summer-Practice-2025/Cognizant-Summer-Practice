# Backend Portfolio Service

## Table of Contents

### [1. Overview](#1-overview)
### [2. Architecture](#2-architecture)
- [2.1 Technology Stack](#21-technology-stack)
- [2.2 Project Structure](#22-project-structure)

### [3. Core Components](#3-core-components)
- [3.1 Data Models](#31-data-models)
  - [3.1.1 Portfolio Entity](#311-portfolio-entity)
  - [3.1.2 Project Entity](#312-project-entity)
  - [3.1.3 Experience Entity](#313-experience-entity)
  - [3.1.4 Skill Entity](#314-skill-entity)
  - [3.1.5 BlogPost Entity](#315-blogpost-entity)
  - [3.1.6 PortfolioTemplate Entity](#316-portfoliotemplate-entity)
  - [3.1.7 Bookmark Entity](#317-bookmark-entity)
- [3.2 Database Layer](#32-database-layer)
  - [3.2.1 PortfolioDbContext](#321-portfoliodbcontext)
  - [3.2.2 Repository Pattern](#322-repository-pattern)
- [3.3 Service Layer](#33-service-layer)
  - [3.3.1 Query Services](#331-query-services)
  - [3.3.2 Command Services](#332-command-services)
  - [3.3.3 Template Services](#333-template-services)
  - [3.3.4 Image Services](#334-image-services)
  - [3.3.5 Authentication Services](#335-authentication-services)
- [3.4 API Controllers](#34-api-controllers)
  - [3.4.1 PortfolioController](#341-portfoliocontroller)
  - [3.4.2 ProjectController](#342-projectcontroller)
  - [3.4.3 PortfolioTemplateController](#343-portfoliotemplatecontroller)
  - [3.4.4 ImageController](#344-imagecontroller)
  - [3.4.5 BookmarkController](#345-bookmarkcontroller)

### [4. Data Flow Architecture](#4-data-flow-architecture)
- [4.1 Portfolio Creation Flow](#41-portfolio-creation-flow)
- [4.2 Portfolio Query Flow](#42-portfolio-query-flow)
- [4.3 Content Management Flow](#43-content-management-flow)
- [4.4 Image Upload Flow](#44-image-upload-flow)

### [5. Configuration](#5-configuration)
- [5.1 Database Configuration](#51-database-configuration)
- [5.2 Service Registration](#52-service-registration)
- [5.3 CORS Configuration](#53-cors-configuration)
- [5.4 HTTP Client Configuration](#54-http-client-configuration)

### [6. Implementation Patterns](#6-implementation-patterns)
- [6.1 CQRS Pattern](#61-cqrs-pattern)
- [6.2 Repository Pattern](#62-repository-pattern)
- [6.3 Mapping System](#63-mapping-system)
- [6.4 Validation Framework](#64-validation-framework)
- [6.5 Caching Strategy](#65-caching-strategy)

### [7. External Service Integration](#7-external-service-integration)
- [7.1 User Service Integration](#71-user-service-integration)
- [7.2 Authentication Strategy](#72-authentication-strategy)
- [7.3 HTTP Client Management](#73-http-client-management)

### [8. Performance Optimizations](#8-performance-optimizations)
- [8.1 Database Indexing](#81-database-indexing)
- [8.2 Caching Implementation](#82-caching-implementation)
- [8.3 Query Optimization](#83-query-optimization)

### [9. Security Features](#9-security-features)
- [9.1 Input Validation](#91-input-validation)
- [9.2 Authentication & Authorization](#92-authentication--authorization)
- [9.3 Data Protection](#93-data-protection)

### [10. Testing Strategy](#10-testing-strategy)
- [10.1 Unit Testing](#101-unit-testing)
- [10.2 Integration Testing](#102-integration-testing)
- [10.3 Test Data Management](#103-test-data-management)

### [11. Deployment](#11-deployment)
- [11.1 Environment Configuration](#111-environment-configuration)
- [11.2 Docker Support](#112-docker-support)

### [12. API Endpoints Summary](#12-api-endpoints-summary)
- [12.1 Portfolio Management](#121-portfolio-management)
- [12.2 Project Management](#122-project-management)
- [12.3 Content Management](#123-content-management)
- [12.4 Image Management](#124-image-management)

### [13. Future Enhancements](#13-future-enhancements)
- [13.1 Planned Features](#131-planned-features)
- [13.2 Scalability Improvements](#132-scalability-improvements)

### [14. Contributing](#14-contributing)
- [14.1 Development Guidelines](#141-development-guidelines)
- [14.2 Code Review Checklist](#142-code-review-checklist)

### [15. Support](#15-support)

---

## 1. Overview

The Backend Portfolio Service is a comprehensive .NET 9.0 microservice responsible for managing portfolio showcases, projects, skills, experience, blog posts, and related content. It provides a robust foundation for portfolio management in a professional showcase platform, implementing CQRS (Command Query Responsibility Segregation) pattern for optimal performance and maintainability.

**Key Features:**
- **Portfolio Management**: Create, update, and manage professional portfolios
- **Content Management**: Handle projects, skills, experience, and blog posts
- **Template System**: Support for multiple portfolio templates
- **Image Management**: Upload and serve portfolio images
- **Caching Strategy**: Redis-based caching for improved performance
- **External Integration**: Seamless integration with user service
- **CQRS Architecture**: Separate command and query responsibilities

## 2. Architecture

### 2.1 Technology Stack
- **Framework**: .NET 9.0
- **Database**: PostgreSQL with Entity Framework Core
- **Caching**: Redis with memory cache fallback
- **Authentication**: OAuth 2.0 integration with external user service
- **API Documentation**: Swagger/OpenAPI
- **Validation**: Custom validation framework with FluentValidation
- **Logging**: Built-in .NET logging with ILogger
- **HTTP Client**: HttpClient with configuration and retry policies

### 2.2 Project Structure
```
backend-portfolio/
├── Config/                          # Configuration classes
│   ├── ApiDocumentationConfiguration.cs
│   ├── CorsConfiguration.cs
│   ├── DatabaseConfiguration.cs
│   ├── HttpClientConfiguration.cs
│   ├── JsonConfiguration.cs
│   ├── MiddlewareConfiguration.cs
│   └── ServiceRegistrationConfiguration.cs
├── Controllers/                     # API endpoints
│   ├── PortfolioController.cs
│   ├── ProjectController.cs
│   ├── PortfolioTemplateController.cs
│   ├── ImageController.cs
│   ├── ImageUploadController.cs
│   └── BookmarkController.cs
├── Data/                           # Database context and configuration
│   └── PortfolioDbContext.cs
├── DTO/                            # Data Transfer Objects
│   ├── Portfolio/
│   │   ├── Request/
│   │   └── Response/
│   ├── Project/
│   ├── Experience/
│   ├── Skill/
│   ├── BlogPost/
│   ├── PortfolioTemplate/
│   ├── Bookmark/
│   ├── ImageUpload/
│   └── Pagination/
├── Middleware/                      # Custom middleware
│   └── OAuth2Middleware.cs
├── Models/                          # Entity models
│   ├── Portfolio.cs
│   ├── Project.cs
│   ├── Experience.cs
│   ├── Skill.cs
│   ├── BlogPost.cs
│   ├── PortfolioTemplate.cs
│   ├── Bookmark.cs
│   ├── Enums.cs
│   └── Validation/
├── Repositories/                    # Data access layer
│   ├── IPortfolioRepository.cs
│   ├── PortfolioRepository.cs
│   ├── IProjectRepository.cs
│   ├── ProjectRepository.cs
│   ├── IExperienceRepository.cs
│   ├── ExperienceRepository.cs
│   ├── ISkillRepository.cs
│   ├── SkillRepository.cs
│   ├── IBlogPostRepository.cs
│   ├── BlogPostRepository.cs
│   ├── IPortfolioTemplateRepository.cs
│   ├── PortfolioTemplateRepository.cs
│   ├── IBookmarkRepository.cs
│   └── BookmarkRepository.cs
├── Services/                        # Business logic layer
│   ├── Abstractions/               # Service interfaces
│   ├── Mappers/                    # Entity-DTO mappers
│   ├── Validators/                 # Validation logic
│   ├── External/                   # External service integration
│   ├── PortfolioQueryService.cs
│   ├── PortfolioCommandService.cs
│   ├── ProjectQueryService.cs
│   ├── ProjectCommandService.cs
│   ├── PortfolioTemplateService.cs
│   ├── ImageUploadUtility.cs
│   ├── MemoryCacheService.cs
│   ├── UserAuthenticationService.cs
│   ├── AuthenticationContextService.cs
│   ├── AuthorizationPathService.cs
│   └── OAuth2AuthenticationStrategy.cs
└── Tests/                          # Unit tests
```

## 3. Core Components

### 3.1 Data Models

#### 3.1.1 Portfolio Entity
The central entity representing user portfolio showcases with comprehensive content management:

**Identity Properties:**
- `Id` (Guid): Primary key, auto-generated using `Guid.NewGuid()`
- `UserId` (Guid): Foreign key to external user service (required)
- `TemplateId` (Guid): Foreign key to PortfolioTemplate entity (required)

**Content Properties:**
- `Title` (string, 255 chars): Portfolio title (required)
- `Bio` (text): User biography and description
- `Components` (text): JSON configuration for portfolio layout and components

**Status Properties:**
- `ViewCount` (int): Number of portfolio views, defaults to 0
- `LikeCount` (int): Number of portfolio likes, defaults to 0
- `Visibility` (Visibility enum): Public, Private, or Unlisted
- `IsPublished` (bool): Publication status, defaults to false

**Timestamps:**
- `CreatedAt` (DateTime): Creation time, auto-set to UTC
- `UpdatedAt` (DateTime): Last modification time, auto-updated

**Navigation Properties (EF Core Relationships):**
- `Template`: One-to-one relationship with PortfolioTemplate
- `Projects`: One-to-many relationship with Project entities
- `Experience`: One-to-many relationship with Experience entities
- `Skills`: One-to-many relationship with Skill entities
- `BlogPosts`: One-to-many relationship with BlogPost entities
- `Bookmarks`: One-to-many relationship with Bookmark entities

**Database Constraints:**
- UserId uniqueness enforced at database level
- Snake case naming convention for database columns
- Cascade delete for related entities
- Default values for counters and status fields

#### 3.1.2 Project Entity
Manages portfolio projects with comprehensive project information:

**Core Properties:**
- `Id` (Guid): Primary key for the project
- `PortfolioId` (Guid): Foreign key to Portfolio entity (required)
- `Title` (string, 255 chars): Project title (required)
- `Description` (text): Detailed project description
- `ImageUrl` (text): Project image or screenshot URL
- `DemoUrl` (text): Live demo URL
- `GithubUrl` (text): GitHub repository URL
- `Technologies` (string[]): Array of technologies used
- `Featured` (bool): Featured project flag, defaults to false

**Timestamps:**
- `CreatedAt` (DateTime): Project creation time
- `UpdatedAt` (DateTime): Last modification time

**Navigation Properties:**
- `Portfolio`: Many-to-one relationship with Portfolio entity
- Cascade delete when portfolio is removed

#### 3.1.3 Experience Entity
Tracks professional experience and work history:

**Core Properties:**
- `Id` (Guid): Primary key for the experience record
- `PortfolioId` (Guid): Foreign key to Portfolio entity (required)
- `JobTitle` (string): Professional job title (required)
- `CompanyName` (string): Company or organization name (required)
- `StartDate` (DateTime): Employment start date
- `EndDate` (DateTime?): Employment end date (nullable for current positions)
- `IsCurrent` (bool): Current position flag, defaults to false
- `Description` (text): Detailed job description and responsibilities

**Timestamps:**
- `CreatedAt` (DateTime): Record creation time
- `UpdatedAt` (DateTime): Last modification time

**Navigation Properties:**
- `Portfolio`: Many-to-one relationship with Portfolio entity
- Cascade delete when portfolio is removed

#### 3.1.4 Skill Entity
Manages technical skills and competencies:

**Core Properties:**
- `Id` (Guid): Primary key for the skill record
- `PortfolioId` (Guid): Foreign key to Portfolio entity (required)
- `Name` (string): Skill name (required)
- `Level` (int?): Skill proficiency level (optional)
- `Category` (string): Skill category or domain
- `Description` (text): Skill description and details

**Timestamps:**
- `CreatedAt` (DateTime): Record creation time
- `UpdatedAt` (DateTime): Last modification time

**Navigation Properties:**
- `Portfolio`: Many-to-one relationship with Portfolio entity
- Cascade delete when portfolio is removed

#### 3.1.5 BlogPost Entity
Manages portfolio blog posts and articles:

**Core Properties:**
- `Id` (Guid): Primary key for the blog post
- `PortfolioId` (Guid): Foreign key to Portfolio entity (required)
- `Title` (string): Blog post title (required)
- `Content` (text): Blog post content and body
- `Excerpt` (text): Short excerpt or summary
- `ImageUrl` (text): Featured image URL
- `Tags` (string[]): Array of tags for categorization
- `IsPublished` (bool): Publication status
- `PublishedAt` (DateTime?): Publication timestamp

**Timestamps:**
- `CreatedAt` (DateTime): Post creation time
- `UpdatedAt` (DateTime): Last modification time

**Navigation Properties:**
- `Portfolio`: Many-to-one relationship with Portfolio entity
- Cascade delete when portfolio is removed

#### 3.1.6 PortfolioTemplate Entity
Provides portfolio design templates and layouts:

**Core Properties:**
- `Id` (Guid): Primary key for the template
- `Name` (string): Template name (required)
- `Description` (text): Template description
- `PreviewImage` (text): Template preview image URL
- `Components` (text): JSON configuration for template components
- `IsActive` (bool): Template availability, defaults to true

**Timestamps:**
- `CreatedAt` (DateTime): Template creation time
- `UpdatedAt` (DateTime): Last modification time

**Navigation Properties:**
- `Portfolios`: One-to-many relationship with Portfolio entities
- Cascade delete when template is removed

#### 3.1.7 Bookmark Entity
Manages user portfolio bookmarks:

**Core Properties:**
- `Id` (Guid): Primary key for the bookmark
- `UserId` (Guid): User who created the bookmark (required)
- `PortfolioId` (Guid): Bookmarked portfolio (required)
- `Notes` (text): User notes about the bookmark
- `IsPublic` (bool): Public bookmark visibility

**Timestamps:**
- `CreatedAt` (DateTime): Bookmark creation time

**Database Constraints:**
- Unique constraint on (UserId, PortfolioId) combination
- Cascade delete when portfolio is removed

### 3.2 Database Layer

#### 3.2.1 PortfolioDbContext
Entity Framework Core DbContext with comprehensive configuration:

**Entity Configuration:**
- **Snake Case Naming**: Uses `UseSnakeCaseNamingConvention()` for database columns
- **Relationship Configuration**: Proper foreign key relationships with cascade deletes
- **Index Strategy**: Strategic indexes for performance optimization
- **Default Values**: Automatic default values for counters and status fields

**Database Indexes:**
```csharp
// Portfolio entity indexes
entity.HasIndex(e => e.UserId);                    // User lookup optimization
entity.Property(e => e.ViewCount).HasDefaultValue(0);
entity.Property(e => e.LikeCount).HasDefaultValue(0);
entity.Property(e => e.Visibility).HasDefaultValue(Visibility.Public);
entity.Property(e => e.IsPublished).HasDefaultValue(false);

// Project entity indexes
entity.HasIndex(e => e.PortfolioId);               // Portfolio lookup optimization
entity.Property(e => e.Featured).HasDefaultValue(false);

// Experience entity indexes
entity.HasIndex(e => e.PortfolioId);               // Portfolio lookup optimization
entity.Property(e => e.IsCurrent).HasDefaultValue(false);

// Skill entity indexes
entity.HasIndex(e => e.PortfolioId);               // Portfolio lookup optimization

// BlogPost entity indexes
entity.HasIndex(e => e.PortfolioId);               // Portfolio lookup optimization
entity.Property(e => e.IsPublished).HasDefaultValue(false);

// Bookmark entity indexes
entity.HasIndex(e => new { e.UserId, e.PortfolioId }).IsUnique();  // Bookmark uniqueness
```

**Cascade Delete Strategy:**
- Portfolio deletion triggers cascade delete of all related entities
- Ensures data consistency and prevents orphaned records
- Proper cleanup of projects, experience, skills, blog posts, and bookmarks

#### 3.2.2 Repository Pattern
Data access layer implementing the Repository pattern for clean separation of concerns:

**IPortfolioRepository:**
```csharp
public interface IPortfolioRepository
{
    Task<IEnumerable<Portfolio>> GetAllPortfoliosAsync();
    Task<Portfolio?> GetPortfolioByIdAsync(Guid id);
    Task<IEnumerable<Portfolio>> GetPortfoliosByUserIdAsync(Guid userId);
    Task<Portfolio> CreatePortfolioAsync(Portfolio portfolio);
    Task<Portfolio?> UpdatePortfolioAsync(Guid id, PortfolioUpdateRequest request);
    Task<bool> DeletePortfolioAsync(Guid id);
    Task<bool> IncrementViewCountAsync(Guid id);
    Task<bool> IncrementLikeCountAsync(Guid id);
}
```

**IProjectRepository:**
```csharp
public interface IProjectRepository
{
    Task<IEnumerable<Project>> GetProjectsByPortfolioIdAsync(Guid portfolioId);
    Task<Project?> GetProjectByIdAsync(Guid id);
    Task<Project> CreateProjectAsync(Project project);
    Task<Project?> UpdateProjectAsync(Guid id, ProjectUpdateRequest request);
    Task<bool> DeleteProjectAsync(Guid id);
}
```

**Repository Implementation Benefits:**
- **Testability**: Easy to mock for unit testing
- **Dependency Injection**: Scoped services for proper lifecycle management
- **Interface Segregation**: Each repository handles specific entity types
- **Data Access Abstraction**: Business logic doesn't depend on EF Core directly

### 3.3 Service Layer

#### 3.3.1 Query Services
Business logic layer implementing CQRS query responsibilities:

**PortfolioQueryService:**
```csharp
public class PortfolioQueryService : IPortfolioQueryService
{
    // Portfolio retrieval operations
    public async Task<IEnumerable<PortfolioSummaryResponse>> GetAllPortfoliosAsync()
    public async Task<IEnumerable<PortfolioDetailResponse>> GetAllPortfoliosDetailedAsync()
    public async Task<PortfolioDetailResponse?> GetPortfolioByIdAsync(Guid id)
    public async Task<IEnumerable<PortfolioSummaryResponse>> GetPortfoliosByUserIdAsync(Guid userId)
    public async Task<PortfolioDetailResponse?> GetPortfolioWithContentAsync(Guid id)
    
    // Search and filtering operations
    public async Task<IEnumerable<PortfolioSummaryResponse>> SearchPortfoliosAsync(string searchTerm)
    public async Task<IEnumerable<PortfolioSummaryResponse>> GetPortfoliosByTemplateAsync(string templateName)
}
```

**ProjectQueryService:**
```csharp
public class ProjectQueryService : IProjectQueryService
{
    // Project retrieval operations
    public async Task<IEnumerable<ProjectResponse>> GetProjectsByPortfolioIdAsync(Guid portfolioId)
    public async Task<ProjectResponse?> GetProjectByIdAsync(Guid id)
    public async Task<IEnumerable<ProjectResponse>> GetFeaturedProjectsAsync(Guid portfolioId)
}
```

**Responsibilities**: Data retrieval, search functionality, content aggregation, caching integration

#### 3.3.2 Command Services
Business logic layer implementing CQRS command responsibilities:

**PortfolioCommandService:**
```csharp
public class PortfolioCommandService : IPortfolioCommandService
{
    // Portfolio creation and modification
    public async Task<PortfolioResponse> CreatePortfolioAsync(PortfolioCreateRequest request)
    public async Task<PortfolioResponse?> UpdatePortfolioAsync(Guid id, PortfolioUpdateRequest request)
    public async Task<bool> DeletePortfolioAsync(Guid id)
    public async Task<bool> PublishPortfolioAsync(Guid id)
    public async Task<bool> UnpublishPortfolioAsync(Guid id)
    
    // Content management operations
    public async Task<bool> AddPortfolioContentAsync(BulkPortfolioContentRequest request)
    public async Task<bool> UpdatePortfolioContentAsync(BulkPortfolioContentRequest request)
}
```

**ProjectCommandService:**
```csharp
public class ProjectCommandService : IProjectCommandService
{
    // Project management operations
    public async Task<ProjectResponse> CreateProjectAsync(ProjectCreateRequest request)
    public async Task<ProjectResponse?> UpdateProjectAsync(Guid id, ProjectUpdateRequest request)
    public async Task<bool> DeleteProjectAsync(Guid id)
    public async Task<bool> ToggleFeaturedStatusAsync(Guid id)
}
```

**Responsibilities**: Data creation, modification, deletion, business rule enforcement, validation

#### 3.3.3 Template Services
Portfolio template management and configuration:

**PortfolioTemplateService:**
```csharp
public class PortfolioTemplateService : IPortfolioTemplateService
{
    // Template management operations
    public async Task<IEnumerable<PortfolioTemplateResponse>> GetAllTemplatesAsync()
    public async Task<PortfolioTemplateResponse?> GetTemplateByNameAsync(string name)
    public async Task<PortfolioTemplateResponse> CreateTemplateAsync(TemplateCreateRequest request)
    public async Task<bool> UpdateTemplateAsync(Guid id, TemplateUpdateRequest request)
    public async Task<bool> DeleteTemplateAsync(Guid id)
    public async Task<bool> ToggleTemplateStatusAsync(Guid id)
}
```

**Responsibilities**: Template CRUD operations, template availability management, component configuration

#### 3.3.4 Image Services
Image upload and management functionality:

**ImageUploadUtility:**
```csharp
public class ImageUploadUtility : IImageUploadUtility
{
    // Image processing operations
    public async Task<ImageUploadResponse> UploadImageAsync(ImageUploadRequest request)
    public async Task<bool> ValidateImageAsync(IFormFile file)
    public async Task<string> ProcessImageAsync(IFormFile file)
    public async Task<bool> DeleteImageAsync(string imageUrl)
}
```

**Responsibilities**: Image validation, processing, storage, and cleanup

#### 3.3.5 Authentication Services
External service integration and authentication:

**UserAuthenticationService:**
```csharp
public class UserAuthenticationService : IUserAuthenticationService
{
    // Authentication operations
    public async Task<UserInfo?> GetUserInfoAsync(string accessToken)
    public async Task<bool> ValidateUserAccessAsync(Guid userId, string accessToken)
    public async Task<bool> IsUserAuthorizedAsync(Guid userId, string accessToken)
}
```

**OAuth2AuthenticationStrategy:**
```csharp
public class OAuth2AuthenticationStrategy : IAuthenticationStrategy
{
    // OAuth 2.0 operations
    public async Task<AuthenticationResult> AuthenticateAsync(string accessToken)
    public async Task<bool> ValidateTokenAsync(string accessToken)
    public async Task<UserInfo?> GetUserFromTokenAsync(string accessToken)
}
```

**Responsibilities**: External user service integration, OAuth 2.0 token validation, user authorization

### 3.4 API Controllers

#### 3.4.1 PortfolioController
Comprehensive portfolio management endpoints:

**Portfolio CRUD Operations:**
```csharp
[HttpGet]                           // GET /api/portfolio
public async Task<IActionResult> GetAllPortfolios()

[HttpGet("{id}")]                   // GET /api/portfolio/{id}
public async Task<IActionResult> GetPortfolioById(Guid id)

[HttpGet("user/{userId}")]          // GET /api/portfolio/user/{userId}
public async Task<IActionResult> GetPortfoliosByUserId(Guid userId)

[HttpPost]                          // POST /api/portfolio
public async Task<IActionResult> CreatePortfolio(PortfolioCreateRequest request)

[HttpPut("{id}")]                   // PUT /api/portfolio/{id}
public async Task<IActionResult> UpdatePortfolio(Guid id, PortfolioUpdateRequest request)

[HttpDelete("{id}")]                // DELETE /api/portfolio/{id}
public async Task<IActionResult> DeletePortfolio(Guid id)
```

**Content Management Endpoints:**
```csharp
[HttpPost("{id}/content")]          // POST /api/portfolio/{id}/content
public async Task<IActionResult> AddPortfolioContent(Guid id, BulkPortfolioContentRequest request)

[HttpPut("{id}/content")]           // PUT /api/portfolio/{id}/content
public async Task<IActionResult> UpdatePortfolioContent(Guid id, BulkPortfolioContentRequest request)

[HttpPost("{id}/publish")]          // POST /api/portfolio/{id}/publish
public async Task<IActionResult> PublishPortfolio(Guid id)

[HttpPost("{id}/unpublish")]        // POST /api/portfolio/{id}/unpublish
public async Task<IActionResult> UnpublishPortfolio(Guid id)
```

**Analytics Endpoints:**
```csharp
[HttpPost("{id}/view")]             // POST /api/portfolio/{id}/view
public async Task<IActionResult> IncrementViewCount(Guid id)

[HttpPost("{id}/like")]             // POST /api/portfolio/{id}/like
public async Task<IActionResult> IncrementLikeCount(Guid id)
```

**Controller Implementation Details:**
- **CQRS Pattern**: Separate query and command service usage
- **Error Handling**: Comprehensive try-catch blocks with proper HTTP status codes
- **Validation**: Input validation before service calls
- **Response Mapping**: Consistent response format across all endpoints
- **Async Operations**: All operations are async for better performance

#### 3.4.2 ProjectController
Project management and content operations:

**Project CRUD Operations:**
```csharp
[HttpGet("portfolio/{portfolioId}")] // GET /api/project/portfolio/{portfolioId}
public async Task<IActionResult> GetProjectsByPortfolioId(Guid portfolioId)

[HttpGet("{id}")]                   // GET /api/project/{id}
public async Task<IActionResult> GetProjectById(Guid id)

[HttpPost]                          // POST /api/project
public async Task<IActionResult> CreateProject(ProjectCreateRequest request)

[HttpPut("{id}")]                   // PUT /api/project/{id}
public async Task<IActionResult> UpdateProject(Guid id, ProjectUpdateRequest request)

[HttpDelete("{id}")]                // DELETE /api/project/{id}
public async Task<IActionResult> DeleteProject(Guid id)
```

**Project Management Endpoints:**
```csharp
[HttpPost("{id}/feature")]          // POST /api/project/{id}/feature
public async Task<IActionResult> ToggleFeaturedStatus(Guid id)

[HttpGet("featured/{portfolioId}")] // GET /api/project/featured/{portfolioId}
public async Task<IActionResult> GetFeaturedProjects(Guid portfolioId)
```

#### 3.4.3 PortfolioTemplateController
Template management and configuration:

**Template CRUD Operations:**
```csharp
[HttpGet]                           // GET /api/portfoliotemplate
public async Task<IActionResult> GetAllTemplates()

[HttpGet("{id}")]                   // GET /api/portfoliotemplate/{id}
public async Task<IActionResult> GetTemplateById(Guid id)

[HttpGet("name/{name}")]            // GET /api/portfoliotemplate/name/{name}
public async Task<IActionResult> GetTemplateByName(string name)

[HttpPost]                          // POST /api/portfoliotemplate
public async Task<IActionResult> CreateTemplate(TemplateCreateRequest request)

[HttpPut("{id}")]                   // PUT /api/portfoliotemplate/{id}
public async Task<IActionResult> UpdateTemplate(Guid id, TemplateUpdateRequest request)

[HttpDelete("{id}")]                // DELETE /api/portfoliotemplate/{id}
public async Task<IActionResult> DeleteTemplate(Guid id)
```

#### 3.4.4 ImageController
Image serving and management:

**Image Operations:**
```csharp
[HttpGet("{imageName}")]            // GET /api/image/{imageName}
public async Task<IActionResult> GetImage(string imageName)

[HttpDelete("{imageName}")]         // DELETE /api/image/{imageName}
public async Task<IActionResult> DeleteImage(string imageName)
```

#### 3.4.5 BookmarkController
Portfolio bookmark management:

**Bookmark Operations:**
```csharp
[HttpGet("user/{userId}")]          // GET /api/bookmark/user/{userId}
public async Task<IActionResult> GetUserBookmarks(Guid userId)

[HttpPost]                          // POST /api/bookmark
public async Task<IActionResult> CreateBookmark(BookmarkCreateRequest request)

[HttpPut("{id}")]                   // PUT /api/bookmark/{id}
public async Task<IActionResult> UpdateBookmark(Guid id, BookmarkUpdateRequest request)

[HttpDelete("{id}")]                // DELETE /api/bookmark/{id}
public async Task<IActionResult> DeleteBookmark(Guid id)
```

## 4. Data Flow Architecture

### 4.1 Portfolio Creation Flow
The portfolio creation process follows a comprehensive workflow that ensures data integrity and proper validation:

**Initial Request Processing:**
1. **Client Request**: Frontend sends `PortfolioCreateRequest` with user ID, template selection, and initial content
2. **Authentication Validation**: OAuth2Middleware validates the access token and extracts user information
3. **Input Validation**: Request undergoes comprehensive validation including required fields, data types, and business rules
4. **Template Verification**: System validates that the selected template exists and is active
5. **User Authorization**: AuthenticationContextService verifies the user has permission to create portfolios

**Business Logic Execution:**
1. **Portfolio Creation**: PortfolioCommandService creates the portfolio entity with default values
2. **Template Association**: Portfolio is linked to the selected template with component configuration
3. **Content Initialization**: Initial content is processed and stored in the Components JSON field
4. **Database Persistence**: PortfolioRepository saves the portfolio to the database
5. **Cache Invalidation**: MemoryCacheService invalidates relevant cache entries

**Response Generation:**
1. **Entity Mapping**: Portfolio entity is mapped to PortfolioResponse DTO
2. **Success Response**: HTTP 201 Created response with portfolio details
3. **Error Handling**: Comprehensive error responses for validation failures or business rule violations

### 4.2 Portfolio Query Flow
The portfolio query process implements efficient data retrieval with caching and optimization:

**Request Processing:**
1. **Cache Check**: MemoryCacheService checks for cached portfolio data
2. **Cache Hit**: If data exists in cache, it's returned immediately
3. **Cache Miss**: If no cached data, database query is executed

**Database Query Execution:**
1. **Repository Call**: PortfolioQueryService calls appropriate repository methods
2. **EF Core Query**: Entity Framework Core generates optimized SQL queries
3. **Data Retrieval**: Database returns portfolio data with related entities
4. **Result Mapping**: Raw data is mapped to response DTOs

**Caching Strategy:**
1. **Cache Population**: Retrieved data is stored in cache with appropriate TTL
2. **Cache Keys**: Structured cache keys for different query types
3. **Cache Invalidation**: Cache is invalidated when portfolio data changes

**Response Delivery:**
1. **Data Aggregation**: Related content (projects, skills, experience) is aggregated
2. **DTO Mapping**: Final response DTOs are created with all necessary data
3. **Performance Metrics**: Query execution time and cache hit rates are logged

### 4.3 Content Management Flow
The content management process handles bulk operations for portfolio content:

**Content Addition Flow:**
1. **Request Validation**: BulkPortfolioContentRequest is validated for structure and content
2. **Portfolio Verification**: System verifies portfolio exists and user has access
3. **Content Processing**: Each content type (projects, skills, experience) is processed individually
4. **Entity Creation**: New entities are created and linked to the portfolio
5. **Batch Persistence**: All content is saved in a single transaction for consistency

**Content Update Flow:**
1. **Existing Content Check**: System verifies content exists before updating
2. **Change Detection**: Only modified fields are updated to minimize database operations
3. **Validation**: Updated content undergoes the same validation as new content
4. **Audit Trail**: UpdatedAt timestamps are automatically updated
5. **Cache Refresh**: Related cache entries are refreshed with new data

**Content Deletion Flow:**
1. **Dependency Check**: System verifies no other entities depend on the content
2. **Cascade Handling**: Related entities are properly cleaned up
3. **Database Cleanup**: Content is removed from the database
4. **Cache Invalidation**: All related cache entries are invalidated

### 4.4 Image Upload Flow
The image upload process handles file validation, processing, and storage:

**File Validation:**
1. **File Type Check**: System validates file extensions and MIME types
2. **Size Validation**: File size is checked against configured limits
3. **Content Validation**: Image content is verified for malicious content
4. **Format Support**: Supported image formats are verified

**Image Processing:**
1. **Resize Operations**: Images are resized to appropriate dimensions
2. **Format Conversion**: Images are converted to web-optimized formats
3. **Quality Optimization**: Image quality is optimized for web delivery
4. **Thumbnail Generation**: Thumbnail versions are created for previews

**Storage Management:**
1. **File Naming**: Unique file names are generated to prevent conflicts
2. **Directory Structure**: Images are organized in logical directory structures
3. **Metadata Storage**: Image metadata is stored in the database
4. **Cleanup Operations**: Orphaned images are periodically cleaned up

## 5. Configuration

### 5.1 Database Configuration
The DatabaseConfiguration class implements comprehensive database setup that optimizes performance and maintains consistency:

**Entity Framework Configuration:**
The AddDbContext method configures Entity Framework Core with the Npgsql provider for PostgreSQL, enabling the use of advanced PostgreSQL features. The UseSnakeCaseNamingConvention method ensures that database columns follow PostgreSQL naming conventions, while the custom data source provides enum type mapping support for portfolio visibility and other enumerated values. The connection string is retrieved from configuration, supporting different environments through appsettings.json files.

**Database Connection Features:**
- **PostgreSQL Integration**: Full support for PostgreSQL-specific features and data types
- **Snake Case Convention**: Database columns follow PostgreSQL naming standards
- **Connection Pooling**: Efficient connection management for high-concurrency scenarios
- **Command Timeout**: Configurable timeout values for long-running queries
- **Retry Policies**: Automatic retry logic for transient database failures

**Entity Configuration Benefits:**
- **Performance Optimization**: Strategic indexes on frequently queried fields
- **Data Integrity**: Proper foreign key relationships with cascade delete support
- **Default Values**: Automatic default values for counters and status fields
- **Validation**: Database-level constraints for data consistency

### 5.2 Service Registration
The ServiceRegistrationConfiguration class implements a clean and organized approach to dependency injection setup:

**Service Registration Methods:**
The AddRepositoryServices method registers all data access layer components including repositories for portfolios, projects, experience, skills, blog posts, templates, and bookmarks. AddMapperServices registers entity-DTO transformation services that handle data mapping between different layers. AddBusinessServices registers core business logic services implementing the CQRS pattern, while AddAuthenticationServices registers OAuth and authentication-related services for external user service integration.

**Dependency Injection Benefits:**
- **Scoped Services**: Repositories and services use scoped lifetime for proper database context management
- **Interface-based**: All services use interfaces for easy mocking and testing
- **Modular Registration**: Services are grouped by functionality for better organization
- **Configuration-driven**: Service registration is driven by configuration files

**Service Lifetime Management:**
- **Singleton Services**: Configuration services and HTTP clients use singleton lifetime
- **Scoped Services**: Business logic services use scoped lifetime for request scope
- **Transient Services**: Utility services use transient lifetime for stateless operations

### 5.3 CORS Configuration
The CorsConfiguration class implements comprehensive Cross-Origin Resource Sharing setup that enables secure communication between the backend service and frontend applications:

**CORS Policy Configuration:**
The AddCors method configures CORS policies with the "AllowFrontend" policy that specifies allowed origins, HTTP methods, headers, and credential support. The policy uses the frontendUrls configuration to dynamically determine which origins are allowed, supporting multiple frontend applications during development. The AllowCredentials option enables cookie and authorization header support for authenticated requests.

**Security Features:**
- **Origin Restriction**: Only configured frontend URLs are allowed to access the API
- **Method Control**: Specific HTTP methods are allowed based on API requirements
- **Header Management**: Authorization and content-type headers are properly handled
- **Credential Support**: Cookies and authentication headers are supported for user sessions

**Development Configuration:**
- **Multiple Frontend Support**: Configuration supports multiple localhost ports for development
- **Environment-specific**: Different CORS settings for development, staging, and production
- **Dynamic Configuration**: CORS settings are loaded from configuration files

### 5.4 HTTP Client Configuration
The HttpClientConfiguration class implements robust HTTP client setup for external service communication:

**HTTP Client Features:**
The AddHttpClient method configures named HTTP clients with specific settings for different external services. The ConfigurePrimaryHttpMessageHandler method sets up the primary message handler with timeout configurations, retry policies, and connection pooling. The AddTransientHttpErrorPolicy method adds Polly retry policies for handling transient HTTP errors and network failures.

**Configuration Benefits:**
- **Timeout Management**: Configurable timeout values for different types of requests
- **Retry Logic**: Automatic retry for transient network failures
- **Connection Pooling**: Efficient connection reuse for better performance
- **Error Handling**: Comprehensive error handling with proper logging

**External Service Integration:**
- **User Service Communication**: HTTP client for user authentication and validation
- **Image Service Integration**: HTTP client for external image processing services
- **Template Service**: HTTP client for external template management services
- **Monitoring**: Request/response logging and performance metrics collection

## 6. Implementation Patterns

### 6.1 CQRS Pattern
The Command Query Responsibility Segregation pattern separates read and write operations for optimal performance and maintainability:

**Query Services Implementation:**
The PortfolioQueryService implements read-only operations with caching and optimization strategies. It handles data retrieval, search functionality, and content aggregation without modifying the underlying data. The service integrates with the MemoryCacheService to provide fast access to frequently requested data, implementing cache-aside patterns for portfolio details and search results.

**Command Services Implementation:**
The PortfolioCommandService handles all data modification operations including creation, updates, and deletion. It enforces business rules, validates input data, and ensures data consistency across related entities. The service implements transaction management for complex operations and provides proper error handling for business rule violations.

**Benefits of CQRS Implementation:**
- **Performance Optimization**: Read and write operations can be optimized independently
- **Scalability**: Query and command services can be scaled separately based on load
- **Maintainability**: Clear separation of concerns makes the codebase easier to maintain
- **Testing**: Services can be tested independently with focused unit tests
- **Caching Strategy**: Query services can implement aggressive caching without affecting write operations

### 6.2 Repository Pattern
The repository pattern provides a clean abstraction over the data access layer:

**Repository Interface Design:**
Each entity type has a dedicated repository interface that defines the contract for data access operations. The interfaces follow consistent naming conventions and provide methods for CRUD operations, specialized queries, and bulk operations. This design enables easy mocking for unit testing and provides a clear contract for data access requirements.

**Repository Implementation Benefits:**
- **Data Access Abstraction**: Business logic doesn't depend on Entity Framework Core directly
- **Testability**: Repositories can be easily mocked for unit testing scenarios
- **Interface Segregation**: Each repository handles specific entity types with focused responsibilities
- **Dependency Injection**: Repositories are registered as scoped services for proper lifecycle management

**Repository Method Patterns:**
- **Async Operations**: All repository methods are async for better performance
- **Nullable Returns**: Get methods return nullable types to handle missing data gracefully
- **Bulk Operations**: Support for bulk insert, update, and delete operations
- **Specialized Queries**: Methods for common query patterns like filtering and searching

### 6.3 Mapping System
The mapping system handles transformation between entities and DTOs:

**Entity to Response DTO Mapping:**
The PortfolioMapper class implements comprehensive mapping from Portfolio entities to various response DTOs. It handles complex transformations including nested object mapping, array processing, and conditional field inclusion. The mapper ensures that sensitive data is excluded from public responses and that data is properly formatted for client consumption.

**Request DTO to Entity Mapping:**
The mapping system converts incoming request DTOs to entity objects for database operations. It handles validation of required fields, type conversions, and default value assignment. The mapper ensures that only valid data reaches the database layer and provides clear error messages for invalid input.

**Mapping Benefits:**
- **Data Transformation**: Clean separation between internal entities and external DTOs
- **Validation**: Input validation during the mapping process
- **Security**: Sensitive data filtering in response mapping
- **Maintainability**: Centralized mapping logic for easy updates and modifications

### 6.4 Validation Framework
The validation framework ensures data integrity and business rule compliance:

**Input Validation:**
The validation framework implements comprehensive input validation for all incoming requests. It checks required fields, data types, string lengths, and business rule compliance. The framework provides detailed error messages and supports both client-side and server-side validation scenarios.

**Business Rule Validation:**
The system enforces business rules such as portfolio uniqueness per user, template availability, and content relationship constraints. Business rule validation occurs at the service layer to ensure data consistency and prevent invalid operations.

**Validation Benefits:**
- **Data Integrity**: Ensures only valid data reaches the database
- **User Experience**: Provides clear error messages for validation failures
- **Security**: Prevents malicious input and data corruption
- **Maintainability**: Centralized validation logic for consistent behavior

### 6.5 Caching Strategy
The caching strategy implements multiple layers of caching for optimal performance:

**Memory Cache Implementation:**
The MemoryCacheService provides in-memory caching for frequently accessed data. It implements cache-aside patterns for portfolio data, search results, and template information. The service uses configurable TTL values and implements cache invalidation strategies for data consistency.

**Cache Key Strategy:**
The caching system uses structured cache keys that include entity types, IDs, and query parameters. This approach enables efficient cache invalidation and prevents cache key collisions between different data types.

**Cache Invalidation:**
The system implements intelligent cache invalidation that removes related cache entries when data changes. This ensures that clients always receive up-to-date information while maintaining the performance benefits of caching.

## 7. External Service Integration

### 7.1 User Service Integration
The portfolio service integrates with an external user service for authentication and user management:

**Authentication Flow:**
The OAuth2Middleware intercepts incoming requests and validates access tokens with the external user service. It extracts user information from valid tokens and provides it to the authentication context for authorization decisions.

**User Validation:**
The UserAuthenticationService validates user access to portfolio resources by checking user permissions and ownership. It ensures that users can only access and modify their own portfolios unless they have appropriate permissions.

**Integration Benefits:**
- **Centralized User Management**: User data is managed in a dedicated service
- **Scalability**: User service can be scaled independently of portfolio service
- **Security**: Centralized authentication and authorization logic
- **Consistency**: Consistent user experience across multiple services

### 7.2 Authentication Strategy
The authentication strategy implements OAuth 2.0 token validation and user authorization:

**Token Validation:**
The OAuth2AuthenticationStrategy validates access tokens by making HTTP requests to the external user service. It handles token expiration, refresh scenarios, and provides user information for authenticated requests.

**Authorization Context:**
The AuthenticationContextService maintains the current user's authentication context throughout the request lifecycle. It provides user information and permissions to business logic services for authorization decisions.

**Security Features:**
- **Token Validation**: Secure token validation with external service
- **User Authorization**: Role-based access control for portfolio operations
- **Audit Logging**: Comprehensive logging of authentication and authorization events
- **Error Handling**: Secure error responses that don't leak sensitive information

### 7.3 HTTP Client Management
The HTTP client management system provides robust communication with external services:

**Named HTTP Clients:**
The system uses named HTTP clients for different external services, enabling service-specific configuration and monitoring. Each client can have different timeout values, retry policies, and connection settings.

**Retry Policies:**
The system implements Polly retry policies for handling transient network failures. Retry policies are configured based on the type of external service and the expected failure patterns.

**Monitoring and Logging:**
All external service communication is logged for monitoring and debugging purposes. The system tracks request/response times, success rates, and error patterns to identify performance issues and service degradation.

## 8. Performance Optimizations

### 8.1 Database Indexing
Strategic database indexing optimizes query performance for common operations:

**Primary Indexes:**
- **Portfolio Lookup**: Index on UserId for fast portfolio retrieval by user
- **Content Lookup**: Indexes on PortfolioId for related content queries
- **Template Lookup**: Index on template name for fast template retrieval
- **Bookmark Uniqueness**: Composite index on (UserId, PortfolioId) for bookmark operations

**Performance Benefits:**
- **Fast User Queries**: Sub-second response times for user portfolio lookups
- **Efficient Content Retrieval**: Optimized queries for projects, skills, and experience
- **Scalable Search**: Fast search operations across large datasets
- **Concurrent Access**: Efficient handling of multiple concurrent requests

### 8.2 Caching Implementation
Multi-layer caching strategy provides fast access to frequently requested data:

**Cache Layers:**
- **Memory Cache**: Fast access to portfolio data and search results
- **Database Query Optimization**: Efficient SQL queries with proper indexing
- **Response Caching**: HTTP-level caching for static content

**Cache Strategies:**
- **Cache-Aside**: Data is loaded into cache on first access
- **Write-Through**: Cache is updated immediately when data changes
- **Cache Invalidation**: Intelligent invalidation of related cache entries
- **TTL Management**: Configurable time-to-live values for different data types

### 8.3 Query Optimization
Entity Framework Core query optimization ensures efficient data retrieval:

**Query Patterns:**
- **Eager Loading**: Related entities are loaded in single queries to avoid N+1 problems
- **Projection**: Only required fields are selected to minimize data transfer
- **Batch Operations**: Multiple operations are batched for better performance
- **Async Operations**: All database operations are async for better scalability

**Performance Monitoring:**
- **Query Execution Time**: Monitoring of slow queries and performance bottlenecks
- **Cache Hit Rates**: Tracking of cache effectiveness and optimization opportunities
- **Database Connection Usage**: Monitoring of connection pool utilization
- **Response Time Metrics**: End-to-end response time tracking for all endpoints

## 9. Security Features

### 9.1 Input Validation
Comprehensive input validation prevents malicious input and data corruption:

**Validation Layers:**
- **Model Validation**: ASP.NET Core model validation for basic data types
- **Business Rule Validation**: Custom validation for business logic compliance
- **Database Constraints**: Database-level constraints for data integrity
- **Output Encoding**: Proper encoding of output data to prevent XSS attacks

**Security Benefits:**
- **SQL Injection Prevention**: Parameterized queries prevent SQL injection attacks
- **XSS Protection**: Input sanitization and output encoding prevent cross-site scripting
- **Data Corruption Prevention**: Validation ensures data consistency and integrity
- **Malicious Input Blocking**: Malicious input is detected and rejected

### 9.2 Authentication & Authorization
Robust authentication and authorization ensure secure access to portfolio resources:

**Authentication Mechanisms:**
- **OAuth 2.0 Integration**: Secure token-based authentication with external user service
- **Token Validation**: Comprehensive token validation and refresh handling
- **Session Management**: Secure session handling for authenticated users
- **Error Handling**: Secure error responses that don't leak sensitive information

**Authorization Features:**
- **Resource Ownership**: Users can only access and modify their own portfolios
- **Role-Based Access**: Support for different user roles and permissions
- **Audit Logging**: Comprehensive logging of authentication and authorization events
- **Access Control**: Fine-grained access control for different portfolio operations

### 9.3 Data Protection
Data protection measures ensure sensitive information is properly secured:

**Data Security:**
- **Encryption at Rest**: Sensitive data is encrypted in the database
- **Secure Communication**: HTTPS enforcement for all API communications
- **Access Logging**: Comprehensive logging of data access and modifications
- **Data Retention**: Configurable data retention policies for compliance

**Privacy Features:**
- **User Consent**: Clear user consent for data collection and processing
- **Data Minimization**: Only necessary data is collected and stored
- **Right to Deletion**: Support for user data deletion requests
- **Data Portability**: Export functionality for user data portability

## 10. Testing Strategy

### 10.1 Unit Testing
Comprehensive unit testing ensures code quality and reliability:

**Service Layer Testing:**
- **Portfolio Services**: Unit tests for portfolio creation, updates, and deletion
- **Project Services**: Tests for project management operations
- **Template Services**: Validation of template management functionality
- **Authentication Services**: Testing of authentication and authorization logic

**Repository Testing:**
- **Data Access**: Testing of repository methods and data persistence
- **Error Handling**: Validation of error scenarios and edge cases
- **Transaction Management**: Testing of transaction rollback and commit scenarios
- **Performance Testing**: Validation of query performance and optimization

### 10.2 Integration Testing
Integration testing validates system behavior and external service integration:

**Database Integration:**
- **Entity Framework**: Testing of EF Core configuration and relationships
- **Migration Testing**: Validation of database schema changes and migrations
- **Data Consistency**: Testing of cascade delete and relationship integrity
- **Performance Testing**: Validation of database query performance

**External Service Integration:**
- **User Service**: Testing of authentication and user validation flows
- **HTTP Client**: Validation of external service communication
- **Error Handling**: Testing of network failures and service unavailability
- **Performance Testing**: Validation of external service response times

### 10.3 Test Data Management
Comprehensive test data management ensures reliable and repeatable tests:

**Test Data Factories:**
- **Portfolio Data**: Factory methods for creating test portfolios with various configurations
- **Content Data**: Test data for projects, skills, experience, and blog posts
- **Template Data**: Test templates for portfolio creation and management
- **User Data**: Test user accounts with different permission levels

**Test Environment Setup:**
- **In-Memory Database**: Fast test execution with in-memory database
- **Mock Services**: Mocked external services for isolated testing
- **Configuration Management**: Test-specific configuration for different test scenarios
- **Cleanup Procedures**: Automatic cleanup of test data between test runs

## 11. Deployment

### 11.1 Environment Configuration
Environment-specific configuration supports different deployment scenarios:

**Configuration Files:**
- **Development**: Local development settings with debug logging
- **Staging**: Pre-production environment with production-like settings
- **Production**: Production environment with optimized performance settings
- **Docker**: Container-specific configuration for containerized deployments

**Environment Variables:**
- **Database Connection**: Environment-specific database connection strings
- **External Services**: Configuration for external service endpoints
- **Caching**: Redis connection strings and cache configuration
- **Logging**: Log level and output configuration for different environments

### 11.2 Docker Support
Containerized deployment support for modern deployment scenarios:

**Docker Configuration:**
- **Multi-Stage Builds**: Optimized Docker images with minimal runtime footprint
- **Health Checks**: Container health monitoring and automatic restart
- **Environment Variables**: Runtime configuration through environment variables
- **Volume Mounts**: Persistent storage for images and configuration files

**Deployment Benefits:**
- **Consistency**: Consistent deployment across different environments
- **Scalability**: Easy horizontal scaling with container orchestration
- **Portability**: Deployable to any platform supporting Docker
- **Versioning**: Clear version control and rollback capabilities

## 12. API Endpoints Summary

### 12.1 Portfolio Management
Comprehensive portfolio management endpoints:

**Portfolio CRUD:**
- `GET /api/portfolio` - Retrieve all portfolios
- `GET /api/portfolio/{id}` - Get specific portfolio by ID
- `GET /api/portfolio/user/{userId}` - Get portfolios by user ID
- `POST /api/portfolio` - Create new portfolio
- `PUT /api/portfolio/{id}` - Update existing portfolio
- `DELETE /api/portfolio/{id}` - Delete portfolio

**Content Management:**
- `POST /api/portfolio/{id}/content` - Add portfolio content
- `PUT /api/portfolio/{id}/content` - Update portfolio content
- `POST /api/portfolio/{id}/publish` - Publish portfolio
- `POST /api/portfolio/{id}/unpublish` - Unpublish portfolio

**Analytics:**
- `POST /api/portfolio/{id}/view` - Increment view count
- `POST /api/portfolio/{id}/like` - Increment like count

### 12.2 Project Management
Project creation and management endpoints:

**Project Operations:**
- `GET /api/project/portfolio/{portfolioId}` - Get projects by portfolio
- `GET /api/project/{id}` - Get specific project
- `POST /api/project` - Create new project
- `PUT /api/project/{id}` - Update project
- `DELETE /api/project/{id}` - Delete project

**Project Features:**
- `POST /api/project/{id}/feature` - Toggle featured status
- `GET /api/project/featured/{portfolioId}` - Get featured projects

### 12.3 Content Management
Content management for portfolio components:

**Experience Management:**
- `GET /api/experience/portfolio/{portfolioId}` - Get experience by portfolio
- `POST /api/experience` - Create experience record
- `PUT /api/experience/{id}` - Update experience
- `DELETE /api/experience/{id}` - Delete experience

**Skills Management:**
- `GET /api/skill/portfolio/{portfolioId}` - Get skills by portfolio
- `POST /api/skill` - Create skill record
- `PUT /api/skill/{id}` - Update skill
- `DELETE /api/skill/{id}` - Delete skill

**Blog Post Management:**
- `GET /api/blogpost/portfolio/{portfolioId}` - Get blog posts by portfolio
- `POST /api/blogpost` - Create blog post
- `PUT /api/blogpost/{id}` - Update blog post
- `DELETE /api/blogpost/{id}` - Delete blog post

### 12.4 Image Management
Image upload and management endpoints:

**Image Operations:**
- `GET /api/image/{imageName}` - Serve image files
- `DELETE /api/image/{imageName}` - Delete image files
- `POST /api/imageupload` - Upload new images

**Image Features:**
- **File Validation**: Type, size, and content validation
- **Processing**: Automatic resizing and optimization
- **Storage**: Organized file storage with metadata
- **Cleanup**: Automatic cleanup of orphaned images

## 13. Future Enhancements

### 13.1 Planned Features
Roadmap for upcoming portfolio service features:

**Advanced Portfolio Features:**
- **Custom Domains**: Support for custom domain names for portfolios
- **Analytics Dashboard**: Comprehensive analytics and insights
- **SEO Optimization**: Built-in SEO tools and meta tag management
- **Social Media Integration**: Automatic social media sharing and previews

**Content Management:**
- **Rich Text Editor**: Advanced content editing with rich text support
- **Media Library**: Comprehensive media management and organization
- **Content Scheduling**: Scheduled publication and content management
- **Version Control**: Content versioning and rollback capabilities

### 13.2 Scalability Improvements
Technical improvements for enhanced scalability:

**Performance Enhancements:**
- **CDN Integration**: Content delivery network for global performance
- **Database Sharding**: Horizontal database scaling for large datasets
- **Microservice Architecture**: Further service decomposition for scalability
- **Event-Driven Architecture**: Asynchronous processing for high-throughput scenarios

**Infrastructure Improvements:**
- **Kubernetes Deployment**: Container orchestration for better resource management
- **Auto-scaling**: Automatic scaling based on load and demand
- **Multi-region Deployment**: Geographic distribution for global users
- **Monitoring and Alerting**: Advanced monitoring and automated alerting systems

## 14. Contributing

### 14.1 Development Guidelines
Guidelines for contributing to the portfolio service:

**Code Standards:**
- **C# Conventions**: Follow Microsoft C# coding conventions
- **Naming Conventions**: Consistent naming for classes, methods, and variables
- **Documentation**: Comprehensive XML documentation for public APIs
- **Error Handling**: Consistent error handling patterns across the codebase

**Testing Requirements:**
- **Unit Test Coverage**: Minimum 80% code coverage for new features
- **Integration Testing**: Integration tests for external service dependencies
- **Performance Testing**: Performance validation for new features
- **Security Testing**: Security validation for authentication and authorization

### 14.2 Code Review Checklist
Comprehensive code review process:

**Code Quality:**
- **Readability**: Code is clear and easy to understand
- **Maintainability**: Code follows established patterns and conventions
- **Performance**: No performance regressions or bottlenecks
- **Security**: Proper validation and security measures implemented

**Testing and Documentation:**
- **Test Coverage**: Adequate test coverage for new functionality
- **Documentation**: Updated documentation for new features
- **API Changes**: Proper versioning and backward compatibility
- **Migration Scripts**: Database migrations for schema changes

## 15. Support

For technical support and questions about the Backend Portfolio Service:

**Documentation Resources:**
- **API Documentation**: Swagger/OpenAPI documentation at `/swagger`
- **Code Examples**: Sample implementations and usage patterns
- **Troubleshooting Guide**: Common issues and solutions
- **Performance Tuning**: Optimization guidelines and best practices

**Contact Information:**
- **Development Team**: Internal development team for technical questions
- **Issue Tracking**: GitHub issues for bug reports and feature requests
- **Code Review**: Pull request reviews and code quality feedback
- **Architecture Decisions**: Technical architecture and design decisions

---

*This documentation provides a comprehensive overview of the Backend Portfolio Service architecture, implementation patterns, and usage guidelines. For specific implementation details, refer to the source code and API documentation.*
