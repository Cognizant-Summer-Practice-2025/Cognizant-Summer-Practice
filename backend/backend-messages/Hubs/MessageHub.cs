using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using BackendMessages.Data;

namespace BackendMessages.Hubs
{
    public class MessageHub : Hub
    {
        // Store user connections
        private static readonly ConcurrentDictionary<string, HashSet<string>> UserConnections = new();
        private readonly ILogger<MessageHub> _logger;
        private readonly MessagesDbContext _context;

        public MessageHub(ILogger<MessageHub> logger, MessagesDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Delete a message and broadcast the deletion to all relevant users
        /// </summary>
        /// <param name="messageId">The ID of the message to delete</param>
        /// <param name="userId">The ID of the user requesting deletion</param>
        public async Task DeleteMessage(string messageId, string userId)
        {
            try
            {
                if (!Guid.TryParse(messageId, out var messageGuid) || !Guid.TryParse(userId, out var userGuid))
                {
                    _logger.LogWarning("Invalid messageId {MessageId} or userId {UserId}", messageId, userId);
                    return;
                }

                var message = await _context.Messages
                    .FirstOrDefaultAsync(m => m.Id == messageGuid && m.DeletedAt == null);

                if (message == null)
                {
                    _logger.LogWarning("Message {MessageId} not found", messageId);
                    return;
                }

                // Only sender can delete message
                if (message.SenderId != userGuid)
                {
                    _logger.LogWarning("User {UserId} attempted to delete message {MessageId} they didn't send", userId, messageId);
                    return;
                }

                // Soft delete the message
                message.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                message.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                await _context.SaveChangesAsync();

                // Broadcast the deletion event
                var messageDeletedEvent = new
                {
                    MessageId = messageId,
                    ConversationId = message.ConversationId.ToString(),
                    DeletedBy = userId,
                    DeletedAt = message.DeletedAt
                };

                // Send to receiver
                await Clients.Group($"user_{message.ReceiverId}")
                    .SendAsync("MessageDeleted", messageDeletedEvent);

                // Send to sender (for multi-device support)
                await Clients.Group($"user_{message.SenderId}")
                    .SendAsync("MessageDeleted", messageDeletedEvent);

                _logger.LogInformation("Message {MessageId} deleted by user {UserId} and broadcasted to sender {SenderId} and receiver {ReceiverId}", 
                    messageId, userId, message.SenderId, message.ReceiverId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message {MessageId} by user {UserId}", messageId, userId);
            }
        }

        /// <summary>
        /// Mark a single message as read and broadcast the read receipt to the sender
        /// </summary>
        /// <param name="messageId">The ID of the message to mark as read</param>
        /// <param name="userId">The ID of the user marking the message as read</param>
        public async Task MarkMessageAsRead(string messageId, string userId)
        {
            try
            {
                if (!Guid.TryParse(messageId, out var messageGuid) || !Guid.TryParse(userId, out var userGuid))
                {
                    _logger.LogWarning("Invalid messageId {MessageId} or userId {UserId}", messageId, userId);
                    return;
                }

                // Find the message
                var message = await _context.Messages
                    .FirstOrDefaultAsync(m => m.Id == messageGuid && m.ReceiverId == userGuid && m.DeletedAt == null);

                if (message == null)
                {
                    _logger.LogWarning("Message {MessageId} not found or user {UserId} is not the receiver", messageId, userId);
                    return;
                }

                // Only mark as read if it's not already read
                if (!message.IsRead)
                {
                    message.IsRead = true;
                    message.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                    await _context.SaveChangesAsync();

                    // Broadcast the read receipt to the sender
                    var readReceipt = new
                    {
                        MessageId = message.Id.ToString(),
                        ConversationId = message.ConversationId.ToString(),
                        ReadByUserId = userId,
                        ReadAt = message.UpdatedAt
                    };

                    // Send read receipt to the sender
                    await Clients.Group($"user_{message.SenderId}")
                        .SendAsync("MessageRead", readReceipt);

                    // Also send to the reader (for multi-device support)
                    await Clients.Group($"user_{userId}")
                        .SendAsync("MessageRead", readReceipt);

                    _logger.LogInformation("Message {MessageId} marked as read by user {UserId}, receipt sent to sender {SenderId}", 
                        messageId, userId, message.SenderId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking message {MessageId} as read by user {UserId}", messageId, userId);
            }
        }

        public async Task JoinUserGroup(string userId)
        {
            try
            {
                var wasOnline = UserConnections.ContainsKey(userId) && UserConnections[userId].Count > 0;

                // Add this connection to the user's group
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                
                // Track the connection
                UserConnections.AddOrUpdate(userId, 
                    new HashSet<string> { Context.ConnectionId },
                    (key, existingConnections) => 
                    {
                        existingConnections.Add(Context.ConnectionId);
                        return existingConnections;
                    });

                // If user just came online, broadcast presence update
                if (!wasOnline)
                {
                    await BroadcastUserPresenceUpdate(userId, true);
                    _logger.LogInformation("User {UserId} came online", userId);
                }

                _logger.LogInformation("User {UserId} joined with connection {ConnectionId}", userId, Context.ConnectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user {UserId} to group", userId);
            }
        }

        public async Task LeaveUserGroup(string userId)
        {
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
                
                // Remove the connection from tracking
                var userWentOffline = false;
                if (UserConnections.TryGetValue(userId, out var connections))
                {
                    connections.Remove(Context.ConnectionId);
                    if (connections.Count == 0)
                    {
                        UserConnections.TryRemove(userId, out _);
                        userWentOffline = true;
                    }
                }

                // If user went offline, broadcast presence update
                if (userWentOffline)
                {
                    await BroadcastUserPresenceUpdate(userId, false);
                    _logger.LogInformation("User {UserId} went offline", userId);
                }

                _logger.LogInformation("User {UserId} left with connection {ConnectionId}", userId, Context.ConnectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user {UserId} from group", userId);
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                // Clean up all user groups for this connection
                var usersWentOffline = new List<string>();
                
                foreach (var kvp in UserConnections.ToList())
                {
                    if (kvp.Value.Contains(Context.ConnectionId))
                    {
                        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{kvp.Key}");
                        kvp.Value.Remove(Context.ConnectionId);
                        if (kvp.Value.Count == 0)
                        {
                            UserConnections.TryRemove(kvp.Key, out _);
                            usersWentOffline.Add(kvp.Key);
                        }
                    }
                }

                // Broadcast offline status for users who went offline
                foreach (var userId in usersWentOffline)
                {
                    await BroadcastUserPresenceUpdate(userId, false);
                    _logger.LogInformation("User {UserId} went offline due to disconnect", userId);
                }

                _logger.LogInformation("Connection {ConnectionId} disconnected", Context.ConnectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during disconnect cleanup for connection {ConnectionId}", Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Method to broadcast user presence updates to all connected clients
        private async Task BroadcastUserPresenceUpdate(string userId, bool isOnline)
        {
            try
            {
                var presenceUpdate = new
                {
                    userId = userId,
                    isOnline = isOnline,
                    timestamp = DateTime.UtcNow
                };

                // Broadcast to all connected users
                await Clients.All.SendAsync("UserPresenceUpdate", presenceUpdate);
                
                _logger.LogInformation("Broadcasted presence update for user {UserId}: {IsOnline}", userId, isOnline);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting presence update for user {UserId}", userId);
            }
        }

        // Method to check if a user is online
        public static bool IsUserOnline(string userId)
        {
            return UserConnections.ContainsKey(userId) && UserConnections[userId].Count > 0;
        }

        // Method to get all online users
        public static IEnumerable<string> GetOnlineUsers()
        {
            return UserConnections.Keys;
        }
    }
} 