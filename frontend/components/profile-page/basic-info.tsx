'use client';

import React, { useState, useEffect } from 'react';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { useUser } from '@/lib/contexts/user-context';

export default function BasicInfo() {
  const { user, loading, error } = useUser();
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    professionalTitle: '',
    bio: '',
    location: '',
    email: ''
  });

  // Update form data when user data is loaded
  useEffect(() => {
    if (user) {
      setFormData({
        firstName: user.firstName || '',
        lastName: user.lastName || '',
        professionalTitle: user.professionalTitle || '',
        bio: user.bio || '',
        location: user.location || '',
        email: user.email || ''
      });
    }
  }, [user]);

  const handleInputChange = (field: string, value: string) => {
    setFormData(prev => ({
      ...prev,
      [field]: value
    }));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    // TODO: Implement user data update logic
    console.log('Updated user data:', formData);
  };

  if (loading) {
    return (
      <div className="bg-white rounded-lg shadow-sm p-4 sm:p-6 lg:p-8 w-full min-h-[600px] flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mx-auto mb-4"></div>
          <p className="text-gray-600">Loading user information...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-white rounded-lg shadow-sm p-4 sm:p-6 lg:p-8 w-full min-h-[600px] flex items-center justify-center">
        <div className="text-center">
          <p className="text-red-600 mb-4">Error loading user information: {error}</p>
          <Button onClick={() => window.location.reload()} className="bg-blue-600 hover:bg-blue-700">
            Retry
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow-sm p-4 sm:p-6 lg:p-8 w-full min-h-[600px]">
      <h1 className="text-xl sm:text-2xl font-semibold text-gray-900 mb-4 sm:mb-6">Basic Information</h1>
      <p className="text-sm sm:text-base text-gray-600 mb-6 sm:mb-8">Update your basic profile information</p>
      
      <form onSubmit={handleSubmit} className="space-y-4 sm:space-y-6">
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 sm:gap-6">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              First Name
            </label>
            <Input 
              type="text" 
              value={formData.firstName}
              onChange={(e) => handleInputChange('firstName', e.target.value)}
              placeholder="Enter your first name" 
              className="w-full"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Last Name
            </label>
            <Input 
              type="text" 
              value={formData.lastName}
              onChange={(e) => handleInputChange('lastName', e.target.value)}
              placeholder="Enter your last name" 
              className="w-full"
            />
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Email
          </label>
          <Input 
            type="email" 
            value={formData.email}
            disabled
            className="w-full bg-gray-50"
            title="Email cannot be changed"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Professional Title
          </label>
          <Input 
            type="text" 
            value={formData.professionalTitle}
            onChange={(e) => handleInputChange('professionalTitle', e.target.value)}
            placeholder="e.g., Full Stack Developer, Data Scientist" 
            className="w-full"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Bio
          </label>
          <textarea 
            value={formData.bio}
            onChange={(e) => handleInputChange('bio', e.target.value)}
            placeholder="Tell us about yourself and your professional experience..."
            className="w-full min-h-[120px] p-3 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent resize-vertical"
            rows={5}
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Location
          </label>
          <Input 
            type="text" 
            value={formData.location}
            onChange={(e) => handleInputChange('location', e.target.value)}
            placeholder="City, Country" 
            className="w-full"
          />
        </div>

        <div className="flex flex-col sm:flex-row gap-3 pt-4">
          <Button 
            type="submit"
            className="bg-blue-600 hover:bg-blue-700 text-white px-6 py-2"
          >
            Save Changes
          </Button>
          <Button 
            type="button"
            variant="outline"
            className="px-6 py-2"
            onClick={() => {
              if (user) {
                setFormData({
                  firstName: user.firstName || '',
                  lastName: user.lastName || '',
                  professionalTitle: user.professionalTitle || '',
                  bio: user.bio || '',
                  location: user.location || '',
                  email: user.email || ''
                });
              }
            }}
          >
            Cancel
          </Button>
        </div>
      </form>
    </div>
  );
} 