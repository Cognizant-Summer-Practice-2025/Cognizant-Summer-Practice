"use client";

import React from 'react';
import Image from 'next/image';
import { BasicInfo } from '@/lib/portfolio';
import { Mail, MapPin } from 'lucide-react';

interface HeaderProps {
  basicInfo: BasicInfo;
}

export function Header({ basicInfo }: HeaderProps) {
  const contactInfo = [
    { icon: Mail, text: 'contact@example.com', href: 'mailto:contact@example.com' },
    { icon: MapPin, text: basicInfo.location || 'Location', href: '#' }
  ];

  return (
    <div className="prof-header">
      <div className="prof-header-main">
        <div className="prof-header-content">
          <div className="prof-header-text">
            <h1 className="prof-header-name">{basicInfo.name}</h1>
            <h2 className="prof-header-title">{basicInfo.title}</h2>
            <p className="prof-header-bio">{basicInfo.bio}</p>
            
            <div className="prof-header-contact">
              {contactInfo.map((contact, index) => (
                <a 
                  key={index}
                  href={contact.href}
                  className="prof-contact-item"
                >
                  <contact.icon size={16} />
                  <span>{contact.text}</span>
                </a>
              ))}
            </div>
          </div>

          <div className="prof-header-image">
            <div className="prof-image-container">
              <Image 
                src={basicInfo.profileImage || 'https://placehold.co/300x300'} 
                alt={basicInfo.name}
                className="prof-profile-image"
                width={300}
                height={300}
              />
              <div className="prof-image-overlay"></div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
} 