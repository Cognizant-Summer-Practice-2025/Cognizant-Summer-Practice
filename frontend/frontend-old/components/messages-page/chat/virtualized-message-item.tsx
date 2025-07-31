import React, { useEffect, useRef } from 'react';
import { Avatar, AvatarImage, AvatarFallback } from '@/components/ui/avatar';
import { useUser } from '@/lib/contexts/user-context';
import MessageMenu from '@/components/messages-page/message-menu/message-menu';

export interface VirtualizedMessageProps {
  id: string;
  sender: "user" | "other";
  text: string;
  timestamp: string;
  status: "read" | "delivered" | "sent";
  senderAvatar?: string;
  senderName?: string;
  currentUserAvatar?: string;
  style?: React.CSSProperties; // For react-window positioning
  isLastMessage?: boolean; // To conditionally apply margin
  markMessageAsRead?: (messageId: string, userId: string) => Promise<void>;
  onDeleteMessage?: (messageId: string) => Promise<void>;
  onReportMessage?: (messageId: string) => Promise<void>;
  onCopyMessage?: (text: string) => void;
}

const VirtualizedMessageItem = React.memo<VirtualizedMessageProps>(({
  id,
  sender,
  text,
  timestamp,
  status,
  senderAvatar,
  senderName,
  currentUserAvatar,
  style,
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
    // Only mark as read if:
    // 1. Message is from another user (sender !== "user")
    // 2. Message is not already read
    // 3. We have a markMessageAsRead function
    // 4. We have a current user
    if (sender !== "user" && status !== "read" && markMessageAsRead && user?.id) {
      const observer = new IntersectionObserver(
        (entries) => {
          entries.forEach((entry) => {
            if (entry.isIntersecting) {
              console.log(`Message ${id} became visible, marking as read...`);
              // Mark message as read when it becomes visible
              markMessageAsRead(id, user.id).catch((error) => {
                console.error('Failed to mark message as read:', error);
              });
              // Stop observing after marking as read
              observer.unobserve(entry.target);
            }
          });
        },
        {
          threshold: 0.3, // Mark as read when 30% visible (more aggressive for testing)
          rootMargin: '0px 0px -20px 0px' // Require message to be within viewport
        }
      );

      if (messageRef.current) {
        observer.observe(messageRef.current);
        console.log(`Started observing message ${id} for visibility`);
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

  // Handle copy action with fallback
  const handleCopy = (text: string) => {
    if (onCopyMessage) {
      onCopyMessage(text);
    } else {
      // Default copy behavior
      navigator.clipboard.writeText(text).then(() => {
        console.log('Message copied to clipboard');
        // You might want to show a toast notification here
      }).catch(err => {
        console.error('Failed to copy message: ', err);
      });
    }
  };

  // Handle delete action
  const handleDelete = async (messageId: string) => {
    if (onDeleteMessage) {
      await onDeleteMessage(messageId);
    } else {
      console.log('Delete functionality not implemented');
    }
  };

  // Handle report action
  const handleReport = async (messageId: string) => {
    if (onReportMessage) {
      await onReportMessage(messageId);
    } else {
      console.log('Report functionality not implemented');
    }
  };

  const isOwnMessage = sender === "user";

  return (
    <div 
      ref={messageRef} 
      style={style} 
      className={`virtualized-message-container ${isLastMessage ? 'last-message' : ''}`}
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

        {/* Message Menu - positioned via CSS order */}
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

VirtualizedMessageItem.displayName = 'VirtualizedMessageItem';

export default VirtualizedMessageItem; 