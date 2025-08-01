import React from 'react';
import './style.css';

interface ContactItemProps {
  icon: string;
  text: string;
  type?: 'phone' | 'email' | 'location';
}

const ContactItem: React.FC<ContactItemProps> = ({ icon, text, type }) => {
  const handleClick = () => {
    switch (type) {
      case 'phone':
        window.open(`tel:${text.replace(/\D/g, '')}`);
        break;
      case 'email':
        window.open(`mailto:${text}`);
        break;
      case 'location':
        window.open(`https://maps.google.com/?q=${encodeURIComponent(text)}`);
        break;
      default:
        break;
    }
  };

  return (
    <div className="contact-item" onClick={handleClick}>
      <div className="contact-icon">
        <div className="contact-icon-text">{icon}</div>
      </div>
      <div className="contact-text-container">
        <div className="contact-text">{text}</div>
      </div>
    </div>
  );
};

const ContactSection = () => {
  const contactInfo = [
    { 
      icon: "📞", 
      text: "+1 (555) 123-4567",
      type: "phone" as const
    },
    { 
      icon: "✉️", 
      text: "alex.johnson@email.com",
      type: "email" as const
    },
    { 
      icon: "📍", 
      text: "San Francisco, CA, USA",
      type: "location" as const
    }
  ];

  return (
    <div className="contact-section">
      {contactInfo.map((contact, index) => (
        <ContactItem 
          key={index} 
          icon={contact.icon} 
          text={contact.text} 
          type={contact.type}
        />
      ))}
    </div>
  );
};

export default ContactSection;