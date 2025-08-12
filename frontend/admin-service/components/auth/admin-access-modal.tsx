'use client';

import React from 'react';
import { AlertCircle, LogOut } from 'lucide-react';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';

interface AdminAccessModalProps {
  isOpen: boolean;
  onSignOut: () => void;
}

export function AdminAccessModal({ isOpen, onSignOut }: AdminAccessModalProps) {
  return (
    <Dialog open={isOpen} onOpenChange={() => {}}>
      <DialogContent className="sm:max-w-[425px] [&>button]:hidden">
        <DialogHeader className="text-center">
          <div className="flex justify-center mb-4">
            <div className="w-16 h-16 bg-red-100 rounded-full flex items-center justify-center">
              <AlertCircle className="w-8 h-8 text-red-600" />
            </div>
          </div>
          <DialogTitle className="text-xl font-semibold text-gray-900">
            Admin Access Required
          </DialogTitle>
          <DialogDescription className="text-gray-600 mt-2">
            You don't have administrator privileges to access this service. 
            Please contact your system administrator for access.
          </DialogDescription>
        </DialogHeader>
        
        <div className="flex justify-center mt-6">
          <Button
            onClick={onSignOut}
            className="flex items-center gap-2 bg-red-600 hover:bg-red-700 text-white px-6 py-2"
          >
            <LogOut className="w-4 h-4" />
            Sign Out
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}
