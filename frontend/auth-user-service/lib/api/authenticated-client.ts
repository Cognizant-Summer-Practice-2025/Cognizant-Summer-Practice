import { getSession } from "next-auth/react"

interface ApiClientOptions {
  method?: 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH'
  body?: unknown
  headers?: Record<string, string>
  requireAuth?: boolean  // Default true for authenticated client
}

export class AuthenticatedApiClient {
  private static baseUrl = process.env.NEXT_PUBLIC_USER_API_URL || 'http://localhost:5200'

  static async request<T>(
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
        throw new Error('No access token available. Please sign in.')
      }
    }

    const config: RequestInit = {
      method,
      headers: requestHeaders,
    }

    if (body && method !== 'GET') {
      config.body = JSON.stringify(body)
    }

    const response = await fetch(`${this.baseUrl}${endpoint}`, config)

    if (!response.ok) {
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

  static async get<T>(endpoint: string, requireAuth = true, headers?: Record<string, string>): Promise<T> {
    return this.request<T>(endpoint, { method: 'GET', requireAuth, headers })
  }

  static async post<T>(endpoint: string, body?: unknown, requireAuth = true, headers?: Record<string, string>): Promise<T> {
    return this.request<T>(endpoint, { method: 'POST', body, requireAuth, headers })
  }

  static async put<T>(endpoint: string, body?: unknown, requireAuth = true, headers?: Record<string, string>): Promise<T> {
    return this.request<T>(endpoint, { method: 'PUT', body, requireAuth, headers })
  }

  static async delete<T>(endpoint: string, requireAuth = true, headers?: Record<string, string>): Promise<T> {
    return this.request<T>(endpoint, { method: 'DELETE', requireAuth, headers })
  }

  // Unauthenticated methods for auth flow
  static async getUnauthenticated<T>(endpoint: string, headers?: Record<string, string>): Promise<T> {
    return this.get<T>(endpoint, false, headers)
  }

  static async postUnauthenticated<T>(endpoint: string, body?: unknown, headers?: Record<string, string>): Promise<T> {
    return this.post<T>(endpoint, body, false, headers)
  }
}