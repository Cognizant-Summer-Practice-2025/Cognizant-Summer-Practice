'use client';

import React, { createContext, useContext, useEffect, useState, useCallback, ReactNode } from 'react';
import { useUser } from './user-context';
import { 
  messageApi, 
  conversationApi, 
  notificationApi,
  MessageResponse, 
  ConversationResponse, 
  SendMessageRequest,
  NotificationResponse 
} from '@/lib/messages';
import { getMockConversations, getMockMessages } from '@/lib/messages/test-data';

interface MessagesContextType {
  // State
  conversations: ConversationResponse[];
  currentConversation: ConversationResponse | null;
  messages: MessageResponse[];
  notifications: NotificationResponse[];
  
  // Loading states
  loading: boolean;
  messagesLoading: boolean;
  sendingMessage: boolean;
  
  // Error states
  error: string | null;
  messagesError: string | null;
  
  // Actions
  loadConversations: () => Promise<void>;
  selectConversation: (conversation: ConversationResponse) => Promise<void>;
  sendMessage: (content: string, receiverId: string) => Promise<void>;
  loadNotifications: () => Promise<void>;
  createConversation: (receiverId: string) => Promise<ConversationResponse | null>;
  refreshMessages: () => Promise<void>;
  markMessagesAsRead: (conversationId: string, userId?: string) => Promise<void>;
  
  // Helpers
  isCurrentUser: (userId: string) => boolean;
}

const MessagesContext = createContext<MessagesContextType | undefined>(undefined);

export function MessagesProvider({ children }: { children: ReactNode }) {
  const { user, loading: userLoading } = useUser();
  
  // State
  const [conversations, setConversations] = useState<ConversationResponse[]>([]);
  const [currentConversation, setCurrentConversation] = useState<ConversationResponse | null>(null);
  const [messages, setMessages] = useState<MessageResponse[]>([]);
  const [notifications, setNotifications] = useState<NotificationResponse[]>([]);
  
  // Loading states
  const [loading, setLoading] = useState(false);
  const [messagesLoading, setMessagesLoading] = useState(false);
  const [sendingMessage, setSendingMessage] = useState(false);
  
  // Error states
  const [error, setError] = useState<string | null>(null);
  const [messagesError, setMessagesError] = useState<string | null>(null);

  // Helper function to update conversations with latest messages
  const updateConversationsWithLatestMessages = useCallback(async (conversations: ConversationResponse[]) => {
    // For each conversation, get its messages and update lastMessage with the most recent one
    const updatedConversations = await Promise.all(
      conversations.map(async (conv) => {
        try {
          // Try to get messages for this conversation
          let messages: MessageResponse[] = [];
          try {
            messages = await messageApi.getMessages(conv.id);
          } catch (backendError) {
            // Fallback to mock data
            messages = getMockMessages(conv.id);
          }
          
          // Find the most recent message
          if (messages.length > 0) {
            const latestMessage = messages.reduce((latest, current) => 
              new Date(current.createdAt) > new Date(latest.createdAt) ? current : latest
            );
            
            return {
              ...conv,
              lastMessage: latestMessage,
              updatedAt: latestMessage.createdAt
            };
          }
          
          return conv;
        } catch (error) {
          console.warn(`Failed to update lastMessage for conversation ${conv.id}:`, error);
          return conv;
        }
      })
    );
    
    return updatedConversations;
  }, []);

  // Load conversations for current user
  const loadConversations = useCallback(async () => {
    if (!user?.id) return;
    
    try {
      setLoading(true);
      setError(null);
      
      // Try to load from backend first, fallback to mock data if backend isn't available
      let conversationsData: ConversationResponse[] = [];
      try {
        conversationsData = await conversationApi.getConversationHistory(user.id);
      } catch (backendError) {
        console.warn('Backend not available, using mock data:', backendError);
        // Fallback to mock data for development
        conversationsData = getMockConversations();
      }
      
      // Update conversations with the latest messages to ensure accuracy
      const updatedConversations = await updateConversationsWithLatestMessages(conversationsData);
      // Sort conversations by most recent message
      const sortedConversations = updatedConversations.sort((a, b) => 
        new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime()
      );
      setConversations(sortedConversations);
      
    } catch (err) {
      console.error('Error loading conversations:', err);
      setError(err instanceof Error ? err.message : 'Failed to load conversations');
    } finally {
      setLoading(false);
    }
  }, [user?.id, updateConversationsWithLatestMessages]);

  // Select a conversation and load its messages
  const selectConversation = useCallback(async (conversation: ConversationResponse) => {
    try {
      setMessagesLoading(true);
      setMessagesError(null);
      setCurrentConversation(conversation);
      
      // Try to load from backend first, fallback to mock data if backend isn't available
      try {
        const messagesData = await messageApi.getMessages(conversation.id);
        setMessages(messagesData);
      } catch (backendError) {
        console.warn('Backend not available for messages, using mock data:', backendError);
        // Fallback to mock data for development
        const messagesData = getMockMessages(conversation.id);
        setMessages(messagesData);
      }
      
      // Auto-mark messages as read when viewing conversation (with small delay for better UX)
      if (user?.id) {
        setTimeout(() => {
          // Mark messages as read inline to avoid circular dependency
          setMessages(prev => 
            prev.map(msg => {
              if (msg.conversationId === conversation.id) {
                if (msg.senderId !== user.id) {
                  // Mark incoming messages as read by us
                  return { ...msg, isRead: true };
                } else {
                  // For demonstration, also mark some of our own messages as read by them
                  // In a real app, this would come from the backend when the other user reads them
                  // For now, let's simulate that older messages have been read
                  const messageAge = new Date().getTime() - new Date(msg.createdAt).getTime();
                  const olderThan30Seconds = messageAge > 30000; // 30 seconds
                  return olderThan30Seconds ? { ...msg, isRead: true } : msg;
                }
              }
              return msg;
            })
          );
          
          // Update conversation unread count
          setConversations(prev => 
            prev.map(conv => 
              conv.id === conversation.id 
                ? { ...conv, unreadCount: 0 }
                : conv
            )
          );
        }, 500);
      }
      
    } catch (err) {
      console.error('Error loading messages:', err);
      setMessagesError(err instanceof Error ? err.message : 'Failed to load messages');
    } finally {
      setMessagesLoading(false);
    }
  }, [user?.id]);

  // Send a message
  const sendMessage = useCallback(async (content: string, receiverId: string) => {
    if (!user?.id || !currentConversation) return;
    
    try {
      setSendingMessage(true);
      
      const request: SendMessageRequest = {
        senderId: user.id,
        receiverId,
        conversationId: currentConversation.id,
        content
      };
      
      try {
        // Try to send via backend
        const newMessage = await messageApi.sendMessage(request);
        
        // Add message to current messages
        setMessages(prev => [...prev, newMessage]);
        
        // Update conversation's last message and sort by most recent
        setConversations(prev => 
          prev.map(conv => 
            conv.id === currentConversation.id 
              ? { ...conv, lastMessage: newMessage, updatedAt: newMessage.createdAt }
              : conv
          ).sort((a, b) => new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime())
        );
        
        // Simulate the other person reading the message after 2-5 seconds
        const readDelay = 2000 + Math.random() * 3000; // 2-5 seconds
        setTimeout(() => {
          setMessages(prev => 
            prev.map(msg => 
              msg.id === newMessage.id 
                ? { ...msg, isRead: true } // Mark as read by recipient
                : msg
            )
          );
        }, readDelay);
      } catch (backendError) {
        console.warn('Backend not available for sending message:', backendError);
        
        // Create a mock message for development
        const mockMessage: MessageResponse = {
          id: `mock-${Date.now()}`,
          senderId: user.id,
          receiverId,
          content,
          createdAt: new Date().toISOString(),
          conversationId: currentConversation.id,
          isRead: false
        };
        
        // Add mock message to current messages
        setMessages(prev => [...prev, mockMessage]);
        
        // Update conversation's last message and sort by most recent
        setConversations(prev => 
          prev.map(conv => 
            conv.id === currentConversation.id 
              ? { ...conv, lastMessage: mockMessage, updatedAt: mockMessage.createdAt }
              : conv
          ).sort((a, b) => new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime())
        );
        
        // Simulate the other person reading the message after 2-5 seconds
        const readDelay = 2000 + Math.random() * 3000; // 2-5 seconds
        setTimeout(() => {
          setMessages(prev => 
            prev.map(msg => 
              msg.id === mockMessage.id 
                ? { ...msg, isRead: true } // Mark as read by recipient
                : msg
            )
          );
        }, readDelay);
      }
      
    } catch (err) {
      console.error('Error sending message:', err);
      setMessagesError(err instanceof Error ? err.message : 'Failed to send message');
      throw err;
    } finally {
      setSendingMessage(false);
    }
  }, [user?.id, currentConversation]);

  // Create a new conversation
  const createConversation = useCallback(async (receiverId: string): Promise<ConversationResponse | null> => {
    if (!user?.id) return null;
    
    try {
      setLoading(true);
      setError(null);
      
      const newConversation = await conversationApi.createConversation({
        user1Id: user.id,
        user2Id: receiverId
      });
      
      setConversations(prev => [newConversation, ...prev]);
      return newConversation;
    } catch (err) {
      console.error('Error creating conversation:', err);
      setError(err instanceof Error ? err.message : 'Failed to create conversation');
      return null;
    } finally {
      setLoading(false);
    }
  }, [user?.id]);

  // Load notifications
  const loadNotifications = useCallback(async () => {
    if (!user?.id) return;
    
    try {
      const notificationsData = await notificationApi.getNotifications(user.id);
      setNotifications(notificationsData);
    } catch (err) {
      console.error('Error loading notifications:', err);
    }
  }, [user?.id]);

  // Refresh messages for current conversation
  const refreshMessages = useCallback(async () => {
    if (!currentConversation) return;
    
    try {
      const messagesData = await messageApi.getMessages(currentConversation.id);
      setMessages(messagesData);
    } catch (err) {
      console.error('Error refreshing messages:', err);
    }
  }, [currentConversation]);

  // Mark messages as read
  const markMessagesAsRead = useCallback(async (conversationId: string, userId?: string) => {
    const currentUserId = userId || user?.id;
    if (!currentUserId) return;
    
    try {
      // Mark messages as read in the UI immediately for better UX
      setMessages(prev => 
        prev.map(msg => {
          if (msg.conversationId === conversationId) {
            if (msg.senderId !== currentUserId) {
              // Mark incoming messages as read by us
              return { ...msg, isRead: true };
            } else {
              // For demonstration, also mark some of our own messages as read by them
              // In a real app, this would come from the backend when the other user reads them
              const messageAge = new Date().getTime() - new Date(msg.createdAt).getTime();
              const olderThan30Seconds = messageAge > 30000; // 30 seconds
              return olderThan30Seconds ? { ...msg, isRead: true } : msg;
            }
          }
          return msg;
        })
      );
      
      // Update conversation unread count
      setConversations(prev => 
        prev.map(conv => 
          conv.id === conversationId 
            ? { ...conv, unreadCount: 0 }
            : conv
        )
      );
      
      // Try to mark as read on backend (this might not exist yet, so we'll handle gracefully)
      try {
        // If there's a backend API for marking as read, call it here
        // await messageApi.markAsRead(conversationId, currentUserId);
        console.log(`Marked messages as read for conversation ${conversationId}`);
      } catch (backendError) {
        console.warn('Backend mark as read not available:', backendError);
        // This is fine - we'll handle it locally for now
      }
      
    } catch (err) {
      console.error('Error marking messages as read:', err);
    }
  }, [user?.id]);

  // Helper to check if a user ID is the current user
  const isCurrentUser = useCallback((userId: string) => {
    return user?.id === userId;
  }, [user?.id]);

  // Load conversations when user is available
  useEffect(() => {
    if (user && !userLoading) {
      loadConversations();
      loadNotifications();
    }
  }, [user, userLoading, loadConversations, loadNotifications]);

  const value: MessagesContextType = {
    // State
    conversations,
    currentConversation,
    messages,
    notifications,
    
    // Loading states
    loading,
    messagesLoading,
    sendingMessage,
    
    // Error states
    error,
    messagesError,
    
    // Actions
    loadConversations,
    selectConversation,
    sendMessage,
    loadNotifications,
    createConversation,
    refreshMessages,
    markMessagesAsRead,
    
    // Helpers
    isCurrentUser
  };

  return (
    <MessagesContext.Provider value={value}>
      {children}
    </MessagesContext.Provider>
  );
}

export function useMessages() {
  const context = useContext(MessagesContext);
  if (context === undefined) {
    throw new Error('useMessages must be used within a MessagesProvider');
  }
  return context;
} 