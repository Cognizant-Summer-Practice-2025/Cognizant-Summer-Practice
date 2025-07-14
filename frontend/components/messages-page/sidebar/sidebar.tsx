import React from 'react';
import './style.css';

interface Contact{
    id: string;
    name: string;
    avatar: string;
    lastMessage: string;
    timestamp: string;
    isActive?: boolean;
    isOnline?: boolean;
    unreadCount?: number;
}

interface MessagesSidebarProps {
    contacts: Contact[];
    selectedContact: Contact | null;
    onSelectContact: (contact: Contact) => void;
}

const ContactItem: React.FC<{
    contact: Contact;
    isSelected: boolean;
    onClick: () => void;
}> = ({contact, isSelected, onClick}) => {
    return (
        <div 
      className={`contact-item ${isSelected ? 'contact-item-active' : ''}`}
      onClick={onClick}
    >
      <div className="contact-avatar-container">
        <img 
          className="contact-avatar" 
          src={contact.avatar} 
          alt={contact.name}
        />
      </div>
      <div className="contact-details">
        <div className="contact-header">
          <div className="contact-name">{contact.name}</div>
          <div className="contact-timestamp">{contact.timestamp}</div>
        </div>
        <div className="contact-message">
          {contact.lastMessage}
        </div>
        <div className="contact-status">
          <div className="message-status">√√</div>
        </div>
      </div>
    </div>
    );
}

const MessagesSidebar: React.FC<MessagesSidebarProps> = ({
  contacts,
  selectedContact,
  onSelectContact
}) => {
  return (
    <div className="messages-sidebar">
      <div className="sidebar-header">
        <div className="sidebar-title">Messages</div>
        <div className="new-message-button">
          <div className="new-message-icon">+</div>
          <div className="new-message-text">New</div>
        </div>
      </div>
      
      <div className="contacts-list">
        {contacts.map((contact) => (
          <ContactItem
            key={contact.id}
            contact={contact}
            isSelected={selectedContact?.id === contact.id}
            onClick={() => onSelectContact(contact)}
          />
        ))}
      </div>
    </div>
  );
};

export default MessagesSidebar;