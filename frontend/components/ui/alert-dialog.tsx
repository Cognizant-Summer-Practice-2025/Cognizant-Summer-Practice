import React, { useState, useEffect, createContext, useContext } from 'react';
import { AlertTriangle, CheckCircle, XCircle, Info, X } from 'lucide-react';
import { Button } from './button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from './dialog';

interface AlertOptions {
  title: string;
  description: string;
  type?: 'info' | 'success' | 'warning' | 'error';
  confirmText?: string;
  cancelText?: string;
  onConfirm?: () => void;
  onCancel?: () => void;
  showCancel?: boolean;
}

interface AlertState extends AlertOptions {
  id: string;
  isOpen: boolean;
}

interface AlertContextType {
  showAlert: (options: AlertOptions) => void;
  showConfirm: (options: AlertOptions) => void;
  hideAlert: () => void;
}

const AlertContext = createContext<AlertContextType | null>(null);

export const useAlert = () => {
  const context = useContext(AlertContext);
  if (!context) {
    // Fallback to browser dialogs if not within provider
    return {
      showAlert: ({ title, description }: AlertOptions) => {
        alert(`${title}\n\n${description}`);
      },
      showConfirm: ({ title, description, onConfirm, onCancel }: AlertOptions) => {
        const confirmed = confirm(`${title}\n\n${description}`);
        if (confirmed && onConfirm) {
          onConfirm();
        } else if (!confirmed && onCancel) {
          onCancel();
        }
      },
      hideAlert: () => {},
    };
  }
  return context;
};

const AlertDialog: React.FC<{
  alert: AlertState;
  onClose: () => void;
  onConfirm: () => void;
  onCancel: () => void;
}> = ({ alert, onClose, onConfirm, onCancel }) => {
  const getIcon = () => {
    switch (alert.type) {
      case 'success':
        return <CheckCircle className="w-6 h-6 text-green-500" />;
      case 'error':
        return <XCircle className="w-6 h-6 text-red-500" />;
      case 'warning':
        return <AlertTriangle className="w-6 h-6 text-yellow-500" />;
      default:
        return <Info className="w-6 h-6 text-blue-500" />;
    }
  };

  const getHeaderStyles = () => {
    switch (alert.type) {
      case 'success':
        return 'text-green-900';
      case 'error':
        return 'text-red-900';
      case 'warning':
        return 'text-yellow-900';
      default:
        return 'text-blue-900';
    }
  };

  const getConfirmButtonVariant = () => {
    switch (alert.type) {
      case 'error':
      case 'warning':
        return 'destructive' as const;
      default:
        return 'default' as const;
    }
  };

  return (
    <Dialog open={alert.isOpen} onOpenChange={() => onClose()}>
      <DialogContent className="sm:max-w-md" showCloseButton={false}>
        <DialogHeader>
          <div className="flex items-center gap-3 mb-2">
            {getIcon()}
            <DialogTitle className={`text-lg font-semibold ${getHeaderStyles()}`}>
              {alert.title}
            </DialogTitle>
          </div>
          <DialogDescription className="text-left text-gray-600 whitespace-pre-line">
            {alert.description}
          </DialogDescription>
        </DialogHeader>
        
        <DialogFooter className="flex-col-reverse sm:flex-row sm:justify-end gap-2">
          {alert.showCancel && (
            <Button
              variant="outline"
              onClick={onCancel}
              className="w-full sm:w-auto"
            >
              {alert.cancelText || 'Cancel'}
            </Button>
          )}
          <Button
            variant={getConfirmButtonVariant()}
            onClick={onConfirm}
            className="w-full sm:w-auto"
          >
            {alert.confirmText || 'OK'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
};

export const AlertProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [alerts, setAlerts] = useState<AlertState[]>([]);
  const currentAlert = alerts[0]; // Show one at a time

  const showAlert = (options: AlertOptions) => {
    const id = Math.random().toString(36).substr(2, 9);
    const newAlert: AlertState = {
      ...options,
      id,
      isOpen: true,
      showCancel: false,
      type: options.type || 'info',
    };
    setAlerts(prev => [...prev, newAlert]);
  };

  const showConfirm = (options: AlertOptions) => {
    const id = Math.random().toString(36).substr(2, 9);
    const newAlert: AlertState = {
      ...options,
      id,
      isOpen: true,
      showCancel: true,
      type: options.type || 'warning',
      confirmText: options.confirmText || 'Confirm',
      cancelText: options.cancelText || 'Cancel',
    };
    setAlerts(prev => [...prev, newAlert]);
  };

  const hideAlert = () => {
    setAlerts(prev => prev.slice(1));
  };

  const handleConfirm = () => {
    if (currentAlert?.onConfirm) {
      currentAlert.onConfirm();
    }
    hideAlert();
  };

  const handleCancel = () => {
    if (currentAlert?.onCancel) {
      currentAlert.onCancel();
    }
    hideAlert();
  };

  const handleClose = () => {
    if (currentAlert?.showCancel && currentAlert?.onCancel) {
      currentAlert.onCancel();
    }
    hideAlert();
  };

  return (
    <AlertContext.Provider value={{ showAlert, showConfirm, hideAlert }}>
      {children}
      {currentAlert && (
        <AlertDialog
          alert={currentAlert}
          onClose={handleClose}
          onConfirm={handleConfirm}
          onCancel={handleCancel}
        />
      )}
    </AlertContext.Provider>
  );
}; 