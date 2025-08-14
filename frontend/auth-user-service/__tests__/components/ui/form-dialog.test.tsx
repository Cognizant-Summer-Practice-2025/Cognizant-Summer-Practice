import React from 'react'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { FormDialog, FormField } from '@/components/ui/form-dialog'

describe('FormDialog', () => {
  const mockFields: FormField[] = [
    {
      name: 'title',
      label: 'Title',
      type: 'text',
      placeholder: 'Enter title',
      required: true
    },
    {
      name: 'description',
      label: 'Description',
      type: 'textarea',
      placeholder: 'Enter description',
      rows: 3
    },
    {
      name: 'url',
      label: 'Website URL',
      type: 'url',
      placeholder: 'https://example.com'
    },
    {
      name: 'featured',
      label: 'Featured',
      type: 'checkbox'
    },
    {
      name: 'publishDate',
      label: 'Publish Date',
      type: 'date'
    }
  ]

  const defaultProps = {
    title: 'Test Form Dialog',
    description: 'This is a test form dialog',
    triggerLabel: 'Open Form',
    isOpen: false,
    onOpenChange: jest.fn(),
    fields: mockFields,
    formData: {},
    onFormChange: jest.fn(),
    onSubmit: jest.fn(),
    onCancel: jest.fn(),
    isEdit: false,
    loading: false,
    error: null
  }

  beforeEach(() => {
    jest.clearAllMocks()
  })

  it('should render trigger button', () => {
    render(<FormDialog {...defaultProps} />)
    
    expect(screen.getByRole('button', { name: 'Open Form' })).toBeInTheDocument()
  })

  it('should render form fields when dialog is open', () => {
    render(<FormDialog {...defaultProps} isOpen={true} />)
    
    expect(screen.getByRole('dialog')).toBeInTheDocument()
    expect(screen.getByText('Test Form Dialog')).toBeInTheDocument()
    expect(screen.getByText('This is a test form dialog')).toBeInTheDocument()
    
    // Check all form fields are rendered
    expect(screen.getByLabelText('Title *')).toBeInTheDocument()
    expect(screen.getByLabelText('Description')).toBeInTheDocument()
    expect(screen.getByLabelText('Website URL')).toBeInTheDocument()
    expect(screen.getByLabelText('Featured')).toBeInTheDocument()
    expect(screen.getByLabelText('Publish Date')).toBeInTheDocument()
  })

  it('should show correct button text for create mode', () => {
    render(<FormDialog {...defaultProps} isOpen={true} />)
    
    expect(screen.getByRole('button', { name: 'Create' })).toBeInTheDocument()
  })

  it('should show correct button text for edit mode', () => {
    render(<FormDialog {...defaultProps} isOpen={true} isEdit={true} />)
    
    expect(screen.getByRole('button', { name: 'Save Changes' })).toBeInTheDocument()
  })

  it('should call onFormChange when input values change', async () => {
    const user = userEvent.setup()
    const handleFormChange = jest.fn()
    
    render(
      <FormDialog 
        {...defaultProps} 
        isOpen={true} 
        onFormChange={handleFormChange}
      />
    )
    
    const titleInput = screen.getByLabelText('Title *')
    await user.type(titleInput, 'Test Title')
    
    expect(handleFormChange).toHaveBeenCalledWith('title', 'Test Title')
  })

  it('should call onFormChange when textarea values change', async () => {
    const user = userEvent.setup()
    const handleFormChange = jest.fn()
    
    render(
      <FormDialog 
        {...defaultProps} 
        isOpen={true} 
        onFormChange={handleFormChange}
      />
    )
    
    const descriptionTextarea = screen.getByLabelText('Description')
    await user.type(descriptionTextarea, 'Test Description')
    
    expect(handleFormChange).toHaveBeenCalledWith('description', 'Test Description')
  })

  it('should call onFormChange when checkbox is toggled', async () => {
    const user = userEvent.setup()
    const handleFormChange = jest.fn()
    
    render(
      <FormDialog 
        {...defaultProps} 
        isOpen={true} 
        onFormChange={handleFormChange}
      />
    )
    
    const checkbox = screen.getByLabelText('Featured')
    await user.click(checkbox)
    
    expect(handleFormChange).toHaveBeenCalledWith('featured', true)
  })

  it('should call onSubmit when form is submitted', async () => {
    const user = userEvent.setup()
    const handleSubmit = jest.fn()
    
    render(
      <FormDialog 
        {...defaultProps} 
        isOpen={true} 
        onSubmit={handleSubmit}
      />
    )
    
    const submitButton = screen.getByRole('button', { name: 'Create' })
    await user.click(submitButton)
    
    expect(handleSubmit).toHaveBeenCalledTimes(1)
  })

  it('should call onCancel when cancel button is clicked', async () => {
    const user = userEvent.setup()
    const handleCancel = jest.fn()
    
    render(
      <FormDialog 
        {...defaultProps} 
        isOpen={true} 
        onCancel={handleCancel}
      />
    )
    
    const cancelButton = screen.getByRole('button', { name: 'Cancel' })
    await user.click(cancelButton)
    
    expect(handleCancel).toHaveBeenCalledTimes(1)
  })

  it('should disable submit button when loading', () => {
    render(
      <FormDialog 
        {...defaultProps} 
        isOpen={true} 
        loading={true}
      />
    )
    
    const submitButton = screen.getByRole('button', { name: 'Creating...' })
    expect(submitButton).toBeDisabled()
  })

  it('should show loading text when submitting in edit mode', () => {
    render(
      <FormDialog 
        {...defaultProps} 
        isOpen={true} 
        isEdit={true} 
        loading={true}
      />
    )
    
    const submitButton = screen.getByRole('button', { name: 'Saving...' })
    expect(submitButton).toBeDisabled()
  })

  it('should display error message when error is provided', () => {
    render(
      <FormDialog 
        {...defaultProps} 
        isOpen={true} 
        error="Something went wrong"
      />
    )
    
    expect(screen.getByText('Something went wrong')).toBeInTheDocument()
  })

  it('should populate form fields with existing data', () => {
    const formData = {
      title: 'Existing Title',
      description: 'Existing Description',
      featured: true
    }
    
    render(
      <FormDialog 
        {...defaultProps} 
        isOpen={true} 
        formData={formData}
      />
    )
    
    expect(screen.getByDisplayValue('Existing Title')).toBeInTheDocument()
    expect(screen.getByDisplayValue('Existing Description')).toBeInTheDocument()
    expect(screen.getByRole('checkbox', { name: 'Featured' })).toBeChecked()
  })

  it('should handle conditional fields based on dependencies', () => {
    const fieldsWithDependency: FormField[] = [
      {
        name: 'hasWebsite',
        label: 'Has Website',
        type: 'checkbox'
      },
      {
        name: 'websiteUrl',
        label: 'Website URL',
        type: 'url',
        dependsOn: 'hasWebsite',
        dependsOnValue: true
      }
    ]
    
    const formData = { hasWebsite: false }
    
    render(
      <FormDialog 
        {...defaultProps} 
        isOpen={true} 
        fields={fieldsWithDependency}
        formData={formData}
      />
    )
    
    // Website URL field should not be visible when hasWebsite is false
    expect(screen.queryByLabelText('Website URL')).not.toBeInTheDocument()
  })

  it('should show conditional fields when dependency is met', () => {
    const fieldsWithDependency: FormField[] = [
      {
        name: 'hasWebsite',
        label: 'Has Website',
        type: 'checkbox'
      },
      {
        name: 'websiteUrl',
        label: 'Website URL',
        type: 'url',
        dependsOn: 'hasWebsite',
        dependsOnValue: true
      }
    ]
    
    const formData = { hasWebsite: true }
    
    render(
      <FormDialog 
        {...defaultProps} 
        isOpen={true} 
        fields={fieldsWithDependency}
        formData={formData}
      />
    )
    
    // Website URL field should be visible when hasWebsite is true
    expect(screen.getByLabelText('Website URL')).toBeInTheDocument()
  })

  it('should validate required fields visually', () => {
    render(<FormDialog {...defaultProps} isOpen={true} />)
    
    const requiredLabel = screen.getByLabelText('Title *')
    expect(requiredLabel).toBeInTheDocument()
    
    // Non-required fields should not have asterisk
    const optionalLabel = screen.getByLabelText('Description')
    expect(optionalLabel).toBeInTheDocument()
  })

  it('should handle date input type correctly', async () => {
    const user = userEvent.setup()
    const handleFormChange = jest.fn()
    
    render(
      <FormDialog 
        {...defaultProps} 
        isOpen={true} 
        onFormChange={handleFormChange}
      />
    )
    
    const dateInput = screen.getByLabelText('Publish Date')
    expect(dateInput).toHaveAttribute('type', 'date')
    
    await user.type(dateInput, '2023-12-25')
    expect(handleFormChange).toHaveBeenCalledWith('publishDate', '2023-12-25')
  })

  it('should handle url input type correctly', async () => {
    const user = userEvent.setup()
    const handleFormChange = jest.fn()
    
    render(
      <FormDialog 
        {...defaultProps} 
        isOpen={true} 
        onFormChange={handleFormChange}
      />
    )
    
    const urlInput = screen.getByLabelText('Website URL')
    expect(urlInput).toHaveAttribute('type', 'url')
    
    await user.type(urlInput, 'https://example.com')
    expect(handleFormChange).toHaveBeenCalledWith('url', 'https://example.com')
  })

  it('should set textarea rows correctly', () => {
    render(<FormDialog {...defaultProps} isOpen={true} />)
    
    const textarea = screen.getByLabelText('Description')
    expect(textarea).toHaveAttribute('rows', '3')
  })

  it('should call onOpenChange when dialog state changes', async () => {
    const user = userEvent.setup()
    const handleOpenChange = jest.fn()
    
    render(
      <FormDialog 
        {...defaultProps} 
        isOpen={true} 
        onOpenChange={handleOpenChange}
      />
    )
    
    // Close dialog using the X button
    const closeButton = screen.getByRole('button', { name: 'Close' })
    await user.click(closeButton)
    
    expect(handleOpenChange).toHaveBeenCalledWith(false)
  })

  it('should have proper accessibility attributes', () => {
    render(<FormDialog {...defaultProps} isOpen={true} />)
    
    const dialog = screen.getByRole('dialog')
    expect(dialog).toBeInTheDocument()
    
    // Form should have proper labels
    expect(screen.getByLabelText('Title *')).toBeInTheDocument()
    expect(screen.getByLabelText('Description')).toBeInTheDocument()
  })

  it('should handle empty fields array', () => {
    render(
      <FormDialog 
        {...defaultProps} 
        isOpen={true} 
        fields={[]}
      />
    )
    
    expect(screen.getByRole('dialog')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Create' })).toBeInTheDocument()
  })
})
