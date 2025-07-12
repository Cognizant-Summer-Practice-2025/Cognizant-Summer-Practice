"use client";

import { useState } from "react";
import { ChevronDown } from "lucide-react";

export default function Settings() {
  const [portfolioVisibility, setPortfolioVisibility] = useState("Public");
  const [allowMessages, setAllowMessages] = useState(true);
  const [emailNotifications, setEmailNotifications] = useState(true);
  const [showVisibilityDropdown, setShowVisibilityDropdown] = useState(false);

  const visibilityOptions = ["Public", "Private", "Friends Only"];

  const handleDeleteAccount = () => {
    alert("ERROR");
  };

  return (
    <div className="bg-white rounded-lg shadow-sm p-4 sm:p-6 lg:p-8 w-full min-h-[600px] overflow-hidden">
      <div className="flex flex-col gap-4 sm:gap-6">
        
        <div className="flex flex-col">
          <h1 className="text-xl sm:text-2xl font-semibold text-[#020817] leading-tight sm:leading-[38.4px] mb-1">
            Settings
          </h1>
          <p className="text-sm text-[#64748B] leading-[22.4px]">
            Manage your account preferences
          </p>
        </div>

        <div className="w-full max-w-[800px] flex flex-col gap-6 sm:gap-8">
          {/* Privacy Section */}
          <div className="border border-[#E2E8F0] rounded-lg p-0 relative">
            <div className="p-4 sm:p-6 pb-0">
              <h2 className="text-base sm:text-lg font-semibold text-[#020817] leading-tight sm:leading-[28.8px] mb-4">
                Privacy
              </h2>
            </div>
            
            {/* Portfolio Visibility */}
            <div className="px-4 sm:px-6 py-4 border-b border-[#E2E8F0] flex flex-col sm:flex-row sm:justify-between sm:items-center gap-3 sm:gap-0">
              <div className="flex flex-col gap-[3px] flex-1">
                <div className="pb-[0.59px]">
                  <h3 className="text-sm sm:text-base font-medium text-[#020817] leading-tight sm:leading-[25.6px]">
                    Portfolio Visibility
                  </h3>
                </div>
                <p className="text-xs sm:text-sm text-[#64748B] leading-[22.4px]">
                  Control who can see your portfolio
                </p>
              </div>
              <div className="relative w-full sm:w-auto">
                <button
                  onClick={() => setShowVisibilityDropdown(!showVisibilityDropdown)}
                  className="flex items-center justify-between w-full sm:w-auto gap-3 bg-white border border-[#E2E8F0] rounded-lg px-4 sm:px-5 py-3 text-sm text-[#020817] leading-4 hover:bg-gray-50"
                >
                  {portfolioVisibility}
                  <ChevronDown className="w-4 h-4" />
                </button>
                {showVisibilityDropdown && (
                  <div className="absolute top-full mt-1 right-0 bg-white border border-[#E2E8F0] rounded-lg shadow-lg z-10 min-w-[120px] w-full sm:w-auto">
                    {visibilityOptions.map((option) => (
                      <button
                        key={option}
                        onClick={() => {
                          setPortfolioVisibility(option);
                          setShowVisibilityDropdown(false);
                        }}
                        className="w-full text-left px-4 py-2 text-sm text-[#020817] hover:bg-gray-50 first:rounded-t-lg last:rounded-b-lg"
                      >
                        {option}
                      </button>
                    ))}
                  </div>
                )}
              </div>
            </div>

            {/* Allow Messages */}
            <div className="px-4 sm:px-6 py-4 flex flex-col sm:flex-row sm:justify-between sm:items-center gap-3 sm:gap-0">
              <div className="flex flex-col gap-[3px] flex-1">
                <div className="pb-[0.59px]">
                  <h3 className="text-sm sm:text-base font-medium text-[#020817] leading-tight sm:leading-[25.6px]">
                    Allow Messages
                  </h3>
                </div>
                <p className="text-xs sm:text-sm text-[#64748B] leading-[22.4px]">
                  Let other users send you messages
                </p>
              </div>
              <div className="flex justify-end">
                <button
                  onClick={() => setAllowMessages(!allowMessages)}
                  className={`w-11 h-6 rounded-full transition-colors ${
                    allowMessages ? "bg-[#2563EB]" : "bg-gray-300"
                  }`}
                >
                  <div
                    className={`w-[18px] h-[18px] bg-white rounded-full transition-transform ${
                      allowMessages ? "translate-x-[23px]" : "translate-x-[3px]"
                    }`}
                  />
                </button>
              </div>
            </div>
          </div>

          {/* Notifications Section */}
          <div className="border border-[#E2E8F0] rounded-lg p-4 sm:p-6">
            <h2 className="text-base sm:text-lg font-semibold text-[#020817] leading-tight sm:leading-[28.8px] mb-4">
              Notifications
            </h2>
            
            <div className="pt-4 flex flex-col sm:flex-row sm:justify-between sm:items-center gap-3 sm:gap-0">
              <div className="flex flex-col gap-[3px] flex-1">
                <div className="pb-[0.59px]">
                  <h3 className="text-sm sm:text-base font-medium text-[#020817] leading-tight sm:leading-[25.6px]">
                    Email Notifications
                  </h3>
                </div>
                <p className="text-xs sm:text-sm text-[#64748B] leading-[22.4px]">
                  Receive email updates
                </p>
              </div>
              <div className="flex justify-end">
                <button
                  onClick={() => setEmailNotifications(!emailNotifications)}
                  className={`w-11 h-6 rounded-full transition-colors ${
                    emailNotifications ? "bg-[#2563EB]" : "bg-gray-300"
                  }`}
                >
                  <div
                    className={`w-[18px] h-[18px] bg-white rounded-full transition-transform ${
                      emailNotifications ? "translate-x-[23px]" : "translate-x-[3px]"
                    }`}
                  />
                </button>
              </div>
            </div>
          </div>

          {/* Delete Section */}
          <div className="border border-[rgba(239,68,68,0.3)] rounded-lg p-4 sm:p-6">
            <h2 className="text-base sm:text-lg font-semibold text-[#EF4444] leading-tight sm:leading-[28.8px] mb-4">
              Delete
            </h2>
            
            <div className="pt-4 flex flex-col sm:flex-row sm:justify-between sm:items-center gap-3 sm:gap-0">
              <div className="flex flex-col gap-[3px] flex-1">
                <div className="pb-[0.59px]">
                  <h3 className="text-sm sm:text-base font-medium text-[#020817] leading-tight sm:leading-[25.6px]">
                    Delete Account
                  </h3>
                </div>
                <p className="text-xs sm:text-sm text-[#64748B] leading-[22.4px]">
                  Permanently delete your account and all data
                </p>
              </div>
              <div className="flex justify-end">
                <button
                  onClick={handleDeleteAccount}
                  className="bg-[#EF4444] text-[#F8FAFC] px-4 py-2 rounded-lg text-sm font-normal hover:bg-red-600 transition-colors w-full sm:w-auto"
                >
                  Delete Account
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
} 