import { signOut } from 'next-auth/react'
import {
  setupCrossServiceLogoutDetection,
  triggerCrossServiceLogout,
  enhancedSignOut
} from '@/lib/auth/sso-auth'

jest.mock('next-auth/react')
jest.mock('@/lib/auth/custom-signout', () => ({
  customSignOut: jest.fn(),
}))

const mockSignOut = signOut as jest.MockedFunction<typeof signOut>

describe('SSO Auth', () => {
  beforeEach(() => {
    jest.clearAllMocks()
    
    // Mock localStorage
    Object.defineProperty(window, 'localStorage', {
      value: {
        getItem: jest.fn(),
        setItem: jest.fn(),
        removeItem: jest.fn(),
        clear: jest.fn(),
      },
      writable: true,
    })

    // Mock window.location
    delete (window as any).location
    ;(window as any).location = {
      href: 'http://localhost:3000',
      assign: jest.fn(),
      replace: jest.fn(),
      reload: jest.fn(),
    }

    process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE = 'http://localhost:3001'
  })

  afterEach(() => {
    delete process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE
  })

  describe('setupCrossServiceLogoutDetection', () => {
    it('should set up storage event listener', () => {
      const addEventListenerSpy = jest.spyOn(window, 'addEventListener')
      
      const cleanup = setupCrossServiceLogoutDetection()

      expect(addEventListenerSpy).toHaveBeenCalledWith('storage', expect.any(Function))
      expect(typeof cleanup).toBe('function')

      addEventListenerSpy.mockRestore()
    })

    it('should remove event listener on cleanup', () => {
      const removeEventListenerSpy = jest.spyOn(window, 'removeEventListener')
      
      const cleanup = setupCrossServiceLogoutDetection()
      cleanup()

      expect(removeEventListenerSpy).toHaveBeenCalledWith('storage', expect.any(Function))

      removeEventListenerSpy.mockRestore()
    })

    it('should handle sso_session removal event', async () => {
      const consoleLogSpy = jest.spyOn(console, 'log').mockImplementation()
      mockSignOut.mockResolvedValue(undefined)

      let storageHandler: ((event: StorageEvent) => void) | undefined

      jest.spyOn(window, 'addEventListener').mockImplementation((type, handler) => {
        if (type === 'storage') {
          storageHandler = handler as (event: StorageEvent) => void
        }
      })

      setupCrossServiceLogoutDetection()

      // Simulate storage event for sso_session removal
      const mockEvent = new StorageEvent('storage', {
        key: 'sso_session',
        newValue: null,
        oldValue: 'some-session-data',
      })

      if (storageHandler) {
        await storageHandler(mockEvent)
      }

      expect(consoleLogSpy).toHaveBeenCalledWith(
        'Logout detected from another service, signing out from NextAuth...'
      )

      expect(mockSignOut).toHaveBeenCalledWith({
        callbackUrl: 'http://localhost:3001',
        redirect: true,
      })

      consoleLogSpy.mockRestore()
    })

    it('should ignore non-sso_session storage events', async () => {
      const consoleLogSpy = jest.spyOn(console, 'log').mockImplementation()

      let storageHandler: ((event: StorageEvent) => void) | undefined

      jest.spyOn(window, 'addEventListener').mockImplementation((type, handler) => {
        if (type === 'storage') {
          storageHandler = handler as (event: StorageEvent) => void
        }
      })

      setupCrossServiceLogoutDetection()

      // Simulate storage event for different key
      const mockEvent = new StorageEvent('storage', {
        key: 'other_key',
        newValue: null,
        oldValue: 'some-data',
      })

      if (storageHandler) {
        await storageHandler(mockEvent)
      }

      expect(consoleLogSpy).not.toHaveBeenCalled()
      expect(mockSignOut).not.toHaveBeenCalled()

      consoleLogSpy.mockRestore()
    })

    it('should ignore sso_session storage events with non-null newValue', async () => {
      const consoleLogSpy = jest.spyOn(console, 'log').mockImplementation()

      let storageHandler: ((event: StorageEvent) => void) | undefined

      jest.spyOn(window, 'addEventListener').mockImplementation((type, handler) => {
        if (type === 'storage') {
          storageHandler = handler as (event: StorageEvent) => void
        }
      })

      setupCrossServiceLogoutDetection()

      // Simulate storage event for sso_session with new value
      const mockEvent = new StorageEvent('storage', {
        key: 'sso_session',
        newValue: 'new-session-data',
        oldValue: 'old-session-data',
      })

      if (storageHandler) {
        await storageHandler(mockEvent)
      }

      expect(consoleLogSpy).not.toHaveBeenCalled()
      expect(mockSignOut).not.toHaveBeenCalled()

      consoleLogSpy.mockRestore()
    })

    it('should handle signOut errors by redirecting to home service', async () => {
      const consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation()
      mockSignOut.mockRejectedValue(new Error('SignOut failed'))

      let storageHandler: ((event: StorageEvent) => void) | undefined

      jest.spyOn(window, 'addEventListener').mockImplementation((type, handler) => {
        if (type === 'storage') {
          storageHandler = handler as (event: StorageEvent) => void
        }
      })

      setupCrossServiceLogoutDetection()

      const mockEvent = new StorageEvent('storage', {
        key: 'sso_session',
        newValue: null,
        oldValue: 'some-session-data',
      })

      if (storageHandler) {
        await storageHandler(mockEvent)
      }

      expect(consoleErrorSpy).toHaveBeenCalledWith(
        'Error signing out from NextAuth after cross-service logout:',
        expect.any(Error)
      )

      expect(window.location.href).toBe('http://localhost:3001')

      consoleErrorSpy.mockRestore()
    })

    it('should use default URL when environment variable is not set', async () => {
      delete process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE

      const consoleLogSpy = jest.spyOn(console, 'log').mockImplementation()
      mockSignOut.mockResolvedValue(undefined)

      let storageHandler: ((event: StorageEvent) => void) | undefined

      jest.spyOn(window, 'addEventListener').mockImplementation((type, handler) => {
        if (type === 'storage') {
          storageHandler = handler as (event: StorageEvent) => void
        }
      })

      setupCrossServiceLogoutDetection()

      const mockEvent = new StorageEvent('storage', {
        key: 'sso_session',
        newValue: null,
        oldValue: 'some-session-data',
      })

      if (storageHandler) {
        await storageHandler(mockEvent)
      }

      expect(mockSignOut).toHaveBeenCalledWith({
        callbackUrl: 'http://localhost:3001',
        redirect: true,
      })

      consoleLogSpy.mockRestore()
    })
  })

  describe('triggerCrossServiceLogout', () => {
    it('should remove existing sso_session from localStorage', () => {
      const mockLocalStorage = window.localStorage as jest.Mocked<Storage>
      mockLocalStorage.getItem.mockReturnValue('existing-session')
      
      const consoleLogSpy = jest.spyOn(console, 'log').mockImplementation()

      triggerCrossServiceLogout()

      expect(mockLocalStorage.getItem).toHaveBeenCalledWith('sso_session')
      expect(mockLocalStorage.removeItem).toHaveBeenCalledWith('sso_session')
      expect(consoleLogSpy).toHaveBeenCalledWith('SSO session cleared, other services will detect logout')

      consoleLogSpy.mockRestore()
    })

    it('should do nothing if no existing sso_session', () => {
      const mockLocalStorage = window.localStorage as jest.Mocked<Storage>
      mockLocalStorage.getItem.mockReturnValue(null)
      
      const consoleLogSpy = jest.spyOn(console, 'log').mockImplementation()

      triggerCrossServiceLogout()

      expect(mockLocalStorage.getItem).toHaveBeenCalledWith('sso_session')
      expect(mockLocalStorage.removeItem).not.toHaveBeenCalled()
      expect(consoleLogSpy).not.toHaveBeenCalled()

      consoleLogSpy.mockRestore()
    })

    it('should handle localStorage errors gracefully', () => {
      const mockLocalStorage = window.localStorage as jest.Mocked<Storage>
      mockLocalStorage.getItem.mockImplementation(() => {
        throw new Error('localStorage error')
      })

      const consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation()

      triggerCrossServiceLogout()

      expect(consoleErrorSpy).toHaveBeenCalledWith(
        'Failed to trigger cross-service logout:',
        expect.any(Error)
      )

      consoleErrorSpy.mockRestore()
    })
  })

  describe('enhancedSignOut', () => {
    const { customSignOut } = require('@/lib/auth/custom-signout')

    beforeEach(() => {
      jest.clearAllMocks()
    })

    it('should trigger cross-service logout and call custom signout', async () => {
      const mockLocalStorage = window.localStorage as jest.Mocked<Storage>
      mockLocalStorage.getItem.mockReturnValue('existing-session')
      
      customSignOut.mockResolvedValue(undefined)

      await enhancedSignOut()

      expect(mockLocalStorage.removeItem).toHaveBeenCalledWith('sso_session')
      expect(customSignOut).toHaveBeenCalled()
    })

    it('should fall back to regular signOut on custom signOut error', async () => {
      const mockLocalStorage = window.localStorage as jest.Mocked<Storage>
      mockLocalStorage.getItem.mockReturnValue('existing-session')
      
      const consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation()
      customSignOut.mockRejectedValue(new Error('Custom signout failed'))
      mockSignOut.mockResolvedValue(undefined)

      await enhancedSignOut()

      expect(consoleErrorSpy).toHaveBeenCalledWith(
        'Error during enhanced sign-out:',
        expect.any(Error)
      )

      expect(mockSignOut).toHaveBeenCalledWith({
        callbackUrl: 'http://localhost:3001',
        redirect: true,
      })

      consoleErrorSpy.mockRestore()
    })

    it('should fall back to regular signOut on module import error', async () => {
      // Mock import error
      jest.doMock('@/lib/auth/custom-signout', () => {
        throw new Error('Module not found')
      })

      const consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation()
      mockSignOut.mockResolvedValue(undefined)

      await enhancedSignOut()

      expect(consoleErrorSpy).toHaveBeenCalledWith(
        'Error during enhanced sign-out:',
        expect.any(Error)
      )

      expect(mockSignOut).toHaveBeenCalledWith({
        callbackUrl: 'http://localhost:3001',
        redirect: true,
      })

      consoleErrorSpy.mockRestore()
    })

    it('should use default URL when environment variable is not set', async () => {
      delete process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE

      customSignOut.mockRejectedValue(new Error('Custom signout failed'))
      mockSignOut.mockResolvedValue(undefined)

      await enhancedSignOut()

      expect(mockSignOut).toHaveBeenCalledWith({
        callbackUrl: 'http://localhost:3001',
        redirect: true,
      })
    })
  })
})
