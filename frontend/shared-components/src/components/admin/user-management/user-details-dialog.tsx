'use client';

import React from 'react';
import { User, Mail, Calendar, Briefcase, MapPin, Shield, Clock } from 'lucide-react';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Badge } from '@/components/ui/badge';
import { UserWithPortfolio } from '@/lib/admin';

interface UserDetailsDialogProps {
  user: UserWithPortfolio | null;
  isOpen: boolean;
  onClose: () => void;
}

const UserDetailsDialog: React.FC<UserDetailsDialogProps> = ({ user, isOpen, onClose }) => {
  if (!user) return null;

  const getUserDisplayName = (user: UserWithPortfolio): string => {
    if (user.firstName && user.lastName) {
      return `${user.firstName} ${user.lastName}`;
    }
    return user.username;
  };

  const getUserAvatar = (user: UserWithPortfolio): string => {
    return user.avatarUrl || `https://ui-avatars.com/api/?name=${encodeURIComponent(getUserDisplayName(user))}&size=80&background=f0f0f0&color=666`;
  };

  const formatDate = (dateString: string): string => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
      year: 'numeric', 
      month: 'long', 
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const getStatusVariant = (isActive: boolean) => {
    return isActive ? 'default' : 'destructive';
  };

  const getPortfolioStatusVariant = (status: string) => {
    switch (status.toLowerCase()) {
      case 'published':
        return 'default' as const;
      case 'draft':
        return 'secondary' as const;
      default:
        return 'outline' as const;
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="sm:max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-3">
            <User className="w-5 h-5 text-blue-600" />
            User Details
          </DialogTitle>
          <DialogDescription>
            Complete information about the selected user
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-6">
          {/* User Avatar and Basic Info */}
          <div className="flex items-start gap-4 p-4 border rounded-lg bg-gray-50">
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img 
              src={getUserAvatar(user)} 
              alt={getUserDisplayName(user)} 
              className="w-20 h-20 rounded-full border-2 border-white shadow-sm"
            />
            <div className="flex-1 space-y-2">
              <div>
                <h3 className="text-xl font-semibold text-gray-900">
                  {getUserDisplayName(user)}
                </h3>
                <p className="text-sm text-gray-600">@{user.username}</p>
              </div>
              <div className="flex flex-wrap gap-2">
                <Badge variant={getStatusVariant(user.isActive)}>
                  {user.isActive ? 'Active' : 'Suspended'}
                </Badge>
                {user.isAdmin && (
                  <Badge variant="secondary">
                    <Shield className="w-3 h-3 mr-1" />
                    Admin
                  </Badge>
                )}
                <Badge variant={getPortfolioStatusVariant(user.portfolioStatus)}>
                  Portfolio: {user.portfolioStatus}
                </Badge>
              </div>
            </div>
          </div>

          {/* Contact Information */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="space-y-4">
              <h4 className="font-semibold text-gray-900 border-b pb-2">Contact Information</h4>
              
              <div className="flex items-center gap-3">
                <Mail className="w-4 h-4 text-gray-500" />
                <div>
                  <p className="text-sm font-medium text-gray-700">Email</p>
                  <p className="text-sm text-gray-600">{user.email}</p>
                </div>
              </div>

              {user.location && (
                <div className="flex items-center gap-3">
                  <MapPin className="w-4 h-4 text-gray-500" />
                  <div>
                    <p className="text-sm font-medium text-gray-700">Location</p>
                    <p className="text-sm text-gray-600">{user.location}</p>
                  </div>
                </div>
              )}

              {user.professionalTitle && (
                <div className="flex items-center gap-3">
                  <Briefcase className="w-4 h-4 text-gray-500" />
                  <div>
                    <p className="text-sm font-medium text-gray-700">Professional Title</p>
                    <p className="text-sm text-gray-600">{user.professionalTitle}</p>
                  </div>
                </div>
              )}
            </div>

            <div className="space-y-4">
              <h4 className="font-semibold text-gray-900 border-b pb-2">Account Information</h4>
              
              <div className="flex items-center gap-3">
                <Calendar className="w-4 h-4 text-gray-500" />
                <div>
                  <p className="text-sm font-medium text-gray-700">Joined Date</p>
                  <p className="text-sm text-gray-600">{formatDate(user.joinedDate)}</p>
                </div>
              </div>

              {user.lastLoginAt && (
                <div className="flex items-center gap-3">
                  <Clock className="w-4 h-4 text-gray-500" />
                  <div>
                    <p className="text-sm font-medium text-gray-700">Last Login</p>
                    <p className="text-sm text-gray-600">{formatDate(user.lastLoginAt)}</p>
                  </div>
                </div>
              )}

              <div className="flex items-center gap-3">
                <User className="w-4 h-4 text-gray-500" />
                <div>
                  <p className="text-sm font-medium text-gray-700">User ID</p>
                  <p className="text-sm text-gray-600 font-mono">{user.id}</p>
                </div>
              </div>
            </div>
          </div>

          {/* Bio Section */}
          {user.bio && (
            <div className="space-y-2">
              <h4 className="font-semibold text-gray-900 border-b pb-2">Biography</h4>
              <p className="text-sm text-gray-600 leading-relaxed bg-gray-50 p-3 rounded-lg">
                {user.bio}
              </p>
            </div>
          )}

          {/* Portfolio Information */}
          {user.portfolioId && (
            <div className="space-y-2">
              <h4 className="font-semibold text-gray-900 border-b pb-2">Portfolio Information</h4>
              <div className="bg-blue-50 p-3 rounded-lg space-y-2">
                <div className="flex items-center justify-between">
                  <span className="text-sm font-medium text-gray-700">Portfolio Title:</span>
                  <span className="text-sm text-gray-600">{user.portfolioTitle || 'Untitled Portfolio'}</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm font-medium text-gray-700">Portfolio ID:</span>
                  <span className="text-sm text-gray-600 font-mono">{user.portfolioId}</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm font-medium text-gray-700">Status:</span>
                  <Badge variant={getPortfolioStatusVariant(user.portfolioStatus)}>
                    {user.portfolioStatus}
                  </Badge>
                </div>
              </div>
            </div>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
};

export default UserDetailsDialog;
