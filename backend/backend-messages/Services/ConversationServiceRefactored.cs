using BackendMessages.DTO.Conversation.Request;
using BackendMessages.DTO.Conversation.Response;
using BackendMessages.Repositories;
using BackendMessages.Services.Abstractions;
using BackendMessages.Services.Mappers;
using BackendMessages.Services.Validators;

namespace BackendMessages.Services
{
    public class ConversationServiceRefactored : IConversationService
    {
        private readonly IConversationRepository _conversationRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IUserSearchService _userSearchService;
        private readonly ILogger<ConversationServiceRefactored> _logger;
        private readonly CreateConversationValidator _createConversationValidator;
        private readonly DeleteConversationValidator _deleteConversationValidator;
        private readonly GetConversationsValidator _getConversationsValidator;
        private readonly GetConversationMessagesValidator _getConversationMessagesValidator;

        public ConversationServiceRefactored(
            IConversationRepository conversationRepository,
            IMessageRepository messageRepository,
            IUserSearchService userSearchService,
            ILogger<ConversationServiceRefactored> logger)
        {
            _conversationRepository = conversationRepository;
            _messageRepository = messageRepository;
            _userSearchService = userSearchService;
            _logger = logger;
            _createConversationValidator = new CreateConversationValidator();
            _deleteConversationValidator = new DeleteConversationValidator();
            _getConversationsValidator = new GetConversationsValidator();
            _getConversationMessagesValidator = new GetConversationMessagesValidator();
        }

        public async Task<CreateConversationResponse> CreateConversationAsync(CreateConversationRequest request)
        {
            try
            {
                // Validate request
                var validation = _createConversationValidator.Validate(request);
                if (!validation.IsValid)
                {
                    return ConversationMapper.ToCreateConversationResponse(new Models.Conversation(), Guid.Empty, null, false, validation.ErrorMessage);
                }

                // Check if conversation already exists
                var existingConversation = await _conversationRepository.GetConversationBetweenUsersAsync(request.InitiatorId, request.ReceiverId);
                if (existingConversation != null)
                {
                    // Check if it's deleted by the initiator and restore it
                    if (existingConversation.IsDeletedByUser(request.InitiatorId))
                    {
                        if (request.InitiatorId == existingConversation.InitiatorId)
                            existingConversation.InitiatorDeletedAt = null;
                        else
                            existingConversation.ReceiverDeletedAt = null;

                        await _conversationRepository.UpdateAsync(existingConversation);
                    }

                    var response = ConversationMapper.ToResponse(existingConversation, request.InitiatorId);
                    return ConversationMapper.ToCreateConversationResponse(existingConversation, request.InitiatorId);
                }

                // Create new conversation
                var conversation = ConversationMapper.ToEntity(request);
                var savedConversation = await _conversationRepository.CreateAsync(conversation);

                // Send initial message if provided
                Models.Message? initialMessage = null;
                if (!string.IsNullOrWhiteSpace(request.InitialMessage))
                {
                    var message = new Models.Message
                    {
                        ConversationId = savedConversation.Id,
                        SenderId = request.InitiatorId,
                        ReceiverId = request.ReceiverId,
                        Content = request.InitialMessage,
                        MessageType = Models.MessageType.Text,
                        IsRead = false
                    };

                    initialMessage = await _messageRepository.CreateAsync(message);
                    await _conversationRepository.UpdateLastMessageAsync(savedConversation.Id, initialMessage.Id);
                }

                _logger.LogInformation("Conversation {ConversationId} created between users {InitiatorId} and {ReceiverId}", 
                    savedConversation.Id, request.InitiatorId, request.ReceiverId);

                return ConversationMapper.ToCreateConversationResponse(savedConversation, request.InitiatorId, initialMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating conversation between users {InitiatorId} and {ReceiverId}", request.InitiatorId, request.ReceiverId);
                return ConversationMapper.ToCreateConversationResponse(new Models.Conversation(), Guid.Empty, null, false, "An error occurred while creating the conversation");
            }
        }

        public async Task<ConversationResponse?> GetConversationByIdAsync(Guid conversationId, Guid userId)
        {
            try
            {
                var canAccess = await _conversationRepository.UserCanAccessConversationAsync(conversationId, userId);
                if (!canAccess)
                {
                    return null;
                }

                var conversation = await _conversationRepository.GetByIdAsync(conversationId);
                if (conversation == null)
                {
                    return null;
                }

                var unreadCount = await _messageRepository.GetUnreadCountByConversationIdAsync(conversationId, userId);
                var otherUserId = conversation.InitiatorId == userId ? conversation.ReceiverId : conversation.InitiatorId;
                
                // Get other user info
                var otherUser = await _userSearchService.GetUserByIdAsync(otherUserId);
                var isOnline = await _userSearchService.IsUserOnlineAsync(otherUserId);

                return ConversationMapper.ToResponse(
                    conversation, 
                    userId, 
                    otherUser?.FullName, 
                    otherUser?.AvatarUrl, 
                    otherUser?.ProfessionalTitle, 
                    unreadCount, 
                    isOnline);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation {ConversationId} for user {UserId}", conversationId, userId);
                return null;
            }
        }

        public async Task<ConversationDetailResponse?> GetConversationDetailAsync(Guid conversationId, Guid userId)
        {
            try
            {
                var canAccess = await _conversationRepository.UserCanAccessConversationAsync(conversationId, userId);
                if (!canAccess)
                {
                    return null;
                }

                var conversation = await _conversationRepository.GetByIdWithMessagesAsync(conversationId, 20);
                if (conversation == null)
                {
                    return null;
                }

                var unreadCount = await _messageRepository.GetUnreadCountByConversationIdAsync(conversationId, userId);
                var otherUserId = conversation.InitiatorId == userId ? conversation.ReceiverId : conversation.InitiatorId;
                
                // Get other user info
                var otherUser = await _userSearchService.GetUserByIdAsync(otherUserId);
                var isOnline = await _userSearchService.IsUserOnlineAsync(otherUserId);

                return ConversationMapper.ToDetailResponse(
                    conversation, 
                    userId, 
                    otherUser?.FullName, 
                    otherUser?.AvatarUrl, 
                    otherUser?.ProfessionalTitle, 
                    unreadCount, 
                    isOnline);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation detail {ConversationId} for user {UserId}", conversationId, userId);
                return null;
            }
        }

        public async Task<ConversationsPagedResponse> GetUserConversationsAsync(GetConversationsRequest request)
        {
            try
            {
                var validation = _getConversationsValidator.Validate(request);
                if (!validation.IsValid)
                {
                    return new ConversationsPagedResponse();
                }

                var conversations = await _conversationRepository.GetByUserIdAsync(request.UserId, request.Page, request.PageSize, request.IncludeDeleted);
                var totalCount = await _conversationRepository.GetTotalCountByUserIdAsync(request.UserId, request.IncludeDeleted);

                // Enhance conversations with user info and unread counts
                var enhancedConversations = new List<Models.Conversation>();
                foreach (var conversation in conversations)
                {
                    // This would typically be done in a more efficient way with joins
                    var unreadCount = await _messageRepository.GetUnreadCountByConversationIdAsync(conversation.Id, request.UserId);
                    var otherUserId = conversation.InitiatorId == request.UserId ? conversation.ReceiverId : conversation.InitiatorId;
                    var otherUser = await _userSearchService.GetUserByIdAsync(otherUserId);
                    var isOnline = await _userSearchService.IsUserOnlineAsync(otherUserId);

                    // Add metadata to conversation for mapping
                    conversation.GetType().GetProperty("UnreadCount")?.SetValue(conversation, unreadCount);
                    
                    enhancedConversations.Add(conversation);
                }

                return ConversationMapper.ToPagedResponse(enhancedConversations, request.UserId, totalCount, request.Page, request.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversations for user {UserId}", request.UserId);
                return new ConversationsPagedResponse();
            }
        }

        public async Task<DeleteConversationResponse> DeleteConversationAsync(DeleteConversationRequest request)
        {
            try
            {
                var validation = _deleteConversationValidator.Validate(request);
                if (!validation.IsValid)
                {
                    return ConversationMapper.ToDeleteConversationResponse(false, validation.ErrorMessage);
                }

                var canAccess = await _conversationRepository.UserCanAccessConversationAsync(request.ConversationId, request.UserId);
                if (!canAccess)
                {
                    return ConversationMapper.ToDeleteConversationResponse(false, "Conversation not found or access denied");
                }

                var success = await _conversationRepository.SoftDeleteAsync(request.ConversationId, request.UserId);
                if (!success)
                {
                    return ConversationMapper.ToDeleteConversationResponse(false, "Failed to delete conversation");
                }

                _logger.LogInformation("Conversation {ConversationId} deleted by user {UserId}", request.ConversationId, request.UserId);

                return ConversationMapper.ToDeleteConversationResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting conversation {ConversationId} by user {UserId}", request.ConversationId, request.UserId);
                return ConversationMapper.ToDeleteConversationResponse(false, "An error occurred while deleting the conversation");
            }
        }

        public async Task<ConversationResponse?> GetOrCreateConversationAsync(Guid user1Id, Guid user2Id)
        {
            try
            {
                var existingConversation = await _conversationRepository.GetConversationBetweenUsersAsync(user1Id, user2Id);
                if (existingConversation != null)
                {
                    var unreadCount = await _messageRepository.GetUnreadCountByConversationIdAsync(existingConversation.Id, user1Id);
                    var otherUserId = existingConversation.InitiatorId == user1Id ? existingConversation.ReceiverId : existingConversation.InitiatorId;
                    var otherUser = await _userSearchService.GetUserByIdAsync(otherUserId);
                    var isOnline = await _userSearchService.IsUserOnlineAsync(otherUserId);

                    return ConversationMapper.ToResponse(
                        existingConversation, 
                        user1Id, 
                        otherUser?.FullName, 
                        otherUser?.AvatarUrl, 
                        otherUser?.ProfessionalTitle, 
                        unreadCount, 
                        isOnline);
                }

                // Create new conversation
                var createRequest = new CreateConversationRequest
                {
                    InitiatorId = user1Id,
                    ReceiverId = user2Id
                };

                var createResponse = await CreateConversationAsync(createRequest);
                return createResponse.Success ? createResponse.Conversation : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting or creating conversation between users {User1Id} and {User2Id}", user1Id, user2Id);
                return null;
            }
        }

        public async Task<ConversationStatsResponse> GetConversationStatsAsync(Guid userId)
        {
            try
            {
                var totalConversations = await _conversationRepository.GetTotalCountByUserIdAsync(userId);
                var unreadConversations = await _conversationRepository.GetUnreadConversationCountAsync(userId);
                var totalMessages = 0; // Would need to implement this in repository
                var unreadMessages = await _messageRepository.GetUnreadCountByUserIdAsync(userId);

                return ConversationMapper.ToStatsResponse(totalConversations, unreadConversations, totalMessages, unreadMessages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation stats for user {UserId}", userId);
                return new ConversationStatsResponse();
            }
        }

        public async Task<bool> UserCanAccessConversationAsync(Guid conversationId, Guid userId)
        {
            try
            {
                return await _conversationRepository.UserCanAccessConversationAsync(conversationId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking conversation access for user {UserId} and conversation {ConversationId}", userId, conversationId);
                return false;
            }
        }

        public async Task<bool> IsConversationDeletedByUserAsync(Guid conversationId, Guid userId)
        {
            try
            {
                return await _conversationRepository.IsConversationDeletedByUserAsync(conversationId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if conversation {ConversationId} is deleted by user {UserId}", conversationId, userId);
                return false;
            }
        }

        public async Task<int> GetUnreadConversationCountAsync(Guid userId)
        {
            try
            {
                return await _conversationRepository.GetUnreadConversationCountAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread conversation count for user {UserId}", userId);
                return 0;
            }
        }

        public async Task UpdateLastMessageAsync(Guid conversationId, Guid messageId)
        {
            try
            {
                await _conversationRepository.UpdateLastMessageAsync(conversationId, messageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating last message for conversation {ConversationId}", conversationId);
                throw;
            }
        }
    }
} 