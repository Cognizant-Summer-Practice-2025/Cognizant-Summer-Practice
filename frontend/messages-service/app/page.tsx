"use client";
import React, { useEffect, useCallback } from "react";
import Header from "@/components/header";
import Sidebar from "@/components/messages-page/sidebar/sidebar";
import Chat from "@/components/messages-page/chat/chat";
import { useUser } from "@/lib/contexts/user-context";
import { useAuth } from "@/lib/hooks/use-auth";
import useMessages from "@/lib/messages";
import { AlertProvider } from "@/components/ui/alert-dialog";
import "./style.css";

// Custom hooks
import { useResponsiveDetection } from "@/hooks/useResponsiveDetection";
import { useMobileViewManager } from "@/hooks/useMobileViewManager";
import { useContactManager } from "@/hooks/useContactManager";
import { useKeyboardShortcuts } from "@/hooks/useKeyboardShortcuts";
import { useMessageManager } from "@/hooks/useMessageManager";

// Loading and error state components
import {
  AuthLoadingState,
  ConversationsLoadingState,
  ConversationsErrorState,
  MessagesLoadingState,
  MessagesErrorState,
  EmptyState,
} from "@/components/messages-page/loading-states/LoadingStates";

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

  // Responsive detection
  const { isMobile } = useResponsiveDetection();

  // Contact management
  const {
    selectedContact,
    contacts,
    handleSelectContact,
    handleNewConversation,
    clearSelectedContact,
  } = useContactManager({
    conversations,
    currentConversation,
    selectConversation,
    createConversation,
    onContactSelect: () => {
      if (isMobile) {
        mobileViewManager.showChat();
      }
    },
  });

  // Mobile view management
  const mobileViewManager = useMobileViewManager(isMobile, selectedContact);

  // Message management
  const {
    currentMessages,
    handleSendMessage,
    handleDeleteConversation: handleDeleteConv,
  } = useMessageManager({
    messages,
    user,
    currentConversation,
    sendMessage,
    deleteConversation,
  });

  // Enhanced delete conversation handler
  const handleDeleteConversation = useCallback(async (conversationId: string) => {
    await handleDeleteConv(conversationId);
    if (selectedContact?.id === conversationId) {
      clearSelectedContact();
    }
  }, [handleDeleteConv, selectedContact?.id, clearSelectedContact]);

  // Keyboard shortcuts
  useKeyboardShortcuts({
    isMobile,
    mobileView: mobileViewManager.mobileView,
    onBackToSidebar: mobileViewManager.showSidebar,
  });

  // Authentication redirect
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
  
  const [enhancedContacts] = useState<Map<string, Partial<Contact>>>(() => {
    if (typeof window !== 'undefined') {
      try {
        const saved = localStorage.getItem('enhancedContacts');
        if (saved) {
          const parsed = JSON.parse(saved);
          return new Map(Object.entries(parsed));
        }
      } catch {}
    }
    return new Map();
  });

  useEffect(() => {
    const checkIsMobile = () => {
      const width = window.innerWidth;
      setIsMobile(width < 480); 
    };

    checkIsMobile();
    window.addEventListener('resize', checkIsMobile);
    return () => window.removeEventListener('resize', checkIsMobile);
  }, []);

  useEffect(() => {
    if (!isMobile) {
      setMobileView('sidebar');
    }
  }, [isMobile]);

  const getValidTimestamp = useCallback((...timestamps: (string | undefined)[]): string => {
    for (const timestamp of timestamps) {
      if (timestamp && timestamp.trim() !== '') {
        const testDate = new Date(timestamp);
        if (!isNaN(testDate.getTime())) {
          return timestamp;
        }
      }
    }
    return new Date().toISOString();
  }, []);

  const formatTimestamp = useCallback((dateString: string): string => {
    if (!dateString || dateString.trim() === '') {
      return 'Now';
    }
    
    const date = new Date(dateString);
    
    if (isNaN(date.getTime())) {
      return 'Now';
    }
    
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
  }, []);

  const formatMessageTimestamp = useCallback((dateString: string): string => {
    if (!dateString || dateString.trim() === '') {
      dateString = new Date().toISOString();
    }
    
    const utcDate = new Date(dateString + (dateString.endsWith('Z') ? '' : 'Z'));
    
    if (isNaN(utcDate.getTime())) {
      return formatMessageTimestamp(new Date().toISOString());
    }
    
    const now = new Date();
    
    const diffInHours = (now.getTime() - utcDate.getTime()) / (1000 * 60 * 60);
    
    if (diffInHours < 12) {
      return utcDate.toLocaleTimeString(undefined, { 
        hour: 'numeric', 
        minute: '2-digit',
        hour12: true
      });
    } else {
      return utcDate.toLocaleDateString(undefined, {
        month: 'short',
        day: 'numeric',
        year: utcDate.getFullYear() !== now.getFullYear() ? 'numeric' : undefined
      });
    }
  }, []);

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
    
    const result = {
      id: conv.id,
      name: enhanced?.name || conv.otherUserName,
      avatar: enhanced?.avatar || conv.otherUserAvatar || "https://placehold.co/40x40",
      lastMessage: conv.lastMessage?.content || "No messages yet", 
      timestamp: formatMessageTimestamp(timestamp),
      isActive: currentConversation?.id === conv.id,
      isOnline: conv.isOnline ?? false, // Prioritize conversation's online status
      unreadCount: conv.unreadCount,
      userId: enhanced?.userId || conv.otherUserId,
      professionalTitle: enhanced?.professionalTitle || conv.otherUserProfessionalTitle
    };
  
    if (conv.otherUserId === '6677b218-6e92-47b3-9e9f-61bea9f15f8d') {
      console.log('getEnhancedContact for conversation:', {
        convOnline: conv.isOnline,
        enhancedOnline: enhanced?.isOnline,
        resultOnline: result.isOnline
      });
    }
    
    return result;
  }, [enhancedContacts, currentConversation?.id, formatMessageTimestamp, getValidTimestamp]);

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
  }, [conversations, selectedContact?.id, selectedContact?.isOnline, selectedContact?.lastMessage, selectedContact?.timestamp, getEnhancedContact]);

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
    } catch (error) {
      console.error('Failed to create conversation:', error);
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
    } catch (error) {
      console.error('Failed to send message:', error);
    }
  };

  const handleDeleteConversation = async (conversationId: string) => {
    try {
      await deleteConversation(conversationId);
      if (selectedContact?.id === conversationId) {
        setSelectedContact(null);
      }
    } catch (error) {
      console.error('Failed to delete conversation:', error);
    }
  };
  // Show loading only for essential auth/user loading
  if (authLoading || userLoading) {
    return <AuthLoadingState />;
  }

  if (!isAuthenticated) {
    return <AuthLoadingState />;
  }

  if (loading && conversations.length === 0 && !cacheState.isFromCache) {
    return <ConversationsLoadingState />;
  }
  
  if (error) {
    return <ConversationsErrorState error={error} />;
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
          className={`messages-sidebar-container ${mobileViewManager.isViewVisible('sidebar') ? 'visible' : 'hidden'}`}
          role="navigation"
          aria-label="Conversations list"
          aria-hidden={isMobile && mobileViewManager.mobileView !== 'sidebar'}
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
          className={`messages-chat ${mobileViewManager.isViewVisible('chat') ? 'visible' : 'hidden'}`}
          role="main"
          aria-label={selectedContact ? `Chat with ${selectedContact.name}` : "Chat area"}
          aria-hidden={isMobile && mobileViewManager.mobileView !== 'chat'}
        >
          {messagesError && <MessagesErrorState error={messagesError} />}
          
          {selectedContact ? (
            messagesLoading ? (
              <MessagesLoadingState />
            ) : (
              <Chat 
                messages={currentMessages} 
                selectedContact={selectedContact}
                currentUserAvatar={user?.avatarUrl}
                onSendMessage={handleSendMessage}
                sendingMessage={sendingMessage}
                onDeleteConversation={handleDeleteConversation}
                markMessageAsRead={markMessageAsRead}
                onBackToSidebar={mobileViewManager.showSidebar}
                isMobile={isMobile}
                onDeleteMessage={deleteMessage}
                onReportMessage={reportMessage}
              />
            )
          ) : (
            <EmptyState 
              isMobile={isMobile} 
              hasContacts={contacts.length > 0} 
            />
          )}
        </div>
      </div>
    </AlertProvider>
  );
};

export default MessagesPage;