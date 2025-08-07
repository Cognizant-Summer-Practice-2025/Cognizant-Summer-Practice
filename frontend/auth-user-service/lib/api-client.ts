import { getSession } from "next-auth/react"

interface ApiClientOptions {
  method?: 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH'
  body?: unknown
  headers?: Record<string, string>
}

export class ApiClient {
  private static baseUrl = process.env.NEXT_PUBLIC_BACKEND_URL

  static async request<T>(
    endpoint: string,
    options: ApiClientOptions = {}
  ): Promise<T> {
    const { method = 'GET', body, headers = {} } = options
    
    // Get the session to access the OAuth access token
    const session = await getSession()
    
    const requestHeaders: Record<string, string> = {
      'Content-Type': 'application/json',
      ...headers,
    }

    // Add OAuth 2.0 Bearer token if available
    if (session?.accessToken) {
      requestHeaders['Authorization'] = `Bearer ${session.accessToken}`
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
        // Token expired or invalid - could trigger re-authentication
        throw new Error('Unauthorized: Please sign in again')
      }
      throw new Error(`API request failed: ${response.status}`)
    }

    return response.json()
  }

  static async get<T>(endpoint: string, headers?: Record<string, string>): Promise<T> {
    return this.request<T>(endpoint, { method: 'GET', headers })
  }

  static async post<T>(endpoint: string, body?: unknown, headers?: Record<string, string>): Promise<T> {
    return this.request<T>(endpoint, { method: 'POST', body, headers })
  }

  static async put<T>(endpoint: string, body?: unknown, headers?: Record<string, string>): Promise<T> {
    return this.request<T>(endpoint, { method: 'PUT', body, headers })
  }

  static async delete<T>(endpoint: string, headers?: Record<string, string>): Promise<T> {
    return this.request<T>(endpoint, { method: 'DELETE', headers })
  }
}