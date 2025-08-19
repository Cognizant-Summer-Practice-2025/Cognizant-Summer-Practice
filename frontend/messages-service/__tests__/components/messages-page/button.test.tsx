import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import Button from '@/components/messages-page/button/button'

describe('Button', () => {
	it('renders with text prop', () => {
		const onClick = jest.fn()
		render(<Button text="Click me" onClick={onClick} />)
		
		const button = screen.getByRole('button', { name: /click me/i })
		expect(button).toBeInTheDocument()
	})

	it('calls onClick when clicked', async () => {
		const user = userEvent.setup()
		const onClick = jest.fn()
		
		render(<Button text="Submit" onClick={onClick} />)
		
		const button = screen.getByRole('button', { name: /submit/i })
		await user.click(button)
		
		expect(onClick).toHaveBeenCalledTimes(1)
	})

	it('renders children instead of text when children provided', () => {
		const onClick = jest.fn()
		render(
			<Button text="Default Text" onClick={onClick}>
				<span>Custom Content</span>
			</Button>
		)
		
		expect(screen.getByText('Custom Content')).toBeInTheDocument()
		expect(screen.queryByText('Default Text')).not.toBeInTheDocument()
	})

	it('applies custom className', () => {
		const onClick = jest.fn()
		render(
			<Button 
				text="Styled Button" 
				onClick={onClick} 
				className="custom-btn"
			/>
		)
		
		const button = screen.getByRole('button', { name: /styled button/i })
		expect(button).toHaveClass('custom-btn')
	})

	it('renders with icon and text as children', () => {
		const onClick = jest.fn()
		render(
			<Button text="Default" onClick={onClick}>
				<span data-testid="icon">📧</span>
				Send Message
			</Button>
		)
		
		expect(screen.getByTestId('icon')).toBeInTheDocument()
		expect(screen.getByText('Send Message')).toBeInTheDocument()
	})
}) 