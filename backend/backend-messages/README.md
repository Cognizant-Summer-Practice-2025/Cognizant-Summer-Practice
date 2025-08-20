# Backend Messages Service

## Table of Contents

### [1. Overview](#1-overview)
### [2. Architecture](#2-architecture)
- [2.1 Technology Stack](#21-technology-stack)
- [2.2 Project Structure](#22-project-structure)

### [3. Core Components](#3-core-components)
- [3.1 Data Models](#31-data-models)
  - [3.1.1 Message Entity](#311-message-entity)
  - [3.1.2 Conversation Entity](#312-conversation-entity)
  - [3.1.3 MessageReport Entity](#313-messagereport-entity)
  - [3.1.4 SearchUser Entity](#314-searchuser-entity)
- [3.2 Database Layer](#32-database-layer)
  - [3.2.1 MessagesDbContext](#321-messagesdbcontext)
  - [3.2.2 Repository Pattern](#322-repository-pattern)
- [3.3 Service Layer](#33-service-layer)
  - [3.3.1 Message Service](#331-message-service)
  - [3.3.2 Conversation Service](#332-conversation-service)
  - [3.3.3 User Authentication Service](#333-user-authentication-service)
  - [3.3.4 User Search Service](#334-user-search-service)
- [3.4 API Controllers](#34-api-controllers)
  - [3.4.1 MessagesController](#341-messagescontroller)
  - [3.4.2 ConversationsController](#342-conversationscontroller)
  - [3.4.3 UsersController](#343-userscontroller)
- [3.5 SignalR Hubs](#35-signalr-hubs)
  - [3.5.1 MessageHub](#351-messagehub)

### [4. Data Flow Architecture](#4-data-flow-architecture)
- [4.1 Message Sending Flow](#41-message-sending-flow)
- [4.2 Real-Time Communication Flow](#42-real-time-communication-flow)
- [4.3 Conversation Management Flow](#43-conversation-management-flow)
- [4.4 Message Reporting Flow](#44-message-reporting-flow)

### [5. Configuration](#5-configuration)
- [5.1 Database Configuration](#51-database-configuration)
- [5.2 Service Registration](#52-service-registration)
- [5.3 CORS Configuration](#53-cors-configuration)
- [5.4 HTTP Client Configuration](#54-http-client-configuration)
- [5.5 SignalR Configuration](#55-signalr-configuration)

### [6. Implementation Patterns](#6-implementation-patterns)
- [6.1 Real-Time Communication Pattern](#61-real-time-communication-pattern)
- [6.2 Repository Pattern](#62-repository-pattern)
- [6.3 Validation Framework](#63-validation-framework)
- [6.4 Mapping System](#64-mapping-system)
- [6.5 Soft Delete Pattern](#65-soft-delete-pattern)

### [7. External Service Integration](#7-external-service-integration)
- [7.1 User Service Integration](#71-user-service-integration)
- [7.2 Authentication Strategy](#72-authentication-strategy)
- [7.3 HTTP Client Management](#73-http-client-management)

### [8. Performance Optimizations](#8-performance-optimizations)
- [8.1 Database Indexing](#81-database-indexing)
- [8.2 Connection Management](#82-connection-management)
- [8.3 Query Optimization](#83-query-optimization)

### [9. Security Features](#9-security-features)
- [9.1 Input Validation](#91-input-validation)
- [9.2 Authentication & Authorization](#92-authentication--authorization)
- [9.3 Data Protection](#93-data-protection)
- [9.4 Message Reporting](#94-message-reporting)

### [10. Testing Strategy](#10-testing-strategy)
- [10.1 Unit Testing](#101-unit-testing)
- [10.2 Integration Testing](#102-integration-testing)
- [10.3 SignalR Testing](#103-signalr-testing)

### [11. Deployment](#11-deployment)
- [11.1 Environment Configuration](#111-environment-configuration)
- [11.2 SignalR Deployment](#112-signalr-deployment)

### [12. API Endpoints Summary](#12-api-endpoints-summary)
- [12.1 Message Management](#121-message-management)
- [12.2 Conversation Management](#122-conversation-management)
- [12.3 User Management](#123-user-management)

### [13. SignalR Events](#13-signalr-events)
- [13.1 Client Events](#131-client-events)
- [13.2 Server Events](#132-server-events)

### [14. Future Enhancements](#14-future-enhancements)
- [14.1 Planned Features](#141-planned-features)
- [14.2 Scalability Improvements](#142-scalability-improvements)

### [15. Contributing](#15-contributing)
- [15.1 Development Guidelines](#151-development-guidelines)
- [15.2 Code Review Checklist](#152-code-review-checklist)

### [16. Support](#16-support)

---

## 1. Overview

The Backend Messages Service is a comprehensive .NET 9.0 microservice responsible for managing real-time messaging, conversations, and communication between users. It provides a robust foundation for instant messaging functionality in a professional platform, implementing SignalR for real-time communication and comprehensive message management capabilities.

**Key Features:**
- **Real-Time Messaging**: Instant message delivery using SignalR
- **Conversation Management**: Create, manage, and organize user conversations
- **Message Types**: Support for text, image, file, audio, video, and system messages
- **Message Threading**: Reply-to functionality for threaded conversations
- **Read Receipts**: Real-time read status tracking and notifications
- **Message Reporting**: Content moderation and reporting system
- **Soft Delete**: Message deletion with data preservation
- **User Search**: Integration with external user service for contact discovery

## 2. Architecture

### 2.1 Technology Stack
- **Framework**: .NET 9.0
- **Database**: PostgreSQL with Entity Framework Core
- **Real-Time Communication**: SignalR with WebSocket fallback
- **Authentication**: OAuth 2.0 integration with external user service
- **API Documentation**: Swagger/OpenAPI
- **Validation**: Custom validation framework with comprehensive input validation
- **Logging**: Built-in .NET logging with ILogger
- **HTTP Client**: HttpClient with configuration and retry policies

### 2.2 Project Structure
```
backend-messages/
├── Config/                          # Configuration classes
│   ├── ApiDocumentationConfiguration.cs
│   ├── CorsConfiguration.cs
│   ├── DatabaseConfiguration.cs
│   ├── HttpClientConfiguration.cs
│   ├── JsonConfiguration.cs
│   ├── MiddlewareConfiguration.cs
│   ├── ServiceRegistrationConfiguration.cs
│   └── SignalRConfiguration.cs
├── Controllers/                     # API endpoints
│   ├── MessagesController.cs
│   ├── ConversationsController.cs
│   └── UsersController.cs
├── Data/                           # Database context and configuration
│   └── MessagesDbContext.cs
├── DTO/                            # Data Transfer Objects
│   ├── Message/
│   │   ├── Request/
│   │   └── Response/
│   ├── Conversation/
│   ├── User/
│   └── Common/
├── Hubs/                           # SignalR real-time communication
│   └── MessageHub.cs
├── Middleware/                      # Custom middleware
├── Models/                          # Entity models
│   ├── Messages.cs
│   ├── Conversation.cs
│   ├── MessageReport.cs
│   └── SearchUser.cs
├── Repositories/                    # Data access layer
│   ├── IMessageRepository.cs
│   ├── MessageRepository.cs
│   ├── IConversationRepository.cs
│   └── ConversationRepository.cs
├── Services/                        # Business logic layer
│   ├── Abstractions/               # Service interfaces
│   ├── Mappers/                    # Entity-DTO mappers
│   ├── Validators/                 # Validation logic
│   ├── MessageService.cs
│   ├── ConversationServiceRefactored.cs
│   ├── UserAuthenticationService.cs
│   └── UserSearchService.cs
└── Tests/                          # Unit tests
```

## 3. Core Components

### 3.1 Data Models

#### 3.1.1 Message Entity
The central entity representing individual messages in conversations with comprehensive metadata:

**Identity Properties:**
- `Id` (Guid): Primary key, auto-generated using database identity
- `ConversationId` (Guid): Foreign key to Conversation entity (required)
- `SenderId` (Guid): User ID who sent the message (required)
- `ReceiverId` (Guid): User ID who receives the message (required)

**Content Properties:**
- `Content` (string?): Message content text (nullable for non-text messages)
- `MessageType` (MessageType enum): Type of message (Text, Image, File, Audio, Video, System)
- `ReplyToMessageId` (Guid?): Reference to replied message for threading (nullable)

**Status Properties:**
- `IsRead` (bool): Read status flag, defaults to false
- `CreatedAt` (DateTime): Message creation timestamp, auto-set to UTC
- `UpdatedAt` (DateTime): Last modification timestamp, auto-updated
- `DeletedAt` (DateTime?): Soft delete timestamp (nullable)

**Navigation Properties (EF Core Relationships):**
- `Conversation`: Many-to-one relationship with Conversation entity
- `ReplyToMessage`: Self-referencing relationship for message threading
- Cascade delete when conversation is removed

**Message Type Support:**
- **Text**: Standard text messages with content
- **Image**: Image messages with file references
- **File**: Document and file sharing
- **Audio**: Voice and audio messages
- **Video**: Video messages and clips
- **System**: Automated system notifications

#### 3.1.2 Conversation Entity
Manages user conversations and chat sessions with metadata tracking:

**Identity Properties:**
- `Id` (Guid): Primary key for the conversation
- `InitiatorId` (Guid): User who started the conversation (required)
- `ReceiverId` (Guid): User who was invited to the conversation (required)

**Metadata Properties:**
- `LastMessageTimestamp` (DateTime): Timestamp of the most recent message
- `LastMessageId` (Guid?): Reference to the last message in the conversation
- `CreatedAt` (DateTime): Conversation creation timestamp
- `UpdatedAt` (DateTime): Last modification timestamp

**Soft Delete Properties:**
- `InitiatorDeletedAt` (DateTime?): Soft delete timestamp for initiator
- `ReceiverDeletedAt` (DateTime?): Soft delete timestamp for receiver

**Navigation Properties:**
- `LastMessage`: One-to-one relationship with Message entity
- `Messages`: One-to-many relationship with Message entities
- Cascade delete for messages when conversation is removed

**Helper Methods:**
- `IsDeletedByUser(Guid userId)`: Checks if conversation is deleted by specific user
- `IsDeletedByBothUsers()`: Checks if conversation is deleted by both participants

#### 3.1.3 MessageReport Entity
Manages content moderation and message reporting system:

**Identity Properties:**
- `Id` (Guid): Primary key for the report
- `MessageId` (Guid): Foreign key to reported Message (required)
- `ReportedByUserId` (Guid): User who submitted the report (required)

**Report Details:**
- `Reason` (string): Detailed reason for the report (required)
- `CreatedAt` (DateTime): Report submission timestamp

**Navigation Properties:**
- `Message`: Many-to-one relationship with Message entity
- Cascade delete when message is removed

**Database Constraints:**
- Unique constraint on (MessageId, ReportedByUserId) combination
- Prevents duplicate reports from the same user for the same message

#### 3.1.4 SearchUser Entity
Lightweight user representation for search and contact discovery:

**Core Properties:**
- `Id` (Guid): User identifier
- `Username` (string): Unique username for the user
- `FirstName` (string?): User's first name (nullable)
- `LastName` (string?): User's last name (nullable)
- `FullName` (string): Computed full name for display
- `ProfessionalTitle` (string?): Professional title or role (nullable)
- `AvatarUrl` (string?): Profile picture URL (nullable)
- `IsActive` (bool): User account activity status

**Usage:**
- Contact discovery and user search functionality
- Integration with external user service
- Lightweight user information for messaging interface

### 3.2 Database Layer

#### 3.2.1 MessagesDbContext
Entity Framework Core DbContext with comprehensive configuration for messaging system:

**Entity Configuration:**
- **Snake Case Naming**: Uses manual column mapping for PostgreSQL snake_case convention
- **Timestamp Handling**: Custom timestamp configuration for UTC time handling
- **Relationship Configuration**: Proper foreign key relationships with cascade deletes
- **Index Strategy**: Strategic indexes for performance optimization

**Database Indexes:**
```csharp
// Message entity indexes
entity.HasIndex(e => e.ConversationId);               // Conversation lookup optimization
entity.HasIndex(e => e.SenderId);                     // Sender lookup optimization
entity.HasIndex(e => e.ReceiverId);                   // Receiver lookup optimization
entity.HasIndex(e => e.CreatedAt);                    // Time-based queries optimization
entity.HasIndex(e => new { e.ConversationId, e.CreatedAt }); // Conversation timeline optimization

// Conversation entity indexes
entity.HasIndex(e => e.InitiatorId);                  // Initiator lookup optimization
entity.HasIndex(e => e.ReceiverId);                   // Receiver lookup optimization
entity.HasIndex(e => e.LastMessageTimestamp);         // Recent conversations optimization

// MessageReport entity indexes
entity.HasIndex(e => new { e.MessageId, e.ReportedByUserId }).IsUnique(); // Report uniqueness
```

**Timestamp Configuration:**
- **UTC Handling**: All timestamps are stored as UTC without timezone information
- **Automatic Updates**: UpdatedAt fields are automatically managed
- **Soft Delete Support**: DeletedAt fields for data preservation

**Cascade Delete Strategy:**
- Conversation deletion triggers cascade delete of all related messages
- Message deletion triggers cascade delete of related reports
- Ensures data consistency and prevents orphaned records

#### 3.2.2 Repository Pattern
Data access layer implementing the Repository pattern for clean separation of concerns:

**IMessageRepository:**
```csharp
public interface IMessageRepository
{
    Task<Message> CreateAsync(Message message);
    Task<Message?> GetByIdAsync(Guid id);
    Task<IEnumerable<Message>> GetByConversationIdAsync(Guid conversationId, int skip, int take);
    Task<bool> UpdateAsync(Message message);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> MarkAsReadAsync(Guid id);
    Task<bool> UserCanAccessMessageAsync(Guid messageId, Guid userId);
    Task<int> GetUnreadCountAsync(Guid conversationId, Guid userId);
}
```

**IConversationRepository:**
```csharp
public interface IConversationRepository
{
    Task<Conversation> CreateAsync(Conversation conversation);
    Task<Conversation?> GetByIdAsync(Guid id);
    Task<IEnumerable<Conversation>> GetByUserIdAsync(Guid userId, int skip, int take);
    Task<bool> UpdateAsync(Conversation conversation);
    Task<bool> UpdateLastMessageAsync(Guid conversationId, Guid messageId);
    Task<bool> UserCanAccessConversationAsync(Guid conversationId, Guid userId);
    Task<bool> SoftDeleteForUserAsync(Guid conversationId, Guid userId);
}
```

**Repository Implementation Benefits:**
- **Testability**: Easy to mock for unit testing scenarios
- **Dependency Injection**: Scoped services for proper lifecycle management
- **Interface Segregation**: Each repository handles specific entity types
- **Data Access Abstraction**: Business logic doesn't depend on EF Core directly

### 3.3 Service Layer

#### 3.3.1 Message Service
Core business logic for message operations and real-time communication:

**Message Operations:**
```csharp
public class MessageService : IMessageService
{
    // Core messaging operations
    public async Task<SendMessageResponse> SendMessageAsync(SendMessageRequest request)
    public async Task<MessageResponse?> GetMessageByIdAsync(Guid messageId, Guid userId)
    public async Task<IEnumerable<MessageResponse>> GetMessagesByConversationAsync(Guid conversationId, Guid userId, int skip, int take)
    public async Task<bool> MarkMessageAsReadAsync(Guid messageId, Guid userId)
    public async Task<bool> MarkMessagesAsReadAsync(Guid conversationId, Guid userId)
    public async Task<bool> DeleteMessageAsync(Guid messageId, Guid userId)
    public async Task<bool> ReportMessageAsync(ReportMessageRequest request)
}
```

**Business Logic Features:**
- **Access Control**: Verifies user permissions for message operations
- **Validation**: Comprehensive input validation using custom validators
- **Real-Time Notifications**: SignalR integration for instant message delivery
- **Conversation Updates**: Automatic conversation metadata updates
- **Error Handling**: Comprehensive error handling with logging

**Validation Framework:**
- **SendMessageValidator**: Validates message content and permissions
- **MarkMessageAsReadValidator**: Ensures read operations are valid
- **DeleteMessageValidator**: Verifies deletion permissions
- **ReportMessageValidator**: Validates report submissions

#### 3.3.2 Conversation Service
Manages conversation lifecycle and user interactions:

**Conversation Operations:**
```csharp
public class ConversationServiceRefactored : IConversationService
{
    // Conversation management operations
    public async Task<ConversationResponse> CreateConversationAsync(CreateConversationRequest request)
    public async Task<ConversationResponse?> GetConversationByIdAsync(Guid id, Guid userId)
    public async Task<IEnumerable<ConversationResponse>> GetUserConversationsAsync(Guid userId, int skip, int take)
    public async Task<bool> DeleteConversationAsync(Guid id, Guid userId)
    public async Task<bool> UpdateConversationAsync(Guid id, UpdateConversationRequest request)
}
```

**Conversation Features:**
- **User Access Control**: Ensures users can only access their conversations
- **Soft Delete Support**: Individual user deletion without affecting other participants
- **Last Message Tracking**: Automatic updates for conversation metadata
- **Pagination Support**: Efficient conversation listing with skip/take parameters

#### 3.3.3 User Authentication Service
External service integration for user authentication and validation:

**Authentication Operations:**
```csharp
public class UserAuthenticationService : IUserAuthenticationService
{
    // User authentication operations
    public async Task<UserInfo?> GetUserInfoAsync(string accessToken)
    public async Task<bool> ValidateUserAccessAsync(Guid userId, string accessToken)
    public async Task<bool> IsUserAuthorizedAsync(Guid userId, string accessToken)
}
```

**Integration Features:**
- **External User Service**: HTTP client communication with user service
- **Token Validation**: OAuth 2.0 token validation and user verification
- **Access Control**: User permission validation for messaging operations
- **Error Handling**: Comprehensive error handling for external service failures

#### 3.3.4 User Search Service
Contact discovery and user search functionality:

**Search Operations:**
```csharp
public class UserSearchService : IUserSearchService
{
    // User search operations
    public async Task<IEnumerable<SearchUser>> SearchUsersAsync(string searchTerm, Guid currentUserId)
    public async Task<SearchUser?> GetUserByIdAsync(Guid userId)
    public async Task<IEnumerable<SearchUser>> GetUsersByIdsAsync(IEnumerable<Guid> userIds)
}
```

**Search Features:**
- **External Integration**: Searches external user service for contact discovery
- **Current User Filtering**: Excludes current user from search results
- **Batch Operations**: Efficient retrieval of multiple users by ID
- **Error Handling**: Graceful handling of external service failures

### 3.4 API Controllers

#### 3.4.1 MessagesController
Comprehensive message management endpoints with real-time integration:

**Message CRUD Operations:**
```csharp
[HttpPost("send")]                   // POST /api/messages/send
public async Task<IActionResult> SendMessage(SendMessageRequest request)

[HttpGet("{id}")]                     // GET /api/messages/{id}
public async Task<IActionResult> GetMessage(Guid id)

[HttpGet("conversation/{conversationId}")] // GET /api/messages/conversation/{conversationId}
public async Task<IActionResult> GetMessagesByConversation(Guid conversationId, int skip = 0, int take = 50)

[HttpPut("{id}/read")]               // PUT /api/messages/{id}/read
public async Task<IActionResult> MarkMessageAsRead(Guid id)

[HttpPut("conversation/{conversationId}/read")] // PUT /api/messages/conversation/{conversationId}/read
public async Task<IActionResult> MarkMessagesAsRead(Guid conversationId)

[HttpDelete("{id}")]                  // DELETE /api/messages/{id}
public async Task<IActionResult> DeleteMessage(Guid id)
```

**Message Management Endpoints:**
```csharp
[HttpPost("report")]                 // POST /api/messages/report
public async Task<IActionResult> ReportMessage(ReportMessageRequest request)

[HttpGet("unread/count")]             // GET /api/messages/unread/count
public async Task<IActionResult> GetUnreadCount(Guid conversationId)
```

**Controller Implementation Details:**
- **Authentication**: JWT token validation for all endpoints
- **Authorization**: User ownership verification for message operations
- **Validation**: Comprehensive input validation before processing
- **Error Handling**: Proper HTTP status codes and error messages
- **Real-Time Integration**: SignalR hub context for notifications

#### 3.4.2 ConversationsController
Conversation management and user interaction endpoints:

**Conversation CRUD Operations:**
```csharp
[HttpPost]                           // POST /api/conversations
public async Task<IActionResult> CreateConversation(CreateConversationRequest request)

[HttpGet("{id}")]                     // GET /api/conversations/{id}
public async Task<IActionResult> GetConversation(Guid id)

[HttpGet]                             // GET /api/conversations
public async Task<IActionResult> GetUserConversations(int skip = 0, int take = 20)

[HttpPut("{id}")]                     // PUT /api/conversations/{id}
public async Task<IActionResult> UpdateConversation(Guid id, UpdateConversationRequest request)

[HttpDelete("{id}")]                  // DELETE /api/conversations/{id}
public async Task<IActionResult> DeleteConversation(Guid id)
```

**Conversation Features:**
- **User Access Control**: Ensures users can only access their conversations
- **Soft Delete Support**: Individual user deletion without affecting other participants
- **Pagination**: Efficient conversation listing with skip/take parameters
- **Real-Time Updates**: SignalR integration for conversation changes

#### 3.4.3 UsersController
User search and contact discovery endpoints:

**User Operations:**
```csharp
[HttpGet("search")]                   // GET /api/users/search?q={searchTerm}
public async Task<IActionResult> SearchUsers(string q)

[HttpGet("{id}")]                     // GET /api/users/{id}
public async Task<IActionResult> GetUser(Guid id)
```

**User Features:**
- **External Integration**: Searches external user service for contacts
- **Current User Filtering**: Excludes current user from search results
- **Error Handling**: Graceful handling of external service failures
- **Caching**: Optional caching for frequently accessed user information

### 3.5 SignalR Hubs

#### 3.5.1 MessageHub
Real-time communication hub for instant messaging functionality:

**Connection Management:**
```csharp
public class MessageHub : Hub
{
    // Connection tracking
    private static readonly ConcurrentDictionary<string, HashSet<string>> UserConnections = new();
    
    // Connection management methods
    public override async Task OnConnectedAsync()
    public override async Task OnDisconnectedAsync(Exception? exception)
    public async Task JoinUserGroup(string userId)
    public async Task LeaveUserGroup(string userId)
}
```

**Real-Time Operations:**
```csharp
// Message operations
public async Task DeleteMessage(string messageId, string userId)
public async Task MarkMessageAsRead(string messageId, string userId)
public async Task MarkMessagesAsRead(string conversationId, string userId)
public async Task SendTypingIndicator(string conversationId, string userId, bool isTyping)
```

**Client Events:**
- **MessageDeleted**: Notifies clients when messages are deleted
- **MessageRead**: Sends read receipts to message senders
- **TypingIndicator**: Real-time typing status updates
- **UserConnected**: Notifies when users come online
- **UserDisconnected**: Notifies when users go offline

**Server Events:**
- **JoinUserGroup**: Adds users to their personal SignalR groups
- **LeaveUserGroup**: Removes users from their groups on disconnect
- **BroadcastToUser**: Sends messages to specific user groups
- **BroadcastToConversation**: Sends messages to conversation participants

**Connection Benefits:**
- **Multi-Device Support**: Users can be connected from multiple devices
- **Real-Time Delivery**: Instant message delivery and notifications
- **Group Management**: Efficient message broadcasting to conversation participants
- **Connection Tracking**: Comprehensive user connection state management

## 4. Data Flow Architecture

### 4.1 Message Sending Flow
The message sending process follows a comprehensive workflow that ensures real-time delivery and data integrity:

**Initial Request Processing:**
1. **Client Request**: Frontend sends `SendMessageRequest` with conversation ID, content, and message type
2. **Authentication Validation**: JWT token validation extracts user information and verifies sender identity
3. **Input Validation**: Request undergoes comprehensive validation including content, permissions, and business rules
4. **Conversation Verification**: System validates that the conversation exists and user has access
5. **Reply Message Validation**: If replying to a message, system verifies reply message accessibility

**Business Logic Execution:**
1. **Message Creation**: MessageService creates the message entity with proper metadata
2. **Permission Check**: System verifies user can access the conversation and send messages
3. **Database Persistence**: MessageRepository saves the message to the database
4. **Conversation Update**: Conversation's last message and timestamp are automatically updated
5. **Real-Time Notification**: SignalR hub broadcasts the message to all conversation participants

**Response Generation:**
1. **Entity Mapping**: Message entity is mapped to SendMessageResponse DTO
2. **Success Response**: HTTP 201 Created response with message details
3. **Real-Time Broadcast**: SignalR events notify all connected clients instantly
4. **Error Handling**: Comprehensive error responses for validation failures or access violations

### 4.2 Real-Time Communication Flow
The real-time communication process implements instant message delivery using SignalR:

**Connection Management:**
1. **Client Connection**: WebSocket connection established with SignalR hub
2. **User Authentication**: JWT token validation for secure connection establishment
3. **Group Assignment**: User is added to personal SignalR group for targeted messaging
4. **Connection Tracking**: Hub maintains user connection state for multi-device support

**Message Broadcasting:**
1. **Message Reception**: Hub receives messages from MessageService
2. **Recipient Resolution**: System determines all conversation participants
3. **Group Broadcasting**: Message is sent to all relevant user groups
4. **Delivery Confirmation**: Clients receive instant message delivery notifications

**Real-Time Features:**
- **Typing Indicators**: Real-time typing status updates between users
- **Read Receipts**: Instant notification when messages are read
- **Online Status**: User connection state tracking and notifications
- **Message Updates**: Real-time message modification and deletion notifications

### 4.3 Conversation Management Flow
The conversation management process handles chat session lifecycle and user interactions:

**Conversation Creation:**
1. **User Request**: Frontend sends conversation creation request with participant IDs
2. **Permission Validation**: System verifies users exist and can participate in conversations
3. **Duplicate Check**: Prevents creation of duplicate conversations between the same users
4. **Entity Creation**: Conversation entity is created with proper metadata
5. **Database Persistence**: ConversationRepository saves the conversation

**Conversation Access:**
1. **User Verification**: System checks if user is part of the conversation
2. **Soft Delete Check**: Filters out conversations deleted by the requesting user
3. **Message Aggregation**: Retrieves conversation messages with pagination
4. **Metadata Updates**: Last message and timestamp information is included

**Conversation Deletion:**
1. **Soft Delete Implementation**: Individual user deletion without affecting other participants
2. **Data Preservation**: Messages and conversation data are preserved for other users
3. **Cleanup Operations**: Automatic cleanup when both users delete the conversation
4. **Real-Time Updates**: SignalR notifications for conversation state changes

### 4.4 Message Reporting Flow
The message reporting process handles content moderation and user safety:

**Report Submission:**
1. **User Report**: Frontend sends message report with reason and message ID
2. **Permission Validation**: System verifies user can access the reported message
3. **Duplicate Prevention**: Unique constraint prevents multiple reports from the same user
4. **Report Creation**: MessageReport entity is created and stored in database

**Content Moderation:**
1. **Report Aggregation**: System tracks multiple reports for the same message
2. **Moderator Review**: Reports are available for content moderation review
3. **Action Tracking**: Moderation actions are logged and tracked
4. **User Notification**: Users are notified of moderation decisions

## 5. Configuration

### 5.1 Database Configuration
The DatabaseConfiguration class implements comprehensive database setup optimized for messaging systems:

**Entity Framework Configuration:**
The AddDatabaseServices method configures Entity Framework Core with the Npgsql provider for PostgreSQL, enabling advanced PostgreSQL features for messaging applications. The configuration includes custom timestamp handling for UTC time management, snake_case naming conventions for database columns, and strategic indexing for conversation and message queries.

**Database Connection Features:**
- **PostgreSQL Integration**: Full support for PostgreSQL-specific features and data types
- **Connection Pooling**: Efficient connection management for high-concurrency messaging scenarios
- **Command Timeout**: Configurable timeout values for long-running queries
- **Retry Policies**: Automatic retry logic for transient database failures

**Messaging-Specific Optimizations:**
- **Conversation Indexes**: Optimized indexes for user conversation lookups
- **Message Timeline Indexes**: Efficient retrieval of conversation message history
- **User Access Indexes**: Fast permission checking for message and conversation access
- **Timestamp Optimization**: UTC timestamp handling for global messaging applications

### 5.2 Service Registration
The ServiceRegistrationConfiguration class implements organized dependency injection for messaging services:

**Service Registration Methods:**
The AddApplicationServices method registers all business logic services including MessageService, ConversationService, UserAuthenticationService, and UserSearchService. The AddRepositoryServices method registers data access layer components, while AddHttpClientConfiguration sets up external service communication for user service integration.

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
The CorsConfiguration class implements secure Cross-Origin Resource Sharing for messaging applications:

**CORS Policy Configuration:**
The AddCors method configures CORS policies with the "AllowFrontend" policy that specifies allowed origins, HTTP methods, headers, and credential support. The policy supports multiple frontend applications during development and enables secure communication between backend and frontend services.

**Security Features:**
- **Origin Restriction**: Only configured frontend URLs are allowed to access the messaging API
- **Method Control**: Specific HTTP methods are allowed based on messaging requirements
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
- **User Search Integration**: HTTP client for external user service search functionality
- **Monitoring**: Request/response logging and performance metrics collection

### 5.5 SignalR Configuration
The SignalRConfiguration class implements real-time communication setup for instant messaging:

**SignalR Features:**
The AddSignalRConfiguration method configures SignalR with WebSocket transport and fallback options. The configuration includes connection management, group management, and real-time event handling for messaging applications.

**Real-Time Communication Benefits:**
- **WebSocket Support**: Primary transport for low-latency communication
- **Fallback Transports**: Automatic fallback to Server-Sent Events or Long Polling
- **Connection Management**: Efficient user connection tracking and group management
- **Scalability**: Support for horizontal scaling with Redis backplane

## 6. Implementation Patterns

### 6.1 Real-Time Communication Pattern
The real-time communication pattern implements instant messaging using SignalR:

**SignalR Hub Implementation:**
The MessageHub class implements comprehensive real-time communication functionality including connection management, user group management, and message broadcasting. The hub maintains user connection state for multi-device support and efficient message delivery.

**Connection Management Benefits:**
- **Multi-Device Support**: Users can be connected from multiple devices simultaneously
- **Group Management**: Efficient message broadcasting to conversation participants
- **Connection Tracking**: Comprehensive user connection state management
- **Real-Time Delivery**: Instant message delivery and notifications

**Real-Time Features:**
- **Message Broadcasting**: Instant delivery to all conversation participants
- **Typing Indicators**: Real-time typing status updates
- **Read Receipts**: Instant notification when messages are read
- **Online Status**: User connection state tracking and notifications

### 6.2 Repository Pattern
The repository pattern provides clean abstraction over the data access layer:

**Repository Interface Design:**
Each entity type has a dedicated repository interface that defines the contract for data access operations. The interfaces follow consistent naming conventions and provide methods for CRUD operations, specialized queries, and business logic operations.

**Repository Implementation Benefits:**
- **Data Access Abstraction**: Business logic doesn't depend on Entity Framework Core directly
- **Testability**: Repositories can be easily mocked for unit testing scenarios
- **Interface Segregation**: Each repository handles specific entity types with focused responsibilities
- **Dependency Injection**: Repositories are registered as scoped services for proper lifecycle management

**Repository Method Patterns:**
- **Async Operations**: All repository methods are async for better performance
- **Nullable Returns**: Get methods return nullable types to handle missing data gracefully
- **Business Logic Methods**: Specialized methods for messaging-specific operations
- **Permission Checking**: Methods for verifying user access to messages and conversations

### 6.3 Validation Framework
The validation framework ensures data integrity and business rule compliance:

**Input Validation:**
The validation framework implements comprehensive input validation for all incoming requests. It checks required fields, data types, string lengths, and business rule compliance. The framework provides detailed error messages and supports both client-side and server-side validation scenarios.

**Business Rule Validation:**
The system enforces business rules such as message ownership, conversation access permissions, and reply message validation. Business rule validation occurs at the service layer to ensure data consistency and prevent invalid operations.

**Validation Benefits:**
- **Data Integrity**: Ensures only valid data reaches the database
- **User Experience**: Provides clear error messages for validation failures
- **Security**: Prevents malicious input and data corruption
- **Maintainability**: Centralized validation logic for consistent behavior

### 6.4 Mapping System
The mapping system handles transformation between entities and DTOs:

**Entity to Response DTO Mapping:**
The MessageMapper class implements comprehensive mapping from Message entities to various response DTOs. It handles complex transformations including nested object mapping, array processing, and conditional field inclusion. The mapper ensures that sensitive data is excluded from public responses and that data is properly formatted for client consumption.

**Request DTO to Entity Mapping:**
The mapping system converts incoming request DTOs to entity objects for database operations. It handles validation of required fields, type conversions, and default value assignment. The mapper ensures that only valid data reaches the database layer and provides clear error messages for invalid input.

**Mapping Benefits:**
- **Data Transformation**: Clean separation between internal entities and external DTOs
- **Validation**: Input validation during the mapping process
- **Security**: Sensitive data filtering in response mapping
- **Maintainability**: Centralized mapping logic for easy updates and modifications

### 6.5 Soft Delete Pattern
The soft delete pattern implements data preservation while maintaining user experience:

**Soft Delete Implementation:**
The system implements soft delete functionality for both messages and conversations. Instead of permanently removing data, the system marks records as deleted using timestamp fields. This approach preserves data integrity while allowing users to "delete" content from their perspective.

**Soft Delete Benefits:**
- **Data Preservation**: Important data is preserved for compliance and moderation
- **User Experience**: Users can remove content from their view without affecting others
- **Moderation Support**: Content remains available for reporting and moderation
- **Audit Trail**: Complete history of content changes and deletions

**Implementation Details:**
- **Message Soft Delete**: DeletedAt timestamp for individual message deletion
- **Conversation Soft Delete**: Individual user deletion timestamps for conversations
- **Automatic Cleanup**: Permanent deletion when all participants delete content
- **Data Consistency**: Maintains referential integrity while supporting soft deletes

## 7. External Service Integration

### 7.1 User Service Integration
The messaging service integrates with an external user service for authentication and user management:

**Authentication Flow:**
The UserAuthenticationService validates access tokens with the external user service and extracts user information for authorization decisions. It ensures that only authenticated users can access messaging functionality and verifies user permissions for specific operations.

**User Validation:**
The system validates user access to messaging resources by checking user permissions and ownership. It ensures that users can only access their own conversations and messages unless they have appropriate permissions.

**Integration Benefits:**
- **Centralized User Management**: User data is managed in a dedicated service
- **Scalability**: User service can be scaled independently of messaging service
- **Security**: Centralized authentication and authorization logic
- **Consistency**: Consistent user experience across multiple services

### 7.2 Authentication Strategy
The authentication strategy implements JWT token validation and user authorization:

**Token Validation:**
The system validates JWT access tokens by extracting user information and verifying token authenticity. It handles token expiration and provides user information for authenticated requests.

**Authorization Context:**
The system maintains the current user's authentication context throughout the request lifecycle. It provides user information and permissions to business logic services for authorization decisions.

**Security Features:**
- **Token Validation**: Secure token validation with external user service
- **User Authorization**: Role-based access control for messaging operations
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
Strategic database indexing optimizes query performance for messaging operations:

**Primary Indexes:**
- **Conversation Lookup**: Index on ConversationId for fast message retrieval
- **User Access**: Indexes on SenderId and ReceiverId for user-specific queries
- **Time-based Queries**: Index on CreatedAt for chronological message ordering
- **Composite Indexes**: Optimized indexes for conversation timeline queries

**Performance Benefits:**
- **Fast Message Retrieval**: Sub-second response times for conversation messages
- **Efficient User Queries**: Optimized queries for user-specific message history
- **Scalable Timeline**: Fast chronological message ordering and pagination
- **Concurrent Access**: Efficient handling of multiple concurrent messaging operations

### 8.2 Connection Management
Efficient connection management optimizes real-time communication performance:

**SignalR Connection Optimization:**
- **Connection Pooling**: Efficient management of WebSocket connections
- **Group Management**: Optimized user group assignment and management
- **Connection Tracking**: Efficient user connection state management
- **Multi-Device Support**: Optimized handling of multiple user connections

**Database Connection Optimization:**
- **Connection Pooling**: Efficient database connection reuse
- **Query Optimization**: Optimized SQL queries for messaging operations
- **Transaction Management**: Efficient transaction handling for message operations
- **Connection Monitoring**: Real-time monitoring of connection pool utilization

### 8.3 Query Optimization
Entity Framework Core query optimization ensures efficient data retrieval:

**Query Patterns:**
- **Eager Loading**: Related entities are loaded in single queries to avoid N+1 problems
- **Projection**: Only required fields are selected to minimize data transfer
- **Pagination**: Efficient skip/take implementation for large result sets
- **Async Operations**: All database operations are async for better scalability

**Performance Monitoring:**
- **Query Execution Time**: Monitoring of slow queries and performance bottlenecks
- **Connection Pool Usage**: Monitoring of database connection pool utilization
- **SignalR Performance**: Tracking of real-time communication performance
- **Response Time Metrics**: End-to-end response time tracking for all endpoints

## 9. Security Features

### 9.1 Input Validation
Comprehensive input validation prevents malicious input and data corruption:

**Validation Layers:**
- **Model Validation**: ASP.NET Core model validation for basic data types
- **Business Rule Validation**: Custom validation for messaging logic compliance
- **Database Constraints**: Database-level constraints for data integrity
- **Output Encoding**: Proper encoding of output data to prevent XSS attacks

**Security Benefits:**
- **SQL Injection Prevention**: Parameterized queries prevent SQL injection attacks
- **XSS Protection**: Input sanitization and output encoding prevent cross-site scripting
- **Data Corruption Prevention**: Validation ensures data consistency and integrity
- **Malicious Input Blocking**: Malicious input is detected and rejected

### 9.2 Authentication & Authorization
Robust authentication and authorization ensure secure access to messaging resources:

**Authentication Mechanisms:**
- **JWT Token Validation**: Secure token-based authentication with external user service
- **Token Validation**: Comprehensive token validation and user verification
- **Session Management**: Secure session handling for authenticated users
- **Error Handling**: Secure error responses that don't leak sensitive information

**Authorization Features:**
- **Resource Ownership**: Users can only access their own messages and conversations
- **Conversation Access**: Verification that users are participants in conversations
- **Message Permissions**: Sender-only operations for message deletion and modification
- **Audit Logging**: Comprehensive logging of authentication and authorization events

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

### 9.4 Message Reporting
Content moderation and reporting system for user safety:

**Reporting Features:**
- **User Reports**: Comprehensive message reporting system
- **Content Moderation**: Support for content moderation workflows
- **Duplicate Prevention**: Prevention of duplicate reports from the same user
- **Moderation Actions**: Tracking of moderation decisions and actions

**Safety Benefits:**
- **Content Moderation**: Automated and manual content review capabilities
- **User Protection**: Protection from harmful or inappropriate content
- **Community Guidelines**: Enforcement of community standards and guidelines
- **Transparency**: Clear reporting and moderation processes

## 10. Testing Strategy

### 10.1 Unit Testing
Comprehensive unit testing ensures code quality and reliability:

**Service Layer Testing:**
- **Message Service**: Unit tests for message operations and business logic
- **Conversation Service**: Tests for conversation management functionality
- **User Services**: Testing of authentication and search functionality
- **Validation Logic**: Testing of input validation and business rules

**Repository Testing:**
- **Data Access**: Testing of repository methods and data persistence
- **Error Handling**: Validation of error scenarios and edge cases
- **Permission Checking**: Testing of user access validation logic
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

### 10.3 SignalR Testing
Real-time communication testing for messaging functionality:

**SignalR Testing:**
- **Connection Management**: Testing of user connection and disconnection
- **Message Broadcasting**: Validation of real-time message delivery
- **Group Management**: Testing of user group assignment and management
- **Error Handling**: Testing of connection failures and error scenarios

**Real-Time Features:**
- **Typing Indicators**: Testing of real-time typing status updates
- **Read Receipts**: Validation of read status notifications
- **Online Status**: Testing of user connection state tracking
- **Multi-Device Support**: Testing of multiple user connections

## 11. Deployment

### 11.1 Environment Configuration
Environment-specific configuration supports different deployment scenarios:

**Configuration Files:**
- **Development**: Local development settings with debug logging
- **Staging**: Pre-production environment with production-like settings
- **Production**: Production environment with optimized performance settings
- **SignalR**: SignalR-specific configuration for real-time communication

**Environment Variables:**
- **Database Connection**: Environment-specific database connection strings
- **External Services**: Configuration for external service endpoints
- **SignalR Configuration**: Real-time communication settings
- **Logging**: Log level and output configuration for different environments

### 11.2 SignalR Deployment
Real-time communication deployment considerations:

**SignalR Deployment Features:**
- **WebSocket Support**: Primary transport for low-latency communication
- **Fallback Transports**: Automatic fallback to alternative transport methods
- **Connection Scaling**: Support for horizontal scaling with Redis backplane
- **Load Balancing**: Efficient load balancing for multiple SignalR instances

**Deployment Benefits:**
- **Real-Time Performance**: Low-latency message delivery and notifications
- **Scalability**: Support for high-concurrency messaging scenarios
- **Reliability**: Automatic fallback and connection recovery
- **Monitoring**: Comprehensive connection and performance monitoring

## 12. API Endpoints Summary

### 12.1 Message Management
Comprehensive message management endpoints:

**Message CRUD:**
- `POST /api/messages/send` - Send a new message
- `GET /api/messages/{id}` - Get specific message by ID
- `GET /api/messages/conversation/{conversationId}` - Get conversation messages
- `PUT /api/messages/{id}/read` - Mark message as read
- `PUT /api/messages/conversation/{conversationId}/read` - Mark all messages as read
- `DELETE /api/messages/{id}` - Delete message

**Message Features:**
- `POST /api/messages/report` - Report inappropriate message
- `GET /api/messages/unread/count` - Get unread message count

### 12.2 Conversation Management
Conversation lifecycle and management endpoints:

**Conversation Operations:**
- `POST /api/conversations` - Create new conversation
- `GET /api/conversations/{id}` - Get specific conversation
- `GET /api/conversations` - Get user conversations
- `PUT /api/conversations/{id}` - Update conversation
- `DELETE /api/conversations/{id}` - Delete conversation

**Conversation Features:**
- **Pagination Support**: Efficient conversation listing with skip/take parameters
- **Soft Delete**: Individual user deletion without affecting other participants
- **Real-Time Updates**: SignalR integration for conversation changes

### 12.3 User Management
User search and contact discovery endpoints:

**User Operations:**
- `GET /api/users/search?q={searchTerm}` - Search for users
- `GET /api/users/{id}` - Get specific user information

**User Features:**
- **External Integration**: Searches external user service for contacts
- **Current User Filtering**: Excludes current user from search results
- **Contact Discovery**: Efficient user search and contact management

## 13. SignalR Events

### 13.1 Client Events
Events that clients can listen to for real-time updates:

**Message Events:**
- **MessageReceived**: New message delivery notification
- **MessageDeleted**: Message deletion notification
- **MessageUpdated**: Message modification notification
- **MessageRead**: Read receipt notification

**Conversation Events:**
- **ConversationCreated**: New conversation notification
- **ConversationUpdated**: Conversation modification notification
- **ConversationDeleted**: Conversation deletion notification

**User Events:**
- **UserConnected**: User online status notification
- **UserDisconnected**: User offline status notification
- **TypingIndicator**: Real-time typing status updates

### 13.2 Server Events
Events that clients can trigger on the server:

**Connection Management:**
- **JoinUserGroup**: Add user to personal SignalR group
- **LeaveUserGroup**: Remove user from personal group
- **JoinConversation**: Add user to conversation group

**Message Operations:**
- **DeleteMessage**: Delete message with real-time notification
- **MarkMessageAsRead**: Mark message as read with notification
- **MarkMessagesAsRead**: Mark multiple messages as read
- **SendTypingIndicator**: Send typing status updates

**Real-Time Features:**
- **BroadcastToUser**: Send message to specific user
- **BroadcastToConversation**: Send message to conversation participants
- **UserStatusUpdate**: Update user online/offline status

## 14. Future Enhancements

### 14.1 Planned Features
Roadmap for upcoming messaging service features:

**Advanced Messaging Features:**
- **Message Encryption**: End-to-end encryption for secure messaging
- **File Sharing**: Enhanced file upload and sharing capabilities
- **Voice Messages**: Audio message recording and playback
- **Video Calls**: Integrated video calling functionality

**Conversation Features:**
- **Group Conversations**: Support for multi-user group chats
- **Conversation Threading**: Enhanced reply and threading capabilities
- **Conversation Search**: Full-text search within conversations
- **Message Reactions**: Emoji and reaction support for messages

### 14.2 Scalability Improvements
Technical improvements for enhanced scalability:

**Performance Enhancements:**
- **Redis Backplane**: Horizontal scaling support for SignalR
- **Message Queuing**: Asynchronous message processing with queues
- **Database Sharding**: Horizontal database scaling for large datasets
- **CDN Integration**: Content delivery network for file sharing

**Infrastructure Improvements:**
- **Kubernetes Deployment**: Container orchestration for better resource management
- **Auto-scaling**: Automatic scaling based on messaging load
- **Multi-region Deployment**: Geographic distribution for global users
- **Monitoring and Alerting**: Advanced monitoring and automated alerting systems

## 15. Contributing

### 15.1 Development Guidelines
Guidelines for contributing to the messaging service:

**Code Standards:**
- **C# Conventions**: Follow Microsoft C# coding conventions
- **Naming Conventions**: Consistent naming for classes, methods, and variables
- **Documentation**: Comprehensive XML documentation for public APIs
- **Error Handling**: Consistent error handling patterns across the codebase

**Testing Requirements:**
- **Unit Test Coverage**: Minimum 80% code coverage for new features
- **Integration Testing**: Integration tests for external service dependencies
- **SignalR Testing**: Real-time communication testing for new features
- **Security Testing**: Security validation for authentication and authorization

### 15.2 Code Review Checklist
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

## 16. Support

For technical support and questions about the Backend Messages Service:

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

*This documentation provides a comprehensive overview of the Backend Messages Service architecture, implementation patterns, and usage guidelines. For specific implementation details, refer to the source code and API documentation.*
