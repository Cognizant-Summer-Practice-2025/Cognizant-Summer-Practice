"use client";
import React, { useState, useEffect, useCallback } from "react";
import Header from "@/components/header";
import Sidebar from "@/components/messages-page/sidebar/sidebar";
import Chat from "@/components/messages-page/chat/chat";
import { useUser } from "@/lib/contexts/user-context";
import { useAuth } from "@/lib/hooks/use-auth";
import { SearchUser } from "@/lib/user";
import useMessages from "@/lib/messages";
import { AlertProvider } from "@/components/ui/alert-dialog";
import { 
  Contact, 
  loadEnhancedContactsFromStorage
} from "@/lib/utils/contact-utils";
import { formatMessageTimestamp, getValidTimestamp, formatTimestamp } from "@/lib/utils/message-utils";
import "./style.css";
import Loading from "@/components/loader/loading";



interface Message {
  id: string;
  sender: "user" | "other";
  text: string;
  timestamp: string;
  status: "read" | "delivered" | "sent";
}

type MobileView = 'sidebar' | 'chat';

const MessagesPage = () => {
  const { user, loading: userLoading } = useUser();
  const { isAuthenticated, loading: authLoading } = useAuth();
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
    markMessageAsRead,
    deleteMessage,
    reportMessage
  } = useMessages();

  

  // No need for artificial delays with JWT-based auth - redirect immediately if not authenticated
  useEffect(() => {
    if (!authLoading && !userLoading && !isAuthenticated) {
      const homeServiceUrl = process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE || 'http://localhost:3001';
      window.location.href = homeServiceUrl;
      return;
    }
  }, [isAuthenticated, authLoading, userLoading]);

  const [selectedContact, setSelectedContact] = useState<Contact | null>(null);
  const [mobileView, setMobileView] = useState<MobileView>('sidebar');
  const [isMobile, setIsMobile] = useState(false);
  
  const [enhancedContacts] = useState<Map<string, Partial<Contact>>>(() => 
    loadEnhancedContactsFromStorage()
  );

  useEffect(() => {
    const checkIsMobile = () => {
      const width = window.innerWidth;
      setIsMobile(width <= 768);
    };

    checkIsMobile();
    window.addEventListener('resize', checkIsMobile);
    return () => window.removeEventListener('resize', checkIsMobile);
  }, []);

  useEffect(() => {
    if (!isMobile) {
      setMobileView('sidebar');
    } else {
      // On mobile, show sidebar if no contact is selected, otherwise show chat
      setMobileView(selectedContact ? 'chat' : 'sidebar');
    }
  }, [isMobile, selectedContact]);



  const currentMessages: Message[] = messages.map(msg => {
    let status: "read" | "delivered" | "sent" = "sent";
    
    if (msg.senderId === user?.id) {
      status = msg.isRead ? "read" : "delivered";
    } else {
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

  const getEnhancedContact = useCallback((conv: typeof conversations[0]): Contact => {
    const enhanced = enhancedContacts.get(conv.id);
    const timestamp = getValidTimestamp(
      conv.lastMessageTimestamp,
      conv.lastMessage?.createdAt,
      conv.updatedAt,
      conv.createdAt
    );
    
    return {
      id: conv.id,
      name: enhanced?.name || conv.otherUserName,
      avatar: enhanced?.avatar || conv.otherUserAvatar || "https://placehold.co/40x40",
      lastMessage: conv.lastMessage?.content || "No messages yet", 
      timestamp: formatTimestamp(timestamp),
      isActive: currentConversation?.id === conv.id,
      isOnline: conv.isOnline ?? false,
      unreadCount: conv.unreadCount,
      userId: enhanced?.userId || conv.otherUserId,
      professionalTitle: enhanced?.professionalTitle || conv.otherUserProfessionalTitle
    };
  }, [enhancedContacts, currentConversation?.id]);

  const contacts: Contact[] = conversations.map(getEnhancedContact);

  useEffect(() => {
    if (enhancedContacts.size > 0) {
      try {
        const contactsObject = Object.fromEntries(enhancedContacts);
        localStorage.setItem('enhancedContacts', JSON.stringify(contactsObject));
      } catch {
        // Failed to save enhanced contacts to localStorage - continue silently
      }
    }
  }, [enhancedContacts]);

  useEffect(() => {
    let cancelled = false;
    const autoSelectFirstContact = async () => {
      if (contacts.length > 0 && !selectedContact) {
        if (cancelled) return;
        const first = contacts[0];
        setSelectedContact(first);
        const conversation = conversations.find(conv => conv.id === first.id);
        if (conversation) {
          await selectConversation(conversation);
        }
      }
    };
    autoSelectFirstContact();
    return () => { cancelled = true; };
  }, [contacts, conversations, selectConversation, selectedContact?.id]);

  useEffect(() => {
    if (selectedContact && conversations.length > 0) {
      const currentConv = conversations.find(conv => conv.id === selectedContact.id);
      if (currentConv) {
        const updatedContact = getEnhancedContact(currentConv);
        // Only update if the relevant fields have actually changed
        if (
          selectedContact.isOnline !== updatedContact.isOnline ||
          selectedContact.lastMessage !== updatedContact.lastMessage ||
          selectedContact.timestamp !== updatedContact.timestamp
        ) {
          setSelectedContact(prev => ({
            ...prev!,
            isOnline: updatedContact.isOnline,
            lastMessage: updatedContact.lastMessage,
            timestamp: updatedContact.timestamp
          }));
        }
      }
    }
  }, [conversations, selectedContact, getEnhancedContact]);

  const handleSelectContact = async (contact: Contact) => {
    const conversation = conversations.find(conv => conv.id === contact.id);
    if (conversation) {
      setSelectedContact({
        ...contact,
        lastMessage: conversation.lastMessage?.content || "No messages yet",
        timestamp: formatTimestamp(conversation.updatedAt)
      });
      await selectConversation(conversation);
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
        if (isMobile) {
          setMobileView('chat');
        }
      }
    } catch {
      // Failed to create conversation - error handling done by useMessages hook
    }
  };


  const handleBackToSidebar = useCallback(() => {
    if (isMobile) {
      setMobileView('sidebar');
    }
  }, [isMobile]);

  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape' && isMobile && mobileView === 'chat') {
        handleBackToSidebar();
        return;
      }
      if (event.ctrlKey || event.metaKey) {
        switch (event.key) {
          case 'f': 
            if (mobileView === 'chat' || !isMobile) {
              event.preventDefault();
              const messageInput = document.querySelector('.message-input') as HTMLInputElement;
              messageInput?.focus();
            }
            break;
        }
      }
    };

    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [isMobile, mobileView, handleBackToSidebar]);


  const handleSendMessage = async (content: string) => {
    if (!currentConversation || !user) return;
    
    try {
      await sendMessage(content);
    } catch {
      // Failed to send message - error handling done by useMessages hook
    }
  };

  const handleDeleteConversation = async (conversationId: string) => {
    try {
      await deleteConversation(conversationId);
      if (selectedContact?.id === conversationId) {
        setSelectedContact(null);
      }
    } catch {
      // Failed to delete conversation - error handling done by useMessages hook
    }
  };
  // Show loading only for essential auth/user loading
  if (authLoading || userLoading) {
    return (
      <AlertProvider>
        <Header />
        <div className="messages-page">
          <div className="flex items-center justify-center w-full h-full">
            <Loading />
          </div>
        </div>
      </AlertProvider>
    );
  }

  // Redirect if not authenticated
  if (!isAuthenticated) {
    return (
      <AlertProvider>
        <Header />
        <div className="messages-page">
          <div className="flex items-center justify-center w-full h-full">
            <Loading />
          </div>
        </div>
      </AlertProvider>
    );
  }

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
  
  if (error) {
    return (
      <div style={{ padding: 32, color: "red", textAlign: "center" }}>
        Error loading conversations: {error}
      </div>
    );
  }

     return (
     <AlertProvider>
       <Header />
       <div 
         className={`messages-page ${isMobile ? 'mobile' : 'desktop'}`}
         role="main"
         aria-label="Messages application"
       >
      
      {/* Sidebar - visible on desktop or when mobile view is 'sidebar' */}
      <div 
        className={`messages-sidebar-container ${(!isMobile || mobileView === 'sidebar') ? 'visible' : 'hidden'}`}
        role="navigation"
        aria-label="Conversations list"
        aria-hidden={isMobile && mobileView !== 'sidebar'}
      >
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
        role="main"
        aria-label={selectedContact ? `Chat with ${selectedContact.name}` : "Chat area"}
        aria-hidden={isMobile && mobileView !== 'chat'}
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
              onDeleteMessage={deleteMessage}
              onReportMessage={reportMessage}
            />
          )
        ) : (
          <div style={{ padding: 32, color: "#888", textAlign: "center" }}>
            {contacts.length === 0 
              ? (isMobile ? "No conversations yet. Tap the + button to start a new conversation." : "No conversations yet") 
              : (isMobile ? "Go back to see your conversations" : "Select a contact to start chatting")
            }
          </div>
        )}
      </div>
      </div>
    </AlertProvider>
  );
};

export default MessagesPage;