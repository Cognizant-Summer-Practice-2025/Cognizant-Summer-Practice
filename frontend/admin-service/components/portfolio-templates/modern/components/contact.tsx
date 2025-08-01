import React from 'react';
import { ContactInfo } from '@/lib/portfolio';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Mail, MapPin } from 'lucide-react';

interface ContactProps {
  data: ContactInfo;
}

export function Contact({ data: contact }: ContactProps) {
  if (!contact) {
    return (
      <div className="text-center text-muted-foreground">
        No contact information available.
      </div>
    );
  }

  return (
    <div className="modern-component-container">
      {/* Count indicator to match other components */}
      <div className="mb-4 pb-2 border-b border-border">
        <p className="text-sm text-muted-foreground">
          Contact Information
        </p>
      </div>

      <div className="max-h-[800px] overflow-y-auto pr-2">
        <div className="modern-grid single-item">
          <Card className="modern-card">
            <CardContent className="p-6">
              <h3 className="text-xl font-semibold mb-4 text-foreground">
                Get In Touch
              </h3>
              
              <div className="space-y-4">
                {contact.email && (
                  <div className="flex items-center gap-3">
                    <div className="p-2 rounded-lg bg-primary/10">
                      <Mail size={16} className="text-primary" />
                    </div>
                    <div>
                      <p className="text-sm text-muted-foreground">Email</p>
                      <a 
                        href={`mailto:${contact.email}`}
                        className="text-foreground hover:text-primary transition-colors"
                      >
                        {contact.email}
                      </a>
                    </div>
                  </div>
                )}
                
                {contact.location && (
                  <div className="flex items-center gap-3">
                    <div className="p-2 rounded-lg bg-primary/10">
                      <MapPin size={16} className="text-primary" />
                    </div>
                    <div>
                      <p className="text-sm text-muted-foreground">Location</p>
                      <p className="text-foreground">{contact.location}</p>
                    </div>
                  </div>
                )}
              </div>
              
              {contact.email && (
                <div className="mt-6 pt-4 border-t border-border">
                  <Button asChild className="w-full">
                    <a href={`mailto:${contact.email}`}>
                      <Mail size={16} className="mr-2" />
                      Send Message
                    </a>
                  </Button>
                </div>
              )}
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
} 