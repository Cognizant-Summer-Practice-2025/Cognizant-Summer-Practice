import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  env: {
    USER_SERVICE_URL: process.env.USER_SERVICE_URL || 'http://localhost:5002',
    MESSAGES_SERVICE_URL: process.env.MESSAGES_SERVICE_URL || 'http://localhost:5003',
    PORTFOLIO_SERVICE_URL: process.env.PORTFOLIO_SERVICE_URL || 'http://localhost:5201',
    AI_SERVICE_URL: process.env.AI_SERVICE_URL || 'http://localhost:5004',
  },
  images: {
    remotePatterns: [
      {
        protocol: 'https',
        hostname: 'avatars.githubusercontent.com',
      },
      {
        protocol: 'https',
        hostname: 'images.unsplash.com',
      },
      {
        protocol: 'https',
        hostname: 'ui-avatars.com',
      },
      {
        protocol: 'https',
        hostname: 'placehold.co',
      },
      {
        protocol: 'https',
        hostname: 'lh3.googleusercontent.com',
      },
      {
        protocol: 'https',
        hostname: 'picsum.photos',
      },
      {
        protocol: 'http',
        hostname: 'localhost',
        port: '5201',
      }
    ],
  },
};

export default nextConfig;
