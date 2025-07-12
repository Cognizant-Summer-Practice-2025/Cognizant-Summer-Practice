import ProfileSidebar from '@/components/profile-sidebar';

export default function ProfileLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <div className="fixed top-16 left-0 right-0 bottom-0 flex overflow-hidden">
      {/* Sidebar */}
      <div className="w-72 h-full">
        <ProfileSidebar activeTab="basic-info" />
      </div>
      
      {/* Main Content */}
      <div className="flex-1 bg-gray-50 overflow-hidden">
        {children}
      </div>
    </div>
  );
}
  