import React, { useState } from "react";
import { MoreHorizontal, Trash2, Copy, Flag } from "lucide-react";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { useAlert } from "@/components/ui/alert-dialog";
import ReportModal from "./report-modal";
import "./style.css";
import "./dropdown-style.css";

interface MessageMenuProps {
  messageId: string;
  messageText: string;
  isOwnMessage: boolean;
  onDelete?: (messageId: string) => Promise<void>;
  onCopy?: (text: string) => void;
  onReport?: (messageId: string, reason: string) => Promise<void>;
}

const MessageMenu: React.FC<MessageMenuProps> = ({
  messageId,
  messageText,
  isOwnMessage,
  onDelete,
  onCopy,
  onReport,
}) => {
  const { showConfirm } = useAlert();
  const [isDeleting, setIsDeleting] = useState(false);
  const [isReporting, setIsReporting] = useState(false);
  const [isReportModalOpen, setIsReportModalOpen] = useState(false);

  const handleDeleteClick = (e: React.MouseEvent) => {
    e.stopPropagation();
    showConfirm({
      title: 'Delete Message',
      description: 'Are you sure you want to delete this message? This action cannot be undone.',
      type: 'warning',
      confirmText: 'Delete',
      cancelText: 'Cancel',
      onConfirm: handleConfirmDelete,
    });
  };

  const handleReportClick = (e: React.MouseEvent) => {
    e.stopPropagation();
    setIsReportModalOpen(true);
  };

  const handleCopyClick = (e: React.MouseEvent) => {
    e.stopPropagation();
    if (onCopy) {
      onCopy(messageText);
    } else {
      navigator.clipboard.writeText(messageText).then(() => {
      }).catch(err => {
        console.error('Failed to copy text: ', err);
      });
    }
  };

  const handleConfirmDelete = async () => {
    if (!onDelete) return;
    
    setIsDeleting(true);
    try {
      await onDelete(messageId);
    } catch (error) {
      console.error("Delete error:", error);
    } finally {
      setIsDeleting(false);
    }
  };

  const handleReportSubmit = async (reason: string) => {
    if (!onReport) return;
    
    setIsReporting(true);
    try {
      await onReport(messageId, reason);
    } catch (error) {
      console.error("Report error:", error);
      throw error; 
    } finally {
      setIsReporting(false);
    }
  };

  return (
    <>
      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <button 
            className={`message-menu-button ${isOwnMessage ? 'own-message-menu' : 'other-message-menu'}`}
            onClick={(e) => e.stopPropagation()}
          >
            <MoreHorizontal className="h-4 w-4" />
          </button>
        </DropdownMenuTrigger>
        <DropdownMenuContent 
          align={isOwnMessage ? "start" : "end"} 
          className="message-dropdown-menu w-40"
        >
          <DropdownMenuItem
            onClick={handleCopyClick}
            className="message-dropdown-item"
          >
            <Copy className="mr-2 h-4 w-4" />
            Copy
          </DropdownMenuItem>
          
          {isOwnMessage ? (
            <DropdownMenuItem
              onClick={handleDeleteClick}
              className="message-dropdown-item text-red-600 focus:text-red-600 focus:bg-red-50"
            >
              <Trash2 className="mr-2 h-4 w-4" />
              Delete
            </DropdownMenuItem>
          ) : (
            <DropdownMenuItem
              onClick={handleReportClick}
              className="message-dropdown-item text-orange-600 focus:text-orange-600 focus:bg-orange-50"
            >
              <Flag className="mr-2 h-4 w-4" />
              Report
            </DropdownMenuItem>
          )}
        </DropdownMenuContent>
      </DropdownMenu>

      <ReportModal
        isOpen={isReportModalOpen}
        onClose={() => setIsReportModalOpen(false)}
        onSubmit={handleReportSubmit}
        messageId={messageId}
      />
    </>
  );
};

export default MessageMenu; 