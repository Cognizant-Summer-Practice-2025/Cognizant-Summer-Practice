import { render, screen } from '@testing-library/react'
import { Input } from '@/components/ui/input'

describe('Input', () => {
	it('renders with placeholder and data-slot', () => {
		render(<Input placeholder="Type here" />)
		const input = screen.getByPlaceholderText(/type here/i)
		expect(input).toBeInTheDocument()
		expect(input).toHaveAttribute('data-slot', 'input')
	})

	it('respects input type and placeholder', () => {
		render(<Input type="text" placeholder="Type your message..." data-testid="text-input" />)
		const input = screen.getByTestId('text-input') as HTMLInputElement
		expect(input.type).toBe('text')
		expect(input.placeholder).toBe('Type your message...')
	})
}) 