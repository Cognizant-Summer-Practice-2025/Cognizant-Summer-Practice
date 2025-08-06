import React, { useCallback, useRef, useEffect } from 'react';
import { VariableSizeList as List } from 'react-window';
import VirtualizedMessageItem from './virtualized-message-item';

export interface Message {
  id: string;
  sender: "user" | "other";
  text: string;
  timestamp: string;
  status: "read" | "delivered" | "sent";
}

interface VirtualizedMessagesListProps {
  messages: Message[];
  selectedContactAvatar?: string;
  selectedContactName?: string;
  currentUserAvatar?: string;
  height: number;
  width: number;
  markMessageAsRead?: (messageId: string, userId: string) => Promise<void>;
  onDeleteMessage?: (messageId: string) => Promise<void>;
  onReportMessage?: (messageId: string, reason: string) => Promise<void>;
  onCopyMessage?: (text: string) => void;
}

const MESSAGE_GAP = 12;
const UNIFORM_MESSAGE_HEIGHT = 100; // Base uniform height for virtualization

const VirtualizedMessagesList: React.FC<VirtualizedMessagesListProps> = ({
  messages,
  selectedContactAvatar,
  selectedContactName,
  currentUserAvatar,
  height,
  width,
  markMessageAsRead,
  onDeleteMessage,
  onReportMessage,
  onCopyMessage
}) => {
  const listRef = useRef<List>(null);

  // Simple uniform height calculation for virtualization
  // CSS will handle the actual visual sizing
  const getItemSize = useCallback((index: number) => {
    const isLastMessage = index === messages.length - 1;
    return isLastMessage ? UNIFORM_MESSAGE_HEIGHT : UNIFORM_MESSAGE_HEIGHT + MESSAGE_GAP;
  }, [messages.length]);

  // Scroll to bottom when new messages arrive
  useEffect(() => {
    if (listRef.current && messages.length > 0) {
      listRef.current.scrollToItem(messages.length - 1, 'end');
    }
  }, [messages.length]);

  // Reset cache when messages change
  useEffect(() => {
    if (listRef.current) {
      listRef.current.resetAfterIndex(0);
    }
  }, [messages]);

  // Render item function for react-window
  const Item = useCallback(({ index, style }: { index: number; style: React.CSSProperties }) => {
    const message = messages[index];
    if (!message) return null;

    const isLastMessage = index === messages.length - 1;

    return (
      <VirtualizedMessageItem
        key={message.id}
        id={message.id}
        sender={message.sender}
        text={message.text}
        timestamp={message.timestamp}
        status={message.status}
        senderAvatar={selectedContactAvatar}
        senderName={selectedContactName}
        currentUserAvatar={currentUserAvatar}
        style={style}
        isLastMessage={isLastMessage}
        markMessageAsRead={markMessageAsRead}
        onDeleteMessage={onDeleteMessage}
        onReportMessage={onReportMessage}
        onCopyMessage={onCopyMessage}
      />
    );
  }, [messages, selectedContactAvatar, selectedContactName, currentUserAvatar, markMessageAsRead, onDeleteMessage, onReportMessage, onCopyMessage]);

  return (
    <div className="virtualized-messages-container">
      <List
        ref={listRef}
        height={height}
        width={width}
        itemCount={messages.length}
        itemSize={getItemSize}
        overscanCount={5} // Render 5 extra items above/below viewport
        className="virtualized-messages-list"
      >
        {Item}
      </List>
    </div>
  );
};

export default VirtualizedMessagesList; 