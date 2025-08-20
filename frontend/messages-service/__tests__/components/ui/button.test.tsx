import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Button } from '@/components/ui/button'

describe('Button', () => {
	it('renders children and has data-slot attribute', () => {
		render(<Button>Click me</Button>)
		const btn = screen.getByRole('button', { name: /click me/i })
		expect(btn).toBeInTheDocument()
		expect(btn).toHaveAttribute('data-slot', 'button')
	})

	it('merges custom className', () => {
		render(<Button className="custom-class">Go</Button>)
		const btn = screen.getByRole('button', { name: /go/i })
		expect(btn.className).toContain('custom-class')
	})

	it('handles click events', async () => {
		const user = userEvent.setup()
		const onClick = jest.fn()
		render(
			<Button onClick={onClick}>Press</Button>
		)
		await user.click(screen.getByRole('button', { name: /press/i }))
		expect(onClick).toHaveBeenCalled()
	})
}) 