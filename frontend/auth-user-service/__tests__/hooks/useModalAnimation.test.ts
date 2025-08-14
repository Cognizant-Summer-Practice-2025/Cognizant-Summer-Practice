import { renderHook, act, waitFor } from '@testing-library/react'
import { useModalAnimation } from '@/hooks/useModalAnimation'

// Mock timers
jest.useFakeTimers()

describe('useModalAnimation', () => {
  afterEach(() => {
    jest.clearAllTimers()
  })

  afterAll(() => {
    jest.useRealTimers()
  })

  it('should initialize with correct default state', () => {
    const mockOnClose = jest.fn()
    
    const { result } = renderHook(() => 
      useModalAnimation({ isOpen: false, onClose: mockOnClose })
    )

    expect(result.current.isVisible).toBe(false)
    expect(result.current.isAnimatingOut).toBe(false)
    expect(typeof result.current.handleClose).toBe('function')
  })

  it('should set isVisible to true when modal is opened', () => {
    const mockOnClose = jest.fn()
    
    const { result } = renderHook(() => 
      useModalAnimation({ isOpen: true, onClose: mockOnClose })
    )

    expect(result.current.isVisible).toBe(true)
    expect(result.current.isAnimatingOut).toBe(false)
  })

  it('should trigger closing animation when modal is closed', async () => {
    const mockOnClose = jest.fn()
    
    const { result, rerender } = renderHook(
      ({ isOpen }) => useModalAnimation({ isOpen, onClose: mockOnClose }),
      { initialProps: { isOpen: true } }
    )

    // Initially open
    expect(result.current.isVisible).toBe(true)
    expect(result.current.isAnimatingOut).toBe(false)

    // Close the modal
    rerender({ isOpen: false })

    expect(result.current.isVisible).toBe(true)
    expect(result.current.isAnimatingOut).toBe(true)

    // Fast-forward time to complete animation
    act(() => {
      jest.advanceTimersByTime(300)
    })

    await waitFor(() => {
      expect(result.current.isVisible).toBe(false)
      expect(result.current.isAnimatingOut).toBe(false)
    })
  })

  it('should use custom duration for animation', async () => {
    const mockOnClose = jest.fn()
    const customDuration = 500
    
    const { result, rerender } = renderHook(
      ({ isOpen }) => useModalAnimation({ 
        isOpen, 
        onClose: mockOnClose, 
        duration: customDuration 
      }),
      { initialProps: { isOpen: true } }
    )

    // Close the modal
    rerender({ isOpen: false })

    expect(result.current.isAnimatingOut).toBe(true)

    // Advance time by less than custom duration
    act(() => {
      jest.advanceTimersByTime(400)
    })

    // Should still be visible
    expect(result.current.isVisible).toBe(true)

    // Advance time to complete custom duration
    act(() => {
      jest.advanceTimersByTime(100)
    })

    await waitFor(() => {
      expect(result.current.isVisible).toBe(false)
    })
  })

  it('should handle rapid open/close transitions', () => {
    const mockOnClose = jest.fn()
    
    const { result, rerender } = renderHook(
      ({ isOpen }) => useModalAnimation({ isOpen, onClose: mockOnClose }),
      { initialProps: { isOpen: false } }
    )

    // Open
    rerender({ isOpen: true })
    expect(result.current.isVisible).toBe(true)
    expect(result.current.isAnimatingOut).toBe(false)

    // Close immediately
    rerender({ isOpen: false })
    expect(result.current.isAnimatingOut).toBe(true)

    // Open again before animation completes
    rerender({ isOpen: true })
    expect(result.current.isVisible).toBe(true)
    expect(result.current.isAnimatingOut).toBe(false)
  })

  it('should call onClose when handleClose is called and not animating out', () => {
    const mockOnClose = jest.fn()
    
    const { result } = renderHook(() => 
      useModalAnimation({ isOpen: true, onClose: mockOnClose })
    )

    act(() => {
      result.current.handleClose()
    })

    expect(mockOnClose).toHaveBeenCalledTimes(1)
  })

  it('should not call onClose when handleClose is called while animating out', () => {
    const mockOnClose = jest.fn()
    
    const { result, rerender } = renderHook(
      ({ isOpen }) => useModalAnimation({ isOpen, onClose: mockOnClose }),
      { initialProps: { isOpen: true } }
    )

    // Start closing animation
    rerender({ isOpen: false })
    expect(result.current.isAnimatingOut).toBe(true)

    // Try to call handleClose while animating
    act(() => {
      result.current.handleClose()
    })

    expect(mockOnClose).not.toHaveBeenCalled()
  })

  it('should clear timeout on unmount', () => {
    const mockOnClose = jest.fn()
    const clearTimeoutSpy = jest.spyOn(global, 'clearTimeout')
    
    const { rerender, unmount } = renderHook(
      ({ isOpen }) => useModalAnimation({ isOpen, onClose: mockOnClose }),
      { initialProps: { isOpen: true } }
    )

    // Start closing animation
    rerender({ isOpen: false })

    // Unmount before animation completes
    unmount()

    expect(clearTimeoutSpy).toHaveBeenCalled()
    
    clearTimeoutSpy.mockRestore()
  })

  it('should handle case when modal is closed but was never visible', () => {
    const mockOnClose = jest.fn()
    
    const { result, rerender } = renderHook(
      ({ isOpen }) => useModalAnimation({ isOpen, onClose: mockOnClose }),
      { initialProps: { isOpen: false } }
    )

    // Should remain in initial state
    expect(result.current.isVisible).toBe(false)
    expect(result.current.isAnimatingOut).toBe(false)

    // Closing when already closed should not trigger animation
    rerender({ isOpen: false })
    
    expect(result.current.isVisible).toBe(false)
    expect(result.current.isAnimatingOut).toBe(false)
  })

  it('should handle multiple consecutive opens', () => {
    const mockOnClose = jest.fn()
    
    const { result, rerender } = renderHook(
      ({ isOpen }) => useModalAnimation({ isOpen, onClose: mockOnClose }),
      { initialProps: { isOpen: false } }
    )

    // First open
    rerender({ isOpen: true })
    expect(result.current.isVisible).toBe(true)

    // "Open" again (should maintain state)
    rerender({ isOpen: true })
    expect(result.current.isVisible).toBe(true)
    expect(result.current.isAnimatingOut).toBe(false)
  })
})
