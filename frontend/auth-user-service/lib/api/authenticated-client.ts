import { getSession } from "next-auth/react"

interface ApiClientOptions {
  method?: 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH'
  body?: unknown
  headers?: Record<string, string>
  requireAuth?: boolean  // Default true for authenticated client
}

export class AuthenticatedApiClient {
  private baseUrl: string

  constructor(baseUrl?: string) {
    this.baseUrl = baseUrl || process.env.NEXT_PUBLIC_USER_API_URL || 'http://localhost:5200'
  }

  async request<T>(
    endpoint: string,
    options: ApiClientOptions = {}
  ): Promise<T> {
    const { method = 'GET', body, headers = {}, requireAuth = true } = options
    
    const requestHeaders: Record<string, string> = {
      'Content-Type': 'application/json',
      ...headers,
    }

    // Add OAuth 2.0 Bearer token if required
    if (requireAuth) {
      const session = await getSession()
      
      if (session?.accessToken) {
        requestHeaders['Authorization'] = `Bearer ${session.accessToken}`
      } else {
        console.error('🔐 Auth: No access token available in session');
        throw new Error('No access token available. Please sign in.')
      }
          } else {
        // Authentication skipped for public endpoint
      }

    const config: RequestInit = {
      method,
      headers: requestHeaders,
    }

    if (body && method !== 'GET') {
      config.body = JSON.stringify(body)
    }

    const fullUrl = `${this.baseUrl}${endpoint}`;
    console.log('🌐 HTTP: Making request:', {
      method,
      url: fullUrl,
      hasBody: !!body,
      headers: Object.keys(requestHeaders),
      requireAuth
    });

    const response = await fetch(fullUrl, config)

    console.log('🌐 HTTP: Response received:', {
      status: response.status,
      statusText: response.statusText,
      ok: response.ok,
      url: response.url
    });

    if (!response.ok) {
      console.error('🌐 HTTP: Request failed:', {
        status: response.status,
        statusText: response.statusText,
        url: response.url
      });
      
      if (response.status === 401) {
        throw new Error('Unauthorized: Please sign in again')
      }
      if (response.status === 404) {
        throw new Error('Resource not found')
      }
      throw new Error(`API request failed: ${response.status}`)
    }

    const contentType = response.headers.get('content-type')
    if (contentType && contentType.includes('application/json')) {
      return response.json()
    }
    
    return response.text() as unknown as T
  }

  async get<T>(endpoint: string, requireAuth = true, headers?: Record<string, string>): Promise<T> {
    return this.request<T>(endpoint, { method: 'GET', requireAuth, headers })
  }

  async post<T>(endpoint: string, body?: unknown, requireAuth = true, headers?: Record<string, string>): Promise<T> {
    return this.request<T>(endpoint, { method: 'POST', body, requireAuth, headers })
  }

  async put<T>(endpoint: string, body?: unknown, requireAuth = true, headers?: Record<string, string>): Promise<T> {
    return this.request<T>(endpoint, { method: 'PUT', body, requireAuth, headers })
  }

  async delete<T>(endpoint: string, requireAuth = true, headers?: Record<string, string>): Promise<T> {
    return this.request<T>(endpoint, { method: 'DELETE', requireAuth, headers })
  }

  async patch<T>(endpoint: string, body?: unknown, requireAuth = true, headers?: Record<string, string>): Promise<T> {
    return this.request<T>(endpoint, { method: 'PATCH', body, requireAuth, headers })
  }

  // Unauthenticated methods for auth flow
  async getUnauthenticated<T>(endpoint: string, headers?: Record<string, string>): Promise<T> {
    return this.get<T>(endpoint, false, headers)
  }

  async postUnauthenticated<T>(endpoint: string, body?: unknown, headers?: Record<string, string>): Promise<T> {
    return this.post<T>(endpoint, body, false, headers)
  }

  // Static factory methods for common service configurations
  static createUserClient(): AuthenticatedApiClient {
    return new AuthenticatedApiClient(process.env.NEXT_PUBLIC_USER_API_URL || 'http://localhost:5200')
  }

  static createPortfolioClient(): AuthenticatedApiClient {
    return new AuthenticatedApiClient(process.env.NEXT_PUBLIC_PORTFOLIO_API_URL || 'http://localhost:5201')
  }

  static createMessagesClient(): AuthenticatedApiClient {
    return new AuthenticatedApiClient(process.env.NEXT_PUBLIC_MESSAGES_API_URL || 'http://localhost:5202')
  }
}