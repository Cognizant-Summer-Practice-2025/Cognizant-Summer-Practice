// Messages API functions
// This file contains API calls for messaging operations

const API_BASE_URL = 'http://localhost:5093'; // Messages service URL

// Request/Response interfaces
export interface SendMessageRequest {
  senderId: string;
  receiverId: string;
  conversationId?: string;
  content: string;
}

export interface MessageResponse {
  id: string;
  senderId: string;
  receiverId: string;
  content: string;
  createdAt: string;
  conversationId?: string;
  isRead?: boolean;
}

export interface ConversationResponse {
  id: string;
  otherUserId: string;
  otherUserName: string;
  otherUserAvatar?: string;
  otherUserProfessionalTitle?: string;
  lastMessage?: MessageResponse;
  unreadCount: number;
  updatedAt: string;
  isOnline?: boolean;
}

export interface CreateConversationRequest {
  user1Id: string;
  user2Id: string;
}

export interface NotificationResponse {
  id: string;
  userId: string;
  message: string;
  isRead: boolean;
  createdAt: string;
}

// API error handling
async function handleApiResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(`API Error ${response.status}: ${errorText}`);
  }
  return await response.json();
}

// Message API functions
export const messageApi = {
  /**
   * Send a message
   */
  async sendMessage(request: SendMessageRequest): Promise<MessageResponse> {
    try {
      const response = await fetch(`${API_BASE_URL}/api/Message/send`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(request),
      });

      return await handleApiResponse<MessageResponse>(response);
    } catch (error) {
      console.error('Error sending message:', error);
      throw error;
    }
  },

  /**
   * Get messages for a conversation
   */
  async getMessages(conversationId: string): Promise<MessageResponse[]> {
    try {
      const response = await fetch(`${API_BASE_URL}/api/Message/${conversationId}`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
      });

      return await handleApiResponse<MessageResponse[]>(response);
    } catch (error) {
      console.error('Error fetching messages:', error);
      throw error;
    }
  }
};

// Conversation API functions
export const conversationApi = {
  /**
   * Create a new conversation
   */
  async createConversation(request: CreateConversationRequest): Promise<ConversationResponse> {
    try {
      const response = await fetch(`${API_BASE_URL}/api/Conversation`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(request),
      });

      return await handleApiResponse<ConversationResponse>(response);
    } catch (error) {
      console.error('Error creating conversation:', error);
      throw error;
    }
  },

  /**
   * Get conversation history for a user
   */
  async getConversationHistory(userId: string): Promise<ConversationResponse[]> {
    try {
      const response = await fetch(`${API_BASE_URL}/api/Conversation/user/${userId}`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
      });

      return await handleApiResponse<ConversationResponse[]>(response);
    } catch (error) {
      console.error('Error fetching conversation history:', error);
      throw error;
    }
  },

  /**
   * Get messages for a specific conversation
   */
  async getConversation(conversationId: string): Promise<MessageResponse[]> {
    try {
      const response = await fetch(`${API_BASE_URL}/api/Conversation/${conversationId}`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
      });

      return await handleApiResponse<MessageResponse[]>(response);
    } catch (error) {
      console.error('Error fetching conversation:', error);
      throw error;
    }
  }
};

// Notification API functions
export const notificationApi = {
  /**
   * Get notifications for a user
   */
  async getNotifications(userId: string): Promise<NotificationResponse[]> {
    try {
      const response = await fetch(`${API_BASE_URL}/api/Notification/${userId}`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
      });

      return await handleApiResponse<NotificationResponse[]>(response);
    } catch (error) {
      console.error('Error fetching notifications:', error);
      throw error;
    }
  }
}; 