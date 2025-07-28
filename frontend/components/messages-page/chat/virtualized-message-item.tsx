import React, { useEffect, useRef } from 'react';
import { Avatar } from 'antd';
import { useUser } from '@/lib/contexts/user-context';

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
  markMessageAsRead?: (messageId: string, userId: string) => Promise<void>;
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
  markMessageAsRead
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

  return (
    <div ref={messageRef} style={style} className="virtualized-message-container">
      <div
        className={`message-wrapper ${sender === "user" ? "user-message" : "other-message"}`}
      >
        {sender === "other" && (
          <Avatar
            size={32}
            src={senderAvatar || ""}
            className="message-avatar"
          >
            {senderName?.charAt(0).toUpperCase() || "?"}
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

        {sender === "user" && (
          <Avatar
            size={32}
            src={currentUserAvatar || "https://placehold.co/32x32"}
            className="message-avatar"
          />
        )}
      </div>
    </div>
  );
});

VirtualizedMessageItem.displayName = 'VirtualizedMessageItem';

export default VirtualizedMessageItem; 