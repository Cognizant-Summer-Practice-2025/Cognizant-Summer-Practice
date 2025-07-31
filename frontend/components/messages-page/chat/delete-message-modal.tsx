import React from 'react';
import { createPortal } from 'react-dom';
import { Trash2, X } from 'lucide-react';
import './delete-message-modal.css';

interface DeleteMessageModalProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
  isDeleting: boolean;
}

const DeleteMessageModal: React.FC<DeleteMessageModalProps> = ({
  isOpen,
  onClose,
  onConfirm,
  isDeleting
}) => {
  if (!isOpen) return null;

  const modalContent = (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <div className="modal-icon">
            <Trash2 size={24} className="text-red-500" />
          </div>
          <button 
            className="modal-close-button"
            onClick={onClose}
            disabled={isDeleting}
          >
            <X size={20} />
          </button>
        </div>
        
        <div className="modal-body">
          <h3 className="modal-title">Delete Message</h3>
          <p className="modal-description">
            Are you sure you want to delete this message?
          </p>
        </div>
        
        <div className="modal-footer">
          <button 
            className="modal-button modal-button-cancel"
            onClick={onClose}
            disabled={isDeleting}
          >
            Cancel
          </button>
          <button 
            className="modal-button modal-button-delete"
            onClick={onConfirm}
            disabled={isDeleting}
          >
            {isDeleting ? (
              <>
                <div className="spinner"></div>
                Deleting...
              </>
            ) : (
              <>
                <Trash2 size={16} />
                Delete
              </>
            )}
          </button>
        </div>
      </div>
    </div>
  );

  return typeof window !== 'undefined' 
    ? createPortal(modalContent, document.body)
    : null;
};

export default DeleteMessageModal; 