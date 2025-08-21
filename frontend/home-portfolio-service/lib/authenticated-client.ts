
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

  // Decode JWT payload safely (no verification, client-side only)
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
    } catch {
      return null;
    }
  }

  // Try to refresh JWT token from auth service using cookies
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
        if (typeof window !== 'undefined') {
          localStorage.setItem('jwt_auth_token', data.token);
        }
        return true;
      }
      return false;
    } catch {
      return false;
    }
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
    const jwt = this.getStoredJwt();
    if (!jwt) throw new Error('Authentication required');
    const payload = this.getJwtPayload(jwt) as { accessToken?: string } | null;
    const access = payload?.accessToken;

    const requestWithBearer = async (bearer: string): Promise<Response> => {
      return fetch(url, {
        ...options,
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${bearer}`,
          ...options.headers,
        },
      });
    };

    // Try provider access token first if available, else JWT
    let response = await requestWithBearer(access && access.length > 0 ? access : jwt);

    // If unauthorized and we tried access token, fallback to JWT once
    if (response.status === 401 && access && access.length > 0) {
      response = await requestWithBearer(jwt);
    }

    // If still unauthorized, try to refresh JWT from auth and retry once
    if (response.status === 401) {
      const refreshed = await this.refreshJwtFromAuth();
      if (refreshed) {
        const newJwt = this.getStoredJwt();
        const newPayload = newJwt ? (this.getJwtPayload(newJwt) as { accessToken?: string } | null) : null;
        const newAccess = newPayload?.accessToken;
        response = await requestWithBearer(newAccess && newAccess.length > 0 ? newAccess : (newJwt || ''));
      }
      if (response.status === 401) {
        localStorage.removeItem('jwt_auth_token');
        throw new Error('Authentication required (401)');
      }
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