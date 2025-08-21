import type { Metadata } from "next";
import "./globals.css";
import { Providers } from "@/components/providers";
import { SignoutHandler } from "@/components/signout-handler";

export const metadata: Metadata = {
  title: "GoalKeeper Messages",
  description: "Messages and communication for GoalKeeper",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  if (typeof window !== 'undefined') {
    try {
      const url = new URL(window.location.href);
      if (url.searchParams.get('signout') === '1') {
        localStorage.removeItem('jwt_auth_token');
        sessionStorage.removeItem('jwt_auth_token');
        url.searchParams.delete('signout');
        window.history.replaceState({}, '', url.toString());
      }
    } catch {}
  }
  return (
    <html lang="en">
      <body className="antialiased">
        <Providers>
          <SignoutHandler />
          {children}
        </Providers>
      </body>
    </html>
  );
}
