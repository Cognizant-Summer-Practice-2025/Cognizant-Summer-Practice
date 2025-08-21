'use client';

import React, { createContext, useContext, useEffect, useState, useCallback, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import { useUser } from './user-context';

export interface Message {
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

export interface ConversationUpdate {
  id: string;
  lastMessageTimestamp: string;
  lastMessage: Message;
  updatedAt: string;
}

export interface UserPresenceUpdate {
  userId: string;
  isOnline: boolean;
  timestamp: string;
}

export interface MessageReadReceipt {
  messageId: string;
  conversationId: string;
  readByUserId: string;
  readAt: string;
}

export interface MessageDeleted {
  messageId: string;
  conversationId: string;
  deletedBy: string;
  deletedAt: string;
}

interface WebSocketContextType {
  connection: signalR.HubConnection | null;
  isConnected: boolean;
  isConnecting: boolean;
  onMessageReceived: (callback: (message: Message) => void) => () => void;
  onConversationUpdated: (callback: (update: ConversationUpdate) => void) => () => void;
  onUserPresenceUpdate: (callback: (update: UserPresenceUpdate) => void) => () => void;
  onMessageReadReceipt: (callback: (receipt: MessageReadReceipt) => void) => () => void;
  onMessageDeleted: (callback: (deletion: MessageDeleted) => void) => () => void;
  markMessageAsRead: (messageId: string, userId: string) => Promise<void>;
  deleteMessage: (messageId: string, userId: string) => Promise<void>;
  connect: () => Promise<void>;
  disconnect: () => Promise<void>;
}

const WebSocketContext = createContext<WebSocketContextType | undefined>(undefined);

export const useWebSocket = () => {
  const context = useContext(WebSocketContext);
  if (context === undefined) {
    throw new Error('useWebSocket must be used within a WebSocketProvider');
  }
  return context;
};

interface WebSocketProviderProps {
  children: React.ReactNode;
}

export const WebSocketProvider: React.FC<WebSocketProviderProps> = ({ children }) => {
  const { user } = useUser();
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
  const [isConnected, setIsConnected] = useState(false);
  const [isConnecting, setIsConnecting] = useState(false);
  
  // Use refs to store callbacks to avoid dependency issues
  const messageCallbacks = useRef<((message: Message) => void)[]>([]);
  const conversationCallbacks = useRef<((update: ConversationUpdate) => void)[]>([]);
  const presenceCallbacks = useRef<((update: UserPresenceUpdate) => void)[]>([]);
  const messageReadCallbacks = useRef<((receipt: MessageReadReceipt) => void)[]>([]);
  const messageDeletedCallbacks = useRef<((deletion: MessageDeleted) => void)[]>([]);

  const connect = useCallback(async () => {
    if (!user?.id || isConnecting || isConnected) {
      return;
    }

    setIsConnecting(true);

    try {
      // Get SignalR URL from environment or use default
      const signalRUrl = process.env.NEXT_PUBLIC_MESSAGES_API_URL;
      
      // Create new connection
      const newConnection = new signalR.HubConnectionBuilder()
        .withUrl(`${signalRUrl}/messagehub`, {
          transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents | signalR.HttpTransportType.LongPolling,
          skipNegotiation: false, // Let SignalR negotiate the best transport
        })
        .withAutomaticReconnect([0, 2000, 10000, 30000]) // Retry delays in ms
        .configureLogging(signalR.LogLevel.Information) // Reduce logging verbosity
        .build();

      // Set up event handlers
      newConnection.on('ReceiveMessage', (message: Message) => {
        console.log('Received message via SignalR:', message);
        messageCallbacks.current.forEach(callback => callback(message));
      });

      newConnection.on('ConversationUpdated', (update: ConversationUpdate) => {
        console.log('Conversation updated via SignalR:', update);
        conversationCallbacks.current.forEach(callback => callback(update));
      });

      newConnection.on('UserPresenceUpdate', (update: UserPresenceUpdate) => {
        console.log('User presence updated via SignalR:', update);
        presenceCallbacks.current.forEach(callback => callback(update));
      });

      newConnection.on('MessageRead', (receipt: MessageReadReceipt) => {
        console.log('Message read receipt via SignalR:', receipt);
        messageReadCallbacks.current.forEach(callback => callback(receipt));
      });

      newConnection.on('MessageDeleted', (deletion: MessageDeleted) => {
        console.log('Message deleted via SignalR:', deletion);
        messageDeletedCallbacks.current.forEach(callback => callback(deletion));
      });

      // Handle connection events
      newConnection.onreconnecting((error) => {
        console.log('SignalR reconnecting:', error);
        setIsConnected(false);
      });

      newConnection.onreconnected((connectionId) => {
        console.log('SignalR reconnected:', connectionId);
        setIsConnected(true);
        // Rejoin user group after reconnection
        if (user?.id) {
          newConnection.invoke('JoinUserGroup', user.id.toString())
            .catch(err => console.error('Failed to rejoin user group:', err));
        }
      });

      newConnection.onclose((error) => {
        console.log('SignalR connection closed:', error);
        setIsConnected(false);
        setIsConnecting(false);
      });

      // Start the connection
      await newConnection.start();
      console.log('SignalR connected successfully');

      // Join user group
      console.log(`Attempting to join user group for user ID: ${user.id}`);
      await newConnection.invoke('JoinUserGroup', user.id.toString());
      console.log(`Successfully joined user group: ${user.id}`);

      setConnection(newConnection);
      setIsConnected(true);
    } catch (error) {
      console.warn('SignalR connection failed (this is optional for portfolio service):', error instanceof Error ? error.message : error);
      // SignalR connection failure is not critical for the portfolio service
      // The service can function without real-time messaging
    } finally {
      setIsConnecting(false);
    }
  }, [user?.id, isConnecting, isConnected]);

  const disconnect = useCallback(async () => {
    if (connection) {
      try {
        if (user?.id) {
          await connection.invoke('LeaveUserGroup', user.id.toString());
        }
        await connection.stop();
      } catch (error) {
        console.error('Error disconnecting from SignalR:', error);
      } finally {
        setConnection(null);
        setIsConnected(false);
        setIsConnecting(false);
      }
    }
  }, [connection, user?.id]);

  const onMessageReceived = useCallback((callback: (message: Message) => void) => {
    messageCallbacks.current.push(callback);
    
    // Return cleanup function
    return () => {
      const index = messageCallbacks.current.indexOf(callback);
      if (index > -1) {
        messageCallbacks.current.splice(index, 1);
      }
    };
  }, []);

  const onConversationUpdated = useCallback((callback: (update: ConversationUpdate) => void) => {
    conversationCallbacks.current.push(callback);
    
    // Return cleanup function
    return () => {
      const index = conversationCallbacks.current.indexOf(callback);
      if (index > -1) {
        conversationCallbacks.current.splice(index, 1);
      }
    };
  }, []);

  const onUserPresenceUpdate = useCallback((callback: (update: UserPresenceUpdate) => void) => {
    presenceCallbacks.current.push(callback);
    
    // Return cleanup function
    return () => {
      const index = presenceCallbacks.current.indexOf(callback);
      if (index > -1) {
        presenceCallbacks.current.splice(index, 1);
      }
    };
  }, []);

  const onMessageReadReceipt = useCallback((callback: (receipt: MessageReadReceipt) => void) => {
    messageReadCallbacks.current.push(callback);
    
    // Return cleanup function
    return () => {
      const index = messageReadCallbacks.current.indexOf(callback);
      if (index > -1) {
        messageReadCallbacks.current.splice(index, 1);
      }
    };
  }, []);

  const onMessageDeleted = useCallback((callback: (deletion: MessageDeleted) => void) => {
    messageDeletedCallbacks.current.push(callback);
    
    // Return cleanup function
    return () => {
      const index = messageDeletedCallbacks.current.indexOf(callback);
      if (index > -1) {
        messageDeletedCallbacks.current.splice(index, 1);
      }
    };
  }, []);

  const markMessageAsRead = useCallback(async (messageId: string, userId: string) => {
    if (!connection) {
      console.error('SignalR connection not established.');
      return;
    }
    try {
      await connection.invoke('MarkMessageAsRead', messageId, userId);
      console.log(`Message with ID ${messageId} marked as read by user ${userId}`);
    } catch (error) {
      console.error('Error marking message as read:', error);
    }
  }, [connection]);

  const deleteMessage = useCallback(async (messageId: string, userId: string) => {
    if (!connection) {
      console.error('SignalR connection not established.');
      return;
    }
    try {
      await connection.invoke('DeleteMessage', messageId, userId);
      console.log(`Message with ID ${messageId} deleted by user ${userId}`);
    } catch (error) {
      console.error('Error deleting message:', error);
    }
  }, [connection]);

  // Auto connect when user is available
  useEffect(() => {
    if (user?.id && !isConnected && !isConnecting) {
      connect();
    }
  }, [user?.id, isConnected, isConnecting, connect]);

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      disconnect();
    };
  }, [disconnect]);

  const value: WebSocketContextType = {
    connection,
    isConnected,
    isConnecting,
    onMessageReceived,
    onConversationUpdated,
    onUserPresenceUpdate,
    onMessageReadReceipt,
    onMessageDeleted,
    markMessageAsRead,
    deleteMessage,
    connect,
    disconnect
  };

  return (
    <WebSocketContext.Provider value={value}>
      {children}
    </WebSocketContext.Provider>
  );
}; 