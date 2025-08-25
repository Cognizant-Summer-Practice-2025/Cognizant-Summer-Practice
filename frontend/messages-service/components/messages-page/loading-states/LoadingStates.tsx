import React from 'react';
import Header from "@/components/header";
import Loading from "@/components/loader/loading";
import { AlertProvider } from "@/components/ui/alert-dialog";

export const AuthLoadingState: React.FC = () => (
  <AlertProvider>
    <Header />
    <div className="messages-page">
      <div className="flex items-center justify-center w-full h-full">
        <Loading />
      </div>
    </div>
  </AlertProvider>
);

export const ConversationsLoadingState: React.FC = () => (
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

export const MessagesLoadingState: React.FC = () => (
  <div style={{ padding: 32, color: "#888", textAlign: "center" }}>
    Loading messages...
  </div>
);

interface ErrorStateProps {
  error: string;
}

export const ConversationsErrorState: React.FC<ErrorStateProps> = ({ error }) => (
  <div style={{ padding: 32, color: "red", textAlign: "center" }}>
    Error loading conversations: {error}
  </div>
);

export const MessagesErrorState: React.FC<ErrorStateProps> = ({ error }) => (
  <div style={{ padding: "1rem", backgroundColor: "#fee", color: "red", textAlign: "center" }}>
    Error loading messages: {error}
  </div>
);

interface EmptyStateProps {
  isMobile: boolean;
  hasContacts: boolean;
}

export const EmptyState: React.FC<EmptyStateProps> = ({ isMobile, hasContacts }) => (
  <div style={{ padding: 32, color: "#888", textAlign: "center" }}>
    {!hasContacts 
      ? (isMobile ? "No conversations yet. Tap the + button to start a new conversation." : "No conversations yet") 
      : (isMobile ? "Go back to see your conversations" : "Select a contact to start chatting")
    }
  </div>
); 