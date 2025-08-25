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