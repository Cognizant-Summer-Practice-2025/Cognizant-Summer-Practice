// Global type declarations for the messages-service

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
  var messagesServiceUserStorage: Map<string, ServiceUserData>;
  var messagesServiceSessionStorage: Map<string, ServiceUserData>;
}

export {};
