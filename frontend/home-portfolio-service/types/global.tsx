// Global type declarations for the home-portfolio-service

export interface ServiceUserData {
  id: string;
  email: string;
  username: string;
  firstName?: string;
  lastName?: string;
  professionalTitle?: string;
  bio?: string;
  location?: string;
  profileImage?: string;
  isActive: boolean;
  isAdmin: boolean;
  lastLoginAt?: string;
  accessToken?: string;
}

declare global {
  // eslint-disable-next-line no-var
  var homePortfolioServiceUserStorage: Map<string, ServiceUserData>;
}

export {};
