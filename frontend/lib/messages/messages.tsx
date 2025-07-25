import { useState, useEffect, useCallback } from "react";
import { useUser } from "../contexts/user-context";
import { SearchUser } from "../user";
import { messagesApi, ApiConversation, ApiMessage, ApiUser } from "./api";
import { MessageEncryption } from "../encryption";

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

const useMessages = () => {
  const { user } = useUser();
  const [conversations, setConversations] = useState<Conversation[]>([]);
  const [currentConversation, setCurrentConversation] = useState<Conversation | null>(null);
  const [messages, setMessages] = useState<Message[]>([]);
  const [loading, setLoading] = useState(false);
  const [messagesLoading, setMessagesLoading] = useState(false);
  const [sendingMessage, setSendingMessage] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [messagesError, setMessagesError] = useState<string | null>(null);

  const loadConversations = useCallback(async () => {
    if (!user?.id) return;
    
    setLoading(true);
    setError(null);
    
    try {
      const apiConversations = await messagesApi.getUserConversations(user.id);
      
      // Enrich conversations with user details
      const enrichedConversations = await Promise.all(
        apiConversations.map(async (apiConv: ApiConversation) => {
          try {
            const otherUser: ApiUser = await messagesApi.getUserById(apiConv.otherUserId);
            
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
              isOnline: false, // TODO: Implement online status
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
    } catch (error) {
      console.error('Failed to load conversations:', error);
      setError('Failed to load conversations');
    } finally {
      setLoading(false);
    }
  }, [user?.id]);

  const loadMessages = useCallback(async (conversationId: string) => {
    if (!user?.id) return;
    
    setMessagesLoading(true);
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
  }, [user?.id]);

  const selectConversation = useCallback((conversation: Conversation) => {
    setCurrentConversation(conversation);
    loadMessages(conversation.id);
  }, [loadMessages]);

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
      
      return newConversation;
    } catch (error) {
      console.error('Failed to create conversation:', error);
      return null;
    }
  }, [user?.id]);

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
  }, [user?.id, currentConversation?.id]);

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
      
      // Add message to current conversation
      setMessages(prev => [...prev, newMessage]);
      
      // Update conversation's last message
      setConversations(prev => 
        prev.map(conv => 
          conv.id === currentConversation.id
            ? { ...conv, lastMessage: newMessage, updatedAt: apiMessage.updatedAt }
            : conv
        )
        .sort((a, b) => new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime())
      );
      
      return newMessage;
    } catch (error) {
      console.error('Failed to send message:', error);
      return null;
    } finally {
      setSendingMessage(false);
    }
  }, [user?.id, currentConversation]);

  useEffect(() => {
    if (user?.id) {
      loadConversations();
    }
  }, [user?.id, loadConversations]);

  return {
    conversations,
    currentConversation,
    messages,
    loading,
    messagesLoading,
    sendingMessage,
    error,
    messagesError,
    selectConversation,
    sendMessage,
    createConversation,
    deleteConversation
  };
};

export default useMessages;