import * as React from "react"
import { cn } from "@/lib/utils"
import { CheckCircle, XCircle, Info, AlertTriangle } from "lucide-react"

interface ToastContextType {
  showToast: (message: string, type?: ToastType) => void
}

type ToastType = 'success' | 'error' | 'info' | 'warning'

interface Toast {
  id: string
  message: string
  type: ToastType
}

const ToastContext = React.createContext<ToastContextType | undefined>(undefined)

export const useToast = () => {
  const context = React.useContext(ToastContext)
  if (!context) {
    // Return no-op functions if context is not available
    return {
      showToast: () => {}
    }
  }
  return context
}

interface ToastItemProps {
  toast: Toast
  onRemove: (id: string) => void
}

const ToastItem: React.FC<ToastItemProps> = ({ toast, onRemove }) => {
  React.useEffect(() => {
    const timer = setTimeout(() => {
      onRemove(toast.id)
    }, 4000)

    return () => clearTimeout(timer)
  }, [toast.id, onRemove])

  const getIcon = () => {
    switch (toast.type) {
      case 'success':
        return <CheckCircle className="h-4 w-4 text-green-600" />
      case 'error':
        return <XCircle className="h-4 w-4 text-red-600" />
      case 'warning':
        return <AlertTriangle className="h-4 w-4 text-yellow-600" />
      default:
        return <Info className="h-4 w-4 text-blue-600" />
    }
  }

  const getBackgroundColor = () => {
    switch (toast.type) {
      case 'success':
        return 'bg-green-50 border-green-200'
      case 'error':
        return 'bg-red-50 border-red-200'
      case 'warning':
        return 'bg-yellow-50 border-yellow-200'
      default:
        return 'bg-blue-50 border-blue-200'
    }
  }

  return (
    <div
      className={cn(
        "flex items-center gap-2 p-3 rounded-md border shadow-sm animate-in slide-in-from-right-full duration-300",
        getBackgroundColor()
      )}
    >
      {getIcon()}
      <span className="text-sm font-medium text-gray-900">{toast.message}</span>
      <button
        onClick={() => onRemove(toast.id)}
        className="ml-auto text-gray-400 hover:text-gray-600"
      >
        <XCircle className="h-4 w-4" />
      </button>
    </div>
  )
}

interface ToastProviderProps {
  children: React.ReactNode
}

let globalShowToast: ((message: string, type?: ToastType) => void) | null = null

export const ToastProvider: React.FC<ToastProviderProps> = ({ children }) => {
  const [toasts, setToasts] = React.useState<Toast[]>([])

  const showToast = React.useCallback((message: string, type: ToastType = 'info') => {
    const id = Math.random().toString(36).substr(2, 9)
    const newToast: Toast = { id, message, type }
    setToasts(prev => [...prev, newToast])
  }, [])

  const removeToast = React.useCallback((id: string) => {
    setToasts(prev => prev.filter(toast => toast.id !== id))
  }, [])

  // Set global reference for message object
  React.useEffect(() => {
    globalShowToast = showToast
    
    // Listen for custom events from message object
    const handleToastEvent = (event: CustomEvent) => {
      const { message, type } = event.detail
      showToast(message, type)
    }

    window.addEventListener('show-toast', handleToastEvent as EventListener)

    return () => {
      window.removeEventListener('show-toast', handleToastEvent as EventListener)
      if (globalShowToast === showToast) {
        globalShowToast = null
      }
    }
  }, [showToast])

  return (
    <ToastContext.Provider value={{ showToast }}>
      {children}
      <div className="fixed top-4 right-4 z-50 flex flex-col gap-2 max-w-sm w-full">
        {toasts.map(toast => (
          <ToastItem
            key={toast.id}
            toast={toast}
            onRemove={removeToast}
          />
        ))}
      </div>
    </ToastContext.Provider>
  )
}

// Simple message object similar to antd for easy replacement
export const message = {
  success: (msg: string) => {
    if (globalShowToast) {
      globalShowToast(msg, 'success')
    } else {
      const event = new CustomEvent('show-toast', { 
        detail: { message: msg, type: 'success' } 
      })
      window.dispatchEvent(event)
    }
  },
  error: (msg: string) => {
    if (globalShowToast) {
      globalShowToast(msg, 'error')
    } else {
      const event = new CustomEvent('show-toast', { 
        detail: { message: msg, type: 'error' } 
      })
      window.dispatchEvent(event)
    }
  },
  info: (msg: string) => {
    if (globalShowToast) {
      globalShowToast(msg, 'info')
    } else {
      const event = new CustomEvent('show-toast', { 
        detail: { message: msg, type: 'info' } 
      })
      window.dispatchEvent(event)
    }
  },
  warning: (msg: string) => {
    if (globalShowToast) {
      globalShowToast(msg, 'warning')
    } else {
      const event = new CustomEvent('show-toast', { 
        detail: { message: msg, type: 'warning' } 
      })
      window.dispatchEvent(event)
    }
  }
} 