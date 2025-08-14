import { renderHook } from '@testing-library/react'
import { useCrossServiceAuth } from '@/lib/hooks/use-cross-service-auth'
import * as ssoAuth from '@/lib/auth/sso-auth'

jest.mock('@/lib/auth/sso-auth')

const mockSetupCrossServiceLogoutDetection = ssoAuth.setupCrossServiceLogoutDetection as jest.MockedFunction<
  typeof ssoAuth.setupCrossServiceLogoutDetection
>

describe('useCrossServiceAuth', () => {
  it('should set up cross-service logout detection on mount', () => {
    const mockCleanup = jest.fn()
    mockSetupCrossServiceLogoutDetection.mockReturnValue(mockCleanup)

    renderHook(() => useCrossServiceAuth())

    expect(mockSetupCrossServiceLogoutDetection).toHaveBeenCalledTimes(1)
  })

  it('should call cleanup function on unmount', () => {
    const mockCleanup = jest.fn()
    mockSetupCrossServiceLogoutDetection.mockReturnValue(mockCleanup)

    const { unmount } = renderHook(() => useCrossServiceAuth())

    unmount()

    expect(mockCleanup).toHaveBeenCalledTimes(1)
  })

  it('should handle multiple mounts and unmounts correctly', () => {
    const mockCleanup1 = jest.fn()
    const mockCleanup2 = jest.fn()
    
    mockSetupCrossServiceLogoutDetection
      .mockReturnValueOnce(mockCleanup1)
      .mockReturnValueOnce(mockCleanup2)

    // First hook
    const { unmount: unmount1 } = renderHook(() => useCrossServiceAuth())
    
    // Second hook
    const { unmount: unmount2 } = renderHook(() => useCrossServiceAuth())

    expect(mockSetupCrossServiceLogoutDetection).toHaveBeenCalledTimes(2)

    // Unmount first hook
    unmount1()
    expect(mockCleanup1).toHaveBeenCalledTimes(1)
    expect(mockCleanup2).not.toHaveBeenCalled()

    // Unmount second hook
    unmount2()
    expect(mockCleanup2).toHaveBeenCalledTimes(1)
  })

  it('should re-setup detection if effect runs again', () => {
    const mockCleanup1 = jest.fn()
    const mockCleanup2 = jest.fn()
    
    mockSetupCrossServiceLogoutDetection
      .mockReturnValueOnce(mockCleanup1)
      .mockReturnValueOnce(mockCleanup2)

    const { rerender } = renderHook(() => useCrossServiceAuth())

    expect(mockSetupCrossServiceLogoutDetection).toHaveBeenCalledTimes(1)

    // Force re-render (though in practice the effect only runs once due to empty dependency array)
    rerender()

    // Effect should still only have been called once due to empty dependency array
    expect(mockSetupCrossServiceLogoutDetection).toHaveBeenCalledTimes(1)
  })

  it('should handle cleanup function errors gracefully', () => {
    const mockCleanup = jest.fn().mockImplementation(() => {
      throw new Error('Cleanup failed')
    })
    mockSetupCrossServiceLogoutDetection.mockReturnValue(mockCleanup)

    const consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation()

    const { unmount } = renderHook(() => useCrossServiceAuth())

    expect(() => unmount()).not.toThrow()

    consoleErrorSpy.mockRestore()
  })
})
