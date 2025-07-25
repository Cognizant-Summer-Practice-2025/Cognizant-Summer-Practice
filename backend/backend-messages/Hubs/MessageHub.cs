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
                if (UserConnections.TryGetValue(userId, out var connections))
                {
                    connections.Remove(Context.ConnectionId);
                    if (connections.Count == 0)
                    {
                        UserConnections.TryRemove(userId, out _);
                    }
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
                foreach (var kvp in UserConnections.ToList())
                {
                    if (kvp.Value.Contains(Context.ConnectionId))
                    {
                        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{kvp.Key}");
                        kvp.Value.Remove(Context.ConnectionId);
                        if (kvp.Value.Count == 0)
                        {
                            UserConnections.TryRemove(kvp.Key, out _);
                        }
                    }
                }

                _logger.LogInformation("Connection {ConnectionId} disconnected", Context.ConnectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during disconnect cleanup for connection {ConnectionId}", Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
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