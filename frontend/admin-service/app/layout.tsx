import type { Metadata } from "next";
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
        {children}
      </body>
    </html>
  );
}
