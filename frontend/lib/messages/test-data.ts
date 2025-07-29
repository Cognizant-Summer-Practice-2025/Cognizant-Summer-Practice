// Test data utilities for development and testing


import { MessageResponse, ConversationResponse } from './api';

// Mock conversations for development
export const mockConversations: ConversationResponse[] = [
  {
    id: "conv-1",
    otherUserId: "user-2",
    otherUserName: "Sarah Wilson",
    otherUserAvatar: "https://placehold.co/40x40",
    lastMessage: {
      id: "msg-5",
      senderId: "user-2",
      receiverId: "current-user",
      content: "Thanks for checking out my portfolio!",
      createdAt: new Date(Date.now() - 2 * 60 * 1000).toISOString(), // 2 minutes ago
      conversationId: "conv-1",
      isRead: false
    },
    unreadCount: 1,
    updatedAt: new Date(Date.now() - 2 * 60 * 1000).toISOString(),
    isOnline: true
  },
  {
    id: "conv-2", 
    otherUserId: "user-3",
    otherUserName: "Mike Chen",
    otherUserAvatar: "https://placehold.co/40x40",
    lastMessage: {
      id: "msg-8",
      senderId: "user-3",
      receiverId: "current-user", 
      content: "Great work on the ML project! 🔥",
      createdAt: new Date(Date.now() - 60 * 60 * 1000).toISOString(), // 1 hour ago
      conversationId: "conv-2",
      isRead: true
    },
    unreadCount: 0,
    updatedAt: new Date(Date.now() - 60 * 60 * 1000).toISOString(),
    isOnline: true
  },
  {
    id: "conv-3",
    otherUserId: "user-4", 
    otherUserName: "Alex Johnson",
    otherUserAvatar: "https://placehold.co/40x40",
    lastMessage: {
      id: "msg-11",
      senderId: "current-user",
      receiverId: "user-4",
      content: "That sounds interesting! What kind of startup are you thinking about?",
      createdAt: new Date(Date.now() - 3 * 60 * 60 * 1000).toISOString(), // 3 hours ago
      conversationId: "conv-3",
      isRead: true
    },
    unreadCount: 0,
    updatedAt: new Date(Date.now() - 3 * 60 * 60 * 1000).toISOString(),
    isOnline: false
  }
];

// Mock messages for each conversation
export const mockMessages: Record<string, MessageResponse[]> = {
  "conv-1": [
    {
      id: "msg-1",
      senderId: "user-2",
      receiverId: "current-user",
      content: "Hi! I saw your portfolio and I'm really impressed with your full-stack projects. The e-commerce platform looks amazing!",
      createdAt: new Date(Date.now() - 24 * 60 * 60 * 1000).toISOString(), // Yesterday
      conversationId: "conv-1",
      isRead: true
    },
    {
      id: "msg-2", 
      senderId: "current-user",
      receiverId: "user-2",
      content: "Thank you so much! I really appreciate the feedback. I checked out your design portfolio too - your UI work is incredible!",
      createdAt: new Date(Date.now() - 23 * 60 * 60 * 1000).toISOString(),
      conversationId: "conv-1",
      isRead: true
    },
    {
      id: "msg-3",
      senderId: "user-2", 
      receiverId: "current-user",
      content: "Thanks! I was wondering if you'd be interested in collaborating on a project? I have a client who needs both design and development work.",
      createdAt: new Date(Date.now() - 4 * 60 * 60 * 1000).toISOString(), // 4 hours ago
      conversationId: "conv-1",
      isRead: true
    },
    {
      id: "msg-4",
      senderId: "current-user",
      receiverId: "user-2", 
      content: "That sounds really interesting! I'd love to hear more about it. What kind of project is it?",
      createdAt: new Date(Date.now() - 3 * 60 * 60 * 1000).toISOString(),
      conversationId: "conv-1", 
      isRead: true
    },
    {
      id: "msg-5",
      senderId: "user-2",
      receiverId: "current-user",
      content: "Thanks for checking out my portfolio!",
      createdAt: new Date(Date.now() - 2 * 60 * 1000).toISOString(),
      conversationId: "conv-1",
      isRead: false
    }
  ],
  "conv-2": [
    {
      id: "msg-6",
      senderId: "user-3",
      receiverId: "current-user", 
      content: "Hey! Just saw your latest ML project on GitHub. The neural network implementation is really clean!",
      createdAt: new Date(Date.now() - 5 * 60 * 60 * 1000).toISOString(),
      conversationId: "conv-2",
      isRead: true
    },
    {
      id: "msg-7",
      senderId: "current-user",
      receiverId: "user-3",
      content: "Thanks Mike! I spent a lot of time optimizing the training pipeline. How's your computer vision project going?", 
      createdAt: new Date(Date.now() - 4 * 60 * 60 * 1000).toISOString(),
      conversationId: "conv-2",
      isRead: true
    },
    {
      id: "msg-8",
      senderId: "user-3",
      receiverId: "current-user",
      content: "Great work on the ML project! 🔥",
      createdAt: new Date(Date.now() - 60 * 60 * 1000).toISOString(),
      conversationId: "conv-2", 
      isRead: true
    }
  ],
  "conv-3": [
    {
      id: "msg-9",
      senderId: "user-4",
      receiverId: "current-user",
      content: "Would love to collaborate on a project! I have some ideas for a new startup.",
      createdAt: new Date(Date.now() - 6 * 60 * 60 * 1000).toISOString(),
      conversationId: "conv-3",
      isRead: true
    },
    {
      id: "msg-10",
      senderId: "current-user", 
      receiverId: "user-4",
      content: "That sounds interesting! What kind of startup are you thinking about?",
      createdAt: new Date(Date.now() - 3 * 60 * 60 * 1000).toISOString(),
      conversationId: "conv-3",
      isRead: true
    }
  ]
};

// Helper function to get mock data (useful during development)
export function getMockConversations(): ConversationResponse[] {
  return mockConversations;
}

export function getMockMessages(conversationId: string): MessageResponse[] {
  return mockMessages[conversationId] || [];
} 