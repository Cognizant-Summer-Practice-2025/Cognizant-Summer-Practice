import React from 'react'
import { render, screen, fireEvent } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Input } from '@/components/ui/input'

describe('Input', () => {
  it('should render input with default props', () => {
    render(<Input />)
    
    const input = screen.getByRole('textbox')
    expect(input).toBeInTheDocument()
    expect(input).toHaveClass('flex', 'h-9', 'w-full')
  })

  it('should accept different input types', () => {
    const { rerender } = render(<Input type="text" />)
    expect(screen.getByRole('textbox')).toHaveAttribute('type', 'text')

    rerender(<Input type="email" />)
    expect(screen.getByRole('textbox')).toHaveAttribute('type', 'email')

    rerender(<Input type="password" />)
    expect(screen.getByDisplayValue('')).toHaveAttribute('type', 'password')

    rerender(<Input type="number" />)
    expect(screen.getByRole('spinbutton')).toHaveAttribute('type', 'number')
  })

  it('should handle text input correctly', async () => {
    const user = userEvent.setup()
    render(<Input placeholder="Enter text" />)
    
    const input = screen.getByPlaceholderText('Enter text')
    await user.type(input, 'Hello World')
    
    expect(input).toHaveValue('Hello World')
  })

  it('should call onChange handler', async () => {
    const handleChange = jest.fn()
    const user = userEvent.setup()
    render(<Input onChange={handleChange} />)
    
    const input = screen.getByRole('textbox')
    await user.type(input, 'test')
    
    expect(handleChange).toHaveBeenCalled()
    expect(handleChange).toHaveBeenCalledTimes(4) // One for each character
  })

  it('should be disabled when disabled prop is true', () => {
    render(<Input disabled />)
    
    const input = screen.getByRole('textbox')
    expect(input).toBeDisabled()
    expect(input).toHaveClass('disabled:pointer-events-none', 'disabled:opacity-50')
  })

  it('should not accept input when disabled', async () => {
    const user = userEvent.setup()
    render(<Input disabled />)
    
    const input = screen.getByRole('textbox')
    await user.type(input, 'test')
    
    expect(input).toHaveValue('')
  })

  it('should accept custom className', () => {
    render(<Input className="custom-class" />)
    
    const input = screen.getByRole('textbox')
    expect(input).toHaveClass('custom-class')
    expect(input).toHaveClass('flex') // Should still have default classes
  })

  it('should show placeholder text', () => {
    render(<Input placeholder="Enter your name" />)
    
    expect(screen.getByPlaceholderText('Enter your name')).toBeInTheDocument()
  })

  it('should forward ref correctly', () => {
    const ref = React.createRef<HTMLInputElement>()
    render(<Input ref={ref} />)
    
    expect(ref.current).toBeInstanceOf(HTMLInputElement)
  })

  it('should accept additional HTML attributes', () => {
    render(
      <Input 
        data-testid="test-input"
        aria-label="Test input"
        maxLength={10}
        required
      />
    )
    
    const input = screen.getByRole('textbox')
    expect(input).toHaveAttribute('data-testid', 'test-input')
    expect(input).toHaveAttribute('aria-label', 'Test input')
    expect(input).toHaveAttribute('maxLength', '10')
    expect(input).toBeRequired()
  })

  it('should have proper focus styles', () => {
    render(<Input />)
    
    const input = screen.getByRole('textbox')
    expect(input).toHaveClass('focus-visible:border-ring', 'focus-visible:ring-ring/50')
  })

  it('should handle focus and blur events', () => {
    const handleFocus = jest.fn()
    const handleBlur = jest.fn()
    render(<Input onFocus={handleFocus} onBlur={handleBlur} />)
    
    const input = screen.getByRole('textbox')
    
    fireEvent.focus(input)
    expect(handleFocus).toHaveBeenCalledTimes(1)
    
    fireEvent.blur(input)
    expect(handleBlur).toHaveBeenCalledTimes(1)
  })

  it('should handle aria-invalid attribute correctly', () => {
    render(<Input aria-invalid="true" />)
    
    const input = screen.getByRole('textbox')
    expect(input).toHaveAttribute('aria-invalid', 'true')
    expect(input).toHaveClass('aria-invalid:ring-destructive/20')
  })

  it('should work with controlled input', async () => {
    const TestControlledInput = () => {
      const [value, setValue] = React.useState('')
      return (
        <Input 
          value={value} 
          onChange={(e) => setValue(e.target.value)}
          data-testid="controlled-input"
        />
      )
    }

    const user = userEvent.setup()
    render(<TestControlledInput />)
    
    const input = screen.getByTestId('controlled-input')
    await user.type(input, 'controlled')
    
    expect(input).toHaveValue('controlled')
  })

  it('should work with uncontrolled input', async () => {
    const user = userEvent.setup()
    render(<Input defaultValue="initial" />)
    
    const input = screen.getByDisplayValue('initial')
    await user.clear(input)
    await user.type(input, 'uncontrolled')
    
    expect(input).toHaveValue('uncontrolled')
  })

  it('should handle file input type', () => {
    render(<Input type="file" />)
    
    const input = screen.getByTestId('input') || document.querySelector('input[type="file"]')
    expect(input).toHaveAttribute('type', 'file')
    expect(input).toHaveClass('file:text-foreground')
  })

  it('should handle keyboard events', () => {
    const handleKeyDown = jest.fn()
    const handleKeyUp = jest.fn()
    render(<Input onKeyDown={handleKeyDown} onKeyUp={handleKeyUp} />)
    
    const input = screen.getByRole('textbox')
    
    fireEvent.keyDown(input, { key: 'Enter' })
    expect(handleKeyDown).toHaveBeenCalledWith(
      expect.objectContaining({ key: 'Enter' })
    )
    
    fireEvent.keyUp(input, { key: 'Enter' })
    expect(handleKeyUp).toHaveBeenCalledWith(
      expect.objectContaining({ key: 'Enter' })
    )
  })

  it('should have data-slot attribute', () => {
    render(<Input />)
    
    const input = screen.getByRole('textbox')
    expect(input).toHaveAttribute('data-slot', 'input')
  })

  it('should handle min and max attributes for number inputs', () => {
    render(<Input type="number" min={0} max={100} />)
    
    const input = screen.getByRole('spinbutton')
    expect(input).toHaveAttribute('min', '0')
    expect(input).toHaveAttribute('max', '100')
  })

  it('should handle step attribute for number inputs', () => {
    render(<Input type="number" step={0.1} />)
    
    const input = screen.getByRole('spinbutton')
    expect(input).toHaveAttribute('step', '0.1')
  })

  it('should handle readonly attribute', () => {
    render(<Input readOnly value="readonly value" />)
    
    const input = screen.getByDisplayValue('readonly value')
    expect(input).toHaveAttribute('readonly')
  })
})
