import {
  User,
  CheckEmailResponse,
  RegisterUserRequest,
  OAuthProvider,
  OAuthProviderSummary,
  RegisterOAuthUserRequest,
  CheckOAuthProviderResponse,
  SearchUser
} from './interfaces';
import { AuthenticatedApiClient } from '../api/authenticated-client';

const API_BASE_URL = process.env.NEXT_PUBLIC_USER_API_URL || 'http://localhost:5200';
const MESSAGES_API_BASE_URL = 'http://localhost:5093';

// Search users by username, first name, last name, or full name
export async function searchUsers(searchTerm: string): Promise<SearchUser[]> {
  try {
    // Note: This calls the messages service, not our user service
    const response = await fetch(`${MESSAGES_API_BASE_URL}/api/users/search?q=${encodeURIComponent(searchTerm)}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    return await response.json();
  } catch (error) {
    console.error('Error searching users:', error);
    throw error;
  }
}

// Backward compatibility alias
export const searchUsersByUsername = searchUsers;

// Check if user exists by email (used during auth flow - no authentication required)
export async function checkUserExists(email: string): Promise<CheckEmailResponse> {
  try {
    return await AuthenticatedApiClient.getUnauthenticated<CheckEmailResponse>(
      `/api/users/check-email/${encodeURIComponent(email)}`
    );
  } catch (error) {
    console.error('Error checking user existence:', error);
    throw error;
  }
}

// Register a new user (no authentication required during registration)
export async function registerUser(userData: RegisterUserRequest): Promise<User> {
  try {
    return await AuthenticatedApiClient.postUnauthenticated<User>(
      '/api/users/register',
      userData
    );
  } catch (error) {
    console.error('Error registering user:', error);
    throw error;
  }
}

// Get user by email (used during auth flow - no authentication required)
export async function getUserByEmail(email: string): Promise<User | null> {
  try {
    return await AuthenticatedApiClient.getUnauthenticated<User>(
      `/api/users/email/${encodeURIComponent(email)}`
    );
  } catch (error) {
    if (error instanceof Error && error.message.includes('Resource not found')) {
      return null;
    }
    console.error('Error getting user by email:', error);
    throw error;
  }
}

// Get user by email (authenticated version for regular app usage)
export async function getUserByEmailAuthenticated(email: string): Promise<User | null> {
  try {
    return await AuthenticatedApiClient.get<User>(
      `/api/users/email/${encodeURIComponent(email)}`
    );
  } catch (error) {
    if (error instanceof Error && error.message.includes('Resource not found')) {
      return null;
    }
    console.error('Error getting user by email (authenticated):', error);
    throw error;
  }
}

// Register OAuth user (creates both user and OAuth provider - no authentication required during registration)
export async function registerOAuthUser(userData: RegisterOAuthUserRequest): Promise<{ user: User; oauthProvider: OAuthProvider }> {
  try {
    console.log('Sending OAuth registration request:', userData);
    
    const result = await AuthenticatedApiClient.postUnauthenticated<{ user: User; oauthProvider: OAuthProvider }>(
      '/api/users/register-oauth',
      userData
    );

    console.log('OAuth registration successful:', result);
    return result;
  } catch (error) {
    console.error('Error registering OAuth user:', error);
    throw error;
  }
}

// Check if OAuth provider exists (used during auth flow - no authentication required)
export async function checkOAuthProvider(provider: string, providerId: string): Promise<CheckOAuthProviderResponse> {
  try {
    // Convert provider string to proper case expected by backend enum
    const providerMapping: { [key: string]: string } = {
      'google': 'Google',
      'github': 'GitHub',
      'facebook': 'Facebook',
      'linkedin': 'LinkedIn'
    };
    
    const formattedProvider = providerMapping[provider.toLowerCase()] || provider;
    
    return await AuthenticatedApiClient.getUnauthenticated<CheckOAuthProviderResponse>(
      `/api/users/oauth-providers/check?provider=${formattedProvider}&providerId=${encodeURIComponent(providerId)}`
    );
  } catch (error) {
    console.error('Error checking OAuth provider:', error);
    throw error;
  }
}

// Check if user has specific OAuth provider type (requires authentication)
export async function checkUserOAuthProvider(userId: string, provider: 'Google' | 'GitHub' | 'Facebook' | 'LinkedIn'): Promise<{ exists: boolean; provider: OAuthProvider | null }> {
  try {
    return await AuthenticatedApiClient.get<{ exists: boolean; provider: OAuthProvider | null }>(
      `/api/users/${userId}/oauth-providers/${provider}`
    );
  } catch (error) {
    console.error('Error checking user OAuth provider:', error);
    throw error;
  }
}

// Update OAuth provider tokens (requires authentication)
export async function updateOAuthProvider(providerId: string, updateData: {
  accessToken?: string;
  refreshToken?: string;
  tokenExpiresAt?: string;
}): Promise<OAuthProvider> {
  try {
    return await AuthenticatedApiClient.put<OAuthProvider>(
      `/api/users/oauth-providers/${providerId}`,
      updateData
    );
  } catch (error) {
    console.error('Error updating OAuth provider:', error);
    throw error;
  }
}

// Get user's OAuth providers (requires authentication)
export async function getUserOAuthProviders(userId: string): Promise<OAuthProviderSummary[]> {
  try {
    return await AuthenticatedApiClient.get<OAuthProviderSummary[]>(
      `/api/users/${userId}/oauth-providers`
    );
  } catch (error) {
    console.error('Error getting user OAuth providers:', error);
    throw error;
  }
}

// Add OAuth provider to existing user (used during auth flow - no authentication required)
export async function addOAuthProvider(oauthData: {
  userId: string;
  provider: 'Google' | 'GitHub' | 'Facebook' | 'LinkedIn';
  providerId: string;
  providerEmail: string;
  accessToken: string;
  refreshToken?: string;
  tokenExpiresAt?: string;
}): Promise<OAuthProvider> {
  try {
    return await AuthenticatedApiClient.postUnauthenticated<OAuthProvider>(
      '/api/users/oauth-providers',
      oauthData
    );
  } catch (error) {
    console.error('Error adding OAuth provider:', error);
    throw error;
  }
}

// Remove OAuth provider (requires authentication)
export async function removeOAuthProvider(providerId: string): Promise<void> {
  try {
    await AuthenticatedApiClient.delete<void>(
      `/api/users/oauth-providers/${providerId}`
    );
  } catch (error) {
    console.error('Error removing OAuth provider:', error);
    throw error;
  }
}

// Update user data (requires authentication)
export async function updateUser(userId: string, userData: {
  firstName?: string;
  lastName?: string;
  professionalTitle?: string;
  bio?: string;
  location?: string;
  profileImage?: string;
}): Promise<User> {
  try {
    return await AuthenticatedApiClient.put<User>(
      `/api/users/${userId}`,
      userData
    );
  } catch (error) {
    console.error('Error updating user:', error);
    throw error;
  }
}
