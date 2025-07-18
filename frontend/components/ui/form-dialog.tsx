"use client"

import * as React from "react"
import { Plus } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { Checkbox } from "@/components/ui/checkbox"
import { 
  Dialog, 
  DialogContent, 
  DialogHeader, 
  DialogTitle, 
  DialogTrigger,
  DialogDescription 
} from "@/components/ui/dialog"

export type FormFieldType = 'text' | 'textarea' | 'date' | 'checkbox' | 'url'

export interface FormField {
  name: string
  label: string
  type: FormFieldType
  placeholder?: string
  required?: boolean
  rows?: number
  dependsOn?: string // For conditional fields
  dependsOnValue?: any // Value that dependsOn field must have
}

export interface FormDialogProps<T = Record<string, any>> {
  // Dialog props
  title: string
  description: string
  triggerLabel: string
  isOpen: boolean
  onOpenChange: (open: boolean) => void
  
  // Form props
  fields: FormField[]
  formData: T
  onFormChange: (field: string, value: any) => void
  onSubmit: () => void
  onCancel: () => void
  
  // State props
  isEdit: boolean
  loading: boolean
  error: string | null
  
  // Optional customization
  maxWidth?: string
  submitLabel?: string
  cancelLabel?: string
}

// Separate memoized form component to prevent re-renders
const FormContent = React.memo<{
  fields: FormField[]
  formData: Record<string, any>
  onFormChange: (field: string, value: any) => void
  onSubmit: () => void
  onCancel: () => void
  isEdit: boolean
  loading: boolean
  error: string | null
  submitLabel?: string
  cancelLabel?: string
}>(({ 
  fields, 
  formData, 
  onFormChange, 
  onSubmit, 
  onCancel, 
  isEdit, 
  loading, 
  error,
  submitLabel,
  cancelLabel
}) => {
  const shouldShowField = React.useCallback((field: FormField) => {
    if (!field.dependsOn) return true
    return formData[field.dependsOn] === field.dependsOnValue
  }, [formData])

  const getFieldId = React.useCallback((fieldName: string) => {
    return isEdit ? `edit-${fieldName}` : `add-${fieldName}`
  }, [isEdit])

  const renderField = React.useCallback((field: FormField) => {
    const fieldId = getFieldId(field.name)
    const value = formData[field.name] || ''

    switch (field.type) {
      case 'textarea':
        return (
          <Textarea
            id={fieldId}
            value={value}
            onChange={(e) => onFormChange(field.name, e.target.value)}
            placeholder={field.placeholder}
            rows={field.rows || 3}
            disabled={loading}
          />
        )
      
      case 'checkbox':
        return (
          <div className="flex items-center space-x-2">
            <Checkbox
              id={fieldId}
              checked={!!value}
              onCheckedChange={(checked) => onFormChange(field.name, checked)}
              disabled={loading}
            />
            <Label htmlFor={fieldId}>{field.label}</Label>
          </div>
        )
      
      case 'date':
        return (
          <Input
            id={fieldId}
            type="date"
            value={value}
            onChange={(e) => onFormChange(field.name, e.target.value)}
            disabled={loading}
          />
        )
      
      case 'url':
      case 'text':
      default:
        return (
          <Input
            id={fieldId}
            type={field.type === 'url' ? 'url' : 'text'}
            value={value}
            onChange={(e) => onFormChange(field.name, e.target.value)}
            placeholder={field.placeholder}
            disabled={loading}
          />
        )
    }
  }, [formData, onFormChange, loading, getFieldId])

  return (
    <div className="space-y-4">
      {error && (
        <div className="p-3 bg-red-50 border border-red-200 rounded-lg">
          <p className="text-sm text-red-600">{error}</p>
        </div>
      )}
      
      {fields.map((field) => {
        if (!shouldShowField(field)) return null
        
        // For checkbox fields, render differently
        if (field.type === 'checkbox') {
          return (
            <div key={field.name}>
              {renderField(field)}
            </div>
          )
        }

        return (
          <div key={field.name} className="space-y-2">
            <Label htmlFor={getFieldId(field.name)}>
              {field.label} {field.required && '*'}
            </Label>
            {renderField(field)}
          </div>
        )
      })}

      <div className="flex gap-2 pt-4">
        <Button
          onClick={onSubmit}
          disabled={loading}
          className="flex-1"
        >
          {loading ? 'Saving...' : submitLabel || (isEdit ? 'Update' : 'Add')}
        </Button>
        <Button
          variant="outline"
          onClick={onCancel}
          disabled={loading}
        >
          {cancelLabel || 'Cancel'}
        </Button>
      </div>
    </div>
  )
})

FormContent.displayName = 'FormContent'

export function FormDialog<T = Record<string, any>>({
  title,
  description,
  triggerLabel,
  isOpen,
  onOpenChange,
  fields,
  formData,
  onFormChange,
  onSubmit,
  onCancel,
  isEdit,
  loading,
  error,
  maxWidth = "max-w-md",
  submitLabel,
  cancelLabel
}: FormDialogProps<T>) {
  return (
    <Dialog open={isOpen} onOpenChange={onOpenChange}>
      <DialogTrigger asChild>
        <Button 
          className="w-full sm:w-auto px-4 py-2 bg-app-blue hover:bg-app-blue-hover text-white text-sm font-normal rounded-lg flex justify-center items-center gap-2"
        >
          <Plus className="w-[14px] h-[14px]" />
          {triggerLabel}
        </Button>
      </DialogTrigger>
      <DialogContent className={maxWidth}>
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          <DialogDescription>
            {description}
          </DialogDescription>
        </DialogHeader>
        <FormContent
          fields={fields}
          formData={formData}
          onFormChange={onFormChange}
          onSubmit={onSubmit}
          onCancel={onCancel}
          isEdit={isEdit}
          loading={loading}
          error={error}
          submitLabel={submitLabel}
          cancelLabel={cancelLabel}
        />
      </DialogContent>
    </Dialog>
  )
}

// Helper function to create a simple dialog for editing (without trigger)
export function EditFormDialog<T = Record<string, any>>({
  title,
  description,
  isOpen,
  onOpenChange,
  fields,
  formData,
  onFormChange,
  onSubmit,
  onCancel,
  loading,
  error,
  maxWidth = "max-w-md",
  submitLabel,
  cancelLabel
}: Omit<FormDialogProps<T>, 'triggerLabel' | 'isEdit'>) {
  return (
    <Dialog open={isOpen} onOpenChange={onOpenChange}>
      <DialogContent className={maxWidth}>
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          <DialogDescription>
            {description}
          </DialogDescription>
        </DialogHeader>
        <FormContent
          fields={fields}
          formData={formData}
          onFormChange={onFormChange}
          onSubmit={onSubmit}
          onCancel={onCancel}
          isEdit={true}
          loading={loading}
          error={error}
          submitLabel={submitLabel}
          cancelLabel={cancelLabel}
        />
      </DialogContent>
    </Dialog>
  )
} 