// Authenticated client for handling authenticated API calls
export class AuthenticatedClient {
  private static instance: AuthenticatedClient;
  private baseUrl: string;

  private constructor() {
    this.baseUrl = process.env.NEXT_PUBLIC_AUTH_USER_SERVICE || 'http://localhost:3000';
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
      type InjectedUserData = {
        id?: string;
        email?: string;
        accessToken?: string;
      };
      // Reference to the same storage used in inject/remove
      // On the client, read the token from injected user API
      if (typeof window !== 'undefined') {
        try {
          const response = await fetch('/api/user/get');
          if (response.ok) {
            const userData: InjectedUserData = await response.json();
            const token = userData.accessToken || null;
            if (token) {
              console.log('[AuthClient] token from /api/user/get:', `${token.slice(0, 12)}...${token.slice(-6)} (len=${token.length})`);
            } else {
              console.log('[AuthClient] no token in /api/user/get payload');
            }
            return token;
          }
          console.log('[AuthClient] /api/user/get not ok:', response.status);
        } catch {
          // ignore
        }
        return null;
      }

      // On the server (Node), read from global storage populated by injection
      const g = global as unknown as { messagesServiceUserStorage?: Map<string, InjectedUserData> };
      const userStorage: Map<string, InjectedUserData> | undefined = g.messagesServiceUserStorage;
      if (userStorage && userStorage.size > 0) {
        const userData = Array.from(userStorage.values())[0] as InjectedUserData;
        const token = userData.accessToken || null;
        if (token) {
          console.log('[AuthClient] token from global storage (server):', `${token.slice(0, 12)}...${token.slice(-6)} (len=${token.length})`);
        } else {
          console.log('[AuthClient] no token in global storage');
        }
        return token;
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
    // Intentionally no redirect. Page-level logic handles unauthenticated redirects to HOME.
  }

  /**
   * Make an authenticated API request
   */
  public async authenticatedRequest<T>(
    url: string,
    options: RequestInit = {}
  ): Promise<T> {
    const perform = async (attempt: number): Promise<Response> => {
      const token = await this.getAuthToken();
      if (!token) throw new Error('Authentication required');
      const method = (options.method || 'GET').toUpperCase();
      console.log('[AuthClient]', method, url, 'attempt', attempt, 'Header token:', `${token.slice(0, 12)}...${token.slice(-6)} (len=${token.length})`);
      return fetch(url, {
        ...options,
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
          ...options.headers,
        },
      });
    };

    // First attempt
    let response = await perform(1);

    // If unauthorized, wait briefly, refresh token and retry once
    if (response.status === 401) {
      console.warn('[AuthClient] 401 on first attempt, retrying after short wait');
      await new Promise(r => setTimeout(r, 600));
      response = await perform(2);
      if (response.status === 401) {
        localStorage.removeItem('auth_token');
        console.error('[AuthClient] 401 on second attempt, giving up');
        throw new Error('Authentication required');
      }
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
