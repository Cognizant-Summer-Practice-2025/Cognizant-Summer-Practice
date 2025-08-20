import type { ServiceUserData } from '@/types/global';

// Authenticated client for handling authenticated API calls using JWT stored in browser storage
export class AuthenticatedClient {
  private static instance: AuthenticatedClient;
  private authServiceBaseUrl: string;

  private constructor() {
    const baseUrl = process.env.NEXT_PUBLIC_AUTH_USER_SERVICE;
    if (!baseUrl) {
      throw new Error('NEXT_PUBLIC_AUTH_USER_SERVICE environment variable is not set');
    }
    this.authServiceBaseUrl = baseUrl;
  }

  public static getInstance(): AuthenticatedClient {
    if (!AuthenticatedClient.instance) {
      AuthenticatedClient.instance = new AuthenticatedClient();
    }
    return AuthenticatedClient.instance;
  }

  // Retrieve JWT issued by auth service from storage
  private getStoredJwt(): string | null {
    if (typeof window === 'undefined') return null;
    return (
      localStorage.getItem('jwt_auth_token') ||
      sessionStorage.getItem('jwt_auth_token') ||
      null
    );
  }

  public async isAuthenticated(): Promise<boolean> {
    return !!this.getStoredJwt();
  }

  // Do not auto-redirect to login here to avoid loops. Let callers decide.
  private buildLoginUrl(): string {
    if (typeof window === 'undefined') return `${this.authServiceBaseUrl}/login`;
    const currentUrl = window.location.href;
    return `${this.authServiceBaseUrl}/api/sso/callback?callbackUrl=${encodeURIComponent(currentUrl)}`;
  }

  public getLoginUrl(): string {
    return this.buildLoginUrl();
  }

  public async authenticatedRequest<T>(
    url: string,
    options: RequestInit = {}
  ): Promise<T> {
    const token = this.getStoredJwt();

    if (!token) {
      // No token yet; avoid redirect to prevent loops during SSO processing
      throw new Error('Authentication required (no JWT)');
    }

    const response = await fetch(url, {
      ...options,
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`,
        ...options.headers,
      },
    });

    if (response.status === 401) {
      // Token invalid/expired; let caller decide how to recover
      throw new Error('Authentication required (401)');
    }

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`API request failed: ${response.status} ${errorText}`);
    }

    return response.json();
  }

  public async post<T>(url: string, data: unknown): Promise<T> {
    return this.authenticatedRequest<T>(url, {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  public async get<T>(url: string): Promise<T> {
    return this.authenticatedRequest<T>(url, {
      method: 'GET',
    });
  }

  public async put<T>(url: string, data: unknown): Promise<T> {
    return this.authenticatedRequest<T>(url, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
  }

  public async delete<T>(url: string): Promise<T> {
    return this.authenticatedRequest<T>(url, {
      method: 'DELETE',
    });
  }
}

export const authenticatedClient = AuthenticatedClient.getInstance(); 