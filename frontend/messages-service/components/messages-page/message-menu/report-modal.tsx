import React, { useState } from "react";
import { createPortal } from "react-dom";
import { X } from "lucide-react";
import "./report-modal.css";

interface ReportModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (reason: string) => Promise<void>;
  messageId: string;
}

const ReportModal: React.FC<ReportModalProps> = ({
  isOpen,
  onClose,
  onSubmit,
  messageId,
}) => {
  const [reason, setReason] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState("");

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (reason.trim().length < 50) {
      setError("Reason must be at least 50 characters long");
      return;
    }

    setIsSubmitting(true);
    setError("");

    try {
      await onSubmit(reason.trim());
      setReason("");
      onClose();
    } catch (error) {
      setError("Failed to submit report. Please try again.");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleClose = () => {
    if (!isSubmitting) {
      setReason("");
      setError("");
      onClose();
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "Escape" && !isSubmitting) {
      handleClose();
    }
  };

  if (!isOpen) return null;

  const modalContent = (
    <div className="report-modal-overlay" onClick={handleClose}>
      <div 
        className="report-modal-content" 
        onClick={(e) => e.stopPropagation()}
        onKeyDown={handleKeyDown}
      >
        <div className="report-modal-header">
          <h2 className="report-modal-title">Report Message</h2>
          <button 
            className="report-modal-close"
            onClick={handleClose}
            disabled={isSubmitting}
          >
            <X className="h-5 w-5" />
          </button>
        </div>

        <div className="report-modal-body">
          <p className="report-modal-description">
            Please provide a reason for reporting this message. This will help our moderators review the content.
          </p>

          <form onSubmit={handleSubmit}>
            <div className="report-modal-field">
              <label htmlFor="reason" className="report-modal-label">
                Reason for report *
              </label>
              <textarea
                id="reason"
                value={reason}
                onChange={(e) => setReason(e.target.value)}
                placeholder="Please describe why you're reporting this message..."
                className="report-modal-textarea"
                rows={4}
                maxLength={500}
                disabled={isSubmitting}
                autoFocus
              />
              <div className={`report-modal-char-count ${reason.length >= 50 ? 'minimum-reached' : 'needs-more'}`}>
                {reason.length}/{50}
              </div>
            </div>

            {error && (
              <div className="report-modal-error">
                {error}
              </div>
            )}

            <div className="report-modal-actions">
              <button
                type="button"
                onClick={handleClose}
                className="report-modal-cancel"
                disabled={isSubmitting}
              >
                Cancel
              </button>
              <button
                type="submit"
                className="report-modal-submit"
                disabled={isSubmitting || reason.trim().length < 50}
              >
                {isSubmitting ? "Submitting..." : "Submit Report"}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );

  // Render modal at document body level to ensure proper positioning
  return createPortal(modalContent, document.body);
};

export default ReportModal; 