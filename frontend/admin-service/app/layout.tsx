import type { Metadata } from "next";
import { Providers } from "@/components/providers";
import { SignoutHandler } from "@/components/signout-handler";
import "./globals.css";

export const metadata: Metadata = {
  title: "GoalKeeper Admin",
  description: "Admin dashboard for GoalKeeper",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
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
