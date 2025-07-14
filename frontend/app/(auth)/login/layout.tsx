import Image from 'next/image';

export default function LoginLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="min-h-screen flex">
      {/* Left side - GoalKeeper Branding */}
      <div className="flex-1 flex items-center justify-center px-8 bg-gradient-to-br from-blue-600 to-blue-600/90">
        <div className="max-w-[500px] min-w-[500px] flex flex-col gap-4">
          {/* GoalKeeper Title */}
          <div className="flex flex-col items-center pb-0.5">
            <h1 className="text-center text-white text-5xl font-bold">
              GoalKeeper
            </h1>
          </div>
          
          {/* Subtitle */}
          <div className="flex flex-col items-center opacity-90">
            <p className="text-center text-white text-xl">
              Showcase your work, connect with professionals,<br />
              and build your career.
            </p>
          </div>
          
          {/* Features List */}
          <div className="flex flex-col gap-6 pt-8">
            <div className="flex items-center gap-4">
              <div className="flex flex-col">
                <Image
                  src="/icons/palette-colors.svg"
                  alt="Customizable templates"
                  width={24}
                  height={24}
                  className="w-6 h-6 brightness-0 invert"
                />
              </div>
              <div className="flex flex-col">
                <span className="text-white text-base">
                  Customizable portfolio templates
                </span>
              </div>
            </div>
            
            <div className="flex items-center gap-4">
              <div className="flex flex-col">
                <Image
                  src="/icons/user-group.svg"
                  alt="Connect with professionals"
                  width={24}
                  height={24}
                  className="w-6 h-6 brightness-0 invert"
                />
              </div>
              <div className="flex flex-col">
                <span className="text-white text-base">
                  Connect with other professionals
                </span>
              </div>
            </div>
            
            <div className="flex items-center gap-4">
              <div className="flex flex-col">
                <Image
                  src="/icons/chart-line-up.svg"
                  alt="Track performance"
                  width={24}
                  height={24}
                  className="w-6 h-6 brightness-0 invert"
                />
              </div>
              <div className="flex flex-col">
                <span className="text-white text-base">
                  Track portfolio performance
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>
      
      {/* Right side - Login Form */}
      <div className="flex-1 flex items-center justify-center p-8 bg-white">
        {children}
      </div>
    </div>
  );
}
