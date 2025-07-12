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
    <div className="bg-white rounded-lg shadow-sm p-8 w-full min-h-[600px] overflow-hidden">
      <div className="flex flex-col gap-6">
        
        <div className="flex flex-col">
          <h1 className="text-2xl font-semibold text-[#020817] leading-[38.4px] mb-1">
            Settings
          </h1>
          <p className="text-sm text-[#64748B] leading-[22.4px]">
            Manage your account preferences
          </p>
        </div>

        <div className="w-full max-w-[600px] flex flex-col gap-8">
          {/* Privacy Section */}
          <div className="border border-[#E2E8F0] rounded-lg p-0 relative">
            <div className="p-6 pb-0">
              <h2 className="text-lg font-semibold text-[#020817] leading-[28.8px] mb-4">
                Privacy
              </h2>
            </div>
            
            {/* Portfolio Visibility */}
            <div className="px-6 py-4 border-b border-[#E2E8F0] flex justify-between items-center">
              <div className="flex flex-col gap-[3px]">
                <div className="pb-[0.59px]">
                  <h3 className="text-base font-medium text-[#020817] leading-[25.6px]">
                    Portfolio Visibility
                  </h3>
                </div>
                <p className="text-sm text-[#64748B] leading-[22.4px]">
                  Control who can see your portfolio
                </p>
              </div>
              <div className="relative">
                <button
                  onClick={() => setShowVisibilityDropdown(!showVisibilityDropdown)}
                  className="flex items-center gap-3 bg-white border border-[#E2E8F0] rounded-lg px-5 py-3 text-sm text-[#020817] leading-4 hover:bg-gray-50"
                >
                  {portfolioVisibility}
                  <ChevronDown className="w-4 h-4" />
                </button>
                {showVisibilityDropdown && (
                  <div className="absolute top-full mt-1 right-0 bg-white border border-[#E2E8F0] rounded-lg shadow-lg z-10 min-w-[120px]">
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
            <div className="px-6 py-4 flex justify-between items-center">
              <div className="flex flex-col gap-[3px]">
                <div className="pb-[0.59px]">
                  <h3 className="text-base font-medium text-[#020817] leading-[25.6px]">
                    Allow Messages
                  </h3>
                </div>
                <p className="text-sm text-[#64748B] leading-[22.4px]">
                  Let other users send you messages
                </p>
              </div>
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

          {/* Notifications Section */}
          <div className="border border-[#E2E8F0] rounded-lg p-6">
            <h2 className="text-lg font-semibold text-[#020817] leading-[28.8px] mb-4">
              Notifications
            </h2>
            
            <div className="pt-4 flex justify-between items-center">
              <div className="flex flex-col gap-[3px]">
                <div className="pb-[0.59px]">
                  <h3 className="text-base font-medium text-[#020817] leading-[25.6px]">
                    Email Notifications
                  </h3>
                </div>
                <p className="text-sm text-[#64748B] leading-[22.4px]">
                  Receive email updates
                </p>
              </div>
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

          {/* Delete Section */}
          <div className="border border-[rgba(239,68,68,0.3)] rounded-lg p-6">
            <h2 className="text-lg font-semibold text-[#EF4444] leading-[28.8px] mb-4">
              Delete
            </h2>
            
            <div className="pt-4 flex justify-between items-center">
              <div className="flex flex-col gap-[3px]">
                <div className="pb-[0.59px]">
                  <h3 className="text-base font-medium text-[#020817] leading-[25.6px]">
                    Delete Account
                  </h3>
                </div>
                <p className="text-sm text-[#64748B] leading-[22.4px]">
                  Permanently delete your account and all data
                </p>
              </div>
              <button
                onClick={handleDeleteAccount}
                className="bg-[#EF4444] text-[#F8FAFC] px-4 py-2 rounded-lg text-sm font-normal hover:bg-red-600 transition-colors"
              >
                Delete Account
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
} 