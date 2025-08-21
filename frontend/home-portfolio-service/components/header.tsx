'use client';

import React, { useState, useEffect } from 'react';
import Link from 'next/link';
import Image from 'next/image';
import { useRouter, usePathname } from 'next/navigation';
import { Search, MessageCircle, Plus, User, LogOut, Menu, X, ChevronLeft, Bookmark, Crown } from 'lucide-react';
import { useAuth } from '@/lib/hooks/use-auth';
import { usePortfolioNavigation } from '@/lib/contexts/use-portfolio-navigation';
import { useUser } from '@/lib/contexts/user-context';
import { redirectToService } from '@/lib/config';
import { getSafeImageUrl } from '@/lib/image';
import { usePortfolioSearch } from '@/hooks/usePortfolioSearch';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import SearchResults from '@/components/ui/search-results';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';

export default function Header() {
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const { isAuthenticated, logout } = useAuth();
  const { user } = useUser();
  const router = useRouter();
  const pathname = usePathname();
  const { navigateBackToHome } = usePortfolioNavigation();
  
  // Portfolio search functionality
  const {
    searchTerm,
    setSearchTerm,
    results,
    loading,
    error,
    showResults,
    setShowResults,
    searchInputRef,
    searchContainerRef,
    handleResultClick
  } = usePortfolioSearch();

  // Helper function to get user's avatar with fallback
  const getUserAvatar = () => {
    if (user?.avatarUrl) {
      return getSafeImageUrl(user.avatarUrl);
    }
    // Generate default avatar with user's initials
    const userName = user ? 
      `${user.firstName || ''} ${user.lastName || ''}`.trim() || user.username || 'User' :
      'User';
    const initials = userName
      .split(' ')
      .map(name => name.charAt(0).toUpperCase())
      .slice(0, 2)
      .join('');
    return `https://ui-avatars.com/api/?name=${encodeURIComponent(initials)}&size=32&background=f0f0f0&color=666`;
  };

  // Check if we're on a portfolio page
  const isPortfolioPage = pathname === '/portfolio' || pathname?.startsWith('/portfolio');

  const toggleMobileMenu = () => {
    setIsMobileMenuOpen(!isMobileMenuOpen);
  };

  // Prevent body scroll when mobile menu is open
  useEffect(() => {
    if (isMobileMenuOpen) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = 'unset';
    }
    
    return () => {
      document.body.style.overflow = 'unset';
    };
  }, [isMobileMenuOpen]);

  const handleLogin = () => {
    const currentUrl = window.location.href;
    redirectToService('AUTH_USER_SERVICE', `login?callbackUrl=${encodeURIComponent(currentUrl)}`);
  };

  const handleMyPortfolioClick = () => {
    if (user?.id) {
      router.push(`/portfolio?user=${user.id}`);
    } else {
      // Fallback to auth service login if no user ID available
      redirectToService('AUTH_USER_SERVICE', 'login');
    }
  };

  const handleSignOut = async () => {
    await logout();
  };


  return (
    <>
      <header className="fixed top-0 left-0 right-0 w-full bg-white border-b border-[#E2E8F0] px-4 sm:px-8 lg:px-80 py-0 z-50">
        <div className="w-full max-w-[1280px] h-16 mx-auto flex items-center relative">
          {/* Back to Results Button - Desktop (Far Left) */}
          {isPortfolioPage && (
            <div className="hidden lg:flex items-center mr-4">
              <Button
                onClick={navigateBackToHome}
                variant="outline"
                size="sm"
                className="flex items-center gap-2 text-sm border-gray-300 hover:border-blue-500 hover:text-blue-600 transition-all duration-200"
              >
                <ChevronLeft size={16} />
                Back to Results
              </Button>
            </div>
          )}

          {/* Mobile Menu Button */}
          <div className="flex lg:hidden">
            <button
              onClick={toggleMobileMenu}
              className="p-2 rounded-md text-gray-600 hover:text-gray-900 hover:bg-gray-100 focus:outline-none focus:ring-2 focus:ring-inset focus:ring-blue-500"
              aria-label="Toggle mobile menu"
            >
              {isMobileMenuOpen ? <X size={24} /> : <Menu size={24} />}
            </button>
          </div>

          {/* Logo - Center on mobile, positioned after back button on desktop */}
          <div className="flex-shrink-0 flex items-center lg:mr-auto">
            <Link href="/">
              <h1 className="text-xl font-bold text-gray-900 hover:text-blue-600 transition-colors">
                GoalKeeper
              </h1>
            </Link>
          </div>

          {/* Search Bar - Desktop */}
          <div className="hidden lg:flex items-center flex-1 max-w-md mx-8">
            <div className="relative w-full">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-[#64748B]" />
              <Input
                ref={searchInputRef}
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                onFocus={() => setShowResults(true)}
                placeholder="Search portfolios, skills, or names..."
                className="w-full pl-10 pr-4 py-2 bg-[#F8FAFC] border border-[#E2E8F0] rounded-lg text-sm text-[#757575] placeholder:text-[#757575] focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
              <SearchResults
                results={results}
                loading={loading}
                error={error}
                showResults={showResults}
                onResultClick={handleResultClick}
                searchTerm={searchTerm}
                searchContainerRef={searchContainerRef}
              />
            </div>
          </div>

          {/* Right side items - Hidden on mobile */}
          <div className="hidden lg:flex items-center gap-4">
            {/* Message Icon - Only show when logged in */}
            {isAuthenticated && (
              <button
                className="p-2 rounded-lg flex flex-col justify-center items-center hover:bg-blue-50 hover:scale-105 transition-all active:scale-90 duration-150"
                onClick={() => redirectToService('MESSAGES_SERVICE', '')}
                aria-label="Messages"
              >
                <div className="flex justify-center items-start">
                <MessageCircle className="w-[13.3px] h-[13.33px] text-[#64748B] group-hover:text-[#2563EB] transition-colors" />
                </div>
              </button>
            )}

            {/* Publish Button */}
            <Button 
              onClick={() => redirectToService('AUTH_USER_SERVICE', 'publish')}
                              className="px-3 xl:px-4 py-2 bg-app-blue hover:bg-app-blue-hover text-white text-sm font-normal rounded-lg flex justify-start items-center gap-1 xl:gap-2"
            >
              <Plus className="w-[14px] h-[14px]" />
              <span className="hidden lg:inline">Publish</span>
              <span className="lg:hidden">+</span>
            </Button>

            {isAuthenticated && user ? (
              <DropdownMenu>
                <DropdownMenuTrigger className="flex items-center space-x-2 p-2 rounded-lg hover:bg-gray-100 transition-colors">
                  <Image
                    src={getUserAvatar()}
                    alt={`${user.firstName} ${user.lastName}` || 'User'}
                    width={32}
                    height={32}
                    className="rounded-full"
                  />
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end" className="w-56">
                  <DropdownMenuItem onClick={() => redirectToService('AUTH_USER_SERVICE', 'profile')}>
                    <User className="mr-2 h-4 w-4" />
                    Profile
                  </DropdownMenuItem>
                  <DropdownMenuItem onClick={() => redirectToService('AUTH_USER_SERVICE', 'ai')}>
                    <Crown className="mr-2 h-4 w-4" />
                    Premium
                  </DropdownMenuItem>
                  <DropdownMenuItem onClick={handleMyPortfolioClick}>
                    <Image
                      src="/icons/documentText.svg"
                      alt="My Portfolio"
                      width={16}
                      height={16}
                      className="mr-2 h-4 w-4"
                    />
                    My Portfolio
                  </DropdownMenuItem>
                  <DropdownMenuItem onClick={() => redirectToService('AUTH_USER_SERVICE', 'bookmarks')}>
                    <Bookmark className="mr-2 h-4 w-4" />
                    Bookmarks
                  </DropdownMenuItem>
                  <DropdownMenuItem onClick={handleSignOut}>
                    <LogOut className="mr-2 h-4 w-4" />
                    Sign Out
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
            ) : (
              <Button 
                onClick={handleLogin}
                variant="outline"
                className="px-4 py-2 text-sm"
              >
                Sign In
              </Button>
            )}
          </div>

          {/* Mobile Actions - Right side */}
          <div className="flex lg:hidden items-center space-x-2 ml-auto">
            {/* Back to Results Button (Mobile) - Next to burger menu */}
            {isPortfolioPage && (
              <Button
                onClick={navigateBackToHome}
                variant="outline"
                size="sm"
                className="flex items-center gap-1 text-xs px-2 py-1 border-gray-300 hover:border-blue-500 hover:text-blue-600 transition-all duration-200"
              >
                <ChevronLeft size={14} />
                <span className="hidden sm:inline">Back</span>
              </Button>
            )}

            {isAuthenticated && user ? (
              <div className="p-1">
                <Image
                  src={getUserAvatar()}
                  alt={`${user.firstName} ${user.lastName}` || 'User'}
                  width={32}
                  height={32}
                  className="rounded-full"
                />
              </div>
            ) : (
              <Button 
                onClick={handleLogin}
                variant="outline"
                size="sm"
                className="px-2 py-1 text-xs"
              >
                Sign In
              </Button>
            )}
          </div>
        </div>
      </header>

      {/* Mobile Menu Overlay */}
      {isMobileMenuOpen && (
        <div
          className="fixed inset-0 z-30 lg:hidden"
          onClick={toggleMobileMenu}
        ></div>
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
                 ref={searchInputRef}
                 value={searchTerm}
                 onChange={(e) => setSearchTerm(e.target.value)}
                 onFocus={() => setShowResults(true)}
                 placeholder="Search portfolios, skills, or names..."
                 className="w-full pl-10 pr-4 py-2 bg-white border border-[#E2E8F0] rounded-lg text-sm text-[#757575] placeholder:text-[#757575] focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
               />
              <SearchResults
                results={results}
                loading={loading}
                error={error}
                showResults={showResults}
                onResultClick={(result) => {
                  handleResultClick(result);
                  setIsMobileMenuOpen(false); // Close mobile menu when result is clicked
                }}
                searchTerm={searchTerm}
                searchContainerRef={searchContainerRef}
              />
            </div>
          </div>

          {/* Messages Button - Only for logged in users */}
          {isAuthenticated && (
            <div className="p-4 border-b border-[#E2E8F0]">
              <Button 
                onClick={() => {
                  setIsMobileMenuOpen(false);
                  redirectToService('MESSAGES_SERVICE', '');
                }}
                className="w-full px-4 py-2 bg-white hover:bg-gray-50 text-gray-700 text-sm font-normal rounded-lg flex justify-center items-center gap-2 border border-gray-200"
              >
                <MessageCircle className="w-[14px] h-[14px]" />
                Messages
              </Button>
            </div>
          )}

          {/* Publish Button */}
          <div className="p-4 border-b border-[#E2E8F0]">
            <Button 
              onClick={() => {
                setIsMobileMenuOpen(false);
                redirectToService('AUTH_USER_SERVICE', 'publish');
              }}
              className="w-full px-4 py-2 bg-app-blue hover:bg-app-blue-hover text-white text-sm font-normal rounded-lg flex justify-center items-center gap-2"
            >
              <Plus className="w-[14px] h-[14px]" />
              Publish
            </Button>
          </div>

          {/* Spacer */}
          <div className="flex-1"></div>

          {/* Bottom Section */}
          <div className="border-t border-[#E2E8F0] p-4">
            {isAuthenticated ? (
              <div className="space-y-1">
                <button
                  onClick={() => {
                    setIsMobileMenuOpen(false);
                    redirectToService('AUTH_USER_SERVICE', 'profile');
                  }}
                  className="w-full px-3 py-2 rounded-lg flex items-center gap-3 text-[#64748B] hover:bg-gray-50 text-left"
                >
                  <User className="w-4 h-4" />
                  <span className="text-sm">Profile</span>
                </button>
                <button
                  onClick={() => {
                    setIsMobileMenuOpen(false);
                    handleMyPortfolioClick();
                  }}
                  className="w-full px-3 py-2 rounded-lg flex items-center gap-3 text-[#64748B] hover:bg-gray-50 text-left"
                >
                  <Image
                    src="/icons/documentText.svg"
                    alt="My Portfolio"
                    width={16}
                    height={16}
                    className="w-4 h-4"
                  />
                  <span className="text-sm">My Portfolio</span>
                </button>
                <button
                  onClick={() => {
                    setIsMobileMenuOpen(false);
                    redirectToService('AUTH_USER_SERVICE', 'bookmarks');
                  }}
                  className="w-full px-3 py-2 rounded-lg flex items-center gap-3 text-[#64748B] hover:bg-gray-50 text-left"
                >
                  <Bookmark className="w-4 h-4" />
                  <span className="text-sm">Bookmarks</span>
                </button>
                <button
                  onClick={() => {
                    setIsMobileMenuOpen(false);
                    redirectToService('AUTH_USER_SERVICE', 'ai');
                  }}
                  className="w-full px-3 py-2 rounded-lg flex items-center gap-3 text-[#64748B] hover:bg-gray-50 text-left"
                >
                  <Crown className="w-4 h-4" />
                  <span className="text-sm">Premium</span>
                </button>
                <button
                  onClick={() => {
                    setIsMobileMenuOpen(false);
                    handleSignOut();
                  }}
                  className="w-full px-3 py-2 rounded-lg flex items-center gap-3 text-[#64748B] hover:bg-gray-50 text-left"
                >
                  <LogOut className="w-4 h-4" />
                  <span className="text-sm">Disconnect</span>
                </button>
              </div>
            ) : (
              <button
                onClick={() => {
                  setIsMobileMenuOpen(false);
                  handleLogin();
                }}
                className="w-full px-3 py-2 rounded-lg flex items-center justify-center gap-3 text-[#2563EB] hover:bg-blue-50 text-left font-medium"
              >
                <User className="w-4 h-4" />
                <span className="text-sm">Login</span>
              </button>
            )}
          </div>
        </div>
      </div>
    </>
  );
}
