import React, { useState } from "react";
import { Avatar, Button, Input } from "antd";
import { SendOutlined, PlusOutlined } from "@ant-design/icons";
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
  isOnline: boolean;
}

interface ChatProps {
  messages: Message[];
  selectedContact: Contact;
}

const Chat: React.FC<ChatProps> = ({ messages, selectedContact }) => {
  const [newMessage, setNewMessage] = useState("");

  const getStatusIcon = (status: string) => {
    switch (status) {
      case "read":
        return "√√";
      case "delivered":
        return "√√";
      case "sent":
        return "√";
      default:
        return "";
    }
  };

  const getStatusColor = (status: string, sender: string) => {
    if (sender === "user") {
      return status === "read" ? "rgba(255, 255, 255, 0.8)" : "rgba(255, 255, 255, 0.8)";
    }
    return status === "read" ? "#22c55e" : "#6b7280";
  };

  const handleSendMessage = () => {
    if (newMessage.trim()) {
      // Handle sending message logic here
      setNewMessage("");
    }
  };

  return (
    <div className="chat-container">
      {/* Messages Area */}
      <div className="messages-area">
        {messages.map((message, index) => (
          <div
            key={message.id}
            className={`message-wrapper ${message.sender === "user" ? "user-message" : "other-message"}`}
          >
            {message.sender === "other" && (
              <Avatar
                size={32}
                src={selectedContact.avatar}
                className="message-avatar"
              />
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
                src="https://placehold.co/32x32"
                className="message-avatar"
              />
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
            className="send-button"
          />
        </div>
      </div>
    </div>
  );
};

export default Chat;