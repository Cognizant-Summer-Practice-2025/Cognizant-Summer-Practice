import React, { useState, useRef, useEffect } from "react";
import { Send } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { message } from "@/components/ui/toast";
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
    onBackToSidebar?: () => void;
    isMobile?: boolean;
    onDeleteMessage?: (messageId: string) => Promise<void>;
    onReportMessage?: (messageId: string) => Promise<void>;
}

const Chat: React.FC<ChatProps> = ({
                                       messages,
                                       selectedContact,
                                       currentUserAvatar,
                                       onSendMessage,
                                       sendingMessage = false,
                                       onDeleteConversation,
                                       markMessageAsRead,
                                       onBackToSidebar,
                                       isMobile = false,
                                       onDeleteMessage,
                                       onReportMessage
                                   }) => {
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

    const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            handleSendMessage();
        }
    };

    // Handle copy message
    const handleCopyMessage = async (text: string) => {
        try {
            await navigator.clipboard.writeText(text);
            message.success('Message copied to clipboard');
        } catch (error) {
            console.error('Failed to copy message:', error);
            message.error('Failed to copy message');
        }
    };

    // Handle delete message
    const handleDeleteMessage = async (messageId: string) => {
        if (onDeleteMessage) {
            try {
                await onDeleteMessage(messageId);
                message.success('Message deleted successfully');
            } catch (error) {
                console.error('Failed to delete message:', error);
                message.error('Failed to delete message');
            }
        } else {
            console.log('Delete message functionality not implemented yet');
            message.info('Delete functionality will be available soon');
        }
    };

    // Handle report message
    const handleReportMessage = async (messageId: string) => {
        if (onReportMessage) {
            try {
                await onReportMessage(messageId);
                message.success('Message reported successfully');
            } catch (error) {
                console.error('Failed to report message:', error);
                message.error('Failed to report message');
            }
        } else {
            console.log('Report message functionality not implemented yet');
            message.info('Report functionality will be available soon');
        }
    };

    return (
        <div className="chat-container">
            {/* Messages Area */}
            <ChatHeader
                selectedContact={selectedContact}
                onDeleteConversation={onDeleteConversation}
                onBackToSidebar={onBackToSidebar}
                isMobile={isMobile}
            />
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
                        onDeleteMessage={handleDeleteMessage}
                        onReportMessage={handleReportMessage}
                        onCopyMessage={handleCopyMessage}
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
                        onKeyDown={handleKeyDown}
                        className="message-input"
                    />
                    <Button
                        onClick={handleSendMessage}
                        disabled={sendingMessage || !newMessage.trim()}
                        className="send-button"
                        size="icon"
                    >
                        {sendingMessage ? (
                            <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                        ) : (
                            <Send className="h-4 w-4" />
                        )}
                    </Button>
                </div>
            </div>
        </div>
    );
};

export default Chat;