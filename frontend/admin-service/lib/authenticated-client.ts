import { SERVICES } from '@/lib/config/services';
import { Logger } from '@/lib/logger';

// Authenticated client for handling authenticated API calls
export class AuthenticatedClient {
  private static instance: AuthenticatedClient;
  private authServiceBaseUrl: string;

  private constructor() {
    this.authServiceBaseUrl = SERVICES.AUTH_USER_SERVICE;
  }

  public static getInstance(): AuthenticatedClient {
    if (!AuthenticatedClient.instance) {
      AuthenticatedClient.instance = new AuthenticatedClient();
    }
    return AuthenticatedClient.instance;
  }

  private getStoredJwt(): string | null {
    if (typeof window === 'undefined') return null;
    return localStorage.getItem('jwt_auth_token') || sessionStorage.getItem('jwt_auth_token') || null;
  }

  private getJwtPayload(token: string): Record<string, unknown> | null {
    try {
      const parts = token.split('.');
      if (parts.length < 2) return null;
      const base64Url = parts[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const json = typeof window !== 'undefined'
        ? decodeURIComponent(atob(base64).split('').map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)).join(''))
        : Buffer.from(base64, 'base64').toString('utf8');
      return JSON.parse(json);
    } catch { return null; }
  }

  private async refreshJwtFromAuth(): Promise<boolean> {
    try {
      const resp = await fetch(`${this.authServiceBaseUrl}/api/auth/get-jwt`, {
        method: 'GET',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
      });
      if (!resp.ok) return false;
      const data = await resp.json();
      if (data?.success && data?.token) {
        if (typeof window !== 'undefined') localStorage.setItem('jwt_auth_token', data.token);
        return true;
      }
      return false;
    } catch { return false; }
  }

  /**
   * Check if user is authenticated
   */
  public async isAuthenticated(): Promise<boolean> {
    const jwt = this.getStoredJwt();
    return !!jwt;
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
    const jwt = this.getStoredJwt();
    if (!jwt) { this.redirectToLogin(); throw new Error('Authentication required'); }
    const payload = this.getJwtPayload(jwt) as { accessToken?: string } | null;
    const access = payload?.accessToken;

    const requestWithBearer = async (bearer: string): Promise<Response> => fetch(url, {
      ...options,
      headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${bearer}`, ...options.headers },
    });

    let response = await requestWithBearer(access && access.length > 0 ? access : jwt);
    if (response.status === 401 && access && access.length > 0) {
      response = await requestWithBearer(jwt);
    }
    if (response.status === 401) {
      const refreshed = await this.refreshJwtFromAuth();
      if (refreshed) {
        const newJwt = this.getStoredJwt();
        const newPayload = newJwt ? (this.getJwtPayload(newJwt) as { accessToken?: string } | null) : null;
        const newAccess = newPayload?.accessToken;
        response = await requestWithBearer(newAccess && newAccess.length > 0 ? newAccess : (newJwt || ''));
      }
      if (response.status === 401) { if (typeof window !== 'undefined') { localStorage.removeItem('jwt_auth_token'); sessionStorage.removeItem('jwt_auth_token'); } this.redirectToLogin(); throw new Error('Authentication required'); }
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
