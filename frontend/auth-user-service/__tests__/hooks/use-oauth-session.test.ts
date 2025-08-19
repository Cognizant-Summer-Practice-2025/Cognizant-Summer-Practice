import { renderHook, waitFor } from '@testing-library/react'
import { useSession } from 'next-auth/react'
import { useOAuthSession } from '@/hooks/use-oauth-session'

jest.mock('next-auth/react')

const mockUseSession = useSession as jest.MockedFunction<typeof useSession>

// Mock fetch globally
const mockFetch = global.fetch as jest.MockedFunction<typeof fetch>

describe('useOAuthSession', () => {
  beforeEach(() => {
    jest.clearAllMocks()
    process.env.NEXT_PUBLIC_BACKEND_URL = 'http://localhost:5000'
  })

  afterEach(() => {
    delete process.env.NEXT_PUBLIC_BACKEND_URL
  })

  it('should return initial state when not authenticated', () => {
    mockUseSession.mockReturnValue({
      data: null,
      status: 'unauthenticated',
    })

    const { result } = renderHook(() => useOAuthSession())

    expect(result.current.user).toBeNull()
    expect(result.current.accessToken).toBeNull()
    expect(result.current.isLoading).toBe(false)
    expect(result.current.isAuthenticated).toBe(false)
  })

  it('should show loading state when session is loading', () => {
    mockUseSession.mockReturnValue({
      data: null,
      status: 'loading',
    })

    const { result } = renderHook(() => useOAuthSession())

    expect(result.current.isLoading).toBe(true)
    expect(result.current.isAuthenticated).toBe(false)
  })

  it('should validate token and set user when access token is available', async () => {
    const mockSession = {
      accessToken: 'mock-access-token',
      user: { email: 'test@example.com' }
    }

    const mockUserData = {
      userId: '123',
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
      lastLoginAt: '2023-01-01T00:00:00Z'
    }

    mockUseSession.mockReturnValue({
      data: mockSession,
      status: 'authenticated',
    })

    mockFetch.mockResolvedValueOnce({
      ok: true,
      json: () => Promise.resolve(mockUserData),
    } as Response)

    const { result } = renderHook(() => useOAuthSession())

    await waitFor(() => {
      expect(result.current.isAuthenticated).toBe(true)
    })

    expect(result.current.user).toEqual({
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
      lastLoginAt: '2023-01-01T00:00:00Z'
    })
    expect(result.current.accessToken).toBe('mock-access-token')
  })

  it('should handle failed token validation', async () => {
    const mockSession = {
      accessToken: 'invalid-token',
      user: { email: 'test@example.com' }
    }

    mockUseSession.mockReturnValue({
      data: mockSession,
      status: 'authenticated',
    })

    mockFetch.mockResolvedValueOnce({
      ok: false,
      status: 401,
    } as Response)

    const { result } = renderHook(() => useOAuthSession())

    await waitFor(() => {
      expect(result.current.user).toBeNull()
    })

    expect(result.current.isAuthenticated).toBe(false)
  })

  it('should handle network errors during token validation', async () => {
    const mockSession = {
      accessToken: 'mock-access-token',
      user: { email: 'test@example.com' }
    }

    mockUseSession.mockReturnValue({
      data: mockSession,
      status: 'authenticated',
    })

    const consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation()
    mockFetch.mockRejectedValueOnce(new Error('Network error'))

    const { result } = renderHook(() => useOAuthSession())

    await waitFor(() => {
      expect(result.current.user).toBeNull()
    })

    expect(result.current.isAuthenticated).toBe(false)
    expect(consoleErrorSpy).toHaveBeenCalledWith('Error validating token:', expect.any(Error))
    
    consoleErrorSpy.mockRestore()
  })

  it('should manually validate token when validateToken is called', async () => {
    const mockSession = {
      accessToken: 'mock-access-token',
      user: { email: 'test@example.com' }
    }

    const mockUserData = {
      userId: '123',
      email: 'test@example.com',
      username: 'testuser',
      isActive: true,
      isAdmin: false
    }

    mockUseSession.mockReturnValue({
      data: mockSession,
      status: 'authenticated',
    })

    mockFetch.mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(mockUserData),
    } as Response)

    const { result } = renderHook(() => useOAuthSession())

    const isValid = await result.current.validateToken()

    expect(isValid).toBe(true)
    expect(mockFetch).toHaveBeenCalledWith(
      'http://localhost:5000/api/oauth/me',
      {
        headers: {
          'Authorization': 'Bearer mock-access-token',
          'Content-Type': 'application/json',
        },
      }
    )
  })

  it('should return false when validateToken is called without access token', async () => {
    mockUseSession.mockReturnValue({
      data: null,
      status: 'unauthenticated',
    })

    const { result } = renderHook(() => useOAuthSession())

    const isValid = await result.current.validateToken()

    expect(isValid).toBe(false)
    expect(mockFetch).not.toHaveBeenCalled()
  })

  it('should show loading state during validation', async () => {
    const mockSession = {
      accessToken: 'mock-access-token',
      user: { email: 'test@example.com' }
    }

    mockUseSession.mockReturnValue({
      data: mockSession,
      status: 'authenticated',
    })

    mockFetch.mockImplementation(() => new Promise((resolve) => {
      setTimeout(() => resolve({
        ok: true,
        json: () => Promise.resolve({ userId: '123', email: 'test@example.com' }),
      } as Response), 100)
    }))

    const { result } = renderHook(() => useOAuthSession())

    // Initially should be validating
    expect(result.current.isLoading).toBe(true)

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false)
    })
  })

  it('should reset user when access token is removed', async () => {
    const mockSession = {
      accessToken: 'mock-access-token',
      user: { email: 'test@example.com' }
    }

    mockUseSession.mockReturnValue({
      data: mockSession,
      status: 'authenticated',
    })

    mockFetch.mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({
        userId: '123',
        email: 'test@example.com',
        username: 'testuser',
        isActive: true,
        isAdmin: false
      }),
    } as Response)

    const { result, rerender } = renderHook(() => useOAuthSession())

    await waitFor(() => {
      expect(result.current.user).toBeTruthy()
    })

    // Remove access token
    mockUseSession.mockReturnValue({
      data: null,
      status: 'unauthenticated',
    })

    rerender()

    expect(result.current.user).toBeNull()
    expect(result.current.isAuthenticated).toBe(false)
  })
})
