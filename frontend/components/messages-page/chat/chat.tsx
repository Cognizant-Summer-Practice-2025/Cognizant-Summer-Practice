import React, { useState } from "react";
import { Avatar, Button, Input } from "antd";
import { SendOutlined} from "@ant-design/icons";
import ChatHeader from "../chat-header/chat-header";
import "./style.css";

interface Message {
  id: string;
  sender: "user" | "other";
  text: string;
  timestamp: string;
  status: "read" | "delivered" | "sent";
}

interface Contact {
  id: string;
  name: string;
  avatar: string;
  lastMessage: string;
  timestamp: string;
  isActive?: boolean;
  isOnline?: boolean;
  userId?: string;
  professionalTitle?: string;
}

interface ChatProps {
  messages: Message[];
  selectedContact: Contact;
  currentUserAvatar?: string;
  onSendMessage?: (content: string) => Promise<void>;
  sendingMessage?: boolean;
}

const Chat: React.FC<ChatProps> = ({ messages, selectedContact, currentUserAvatar, onSendMessage, sendingMessage = false }) => {
  const [newMessage, setNewMessage] = useState("");

  const getStatusIcon = (status: string) => {
    switch (status) {
      case "read":
        return "✓✓"; // Double check for read
      case "delivered":
        return "✓✓"; // Double check for delivered
      case "sent":
        return "✓"; // Single check for sent
      default:
        return "";
    }
  };

  const getStatusColor = (status: string, sender: string) => {
    if (sender === "user") {
      // Only show status for user's own messages
      switch (status) {
        case "read":
          return "#22c55e"; // Green for read (seen)
        case "delivered":
          return "rgba(255, 255, 255, 0.7)"; // Light white for delivered
        case "sent":
          return "rgba(255, 255, 255, 0.5)"; // Faded white for sent
        default:
          return "rgba(255, 255, 255, 0.8)";
      }
    }
    return "transparent"; // Hide status for other user's messages
  };

  const handleSendMessage = async () => {
    if (newMessage.trim() && onSendMessage && !sendingMessage) {
      try {
        await onSendMessage(newMessage.trim());
        setNewMessage("");
      } catch (error) {
        console.error('Failed to send message:', error);
        // Message will stay in input field if sending fails
      }
    }
  };

  return (
    <div className="chat-container">
      {/* Messages Area */}
      <ChatHeader selectedContact={selectedContact} />
      <div className="messages-area">
        {messages.map((message) => (
          <div
            key={message.id}
            className={`message-wrapper ${message.sender === "user" ? "user-message" : "other-message"}`}
          >
            {message.sender === "other" && (
              <Avatar
                size={32}
                src={selectedContact.avatar}
                className="message-avatar"
              >
                {selectedContact.name.charAt(0).toUpperCase()}
              </Avatar>
            )}
            
            <div className={`message-bubble ${message.sender === "user" ? "user-bubble" : "other-bubble"}`}>
              <div className="message-text">
                {message.text}
              </div>
              <div className="message-footer">
                <span className="message-timestamp">
                  {message.timestamp}
                </span>
                <span 
                  className="message-status"
                  style={{ color: getStatusColor(message.status, message.sender) }}
                >
                  {getStatusIcon(message.status)}
                </span>
              </div>
            </div>

            {message.sender === "user" && (
              <Avatar
                size={32}
                src={currentUserAvatar || "https://placehold.co/32x32"}
                className="message-avatar"
              >
                {!currentUserAvatar && "You"}
              </Avatar>
            )}
          </div>
        ))}
      </div>

      {/* Message Input */}
      <div className="message-input-container">
        <div className="message-input-wrapper">
          <Input
            placeholder="Type your message..."
            value={newMessage}
            onChange={(e) => setNewMessage(e.target.value)}
            onPressEnter={handleSendMessage}
            className="message-input"
          />
          <Button
            type="primary"
            icon={<SendOutlined />}
            onClick={handleSendMessage}
            disabled={sendingMessage || !newMessage.trim()}
            loading={sendingMessage}
            className="send-button"
          />
        </div>
      </div>
    </div>
  );
};

export default Chat;