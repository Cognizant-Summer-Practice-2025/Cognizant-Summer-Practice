'use client';

import React, { useState } from 'react';
import { X } from 'lucide-react';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';

export default function Skills() {
  const [skills, setSkills] = useState([
    'JavaScript',
    'React',
    'Node.js',
    'TypeScript'
  ]);
  const [newSkill, setNewSkill] = useState('');

  const addSkill = () => {
    if (newSkill.trim() && !skills.includes(newSkill.trim())) {
      setSkills([...skills, newSkill.trim()]);
      setNewSkill('');
    }
  };

  const removeSkill = (skillToRemove: string) => {
    setSkills(skills.filter(skill => skill !== skillToRemove));
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      addSkill();
    }
  };

  return (
    <div className="bg-white rounded-lg shadow-sm p-8 w-full min-h-[600px]">
      <h1 className="text-2xl font-semibold text-gray-900 mb-6">Skills</h1>
      <p className="text-gray-600 mb-8">Add and manage your technical skills</p>
      
      <div className="w-full max-w-[600px] space-y-6">
        {/* Add Skill Section */}
        <div className="flex justify-start items-start gap-3">
          <div className="flex-1">
            <Input
              type="text"
              placeholder="Add a skill..."
              value={newSkill}
              onChange={(e) => setNewSkill(e.target.value)}
              onKeyPress={handleKeyPress}
              className="w-full px-4 py-3 bg-white border border-[#E2E8F0] rounded-lg text-sm text-[#757575] placeholder:text-[#757575] focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
          </div>
          <Button
            onClick={addSkill}
            className="px-4 py-3 bg-[#2563EB] hover:bg-[#1d4ed8] text-[#F8FAFC] text-sm font-normal rounded-lg"
          >
            Add
          </Button>
        </div>

        {/* Skills Display */}
        <div className="flex justify-start items-start gap-3 flex-wrap">
          {skills.map((skill, index) => (
            <div
              key={index}
              className="py-2 px-3 bg-[#F1F5F9] rounded-lg flex justify-start items-center gap-2"
            >
              <div className="flex flex-col justify-start items-start">
                <span className="text-[#020817] text-sm font-normal font-['Inter'] leading-[22.4px]">
                  {skill}
                </span>
              </div>
              <button
                onClick={() => removeSkill(skill)}
                className="w-4 h-4 flex justify-center items-center hover:bg-gray-300 rounded-sm transition-colors"
              >
                <X className="w-3 h-3 text-[#64748B]" />
              </button>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
} 