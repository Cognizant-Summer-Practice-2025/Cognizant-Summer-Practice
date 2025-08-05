import React, { useCallback, useRef, useEffect, useMemo } from 'react';
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

// Message height estimation based on content length and layout
const estimateMessageHeight = (message: Message): number => {
  const baseHeight = 60; 
  const lineHeight = 24; 
  const maxWidth = Math.min(window.innerWidth * 0.7, 500); 
  const avgCharWidth = 9; 
  
  const charsPerLine = Math.floor(maxWidth / avgCharWidth);
  const estimatedLines = Math.max(1, Math.ceil(message.text.length / charsPerLine));

  const footerHeight = 25;
  const minMessageHeight = 80;
  const calculatedHeight = baseHeight + (estimatedLines * lineHeight) + footerHeight;
  
  return Math.max(minMessageHeight, calculatedHeight);
};

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
  const heightCache = useRef<Record<number, number>>({});


  // Memoize message heights to avoid recalculation
  const messageHeights = useMemo(() => {
    return messages.map((message, index) => {
      if (heightCache.current[index]) {
        return heightCache.current[index];
      }
      const estimatedHeight = estimateMessageHeight(message);
      heightCache.current[index] = estimatedHeight;
      return estimatedHeight;
    });
  }, [messages]);

  // Get item height for react-window
  const getItemSize = useCallback((index: number) => {
    const baseHeight = messageHeights[index] || 80;
    // Add fixed gap to all messages except the last one
    const isLastMessage = index === messages.length - 1;
    return isLastMessage ? baseHeight : baseHeight + MESSAGE_GAP;
  }, [messageHeights, messages.length]);

  // Scroll to bottom when new messages arrive
  useEffect(() => {
    if (listRef.current && messages.length > 0) {
      listRef.current.scrollToItem(messages.length - 1, 'end');
    }
  }, [messages.length]);

  // Reset height cache when messages change significantly
  useEffect(() => {
    heightCache.current = {};
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