import { authenticatedClient } from '@/lib/authenticated-client';

const MESSAGES_API_BASE = process.env.NEXT_PUBLIC_MESSAGES_API_URL || 'http://localhost:5093';

export interface ApiMessage {
  id: string;
  conversationId: string;
  senderId: string;
  receiverId: string;
  content: string;
  messageType: number;
  replyToMessageId?: string;
  isRead: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface ApiConversation {
  id: string;
  otherUserId: string;
  isInitiator: boolean;
  lastMessageTimestamp: string;
  lastMessage?: ApiMessage;
  unreadCount: number;
  createdAt: string;
  updatedAt: string;
  isExisting?: boolean;
}

export interface CreateConversationRequest {
  initiatorId: string;
  receiverId: string;
}

export interface SendMessageRequest {
  conversationId: string;
  senderId: string;
  content: string;
  messageType?: number;
  replyToMessageId?: string;
}

export interface MarkMessagesReadRequest {
  conversationId: string;
  userId: string;
}

export interface MarkSingleMessageReadRequest {
  userId: string;
}

export interface MessagesResponse {
  messages: ApiMessage[];
  pagination: {
    page: number;
    pageSize: number;
    totalItems: number;
    totalPages: number;
    hasNext: boolean;
    hasPrevious: boolean;
  };
}

export interface ApiUser {
  id: string;
  username: string;
  firstName?: string;
  lastName?: string;
  professionalTitle?: string;
  bio?: string;
  location?: string;
  avatarUrl?: string;
  isActive: boolean;
  isAdmin: boolean;
  lastLoginAt?: string;
}

export interface UserOnlineStatus {
  userId: string;
  isOnline: boolean;
  timestamp: string;
}

export const messagesApi = {
  // Conversations
  async getUserConversations(userId: string): Promise<ApiConversation[]> {
    return authenticatedClient.get<ApiConversation[]>(`${MESSAGES_API_BASE}/api/conversations/user/${userId}`);
  },

  async createOrGetConversation(request: CreateConversationRequest): Promise<ApiConversation> {
    return authenticatedClient.post<ApiConversation>(`${MESSAGES_API_BASE}/api/conversations/create`, request);
  },

  async getConversation(conversationId: string): Promise<ApiConversation> {
    return authenticatedClient.get<ApiConversation>(`${MESSAGES_API_BASE}/api/conversations/${conversationId}`);
  },

  // Messages
  async sendMessage(request: SendMessageRequest): Promise<ApiMessage> {
    return authenticatedClient.post<ApiMessage>(`${MESSAGES_API_BASE}/api/messages/send`, request);
  },

  async getConversationMessages(
    conversationId: string,
    page: number = 1,
    pageSize: number = 50
  ): Promise<MessagesResponse> {
    return authenticatedClient.get<MessagesResponse>(
      `${MESSAGES_API_BASE}/api/messages/conversation/${conversationId}?page=${page}&pageSize=${pageSize}`
    );
  },

  async markMessagesAsRead(request: MarkMessagesReadRequest): Promise<{ markedCount: number }> {
    return authenticatedClient.put<{ markedCount: number }>(`${MESSAGES_API_BASE}/api/messages/mark-read`, request);
  },

  async markSingleMessageAsRead(messageId: string, request: MarkSingleMessageReadRequest): Promise<{ messageId: string; isRead: boolean; readAt: string }> {
    return authenticatedClient.put<{ messageId: string; isRead: boolean; readAt: string }>(`${MESSAGES_API_BASE}/api/messages/${messageId}/mark-read`, request);
  },

  async deleteMessage(messageId: string, userId: string): Promise<{ message: string }> {
    return authenticatedClient.delete<{ message: string }>(`${MESSAGES_API_BASE}/api/messages/${messageId}?userId=${userId}`);
  },

  async deleteConversation(conversationId: string, userId: string): Promise<{ message: string }> {
    const url = `${MESSAGES_API_BASE}/api/conversations/${conversationId}?userId=${userId}`;
    console.log('Making DELETE request to:', url);
    
    try {
      const result = await authenticatedClient.delete<{ message: string }>(url);
      console.log('Delete conversation successful:', result);
      return result;
    } catch (error) {
      console.error('Delete conversation failed:', error);
      throw error;
    }
  },

  // User details
  async getUserById(userId: string): Promise<ApiUser> {
    const USER_API_BASE = process.env.NEXT_PUBLIC_USER_API_URL || 'http://localhost:5200';
    return authenticatedClient.get<ApiUser>(`${USER_API_BASE}/api/users/${userId}`);
  },

  // Online status
  async getUserOnlineStatus(userId: string): Promise<UserOnlineStatus> {
    return authenticatedClient.get<UserOnlineStatus>(`${MESSAGES_API_BASE}/api/users/${userId}/online-status`);
  },

  // Report message
  async reportMessage(messageId: string, reportedByUserId: string, reason: string): Promise<{ message: string; reportId: string; reportedAt: string }> {
    return authenticatedClient.post<{ message: string; reportId: string; reportedAt: string }>(
      `${MESSAGES_API_BASE}/api/messages/${messageId}/report`, 
      {
        reportedByUserId,
        reason,
      }
    );
  },
}; 