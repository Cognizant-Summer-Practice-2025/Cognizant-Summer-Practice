'use client';

import React, { useState } from 'react';
import Link from 'next/link';
import { Search, MessageCircle, Plus, User, Settings, LogOut, Menu, X } from 'lucide-react';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';

export default function Header() {
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

  const toggleMobileMenu = () => {
    setIsMobileMenuOpen(!isMobileMenuOpen);
  };



  return (
    <>
      <header className="w-full bg-white border-b border-[#E2E8F0] px-4 sm:px-8 lg:px-80 py-0 relative z-40">
        <div className="w-full max-w-[1280px] h-16 relative mx-auto">
          {/* Mobile Menu Button */}
          <div className="absolute left-0 top-4 flex lg:hidden">
            <button
              onClick={toggleMobileMenu}
              className="p-2 rounded-lg hover:bg-gray-100 transition-colors"
            >
              {isMobileMenuOpen ? (
                <X className="w-6 h-6 text-[#020817]" />
              ) : (
                <Menu className="w-6 h-6 text-[#020817]" />
              )}
            </button>
          </div>

          {/* Logo */}
          <div className="absolute left-12 lg:left-6 top-4 flex flex-col justify-start items-start">
            <Link href="/" className="text-[#020817] text-xl font-semibold font-['Inter'] leading-8 hover:text-[#2563EB] transition-colors cursor-pointer">
              GoalKeeper
            </Link>
          </div>

          {/* Search Bar - Hidden on mobile */}
          <div className="hidden lg:block absolute left-[368.88px] top-[15px] w-[464px] max-w-[464px] px-8">
            <div className="w-full max-w-[400px] relative flex flex-col justify-start items-start">
              <div className="relative w-full">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-[#64748B]" />
                <Input
                  placeholder="Search portfolios, skills, or names..."
                  className="w-full pl-10 pr-4 py-2 bg-white border border-[#E2E8F0] rounded-lg text-sm text-[#757575] placeholder:text-[#757575] focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
            </div>
          </div>

          {/* Right side items - Hidden on mobile */}
          <div className="hidden lg:flex absolute right-0 top-4 justify-start items-center gap-4">
            {/* Message Icon */}
            <div className="p-2 rounded-lg flex flex-col justify-center items-center">
              <div className="flex justify-center items-start">
                <MessageCircle className="w-[13.3px] h-[13.33px] text-[#64748B]" />
              </div>
            </div>

            {/* Publish Button */}
            <Button className="px-4 py-2 bg-[#2563EB] hover:bg-[#1d4ed8] text-[#F8FAFC] text-sm font-normal rounded-lg flex justify-start items-center gap-2">
              <Plus className="w-[14px] h-[14px]" />
              Publish
            </Button>

            {/* Profile  */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <img
                  className="w-8 h-8 rounded-2xl cursor-pointer hover:ring-2 hover:ring-blue-500 hover:ring-offset-2 transition-all"
                  src="https://placehold.co/32x32"
                  alt="Profile"
                />
              </DropdownMenuTrigger>
              <DropdownMenuContent className="w-48" align="end">
                <DropdownMenuItem className="flex items-center gap-2 cursor-pointer">
                  <User className="w-4 h-4" />
                  Profile
                </DropdownMenuItem>
                <DropdownMenuItem className="flex items-center gap-2 cursor-pointer">
                  <Settings className="w-4 h-4" />
                  Settings
                </DropdownMenuItem>
                <DropdownMenuItem className="flex items-center gap-2 cursor-pointer">
                  <LogOut className="w-4 h-4" />
                  Disconnect
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          </div>

          {/* Mobile Profile Picture - Only on mobile */}
          <div className="lg:hidden absolute right-4 top-4">
            <img
              className="w-8 h-8 rounded-2xl"
              src="https://placehold.co/32x32"
              alt="Profile"
            />
          </div>
        </div>
      </header>

      {/* Mobile Menu Overlay */}
      {isMobileMenuOpen && (
        <div 
          className="fixed inset-0 bg-black bg-opacity-50 z-30 lg:hidden"
          onClick={toggleMobileMenu}
        />
      )}

      {/* Mobile Menu */}
      <div className={`fixed top-16 left-0 w-80 h-[calc(100vh-4rem)] bg-white border-r border-[#E2E8F0] transform transition-transform duration-300 ease-in-out z-40 lg:hidden ${
        isMobileMenuOpen ? 'translate-x-0' : '-translate-x-full'
      }`}>
        <div className="flex flex-col h-full">
          {/* Search Section */}
          <div className="p-4 border-b border-[#E2E8F0]">
            <div className="relative w-full">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-[#64748B]" />
              <Input
                placeholder="Search portfolios, skills, or names..."
                className="w-full pl-10 pr-4 py-2 bg-white border border-[#E2E8F0] rounded-lg text-sm text-[#757575] placeholder:text-[#757575] focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>
          </div>

          {/* Publish Button */}
          <div className="p-4 border-b border-[#E2E8F0]">
            <Button className="w-full px-4 py-2 bg-[#2563EB] hover:bg-[#1d4ed8] text-[#F8FAFC] text-sm font-normal rounded-lg flex justify-center items-center gap-2">
              <Plus className="w-[14px] h-[14px]" />
              Publish
            </Button>
          </div>

          {/* Spacer */}
          <div className="flex-1"></div>

          {/* Bottom Section */}
          <div className="border-t border-[#E2E8F0] p-4">
            <div className="space-y-1">
              <button
                onClick={() => setIsMobileMenuOpen(false)}
                className="w-full px-3 py-2 rounded-lg flex items-center gap-3 text-[#64748B] hover:bg-gray-50 text-left"
              >
                <User className="w-4 h-4" />
                <span className="text-sm">Profile</span>
              </button>
              <button
                onClick={() => setIsMobileMenuOpen(false)}
                className="w-full px-3 py-2 rounded-lg flex items-center gap-3 text-[#64748B] hover:bg-gray-50 text-left"
              >
                <Settings className="w-4 h-4" />
                <span className="text-sm">Settings</span>
              </button>
              <button
                onClick={() => setIsMobileMenuOpen(false)}
                className="w-full px-3 py-2 rounded-lg flex items-center gap-3 text-[#64748B] hover:bg-gray-50 text-left"
              >
                <LogOut className="w-4 h-4" />
                <span className="text-sm">Disconnect</span>
              </button>
            </div>
          </div>
        </div>
      </div>
    </>
  );
} 