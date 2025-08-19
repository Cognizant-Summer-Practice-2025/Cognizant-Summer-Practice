import { ApiClient } from '@/lib/api-client'
import { getSession } from 'next-auth/react'

jest.mock('next-auth/react')

const mockGetSession = getSession as jest.MockedFunction<typeof getSession>
const mockFetch = global.fetch as jest.MockedFunction<typeof fetch>

describe('ApiClient', () => {
  beforeEach(() => {
    jest.clearAllMocks()
    process.env.NEXT_PUBLIC_BACKEND_URL = 'http://localhost:5000'
  })

  afterEach(() => {
    delete process.env.NEXT_PUBLIC_BACKEND_URL
  })

  describe('request', () => {
    it('should make a GET request without session', async () => {
      mockGetSession.mockResolvedValue(null)
      
      const mockResponse = { data: 'test' }
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockResponse),
      } as Response)

      const result = await ApiClient.request('/test')

      expect(mockFetch).toHaveBeenCalledWith(
        'http://localhost:5000/test',
        {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json',
          },
        }
      )

      expect(result).toEqual(mockResponse)
    })

    it('should make a GET request with session access token', async () => {
      const mockSession = {
        accessToken: 'mock-access-token',
        user: { email: 'test@example.com' }
      }

      mockGetSession.mockResolvedValue(mockSession)
      
      const mockResponse = { data: 'test' }
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockResponse),
      } as Response)

      const result = await ApiClient.request('/test')

      expect(mockFetch).toHaveBeenCalledWith(
        'http://localhost:5000/test',
        {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer mock-access-token',
          },
        }
      )

      expect(result).toEqual(mockResponse)
    })

    it('should make a POST request with body', async () => {
      mockGetSession.mockResolvedValue(null)
      
      const requestBody = { name: 'test' }
      const mockResponse = { id: '123' }
      
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockResponse),
      } as Response)

      const result = await ApiClient.request('/test', {
        method: 'POST',
        body: requestBody,
      })

      expect(mockFetch).toHaveBeenCalledWith(
        'http://localhost:5000/test',
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(requestBody),
        }
      )

      expect(result).toEqual(mockResponse)
    })

    it('should include custom headers', async () => {
      mockGetSession.mockResolvedValue(null)
      
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve({}),
      } as Response)

      await ApiClient.request('/test', {
        headers: {
          'Custom-Header': 'custom-value',
        },
      })

      expect(mockFetch).toHaveBeenCalledWith(
        'http://localhost:5000/test',
        {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json',
            'Custom-Header': 'custom-value',
          },
        }
      )
    })

    it('should not include body for GET requests', async () => {
      mockGetSession.mockResolvedValue(null)
      
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve({}),
      } as Response)

      await ApiClient.request('/test', {
        method: 'GET',
        body: { data: 'should not be included' },
      })

      const fetchCall = mockFetch.mock.calls[0]
      expect(fetchCall[1]).not.toHaveProperty('body')
    })

    it('should throw error for 401 unauthorized', async () => {
      mockGetSession.mockResolvedValue(null)
      
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 401,
      } as Response)

      await expect(ApiClient.request('/test')).rejects.toThrow(
        'Unauthorized: Please sign in again'
      )
    })

    it('should throw error for other HTTP errors', async () => {
      mockGetSession.mockResolvedValue(null)
      
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 500,
      } as Response)

      await expect(ApiClient.request('/test')).rejects.toThrow(
        'API request failed: 500'
      )
    })

    it('should handle network errors', async () => {
      mockGetSession.mockResolvedValue(null)
      mockFetch.mockRejectedValueOnce(new Error('Network error'))

      await expect(ApiClient.request('/test')).rejects.toThrow('Network error')
    })

    it('should merge authorization header with custom headers', async () => {
      const mockSession = {
        accessToken: 'mock-access-token',
        user: { email: 'test@example.com' }
      }

      mockGetSession.mockResolvedValue(mockSession)
      
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve({}),
      } as Response)

      await ApiClient.request('/test', {
        headers: {
          'Custom-Header': 'custom-value',
        },
      })

      expect(mockFetch).toHaveBeenCalledWith(
        'http://localhost:5000/test',
        {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer mock-access-token',
            'Custom-Header': 'custom-value',
          },
        }
      )
    })
  })

  describe('convenience methods', () => {
    beforeEach(() => {
      mockGetSession.mockResolvedValue(null)
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve({ success: true }),
      } as Response)
    })

    it('should make GET request using get method', async () => {
      await ApiClient.get('/test')

      expect(mockFetch).toHaveBeenCalledWith(
        'http://localhost:5000/test',
        expect.objectContaining({
          method: 'GET',
        })
      )
    })

    it('should make POST request using post method', async () => {
      const body = { data: 'test' }
      await ApiClient.post('/test', body)

      expect(mockFetch).toHaveBeenCalledWith(
        'http://localhost:5000/test',
        expect.objectContaining({
          method: 'POST',
          body: JSON.stringify(body),
        })
      )
    })

    it('should make PUT request using put method', async () => {
      const body = { data: 'test' }
      await ApiClient.put('/test', body)

      expect(mockFetch).toHaveBeenCalledWith(
        'http://localhost:5000/test',
        expect.objectContaining({
          method: 'PUT',
          body: JSON.stringify(body),
        })
      )
    })

    it('should make DELETE request using delete method', async () => {
      await ApiClient.delete('/test')

      expect(mockFetch).toHaveBeenCalledWith(
        'http://localhost:5000/test',
        expect.objectContaining({
          method: 'DELETE',
        })
      )
    })

    it('should include custom headers in convenience methods', async () => {
      const customHeaders = { 'Custom-Header': 'value' }
      
      await ApiClient.get('/test', customHeaders)

      expect(mockFetch).toHaveBeenCalledWith(
        'http://localhost:5000/test',
        expect.objectContaining({
          headers: expect.objectContaining(customHeaders),
        })
      )
    })
  })

  describe('error handling', () => {
    it('should handle getSession errors', async () => {
      mockGetSession.mockRejectedValue(new Error('Session error'))
      
      await expect(ApiClient.request('/test')).rejects.toThrow('Session error')
    })

    it('should handle JSON parsing errors', async () => {
      mockGetSession.mockResolvedValue(null)
      
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.reject(new Error('Invalid JSON')),
      } as Response)

      await expect(ApiClient.request('/test')).rejects.toThrow('Invalid JSON')
    })
  })

  describe('configuration', () => {
    it('should use undefined base URL when environment variable is not set', async () => {
      delete process.env.NEXT_PUBLIC_BACKEND_URL
      
      mockGetSession.mockResolvedValue(null)
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve({}),
      } as Response)

      await ApiClient.request('/test')

      expect(mockFetch).toHaveBeenCalledWith(
        'undefined/test',
        expect.any(Object)
      )
    })
  })
})
