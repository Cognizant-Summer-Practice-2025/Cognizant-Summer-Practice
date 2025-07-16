import React, { useState } from "react";
import { Avatar, Button, Dropdown } from "antd";
import type { MenuProps } from 'antd';
import { UserOutlined, MoreOutlined } from "@ant-design/icons";
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

interface ChatHeaderProps {
  selectedContact: Contact;
}

const ChatHeader: React.FC<ChatHeaderProps> = ({ selectedContact }) => {
  const [dropdownOpen, setDropdownOpen] = useState(false);

  const handleMenuClick: MenuProps['onClick'] = (e) => {
    const { key } = e;
    
    switch (key) {
      case 'block':
        console.log('Block user:', selectedContact.name);
        // Add block user logic here
        break;
      case 'report':
        console.log('Report user:', selectedContact.name);
        // Add report user logic here
        break;
      case 'mute':
        console.log('Mute user:', selectedContact.name);
        // Add mute user logic here
        break;
      case 'delete':
        console.log('Delete conversation with:', selectedContact.name);
        // Add delete conversation logic here
        break;
      default:
        break;
    }
    
    setDropdownOpen(false);
  };

  const menuItems: MenuProps['items'] = [
    {
      key: 'mute',
      label: 'Mute notifications',
      icon: '🔇',
    },
    {
      key: 'block',
      label: 'Block user',
      icon: '🚫',
      danger: true,
    },
    {
      key: 'report',
      label: 'Report user',
      icon: '⚠️',
      danger: true,
    },
    {
      type: 'divider',
    },
    {
      key: 'delete',
      label: 'Delete conversation',
      icon: '🗑️',
      danger: true,
    },
  ];

  const handleViewProfile = () => {
    console.log('View profile:', selectedContact.name);
    // Add view profile logic here - could open a modal or navigate to profile page
  };

  return (
    <div className="chat-header">
      <div className="chat-header-left">
        <Avatar 
          size={40} 
          src={selectedContact.avatar} 
          className="contact-avatar"
        />
        <div className="contact-info">
          <h3 className="contact-name">{selectedContact.name}</h3>
          <div className="contact-status">
            <span className="contact-role">UI/UX Designer</span>
            <span className="status-separator">•</span>
            <span className={`online-status ${selectedContact.isOnline ? 'online' : 'offline'}`}>
              <span className="status-dot"></span>
              {selectedContact.isOnline ? 'Online' : 'Offline'}
            </span>
          </div>
        </div>
      </div>
      
      <div className="chat-header-right">
        <Button 
          className="view-profile-btn"
          onClick={handleViewProfile}
          icon={<UserOutlined />}
        >
          View Profile
        </Button>
        
        <Dropdown
          menu={{ items: menuItems, onClick: handleMenuClick }}
          trigger={['click']}
          open={dropdownOpen}
          onOpenChange={setDropdownOpen}
          placement="bottomRight"
        >
          <Button 
            className="more-options-btn"
            icon={<MoreOutlined />}
            onClick={(e) => e.preventDefault()}
          />
        </Dropdown>
      </div>
    </div>
  );
};

export default ChatHeader;