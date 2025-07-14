"use client";

import React, { useState } from 'react';
import MessagesSidebar from '@/components/messages-page/sidebar/sidebar';
//import MessagesChat from '@/components/messages-page/chat/messages-chat';
//import MessagesHeader from '@/components/messages-page/header/messages-header';
import './style.css';

interface Message {
  id: string;
  sender: 'user' | 'other';
  text: string;
  timestamp: string;
  status: 'sent' | 'delivered' | 'read';
}

interface Contact {
  id: string;
  name: string;
  avatar: string;
  lastMessage: string;
  timestamp: string;
  isActive?: boolean;
  isOnline?: boolean;
  unreadCount?: number;
}

const MessagesPage = () => {
  const [selectedContact, setSelectedContact] = useState<Contact | null>(null);
  const [contacts] = useState<Contact[]>([
    {
      id: '1',
      name: 'Sarah Wilson',
      avatar: 'https://placehold.co/40x40',
      lastMessage: 'Thanks for checking out my portfolio',
      timestamp: '2m',
      isActive: true,
      isOnline: true
    },
    {
      id: '2',
      name: 'Mike Chen',
      avatar: 'https://placehold.co/40x40',
      lastMessage: 'Great work on the ML project! 🔥',
      timestamp: '1h',
      isOnline: true
    },
    {
      id: '3',
      name: 'Alex Johnson',
      avatar: 'https://placehold.co/40x40',
      lastMessage: 'Would love to collaborate on a proj…',
      timestamp: '3h',
      isOnline: false
    },
    {
      id: '4',
      name: 'Emma Davis',
      avatar: 'https://placehold.co/40x40',
      lastMessage: 'Your React components are amazi…',
      timestamp: '1d',
      isOnline: false
    }
  ]);

  const [messages] = useState<Message[]>([
    {
      id: '1',
      sender: 'other',
      text: "Hi! I saw your portfolio and I'm really impressed with your full-stack projects. The e-commerce platform looks amazing!",
      timestamp: 'Yesterday, 2:30 PM',
      status: 'read'
    },
    {
      id: '2',
      sender: 'user',
      text: "Thank you so much! I really appreciate the feedback. I checked out your design portfolio too - your UI work is incredible!",
      timestamp: 'Yesterday, 3:15 PM',
      status: 'read'
    },
    {
      id: '3',
      sender: 'other',
      text: "Thanks! I was wondering if you'd be interested in collaborating on a project? I have a client who needs both design and development work.",
      timestamp: 'Today, 10:20 AM',
      status: 'read'
    },
    {
      id: '4',
      sender: 'user',
      text: "That sounds really interesting! I'd love to hear more about it. What kind of project is it?",
      timestamp: 'Today, 11:45 AM',
      status: 'read'
    },
    {
      id: '5',
      sender: 'other',
      text: "It's a fintech startup that needs a complete web platform. They want a modern design with React frontend and Node.js backend. Perfect for both our skills!",
      timestamp: 'Today, 12:30 PM',
      status: 'delivered'
    }
  ]);

  React.useEffect(() => {
    if (contacts.length > 0 && !selectedContact) {
      setSelectedContact(contacts[0]);
    }
  }, [contacts, selectedContact]);

  return (
    <div className="messages-page">
      <div className="messages-container">
        <MessagesSidebar 
          contacts={contacts}
          selectedContact={selectedContact}
          onSelectContact={setSelectedContact}
        />
      </div>
    </div>
  );
};

export default MessagesPage;