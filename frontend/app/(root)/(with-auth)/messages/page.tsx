"use client";
import React, { useState, useEffect } from "react";
import Sidebar from "@/components/messages-page/sidebar/sidebar";
import Chat from "@/components/messages-page/chat/chat";
import { useMessages } from "@/lib/contexts/messages-context";
import { useUser } from "@/lib/contexts/user-context";
import { SearchUser } from "@/lib/user";
import "./style.css";

interface Contact {
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

interface Message {
  id: string;
  sender: "user" | "other";
  text: string;
  timestamp: string;
  status: "read" | "delivered" | "sent";
}

const MessagesPage = () => {
  const { user } = useUser();
  const { 
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
  } = useMessages();
  
  const [selectedContact, setSelectedContact] = useState<Contact | null>(null);
  
  const [enhancedContacts, setEnhancedContacts] = useState<Map<string, Partial<Contact>>>(() => {
    if (typeof window !== 'undefined') {
      try {
        const stored = localStorage.getItem('enhancedContacts');
        if (stored) {
          const parsed = JSON.parse(stored) as Record<string, Partial<Contact>>;
          return new Map(Object.entries(parsed));
        }
      } catch (error) {
        console.warn('Failed to load enhanced contacts from localStorage:', error);
      }
    }
    return new Map();
  });

  const formatTimestamp = (dateString: string): string => {
    const date = new Date(dateString);
    const now = new Date();
    const diffInHours = Math.floor((now.getTime() - date.getTime()) / (1000 * 60 * 60));
    
    if (diffInHours < 1) {
      const diffInMinutes = Math.floor((now.getTime() - date.getTime()) / (1000 * 60));
      return `${diffInMinutes}m`;
    } else if (diffInHours < 24) {
      return `${diffInHours}h`;
    } else {
      const diffInDays = Math.floor(diffInHours / 24);
      return `${diffInDays}d`;
    }
  };

  const formatMessageTimestamp = (dateString: string): string => {
    const date = new Date(dateString);
    const now = new Date();
    const isToday = date.toDateString() === now.toDateString();
    
    if (isToday) {
      return `Today, ${date.toLocaleTimeString('en-US', { 
        hour: 'numeric', 
        minute: '2-digit',
        hour12: true 
      })}`;
    } else {
      const yesterday = new Date(now);
      yesterday.setDate(yesterday.getDate() - 1);
      const isYesterday = date.toDateString() === yesterday.toDateString();
      
      if (isYesterday) {
        return `Yesterday, ${date.toLocaleTimeString('en-US', { 
          hour: 'numeric', 
          minute: '2-digit',
          hour12: true 
        })}`;
      } else {
        return date.toLocaleDateString('en-US', { 
          month: 'short', 
          day: 'numeric',
          hour: 'numeric', 
          minute: '2-digit',
          hour12: true 
        });
      }
    }
  };

  const currentMessages: Message[] = messages.map(msg => {
    let status: "read" | "delivered" | "sent" = "sent";
    
    if (msg.senderId === user?.id) {
      // For user's own messages, show if they've been read by the other person
      status = msg.isRead ? "read" : "delivered";
    } else {
      // For other person's messages, they're always "delivered" to us
      // (we don't show read status for messages we receive)
      status = "delivered";
    }
    
    return {
      id: msg.id,
      sender: msg.senderId === user?.id ? "user" : "other",
      text: msg.content,
      timestamp: formatMessageTimestamp(msg.createdAt),
      status
    };
  });

  const getEnhancedContact = (conv: typeof conversations[0]): Contact => {
    const enhanced = enhancedContacts.get(conv.id);
    return {
      id: conv.id,
      name: enhanced?.name || conv.otherUserName,
      avatar: enhanced?.avatar || conv.otherUserAvatar || "https://placehold.co/40x40",
      lastMessage: conv.lastMessage?.content || "No messages yet", 
      timestamp: formatTimestamp(conv.updatedAt),
      isActive: currentConversation?.id === conv.id,
      isOnline: enhanced?.isOnline ?? conv.isOnline ?? false,
      unreadCount: conv.unreadCount,
      userId: enhanced?.userId || conv.otherUserId,
      professionalTitle: enhanced?.professionalTitle || conv.otherUserProfessionalTitle
    };
  };

  const contacts: Contact[] = conversations.map(getEnhancedContact);

  useEffect(() => {
    if (typeof window !== 'undefined' && enhancedContacts.size > 0) {
      try {
        const contactsObject = Object.fromEntries(enhancedContacts);
        localStorage.setItem('enhancedContacts', JSON.stringify(contactsObject));
      } catch (error) {
        console.warn('Failed to save enhanced contacts to localStorage:', error);
      }
    }
  }, [enhancedContacts]);

  useEffect(() => {
    if (contacts.length > 0 && !selectedContact) {
      setSelectedContact(contacts[0]);
      const conversation = conversations.find(conv => conv.id === contacts[0].id);
      if (conversation) {
        selectConversation(conversation);
      }
    }
  }, [contacts, selectedContact, conversations, selectConversation]);

  const handleSelectContact = (contact: Contact) => {
    const conversation = conversations.find(conv => conv.id === contact.id);
    if (conversation) {
      setSelectedContact({
        ...contact,
        lastMessage: conversation.lastMessage?.content || "No messages yet", // Use latest message
        timestamp: formatTimestamp(conversation.updatedAt)
      });
      selectConversation(conversation);
    } else {
      setSelectedContact(contact);
    }
  };

  const handleNewConversation = async (searchUser: SearchUser) => {
    if (!user?.id) return;
    
    try {
      const existingConversation = conversations.find(conv => 
        conv.otherUserId === searchUser.id
      );
      
      if (existingConversation) {
        setEnhancedContacts(prev => new Map(prev).set(existingConversation.id, {
          name: searchUser.fullName,
          avatar: searchUser.avatarUrl,
          userId: searchUser.id,
          professionalTitle: searchUser.professionalTitle,
          isOnline: false
        }));
      
        setSelectedContact({
          id: existingConversation.id,
          name: searchUser.fullName, 
          avatar: searchUser.avatarUrl || existingConversation.otherUserAvatar || "https://placehold.co/40x40",
          lastMessage: existingConversation.lastMessage?.content || "No messages yet",
          timestamp: formatTimestamp(existingConversation.updatedAt),
          isActive: true,
          isOnline: existingConversation.isOnline || false,
          unreadCount: existingConversation.unreadCount,
          userId: searchUser.id, 
          professionalTitle: searchUser.professionalTitle 
        });
        selectConversation(existingConversation);
      } else {
        const newConversation = await createConversation(searchUser.id);
        if (newConversation) {
          setEnhancedContacts(prev => new Map(prev).set(newConversation.id, {
            name: searchUser.fullName,
            avatar: searchUser.avatarUrl,
            userId: searchUser.id,
            professionalTitle: searchUser.professionalTitle,
            isOnline: false
          }));
          
          setSelectedContact({
            id: newConversation.id,
            name: searchUser.fullName,
            avatar: searchUser.avatarUrl || newConversation.otherUserAvatar || "https://placehold.co/40x40",
            lastMessage: "No messages yet",
            timestamp: formatTimestamp(newConversation.updatedAt),
            isActive: true,
            isOnline: newConversation.isOnline || false,
            unreadCount: 0,
            userId: searchUser.id, 
            professionalTitle: searchUser.professionalTitle
          });
          selectConversation(newConversation);
        }
      }
    } catch (error) {
      console.error('Failed to create or select conversation:', error);
    }
  };

  const handleSendMessage = async (content: string) => {
    if (!currentConversation || !user) return;
    
    try {
      await sendMessage(content, currentConversation.otherUserId);
    } catch (error) {
      console.error('Failed to send message:', error);
    }
  };

  if (loading) {
    return (
      <div style={{ padding: 32, color: "#888", textAlign: "center" }}>
        Loading conversations...
      </div>
    );
  }

  if (error) {
    return (
      <div style={{ padding: 32, color: "red", textAlign: "center" }}>
        Error loading conversations: {error}
      </div>
    );
  }

  return (
    <div className="messages-page">
      <div className="messages-sidebar-container">
        <Sidebar 
          contacts={contacts} 
          selectedContact={selectedContact}
          onSelectContact={handleSelectContact}
          onNewConversation={handleNewConversation}
        />
      </div>
      
      <div
        className="messages-chat"
        style={{ flex: 1, display: "flex", flexDirection: "column" }}
      >

        {messagesError && (
          <div style={{ padding: "1rem", backgroundColor: "#fee", color: "red", textAlign: "center" }}>
            Error loading messages: {messagesError}
          </div>
        )}
        {selectedContact ? (
          messagesLoading ? (
            <div style={{ padding: 32, color: "#888", textAlign: "center" }}>
              Loading messages...
            </div>
          ) : (
            <Chat 
              messages={currentMessages} 
              selectedContact={selectedContact}
              currentUserAvatar={user?.avatarUrl}
              onSendMessage={handleSendMessage}
              sendingMessage={sendingMessage}
            />
          )
        ) : (
          <div style={{ padding: 32, color: "#888", textAlign: "center" }}>
            {contacts.length === 0 ? "No conversations yet" : "Select a contact to start chatting"}
          </div>
        )}
      </div>
    </div>
  );
};

export default MessagesPage;