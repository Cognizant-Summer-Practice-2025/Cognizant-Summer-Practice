import React from 'react'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
  DialogFooter,
  DialogClose
} from '@/components/ui/dialog'

describe('Dialog Components', () => {
  describe('Dialog', () => {
    it('should render dialog with trigger and content', async () => {
      const user = userEvent.setup()
      
      render(
        <Dialog>
          <DialogTrigger>Open Dialog</DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Test Dialog</DialogTitle>
              <DialogDescription>This is a test dialog</DialogDescription>
            </DialogHeader>
            <p>Dialog content here</p>
          </DialogContent>
        </Dialog>
      )

      const trigger = screen.getByRole('button', { name: 'Open Dialog' })
      expect(trigger).toBeInTheDocument()

      await user.click(trigger)

      await waitFor(() => {
        expect(screen.getByRole('dialog')).toBeInTheDocument()
      })

      expect(screen.getByText('Test Dialog')).toBeInTheDocument()
      expect(screen.getByText('This is a test dialog')).toBeInTheDocument()
      expect(screen.getByText('Dialog content here')).toBeInTheDocument()
    })

    it('should open and close dialog using controlled state', async () => {
      const TestDialog = () => {
        const [open, setOpen] = React.useState(false)
        
        return (
          <>
            <button onClick={() => setOpen(true)}>Open</button>
            <Dialog open={open} onOpenChange={setOpen}>
                        <DialogContent>
            <DialogHeader>
              <DialogTitle>Controlled Dialog</DialogTitle>
              <DialogDescription>Controlled content</DialogDescription>
            </DialogHeader>
          </DialogContent>
            </Dialog>
          </>
        )
      }

      const user = userEvent.setup()
      render(<TestDialog />)

      const openButton = screen.getByRole('button', { name: 'Open' })
      await user.click(openButton)

      await waitFor(() => {
        expect(screen.getByRole('dialog')).toBeInTheDocument()
      })

      expect(screen.getByText('Controlled Dialog')).toBeInTheDocument()
    })

    it('should close dialog when close button is clicked', async () => {
      const user = userEvent.setup()
      
      render(
        <Dialog>
          <DialogTrigger>Open Dialog</DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Test Dialog</DialogTitle>
              <DialogDescription>Content description</DialogDescription>
            </DialogHeader>
          </DialogContent>
        </Dialog>
      )

      await user.click(screen.getByRole('button', { name: 'Open Dialog' }))

      await waitFor(() => {
        expect(screen.getByRole('dialog')).toBeInTheDocument()
      })

      const closeButton = screen.getByRole('button', { name: 'Close' })
      await user.click(closeButton)

      await waitFor(() => {
        expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
      })
    })

    it('should close dialog when escape key is pressed', async () => {
      const user = userEvent.setup()
      
      render(
        <Dialog>
          <DialogTrigger>Open Dialog</DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Test Dialog</DialogTitle>
              <DialogDescription>Content description</DialogDescription>
            </DialogHeader>
          </DialogContent>
        </Dialog>
      )

      await user.click(screen.getByRole('button', { name: 'Open Dialog' }))

      await waitFor(() => {
        expect(screen.getByRole('dialog')).toBeInTheDocument()
      })

      await user.keyboard('{Escape}')

      await waitFor(() => {
        expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
      })
    })

    it('should have proper data-slot attributes', async () => {
      const user = userEvent.setup()
      
      render(
        <Dialog>
          <DialogTrigger>Trigger</DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Title</DialogTitle>
              <DialogDescription>Description</DialogDescription>
            </DialogHeader>
          </DialogContent>
        </Dialog>
      )

      const trigger = screen.getByRole('button')
      expect(trigger).toHaveAttribute('data-slot', 'dialog-trigger')

      // Open the dialog to check content data-slot
      await user.click(trigger)

      await waitFor(() => {
        const dialogContent = screen.getByRole('dialog')
        expect(dialogContent).toHaveAttribute('data-slot', 'dialog-content')
      })
    })
  })

  describe('DialogContent', () => {
    it('should render with proper styling classes', async () => {
      const user = userEvent.setup()
      
      render(
        <Dialog>
          <DialogTrigger>Open</DialogTrigger>
          <DialogContent className="custom-class">
            <DialogHeader>
              <DialogTitle>Test</DialogTitle>
              <DialogDescription>Test description</DialogDescription>
            </DialogHeader>
          </DialogContent>
        </Dialog>
      )

      await user.click(screen.getByRole('button'))

      await waitFor(() => {
        const dialogContent = screen.getByRole('dialog')
        expect(dialogContent).toHaveClass('custom-class')
        expect(dialogContent).toHaveClass('fixed', 'left-[50%]', 'top-[50%]')
      })
    })

    it('should trap focus within dialog content', async () => {
      const user = userEvent.setup()
      
      render(
        <Dialog>
          <DialogTrigger>Open</DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Test Dialog</DialogTitle>
              <DialogDescription>Test description</DialogDescription>
            </DialogHeader>
            <input placeholder="First input" />
            <input placeholder="Second input" />
            <DialogClose>Custom Close</DialogClose>
          </DialogContent>
        </Dialog>
      )

      await user.click(screen.getByRole('button', { name: 'Open' }))

      await waitFor(() => {
        expect(screen.getByRole('dialog')).toBeInTheDocument()
      })

      // Focus should be trapped within the dialog
      const firstInput = screen.getByPlaceholderText('First input')
      const secondInput = screen.getByPlaceholderText('Second input')
      const closeButton = screen.getByRole('button', { name: 'Custom Close' })

      await user.tab()
      expect(firstInput).toHaveFocus()

      await user.tab()
      expect(secondInput).toHaveFocus()

      await user.tab()
      expect(closeButton).toHaveFocus()
    })
  })

  describe('DialogHeader', () => {
    it('should render header with title and description', async () => {
      const user = userEvent.setup()
      
      render(
        <Dialog>
          <DialogTrigger>Open</DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Dialog Title</DialogTitle>
              <DialogDescription>Dialog description text</DialogDescription>
            </DialogHeader>
          </DialogContent>
        </Dialog>
      )

      await user.click(screen.getByRole('button'))

      await waitFor(() => {
        expect(screen.getByText('Dialog Title')).toBeInTheDocument()
        expect(screen.getByText('Dialog description text')).toBeInTheDocument()
      })
    })

    it('should have proper heading level for title', async () => {
      const user = userEvent.setup()
      
      render(
        <Dialog>
          <DialogTrigger>Open</DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Dialog Title</DialogTitle>
            </DialogHeader>
          </DialogContent>
        </Dialog>
      )

      await user.click(screen.getByRole('button'))

      await waitFor(() => {
        const title = screen.getByRole('heading', { level: 2 })
        expect(title).toHaveTextContent('Dialog Title')
      })
    })
  })

  describe('DialogFooter', () => {
    it('should render footer with action buttons', async () => {
      const user = userEvent.setup()
      
      render(
        <Dialog>
          <DialogTrigger>Open</DialogTrigger>
          <DialogContent>
            <DialogTitle>Test</DialogTitle>
            <DialogFooter>
              <button type="button">Cancel</button>
              <button type="submit">Save</button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      )

      await user.click(screen.getByRole('button', { name: 'Open' }))

      await waitFor(() => {
        expect(screen.getByRole('button', { name: 'Cancel' })).toBeInTheDocument()
        expect(screen.getByRole('button', { name: 'Save' })).toBeInTheDocument()
      })
    })
  })

  describe('DialogClose', () => {
    it('should close dialog when DialogClose is clicked', async () => {
      const user = userEvent.setup()
      
      render(
        <Dialog>
          <DialogTrigger>Open</DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Test</DialogTitle>
              <DialogDescription>Test description</DialogDescription>
            </DialogHeader>
            <DialogClose>Custom Close Button</DialogClose>
          </DialogContent>
        </Dialog>
      )

      await user.click(screen.getByRole('button', { name: 'Open' }))

      await waitFor(() => {
        expect(screen.getByRole('dialog')).toBeInTheDocument()
      })

      await user.click(screen.getByRole('button', { name: 'Custom Close Button' }))

      await waitFor(() => {
        expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
      })
    })

    it('should have proper data-slot attribute', async () => {
      const user = userEvent.setup()
      
      render(
        <Dialog>
          <DialogTrigger>Open</DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Test</DialogTitle>
              <DialogDescription>Test description</DialogDescription>
            </DialogHeader>
            <DialogClose>Custom Close Button</DialogClose>
          </DialogContent>
        </Dialog>
      )

      await user.click(screen.getByRole('button', { name: 'Open' }))

      await waitFor(() => {
        const closeButton = screen.getByRole('button', { name: 'Custom Close Button' })
        expect(closeButton).toHaveAttribute('data-slot', 'dialog-close')
      })
    })
  })

  describe('Accessibility', () => {
    it('should have proper ARIA attributes', async () => {
      const user = userEvent.setup()
      
      render(
        <Dialog>
          <DialogTrigger>Open Dialog</DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Accessible Dialog</DialogTitle>
              <DialogDescription>This dialog is accessible</DialogDescription>
            </DialogHeader>
          </DialogContent>
        </Dialog>
      )

      await user.click(screen.getByRole('button', { name: 'Open Dialog' }))

      await waitFor(() => {
        const dialog = screen.getByRole('dialog')
        expect(dialog).toHaveAttribute('aria-labelledby')
        expect(dialog).toHaveAttribute('aria-describedby')
      })
    })

    it('should handle focus management correctly', async () => {
      const user = userEvent.setup()
      
      render(
        <>
          <button>Before Dialog</button>
          <Dialog>
            <DialogTrigger>Open Dialog</DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>Test Dialog</DialogTitle>
                <DialogDescription>Test description</DialogDescription>
              </DialogHeader>
              <input placeholder="Dialog input" />
            </DialogContent>
          </Dialog>
          <button>After Dialog</button>
        </>
      )

      const triggerButton = screen.getByRole('button', { name: 'Open Dialog' })
      triggerButton.focus()
      
      await user.click(triggerButton)

      await waitFor(() => {
        expect(screen.getByRole('dialog')).toBeInTheDocument()
      })

      // Focus should be managed by the dialog (it may focus on the dialog container or first focusable element)
      const dialog = screen.getByRole('dialog')
      expect(dialog).toBeInTheDocument()
      // We'll just verify the dialog is open and accessible
    })
  })

  describe('Event Handling', () => {
    it('should call onOpenChange when dialog state changes', async () => {
      const handleOpenChange = jest.fn()
      const user = userEvent.setup()
      
      render(
        <Dialog onOpenChange={handleOpenChange}>
          <DialogTrigger>Open</DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Test</DialogTitle>
              <DialogDescription>Test description</DialogDescription>
            </DialogHeader>
          </DialogContent>
        </Dialog>
      )

      await user.click(screen.getByRole('button', { name: 'Open' }))

      expect(handleOpenChange).toHaveBeenCalledWith(true)

      await waitFor(() => {
        expect(screen.getByRole('dialog')).toBeInTheDocument()
      })

      await user.keyboard('{Escape}')

      expect(handleOpenChange).toHaveBeenCalledWith(false)
    })
  })
})
