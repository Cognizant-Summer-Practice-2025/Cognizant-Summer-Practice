import { useState, useEffect, useCallback } from 'react';
import { Contact, loadEnhancedContactsFromStorage } from '@/lib/utils/contact-utils';
import { SearchUser } from '@/lib/user';
import { formatTimestamp, getValidTimestamp } from '@/lib/utils/message-utils';
import { Conversation } from '@/lib/messages';

interface UseContactManagerProps {
  conversations: Conversation[];
  currentConversation: Conversation | null;
  selectConversation: (conversation: Conversation) => Promise<void>;
  createConversation: (searchUser: SearchUser) => Promise<Conversation | null>;
  onContactSelect?: (contact: Contact) => void;
}

export const useContactManager = ({
  conversations,
  currentConversation,
  selectConversation,
  createConversation,
  onContactSelect,
}: UseContactManagerProps) => {
  const [selectedContact, setSelectedContact] = useState<Contact | null>(null);
  const [enhancedContacts] = useState<Map<string, Partial<Contact>>>(() => 
    loadEnhancedContactsFromStorage()
  );

  const getEnhancedContact = useCallback((conv: typeof conversations[0]): Contact => {
    const enhanced = enhancedContacts.get(conv.id);
    const timestamp = getValidTimestamp(
      conv.lastMessageTimestamp,
      conv.lastMessage?.createdAt,
      conv.updatedAt,
      conv.createdAt
    );
    
    return {
      id: conv.id,
      name: enhanced?.name || conv.otherUserName,
      avatar: enhanced?.avatar || conv.otherUserAvatar || "https://placehold.co/40x40",
      lastMessage: conv.lastMessage?.content || "No messages yet", 
      timestamp: formatTimestamp(timestamp),
      isActive: currentConversation?.id === conv.id,
      isOnline: conv.isOnline ?? false,
      unreadCount: conv.unreadCount,
      userId: enhanced?.userId || conv.otherUserId,
      professionalTitle: enhanced?.professionalTitle || conv.otherUserProfessionalTitle
    };
  }, [enhancedContacts, currentConversation?.id]);

  const contacts: Contact[] = conversations.map(getEnhancedContact);

  // Save enhanced contacts to localStorage
  useEffect(() => {
    if (enhancedContacts.size > 0) {
      try {
        const contactsObject = Object.fromEntries(enhancedContacts);
        localStorage.setItem('enhancedContacts', JSON.stringify(contactsObject));
      } catch {
        // Failed to save enhanced contacts to localStorage - continue silently
      }
    }
  }, [enhancedContacts]);

  // Auto-select first contact
  useEffect(() => {
    let cancelled = false;
    const autoSelectFirstContact = async () => {
      if (contacts.length > 0 && !selectedContact) {
        if (cancelled) return;
        const first = contacts[0];
        setSelectedContact(first);
        const conversation = conversations.find(conv => conv.id === first.id);
        if (conversation) {
          await selectConversation(conversation);
        }
      }
    };
    autoSelectFirstContact();
    return () => { cancelled = true; };
  }, [contacts, conversations, selectConversation, selectedContact?.id]);

  // Update selected contact when conversations change
  useEffect(() => {
    if (selectedContact && conversations.length > 0) {
      const currentConv = conversations.find(conv => conv.id === selectedContact.id);
      if (currentConv) {
        const updatedContact = getEnhancedContact(currentConv);
        // Only update if the relevant fields have actually changed
        if (
          selectedContact.isOnline !== updatedContact.isOnline ||
          selectedContact.lastMessage !== updatedContact.lastMessage ||
          selectedContact.timestamp !== updatedContact.timestamp
        ) {
          setSelectedContact(prev => ({
            ...prev!,
            isOnline: updatedContact.isOnline,
            lastMessage: updatedContact.lastMessage,
            timestamp: updatedContact.timestamp
          }));
        }
      }
    }
  }, [conversations, selectedContact, getEnhancedContact]);

  const handleSelectContact = async (contact: Contact) => {
    const conversation = conversations.find(conv => conv.id === contact.id);
    if (conversation) {
      setSelectedContact({
        ...contact,
        lastMessage: conversation.lastMessage?.content || "No messages yet",
        timestamp: formatTimestamp(conversation.updatedAt)
      });
      await selectConversation(conversation);
    } else {
      setSelectedContact(contact);
    }
    onContactSelect?.(contact);
  };

  const handleNewConversation = async (searchUser: SearchUser) => {
    try {
      const newConversation = await createConversation(searchUser);
      if (newConversation) {
        const newContact: Contact = {
          id: newConversation.id,
          name: searchUser.fullName,
          avatar: searchUser.avatarUrl || "https://placehold.co/40x40",
          lastMessage: "No messages yet",
          timestamp: formatTimestamp(newConversation.updatedAt),
          isActive: false,
          isOnline: false,
          unreadCount: 0,
          userId: newConversation.otherUserId,
          professionalTitle: searchUser.professionalTitle
        };
        
        setSelectedContact(newContact);
        await selectConversation(newConversation);
        onContactSelect?.(newContact);
      }
    } catch {
      // Failed to create conversation - error handling done by useMessages hook
    }
  };

  const clearSelectedContact = () => {
    setSelectedContact(null);
  };

  return {
    selectedContact,
    contacts,
    handleSelectContact,
    handleNewConversation,
    clearSelectedContact,
  };
}; 