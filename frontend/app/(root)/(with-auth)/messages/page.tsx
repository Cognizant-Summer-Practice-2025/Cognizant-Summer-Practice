"use client";
import React, { useState, useEffect } from "react";
import Sidebar from "@/components/messages-page/sidebar/sidebar";
import Chat from "@/components/messages-page/chat/chat";
import { useUser } from "@/lib/contexts/user-context";
import { SearchUser } from "@/lib/user";
import useMessages from "@/lib/messages";
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

type MobileView = 'sidebar' | 'chat';

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
    cacheState,
    selectConversation,
    sendMessage,
    createConversation,
    deleteConversation,
    markMessageAsRead
  } = useMessages();

  const [selectedContact, setSelectedContact] = useState<Contact | null>(null);
  const [mobileView, setMobileView] = useState<MobileView>('sidebar');
  const [isMobile, setIsMobile] = useState(false);
  
  // Enhanced contacts storage for additional metadata (keeping for potential future use)
  const [enhancedContacts] = useState<Map<string, Partial<Contact>>>(() => {
    if (typeof window !== 'undefined') {
      try {
        const saved = localStorage.getItem('enhancedContacts');
        if (saved) {
          const parsed = JSON.parse(saved);
          return new Map(Object.entries(parsed));
        }
      } catch (error) {
        console.warn('Failed to load enhanced contacts from localStorage:', error);
      }
    }
    return new Map();
  });

  // Check if mobile on mount and window resize
  useEffect(() => {
    const checkIsMobile = () => {
      setIsMobile(window.innerWidth <= 768);
    };

    checkIsMobile();
    window.addEventListener('resize', checkIsMobile);
    return () => window.removeEventListener('resize', checkIsMobile);
  }, []);

  // Reset to sidebar view when switching back to desktop
  useEffect(() => {
    if (!isMobile) {
      setMobileView('sidebar');
    }
  }, [isMobile]);

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
    // Backend sends UTC time without timezone indicator, so we need to treat it as UTC
    const utcDate = new Date(dateString + 'Z'); // Add 'Z' to indicate it's UTC
    const now = new Date();
    
    // Calculate time difference in hours
    const diffInHours = (now.getTime() - utcDate.getTime()) / (1000 * 60 * 60);
    
    if (diffInHours < 24) {
      // Less than 24 hours: show time only
      return utcDate.toLocaleTimeString(undefined, { 
        hour: 'numeric', 
        minute: '2-digit',
        hour12: true
      });
    } else {
      // More than 24 hours: show date
      return utcDate.toLocaleDateString(undefined, {
        month: 'short',
        day: 'numeric',
        year: utcDate.getFullYear() !== now.getFullYear() ? 'numeric' : undefined
      });
    }
  };

  const currentMessages: Message[] = messages.map(msg => {
    let status: "read" | "delivered" | "sent" = "sent";
    
    if (msg.senderId === user?.id) {
      // Messages sent by current user
      status = msg.isRead ? "read" : "delivered";
    } else {
      // Messages received by current user - always show as delivered/sent since they're not our messages to track
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
    const result = {
      id: conv.id,
      name: enhanced?.name || conv.otherUserName,
      avatar: enhanced?.avatar || conv.otherUserAvatar || "https://placehold.co/40x40",
      lastMessage: conv.lastMessage?.content || "No messages yet", 
      timestamp: formatMessageTimestamp(conv.lastMessageTimestamp),
      isActive: currentConversation?.id === conv.id,
      isOnline: conv.isOnline ?? false, // Prioritize conversation's online status
      unreadCount: conv.unreadCount,
      userId: enhanced?.userId || conv.otherUserId,
      professionalTitle: enhanced?.professionalTitle || conv.otherUserProfessionalTitle
    };
    
    // Debug logging
    if (conv.otherUserId === '6677b218-6e92-47b3-9e9f-61bea9f15f8d') {
      console.log(`getEnhancedContact for user ${conv.otherUserId}:`, {
        convOnline: conv.isOnline,
        enhancedOnline: enhanced?.isOnline,
        resultOnline: result.isOnline
      });
    }
    
    return result;
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
    const autoSelectFirstContact = async () => {
      if (contacts.length > 0 && !selectedContact) {
        setSelectedContact(contacts[0]);
        const conversation = conversations.find(conv => conv.id === contacts[0].id);
        if (conversation) {
          await selectConversation(conversation);
        }
      }
    };
    
    autoSelectFirstContact();
  }, [contacts, selectedContact, conversations, selectConversation]);

  // Simpler approach: Update selectedContact whenever conversations change
  useEffect(() => {
    if (selectedContact && conversations.length > 0) {
      const currentConv = conversations.find(conv => conv.id === selectedContact.id);
      if (currentConv) {
        const updatedContact = getEnhancedContact(currentConv);
        console.log(`Updating selectedContact for ${updatedContact.name}:`, {
          isOnline: updatedContact.isOnline,
          conversationOnline: currentConv.isOnline
        });
        setSelectedContact(prev => ({
          ...prev!,
          isOnline: updatedContact.isOnline,
          lastMessage: updatedContact.lastMessage,
          timestamp: updatedContact.timestamp
        }));
      }
    }
  }, [conversations]);

  const handleSelectContact = async (contact: Contact) => {
    const conversation = conversations.find(conv => conv.id === contact.id);
    if (conversation) {
      setSelectedContact({
        ...contact,
        lastMessage: conversation.lastMessage?.content || "No messages yet",
        timestamp: formatTimestamp(conversation.updatedAt)
      });
      await selectConversation(conversation);
      // Switch to chat view on mobile when a contact is selected
      if (isMobile) {
        setMobileView('chat');
      }
    } else {
      setSelectedContact(contact);
      if (isMobile) {
        setMobileView('chat');
      }
    }
  };

  const handleNewConversation = async (searchUser: SearchUser) => {
    try {
      const newConversation = await createConversation(searchUser);
      if (newConversation) {
        const newContact: Contact = {
          id: newConversation.id,
          name: searchUser.fullName,
          avatar: searchUser.avatarUrl || "https://placehold.co/40x40",
          lastMessage: "No messages yet",
          timestamp: formatTimestamp(newConversation.updatedAt),
          isActive: false,
          isOnline: false,
          unreadCount: 0,
          userId: newConversation.otherUserId,
          professionalTitle: searchUser.professionalTitle
        };
        
        setSelectedContact(newContact);
        await selectConversation(newConversation);
        // Switch to chat view on mobile when a new conversation is created
        if (isMobile) {
          setMobileView('chat');
        }
      }
    } catch (error) {
      console.error('Failed to create conversation:', error);
    }
  };

  // Handle back button navigation on mobile
  const handleBackToSidebar = () => {
    if (isMobile) {
      setMobileView('sidebar');
    }
  };

  // Debug function to test online status (removing for now to fix linter)
  // const testOnlineStatus = async () => {
  //   if (selectedContact?.userId) {
  //     console.log('Testing online status for:', selectedContact.userId);
  //     try {
  //       const response = await fetch(`http://localhost:5093/api/users/${selectedContact.userId}/online-status`);
  //       const data = await response.json();
  //       console.log('Online status response:', data);
  //       alert(`User ${selectedContact.name} is ${data.isOnline ? 'ONLINE' : 'OFFLINE'}`);
  //     } catch (error) {
  //       console.error('Error testing online status:', error);
  //       alert('Error checking online status');
  //     }
  //   }
  // };

  const handleSendMessage = async (content: string) => {
    if (!currentConversation || !user) return;
    
    try {
      await sendMessage(content);
    } catch (error) {
      console.error('Failed to send message:', error);
    }
  };

  const handleDeleteConversation = async (conversationId: string) => {
    console.log("handleDeleteConversation called with ID:", conversationId);
    try {
      console.log("Calling deleteConversation from useMessages...");
      await deleteConversation(conversationId);
      console.log("Delete conversation successful");
      // If the deleted conversation was selected, clear the selection
      if (selectedContact?.id === conversationId) {
        setSelectedContact(null);
      }
    } catch (error) {
      console.error('Failed to delete conversation:', error);
    }
  };

  // Only show loading spinner if there's no cache at all (first visit or cache cleared)
  if (loading && conversations.length === 0 && !cacheState.isFromCache) {
    return (
      <div className="messages-page">

        <div className="messages-sidebar-container">
          {/* Skeleton loader for conversations */}
          <div style={{ padding: '20px' }}>
            <div style={{ marginBottom: '20px' }}>
              <div style={{ 
                height: '40px', 
                backgroundColor: '#f0f0f0', 
                borderRadius: '8px',
                marginBottom: '10px'
              }}></div>
            </div>
            {[...Array(5)].map((_, i) => (
              <div key={i} style={{
                display: 'flex',
                alignItems: 'center',
                padding: '12px',
                marginBottom: '8px',
                backgroundColor: '#f9f9f9',
                borderRadius: '8px'
              }}>
                <div style={{
                  width: '40px',
                  height: '40px',
                  backgroundColor: '#e0e0e0',
                  borderRadius: '50%',
                  marginRight: '12px'
                }}></div>
                <div style={{ flex: 1 }}>
                  <div style={{
                    height: '16px',
                    backgroundColor: '#e0e0e0',
                    borderRadius: '4px',
                    marginBottom: '6px',
                    width: '70%'
                  }}></div>
                  <div style={{
                    height: '12px',
                    backgroundColor: '#e0e0e0',
                    borderRadius: '4px',
                    width: '50%'
                  }}></div>
                </div>
              </div>
            ))}
          </div>
        </div>
        
        <div className="messages-chat" style={{ 
          flex: 1, 
          display: "flex", 
          flexDirection: "column", 
          padding: "4rem 0 0 0",
          justifyContent: 'center',
          alignItems: 'center',
          color: '#888'
        }}>
          Loading conversations...
        </div>
      </div>
    );
  }

  // Removed old loading check - now using optimized skeleton loader above

  if (error) {
    return (
      <div style={{ padding: 32, color: "red", textAlign: "center" }}>
        Error loading conversations: {error}
      </div>
    );
  }

  return (
    <div className={`messages-page ${isMobile ? 'mobile' : 'desktop'}`}>
      
      {/* Sidebar - visible on desktop or when mobile view is 'sidebar' */}
      <div className={`messages-sidebar-container ${(!isMobile || mobileView === 'sidebar') ? 'visible' : 'hidden'}`}>
        <Sidebar 
          contacts={contacts} 
          selectedContact={selectedContact}
          onSelectContact={handleSelectContact}
          onNewConversation={handleNewConversation}
        />
      </div>
      
      {/* Chat - visible on desktop or when mobile view is 'chat' */}
      <div
        className={`messages-chat ${(!isMobile || mobileView === 'chat') ? 'visible' : 'hidden'}`}
        style={{ flex: 1, display: "flex", flexDirection: "column", padding: "4rem 0 0 0" }}
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
              onDeleteConversation={handleDeleteConversation}
              markMessageAsRead={markMessageAsRead}
              onBackToSidebar={handleBackToSidebar}
              isMobile={isMobile}
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