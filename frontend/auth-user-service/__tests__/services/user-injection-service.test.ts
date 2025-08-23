import { UserInjectionService } from '@/lib/services/user-injection-service'
import { User } from '@/lib/user/interfaces'

// Mock fetch globally
const mockFetch = global.fetch as jest.MockedFunction<typeof fetch>

describe('UserInjectionService', () => {
  beforeEach(() => {
    jest.clearAllMocks()
    process.env.NEXT_PUBLIC_AUTH_USER_SERVICE = 'http://localhost:3000'
  })

  afterEach(() => {
    delete process.env.NEXT_PUBLIC_AUTH_USER_SERVICE
  })

  describe('injectUser', () => {
    const mockUser: User = {
      id: '123',
      email: 'test@example.com',
      username: 'testuser',
      firstName: 'Test',
      lastName: 'User',
      professionalTitle: 'Developer',
      bio: 'Test bio',
      location: 'Test City',
      avatarUrl: 'https://example.com/avatar.jpg',
      isActive: true,
      isAdmin: false,
      lastLoginAt: '2023-01-01T00:00:00Z',

    }

    it('should inject user data successfully', async () => {
      const mockResponse = {
        results: {
          'messages-service': { success: true },
          'home-portfolio-service': { success: true }
        }
      }

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockResponse),
      } as Response)

      const consoleLogSpy = jest.spyOn(console, 'log').mockImplementation()

      await UserInjectionService.injectUser(mockUser, 'mock-token')

      expect(mockFetch).toHaveBeenCalledWith(
        'http://localhost:3000/api/services/user-injection',
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({
            action: 'inject',
            userData: {
              id: '123',
              email: 'test@example.com',
              username: 'testuser',
              firstName: 'Test',
              lastName: 'User',
              professionalTitle: 'Developer',
              bio: 'Test bio',
              location: 'Test City',
              profileImage: 'https://example.com/avatar.jpg',
              isActive: true,
              isAdmin: false,
              lastLoginAt: '2023-01-01T00:00:00Z',
              accessToken: 'mock-token',
            },
          }),
        }
      )

      expect(consoleLogSpy).toHaveBeenCalledWith('User injection results:', mockResponse.results)
      
      consoleLogSpy.mockRestore()
    })

    it('should inject user data without access token', async () => {
      const mockResponse = {
        results: {
          'messages-service': { success: true }
        }
      }

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockResponse),
      } as Response)

      await UserInjectionService.injectUser(mockUser)

      expect(mockFetch).toHaveBeenCalledWith(
        'http://localhost:3000/api/services/user-injection',
        expect.objectContaining({
          body: expect.stringContaining('"userData"')
        })
      )
      
      // Check that the request body contains the expected user data
      const actualCall = mockFetch.mock.calls[0]
      const body = actualCall[1]?.body
      const requestBody = typeof body === 'string' ? JSON.parse(body) : {}
      expect(requestBody.userData.accessToken).toBeUndefined()
    })

    it('should handle user with isAdmin undefined', async () => {
      const userWithoutAdmin = { ...mockUser, isAdmin: undefined } as any

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve({ results: {} }),
      } as Response)

      await UserInjectionService.injectUser(userWithoutAdmin)

      const requestBody = JSON.parse(mockFetch.mock.calls[0][1]?.body as string)
      expect(requestBody.userData.isAdmin).toBe(false)
    })

    it('should handle API errors gracefully', async () => {
      const errorResponse = { error: 'Service unavailable' }

      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 503,
        statusText: 'Service Unavailable',
        json: () => Promise.resolve(errorResponse),
      } as Response)

      const consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation()

      // Should not throw error
      await expect(UserInjectionService.injectUser(mockUser)).resolves.toBeUndefined()

      expect(consoleErrorSpy).toHaveBeenCalledWith(
        'Error injecting user across services:',
        expect.any(Error)
      )

      consoleErrorSpy.mockRestore()
    })

    it('should handle network errors gracefully', async () => {
      mockFetch.mockRejectedValueOnce(new Error('Network error'))

      const consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation()

      // Should not throw error
      await expect(UserInjectionService.injectUser(mockUser)).resolves.toBeUndefined()

      expect(consoleErrorSpy).toHaveBeenCalledWith(
        'Error injecting user across services:',
        expect.any(Error)
      )

      consoleErrorSpy.mockRestore()
    })

    it('should handle API errors with invalid JSON response', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 500,
        statusText: 'Internal Server Error',
        json: () => Promise.reject(new Error('Invalid JSON')),
      } as Response)

      const consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation()

      await UserInjectionService.injectUser(mockUser)

      expect(consoleErrorSpy).toHaveBeenCalledWith(
        'Error injecting user across services:',
        expect.any(Error)
      )

      consoleErrorSpy.mockRestore()
    })

    it('should use default URL when environment variable is not set', async () => {
      delete process.env.NEXT_PUBLIC_AUTH_USER_SERVICE

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve({ results: {} }),
      } as Response)

      await UserInjectionService.injectUser(mockUser)

      expect(mockFetch).toHaveBeenCalledWith(
        'http://localhost:3000/api/services/user-injection',
        expect.any(Object)
      )
    })
  })

  describe('removeUser', () => {
    it('should remove user data successfully', async () => {
      const mockResponse = {
        results: {
          'messages-service': { success: true },
          'home-portfolio-service': { success: true }
        }
      }

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockResponse),
      } as Response)

      const consoleLogSpy = jest.spyOn(console, 'log').mockImplementation()

      await UserInjectionService.removeUser('test@example.com')

      expect(mockFetch).toHaveBeenCalledWith(
        'http://localhost:3000/api/services/user-injection',
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({
            action: 'remove',
            userEmail: 'test@example.com',
          }),
        }
      )

      expect(consoleLogSpy).toHaveBeenCalledWith('User removal results:', mockResponse.results)
      
      consoleLogSpy.mockRestore()
    })

    it('should handle API errors gracefully', async () => {
      const errorResponse = { error: 'User not found' }

      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 404,
        statusText: 'Not Found',
        json: () => Promise.resolve(errorResponse),
      } as Response)

      const consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation()

      // Should not throw error
      await expect(UserInjectionService.removeUser('test@example.com')).resolves.toBeUndefined()

      expect(consoleErrorSpy).toHaveBeenCalledWith(
        'Error removing user across services:',
        expect.any(Error)
      )

      consoleErrorSpy.mockRestore()
    })

    it('should handle network errors gracefully', async () => {
      mockFetch.mockRejectedValueOnce(new Error('Network error'))

      const consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation()

      // Should not throw error
      await expect(UserInjectionService.removeUser('test@example.com')).resolves.toBeUndefined()

      expect(consoleErrorSpy).toHaveBeenCalledWith(
        'Error removing user across services:',
        expect.any(Error)
      )

      consoleErrorSpy.mockRestore()
    })

    it('should handle API errors with invalid JSON response', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 500,
        statusText: 'Internal Server Error',
        json: () => Promise.reject(new Error('Invalid JSON')),
      } as Response)

      const consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation()

      await UserInjectionService.removeUser('test@example.com')

      expect(consoleErrorSpy).toHaveBeenCalledWith(
        'Error removing user across services:',
        expect.any(Error)
      )

      consoleErrorSpy.mockRestore()
    })
  })
})
