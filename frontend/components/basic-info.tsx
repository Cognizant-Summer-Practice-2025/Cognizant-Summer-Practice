'use client';

import React from 'react';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';

export default function BasicInfo() {
  return (
    <div className="bg-white rounded-lg shadow-sm p-8 w-full min-h-[600px]">
      <h1 className="text-2xl font-semibold text-gray-900 mb-6">Basic Information</h1>
      <p className="text-gray-600 mb-8">Update your basic profile information</p>
      
      <form className="space-y-6">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              First Name
            </label>
            <Input 
              type="text" 
              placeholder="John" 
              className="w-full"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Last Name
            </label>
            <Input 
              type="text" 
              placeholder="Doe" 
              className="w-full"
            />
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Professional Title
          </label>
          <Input 
            type="text" 
            placeholder="Full Stack Developer" 
            className="w-full"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Bio
          </label>
          <textarea 
            placeholder="Passionate developer creating modern web applications with React and Node.js"
            className="w-full min-h-[120px] p-3 border border-gray-300 rounded-md resize-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Location
          </label>
          <Input 
            type="text" 
            placeholder="San Francisco, CA" 
            className="w-full"
          />
        </div>

        <div className="flex justify-start gap-4 pt-6">
          <Button variant="outline" className="px-6">
            Cancel
          </Button>
          <Button className="px-6 bg-blue-600 hover:bg-blue-700">
            Save Changes
          </Button>
        </div>
      </form>
    </div>
  );
} 