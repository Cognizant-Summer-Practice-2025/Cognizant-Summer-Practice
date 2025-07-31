'use client';

import React, { useState } from 'react';
import Image from 'next/image';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { X } from 'lucide-react';

interface RegistrationModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (userData: {
    firstName: string;
    lastName: string;
    professionalTitle: string;
    bio: string;
    location: string;
  }) => void;
  userEmail: string;
  userName: string;
  userAvatar?: string;
  isLoading?: boolean;
}

export default function RegistrationModal({
  isOpen,
  onClose,
  onSubmit,
  userEmail,
  userName,
  userAvatar,
  isLoading = false
}: RegistrationModalProps) {
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    professionalTitle: '',
    bio: '',
    location: ''
  });

  const [errors, setErrors] = useState({
    firstName: '',
    lastName: ''
  });

  const handleInputChange = (field: string, value: string) => {
    setFormData(prev => ({
      ...prev,
      [field]: value
    }));

    // Clear errors when user starts typing
    if (errors[field as keyof typeof errors]) {
      setErrors(prev => ({
        ...prev,
        [field]: ''
      }));
    }
  };

  const validateForm = () => {
    const newErrors = {
      firstName: formData.firstName.trim() ? '' : 'First name is required',
      lastName: formData.lastName.trim() ? '' : 'Last name is required'
    };

    setErrors(newErrors);
    return !newErrors.firstName && !newErrors.lastName;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (validateForm()) {
      onSubmit(formData);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50 backdrop-blur-sm p-4">
      <div className="bg-white rounded-xl shadow-2xl w-full max-w-md max-h-[95vh] sm:max-h-[90vh] overflow-y-auto animate-in fade-in zoom-in-95 slide-in-from-top-2 duration-300">
        {/* Header */}
        <div className="flex items-center justify-between p-4 sm:p-6 border-b border-gray-200">
          <div>
            <h2 className="text-lg sm:text-xl font-semibold text-gray-900">Complete Your Profile</h2>
            <p className="text-sm text-gray-600 mt-1">
              Please provide some basic information to continue
            </p>
          </div>
          <button
            onClick={onClose}
            disabled={isLoading}
            className="p-2 rounded-lg hover:bg-gray-100 transition-colors disabled:opacity-50"
          >
            <X className="w-5 h-5 text-gray-500" />
          </button>
        </div>

        {/* User Info Display */}
        <div className="px-4 sm:px-6 py-4 bg-gray-50 border-b border-gray-200">
          <div className="flex items-center space-x-3">
            {userAvatar && (
              <Image
                src={userAvatar}
                alt="Profile"
                className="w-10 h-10 rounded-full"
                width={40}
                height={40}
              />
            )}
            <div>
              <p className="font-medium text-gray-900">{userName}</p>
              <p className="text-sm text-gray-600">{userEmail}</p>
            </div>
          </div>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit} className="p-4 sm:p-6 space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                First Name *
              </label>
              <Input
                type="text"
                placeholder="John"
                value={formData.firstName}
                onChange={(e) => handleInputChange('firstName', e.target.value)}
                className={`w-full ${errors.firstName ? 'border-red-500' : ''}`}
                disabled={isLoading}
              />
              {errors.firstName && (
                <p className="text-xs text-red-500 mt-1">{errors.firstName}</p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Last Name *
              </label>
              <Input
                type="text"
                placeholder="Doe"
                value={formData.lastName}
                onChange={(e) => handleInputChange('lastName', e.target.value)}
                className={`w-full ${errors.lastName ? 'border-red-500' : ''}`}
                disabled={isLoading}
              />
              {errors.lastName && (
                <p className="text-xs text-red-500 mt-1">{errors.lastName}</p>
              )}
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Professional Title
            </label>
            <Input
              type="text"
              placeholder="Full Stack Developer"
              value={formData.professionalTitle}
              onChange={(e) => handleInputChange('professionalTitle', e.target.value)}
              className="w-full"
              disabled={isLoading}
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Bio
            </label>
            <textarea
              placeholder="Tell us about yourself..."
              value={formData.bio}
              onChange={(e) => handleInputChange('bio', e.target.value)}
              className="w-full min-h-[100px] p-3 border border-gray-300 rounded-md resize-none focus:ring-2 focus:ring-blue-500 focus:border-transparent text-sm disabled:opacity-50 disabled:cursor-not-allowed"
              disabled={isLoading}
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Location
            </label>
            <Input
              type="text"
              placeholder="San Francisco, CA"
              value={formData.location}
              onChange={(e) => handleInputChange('location', e.target.value)}
              className="w-full"
              disabled={isLoading}
            />
          </div>

          {/* Action Buttons */}
          <div className="flex justify-end space-x-3 pt-4 border-t border-gray-200">
            <Button
              type="button"
              variant="outline"
              onClick={onClose}
              disabled={isLoading}
              className="px-6"
            >
              Cancel
            </Button>
            <Button
              type="submit"
              disabled={isLoading}
                              className="px-6 bg-app-blue hover:bg-app-blue-hover"
            >
              {isLoading ? 'Creating Account...' : 'Complete Registration'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
} 