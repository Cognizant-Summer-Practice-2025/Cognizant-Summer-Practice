import { useState, useEffect, useCallback, useRef } from "react";
import { useUser } from "../contexts/user-context";
import { SearchUser } from "../user";
import { messagesApi, ApiMessage } from "./api";
import { MessageEncryption } from "../encryption";
import { useWebSocket } from "../contexts/websocket-context";
import { authenticatedClient } from "../authenticated-client";

// Helper function to safely decrypt messages
const safeDecrypt = (content: string, senderId: string): string => {
  if (!content || !senderId) {
    return content || '';
  }

  try {
    return MessageEncryption.decrypt(content, senderId);
  } catch (error) {
    console.warn('Failed to decrypt message, returning original content:', error);
    return content;
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
  lastMessageTimestamp: string;
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
  const { onMessageReceived, onConversationUpdated, onUserPresenceUpdate, onMessageReadReceipt, onMessageDeleted, markMessageAsRead, deleteMessage: deleteMessageViaWebSocket, isConnected } = useWebSocket();
  
  // Initialize conversations from cache if available
  const [conversations, setConversations] = useState<Conversation[]>(() => {
    if (typeof window !== 'undefined' && user?.id) {
      try {
        const cacheKey = `conversations_${user.id}`;
        const cached = localStorage.getItem(cacheKey);
        const cacheTimestamp = localStorage.getItem(`${cacheKey}_timestamp`);
        
        const CACHE_MAX_AGE = 24 * 60 * 60 * 1000; 
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
          const MESSAGE_CACHE_MAX_AGE = 60 * 60 * 1000; 
          const cacheAge = Date.now() - parseInt(cacheTimestamp);
          
          if (cacheAge > MESSAGE_CACHE_MAX_AGE) {
            console.log(`Message cache for conversation ${conversationId} is too old, removing`);
            localStorage.removeItem(cacheKey);
            localStorage.removeItem(`${cacheKey}_timestamp`);
            return null;
          }

          return JSON.parse(cached);
        }
      } catch (error) {
        console.warn(`Failed to load messages from cache for conversation ${conversationId}:`, error);
      }
    }
    return null;
  }, [user?.id]);

  const clearMessageCache = useCallback((conversationId: string) => {
    if (typeof window !== 'undefined' && user?.id) {
      try {
        const cacheKey = `messages_${user.id}_${conversationId}`;
        localStorage.removeItem(cacheKey);
        localStorage.removeItem(`${cacheKey}_timestamp`);
        
      } catch (error) {
        console.warn(`Failed to clear message cache for conversation ${conversationId}:`, error);
      }
    }
  }, [user?.id]);

  const cacheConversationsRef = useRef(cacheConversations);
  useEffect(() => {
    cacheConversationsRef.current = cacheConversations;
  }, [cacheConversations]);

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
              if (key.startsWith('conversations_') && key !== currentUserConversationsKey) {
                const timestampKey = `${key}_timestamp`;
                const timestamp = localStorage.getItem(timestampKey);
                
                if (timestamp) {
                  const age = Date.now() - parseInt(timestamp);
                  const CLEANUP_AGE = 7 * 24 * 60 * 60 * 1000; 
                  
                  if (age > CLEANUP_AGE) {
                    keysToRemove.push(key, timestampKey);
                  }
                } else {
                  keysToRemove.push(key);
                }
              }
              
              if (key.startsWith('messages_')) {
                const timestampKey = `${key}_timestamp`;
                const timestamp = localStorage.getItem(timestampKey);
                
                if (timestamp) {
                  const age = Date.now() - parseInt(timestamp);
                  const MESSAGE_CLEANUP_AGE = 3 * 24 * 60 * 60 * 1000; 
                  
                  if (age > MESSAGE_CLEANUP_AGE) {
                    keysToRemove.push(key, timestampKey);
                  }
                } else if (!key.startsWith(currentUserPrefix)) {
                  keysToRemove.push(key);
                }
              }
            }
          }
          
          keysToRemove.forEach(key => localStorage.removeItem(key));
        } catch (error) {
          console.warn('Failed to cleanup old caches:', error);
        }
      };
      
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
      } catch {}
    }
  }, [user?.id]);

  // Function to check online status for a specific user 
  const checkUserOnlineStatus = useCallback(async (userId: string) => {
    try {
      const statusResponse = await messagesApi.getUserOnlineStatus(userId);
      return statusResponse.isOnline;
    } catch {
      return false;
    }
  }, []);

  // Update online status for conversations when presence changes
  const updateConversationOnlineStatus = useCallback((userId: string, isOnline: boolean) => {
    
    setConversations(prev => {
      const updated = prev.map(conv => {
        if (conv.otherUserId === userId) {
          return { ...conv, isOnline };
        }
        return conv;
      });
      
      return updated;
    });
    
    setCurrentConversation(prev => {
        if (prev && prev.otherUserId === userId) {
          return { ...prev, isOnline };
        }
      return prev;
    });
    
  }, []);

  const waitForAuthentication = useCallback(async (maxMs: number = 8000): Promise<boolean> => {
    const start = Date.now();
    const USER_API_BASE = process.env.NEXT_PUBLIC_USER_API_URL || 'http://localhost:5200';

    const getAccessToken = async (): Promise<string | null> => {
      try {
        const resp = await fetch('/api/user/get');
        if (!resp.ok) return null;
        const data = await resp.json();
        return data?.accessToken || null;
      } catch {
        return null;
      }
    };

    while (Date.now() - start < maxMs) {
      const token = await getAccessToken();
      if (token) {
        console.debug('[useMessages] waitForAuthentication: got token candidate', `${token.slice(0, 12)}...${token.slice(-6)} (len=${token.length})`);
        // Validate the token against user service to ensure backend will accept it
        try {
          // Validate via authenticated client so we see its debug and consistent header formatting
          await authenticatedClient.get<unknown>(`${USER_API_BASE}/api/oauth/me`);
          console.debug('[useMessages] token validation via AuthClient: OK');
          return true;
        } catch {
          console.debug('[useMessages] token validation via AuthClient failed');
        }
      }
      await new Promise((res) => setTimeout(res, 250));
    }
    return false;
  }, []);

  const loadConversations = useCallback(async (showLoadingSpinner: boolean = true) => {
    if (!user?.id) return;

    if (showLoadingSpinner) {
      setLoading(true);
    } else {
      setCacheState(prev => ({ ...prev, isRefreshing: true }));
    }
    setError(null);

    // Ensure we have an authenticated token before calling APIs
    try {
      
      const becameAuthed = await waitForAuthentication(8000);
      if (!becameAuthed) {
        console.warn('[useMessages] loadConversations: still unauthenticated after wait, skipping fetch');
        return;
      }

      
      const apiConversations = await messagesApi.getUserConversations(user.id);

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
              lastMessageTimestamp: apiConv.lastMessageTimestamp,
              createdAt: apiConv.createdAt,
              updatedAt: apiConv.updatedAt
            };
            
            return conversation;
          } catch (userError) {
            console.error(`Failed to get user details for ${apiConv.otherUserId}:`, userError);

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
              lastMessageTimestamp: apiConv.lastMessageTimestamp,
              createdAt: apiConv.createdAt,
              updatedAt: apiConv.updatedAt
            };
            
            return conversation;
          }
        })
      );
      
      setConversations(enrichedConversations);
      cacheConversations(enrichedConversations);

    } catch (err) {
      console.error('Error loading conversations:', err);
      if (!(err instanceof Error && err.message && err.message.includes('Authentication required'))) {
        setError('Failed to load conversations');
      }
    } finally {
      setLoading(false);
      setCacheState(prev => ({ ...prev, isRefreshing: false }));
    }
  }, [user?.id, cacheConversations, waitForAuthentication]);

  // Load conversations with optimized cache-first strategy
  const loadConversationsWithCache = useCallback(async () => {
    if (!user?.id) return;
    
    const cacheKey = `conversations_${user.id}`;
    const cached = localStorage.getItem(cacheKey);
    const cacheTimestamp = localStorage.getItem(`${cacheKey}_timestamp`);
    const hasCache = conversations.length > 0 || cached;
    
    const CACHE_FRESH_TTL = 2 * 60 * 1000;
    const isCacheFresh = cacheTimestamp && (Date.now() - parseInt(cacheTimestamp) < CACHE_FRESH_TTL);

    if (hasCache) {
      
      setCacheState(prev => ({ ...prev, isFromCache: true }));
      
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
      // Ensure backend-validated auth before loading messages
      
      const becameAuthed = await waitForAuthentication(8000);
      if (!becameAuthed) {
        console.warn('[useMessages] loadMessages: still unauthenticated after wait, skipping fetch');
        return;
      }

      
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
        } catch {}
      }
    } catch (error) {
      console.error('Failed to load messages:', error);
      if (!(error instanceof Error && error.message && error.message.includes('Authentication required'))) {
        setMessagesError('Failed to load messages');
      }
    } finally {
      setMessagesLoading(false);
    }
  }, [user?.id, cacheMessages, waitForAuthentication]);

  // Load messages with cache-first strategy
  const loadMessagesWithCache = useCallback(async (conversationId: string) => {
    if (!user?.id) return;

    // Try to load from cache first
    const cachedMessages = loadMessagesFromCache(conversationId);
    
    if (cachedMessages && cachedMessages.length > 0) {
      
      
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
              
      await loadMessages(conversationId, true); // Show loading spinner
    }
  }, [user?.id, loadMessagesFromCache, loadMessages]);

  const selectConversation = useCallback(async (conversation: Conversation) => {
    setCurrentConversation(conversation);
    loadMessagesWithCache(conversation.id);
    
    // Check online status for the selected user
    
    try {
      const isOnline = await checkUserOnlineStatus(conversation.otherUserId);
      
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
        lastMessageTimestamp: apiConversation.lastMessageTimestamp,
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

    

    try {
      await messagesApi.deleteConversation(conversationId, user.id);
      
      // Remove conversation from state
      setConversations(prev => prev.filter(conv => conv.id !== conversationId));
      cacheConversations(conversations); // Cache the updated conversations
      
      // Clear current conversation if it was the deleted one
      if (currentConversation?.id === conversationId) {
        setCurrentConversation(null);
        setMessages([]);
      }
      
    } catch (error) {
      throw error;
    }
  }, [user?.id, currentConversation?.id, conversations, cacheConversations]);

  const deleteMessage = useCallback(async (messageId: string) => {
    if (!user?.id) {
      console.error('No user ID available for delete message');
      return;
    }

    

    try {
      await deleteMessageViaWebSocket(messageId, user.id);
      
      // Remove message from state
      setMessages(prev => prev.filter(msg => msg.id !== messageId));
      
      // Clear message cache for current conversation since we deleted a message
      if (currentConversation?.id) {
        clearMessageCache(currentConversation.id);
      }
      
      
    } catch (error) {
      throw error;
    }
  }, [user?.id, currentConversation?.id, clearMessageCache, deleteMessageViaWebSocket]);

  const reportMessage = useCallback(async (messageId: string, reason: string) => {
    if (!user?.id) {
      console.error('No user ID available for report message');
      return;
    }

    

    try {
      // Call the report message API
      await messagesApi.reportMessage(messageId, user.id, reason);
    } catch (error) {
      throw error;
    }
  }, [user?.id]);

  const sendMessage = useCallback(async (content: string) => {
    if (!user?.id || !currentConversation) return;
    
    setSendingMessage(true);
    
    try {
      const encryptedContent = MessageEncryption.encrypt(content, user.id);
      
      const apiMessage = await messagesApi.sendMessage({
        conversationId: currentConversation.id,
        senderId: user.id,
        content: encryptedContent,
        messageType: 0 // Text message
      });
      
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
      
      
      
      // Add message to current conversation immediately for instant UI update
      setMessages(prevMessages => {
        // Check if message already exists to avoid duplicates
        const exists = prevMessages.some(msg => msg.id === newMessage.id);
        if (exists) {
          return prevMessages;
        }
        const updatedMessages = [...prevMessages, newMessage];
        
        // Update message cache with the new message
        cacheMessages(currentConversation.id, updatedMessages);
        return updatedMessages;
      });
      
      // Update conversation's last message and move to top
      setConversations(prev => {
        const updatedConversations = prev.map(conv => 
          conv.id === currentConversation.id
            ? { 
                ...conv, 
                lastMessage: newMessage, 
                updatedAt: apiMessage.updatedAt 
              }
            : conv
        )
        .sort((a, b) => new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime());
        
        cacheConversationsRef.current(updatedConversations);
        return updatedConversations;
      });
    
      clearMessageCache(currentConversation.id);
      
      return newMessage;
    } catch {
      return null;
    } finally {
      setSendingMessage(false);
    }
  }, [user?.id, currentConversation, clearMessageCache, cacheMessages]);

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
              
              return prevMessages;
            }
            
            const updatedMessages = [...prevMessages, formattedMessage];
            
            // Update message cache with the new message
            cacheMessages(newMessage.conversationId, updatedMessages);
            
            return updatedMessages;
          });

          // Mark as read if the user is the receiver and viewing this conversation
          if (newMessage.receiverId === user.id) {
            setTimeout(async () => {
              try {
                await markMessageAsRead(newMessage.id, user.id);
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

    // Handle message read receipts
    const unsubscribeReadReceipt = onMessageReadReceipt((receipt) => {
      console.log('Received message read receipt:', receipt);
      
      // Update the specific message as read
      setMessages(prevMessages => {
        const updatedMessages = prevMessages.map(msg => 
          msg.id === receipt.messageId ? { ...msg, isRead: true } : msg
        );
        
        // Cache the updated messages if this is the current conversation
        if (currentConversation?.id === receipt.conversationId) {
          cacheMessages(receipt.conversationId, updatedMessages);
        }
        
        return updatedMessages;
      });
      
      // Update conversation's last message if it was the one that was read
      setConversations(prev => prev.map(conv => {
        if (conv.id === receipt.conversationId && conv.lastMessage?.id === receipt.messageId) {
          return {
            ...conv,
            lastMessage: conv.lastMessage ? {
              ...conv.lastMessage,
              isRead: true
            } : undefined,
            // Reduce unread count for the sender if this was their message
            unreadCount: conv.lastMessage?.senderId === user?.id ? Math.max(0, conv.unreadCount - 1) : conv.unreadCount
          };
        }
        return conv;
      }));
    });

    // Handle message deletion
    const unsubscribeMessageDeleted = onMessageDeleted((deletedMessage) => {
      console.log('Received message deleted event:', deletedMessage);
      setMessages(prevMessages => prevMessages.filter(msg => msg.id !== deletedMessage.messageId));
      clearMessageCache(deletedMessage.conversationId);
      setConversations(prev => prev.map(conv => 
        conv.id === deletedMessage.conversationId ? {
          ...conv,
          unreadCount: conv.lastMessage?.id === deletedMessage.messageId ? Math.max(0, conv.unreadCount - 1) : conv.unreadCount
        } : conv
      ));
    });

    return () => {
      unsubscribeMessage();
      unsubscribeConversation();
      unsubscribePresence();
      unsubscribeReadReceipt();
      unsubscribeMessageDeleted();
    };
  }, [user?.id, onMessageReceived, onConversationUpdated, onUserPresenceUpdate, updateConversationOnlineStatus, onMessageReadReceipt, markMessageAsRead, currentConversation?.id, cacheMessages, onMessageDeleted, clearMessageCache]);

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
    clearMessageCache,
    markMessageAsRead,
    deleteMessage,
    reportMessage
  };
};

export default useMessages;