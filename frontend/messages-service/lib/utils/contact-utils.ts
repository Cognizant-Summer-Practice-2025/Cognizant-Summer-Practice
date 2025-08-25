import { formatTimestamp, getValidTimestamp, truncateMessage } from './message-utils';

export interface Contact {
  id: string;
  name: string;
  avatar: string;
  lastMessage: string;
  timestamp: string;
  isActive?: boolean;
  isOnline?: boolean;
  unreadCount?: number;
  userId?: string;
  professionalTitle?: string;
}

export interface EnhancedContactData {
  name?: string;
  isOnline?: boolean;
  professionalTitle?: string;
}

export interface ConversationData {
  id: string;
  otherUserName: string;
  otherUserId: string;
  otherUserAvatar: string;
  lastMessage?: {
    content: string;
    createdAt: string;
  };
  lastMessageTimestamp?: string;
  updatedAt: string;
  createdAt: string;
  isOnline?: boolean;
  unreadCount?: number;
}

export const createEnhancedContact = (
  conversation: ConversationData,
  enhancedData?: EnhancedContactData,
  currentConversationId?: string
): Contact => {
  const timestamp = getValidTimestamp(
    conversation.lastMessageTimestamp,
    conversation.lastMessage?.createdAt,
    conversation.updatedAt,
    conversation.createdAt
  );
  
  return {
    id: conversation.id,
    name: enhancedData?.name || conversation.otherUserName,
    avatar: conversation.otherUserAvatar,
    lastMessage: truncateMessage(conversation.lastMessage?.content || 'No messages yet'),
    timestamp: formatTimestamp(timestamp),
    isActive: conversation.id === currentConversationId,
    isOnline: enhancedData?.isOnline ?? conversation.isOnline ?? false,
    unreadCount: conversation.unreadCount || 0,
    userId: conversation.otherUserId,
    professionalTitle: enhancedData?.professionalTitle || 'Professional'
  };
};

export const saveEnhancedContactsToStorage = (enhancedContacts: Map<string, Partial<Contact>>): void => {
  if (typeof window !== 'undefined') {
    try {
      const contactsObject = Object.fromEntries(enhancedContacts);
      localStorage.setItem('enhancedContacts', JSON.stringify(contactsObject));
    } catch {
      // Failed to save enhanced contacts to localStorage - continue silently
    }
  }
};

export const loadEnhancedContactsFromStorage = (): Map<string, Partial<Contact>> => {
  if (typeof window !== 'undefined') {
    try {
      const saved = localStorage.getItem('enhancedContacts');
      if (saved) {
        const parsed = JSON.parse(saved);
        return new Map(Object.entries(parsed));
      }
    } catch {
      // Failed to load from localStorage - return empty map
    }
  }
  return new Map();
}; 