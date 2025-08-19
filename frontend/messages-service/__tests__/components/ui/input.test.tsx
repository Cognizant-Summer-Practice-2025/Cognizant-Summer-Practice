import { render, screen } from '@testing-library/react'
import { Input } from '@/components/ui/input'

describe('Input', () => {
	it('renders with placeholder and data-slot', () => {
		render(<Input placeholder="Type here" />)
		const input = screen.getByPlaceholderText(/type here/i)
		expect(input).toBeInTheDocument()
		expect(input).toHaveAttribute('data-slot', 'input')
	})

	it('respects input type', () => {
		render(<Input type="password" />)
		const input = screen.getByRole('textbox') as HTMLInputElement
		expect(input.type).toBe('password')
	})
}) 