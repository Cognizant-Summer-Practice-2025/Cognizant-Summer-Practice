import React, { useState, useEffect } from "react";
import { User, MoreHorizontal, Volume2, Shield, AlertTriangle, Trash2 } from "lucide-react";
import { useRouter } from "next/navigation";
import { getPortfoliosByUserId } from "@/lib/portfolio/api";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
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
}

const ChatHeader: React.FC<ChatHeaderProps> = ({ selectedContact, onDeleteConversation }) => {
  const [portfolioId, setPortfolioId] = useState<string | null>(null);
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);
  const router = useRouter();

  const handleMuteNotifications = () => {
    console.log('Mute user:', selectedContact.name);
    // Add mute user logic here
  };

  const handleBlockUser = () => {
    console.log('Block user:', selectedContact.name);
    // Add block user logic here
  };

  const handleReportUser = () => {
    console.log('Report user:', selectedContact.name);
    // Add report user logic here
  };

  const handleDeleteClick = () => {
    console.log('Delete conversation clicked for:', selectedContact.name);
    setShowDeleteDialog(true);
  };

  const handleConfirmDelete = async () => {
    if (!onDeleteConversation) return;
    
    console.log('Confirming delete for conversation:', selectedContact.id);
    setIsDeleting(true);
    try {
      console.log('Calling onDeleteConversation...');
      await onDeleteConversation(selectedContact.id);
      console.log('Delete successful, closing dialog');
      setShowDeleteDialog(false);
    } catch (error) {
      console.error('Delete conversation error:', error);
    } finally {
      setIsDeleting(false);
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
        } catch (error) {
          console.error('Error checking user portfolio:', error);
          setPortfolioId(null);
        }
      } else {
        setPortfolioId(null);
      }
    };

    checkUserPortfolio();
  }, [selectedContact.userId]);

  const handleViewProfile = () => {
    console.log('View Portfolio clicked for:', selectedContact.name);
    console.log('Selected contact data:', selectedContact);
    
    if (portfolioId) {
      console.log('Navigating to portfolio with portfolioId:', portfolioId);
      // Navigate to the user's portfolio page using portfolio ID
      router.push(`/portfolio?portfolio=${portfolioId}`);
    } else {
      console.log('❌ No portfolio ID available for:', selectedContact.name);
    }
  };

  return (
    <div className="chat-header">
      <div className="chat-header-left">
        <img 
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
            <User className="w-4 h-4 mr-2" />
            View Portfolio
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
            <DropdownMenuItem onClick={handleMuteNotifications}>
              <Volume2 className="mr-2 h-4 w-4" />
              Mute notifications
            </DropdownMenuItem>
            <DropdownMenuItem 
              onClick={handleBlockUser}
              className="text-red-600 focus:text-red-600 focus:bg-red-50"
            >
              <Shield className="mr-2 h-4 w-4" />
              Block user
            </DropdownMenuItem>
            <DropdownMenuItem 
              onClick={handleReportUser}
              className="text-red-600 focus:text-red-600 focus:bg-red-50"
            >
              <AlertTriangle className="mr-2 h-4 w-4" />
              Report user
            </DropdownMenuItem>
            <DropdownMenuSeparator />
            <DropdownMenuItem 
              onClick={handleDeleteClick}
              className="text-red-600 focus:text-red-600 focus:bg-red-50"
            >
              <Trash2 className="mr-2 h-4 w-4" />
              Delete for me
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </div>
      
      <AlertDialog open={showDeleteDialog} onOpenChange={setShowDeleteDialog}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete Conversation</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to delete the conversation with {selectedContact.name}? 
              This will remove the conversation from your chat list, but {selectedContact.name} will still see it. 
              You can restore it by sending a new message.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel disabled={isDeleting}>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleConfirmDelete}
              disabled={isDeleting}
              className="bg-red-600 hover:bg-red-700 focus:ring-red-600"
            >
              {isDeleting ? "Deleting..." : "Delete"}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
};

export default ChatHeader;