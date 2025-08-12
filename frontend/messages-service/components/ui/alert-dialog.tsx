"use client"

import * as React from "react"
import * as AlertDialogPrimitive from "@radix-ui/react-alert-dialog"
import { useState, createContext, useContext } from 'react';

import { cn } from "@/lib/utils"
import { buttonVariants } from "@/components/ui/button"

const AlertDialog = AlertDialogPrimitive.Root

const AlertDialogTrigger = AlertDialogPrimitive.Trigger

const AlertDialogPortal = AlertDialogPrimitive.Portal

const AlertDialogOverlay = React.forwardRef<
  React.ElementRef<typeof AlertDialogPrimitive.Overlay>,
  React.ComponentPropsWithoutRef<typeof AlertDialogPrimitive.Overlay>
>(({ className, ...props }, ref) => (
  <AlertDialogPrimitive.Overlay
    className={cn(
      "fixed inset-0 z-50 bg-black/80  data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0",
      className
    )}
    {...props}
    ref={ref}
  />
))
AlertDialogOverlay.displayName = AlertDialogPrimitive.Overlay.displayName

const AlertDialogContent = React.forwardRef<
  React.ElementRef<typeof AlertDialogPrimitive.Content>,
  React.ComponentPropsWithoutRef<typeof AlertDialogPrimitive.Content>
>(({ className, ...props }, ref) => (
  <AlertDialogPortal>
    <AlertDialogOverlay />
    <AlertDialogPrimitive.Content
      ref={ref}
      className={cn(
        "fixed left-[50%] top-[50%] z-50 grid w-full max-w-lg translate-x-[-50%] translate-y-[-50%] gap-4 border bg-background p-6 shadow-lg duration-200 data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0 data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95 data-[state=closed]:slide-out-to-left-1/2 data-[state=closed]:slide-out-to-top-[48%] data-[state=open]:slide-in-from-left-1/2 data-[state=open]:slide-in-from-top-[48%] sm:rounded-lg",
        className
      )}
      {...props}
    />
  </AlertDialogPortal>
))
AlertDialogContent.displayName = AlertDialogPrimitive.Content.displayName

const AlertDialogHeader = ({
  className,
  ...props
}: React.HTMLAttributes<HTMLDivElement>) => (
  <div
    className={cn(
      "flex flex-col space-y-2 text-center sm:text-left",
      className
    )}
    {...props}
  />
)
AlertDialogHeader.displayName = "AlertDialogHeader"

const AlertDialogFooter = ({
  className,
  ...props
}: React.HTMLAttributes<HTMLDivElement>) => (
  <div
    className={cn(
      "flex flex-col-reverse sm:flex-row sm:justify-end sm:space-x-2",
      className
    )}
    {...props}
  />
)
AlertDialogFooter.displayName = "AlertDialogFooter"

const AlertDialogTitle = React.forwardRef<
  React.ElementRef<typeof AlertDialogPrimitive.Title>,
  React.ComponentPropsWithoutRef<typeof AlertDialogPrimitive.Title>
>(({ className, ...props }, ref) => (
  <AlertDialogPrimitive.Title
    ref={ref}
    className={cn("text-lg font-semibold", className)}
    {...props}
  />
))
AlertDialogTitle.displayName = AlertDialogPrimitive.Title.displayName

const AlertDialogDescription = React.forwardRef<
  React.ElementRef<typeof AlertDialogPrimitive.Description>,
  React.ComponentPropsWithoutRef<typeof AlertDialogPrimitive.Description>
>(({ className, ...props }, ref) => (
  <AlertDialogPrimitive.Description
    ref={ref}
    className={cn("text-sm text-muted-foreground", className)}
    {...props}
  />
))
AlertDialogDescription.displayName =
  AlertDialogPrimitive.Description.displayName

const AlertDialogAction = React.forwardRef<
  React.ElementRef<typeof AlertDialogPrimitive.Action>,
  React.ComponentPropsWithoutRef<typeof AlertDialogPrimitive.Action>
>(({ className, ...props }, ref) => (
  <AlertDialogPrimitive.Action
    ref={ref}
    className={cn(buttonVariants(), className)}
    {...props}
  />
))
AlertDialogAction.displayName = AlertDialogPrimitive.Action.displayName

const AlertDialogCancel = React.forwardRef<
  React.ElementRef<typeof AlertDialogPrimitive.Cancel>,
  React.ComponentPropsWithoutRef<typeof AlertDialogPrimitive.Cancel>
>(({ className, ...props }, ref) => (
  <AlertDialogPrimitive.Cancel
    ref={ref}
    className={cn(
      buttonVariants({ variant: "outline" }),
      "mt-2 sm:mt-0",
      className
    )}
    {...props}
  />
))
AlertDialogCancel.displayName = AlertDialogPrimitive.Cancel.displayName

// Custom Alert Provider and Hook for compatibility with existing code
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

const CustomAlertDialog: React.FC<{
    alert: AlertState;
    onClose: () => void;
    onConfirm: () => void;
    onCancel: () => void;
}> = ({ alert, onClose, onConfirm, onCancel }) => {
    return (
        <AlertDialog open={alert.isOpen} onOpenChange={() => onClose()}>
            <AlertDialogContent className="sm:max-w-md">
                <AlertDialogHeader>
                    <AlertDialogTitle className="text-lg font-semibold">
                        {alert.title}
                    </AlertDialogTitle>
                    <AlertDialogDescription className="text-left text-gray-600 whitespace-pre-line">
                        {alert.description}
                    </AlertDialogDescription>
                </AlertDialogHeader>

                <AlertDialogFooter className="flex-col-reverse sm:flex-row sm:justify-end gap-2">
                    {alert.showCancel && (
                        <AlertDialogCancel onClick={onCancel} className="w-full sm:w-auto">
                            {alert.cancelText || 'Cancel'}
                        </AlertDialogCancel>
                    )}
                    <AlertDialogAction
                        onClick={onConfirm}
                        className="w-full sm:w-auto"
                    >
                        {alert.confirmText || 'OK'}
                    </AlertDialogAction>
                </AlertDialogFooter>
            </AlertDialogContent>
        </AlertDialog>
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
                <CustomAlertDialog
                    alert={currentAlert}
                    onClose={handleClose}
                    onConfirm={handleConfirm}
                    onCancel={handleCancel}
                />
            )}
        </AlertContext.Provider>
    );
};

export {
  AlertDialog,
  AlertDialogPortal,
  AlertDialogOverlay,
  AlertDialogTrigger,
  AlertDialogContent,
  AlertDialogHeader,
  AlertDialogFooter,
  AlertDialogTitle,
  AlertDialogDescription,
  AlertDialogAction,
  AlertDialogCancel,
}