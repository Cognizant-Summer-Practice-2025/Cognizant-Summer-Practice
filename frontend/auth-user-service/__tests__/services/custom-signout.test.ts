import { signOut } from 'next-auth/react'
import { customSignOut } from '@/lib/auth/custom-signout'
import { triggerCrossServiceLogout } from '@/lib/auth/sso-auth'

jest.mock('next-auth/react')
jest.mock('@/lib/auth/sso-auth')

const mockSignOut = signOut as jest.MockedFunction<typeof signOut>
const mockTriggerCrossServiceLogout = triggerCrossServiceLogout as jest.MockedFunction<typeof triggerCrossServiceLogout>
const mockFetch = global.fetch as jest.MockedFunction<typeof fetch>

describe('customSignOut', () => {
  beforeEach(() => {
    jest.clearAllMocks()
    process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE = 'http://localhost:3001'
  })

  afterEach(() => {
    delete process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE
  })

  it('should perform complete sign-out flow successfully', async () => {
    mockTriggerCrossServiceLogout.mockImplementation(() => {})
    mockFetch.mockResolvedValueOnce({
      ok: true,
      json: () => Promise.resolve({ success: true }),
    } as Response)
    mockSignOut.mockResolvedValue(undefined)

    const consoleLogSpy = jest.spyOn(console, 'log').mockImplementation()

    await customSignOut()

    expect(mockTriggerCrossServiceLogout).toHaveBeenCalled()
    expect(mockFetch).toHaveBeenCalledWith('/api/auth/signout-all', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
    })
    expect(consoleLogSpy).toHaveBeenCalledWith('Successfully removed user data from all services')
    expect(mockSignOut).toHaveBeenCalledWith({
      callbackUrl: 'http://localhost:3001',
      redirect: true,
    })

    consoleLogSpy.mockRestore()
  })

  it('should continue with sign-out even if service cleanup fails', async () => {
    mockTriggerCrossServiceLogout.mockImplementation(() => {})
    mockFetch.mockResolvedValueOnce({
      ok: false,
      status: 500,
      statusText: 'Internal Server Error',
    } as Response)
    mockSignOut.mockResolvedValue(undefined)

    const consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation()

    await customSignOut()

    expect(mockTriggerCrossServiceLogout).toHaveBeenCalled()
    expect(consoleErrorSpy).toHaveBeenCalledWith('Failed to remove user data from all services')
    expect(mockSignOut).toHaveBeenCalledWith({
      callbackUrl: 'http://localhost:3001',
      redirect: true,
    })

    consoleErrorSpy.mockRestore()
  })

  it('should handle network errors during service cleanup', async () => {
    mockTriggerCrossServiceLogout.mockImplementation(() => {})
    mockFetch.mockRejectedValueOnce(new Error('Network error'))
    mockSignOut.mockResolvedValue(undefined)

    const consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation()

    await customSignOut()

    expect(mockTriggerCrossServiceLogout).toHaveBeenCalled()
    expect(consoleErrorSpy).toHaveBeenCalledWith('Error during sign-out:', expect.any(Error))
    expect(mockSignOut).toHaveBeenCalledWith({
      callbackUrl: 'http://localhost:3001',
      redirect: true,
    })

    consoleErrorSpy.mockRestore()
  })

  it('should fall back to regular sign-out if cross-service logout fails', async () => {
    mockTriggerCrossServiceLogout.mockImplementation(() => {
      throw new Error('Cross-service logout failed')
    })
    mockSignOut.mockResolvedValue(undefined)

    const consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation()

    await customSignOut()

    expect(consoleErrorSpy).toHaveBeenCalledWith('Error during sign-out:', expect.any(Error))
    expect(mockSignOut).toHaveBeenCalledWith({
      callbackUrl: 'http://localhost:3001',
      redirect: true,
    })

    consoleErrorSpy.mockRestore()
  })

  it('should handle NextAuth sign-out errors', async () => {
    mockTriggerCrossServiceLogout.mockImplementation(() => {})
    mockFetch.mockResolvedValueOnce({
      ok: true,
      json: () => Promise.resolve({ success: true }),
    } as Response)
    mockSignOut.mockRejectedValue(new Error('SignOut failed'))

    const consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation()

    await expect(customSignOut()).rejects.toThrow('SignOut failed')

    expect(mockTriggerCrossServiceLogout).toHaveBeenCalled()
    expect(mockFetch).toHaveBeenCalled()

    consoleErrorSpy.mockRestore()
  })

  it('should use default URL when environment variable is not set', async () => {
    delete process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE

    mockTriggerCrossServiceLogout.mockImplementation(() => {})
    mockFetch.mockResolvedValueOnce({
      ok: true,
      json: () => Promise.resolve({ success: true }),
    } as Response)
    mockSignOut.mockResolvedValue(undefined)

    await customSignOut()

    expect(mockSignOut).toHaveBeenCalledWith({
      callbackUrl: 'http://localhost:3001',
      redirect: true,
    })
  })

  it('should handle fetch errors during service cleanup and still continue', async () => {
    mockTriggerCrossServiceLogout.mockImplementation(() => {})
    
    // Mock fetch to throw an error
    mockFetch.mockImplementationOnce(() => {
      throw new Error('Fetch failed')
    })
    
    mockSignOut.mockResolvedValue(undefined)

    const consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation()

    await customSignOut()

    expect(mockTriggerCrossServiceLogout).toHaveBeenCalled()
    expect(consoleErrorSpy).toHaveBeenCalledWith('Error during sign-out:', expect.any(Error))
    
    // Should still call NextAuth signOut
    expect(mockSignOut).toHaveBeenCalledWith({
      callbackUrl: 'http://localhost:3001',
      redirect: true,
    })

    consoleErrorSpy.mockRestore()
  })

  it('should handle JSON parsing errors during service cleanup', async () => {
    mockTriggerCrossServiceLogout.mockImplementation(() => {})
    mockFetch.mockResolvedValueOnce({
      ok: true,
      json: () => Promise.reject(new Error('Invalid JSON')),
    } as Response)
    mockSignOut.mockResolvedValue(undefined)

    const consoleLogSpy = jest.spyOn(console, 'log').mockImplementation()

    await customSignOut()

    expect(mockTriggerCrossServiceLogout).toHaveBeenCalled()
    expect(mockFetch).toHaveBeenCalled()
    
    // Should still consider it successful since response.ok was true
    expect(consoleLogSpy).toHaveBeenCalledWith('Successfully removed user data from all services')
    expect(mockSignOut).toHaveBeenCalled()

    consoleLogSpy.mockRestore()
  })

  it('should execute sign-out steps in correct order', async () => {
    const executionOrder: string[] = []

    mockTriggerCrossServiceLogout.mockImplementation(() => {
      executionOrder.push('triggerCrossServiceLogout')
    })

    mockFetch.mockImplementation(async () => {
      executionOrder.push('fetch')
      return {
        ok: true,
        json: () => Promise.resolve({ success: true }),
      } as Response
    })

    mockSignOut.mockImplementation(async () => {
      executionOrder.push('signOut')
      return undefined
    })

    await customSignOut()

    expect(executionOrder).toEqual([
      'triggerCrossServiceLogout',
      'fetch',
      'signOut'
    ])
  })
})
