using BackendMessages.Data;
using BackendMessages.DTO.Notification;
using BackendMessages.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BackendMessages.Services
{
    public class NotificationService : INotificationService
    {
        private readonly MessagesDbContext _context;
        private readonly IUserSearchService _userSearchService;
        private readonly IEmailService _emailService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            MessagesDbContext context,
            IUserSearchService userSearchService,
            IEmailService emailService,
            ILogger<NotificationService> logger)
        {
            _context = context;
            _userSearchService = userSearchService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<List<UnreadMessagesSummary>> GetUsersWithUnreadMessagesAsync()
        {
            try
            {
                // Get all users who have unread messages
                var usersWithUnreadMessages = await _context.Messages
                    .Where(m => !m.IsRead && m.DeletedAt == null)
                    .GroupBy(m => m.ReceiverId)
                    .Select(g => new
                    {
                        UserId = g.Key,
                        UnreadCount = g.Count(),
                        SenderIds = g.Select(m => m.SenderId).Distinct().ToList()
                    })
                    .ToListAsync();

                var summaries = new List<UnreadMessagesSummary>();

                foreach (var userGroup in usersWithUnreadMessages)
                {
                    try
                    {
                        // Get user information
                        var user = await _userSearchService.GetUserByIdAsync(userGroup.UserId);
                        if (user == null)
                        {
                            _logger.LogWarning("User {UserId} not found when preparing notification", userGroup.UserId);
                            continue;
                        }

                        // Get sender names
                        var senderNames = new List<string>();
                        foreach (var senderId in userGroup.SenderIds)
                        {
                            var sender = await _userSearchService.GetUserByIdAsync(senderId);
                            if (sender != null)
                            {
                                senderNames.Add(sender.FullName ?? sender.Username);
                            }
                        }

                        // For now, we'll use username as email since we don't have email field in SearchUser
                        // In a real implementation, you'd need to get the email from the user service
                        var userEmail = $"{user.Username}@company.com"; // This should be replaced with actual email

                        summaries.Add(new UnreadMessagesSummary
                        {
                            UserId = userGroup.UserId,
                            UserEmail = userEmail,
                            UserName = user.FullName ?? user.Username,
                            UnreadCount = userGroup.UnreadCount,
                            SenderNames = senderNames
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing unread messages for user {UserId}", userGroup.UserId);
                    }
                }

                return summaries;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users with unread messages");
                return new List<UnreadMessagesSummary>();
            }
        }

        public async Task SendDailyUnreadMessagesNotificationsAsync()
        {
            try
            {
                _logger.LogInformation("Starting daily unread messages notification job");

                var usersWithUnreadMessages = await GetUsersWithUnreadMessagesAsync();
                
                if (usersWithUnreadMessages.Count == 0)
                {
                    _logger.LogInformation("No users with unread messages found");
                    return;
                }

                _logger.LogInformation("Found {UserCount} users with unread messages", usersWithUnreadMessages.Count);

                var successCount = 0;
                var failureCount = 0;

                foreach (var userSummary in usersWithUnreadMessages)
                {
                    try
                    {
                        var success = await _emailService.SendUnreadMessagesNotificationAsync(
                            userSummary.UserEmail,
                            userSummary.UserName,
                            userSummary.UnreadCount,
                            userSummary.SenderNames);

                        if (success)
                        {
                            successCount++;
                        }
                        else
                        {
                            failureCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send notification to user {UserId}", userSummary.UserId);
                        failureCount++;
                    }
                }

                _logger.LogInformation("Daily notification job completed. Success: {SuccessCount}, Failures: {FailureCount}", 
                    successCount, failureCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in daily unread messages notification job");
            }
        }
    }
} 