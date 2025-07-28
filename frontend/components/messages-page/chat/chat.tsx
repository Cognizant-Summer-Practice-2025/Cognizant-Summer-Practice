import React, { useState, useRef, useEffect } from "react";
import { Button, Input } from "antd";
import { SendOutlined} from "@ant-design/icons";
import ChatHeader from "../chat-header/chat-header";
import VirtualizedMessagesList from "./virtualized-messages-list";
import "./style.css";
import "./virtualized-styles.css";

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
  onDeleteConversation?: (conversationId: string) => Promise<void>;
  markMessageAsRead?: (messageId: string, userId: string) => Promise<void>;
}

const Chat: React.FC<ChatProps> = ({ messages, selectedContact, currentUserAvatar, onSendMessage, sendingMessage = false, onDeleteConversation, markMessageAsRead }) => {
  const [newMessage, setNewMessage] = useState("");
  const messagesContainerRef = useRef<HTMLDivElement>(null);
  const [containerSize, setContainerSize] = useState({ width: 0, height: 0 });



  // Handle container resize for virtualized list
  useEffect(() => {
    const updateSize = () => {
      if (messagesContainerRef.current) {
        const { width, height } = messagesContainerRef.current.getBoundingClientRect();
        setContainerSize({ width, height });
      }
    };

    updateSize();
    
    const resizeObserver = new ResizeObserver(updateSize);
    if (messagesContainerRef.current) {
      resizeObserver.observe(messagesContainerRef.current);
    }

    return () => {
      resizeObserver.disconnect();
    };
  }, []);

  const handleSendMessage = async () => {
    if (newMessage.trim() && onSendMessage && !sendingMessage) {
      try {
        await onSendMessage(newMessage.trim());
        setNewMessage("");
      } catch (error) {
        console.error('Failed to send message:', error);
      }
    }
  };

  return (
    <div className="chat-container">
      {/* Messages Area */}
      <ChatHeader selectedContact={selectedContact} onDeleteConversation={onDeleteConversation} />
      <div 
        ref={messagesContainerRef}
        className="messages-area virtualized-messages-area"
      >
        {containerSize.height > 0 && (
          <VirtualizedMessagesList
            messages={messages}
            selectedContactAvatar={selectedContact.avatar}
            selectedContactName={selectedContact.name}
            currentUserAvatar={currentUserAvatar}
            height={containerSize.height}
            width={containerSize.width}
            markMessageAsRead={markMessageAsRead}
          />
        )}
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