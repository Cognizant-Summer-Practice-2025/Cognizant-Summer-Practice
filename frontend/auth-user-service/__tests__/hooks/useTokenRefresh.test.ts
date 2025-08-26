import { renderHook, waitFor } from '@testing-library/react'
import { useSession, signIn } from 'next-auth/react'
import { useTokenRefresh, useTokenStatus } from '@/hooks/useTokenRefresh'

jest.mock('next-auth/react')

const mockUseSession = useSession as jest.MockedFunction<typeof useSession>
const mockSignIn = signIn as jest.MockedFunction<typeof signIn>
const mockFetch = global.fetch as jest.MockedFunction<typeof fetch>

describe('useTokenRefresh', () => {
  beforeEach(() => {
    jest.clearAllMocks()
  })

  it('should return session data and status', () => {
    const mockSession = {
      user: { email: 'test@example.com' },
      accessToken: 'mock-token',
      expires: '2024-12-31T23:59:59.999Z'
    }

    mockUseSession.mockReturnValue({
      data: mockSession,
      status: 'authenticated',
      update: jest.fn(),
    })

    const { result } = renderHook(() => useTokenRefresh())

    expect(result.current.session).toBe(mockSession)
    expect(result.current.status).toBe('authenticated')
    expect(result.current.isTokenExpired).toBe(false)
    expect(result.current.needsReauth).toBe(false)
  })

  it('should detect token expiration and trigger re-authentication', async () => {
    const mockSession = {
      user: { email: 'test@example.com' },
      accessToken: 'expired-token',
      error: 'RefreshAccessTokenError',
      expires: '2024-12-31T23:59:59.999Z'
    }

    const consoleWarnSpy = jest.spyOn(console, 'warn').mockImplementation()

    mockUseSession.mockReturnValue({
      data: mockSession,
      status: 'authenticated',
    })

    renderHook(() => useTokenRefresh())

    await waitFor(() => {
      expect(mockSignIn).toHaveBeenCalled()
    })

    expect(consoleWarnSpy).toHaveBeenCalledWith('Token refresh failed, prompting for re-authentication')
    
    consoleWarnSpy.mockRestore()
  })

  it('should return correct token expiration status', () => {
    const mockSession = {
      user: { email: 'test@example.com' },
      accessToken: 'expired-token',
      error: 'RefreshAccessTokenError' as const,
      expires: '2024-12-31T23:59:59.999Z'
    }

    mockUseSession.mockReturnValue({
      data: mockSession,
      status: 'authenticated',
    })

    const { result } = renderHook(() => useTokenRefresh())

    expect(result.current.isTokenExpired).toBe(true)
    expect(result.current.needsReauth).toBe(true)
  })

  it('should not trigger sign in when not authenticated', () => {
    mockUseSession.mockReturnValue({
      data: null,
      status: 'unauthenticated',
    })

    renderHook(() => useTokenRefresh())

    expect(mockSignIn).not.toHaveBeenCalled()
  })

  it('should not trigger sign in when there is no error', () => {
    const mockSession = {
      user: { email: 'test@example.com' },
      accessToken: 'valid-token'
    }

    mockUseSession.mockReturnValue({
      data: mockSession,
      status: 'authenticated',
    })

    renderHook(() => useTokenRefresh())

    expect(mockSignIn).not.toHaveBeenCalled()
  })
})

describe('useTokenStatus', () => {
  beforeEach(() => {
    jest.clearAllMocks()
    process.env.NEXT_PUBLIC_USER_API_URL = 'http://localhost:5200'
  })

  afterEach(() => {
    delete process.env.NEXT_PUBLIC_USER_API_URL
  })

  it('should check token status successfully', async () => {
    const mockSession = {
      accessToken: 'mock-token',
      user: { email: 'test@example.com' }
    }

    const mockTokenStatus = {
      valid: true,
      expiresAt: '2024-01-01T00:00:00Z',
      providers: [
        { name: 'github', hasRefreshToken: true, supportsRefresh: true }
      ]
    }

    mockUseSession.mockReturnValue({
      data: mockSession,
      status: 'authenticated',
    })

    mockFetch.mockResolvedValueOnce({
      ok: true,
      json: () => Promise.resolve(mockTokenStatus),
    } as Response)

    const { result } = renderHook(() => useTokenStatus())

    const status = await result.current.checkTokenStatus()

    expect(status).toEqual(mockTokenStatus)
    expect(mockFetch).toHaveBeenCalledWith(
      'http://localhost:5200/api/oauth2/token-status',
      {
        method: 'GET',
        headers: {
          'Authorization': 'Bearer mock-token',
          'Content-Type': 'application/json',
        },
      }
    )
  })

  it('should return null when no access token is available', async () => {
    mockUseSession.mockReturnValue({
      data: null,
      status: 'unauthenticated',
    })

    const { result } = renderHook(() => useTokenStatus())

    const status = await result.current.checkTokenStatus()

    expect(status).toBeNull()
    expect(mockFetch).not.toHaveBeenCalled()
  })

  it('should handle failed token status check', async () => {
    const mockSession = {
      accessToken: 'mock-token',
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

    const { result } = renderHook(() => useTokenStatus())

    const status = await result.current.checkTokenStatus()

    expect(status).toBeNull()
  })

  it('should handle network errors during token status check', async () => {
    const mockSession = {
      accessToken: 'mock-token',
      user: { email: 'test@example.com' }
    }

    mockUseSession.mockReturnValue({
      data: mockSession,
      status: 'authenticated',
    })

    const consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation()
    mockFetch.mockRejectedValueOnce(new Error('Network error'))

    const { result } = renderHook(() => useTokenStatus())

    const status = await result.current.checkTokenStatus()

    expect(status).toBeNull()
    expect(consoleErrorSpy).toHaveBeenCalledWith('Error checking token status:', expect.any(Error))
    
    consoleErrorSpy.mockRestore()
  })

  it('should perform manual refresh successfully', async () => {
    const mockSession = {
      accessToken: 'mock-token',
      user: { email: 'test@example.com' }
    }

    const mockTokenStatus = {
      providers: [
        { name: 'github', hasRefreshToken: true, supportsRefresh: true }
      ]
    }

    mockUseSession.mockReturnValue({
      data: mockSession,
      status: 'authenticated',
    })

    mockFetch.mockResolvedValueOnce({
      ok: true,
      json: () => Promise.resolve(mockTokenStatus),
    } as Response)

    mockSignIn.mockResolvedValueOnce(undefined)

    const { result } = renderHook(() => useTokenStatus())

    const success = await result.current.manualRefresh()

    expect(success).toBe(true)
    expect(mockSignIn).toHaveBeenCalled()
  })

  it('should fail manual refresh when no access token', async () => {
    mockUseSession.mockReturnValue({
      data: null,
      status: 'unauthenticated',
    })

    const consoleWarnSpy = jest.spyOn(console, 'warn').mockImplementation()

    const { result } = renderHook(() => useTokenStatus())

    const success = await result.current.manualRefresh()

    expect(success).toBe(false)
    expect(consoleWarnSpy).toHaveBeenCalledWith('No access token available for manual refresh')
    
    consoleWarnSpy.mockRestore()
  })

  it('should fail manual refresh when no refresh token available', async () => {
    const mockSession = {
      accessToken: 'mock-token',
      user: { email: 'test@example.com' }
    }

    const mockTokenStatus = {
      providers: [
        { name: 'github', hasRefreshToken: false, supportsRefresh: false }
      ]
    }

    mockUseSession.mockReturnValue({
      data: mockSession,
      status: 'authenticated',
    })

    mockFetch.mockResolvedValueOnce({
      ok: true,
      json: () => Promise.resolve(mockTokenStatus),
    } as Response)

    const consoleWarnSpy = jest.spyOn(console, 'warn').mockImplementation()

    const { result } = renderHook(() => useTokenStatus())

    const success = await result.current.manualRefresh()

    expect(success).toBe(false)
    expect(consoleWarnSpy).toHaveBeenCalledWith('No refresh token available or provider does not support refresh')
    
    consoleWarnSpy.mockRestore()
  })

  it('should handle manual refresh errors', async () => {
    const mockSession = {
      accessToken: 'mock-token',
      user: { email: 'test@example.com' }
    }

    const mockTokenStatus = {
      providers: [
        { name: 'github', hasRefreshToken: true, supportsRefresh: true }
      ]
    }

    mockUseSession.mockReturnValue({
      data: mockSession,
      status: 'authenticated',
    })

    mockFetch.mockResolvedValueOnce({
      ok: true,
      json: () => Promise.resolve(mockTokenStatus),
    } as Response)

    const consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation()
    mockSignIn.mockRejectedValueOnce(new Error('Sign in failed'))

    const { result } = renderHook(() => useTokenStatus())

    const success = await result.current.manualRefresh()

    expect(success).toBe(false)
    expect(consoleErrorSpy).toHaveBeenCalledWith('Error manually refreshing token:', expect.any(Error))
    
    consoleErrorSpy.mockRestore()
  })
})
