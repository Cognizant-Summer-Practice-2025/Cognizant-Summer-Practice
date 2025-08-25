import { useMemo } from 'react';
import { formatMessageTimestamp } from '@/lib/utils/message-utils';

interface Message {
  id: string;
  sender: "user" | "other";
  text: string;
  timestamp: string;
  status: "read" | "delivered" | "sent";
}

interface UseMessageManagerProps {
  messages: Array<{
    id: string;
    senderId: string;
    content: string;
    createdAt: string;
    isRead: boolean;
  }>;
  user: {
    id: string;
  } | null;
  currentConversation: {
    id: string;
  } | null;
  sendMessage: (content: string) => Promise<unknown>;
  deleteConversation: (conversationId: string) => Promise<void>;
}

export const useMessageManager = ({
  messages,
  user,
  currentConversation,
  sendMessage,
  deleteConversation,
}: UseMessageManagerProps) => {
  const currentMessages: Message[] = useMemo(() => {
    return messages.map(msg => {
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
  }, [messages, user?.id]);

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
    } catch {
      // Failed to delete conversation - error handling done by useMessages hook
    }
  };

  return {
    currentMessages,
    handleSendMessage,
    handleDeleteConversation,
  };
}; 