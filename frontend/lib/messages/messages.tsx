import { useState, useEffect, useCallback, useRef } from "react";
import { useUser } from "../contexts/user-context";
import { SearchUser } from "../user";
import { messagesApi, ApiMessage } from "./api";
import { MessageEncryption } from "../encryption";
import { useWebSocket } from "../contexts/websocket-context";

// Helper function to safely decrypt messages
const safeDecrypt = (content: string, senderId: string): string => {
  // If content is empty or sender ID is missing, return as-is
  if (!content || !senderId) {
    return content || '';
  }

  try {
    return MessageEncryption.decrypt(content, senderId);
  } catch (error) {
    console.warn('Failed to decrypt message, returning original content:', error);
    return content; // Fallback to original content if decryption fails
  }
};

export interface Message {
  id: string;
  senderId: string;
  receiverId: string;
  content: string;
  isRead: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface Conversation {
  id: string;
  otherUserId: string;
  otherUserName: string;
  otherUserAvatar?: string;
  otherUserProfessionalTitle?: string;
  lastMessage?: Message;
  isOnline?: boolean;
  unreadCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface CacheState {
  isFromCache: boolean;
  lastRefresh: number | null;
  isRefreshing: boolean;
}

const useMessages = () => {
  const { user } = useUser();
  const { onMessageReceived, onConversationUpdated, onUserPresenceUpdate, isConnected } = useWebSocket();
  
  // Initialize conversations from cache if available
  const [conversations, setConversations] = useState<Conversation[]>(() => {
    if (typeof window !== 'undefined' && user?.id) {
      try {
        const cacheKey = `conversations_${user.id}`;
        const cached = localStorage.getItem(cacheKey);
        const cacheTimestamp = localStorage.getItem(`${cacheKey}_timestamp`);
        
        // Check if cache is very old (older than 1 day) and clean it up
        const CACHE_MAX_AGE = 24 * 60 * 60 * 1000; // 24 hours
        if (cached && cacheTimestamp) {
          const cacheAge = Date.now() - parseInt(cacheTimestamp);
          if (cacheAge > CACHE_MAX_AGE) {
            console.log('Cache is too old, cleaning up');
            localStorage.removeItem(cacheKey);
            localStorage.removeItem(`${cacheKey}_timestamp`);
            return [];
          }
          
          console.log('Loading conversations from cache');
          return JSON.parse(cached);
        }
      } catch (error) {
        console.warn('Failed to load conversations from cache:', error);
      }
    }
    return [];
  });
  
  const [currentConversation, setCurrentConversation] = useState<Conversation | null>(null);
  const [messages, setMessages] = useState<Message[]>([]);
  const [loading, setLoading] = useState(false);
  const [messagesLoading, setMessagesLoading] = useState(false);
  const [sendingMessage, setSendingMessage] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [messagesError, setMessagesError] = useState<string | null>(null);
  
  // Track cache state for better user experience
  const [cacheState, setCacheState] = useState<{
    isFromCache: boolean;
    lastRefresh: number | null;
    isRefreshing: boolean;
  }>(() => {
    if (typeof window !== 'undefined' && user?.id) {
      const cacheKey = `conversations_${user.id}`;
      const cacheTimestamp = localStorage.getItem(`${cacheKey}_timestamp`);
      const hasCache = localStorage.getItem(cacheKey) !== null;
      
      return {
        isFromCache: hasCache,
        lastRefresh: cacheTimestamp ? parseInt(cacheTimestamp) : null,
        isRefreshing: false
      };
    }
    return {
      isFromCache: false,
      lastRefresh: null,
      isRefreshing: false
    };
  });

  // Cache conversations in localStorage
  const cacheConversations = useCallback((conversations: Conversation[]) => {
    if (typeof window !== 'undefined' && user?.id) {
      try {
        const cacheKey = `conversations_${user.id}`;
        localStorage.setItem(cacheKey, JSON.stringify(conversations));
        const timestamp = Date.now();
        localStorage.setItem(`${cacheKey}_timestamp`, timestamp.toString());
        console.log('Conversations cached successfully');
        
        // Update cache state
        setCacheState(prev => ({
          ...prev,
          lastRefresh: timestamp,
          isFromCache: false
        }));
      } catch (error) {
        console.warn('Failed to cache conversations:', error);
      }
    }
  }, [user?.id]);

  // Cache messages for a specific conversation
  const cacheMessages = useCallback((conversationId: string, messages: Message[]) => {
    if (typeof window !== 'undefined' && user?.id) {
      try {
        const cacheKey = `messages_${user.id}_${conversationId}`;
        localStorage.setItem(cacheKey, JSON.stringify(messages));
        const timestamp = Date.now();
        localStorage.setItem(`${cacheKey}_timestamp`, timestamp.toString());
        console.log(`Messages cached for conversation ${conversationId}`);
      } catch (error) {
        console.warn(`Failed to cache messages for conversation ${conversationId}:`, error);
      }
    }
  }, [user?.id]);

  // Load messages from cache for a specific conversation
  const loadMessagesFromCache = useCallback((conversationId: string): Message[] | null => {
    if (typeof window !== 'undefined' && user?.id) {
      try {
        const cacheKey = `messages_${user.id}_${conversationId}`;
        const cached = localStorage.getItem(cacheKey);
        const cacheTimestamp = localStorage.getItem(`${cacheKey}_timestamp`);
        
        if (cached && cacheTimestamp) {
          // Check if cache is too old (older than 1 hour for messages)
          const MESSAGE_CACHE_MAX_AGE = 60 * 60 * 1000; // 1 hour
          const cacheAge = Date.now() - parseInt(cacheTimestamp);
          
          if (cacheAge > MESSAGE_CACHE_MAX_AGE) {
            console.log(`Message cache for conversation ${conversationId} is too old, removing`);
            localStorage.removeItem(cacheKey);
            localStorage.removeItem(`${cacheKey}_timestamp`);
            return null;
          }
          
          console.log(`Loading messages from cache for conversation ${conversationId}`);
          return JSON.parse(cached);
        }
      } catch (error) {
        console.warn(`Failed to load messages from cache for conversation ${conversationId}:`, error);
      }
    }
    return null;
  }, [user?.id]);

  // Clear message cache for a specific conversation
  const clearMessageCache = useCallback((conversationId: string) => {
    if (typeof window !== 'undefined' && user?.id) {
      try {
        const cacheKey = `messages_${user.id}_${conversationId}`;
        localStorage.removeItem(cacheKey);
        localStorage.removeItem(`${cacheKey}_timestamp`);
        console.log(`Message cache cleared for conversation ${conversationId}`);
      } catch (error) {
        console.warn(`Failed to clear message cache for conversation ${conversationId}:`, error);
      }
    }
  }, [user?.id]);

  // Use ref to avoid dependency issues in WebSocket handlers
  const cacheConversationsRef = useRef(cacheConversations);
  useEffect(() => {
    cacheConversationsRef.current = cacheConversations;
  }, [cacheConversations]);

  // Clean up old cache entries from localStorage periodically
  useEffect(() => {
    if (typeof window !== 'undefined' && user?.id) {
      const cleanupOldCaches = () => {
        try {
          const keysToRemove: string[] = [];
          const currentUserConversationsKey = `conversations_${user.id}`;
          const currentUserPrefix = `messages_${user.id}_`;
          
          for (let i = 0; i < localStorage.length; i++) {
            const key = localStorage.key(i);
            if (key) {
              // Clean up old conversation caches from other users
              if (key.startsWith('conversations_') && key !== currentUserConversationsKey) {
                const timestampKey = `${key}_timestamp`;
                const timestamp = localStorage.getItem(timestampKey);
                
                if (timestamp) {
                  const age = Date.now() - parseInt(timestamp);
                  const CLEANUP_AGE = 7 * 24 * 60 * 60 * 1000; // 7 days
                  
                  if (age > CLEANUP_AGE) {
                    keysToRemove.push(key, timestampKey);
                  }
                } else {
                  // Remove entries without timestamp
                  keysToRemove.push(key);
                }
              }
              
              // Clean up old message caches (both current user and other users)
              if (key.startsWith('messages_')) {
                const timestampKey = `${key}_timestamp`;
                const timestamp = localStorage.getItem(timestampKey);
                
                if (timestamp) {
                  const age = Date.now() - parseInt(timestamp);
                  // More aggressive cleanup for message caches (3 days)
                  const MESSAGE_CLEANUP_AGE = 3 * 24 * 60 * 60 * 1000; // 3 days
                  
                  if (age > MESSAGE_CLEANUP_AGE) {
                    keysToRemove.push(key, timestampKey);
                  }
                } else if (!key.startsWith(currentUserPrefix)) {
                  // Remove message entries from other users without timestamp
                  keysToRemove.push(key);
                }
              }
            }
          }
          
          keysToRemove.forEach(key => localStorage.removeItem(key));
          if (keysToRemove.length > 0) {
            console.log(`Cleaned up ${keysToRemove.length} old cache entries`);
          }
        } catch (error) {
          console.warn('Failed to cleanup old caches:', error);
        }
      };
      
      // Run cleanup once when component mounts
      cleanupOldCaches();
    }
  }, [user?.id]);

  // Clear cache for current user
  const clearConversationsCache = useCallback(() => {
    if (typeof window !== 'undefined' && user?.id) {
      const cacheKey = `conversations_${user.id}`;
      localStorage.removeItem(cacheKey);
      localStorage.removeItem(`${cacheKey}_timestamp`);
      console.log('Conversations cache cleared');
      
      // Reset cache state
      setCacheState({
        isFromCache: false,
        lastRefresh: null,
        isRefreshing: false
      });
    }
  }, [user?.id]);

  // Clear all message caches for current user
  const clearAllMessageCaches = useCallback(() => {
    if (typeof window !== 'undefined' && user?.id) {
      try {
        const prefix = `messages_${user.id}_`;
        const keysToRemove: string[] = [];
        
        for (let i = 0; i < localStorage.length; i++) {
          const key = localStorage.key(i);
          if (key && key.startsWith(prefix)) {
            keysToRemove.push(key);
            keysToRemove.push(`${key}_timestamp`);
          }
        }
        
        keysToRemove.forEach(key => localStorage.removeItem(key));
        console.log(`Cleared ${keysToRemove.length} message cache entries`);
      } catch (error) {
        console.warn('Failed to clear message caches:', error);
      }
    }
  }, [user?.id]);

  // Function to check online status for a specific user (only when needed)
  const checkUserOnlineStatus = useCallback(async (userId: string) => {
    try {
      console.log(`Making API call to check online status for user: ${userId}`);
      const statusResponse = await messagesApi.getUserOnlineStatus(userId);
      console.log(`API response for user ${userId}:`, statusResponse);
      return statusResponse.isOnline;
    } catch (error) {
      console.error(`Failed to check online status for user ${userId}:`, error);
      return false;
    }
  }, []);

  // Update online status for conversations when presence changes
  const updateConversationOnlineStatus = useCallback((userId: string, isOnline: boolean) => {
    console.log(`updateConversationOnlineStatus called: userId=${userId}, isOnline=${isOnline}`);
    
    // Update conversations list
    setConversations(prev => {
      const updated = prev.map(conv => {
        if (conv.otherUserId === userId) {
          console.log(`Updating conversation ${conv.id} online status from ${conv.isOnline} to ${isOnline}`);
          return { ...conv, isOnline };
        }
        return conv;
      });
      console.log('Updated conversations:', updated.map(c => ({ id: c.id, otherUserId: c.otherUserId, isOnline: c.isOnline })));
      return updated;
    });

    // Update current conversation if it's the same user
    setCurrentConversation(prev => {
      if (prev && prev.otherUserId === userId) {
        console.log(`Updating currentConversation online status from ${prev.isOnline} to ${isOnline}`);
        return { ...prev, isOnline };
      }
      return prev;
    });

    console.log(`Updated online status for user ${userId}: ${isOnline ? 'online' : 'offline'}`);
  }, []);

  const loadConversations = useCallback(async (showLoadingSpinner: boolean = true) => {
    if (!user?.id) return;
    
    if (showLoadingSpinner) {
      setLoading(true);
    } else {
      setCacheState(prev => ({ ...prev, isRefreshing: true }));
    }
    setError(null);
    
    try {
      const apiConversations = await messagesApi.getUserConversations(user.id);
      
      // Enrich conversations with user details
      const enrichedConversations = await Promise.all(
        apiConversations.map(async (apiConv) => {
          try {
            const otherUser = await messagesApi.getUserById(apiConv.otherUserId);
            
            const conversation: Conversation = {
              id: apiConv.id,
              otherUserId: apiConv.otherUserId,
              otherUserName: `${otherUser.firstName || ''} ${otherUser.lastName || ''}`.trim() || otherUser.username,
              otherUserAvatar: otherUser.avatarUrl,
              otherUserProfessionalTitle: otherUser.professionalTitle,
              lastMessage: apiConv.lastMessage ? {
                id: apiConv.lastMessage.id,
                senderId: apiConv.lastMessage.senderId,
                receiverId: apiConv.lastMessage.receiverId,
                content: safeDecrypt(apiConv.lastMessage.content, apiConv.lastMessage.senderId),
                isRead: apiConv.lastMessage.isRead,
                createdAt: apiConv.lastMessage.createdAt,
                updatedAt: apiConv.lastMessage.updatedAt
              } : undefined,
              isOnline: false,
              unreadCount: apiConv.unreadCount,
              createdAt: apiConv.createdAt,
              updatedAt: apiConv.updatedAt
            };
            
            return conversation;
          } catch (userError) {
            console.error(`Failed to get user details for ${apiConv.otherUserId}:`, userError);
            // Fallback conversation without user details
            const conversation: Conversation = {
              id: apiConv.id,
              otherUserId: apiConv.otherUserId,
              otherUserName: 'Unknown User',
              lastMessage: apiConv.lastMessage ? {
                id: apiConv.lastMessage.id,
                senderId: apiConv.lastMessage.senderId,
                receiverId: apiConv.lastMessage.receiverId,
                content: safeDecrypt(apiConv.lastMessage.content, apiConv.lastMessage.senderId),
                isRead: apiConv.lastMessage.isRead,
                createdAt: apiConv.lastMessage.createdAt,
                updatedAt: apiConv.lastMessage.updatedAt
              } : undefined,
              isOnline: false,
              unreadCount: apiConv.unreadCount,
              createdAt: apiConv.createdAt,
              updatedAt: apiConv.updatedAt
            };
            
            return conversation;
          }
        })
      );
      
      setConversations(enrichedConversations);
      cacheConversations(enrichedConversations);
      console.log('Conversations loaded and cached');
    } catch (err) {
      console.error('Error loading conversations:', err);
      setError('Failed to load conversations');
    } finally {
      setLoading(false);
      setCacheState(prev => ({ ...prev, isRefreshing: false }));
    }
  }, [user?.id, cacheConversations]);

  // Load conversations with optimized cache-first strategy
  const loadConversationsWithCache = useCallback(async () => {
    if (!user?.id) return;

    // Check for cache
    const cacheKey = `conversations_${user.id}`;
    const cached = localStorage.getItem(cacheKey);
    const cacheTimestamp = localStorage.getItem(`${cacheKey}_timestamp`);
    const hasCache = conversations.length > 0 || cached;
    
    // Determine cache freshness (consider fresh if less than 2 minutes old)
    const CACHE_FRESH_TTL = 2 * 60 * 1000; // 2 minutes
    const isCacheFresh = cacheTimestamp && (Date.now() - parseInt(cacheTimestamp) < CACHE_FRESH_TTL);

    if (hasCache) {
      console.log('Using cached data, refreshing in background');
      setCacheState(prev => ({ ...prev, isFromCache: true }));
      
      // If cache is not fresh, refresh in background without loading spinner
      if (!isCacheFresh) {
        try {
          await loadConversations(false); // Don't show loading spinner
        } catch (error) {
          console.error('Background refresh failed:', error);
        }
      }
    } else {
      // No cache exists, load with loading spinner
      console.log('No cache found, loading with spinner');
      setCacheState(prev => ({ ...prev, isFromCache: false }));
      await loadConversations(true); // Show loading spinner
    }
  }, [user?.id, conversations.length, loadConversations]);

  const loadMessages = useCallback(async (conversationId: string, showLoadingSpinner: boolean = true) => {
    if (!user?.id) return;
    
    if (showLoadingSpinner) {
      setMessagesLoading(true);
    }
    setMessagesError(null);
    
    try {
      const response = await messagesApi.getConversationMessages(conversationId);
      
      const formattedMessages: Message[] = response.messages.map((apiMsg: ApiMessage) => {
        // Decrypt the message content using the sender's user ID
        const decryptedContent = safeDecrypt(apiMsg.content, apiMsg.senderId);
        
        return {
          id: apiMsg.id,
          senderId: apiMsg.senderId,
          receiverId: apiMsg.receiverId,
          content: decryptedContent,
          isRead: apiMsg.isRead,
          createdAt: apiMsg.createdAt,
          updatedAt: apiMsg.updatedAt
        };
      });
      
      setMessages(formattedMessages);
      
      // Cache the messages for future use
      cacheMessages(conversationId, formattedMessages);
      
      // Mark messages as read if they're not from the current user
      const unreadMessages = formattedMessages.filter(
        msg => msg.receiverId === user.id && !msg.isRead
      );
      
      if (unreadMessages.length > 0) {
        try {
          await messagesApi.markMessagesAsRead({
            conversationId,
            userId: user.id
          });
          
          // Update local state to mark messages as read
          setMessages(prev => prev.map(msg => 
            msg.receiverId === user.id ? { ...msg, isRead: true } : msg
          ));
          
          // Update conversation unread count
          setConversations(prev => prev.map(conv => 
            conv.id === conversationId ? { ...conv, unreadCount: 0 } : conv
          ));
        } catch (markReadError) {
          console.error('Failed to mark messages as read:', markReadError);
        }
      }
    } catch (error) {
      console.error('Failed to load messages:', error);
      setMessagesError('Failed to load messages');
    } finally {
      setMessagesLoading(false);
    }
  }, [user?.id, cacheMessages]);

  // Load messages with cache-first strategy
  const loadMessagesWithCache = useCallback(async (conversationId: string) => {
    if (!user?.id) return;

    // Try to load from cache first
    const cachedMessages = loadMessagesFromCache(conversationId);
    
    if (cachedMessages && cachedMessages.length > 0) {
      console.log(`Using cached messages for conversation ${conversationId}, refreshing in background`);
      
      // Show cached messages immediately
      setMessages(cachedMessages);
      
      // Check cache freshness (consider fresh if less than 5 minutes old)
      const MESSAGE_CACHE_FRESH_TTL = 5 * 60 * 1000; // 5 minutes
      const cacheKey = `messages_${user.id}_${conversationId}`;
      const cacheTimestamp = localStorage.getItem(`${cacheKey}_timestamp`);
      const isCacheFresh = cacheTimestamp && (Date.now() - parseInt(cacheTimestamp) < MESSAGE_CACHE_FRESH_TTL);
      
      // If cache is not fresh, refresh in background without loading spinner
      if (!isCacheFresh) {
        try {
          await loadMessages(conversationId, false); // Don't show loading spinner
        } catch (error) {
          console.error('Background message refresh failed:', error);
        }
      }
    } else {
      // No cache exists, load with loading spinner
      console.log(`No message cache found for conversation ${conversationId}, loading with spinner`);
      await loadMessages(conversationId, true); // Show loading spinner
    }
  }, [user?.id, loadMessagesFromCache, loadMessages]);;

  const selectConversation = useCallback(async (conversation: Conversation) => {
    setCurrentConversation(conversation);
    loadMessagesWithCache(conversation.id);
    
    // Check online status for the selected user
    console.log(`Checking online status for user: ${conversation.otherUserId}`);
    try {
      const isOnline = await checkUserOnlineStatus(conversation.otherUserId);
      console.log(`User ${conversation.otherUserId} online status: ${isOnline}`);
      updateConversationOnlineStatus(conversation.otherUserId, isOnline);
    } catch (error) {
      console.error('Failed to check online status for selected conversation:', error);
    }
  }, [loadMessagesWithCache, checkUserOnlineStatus, updateConversationOnlineStatus]);

  const createConversation = useCallback(async (searchUser: SearchUser) => {
    if (!user?.id) return null;
    
    try {
      const apiConversation = await messagesApi.createOrGetConversation({
        initiatorId: user.id,
        receiverId: searchUser.id
      });
      
      const newConversation: Conversation = {
        id: apiConversation.id,
        otherUserId: apiConversation.otherUserId,
        otherUserName: searchUser.fullName,
        otherUserAvatar: searchUser.avatarUrl,
        otherUserProfessionalTitle: searchUser.professionalTitle,
        lastMessage: apiConversation.lastMessage ? {
          id: apiConversation.lastMessage.id,
          senderId: apiConversation.lastMessage.senderId,
          receiverId: apiConversation.lastMessage.receiverId,
          content: safeDecrypt(apiConversation.lastMessage.content, apiConversation.lastMessage.senderId),
          isRead: apiConversation.lastMessage.isRead,
          createdAt: apiConversation.lastMessage.createdAt,
          updatedAt: apiConversation.lastMessage.updatedAt
        } : undefined,
        isOnline: false,
        unreadCount: apiConversation.unreadCount,
        createdAt: apiConversation.createdAt,
        updatedAt: apiConversation.updatedAt
      };
      
      // Only add to conversations if it's a new conversation
      if (!apiConversation.isExisting) {
        setConversations(prev => [newConversation, ...prev]);
      } else {
        // Update existing conversation or ensure it's in the list
        setConversations(prev => {
          const existingIndex = prev.findIndex(conv => conv.id === newConversation.id);
          if (existingIndex >= 0) {
            const updated = [...prev];
            updated[existingIndex] = newConversation;
            return updated;
          } else {
            return [newConversation, ...prev];
          }
        });
      }
      cacheConversations(conversations); // Cache the updated conversations
      
      return newConversation;
    } catch (error) {
      console.error('Failed to create conversation:', error);
      return null;
    }
  }, [user?.id, conversations, cacheConversations]);

  const deleteConversation = useCallback(async (conversationId: string) => {
    if (!user?.id) {
      console.error('No user ID available for delete conversation');
      return;
    }

    console.log('deleteConversation called with:', { conversationId, userId: user.id });

    try {
      console.log('Making API call to delete conversation...');
      await messagesApi.deleteConversation(conversationId, user.id);
      console.log('API call successful, updating state...');
      
      // Remove conversation from state
      setConversations(prev => prev.filter(conv => conv.id !== conversationId));
      cacheConversations(conversations); // Cache the updated conversations
      
      // Clear current conversation if it was the deleted one
      if (currentConversation?.id === conversationId) {
        setCurrentConversation(null);
        setMessages([]);
      }
      console.log('State updated successfully');
    } catch (error) {
      console.error('Failed to delete conversation:', error);
      throw error;
    }
  }, [user?.id, currentConversation?.id, conversations, cacheConversations]);

  const sendMessage = useCallback(async (content: string) => {
    if (!user?.id || !currentConversation) return;
    
    setSendingMessage(true);
    
    try {
      // Encrypt the message content using the sender's user ID as the key
      const encryptedContent = MessageEncryption.encrypt(content, user.id);
      
      const apiMessage = await messagesApi.sendMessage({
        conversationId: currentConversation.id,
        senderId: user.id,
        content: encryptedContent,
        messageType: 0 // Text message
      });
      
      // Decrypt the message content for display using the sender's user ID
      const decryptedContent = safeDecrypt(apiMessage.content, apiMessage.senderId);
      
      const newMessage: Message = {
        id: apiMessage.id,
        senderId: apiMessage.senderId,
        receiverId: apiMessage.receiverId,
        content: decryptedContent,
        isRead: apiMessage.isRead,
        createdAt: apiMessage.createdAt,
        updatedAt: apiMessage.updatedAt
      };
      
      console.log('Message sent via API:', newMessage.id, '- waiting for WebSocket event');
      
      // Don't add the message locally - let the WebSocket event handle it
      // This prevents duplicates and ensures consistent behavior
      
      // Update conversation's last message
      setConversations(prev => {
        const updatedConversations = prev.map(conv => 
          conv.id === currentConversation.id
            ? { ...conv, lastMessage: newMessage, updatedAt: apiMessage.updatedAt }
            : conv
        )
        .sort((a, b) => new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime());
        
        // Cache the updated conversations
        cacheConversations(updatedConversations);
        return updatedConversations;
      });
      
      // Clear message cache for this conversation since we sent a new message
      // It will be refreshed when WebSocket event arrives
      clearMessageCache(currentConversation.id);
      
      return newMessage;
    } catch (error) {
      console.error('Failed to send message:', error);
      return null;
    } finally {
      setSendingMessage(false);
    }
  }, [user?.id, currentConversation, conversations, cacheConversations]);

  useEffect(() => {
    if (user?.id) {
      loadConversationsWithCache();
    }
  }, [user?.id, loadConversationsWithCache]);

  // WebSocket event handlers for real-time updates
  useEffect(() => {
    if (!user?.id) return;

    // Handle new messages
    const unsubscribeMessage = onMessageReceived((newMessage) => {
      console.log('Received new message:', newMessage);
      
      const formattedMessage: Message = {
        id: newMessage.id,
        senderId: newMessage.senderId,
        receiverId: newMessage.receiverId,
        content: safeDecrypt(newMessage.content, newMessage.senderId),
        isRead: newMessage.isRead,
        createdAt: newMessage.createdAt,
        updatedAt: newMessage.updatedAt
      };

      // Add message to current conversation if it matches
      setCurrentConversation(currentConv => {
        if (currentConv && newMessage.conversationId === currentConv.id) {
          setMessages(prevMessages => {
            // Check if message already exists to avoid duplicates
            const exists = prevMessages.some(msg => msg.id === newMessage.id);
            if (exists) {
              console.log('Duplicate message prevented:', newMessage.id);
              return prevMessages;
            }
            console.log('Adding real-time message:', newMessage.id);
            const updatedMessages = [...prevMessages, formattedMessage];
            
            // Update message cache with the new message
            cacheMessages(newMessage.conversationId, updatedMessages);
            
            return updatedMessages;
          });

          // Mark as read if the user is the receiver and viewing this conversation
          if (newMessage.receiverId === user.id) {
            setTimeout(async () => {
              try {
                await messagesApi.markMessagesAsRead({
                  conversationId: newMessage.conversationId,
                  userId: user.id
                });
              } catch (error) {
                console.error('Failed to mark message as read:', error);
              }
            }, 500);
          }
        }
        return currentConv;
      });

      // Update conversations list using functional update
      setConversations(prevConversations => {
        const updatedConversations = prevConversations.map(conv => {
          if (conv.id === newMessage.conversationId) {
            return {
              ...conv,
              lastMessage: formattedMessage,
              updatedAt: newMessage.updatedAt,
              unreadCount: newMessage.receiverId === user.id && currentConversation?.id !== newMessage.conversationId 
                ? conv.unreadCount + 1 
                : conv.unreadCount
            };
          }
          return conv;
        }).sort((a, b) => new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime());
        
        // Cache the updated conversations
        cacheConversationsRef.current(updatedConversations);
        return updatedConversations;
      });
    });

    // Handle conversation updates
    const unsubscribeConversation = onConversationUpdated((update) => {
      console.log('Conversation updated:', update);
      
      setConversations(prevConversations => {
        const updatedConversations = prevConversations.map(conv => {
          if (conv.id === update.id) {
            return {
              ...conv,
              lastMessage: {
                id: update.lastMessage.id,
                senderId: update.lastMessage.senderId,
                receiverId: update.lastMessage.receiverId,
                content: safeDecrypt(update.lastMessage.content, update.lastMessage.senderId),
                isRead: update.lastMessage.isRead,
                createdAt: update.lastMessage.createdAt,
                updatedAt: update.lastMessage.updatedAt
              },
              updatedAt: update.updatedAt
            };
          }
          return conv;
        }).sort((a, b) => new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime());
        
        // Cache the updated conversations
        cacheConversationsRef.current(updatedConversations);
        return updatedConversations;
      });
    });

    // Handle user presence updates
    const unsubscribePresence = onUserPresenceUpdate((update) => {
      console.log('Received user presence update:', update);
      updateConversationOnlineStatus(update.userId, update.isOnline);
    });

    return () => {
      unsubscribeMessage();
      unsubscribeConversation();
      unsubscribePresence();
    };
  }, [user?.id, onMessageReceived, onConversationUpdated, onUserPresenceUpdate, updateConversationOnlineStatus]);

  return {
    conversations,
    currentConversation,
    messages,
    loading,
    messagesLoading,
    sendingMessage,
    error,
    messagesError,
    isConnected,
    cacheState,
    selectConversation,
    sendMessage,
    createConversation,
    deleteConversation,
    clearConversationsCache,
    clearAllMessageCaches,
    clearMessageCache
  };
};

export default useMessages;