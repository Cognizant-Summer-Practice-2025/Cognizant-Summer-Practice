import type { ServiceUserData } from '@/types/global';
import { Logger } from '@/lib/logger';

// Authenticated client for handling authenticated API calls
export class AuthenticatedClient {
  private static instance: AuthenticatedClient;
  private authServiceBaseUrl: string;

  private constructor() {
    this.authServiceBaseUrl = process.env.NEXT_PUBLIC_AUTH_USER_SERVICE || 'http://localhost:3000';
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
      if (typeof global !== 'undefined' && global.adminServiceUserStorage) {
        const userStorage = global.adminServiceUserStorage;
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
      Logger.error('Error getting auth token', error);
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
    window.location.href = `${this.authServiceBaseUrl}/login?redirect=${encodeURIComponent(window.location.href)}`;
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

    const response = await fetch(url, {
      ...options,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`,
        ...options.headers,
      },
    });

    if (response.status === 401) {
      // Token is invalid, redirect to login
      this.redirectToLogin();
      throw new Error('Authentication required');
    }

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`API request failed: ${response.status} ${errorText}`);
    }

    // Check if response has content before trying to parse JSON
    const contentType = response.headers.get('content-type');
    const hasContent = contentType && contentType.includes('application/json');
    
    // For responses that might be empty (like DELETE), handle appropriately
    if (!hasContent || response.status === 204) {
      return {} as T; // Return empty object for successful responses without content
    }

    const text = await response.text();
    if (!text.trim()) {
      return {} as T; // Return empty object for empty responses
    }

    try {
      return JSON.parse(text);
    } catch (error) {
      Logger.error('Failed to parse JSON response', { text, error });
      throw new Error('Invalid JSON response from server');
    }
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
  public async delete<T = void>(url: string): Promise<T> {
    return this.authenticatedRequest<T>(url, {
      method: 'DELETE',
    });
  }

  /**
   * Make a DELETE request that returns void (for simpler delete operations)
   */
  public async deleteVoid(url: string): Promise<void> {
    await this.authenticatedRequest<void>(url, {
      method: 'DELETE',
    });
  }
}

// Export singleton instance
export const authenticatedClient = AuthenticatedClient.getInstance();
