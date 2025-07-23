// Message types and interfaces

export interface Message {
  id: string;
  senderId: string;
  receiverId?: string;
  conversationId?: string;
  content: string;
  createdAt: string;
  isRead?: boolean;
  messageType?: string;
}

export interface Conversation {
  id: string;
  otherUserId: string;
  otherUserName: string;
  otherUserAvatar?: string;
  otherUserProfessionalTitle?: string;
  lastMessage?: Message;
  unreadCount: number;
  updatedAt: string;
  isOnline?: boolean;
}

export interface Contact {
  id: string;
  name: string;
  avatar: string;
  lastMessage: string;
  timestamp: string;
  isActive?: boolean;
  isOnline: boolean;
  unreadCount?: number;
}

export interface ChatMessage {
  id: string;
  sender: "user" | "other";
  text: string;
  timestamp: string;
  status: "read" | "delivered" | "sent";
}

export interface UserForMessaging {
  id: string;
  name: string;
  avatar?: string;
  professionalTitle?: string;
  isOnline?: boolean;
}

export interface Notification {
  id: string;
  userId: string;
  message: string;
  isRead: boolean;
  createdAt: string;
} 