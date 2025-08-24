import React, { useState, useEffect } from "react";
import Image from "next/image";
import { User, MoreHorizontal, AlertTriangle, ArrowLeft, UserCircle } from "lucide-react";
import { getPortfoliosByUserId } from "@/lib/portfolio/api";
import { redirectToService } from "@/lib/config/services";
import { reportUser } from "@/lib/user/api";
import { useUser } from "@/lib/contexts/user-context";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import ReportModal from "../message-menu/report-modal";
import "./style.css";

interface Contact {
  id: string;
  name: string;
  avatar: string;
  lastMessage: string;
  timestamp: string;
  isActive?: boolean;
  isOnline?: boolean;
  userId?: string;
  professionalTitle?: string;
}

interface ChatHeaderProps {
  selectedContact: Contact;
  onDeleteConversation?: (conversationId: string) => Promise<void>;
  onBackToSidebar?: () => void;
  isMobile?: boolean;
}

const ChatHeader: React.FC<ChatHeaderProps> = ({ selectedContact, onBackToSidebar, isMobile = false }) => {
  const [portfolioId, setPortfolioId] = useState<string | null>(null);
  const [isReportModalOpen, setIsReportModalOpen] = useState(false);
  const [isReporting, setIsReporting] = useState(false);
  const { user } = useUser();

  const handleReportUser = () => {
    setIsReportModalOpen(true);
  };

  const handleReportSubmit = async (reason: string) => {
    if (!user?.id || !selectedContact.userId) {
      return;
    }

    setIsReporting(true);
    try {
      await reportUser(selectedContact.userId, user.id, reason);
    } catch (error) {
      throw error;
    } finally {
      setIsReporting(false);
    }
  };

  // Check if user has a portfolio when selectedContact changes
  useEffect(() => {
    const checkUserPortfolio = async () => {
      if (selectedContact.userId) {
        try {
          const portfolios = await getPortfoliosByUserId(selectedContact.userId);
          // Find the first published portfolio and get its ID
          const publishedPortfolio = portfolios.find(portfolio => portfolio.isPublished);
          setPortfolioId(publishedPortfolio ? publishedPortfolio.id : null);
        } catch {
          setPortfolioId(null);
        }
      } else {
        setPortfolioId(null);
      }
    };

    checkUserPortfolio();
  }, [selectedContact.userId]);

  const handleViewProfile = () => {
    if (portfolioId) {
      redirectToService('HOME_PORTFOLIO_SERVICE', `portfolio?portfolio=${portfolioId}`);
    }
  };

  return (
    <>
    <div className="chat-header">
      <div className="chat-header-left">
        {/* Back button for mobile */}
        {isMobile && (
          <Button
            variant="ghost"
            size="icon"
            className="back-button"
            onClick={onBackToSidebar}
          >
            <ArrowLeft className="w-5 h-5" />
          </Button>
        )}
        <Image 
          width={40}
          height={40}
          src={selectedContact.avatar} 
          alt={selectedContact.name}
          className="contact-avatar"
        />
        <div className="contact-info">
          <h3 className="contact-name">{selectedContact.name}</h3>
          <div className="contact-status">
            <span className="contact-role">{selectedContact.professionalTitle || 'Professional'}</span>
            <span className="status-separator">•</span>
            <span className={`online-status ${selectedContact.isOnline ? 'online' : 'offline'}`}>
              <span className="status-dot"></span>
              {selectedContact.isOnline ? 'Online' : 'Offline'}
            </span>
          </div>
        </div>
      </div>
      
      <div className="chat-header-right">
        {portfolioId && (
          <Button 
            variant="outline"
            className="view-portfolio-btn"
            onClick={handleViewProfile}
          >
            <User className="w-4 h-4 mr-2 hidden sm:block" />
            <UserCircle className="w-4 h-4 sm:hidden" />
            <span className="hidden sm:inline">View Portfolio</span>
          </Button>
        )}
        
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button 
              variant="outline"
              size="icon"
              className="more-options-btn"
            >
              <MoreHorizontal className="w-4 h-4" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end" className="w-48">
            <DropdownMenuItem 
              onClick={handleReportUser}
              disabled={isReporting}
              className="text-red-600 focus:text-red-600 focus:bg-red-50"
            >
              <AlertTriangle className="mr-2 h-4 w-4" />
              {isReporting ? "Reporting..." : "Report user"}
            </DropdownMenuItem>
            <DropdownMenuSeparator />
          </DropdownMenuContent>
        </DropdownMenu>
      </div>
    </div>

    <ReportModal
      isOpen={isReportModalOpen}
      onClose={() => setIsReportModalOpen(false)}
      onSubmit={handleReportSubmit}
      reportType="user"
      targetName={selectedContact.name}
    />
    </>
  );
};

export default ChatHeader;