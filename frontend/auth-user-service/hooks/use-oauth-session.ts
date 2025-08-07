import { useSession } from "next-auth/react"
import { useEffect, useState } from "react"

interface User {
  id: string
  email: string
  username: string
  firstName?: string
  lastName?: string
  professionalTitle?: string
  bio?: string
  location?: string
  avatarUrl?: string
  isActive: boolean
  isAdmin: boolean
  lastLoginAt?: string
}

interface UseOAuthSessionReturn {
  user: User | null
  accessToken: string | null
  isLoading: boolean
  isAuthenticated: boolean
  validateToken: () => Promise<boolean>
}

export function useOAuthSession(): UseOAuthSessionReturn {
  const { data: session, status } = useSession()
  const [user, setUser] = useState<User | null>(null)
  const [isValidating, setIsValidating] = useState(false)

  const isLoading = status === "loading" || isValidating
  const isAuthenticated = !!session?.accessToken && !!user
  const accessToken = session?.accessToken || null

  const validateToken = async (): Promise<boolean> => {
    if (!accessToken) return false

    setIsValidating(true)
    try {
      const response = await fetch(`${process.env.NEXT_PUBLIC_BACKEND_URL}/api/oauth/me`, {
        headers: {
          'Authorization': `Bearer ${accessToken}`,
          'Content-Type': 'application/json',
        },
      })

      if (response.ok) {
        const userData = await response.json()
        setUser({
          id: userData.userId,
          email: userData.email,
          username: userData.username,
          firstName: userData.firstName,
          lastName: userData.lastName,
          professionalTitle: userData.professionalTitle,
          bio: userData.bio,
          location: userData.location,
          avatarUrl: userData.avatarUrl,
          isActive: userData.isActive,
          isAdmin: userData.isAdmin,
          lastLoginAt: userData.lastLoginAt,
        })
        return true
      } else {
        setUser(null)
        return false
      }
    } catch (error) {
      console.error('Error validating token:', error)
      setUser(null)
      return false
    } finally {
      setIsValidating(false)
    }
  }

  useEffect(() => {
    if (accessToken) {
      validateToken()
    } else {
      setUser(null)
    }
  }, [accessToken])

  return {
    user,
    accessToken,
    isLoading,
    isAuthenticated,
    validateToken,
  }
}