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
            _logger.LogInformation("Starting to retrieve users with unread messages...");
            
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
                        // Get user details
                        var user = await _userSearchService.GetUserByIdAsync(userGroup.UserId);
                        if (user == null)
                        {
                            _logger.LogWarning("User {UserId} not found in user service, skipping notification", userGroup.UserId);
                            continue;
                        }

                        // Get sender names
                        var senderNames = new List<string>();
                        foreach (var senderId in userGroup.SenderIds)
                        {
                            var sender = await _userSearchService.GetUserByIdAsync(senderId);
                            if (sender != null)
                            {
                                senderNames.Add(sender.Username);
                            }
                            else
                            {
                                _logger.LogWarning("Sender {SenderId} not found in user service", senderId);
                            }
                        }

                        summaries.Add(new UnreadMessagesSummary
                        {
                            UserId = userGroup.UserId,
                            UserName = user.FullName,
                            UserEmail = user.Email,
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

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                var usersWithUnreadMessages = await GetUsersWithUnreadMessagesAsync();
                
                if (usersWithUnreadMessages.Count == 0)
                {
                    _logger.LogInformation("No users with unread messages found - no notifications to send");
                     return;
                }

                var successCount = 0;
                var failureCount = 0;

                foreach (var userSummary in usersWithUnreadMessages)
                {
                     try
                    {
                        var notification = new BackendMessages.Models.Email.UnreadMessagesNotification
                        {
                            RecipientEmail = userSummary.UserEmail,
                            RecipientName = userSummary.UserName,
                            UnreadCount = userSummary.UnreadCount,
                            SenderNames = userSummary.SenderNames
                        };
                        var success = await _emailService.SendUnreadMessagesNotificationAsync(notification);

                        if (success)
                        {
                            successCount++;
                            _logger.LogInformation("Successfully sent notification to {UserEmail}", userSummary.UserEmail);
                        }
                        else
                        {
                            failureCount++;
                            _logger.LogWarning("Failed to send notification to {UserEmail} - email service returned false", userSummary.UserEmail);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send notification to user {UserId} ({UserEmail})", userSummary.UserId, userSummary.UserEmail);
                        failureCount++;
                    }
                }

                stopwatch.Stop();
                _logger.LogInformation("Unread messages notification job completed in {ElapsedMilliseconds}ms. Success: {SuccessCount}, Failures: {FailureCount}", 
                    stopwatch.ElapsedMilliseconds, successCount, failureCount);
           }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Critical error in unread messages notification job after {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
             }
        }
    }
} 