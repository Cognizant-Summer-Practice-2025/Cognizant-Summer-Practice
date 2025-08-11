import React from 'react';
import { ContactInfo } from '@/lib/portfolio';

interface ContactProps {
  data: ContactInfo;
}

export function Contact({ data: contacts }: ContactProps) {
  return (
    <section className="gb-contact">
      <h3 className="section-title">Contact</h3>
      <div className="contact-info">
        <div className="contact-item">
          <div className="contact-label">Email</div>
          <div className="contact-value">
            <a href={`mailto:${contacts.email}`} className="contact-link">
              {contacts.email}
            </a>
          </div>
        </div>
        
        {contacts.location && (
          <div className="contact-item">
            <div className="contact-label">Location</div>
            <div className="contact-value">{contacts.location}</div>
          </div>
        )}
      </div>
    </section>
  );
} 