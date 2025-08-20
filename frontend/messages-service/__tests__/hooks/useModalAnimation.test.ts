import { renderHook, act } from '@testing-library/react'
import { useModalAnimation } from '@/hooks/useModalAnimation'

describe('useModalAnimation', () => {
	beforeEach(() => {
		jest.useFakeTimers()
	})

	afterEach(() => {
		jest.runOnlyPendingTimers()
		jest.useRealTimers()
	})

	it('sets visible when opened and hides after duration when closed', () => {
		const onClose = jest.fn()
		const { result, rerender } = renderHook((props) => useModalAnimation(props), {
			initialProps: { isOpen: true, onClose, duration: 50 },
		})

		expect(result.current.isVisible).toBe(true)
		expect(result.current.isAnimatingOut).toBe(false)

		// Trigger close
		act(() => {
			rerender({ isOpen: false, onClose, duration: 50 })
		})
		
		expect(result.current.isAnimatingOut).toBe(true)

		// Advance timers in act
		act(() => {
			jest.advanceTimersByTime(50)
		})

		expect(result.current.isVisible).toBe(false)
		expect(result.current.isAnimatingOut).toBe(false)
	})

	it('does not call onClose while animating out, but calls when not animating', () => {
		const onClose = jest.fn()
		const { result, rerender } = renderHook((props) => useModalAnimation(props), {
			initialProps: { isOpen: true, onClose, duration: 50 },
		})

		// When open, handleClose should call onClose
		act(() => {
			result.current.handleClose()
		})
		expect(onClose).toHaveBeenCalledTimes(1)

		// Start closing animation
		act(() => {
			rerender({ isOpen: false, onClose, duration: 50 })
		})
		
		act(() => {
			result.current.handleClose()
		})
		// Should not increment while animating out
		expect(onClose).toHaveBeenCalledTimes(1)
	})
}) 