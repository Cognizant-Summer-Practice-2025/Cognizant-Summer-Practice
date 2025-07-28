using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace BackendMessages.Hubs
{
    public class MessageHub : Hub
    {
        // Store user connections - in production, you might want to use Redis or a database
        private static readonly ConcurrentDictionary<string, HashSet<string>> UserConnections = new();
        private readonly ILogger<MessageHub> _logger;

        public MessageHub(ILogger<MessageHub> logger)
        {
            _logger = logger;
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