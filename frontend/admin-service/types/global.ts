// Global type declarations for the admin-service

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
  var adminServiceUserStorage: Map<string, ServiceUserData>;
}
