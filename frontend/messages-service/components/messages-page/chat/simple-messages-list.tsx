import React, { useEffect, useRef } from 'react';
import { Avatar, AvatarImage, AvatarFallback } from '@/components/ui/avatar';
import { useUser } from '@/lib/contexts/user-context';
import MessageMenu from '@/components/messages-page/message-menu/message-menu';

export interface Message {
  id: string;
  sender: "user" | "other";
  text: string;
  timestamp: string;
  status: "read" | "delivered" | "sent";
}

export interface SimpleMessageItemProps {
  id: string;
  sender: "user" | "other";
  text: string;
  timestamp: string;
  status: "read" | "delivered" | "sent";
  senderAvatar?: string;
  senderName?: string;
  currentUserAvatar?: string;
  isLastMessage?: boolean;
  markMessageAsRead?: (messageId: string, userId: string) => Promise<void>;
  onDeleteMessage?: (messageId: string) => Promise<void>;
  onReportMessage?: (messageId: string, reason: string) => Promise<void>;
  onCopyMessage?: (text: string) => void;
}

interface SimpleMessagesListProps {
  messages: Message[];
  selectedContactAvatar?: string;
  selectedContactName?: string;
  currentUserAvatar?: string;
  markMessageAsRead?: (messageId: string, userId: string) => Promise<void>;
  onDeleteMessage?: (messageId: string) => Promise<void>;
  onReportMessage?: (messageId: string, reason: string) => Promise<void>;
  onCopyMessage?: (text: string) => void;
}

const SimpleMessageItem = React.memo<SimpleMessageItemProps>(({
  id,
  sender,
  text,
  timestamp,
  status,
  senderAvatar,
  senderName,
  currentUserAvatar,
  isLastMessage,
  markMessageAsRead,
  onDeleteMessage,
  onReportMessage,
  onCopyMessage
}) => {
  const { user } = useUser();
  const messageRef = useRef<HTMLDivElement>(null);

  // Implement visibility detection to mark messages as read
  useEffect(() => {
    if (sender !== "user" && status !== "read" && markMessageAsRead && user?.id) {
      const observer = new IntersectionObserver(
        (entries) => {
          entries.forEach((entry) => {
            if (entry.isIntersecting) {
              markMessageAsRead(id, user.id).catch((error) => {
                console.error('Failed to mark message as read:', error);
              });
              observer.unobserve(entry.target);
            }
          });
        },
        {
          threshold: 0.3,
          rootMargin: '0px 0px -20px 0px'
        }
      );

      if (messageRef.current) {
        observer.observe(messageRef.current);
      }

      return () => {
        observer.disconnect();
      };
    }
  }, [id, sender, status, markMessageAsRead, user?.id]);

  const getStatusIcon = (status: string) => {
    switch (status) {
      case "read":
        return "✓✓"; 
      case "delivered":
        return "✓✓"; 
      case "sent":
        return "✓"; 
      default:
        return "";
    }
  };

  const getStatusColor = (status: string, sender: string) => {
    if (sender === "user") {
      switch (status) {
        case "read":
          return "#22c55e"; 
        case "delivered":
          return "rgba(255, 255, 255, 0.7)"; 
        case "sent":
          return "rgba(255, 255, 255, 0.5)"; 
        default:
          return "rgba(255, 255, 255, 0.8)";
      }
    }
    return "transparent"; 
  };

  // Handle copy action
  const handleCopy = (text: string) => {
    if (onCopyMessage) {
      onCopyMessage(text);
    } else {
      navigator.clipboard.writeText(text).then(() => {
        console.log('Message copied to clipboard');
      }).catch(err => {
        console.error('Failed to copy message: ', err);
      });
    }
  };

  // Handle delete action
  const handleDelete = async (messageId: string) => {
    if (onDeleteMessage) {
      await onDeleteMessage(messageId);
    }
  };

  // Handle report action
  const handleReport = async (messageId: string, reason: string) => {
    if (onReportMessage) {
      await onReportMessage(messageId, reason);
    }
  };

  const isOwnMessage = sender === "user";

  return (
    <div 
      ref={messageRef} 
      className={`simple-message-container ${isLastMessage ? 'last-message' : ''}`}
    >
      <div
        className={`message-wrapper ${sender === "user" ? "user-message" : "other-message"}`}
      >
        {sender === "other" && (
          <Avatar className="message-avatar w-8 h-8">
            <AvatarImage src={senderAvatar || ""} alt={senderName || "Contact"} />
            <AvatarFallback>{senderName?.charAt(0).toUpperCase() || "?"}</AvatarFallback>
          </Avatar>
        )}
        
        <div className={`message-bubble ${sender === "user" ? "user-bubble" : "other-bubble"}`}>
          <div className="message-text">
            {text}
          </div>
          <div className="message-footer">
            <span className="message-timestamp">
              {timestamp}
            </span>
            <span 
              className="message-status"
              style={{ color: getStatusColor(status, sender) }}
            >
              {getStatusIcon(status)}
            </span>
          </div>
        </div>

        <MessageMenu
          messageId={id}
          messageText={text}
          isOwnMessage={isOwnMessage}
          onDelete={handleDelete}
          onCopy={handleCopy}
          onReport={handleReport}
        />

        {sender === "user" && (
          <Avatar className="message-avatar w-8 h-8">
            <AvatarImage src={currentUserAvatar || "https://placehold.co/32x32"} alt="You" />
            <AvatarFallback>{user?.firstName?.charAt(0).toUpperCase() || user?.username?.charAt(0).toUpperCase() || "U"}</AvatarFallback>
          </Avatar>
        )}
      </div>
    </div>
  );
});

SimpleMessageItem.displayName = 'SimpleMessageItem';

const SimpleMessagesList: React.FC<SimpleMessagesListProps> = ({
  messages,
  selectedContactAvatar,
  selectedContactName,
  currentUserAvatar,
  markMessageAsRead,
  onDeleteMessage,
  onReportMessage,
  onCopyMessage
}) => {
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const isInitialLoad = useRef(true);
  const previousMessageCount = useRef(0);

  useEffect(() => {
    if (messages.length === 0) return;

    if (isInitialLoad.current) {
      messagesEndRef.current?.scrollIntoView({ behavior: 'instant' });
      isInitialLoad.current = false;
      previousMessageCount.current = messages.length;
    } 
    else if (messages.length > previousMessageCount.current) {
      messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
      previousMessageCount.current = messages.length;
    }
  }, [messages.length]);

  useEffect(() => {
    isInitialLoad.current = true;
    previousMessageCount.current = 0;
  }, [selectedContactName, selectedContactAvatar]);

  return (
    <div className="simple-messages-container">
      <div className="simple-messages-list">
        {messages.map((message, index) => (
          <SimpleMessageItem
            key={message.id}
            id={message.id}
            sender={message.sender}
            text={message.text}
            timestamp={message.timestamp}
            status={message.status}
            senderAvatar={selectedContactAvatar}
            senderName={selectedContactName}
            currentUserAvatar={currentUserAvatar}
            isLastMessage={index === messages.length - 1}
            markMessageAsRead={markMessageAsRead}
            onDeleteMessage={onDeleteMessage}
            onReportMessage={onReportMessage}
            onCopyMessage={onCopyMessage}
          />
        ))}
        <div ref={messagesEndRef} />
      </div>
    </div>
  );
};

export default SimpleMessagesList; 