import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import Input from '@/components/messages-page/input/input'

describe('Messages Page Input', () => {
	it('renders with placeholder and value', () => {
		const onChange = jest.fn()
		render(
			<Input 
				placeholder="Type your message..." 
				value="Hello world" 
				onChange={onChange} 
			/>
		)
		
		const input = screen.getByDisplayValue('Hello world')
		expect(input).toBeInTheDocument()
		expect(input).toHaveAttribute('placeholder', 'Type your message...')
	})

	it('calls onChange when user types', async () => {
		const user = userEvent.setup()
		const onChange = jest.fn()
		
		render(
			<Input 
				placeholder="Enter text" 
				value="" 
				onChange={onChange} 
			/>
		)
		
		const input = screen.getByPlaceholderText('Enter text')
		await user.type(input, 'Hello')
		
		// Should be called for each character typed
		expect(onChange).toHaveBeenCalledTimes(5)
		
		// Check that onChange was called with proper event structure
		const firstCall = onChange.mock.calls[0][0]
		expect(firstCall).toHaveProperty('target')
		expect(firstCall).toHaveProperty('type', 'change')
	})

	it('applies default className when none provided', () => {
		const onChange = jest.fn()
		render(
			<Input 
				placeholder="Test" 
				value="" 
				onChange={onChange} 
			/>
		)
		
		const input = screen.getByPlaceholderText('Test')
		expect(input).toHaveClass('w-full')
	})

	it('applies custom className', () => {
		const onChange = jest.fn()
		render(
			<Input 
				placeholder="Test" 
				value="" 
				onChange={onChange} 
				className="custom-input"
			/>
		)
		
		const input = screen.getByPlaceholderText('Test')
		expect(input).toHaveClass('custom-input')
	})

	it('is a controlled component', async () => {
		const user = userEvent.setup()
		let value = 'initial'
		const onChange = jest.fn((e) => {
			value = e.target.value
		})
		
		const { rerender } = render(
			<Input 
				placeholder="Controlled input" 
				value={value} 
				onChange={onChange} 
			/>
		)
		
		const input = screen.getByDisplayValue('initial')
		await user.clear(input)
		await user.type(input, 'new value')
		
		// Re-render with updated value to simulate parent component updating
		rerender(
			<Input 
				placeholder="Controlled input" 
				value="new value" 
				onChange={onChange} 
			/>
		)
		
		expect(screen.getByDisplayValue('new value')).toBeInTheDocument()
	})
}) 