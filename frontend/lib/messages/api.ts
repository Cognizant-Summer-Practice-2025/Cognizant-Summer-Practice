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

export const messagesApi = {
  // Conversations
  async getUserConversations(userId: string): Promise<ApiConversation[]> {
    const response = await fetch(`${MESSAGES_API_BASE}/api/conversations/user/${userId}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error(`Failed to get user conversations: ${response.statusText}`);
    }

    return response.json();
  },

  async createOrGetConversation(request: CreateConversationRequest): Promise<ApiConversation> {
    const response = await fetch(`${MESSAGES_API_BASE}/api/conversations/create`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      throw new Error(`Failed to create conversation: ${response.statusText}`);
    }

    return response.json();
  },

  async getConversation(conversationId: string): Promise<ApiConversation> {
    const response = await fetch(`${MESSAGES_API_BASE}/api/conversations/${conversationId}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error(`Failed to get conversation: ${response.statusText}`);
    }

    return response.json();
  },

  // Messages
  async sendMessage(request: SendMessageRequest): Promise<ApiMessage> {
    const response = await fetch(`${MESSAGES_API_BASE}/api/messages/send`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      throw new Error(`Failed to send message: ${response.statusText}`);
    }

    return response.json();
  },

  async getConversationMessages(
    conversationId: string,
    page: number = 1,
    pageSize: number = 50
  ): Promise<MessagesResponse> {
    const response = await fetch(
      `${MESSAGES_API_BASE}/api/messages/conversation/${conversationId}?page=${page}&pageSize=${pageSize}`,
      {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
      }
    );

    if (!response.ok) {
      throw new Error(`Failed to get conversation messages: ${response.statusText}`);
    }

    return response.json();
  },

  async markMessagesAsRead(request: MarkMessagesReadRequest): Promise<{ markedCount: number }> {
    const response = await fetch(`${MESSAGES_API_BASE}/api/messages/mark-read`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      throw new Error(`Failed to mark messages as read: ${response.statusText}`);
    }

    return response.json();
  },

  async deleteMessage(messageId: string, userId: string): Promise<{ message: string }> {
    const response = await fetch(`${MESSAGES_API_BASE}/api/messages/${messageId}?userId=${userId}`, {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error(`Failed to delete message: ${response.statusText}`);
    }

    return response.json();
  },

  async deleteConversation(conversationId: string, userId: string): Promise<{ message: string }> {
    const url = `${MESSAGES_API_BASE}/api/conversations/${conversationId}?userId=${userId}`;
    console.log('Making DELETE request to:', url);
    
    const response = await fetch(url, {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    console.log('Delete conversation response status:', response.status);
    console.log('Delete conversation response headers:', Object.fromEntries(response.headers.entries()));

    if (!response.ok) {
      const errorText = await response.text();
      console.error('Delete conversation failed:', errorText);
      throw new Error(`Failed to delete conversation: ${response.statusText}`);
    }

    const result = await response.json();
    console.log('Delete conversation successful:', result);
    return result;
  },

  // User details
  async getUserById(userId: string): Promise<ApiUser> {
    const USER_API_BASE = process.env.NEXT_PUBLIC_USER_API_URL || 'http://localhost:5200';
    const response = await fetch(`${USER_API_BASE}/api/users/${userId}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error(`Failed to get user: ${response.statusText}`);
    }

    return response.json();
  },
}; 