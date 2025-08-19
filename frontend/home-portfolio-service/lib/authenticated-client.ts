import type { ServiceUserData } from '@/types/global';

// Authenticated client for handling authenticated API calls
export class AuthenticatedClient {
  private static instance: AuthenticatedClient;
  private baseUrl: string;

  private constructor() {
    const baseUrl = process.env.NEXT_PUBLIC_AUTH_USER_SERVICE;
    if (!baseUrl) {
      throw new Error('NEXT_PUBLIC_AUTH_USER_SERVICE environment variable is not set');
    }
    this.baseUrl = baseUrl;
  }

  public static getInstance(): AuthenticatedClient {
    if (!AuthenticatedClient.instance) {
      AuthenticatedClient.instance = new AuthenticatedClient();
    }
    return AuthenticatedClient.instance;
  }

  /**
   * Get the current user's authentication token from injected user data
   */
  private async getAuthToken(): Promise<string | null> {
    try {
      // Reference to the same storage used in inject/remove
      if (typeof global !== 'undefined' && global.homePortfolioServiceUserStorage) {
        const userStorage = global.homePortfolioServiceUserStorage;
        if (userStorage.size > 0) {
          const userData: ServiceUserData = Array.from(userStorage.values())[0];
          return userData.accessToken || null;
        }
      }

      // If no token found, try to get from local API (for client-side)
      try {
        const response = await fetch('/api/user/get');
        if (response.ok) {
          const userData = await response.json();
          return userData.accessToken || null;
        }
      } catch {
        // Ignore API errors, just return null
      }

      return null;
    } catch (error) {
      console.error('Error getting auth token:', error);
      return null;
    }
  }

  /**
   * Check if user is authenticated
   */
  public async isAuthenticated(): Promise<boolean> {
    const token = await this.getAuthToken();
    return !!token;
  }

  /**
   * Redirect to login page
   */
  private redirectToLogin(): void {
    window.location.href = `${this.baseUrl}/login?redirect=${encodeURIComponent(window.location.href)}`;
  }

  /**
   * Make an authenticated API request
   */
  public async authenticatedRequest<T>(
    url: string,
    options: RequestInit = {}
  ): Promise<T> {
    const token = await this.getAuthToken();
    
    if (!token) {
      this.redirectToLogin();
      throw new Error('Authentication required');
    }
    console.log('Header token:', token);
    const response = await fetch(url, {
      ...options,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`,
        ...options.headers,
      },
    });

    if (response.status === 401) {
      // Token is invalid, clear it and redirect to login
      localStorage.removeItem('auth_token');
      this.redirectToLogin();
      throw new Error('Authentication required');
    }

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`API request failed: ${response.status} ${errorText}`);
    }

    return response.json();
  }

  /**
   * Make a POST request with authentication
   */
  public async post<T>(url: string, data: unknown): Promise<T> {
    return this.authenticatedRequest<T>(url, {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  /**
   * Make a GET request with authentication
   */
  public async get<T>(url: string): Promise<T> {
    return this.authenticatedRequest<T>(url, {
      method: 'GET',
    });
  }

  /**
   * Make a PUT request with authentication
   */
  public async put<T>(url: string, data: unknown): Promise<T> {
    return this.authenticatedRequest<T>(url, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
  }

  /**
   * Make a DELETE request with authentication
   */
  public async delete<T>(url: string): Promise<T> {
    return this.authenticatedRequest<T>(url, {
      method: 'DELETE',
    });
  }
}

// Export singleton instance
export const authenticatedClient = AuthenticatedClient.getInstance(); 