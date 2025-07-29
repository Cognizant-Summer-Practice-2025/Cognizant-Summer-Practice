"use client";

import React, { useState } from 'react';
import { ContactInfo, BasicInfo } from '@/lib/portfolio';
import { Card } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Mail, MapPin, Send, MessageCircle, CheckCircle, AlertCircle, Loader2 } from 'lucide-react';
import emailjs from '@emailjs/browser';

interface ContactProps {
  data: ContactInfo;
  basicInfo: BasicInfo;
}

export function Contact({ data, basicInfo }: ContactProps) {
  const [formData, setFormData] = useState({
    name: '',
    email: '',
    subject: '',
    message: ''
  });

  const [formState, setFormState] = useState<{
    isSubmitting: boolean;
    isSuccess: boolean;
    isError: boolean;
    errorMessage: string;
  }>({
    isSubmitting: false,
    isSuccess: false,
    isError: false,
    errorMessage: ''
  });

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
    
    // Clear previous status when user starts typing
    if (formState.isSuccess || formState.isError) {
      setFormState(prev => ({
        ...prev,
        isSuccess: false,
        isError: false,
        errorMessage: ''
      }));
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    setFormState(prev => ({
      ...prev,
      isSubmitting: true,
      isError: false,
      errorMessage: ''
    }));

    try {
      // EmailJS configuration
      const serviceId = process.env.NEXT_PUBLIC_EMAILJS_SERVICE_ID || 'YOUR_SERVICE_ID';
      const templateId = process.env.NEXT_PUBLIC_EMAILJS_TEMPLATE_ID || 'YOUR_TEMPLATE_ID';
      const publicKey = process.env.NEXT_PUBLIC_EMAILJS_PUBLIC_KEY || 'YOUR_PUBLIC_KEY';

      // Check if EmailJS is configured
      if (serviceId === 'YOUR_SERVICE_ID' || templateId === 'YOUR_TEMPLATE_ID' || publicKey === 'YOUR_PUBLIC_KEY') {
        // Demo mode - log the email details
        console.log('EmailJS not configured. Email details that would be sent:');
        console.log({
          to: data?.email || basicInfo?.email || 'Portfolio Owner Email',
          from_name: formData.name,
          from_email: formData.email,
          subject: formData.subject,
          message: formData.message,
          portfolio_owner: basicInfo?.name || 'Portfolio Owner'
        });

        setFormState({
          isSubmitting: false,
          isSuccess: true,
          isError: false,
          errorMessage: ''
        });

        // Reset form
        setFormData({ name: '', email: '', subject: '', message: '' });

        // Auto-hide success message after 5 seconds
        setTimeout(() => {
          setFormState(prev => ({ ...prev, isSuccess: false }));
        }, 5000);

        return;
      }

      // Prepare template parameters for EmailJS
      const templateParams = {
        to_email: data?.email || basicInfo?.email || 'contact@example.com',
        from_name: formData.name,
        from_email: formData.email,
        subject: formData.subject,
        message: formData.message,
        portfolio_owner: basicInfo?.name || 'Portfolio Owner',
        reply_to: formData.email
      };

      // Send email using EmailJS
      await emailjs.send(serviceId, templateId, templateParams, publicKey);

      // Success
      setFormState({
        isSubmitting: false,
        isSuccess: true,
        isError: false,
        errorMessage: ''
      });

      // Reset form
      setFormData({ name: '', email: '', subject: '', message: '' });

      // Auto-hide success message after 5 seconds
      setTimeout(() => {
        setFormState(prev => ({ ...prev, isSuccess: false }));
      }, 5000);

    } catch (error) {
      console.error('EmailJS error:', error);
      
      let errorMessage = 'Failed to send message. Please try again.';
      
      if (error instanceof Error) {
        if (error.message.includes('network') || error.message.includes('Network')) {
          errorMessage = 'Network error. Please check your connection and try again.';
        } else if (error.message.includes('invalid') || error.message.includes('Invalid')) {
          errorMessage = 'Email service configuration error. Please contact the site administrator.';
        }
      }

      setFormState({
        isSubmitting: false,
        isSuccess: false,
        isError: true,
        errorMessage
      });
    }
  };

  const contactItems = [
    {
      icon: Mail,
      label: 'Email',
      value: data?.email || basicInfo?.email || 'contact@example.com',
      href: `mailto:${data?.email || basicInfo?.email || 'contact@example.com'}`
    },
    {
      icon: MapPin,
      label: 'Location',
      value: basicInfo?.location || 'Location',
      href: '#'
    }
  ];

  return (
    <div className="prof-contact">
      <div className="prof-contact-container">
        <div className="prof-contact-grid">
          {/* Contact Information */}
          <div className="prof-contact-info">
            <Card className="prof-contact-card">
              <div className="prof-contact-header">
                <MessageCircle size={24} />
                <h3>Get In Touch</h3>
                <p>Ready to start your next project? Let's discuss how we can work together.</p>
              </div>
              
              <div className="prof-contact-items">
                {contactItems.map((item, index) => (
                  <a 
                    key={index}
                    href={item.href}
                    className="prof-contact-item"
                  >
                    <div className="prof-contact-icon">
                      <item.icon size={20} />
                    </div>
                    <div className="prof-contact-details">
                      <div className="prof-contact-label">{item.label}</div>
                      <div className="prof-contact-value">{item.value}</div>
                    </div>
                  </a>
                ))}
              </div>

              <div className="prof-contact-availability">
                <h4>Availability</h4>
                <p>I'm currently available for new projects and collaborations. Response time is typically within 24 hours.</p>
              </div>
            </Card>
          </div>

          {/* Contact Form */}
          <div className="prof-contact-form">
            <Card className="prof-form-card">
              <div className="prof-form-header">
                <h3>Send a Message</h3>
                <p>Let's start a conversation about your project</p>
              </div>

              {/* Success Message */}
              {formState.isSuccess && (
                <div className="prof-form-success">
                  <CheckCircle size={20} />
                  <div>
                    <h4>Message sent successfully!</h4>
                    <p>Thank you for reaching out. I'll get back to you within 24 hours.</p>
                  </div>
                </div>
              )}

              {/* Error Message */}
              {formState.isError && (
                <div className="prof-form-error">
                  <AlertCircle size={20} />
                  <div>
                    <h4>Failed to send message</h4>
                    <p>{formState.errorMessage}</p>
                  </div>
                </div>
              )}
              
              <form onSubmit={handleSubmit} className="prof-form">
                <div className="prof-form-row">
                  <div className="prof-form-group">
                    <label htmlFor="name" className="prof-form-label">Full Name</label>
                    <input
                      type="text"
                      id="name"
                      name="name"
                      value={formData.name}
                      onChange={handleInputChange}
                      className="prof-form-input"
                      placeholder="Your full name"
                      required
                      disabled={formState.isSubmitting}
                    />
                  </div>
                  
                  <div className="prof-form-group">
                    <label htmlFor="email" className="prof-form-label">Your Email Address</label>
                    <input
                      type="email"
                      id="email"
                      name="email"
                      value={formData.email}
                      onChange={handleInputChange}
                      className="prof-form-input"
                      placeholder="your@email.com"
                      required
                      disabled={formState.isSubmitting}
                    />
                  </div>
                </div>
                
                <div className="prof-form-group">
                  <label htmlFor="subject" className="prof-form-label">Subject</label>
                  <input
                    type="text"
                    id="subject"
                    name="subject"
                    value={formData.subject}
                    onChange={handleInputChange}
                    className="prof-form-input"
                    placeholder="Project inquiry, collaboration, etc."
                    required
                    disabled={formState.isSubmitting}
                  />
                </div>
                
                <div className="prof-form-group">
                  <label htmlFor="message" className="prof-form-label">Message</label>
                  <textarea
                    id="message"
                    name="message"
                    value={formData.message}
                    onChange={handleInputChange}
                    className="prof-form-textarea"
                    placeholder="Tell me about your project..."
                    rows={5}
                    required
                    disabled={formState.isSubmitting}
                  />
                </div>
                
                <Button 
                  type="submit" 
                  className="prof-form-submit"
                  disabled={formState.isSubmitting}
                >
                  {formState.isSubmitting ? (
                    <>
                      <Loader2 size={16} className="animate-spin" />
                      Sending...
                    </>
                  ) : (
                    <>
                      <Send size={16} />
                      Send Message
                    </>
                  )}
                </Button>
              </form>
            </Card>
          </div>
        </div>
      </div>
    </div>
  );
} 