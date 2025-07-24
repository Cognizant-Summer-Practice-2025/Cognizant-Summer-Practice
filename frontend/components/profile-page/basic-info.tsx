'use client';

import React, { useState, useEffect } from 'react';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { useUser } from '@/lib/contexts/user-context';
import { Loading } from '@/components/loader';

export default function BasicInfo() {
  const { user, loading, error, updateUserData } = useUser();
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    professionalTitle: '',
    bio: '',
    location: '',
    email: ''
  });
  const [submitting, setSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

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
    // Clear success/error states when user types
    if (success) setSuccess(false);
    if (submitError) setSubmitError(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!user) {
      setSubmitError('User data not available');
      return;
    }

    try {
      setSubmitting(true);
      setSubmitError(null);
      
      await updateUserData({
        firstName: formData.firstName.trim() || undefined,
        lastName: formData.lastName.trim() || undefined,
        professionalTitle: formData.professionalTitle.trim() || undefined,
        bio: formData.bio.trim() || undefined,
        location: formData.location.trim() || undefined,
      });
      
      setSuccess(true);
      setTimeout(() => setSuccess(false), 3000); // Clear success message after 3 seconds
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to update profile';
      setSubmitError(errorMessage);
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return (
      <div className="bg-white rounded-lg shadow-sm p-4 sm:p-6 lg:p-8 w-full min-h-[600px] flex items-center justify-center">
        <div className="text-center">
          <Loading className="scale-50" backgroundColor="white" />
          <p className="text-gray-600 mt-4">Loading user information...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-white rounded-lg shadow-sm p-4 sm:p-6 lg:p-8 w-full min-h-[600px] flex items-center justify-center">
        <div className="text-center">
          <p className="text-red-600 mb-4">Error loading user information: {error}</p>
          <Button onClick={() => window.location.reload()} className="bg-app-blue hover:bg-app-blue-hover">
            Retry
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow-sm p-4 sm:p-6 lg:p-8 w-full">
      <h1 className="text-xl sm:text-2xl font-semibold text-gray-900 mb-4 sm:mb-6">Basic Information</h1>
      <p className="text-sm sm:text-base text-gray-600 mb-6 sm:mb-8">Update your basic profile information</p>
      
      {/* Success Message */}
      {success && (
        <div className="mb-4 p-3 bg-green-50 border border-green-200 rounded-lg">
          <p className="text-sm text-green-600">Profile updated successfully!</p>
        </div>
      )}
      
      {/* Error Message */}
      {submitError && (
        <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg">
          <p className="text-sm text-red-600">{submitError}</p>
        </div>
      )}
      
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
            disabled={submitting}
            className="bg-app-blue hover:bg-app-blue-hover text-white px-6 py-2"
          >
            {submitting ? 'Saving...' : 'Save Changes'}
          </Button>
          <Button 
            type="button"
            variant="outline"
            disabled={submitting}
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
                setSubmitError(null);
                setSuccess(false);
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