import React from 'react'
import { render, screen, fireEvent } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Button } from '@/components/ui/button'

describe('Button', () => {
  it('should render button with default props', () => {
    render(<Button>Click me</Button>)
    
    const button = screen.getByRole('button', { name: 'Click me' })
    expect(button).toBeInTheDocument()
    expect(button).toHaveClass('inline-flex', 'items-center', 'justify-center')
  })

  it('should render as child component when asChild is true', () => {
    render(
      <Button asChild>
        <a href="/test">Link Button</a>
      </Button>
    )
    
    const link = screen.getByRole('link', { name: 'Link Button' })
    expect(link).toBeInTheDocument()
    expect(link).toHaveAttribute('href', '/test')
  })

  it('should apply different variants correctly', () => {
    const { rerender } = render(<Button variant="default">Default</Button>)
    expect(screen.getByRole('button')).toHaveClass('bg-primary')

    rerender(<Button variant="destructive">Destructive</Button>)
    expect(screen.getByRole('button')).toHaveClass('bg-destructive')

    rerender(<Button variant="outline">Outline</Button>)
    expect(screen.getByRole('button')).toHaveClass('border', 'bg-background')

    rerender(<Button variant="secondary">Secondary</Button>)
    expect(screen.getByRole('button')).toHaveClass('bg-secondary')

    rerender(<Button variant="ghost">Ghost</Button>)
    expect(screen.getByRole('button')).toHaveClass('hover:bg-accent')

    rerender(<Button variant="link">Link</Button>)
    expect(screen.getByRole('button')).toHaveClass('text-primary', 'underline-offset-4')
  })

  it('should apply different sizes correctly', () => {
    const { rerender } = render(<Button size="default">Default Size</Button>)
    expect(screen.getByRole('button')).toHaveClass('h-9', 'px-4', 'py-2')

    rerender(<Button size="sm">Small</Button>)
    expect(screen.getByRole('button')).toHaveClass('h-8', 'px-3')

    rerender(<Button size="lg">Large</Button>)
    expect(screen.getByRole('button')).toHaveClass('h-10', 'px-6')

    rerender(<Button size="icon">Icon</Button>)
    expect(screen.getByRole('button')).toHaveClass('size-9')
  })

  it('should handle click events', () => {
    const handleClick = jest.fn()
    render(<Button onClick={handleClick}>Click me</Button>)
    
    fireEvent.click(screen.getByRole('button'))
    expect(handleClick).toHaveBeenCalledTimes(1)
  })

  it('should be disabled when disabled prop is true', () => {
    render(<Button disabled>Disabled Button</Button>)
    
    const button = screen.getByRole('button')
    expect(button).toBeDisabled()
    expect(button).toHaveClass('disabled:pointer-events-none', 'disabled:opacity-50')
  })

  it('should not trigger click when disabled', () => {
    const handleClick = jest.fn()
    render(<Button disabled onClick={handleClick}>Disabled Button</Button>)
    
    fireEvent.click(screen.getByRole('button'))
    expect(handleClick).not.toHaveBeenCalled()
  })

  it('should accept custom className', () => {
    render(<Button className="custom-class">Custom Button</Button>)
    
    const button = screen.getByRole('button')
    expect(button).toHaveClass('custom-class')
    expect(button).toHaveClass('inline-flex') // Should still have default classes
  })

  it('should render children correctly', () => {
    render(
      <Button>
        <span>Icon</span>
        Button Text
      </Button>
    )
    
    expect(screen.getByText('Icon')).toBeInTheDocument()
    expect(screen.getByText('Button Text')).toBeInTheDocument()
  })

  it('should forward ref correctly', () => {
    const ref = React.createRef<HTMLButtonElement>()
    render(<Button ref={ref}>Button with ref</Button>)
    
    expect(ref.current).toBeInstanceOf(HTMLButtonElement)
    expect(ref.current?.textContent).toBe('Button with ref')
  })

  it('should accept additional HTML attributes', () => {
    render(
      <Button 
        type="submit" 
        data-testid="submit-button"
        aria-label="Submit form"
      >
        Submit
      </Button>
    )
    
    const button = screen.getByRole('button')
    expect(button).toHaveAttribute('type', 'submit')
    expect(button).toHaveAttribute('data-testid', 'submit-button')
    expect(button).toHaveAttribute('aria-label', 'Submit form')
  })

  it('should have proper focus styles', () => {
    render(<Button>Focus me</Button>)
    
    const button = screen.getByRole('button')
    expect(button).toHaveClass('focus-visible:border-ring', 'focus-visible:ring-ring/50')
  })

  it('should handle keyboard navigation', async () => {
    const user = userEvent.setup()
    const handleClick = jest.fn()
    render(<Button onClick={handleClick}>Keyboard Button</Button>)
    
    const button = screen.getByRole('button')
    await user.click(button) // First click to focus
    
    await user.keyboard('{Enter}')
    expect(handleClick).toHaveBeenCalledTimes(2) // Once for focus click, once for Enter
    
    await user.keyboard(' ')
    expect(handleClick).toHaveBeenCalledTimes(3) // Once more for Space
  })

  it('should render with SVG icons correctly', () => {
    render(
      <Button>
        <svg data-testid="icon" width="16" height="16">
          <circle cx="8" cy="8" r="4" />
        </svg>
        Button with icon
      </Button>
    )
    
    const icon = screen.getByTestId('icon')
    expect(icon).toBeInTheDocument()
    expect(screen.getByText('Button with icon')).toBeInTheDocument()
  })

  it('should handle aria-invalid attribute correctly', () => {
    render(<Button aria-invalid="true">Invalid Button</Button>)
    
    const button = screen.getByRole('button')
    expect(button).toHaveAttribute('aria-invalid', 'true')
    expect(button).toHaveClass('aria-invalid:ring-destructive/20')
  })

  it('should work with form submission', () => {
    const handleSubmit = jest.fn()
    render(
      <form onSubmit={handleSubmit}>
        <Button type="submit">Submit</Button>
      </form>
    )
    
    fireEvent.click(screen.getByRole('button'))
    expect(handleSubmit).toHaveBeenCalled()
  })

  it('should prevent form submission when disabled', () => {
    const handleSubmit = jest.fn()
    render(
      <form onSubmit={handleSubmit}>
        <Button type="submit" disabled>Submit</Button>
      </form>
    )
    
    fireEvent.click(screen.getByRole('button'))
    expect(handleSubmit).not.toHaveBeenCalled()
  })
})
