import '@testing-library/jest-dom'

declare global {
  namespace jest {
    interface Matchers<R> {
      toBeInTheDocument(): R
      toHaveAttribute(attr: string, value?: string): R
      toHaveClass(...classNames: string[]): R
      toHaveValue(value: string | number | string[]): R
      toBeDisabled(): R
      toBeRequired(): R
      toBeChecked(): R
      toHaveTextContent(text: string | RegExp): R
    }
  }
}

export {}
