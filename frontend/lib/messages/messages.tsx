import { useState, useEffect, useCallback } from "react";
import { useUser } from "../contexts/user-context";
import { SearchUser } from "../user";

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

  const loadConversations = useCallback(() => {
    if (!user?.id) return;
    
    setLoading(true);
    setError(null);
    
    // TODO: Replace with real API call to backend-messages service
    // For now, just set empty conversations
    setTimeout(() => {
      setConversations([]);
      setLoading(false);
    }, 500);
  }, [user?.id]);

  const loadMessages = useCallback((conversationId: string) => {
    if (!user?.id) return;
    
    setMessagesLoading(true);
    setMessagesError(null);
    
    // TODO: Replace with real API call to backend-messages service
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    setTimeout(() => {
      setMessages([]);
      setMessagesLoading(false);
    }, 300);
  }, [user?.id]);

  const selectConversation = useCallback((conversation: Conversation) => {
    setCurrentConversation(conversation);
    loadMessages(conversation.id);
  }, [loadMessages]);

  const createConversation = useCallback(async (searchUser: SearchUser) => {
    if (!user?.id) return null;
    
    // TODO: Replace with real API call to backend-messages service
    // For now, create a conversation structure with real user data
    const newConversation: Conversation = {
      id: `conv-${Date.now()}`,
      otherUserId: searchUser.id,
      otherUserName: searchUser.fullName,
      otherUserAvatar: searchUser.avatarUrl,
      otherUserProfessionalTitle: searchUser.professionalTitle,
      isOnline: false,
      unreadCount: 0,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString()
    };
    
    setConversations(prev => [newConversation, ...prev]);
    
    return newConversation;
  }, [user?.id]);

  const sendMessage = useCallback(async (content: string, receiverId: string) => {
    if (!user?.id || !currentConversation) return;
    
    setSendingMessage(true);
    
    // TODO: Replace with real API call to backend-messages service
    setTimeout(() => {
      const newMessage: Message = {
        id: `msg-${Date.now()}`,
        senderId: user.id!,
        receiverId,
        content,
        isRead: false,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      };
      
      // Add message to current conversation
      setMessages(prev => [...prev, newMessage]);
      
      // Update conversation's last message
      setConversations(prev => 
        prev.map(conv => 
          conv.id === currentConversation.id
            ? { ...conv, lastMessage: newMessage, updatedAt: new Date().toISOString() }
            : conv
        )
        .sort((a, b) => new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime())
      );
      
      setSendingMessage(false);
    }, 500);
    
    return null;
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
    createConversation
  };
};

export default useMessages;