import React from 'react';
import Avatar from '../avatar/avatar';

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
  style
}) => {
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
    <div style={style} className="virtualized-message-container">
      <div
        className={`message-wrapper ${sender === "user" ? "user-message" : "other-message"}`}
      >
        {sender === "other" && (
          <Avatar
            size={32}
            src={senderAvatar}
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
          >
            {!currentUserAvatar && "You"}
          </Avatar>
        )}
      </div>
    </div>
  );
});

VirtualizedMessageItem.displayName = 'VirtualizedMessageItem';

export default VirtualizedMessageItem; 