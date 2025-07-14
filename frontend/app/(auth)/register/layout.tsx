import Image from 'next/image';

export default function RegisterLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="min-h-screen flex">
      {/* Left side - GoalKeeper Branding */}
      <div className="flex-1 flex items-center justify-center px-8 bg-gradient-to-br from-green-600 to-green-600/90">
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
              Welcome! Let's complete your profile<br />
              to get you started.
            </p>
          </div>
          
          {/* Features List */}
          <div className="flex flex-col gap-6 pt-8">
            <div className="flex items-center gap-4">
              <div className="flex flex-col">
                <Image
                  src="/icons/user-circle.svg"
                  alt="Profile setup"
                  width={24}
                  height={24}
                  className="w-6 h-6 brightness-0 invert"
                />
              </div>
              <div className="flex flex-col">
                <span className="text-white text-base">
                  Create your professional profile
                </span>
              </div>
            </div>
            
            <div className="flex items-center gap-4">
              <div className="flex flex-col">
                <Image
                  src="/icons/lightning-bolt.svg"
                  alt="Quick setup"
                  width={24}
                  height={24}
                  className="w-6 h-6 brightness-0 invert"
                />
              </div>
              <div className="flex flex-col">
                <span className="text-white text-base">
                  Quick and easy setup process
                </span>
              </div>
            </div>
            
            <div className="flex items-center gap-4">
              <div className="flex flex-col">
                <Image
                  src="/icons/rocket.svg"
                  alt="Get started"
                  width={24}
                  height={24}
                  className="w-6 h-6 brightness-0 invert"
                />
              </div>
              <div className="flex flex-col">
                <span className="text-white text-base">
                  Start building your portfolio
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>
      
      {/* Right side - Registration Form */}
      <div className="flex-1 flex items-center justify-center p-8 bg-white">
        {children}
      </div>
    </div>
  );
} 