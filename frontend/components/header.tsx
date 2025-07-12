import React from 'react';
import Link from 'next/link';
import { Search, MessageCircle, Plus, User, Settings, LogOut } from 'lucide-react';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';

export default function Header() {
  return (
    <header className="w-full bg-white border-b border-[#E2E8F0] px-80 py-0">
      <div className="w-full max-w-[1280px] h-16 relative">
        {/* Logo */}
        <div className="absolute left-6 top-4 flex flex-col justify-start items-start">
          <Link href="/" className="text-[#020817] text-xl font-semibold font-['Inter'] leading-8 hover:text-[#2563EB] transition-colors cursor-pointer">
            GoalKeeper
          </Link>
        </div>

        {/* Search Bar */}
        <div className="absolute left-[368.88px] top-[15px] w-[464px] max-w-[464px] px-8 flex flex-col justify-start items-start">
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

        {/* Right side items */}
        <div className="absolute left-[1064.50px] top-4 flex justify-start items-center gap-4">
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
      </div>
    </header>
  );
} 