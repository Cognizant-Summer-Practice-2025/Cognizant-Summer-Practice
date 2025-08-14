import React from 'react'
import { render, screen, waitFor, act } from '@testing-library/react'
import { useSession } from 'next-auth/react'
import { UserProvider, useUser } from '@/lib/contexts/user-context'
import { getUserByEmail, updateUser } from '@/lib/user/api'
import { UserInjectionService } from '@/lib/services/user-injection-service'

jest.mock('next-auth/react')
jest.mock('@/lib/user/api')
jest.mock('@/lib/services/user-injection-service')

const mockUseSession = useSession as jest.MockedFunction<typeof useSession>
const mockGetUserByEmail = getUserByEmail as jest.MockedFunction<typeof getUserByEmail>
const mockUpdateUser = updateUser as jest.MockedFunction<typeof updateUser>
const mockUserInjectionService = UserInjectionService as jest.Mocked<typeof UserInjectionService>

// Test component to access context
const TestComponent = () => {
  const { user, loading, error, refetchUser, updateUserData } = useUser()
  
  return (
    <div>
      <div data-testid="user">{user ? JSON.stringify(user) : 'null'}</div>
      <div data-testid="loading">{loading.toString()}</div>
      <div data-testid="error">{error || 'null'}</div>
      <button data-testid="refetch" onClick={refetchUser}>Refetch</button>
      <button 
        data-testid="update" 
        onClick={() => updateUserData({ firstName: 'Updated' })}
      >
        Update
      </button>
    </div>
  )
}

const renderWithProvider = (children: React.ReactNode) => {
  return render(
    <UserProvider>
      {children}
    </UserProvider>
  )
}

describe('UserProvider', () => {
  beforeEach(() => {
    jest.clearAllMocks()
    // Mock sessionStorage
    Object.defineProperty(window, 'sessionStorage', {
      value: {
        getItem: jest.fn(),
        setItem: jest.fn(),
        removeItem: jest.fn(),
        clear: jest.fn(),
      },
      writable: true,
    })
  })

  it('should provide initial state when not authenticated', () => {
    mockUseSession.mockReturnValue({
      data: null,
      status: 'unauthenticated',
    })

    renderWithProvider(<TestComponent />)

    expect(screen.getByTestId('user')).toHaveTextContent('null')
    expect(screen.getByTestId('loading')).toHaveTextContent('false')
    expect(screen.getByTestId('error')).toHaveTextContent('null')
  })

  it('should show loading state when session is loading', () => {
    mockUseSession.mockReturnValue({
      data: null,
      status: 'loading',
    })

    renderWithProvider(<TestComponent />)

    expect(screen.getByTestId('loading')).toHaveTextContent('true')
  })

  it('should fetch user data when session is authenticated', async () => {
    const mockSession = {
      user: { email: 'test@example.com' },
      accessToken: 'mock-token'
    }

    const mockUser = {
      id: '123',
      email: 'test@example.com',
      username: 'testuser',
      firstName: 'Test',
      lastName: 'User',
      isActive: true,
      isAdmin: false,
      createdAt: '2023-01-01',
      updatedAt: '2023-01-01'
    }

    mockUseSession.mockReturnValue({
      data: mockSession,
      status: 'authenticated',
    })

    mockGetUserByEmail.mockResolvedValue(mockUser)
    mockUserInjectionService.injectUser.mockResolvedValue(undefined)

    const sessionStorageMock = jest.fn().mockReturnValue(
      JSON.stringify({ accessToken: 'mock-token' })
    )
    ;(window.sessionStorage.getItem as jest.Mock).mockImplementation(sessionStorageMock)

    renderWithProvider(<TestComponent />)

    await waitFor(() => {
      expect(screen.getByTestId('user')).toHaveTextContent(JSON.stringify(mockUser))
    })

    expect(mockGetUserByEmail).toHaveBeenCalledWith('test@example.com')
    expect(mockUserInjectionService.injectUser).toHaveBeenCalledWith(mockUser, 'mock-token')
  })

  it('should handle user fetch errors', async () => {
    const mockSession = {
      user: { email: 'test@example.com' },
      accessToken: 'mock-token'
    }

    mockUseSession.mockReturnValue({
      data: mockSession,
      status: 'authenticated',
    })

    const consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation()
    mockGetUserByEmail.mockRejectedValue(new Error('User not found'))

    renderWithProvider(<TestComponent />)

    await waitFor(() => {
      expect(screen.getByTestId('error')).toHaveTextContent('User not found')
    })

    expect(screen.getByTestId('loading')).toHaveTextContent('false')
    
    consoleErrorSpy.mockRestore()
  })

  it('should handle user injection service errors gracefully', async () => {
    const mockSession = {
      user: { email: 'test@example.com' },
      accessToken: 'mock-token'
    }

    const mockUser = {
      id: '123',
      email: 'test@example.com',
      username: 'testuser',
      isActive: true,
      isAdmin: false,
      createdAt: '2023-01-01',
      updatedAt: '2023-01-01'
    }

    mockUseSession.mockReturnValue({
      data: mockSession,
      status: 'authenticated',
    })

    mockGetUserByEmail.mockResolvedValue(mockUser)
    
    const consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation()
    mockUserInjectionService.injectUser.mockRejectedValue(new Error('Injection failed'))

    const sessionStorageMock = jest.fn().mockReturnValue(
      JSON.stringify({ accessToken: 'mock-token' })
    )
    ;(window.sessionStorage.getItem as jest.Mock).mockImplementation(sessionStorageMock)

    renderWithProvider(<TestComponent />)

    await waitFor(() => {
      expect(screen.getByTestId('user')).toHaveTextContent(JSON.stringify(mockUser))
    })

    // Should still show user even if injection fails
    expect(screen.getByTestId('error')).toHaveTextContent('null')
    expect(consoleErrorSpy).toHaveBeenCalledWith(
      '❌ Failed to inject user data to other services:',
      expect.any(Error)
    )
    
    consoleErrorSpy.mockRestore()
  })

  it('should refetch user data when refetchUser is called', async () => {
    const mockSession = {
      user: { email: 'test@example.com' },
      accessToken: 'mock-token'
    }

    const mockUser = {
      id: '123',
      email: 'test@example.com',
      username: 'testuser',
      isActive: true,
      isAdmin: false,
      createdAt: '2023-01-01',
      updatedAt: '2023-01-01'
    }

    mockUseSession.mockReturnValue({
      data: mockSession,
      status: 'authenticated',
    })

    mockGetUserByEmail.mockResolvedValue(mockUser)
    mockUserInjectionService.injectUser.mockResolvedValue(undefined)

    renderWithProvider(<TestComponent />)

    // Wait for initial load
    await waitFor(() => {
      expect(screen.getByTestId('user')).toHaveTextContent(JSON.stringify(mockUser))
    })

    // Clear mocks and click refetch
    mockGetUserByEmail.mockClear()
    
    act(() => {
      screen.getByTestId('refetch').click()
    })

    await waitFor(() => {
      expect(mockGetUserByEmail).toHaveBeenCalledWith('test@example.com')
    })
  })

  it('should update user data when updateUserData is called', async () => {
    const mockSession = {
      user: { email: 'test@example.com' },
      accessToken: 'mock-token'
    }

    const mockUser = {
      id: '123',
      email: 'test@example.com',
      username: 'testuser',
      firstName: 'Test',
      isActive: true,
      isAdmin: false,
      createdAt: '2023-01-01',
      updatedAt: '2023-01-01'
    }

    const updatedUser = {
      ...mockUser,
      firstName: 'Updated'
    }

    mockUseSession.mockReturnValue({
      data: mockSession,
      status: 'authenticated',
    })

    mockGetUserByEmail.mockResolvedValue(mockUser)
    mockUpdateUser.mockResolvedValue(updatedUser)
    mockUserInjectionService.injectUser.mockResolvedValue(undefined)

    renderWithProvider(<TestComponent />)

    // Wait for initial load
    await waitFor(() => {
      expect(screen.getByTestId('user')).toHaveTextContent(JSON.stringify(mockUser))
    })

    // Click update
    act(() => {
      screen.getByTestId('update').click()
    })

    await waitFor(() => {
      expect(screen.getByTestId('user')).toHaveTextContent(JSON.stringify(updatedUser))
    })

    expect(mockUpdateUser).toHaveBeenCalledWith('123', { firstName: 'Updated' })
    expect(mockUserInjectionService.injectUser).toHaveBeenCalledWith(updatedUser, undefined)
  })

  it('should handle update user errors', async () => {
    const mockSession = {
      user: { email: 'test@example.com' },
      accessToken: 'mock-token'
    }

    const mockUser = {
      id: '123',
      email: 'test@example.com',
      username: 'testuser',
      isActive: true,
      isAdmin: false,
      createdAt: '2023-01-01',
      updatedAt: '2023-01-01'
    }

    mockUseSession.mockReturnValue({
      data: mockSession,
      status: 'authenticated',
    })

    mockGetUserByEmail.mockResolvedValue(mockUser)
    mockUpdateUser.mockRejectedValue(new Error('Update failed'))

    renderWithProvider(<TestComponent />)

    // Wait for initial load
    await waitFor(() => {
      expect(screen.getByTestId('user')).toHaveTextContent(JSON.stringify(mockUser))
    })

    // Click update
    act(() => {
      screen.getByTestId('update').click()
    })

    await waitFor(() => {
      expect(screen.getByTestId('error')).toHaveTextContent('Update failed')
    })
  })

  it('should throw error when updateUserData is called without user ID', async () => {
    const mockSession = {
      user: { email: 'test@example.com' },
      accessToken: 'mock-token'
    }

    mockUseSession.mockReturnValue({
      data: mockSession,
      status: 'authenticated',
    })

    // Mock user without ID
    const mockUser = {
      email: 'test@example.com',
      username: 'testuser',
      isActive: true,
      isAdmin: false,
      createdAt: '2023-01-01',
      updatedAt: '2023-01-01'
    }

    mockGetUserByEmail.mockResolvedValue(mockUser as any)

    renderWithProvider(<TestComponent />)

    // Wait for initial load
    await waitFor(() => {
      expect(screen.getByTestId('user')).toHaveTextContent(JSON.stringify(mockUser))
    })

    // Click update
    act(() => {
      screen.getByTestId('update').click()
    })

    await waitFor(() => {
      expect(screen.getByTestId('error')).toHaveTextContent('No user ID available')
    })
  })

  it('should reset user state when session becomes unauthenticated', async () => {
    const mockSession = {
      user: { email: 'test@example.com' },
      accessToken: 'mock-token'
    }

    const mockUser = {
      id: '123',
      email: 'test@example.com',
      username: 'testuser',
      isActive: true,
      isAdmin: false,
      createdAt: '2023-01-01',
      updatedAt: '2023-01-01'
    }

    const { rerender } = render(
      <UserProvider>
        <TestComponent />
      </UserProvider>
    )

    // Initially authenticated
    mockUseSession.mockReturnValue({
      data: mockSession,
      status: 'authenticated',
    })

    mockGetUserByEmail.mockResolvedValue(mockUser)
    mockUserInjectionService.injectUser.mockResolvedValue(undefined)

    rerender(
      <UserProvider>
        <TestComponent />
      </UserProvider>
    )

    await waitFor(() => {
      expect(screen.getByTestId('user')).toHaveTextContent(JSON.stringify(mockUser))
    })

    // Change to unauthenticated
    mockUseSession.mockReturnValue({
      data: null,
      status: 'unauthenticated',
    })

    rerender(
      <UserProvider>
        <TestComponent />
      </UserProvider>
    )

    expect(screen.getByTestId('user')).toHaveTextContent('null')
    expect(screen.getByTestId('loading')).toHaveTextContent('false')
    expect(screen.getByTestId('error')).toHaveTextContent('null')
  })

  it('should not refetch user when session email is not available', async () => {
    const mockSession = {
      user: { email: null },
      accessToken: 'mock-token'
    }

    mockUseSession.mockReturnValue({
      data: mockSession,
      status: 'authenticated',
    })

    renderWithProvider(<TestComponent />)

    act(() => {
      screen.getByTestId('refetch').click()
    })

    expect(mockGetUserByEmail).not.toHaveBeenCalled()
  })
})

describe('useUser', () => {
  it('should throw error when used outside UserProvider', () => {
    const consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation()
    
    expect(() => {
      render(<TestComponent />)
    }).toThrow('useUser must be used within a UserProvider')
    
    consoleErrorSpy.mockRestore()
  })
})
