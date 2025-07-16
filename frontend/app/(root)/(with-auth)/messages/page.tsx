"use client";
import React, { useState, useEffect } from "react";
import Sidebar from "@/components/messages-page/sidebar/sidebar";
import Chat from "@/components/messages-page/chat/chat";
import MessagesHeader from "@/components/header";
import "./style.css";

interface Contact {
  id: string;
  name: string;
  avatar: string;
  lastMessage: string;
  timestamp: string;
  isActive?: boolean;
  isOnline: boolean;
}

interface Message {
  id: string;
  sender: "user" | "other";
  text: string;
  timestamp: string;
  status: "read" | "delivered" | "sent";
}

const MessagesPage = () => {
  const [selectedContact, setSelectedContact] = useState<Contact | null>(null);
  const [contacts] = useState<Contact[]>([
    {
      id: "1",
      name: "Sarah Wilson",
      avatar: "https://placehold.co/40x40",
      lastMessage: "Thanks for checking out my portfolio",
      timestamp: "2m",
      isActive: true,
      isOnline: true,
    },
    {
      id: "2",
      name: "Mike Chen",
      avatar: "https://placehold.co/40x40",
      lastMessage: "Great work on the ML project! 🔥",
      timestamp: "1h",
      isOnline: true,
    },
    {
      id: "3",
      name: "Alex Johnson",
      avatar: "https://placehold.co/40x40",
      lastMessage: "Would love to collaborate on a proj…",
      timestamp: "3h",
      isOnline: false,
    },
    {
      id: "4",
      name: "Emma Davis",
      avatar: "https://placehold.co/40x40",
      lastMessage: "Your React components are amazi…",
      timestamp: "1d",
      isOnline: false,
    },
  ]);

  // Messages organized by contact ID
  const [allMessages] = useState<Record<string, Message[]>>({
    "1": [ // Sarah Wilson's messages
      {
        id: "1",
        sender: "other",
        text: "Hi! I saw your portfolio and I'm really impressed with your full-stack projects. The e-commerce platform looks amazing!",
        timestamp: "Yesterday, 2:30 PM",
        status: "read",
      },
      {
        id: "2",
        sender: "user",
        text: "Thank you so much! I really appreciate the feedback. I checked out your design portfolio too - your UI work is incredible!",
        timestamp: "Yesterday, 3:15 PM",
        status: "read",
      },
      {
        id: "3",
        sender: "other",
        text: "Thanks! I was wondering if you'd be interested in collaborating on a project? I have a client who needs both design and development work.",
        timestamp: "Today, 10:20 AM",
        status: "read",
      },
      {
        id: "4",
        sender: "user",
        text: "That sounds really interesting! I'd love to hear more about it. What kind of project is it?",
        timestamp: "Today, 11:45 AM",
        status: "read",
      },
      {
        id: "5",
        sender: "other",
        text: "It's a fintech startup that needs a complete web platform. They want a modern design with React frontend and Node.js backend. Perfect for both our skills!",
        timestamp: "Today, 12:30 PM",
        status: "delivered",
      },
    ],
    "2": [ // Mike Chen's messages
      {
        id: "1",
        sender: "other",
        text: "Hey! Just saw your latest ML project on GitHub. The neural network implementation is really clean!",
        timestamp: "Today, 9:15 AM",
        status: "read",
      },
      {
        id: "2",
        sender: "user",
        text: "Thanks Mike! I spent a lot of time optimizing the training pipeline. How's your computer vision project going?",
        timestamp: "Today, 9:30 AM",
        status: "read",
      },
      {
        id: "3",
        sender: "other",
        text: "Great work on the ML project! 🔥 We should definitely collaborate on something soon.",
        timestamp: "Today, 10:45 AM",
        status: "delivered",
      },
    ],
    "3": [ // Alex Johnson's messages
      {
        id: "1",
        sender: "other",
        text: "Would love to collaborate on a project! I have some ideas for a new startup.",
        timestamp: "Yesterday, 4:20 PM",
        status: "read",
      },
      {
        id: "2",
        sender: "user",
        text: "That sounds interesting! What kind of startup are you thinking about?",
        timestamp: "Yesterday, 5:10 PM",
        status: "read",
      },
      {
        id: "3",
        sender: "other",
        text: "It's in the EdTech space. Think personalized learning platforms with AI integration.",
        timestamp: "Today, 8:30 AM",
        status: "delivered",
      },
    ],
    "4": [ // Emma Davis's messages
      {
        id: "1",
        sender: "other",
        text: "Your React components are amazing! The attention to detail in the UI is impressive.",
        timestamp: "2 days ago, 3:45 PM",
        status: "read",
      },
      {
        id: "2",
        sender: "user",
        text: "Thank you Emma! I really focus on creating reusable and accessible components.",
        timestamp: "2 days ago, 4:20 PM",
        status: "read",
      },
      {
        id: "3",
        sender: "other",
        text: "I'd love to learn more about your component architecture. Do you have any documentation?",
        timestamp: "Yesterday, 2:15 PM",
        status: "read",
      },
    ],
  });

  // Get current messages based on selected contact
  const currentMessages = selectedContact ? allMessages[selectedContact.id] || [] : [];

  useEffect(() => {
    if (contacts.length > 0 && !selectedContact) {
      setSelectedContact(contacts[0]);
    }
  }, [contacts, selectedContact]);

  const handleSelectContact = (contact: Contact) => {
    setSelectedContact(contact);
  };

  return (
    <div className="messages-page" style={{ display: "flex", height: "100vh" }}>
      <div
        className="messages-sidebar"
        style={{
          width: 320,
          borderRight: "1px solid #eee",
          background: "#fafafa",
        }}
      >
        <Sidebar
          contacts={contacts}
          selectedContact={selectedContact}
          onSelectContact={handleSelectContact}
        />
      </div>

      <div
        className="messages-chat"
        style={{ flex: 1, display: "flex", flexDirection: "column" }}
      >
        <MessagesHeader selectedContact={selectedContact} />
        {selectedContact ? (
          <Chat messages={currentMessages} selectedContact={selectedContact} />
        ) : (
          <div style={{ padding: 32, color: "#888" }}>
            Select a contact to start chatting
          </div>
        )}
      </div>
    </div>
  );
};

export default MessagesPage;