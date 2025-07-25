import React from 'react';
import { SocialLink } from '@/lib/portfolio';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { 
  Github, 
  Linkedin, 
  Twitter, 
  Instagram, 
  Facebook, 
  Youtube,
  ExternalLink,
  Globe,
  Mail
} from 'lucide-react';

interface FooterProps {
  socialLinks: SocialLink[];
}

export function Footer({ socialLinks }: FooterProps) {
  const getIcon = (platform: string) => {
    const platformLower = platform.toLowerCase();
    const iconProps = { size: 18 };
    
    switch (platformLower) {
      case 'github':
        return <Github {...iconProps} />;
      case 'linkedin':
        return <Linkedin {...iconProps} />;
      case 'twitter':
      case 'x':
        return <Twitter {...iconProps} />;
      case 'instagram':
        return <Instagram {...iconProps} />;
      case 'facebook':
        return <Facebook {...iconProps} />;
      case 'youtube':
        return <Youtube {...iconProps} />;
      case 'email':
        return <Mail {...iconProps} />;
      case 'website':
      case 'portfolio':
        return <Globe {...iconProps} />;
      default:
        return <ExternalLink {...iconProps} />;
    }
  };

  return (
    <footer className="mt-12 mb-8">
      <Card className="modern-section-card">
        <div className="p-8 text-center">
          {socialLinks && socialLinks.length > 0 && (
            <div className="mb-6">
              <h3 className="text-lg font-semibold text-foreground mb-4">
                Let's Connect
              </h3>
              <div className="flex justify-center gap-3 flex-wrap">
                {socialLinks.map((link) => (
                  <Button
                    key={link.id}
                    variant="outline"
                    size="sm"
                    asChild
                    className="transition-all hover:scale-105"
                  >
                    <a
                      href={link.url}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="flex items-center gap-2"
                      title={link.platform}
                    >
                      {getIcon(link.platform)}
                      <span className="capitalize">{link.platform}</span>
                    </a>
                  </Button>
                ))}
              </div>
            </div>
          )}
          
          <div className="border-t border-border pt-6">
            <p className="text-sm text-muted-foreground">
              © {new Date().getFullYear()} Built with Modern Portfolio Template
            </p>
            <p className="text-xs text-muted-foreground mt-2">
              Crafted with care and attention to detail
            </p>
          </div>
        </div>
      </Card>
    </footer>
  );
} 