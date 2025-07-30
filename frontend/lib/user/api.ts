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

const API_BASE_URL = 'http://localhost:5200';

// Search users by username, first name, last name, or full name
export async function searchUsers(searchTerm: string): Promise<SearchUser[]> {
  try {
    const response = await fetch(`${API_BASE_URL}/api/users/search?q=${encodeURIComponent(searchTerm)}`, {
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

// Check if user exists by email
export async function checkUserExists(email: string): Promise<CheckEmailResponse> {
  try {
    const response = await fetch(`${API_BASE_URL}/api/users/check-email/${encodeURIComponent(email)}`, {
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
    console.error('Error checking user existence:', error);
    throw error;
  }
}

// Register a new user
export async function registerUser(userData: RegisterUserRequest): Promise<User> {
  try {
    const response = await fetch(`${API_BASE_URL}/api/users/register`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(userData),
    });

    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
    }

    return await response.json();
  } catch (error) {
    console.error('Error registering user:', error);
    throw error;
  }
}

// Get user by email
export async function getUserByEmail(email: string): Promise<User | null> {
  try {
    const response = await fetch(`${API_BASE_URL}/api/users/email/${encodeURIComponent(email)}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      if (response.status === 404) {
        return null;
      }
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    return await response.json();
  } catch (error) {
    console.error('Error getting user by email:', error);
    throw error;
  }
}

// Register OAuth user (creates both user and OAuth provider)
export async function registerOAuthUser(userData: RegisterOAuthUserRequest): Promise<{ user: User; oauthProvider: OAuthProvider }> {
  try {
    console.log('Sending OAuth registration request:', userData);
    
    const response = await fetch(`${API_BASE_URL}/api/users/register-oauth`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(userData),
    });

    console.log('OAuth registration response status:', response.status);

    if (!response.ok) {
      const errorData = await response.json();
      console.error('OAuth registration error details:', errorData);
      throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
    }

    return await response.json();
  } catch (error) {
    console.error('Error registering OAuth user:', error);
    throw error;
  }
}

// Check if OAuth provider exists
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
    
    const response = await fetch(`${API_BASE_URL}/api/users/oauth-providers/check?provider=${formattedProvider}&providerId=${encodeURIComponent(providerId)}`, {
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
    console.error('Error checking OAuth provider:', error);
    throw error;
  }
}

// Check if user has specific OAuth provider type
export async function checkUserOAuthProvider(userId: string, provider: 'Google' | 'GitHub' | 'Facebook' | 'LinkedIn'): Promise<{ exists: boolean; provider: OAuthProvider | null }> {
  try {
    const response = await fetch(`${API_BASE_URL}/api/users/${userId}/oauth-providers/${provider}`, {
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
    console.error('Error checking user OAuth provider:', error);
    throw error;
  }
}

// Update OAuth provider tokens
export async function updateOAuthProvider(providerId: string, updateData: {
  accessToken?: string;
  refreshToken?: string;
  tokenExpiresAt?: string;
}): Promise<OAuthProvider> {
  try {
    const response = await fetch(`${API_BASE_URL}/api/users/oauth-providers/${providerId}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(updateData),
    });

    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
    }

    return await response.json();
  } catch (error) {
    console.error('Error updating OAuth provider:', error);
    throw error;
  }
}

// Get user's OAuth providers
export async function getUserOAuthProviders(userId: string): Promise<OAuthProviderSummary[]> {
  try {
    const response = await fetch(`${API_BASE_URL}/api/users/${userId}/oauth-providers`, {
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
    console.error('Error getting user OAuth providers:', error);
    throw error;
  }
}

// Add OAuth provider to existing user
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
    const response = await fetch(`${API_BASE_URL}/api/users/oauth-providers`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(oauthData),
    });

    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
    }

    return await response.json();
  } catch (error) {
    console.error('Error adding OAuth provider:', error);
    throw error;
  }
}

// Remove OAuth provider
export async function removeOAuthProvider(providerId: string): Promise<void> {
  try {
    const response = await fetch(`${API_BASE_URL}/api/users/oauth-providers/${providerId}`, {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
  } catch (error) {
    console.error('Error removing OAuth provider:', error);
    throw error;
  }
}

// Update user data
export async function updateUser(userId: string, userData: {
  firstName?: string;
  lastName?: string;
  professionalTitle?: string;
  bio?: string;
  location?: string;
  profileImage?: string;
}): Promise<User> {
  try {
    const response = await fetch(`${API_BASE_URL}/api/users/${userId}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(userData),
    });

    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
    }

    return await response.json();
  } catch (error) {
    console.error('Error updating user:', error);
    throw error;
  }
}
