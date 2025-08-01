export interface User {
  id: string;
  email: string;
  username: string;
  firstName?: string;
  lastName?: string;
  professionalTitle?: string;
  bio?: string;
  location?: string;
  avatarUrl?: string;
  isActive: boolean;
  isAdmin: boolean;
  lastLoginAt?: string;
}

export interface SearchUser {
  id: string;
  username: string;
  firstName?: string;
  lastName?: string;
  fullName: string;
  professionalTitle?: string;
  avatarUrl?: string;
  isActive: boolean;
}

export interface CheckEmailResponse {
  exists: boolean;
  user: User | null;
}

export interface RegisterUserRequest {
  email: string;
  firstName?: string;
  lastName?: string;
  professionalTitle?: string;
  bio?: string;
  location?: string;
  profileImage?: string;
}

export interface OAuthProvider {
  id: string;
  userId: string;
  provider: 'Google' | 'GitHub' | 'Facebook' | 'LinkedIn';
  providerId: string;
  providerEmail: string;
  tokenExpiresAt?: string;
  createdAt: string;
  updatedAt: string;
}

export interface OAuthProviderSummary {
  id: string;
  provider: 'Google' | 'GitHub' | 'Facebook' | 'LinkedIn';
  providerEmail: string;
  createdAt: string;
}

export interface RegisterOAuthUserRequest {
  email: string;
  firstName: string;
  lastName: string;
  professionalTitle?: string;
  bio?: string;
  location?: string;
  profileImage?: string;
  provider: number; // OAuthProviderType enum: 0=Google, 1=GitHub, 2=LinkedIn, 3=Facebook
  providerId: string;
  providerEmail: string;
  accessToken: string;
  refreshToken?: string;
  tokenExpiresAt?: string;
}

export interface CheckOAuthProviderResponse {
  exists: boolean;
  provider: OAuthProvider | null;
  user: { id: string } | null;
}
