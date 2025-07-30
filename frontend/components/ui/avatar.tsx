import * as React from "react"
import { cn } from "@/lib/utils"

interface AvatarProps extends React.HTMLAttributes<HTMLDivElement> {
  src?: string
  alt?: string
  size?: number
  children?: React.ReactNode
}

const Avatar = React.forwardRef<HTMLDivElement, AvatarProps>(
  ({ className, src, alt, size = 32, children, ...props }, ref) => {
    const [hasError, setHasError] = React.useState(false)

    const handleImageError = () => {
      setHasError(true)
    }

    return (
      <div
        ref={ref}
        className={cn(
          "relative flex shrink-0 overflow-hidden rounded-full bg-muted",
          className
        )}
        style={{ width: size, height: size }}
        {...props}
      >
        {src && !hasError ? (
          <img
            src={src}
            alt={alt}
            className="aspect-square h-full w-full object-cover"
            onError={handleImageError}
          />
        ) : (
          <div className="flex h-full w-full items-center justify-center bg-muted text-muted-foreground">
            {children || <span className="text-xs font-medium">?</span>}
          </div>
        )}
      </div>
    )
  }
)

Avatar.displayName = "Avatar"

export { Avatar } 