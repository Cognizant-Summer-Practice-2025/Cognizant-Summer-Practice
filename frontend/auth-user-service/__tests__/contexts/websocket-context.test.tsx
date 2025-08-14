import React from 'react'
import { render, screen, waitFor, act } from '@testing-library/react'
import * as signalR from '@microsoft/signalr'
import { WebSocketProvider, useWebSocket } from '@/lib/contexts/websocket-context'
import { useUser } from '@/lib/contexts/user-context'

jest.mock('@microsoft/signalr')
jest.mock('@/lib/contexts/user-context')

const mockUseUser = useUser as jest.MockedFunction<typeof useUser>
const mockSignalR = signalR as jest.Mocked<typeof signalR>

// Mock HubConnection
const mockHubConnection = {
  start: jest.fn(),
  stop: jest.fn(),
  invoke: jest.fn(),
  on: jest.fn(),
  onreconnecting: jest.fn(),
  onreconnected: jest.fn(),
  onclose: jest.fn(),
} as any

// Mock HubConnectionBuilder
const mockHubConnectionBuilder = {
  withUrl: jest.fn(),
  withAutomaticReconnect: jest.fn(),
  configureLogging: jest.fn(),
  build: jest.fn(),
} as any

// Test component to access context
const TestComponent = () => {
  const {
    connection,
    isConnected,
    isConnecting,
    onMessageReceived,
    onConversationUpdated,
    onUserPresenceUpdate,
    onMessageReadReceipt,
    onMessageDeleted,
    markMessageAsRead,
    deleteMessage,
    connect,
    disconnect
  } = useWebSocket()
  
  return (
    <div>
      <div data-testid="connection">{connection ? 'connected' : 'null'}</div>
      <div data-testid="is-connected">{isConnected.toString()}</div>
      <div data-testid="is-connecting">{isConnecting.toString()}</div>
      <button data-testid="connect" onClick={connect}>Connect</button>
      <button data-testid="disconnect" onClick={disconnect}>Disconnect</button>
      <button 
        data-testid="mark-read" 
        onClick={() => markMessageAsRead('msg1', 'user1')}
      >
        Mark Read
      </button>
      <button 
        data-testid="delete-message" 
        onClick={() => deleteMessage('msg1', 'user1')}
      >
        Delete Message
      </button>
      <button 
        data-testid="subscribe-message" 
        onClick={() => {
          const unsubscribe = onMessageReceived((msg) => console.log('Message:', msg))
          // Store unsubscribe function for testing
          ;(window as any).unsubscribeMessage = unsubscribe
        }}
      >
        Subscribe Message
      </button>
    </div>
  )
}

const renderWithProvider = (children: React.ReactNode) => {
  return render(
    <WebSocketProvider>
      {children}
    </WebSocketProvider>
  )
}

describe('WebSocketProvider', () => {
  beforeEach(() => {
    jest.clearAllMocks()
    process.env.NEXT_PUBLIC_MESSAGES_API_URL = 'http://localhost:5093'
    
    // Reset mocks
    mockHubConnectionBuilder.withUrl.mockReturnThis()
    mockHubConnectionBuilder.withAutomaticReconnect.mockReturnThis()
    mockHubConnectionBuilder.configureLogging.mockReturnThis()
    mockHubConnectionBuilder.build.mockReturnValue(mockHubConnection)
    
    mockSignalR.HubConnectionBuilder.mockImplementation(() => mockHubConnectionBuilder)
    mockSignalR.HttpTransportType = {
      WebSockets: 1,
      ServerSentEvents: 2,
      LongPolling: 4,
    } as any
    mockSignalR.LogLevel = {
      Information: 2,
    } as any

    mockHubConnection.start.mockResolvedValue(undefined)
    mockHubConnection.stop.mockResolvedValue(undefined)
    mockHubConnection.invoke.mockResolvedValue(undefined)
  })

  afterEach(() => {
    delete process.env.NEXT_PUBLIC_MESSAGES_API_URL
    // Clean up any stored unsubscribe functions
    delete (window as any).unsubscribeMessage
  })

  it('should provide initial state when no user', () => {
    mockUseUser.mockReturnValue({
      user: null,
      loading: false,
      error: null,
      refetchUser: jest.fn(),
      updateUserData: jest.fn(),
    })

    renderWithProvider(<TestComponent />)

    expect(screen.getByTestId('connection')).toHaveTextContent('null')
    expect(screen.getByTestId('is-connected')).toHaveTextContent('false')
    expect(screen.getByTestId('is-connecting')).toHaveTextContent('false')
  })

  it('should auto-connect when user is available', async () => {
    const mockUser = {
      id: '123',
      email: 'test@example.com',
      username: 'testuser',
      isActive: true,
      isAdmin: false,
      createdAt: '2023-01-01',
      updatedAt: '2023-01-01'
    }

    mockUseUser.mockReturnValue({
      user: mockUser,
      loading: false,
      error: null,
      refetchUser: jest.fn(),
      updateUserData: jest.fn(),
    })

    const consoleLogSpy = jest.spyOn(console, 'log').mockImplementation()

    renderWithProvider(<TestComponent />)

    await waitFor(() => {
      expect(screen.getByTestId('is-connected')).toHaveTextContent('true')
    })

    expect(mockHubConnectionBuilder.withUrl).toHaveBeenCalledWith(
      'http://localhost:5093/messagehub',
      expect.objectContaining({
        transport: 7, // WebSockets | ServerSentEvents | LongPolling
        skipNegotiation: false,
      })
    )

    expect(mockHubConnection.start).toHaveBeenCalled()
    expect(mockHubConnection.invoke).toHaveBeenCalledWith('JoinUserGroup', '123')
    expect(consoleLogSpy).toHaveBeenCalledWith('SignalR connected successfully')
    
    consoleLogSpy.mockRestore()
  })

  it('should handle connection errors gracefully', async () => {
    const mockUser = {
      id: '123',
      email: 'test@example.com',
      username: 'testuser',
      isActive: true,
      isAdmin: false,
      createdAt: '2023-01-01',
      updatedAt: '2023-01-01'
    }

    mockUseUser.mockReturnValue({
      user: mockUser,
      loading: false,
      error: null,
      refetchUser: jest.fn(),
      updateUserData: jest.fn(),
    })

    const consoleWarnSpy = jest.spyOn(console, 'warn').mockImplementation()
    mockHubConnection.start.mockRejectedValue(new Error('Connection failed'))

    renderWithProvider(<TestComponent />)

    await waitFor(() => {
      expect(screen.getByTestId('is-connecting')).toHaveTextContent('false')
    })

    expect(screen.getByTestId('is-connected')).toHaveTextContent('false')
    expect(consoleWarnSpy).toHaveBeenCalledWith(
      'SignalR connection failed (this is optional for auth service):',
      'Connection failed'
    )
    
    consoleWarnSpy.mockRestore()
  })

  it('should set up event handlers correctly', async () => {
    const mockUser = {
      id: '123',
      email: 'test@example.com',
      username: 'testuser',
      isActive: true,
      isAdmin: false,
      createdAt: '2023-01-01',
      updatedAt: '2023-01-01'
    }

    mockUseUser.mockReturnValue({
      user: mockUser,
      loading: false,
      error: null,
      refetchUser: jest.fn(),
      updateUserData: jest.fn(),
    })

    renderWithProvider(<TestComponent />)

    await waitFor(() => {
      expect(screen.getByTestId('is-connected')).toHaveTextContent('true')
    })

    expect(mockHubConnection.on).toHaveBeenCalledWith('ReceiveMessage', expect.any(Function))
    expect(mockHubConnection.on).toHaveBeenCalledWith('ConversationUpdated', expect.any(Function))
    expect(mockHubConnection.on).toHaveBeenCalledWith('UserPresenceUpdate', expect.any(Function))
    expect(mockHubConnection.on).toHaveBeenCalledWith('MessageRead', expect.any(Function))
    expect(mockHubConnection.on).toHaveBeenCalledWith('MessageDeleted', expect.any(Function))
  })

  it('should handle reconnection events', async () => {
    const mockUser = {
      id: '123',
      email: 'test@example.com',
      username: 'testuser',
      isActive: true,
      isAdmin: false,
      createdAt: '2023-01-01',
      updatedAt: '2023-01-01'
    }

    mockUseUser.mockReturnValue({
      user: mockUser,
      loading: false,
      error: null,
      refetchUser: jest.fn(),
      updateUserData: jest.fn(),
    })

    const consoleLogSpy = jest.spyOn(console, 'log').mockImplementation()

    renderWithProvider(<TestComponent />)

    await waitFor(() => {
      expect(screen.getByTestId('is-connected')).toHaveTextContent('true')
    })

    // Get the reconnection handlers
    const onreconnectingCall = mockHubConnection.onreconnecting.mock.calls[0]
    const onreconnectedCall = mockHubConnection.onreconnected.mock.calls[0]
    const oncloseCall = mockHubConnection.onclose.mock.calls[0]

    expect(onreconnectingCall).toBeDefined()
    expect(onreconnectedCall).toBeDefined()
    expect(oncloseCall).toBeDefined()

    // Test reconnecting handler
    const reconnectingHandler = onreconnectingCall[0]
    act(() => {
      reconnectingHandler(new Error('Connection lost'))
    })

    expect(screen.getByTestId('is-connected')).toHaveTextContent('false')

    // Test reconnected handler
    const reconnectedHandler = onreconnectedCall[0]
    act(() => {
      reconnectedHandler('new-connection-id')
    })

    expect(screen.getByTestId('is-connected')).toHaveTextContent('true')
    expect(consoleLogSpy).toHaveBeenCalledWith('SignalR reconnected:', 'new-connection-id')

    // Test close handler
    const closeHandler = oncloseCall[0]
    act(() => {
      closeHandler(new Error('Connection closed'))
    })

    expect(screen.getByTestId('is-connected')).toHaveTextContent('false')
    expect(screen.getByTestId('is-connecting')).toHaveTextContent('false')

    consoleLogSpy.mockRestore()
  })

  it('should disconnect properly', async () => {
    const mockUser = {
      id: '123',
      email: 'test@example.com',
      username: 'testuser',
      isActive: true,
      isAdmin: false,
      createdAt: '2023-01-01',
      updatedAt: '2023-01-01'
    }

    mockUseUser.mockReturnValue({
      user: mockUser,
      loading: false,
      error: null,
      refetchUser: jest.fn(),
      updateUserData: jest.fn(),
    })

    renderWithProvider(<TestComponent />)

    await waitFor(() => {
      expect(screen.getByTestId('is-connected')).toHaveTextContent('true')
    })

    act(() => {
      screen.getByTestId('disconnect').click()
    })

    await waitFor(() => {
      expect(screen.getByTestId('is-connected')).toHaveTextContent('false')
    })

    expect(mockHubConnection.invoke).toHaveBeenCalledWith('LeaveUserGroup', '123')
    expect(mockHubConnection.stop).toHaveBeenCalled()
  })

  it('should mark message as read', async () => {
    const mockUser = {
      id: '123',
      email: 'test@example.com',
      username: 'testuser',
      isActive: true,
      isAdmin: false,
      createdAt: '2023-01-01',
      updatedAt: '2023-01-01'
    }

    mockUseUser.mockReturnValue({
      user: mockUser,
      loading: false,
      error: null,
      refetchUser: jest.fn(),
      updateUserData: jest.fn(),
    })

    const consoleLogSpy = jest.spyOn(console, 'log').mockImplementation()

    renderWithProvider(<TestComponent />)

    await waitFor(() => {
      expect(screen.getByTestId('is-connected')).toHaveTextContent('true')
    })

    act(() => {
      screen.getByTestId('mark-read').click()
    })

    expect(mockHubConnection.invoke).toHaveBeenCalledWith('MarkMessageAsRead', 'msg1', 'user1')
    expect(consoleLogSpy).toHaveBeenCalledWith('Message with ID msg1 marked as read by user user1')

    consoleLogSpy.mockRestore()
  })

  it('should delete message', async () => {
    const mockUser = {
      id: '123',
      email: 'test@example.com',
      username: 'testuser',
      isActive: true,
      isAdmin: false,
      createdAt: '2023-01-01',
      updatedAt: '2023-01-01'
    }

    mockUseUser.mockReturnValue({
      user: mockUser,
      loading: false,
      error: null,
      refetchUser: jest.fn(),
      updateUserData: jest.fn(),
    })

    const consoleLogSpy = jest.spyOn(console, 'log').mockImplementation()

    renderWithProvider(<TestComponent />)

    await waitFor(() => {
      expect(screen.getByTestId('is-connected')).toHaveTextContent('true')
    })

    act(() => {
      screen.getByTestId('delete-message').click()
    })

    expect(mockHubConnection.invoke).toHaveBeenCalledWith('DeleteMessage', 'msg1', 'user1')
    expect(consoleLogSpy).toHaveBeenCalledWith('Message with ID msg1 deleted by user user1')

    consoleLogSpy.mockRestore()
  })

  it('should handle mark message as read without connection', async () => {
    mockUseUser.mockReturnValue({
      user: null,
      loading: false,
      error: null,
      refetchUser: jest.fn(),
      updateUserData: jest.fn(),
    })

    const consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation()

    renderWithProvider(<TestComponent />)

    act(() => {
      screen.getByTestId('mark-read').click()
    })

    expect(consoleErrorSpy).toHaveBeenCalledWith('SignalR connection not established.')
    expect(mockHubConnection.invoke).not.toHaveBeenCalled()

    consoleErrorSpy.mockRestore()
  })

  it('should handle delete message without connection', async () => {
    mockUseUser.mockReturnValue({
      user: null,
      loading: false,
      error: null,
      refetchUser: jest.fn(),
      updateUserData: jest.fn(),
    })

    const consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation()

    renderWithProvider(<TestComponent />)

    act(() => {
      screen.getByTestId('delete-message').click()
    })

    expect(consoleErrorSpy).toHaveBeenCalledWith('SignalR connection not established.')
    expect(mockHubConnection.invoke).not.toHaveBeenCalled()

    consoleErrorSpy.mockRestore()
  })

  it('should subscribe and unsubscribe to message events', async () => {
    const mockUser = {
      id: '123',
      email: 'test@example.com',
      username: 'testuser',
      isActive: true,
      isAdmin: false,
      createdAt: '2023-01-01',
      updatedAt: '2023-01-01'
    }

    mockUseUser.mockReturnValue({
      user: mockUser,
      loading: false,
      error: null,
      refetchUser: jest.fn(),
      updateUserData: jest.fn(),
    })

    renderWithProvider(<TestComponent />)

    await waitFor(() => {
      expect(screen.getByTestId('is-connected')).toHaveTextContent('true')
    })

    // Subscribe to messages
    act(() => {
      screen.getByTestId('subscribe-message').click()
    })

    // Verify unsubscribe function is stored
    expect((window as any).unsubscribeMessage).toBeDefined()

    // Call unsubscribe
    act(() => {
      ;(window as any).unsubscribeMessage()
    })

    // Should not throw any errors
  })

  it('should use default URL when environment variable is not set', async () => {
    delete process.env.NEXT_PUBLIC_MESSAGES_API_URL

    const mockUser = {
      id: '123',
      email: 'test@example.com',
      username: 'testuser',
      isActive: true,
      isAdmin: false,
      createdAt: '2023-01-01',
      updatedAt: '2023-01-01'
    }

    mockUseUser.mockReturnValue({
      user: mockUser,
      loading: false,
      error: null,
      refetchUser: jest.fn(),
      updateUserData: jest.fn(),
    })

    renderWithProvider(<TestComponent />)

    await waitFor(() => {
      expect(mockHubConnectionBuilder.withUrl).toHaveBeenCalledWith(
        'http://localhost:5093/messagehub',
        expect.any(Object)
      )
    })
  })

  it('should handle connection errors during invoke operations', async () => {
    const mockUser = {
      id: '123',
      email: 'test@example.com',
      username: 'testuser',
      isActive: true,
      isAdmin: false,
      createdAt: '2023-01-01',
      updatedAt: '2023-01-01'
    }

    mockUseUser.mockReturnValue({
      user: mockUser,
      loading: false,
      error: null,
      refetchUser: jest.fn(),
      updateUserData: jest.fn(),
    })

    const consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation()
    mockHubConnection.invoke.mockRejectedValue(new Error('Invoke failed'))

    renderWithProvider(<TestComponent />)

    await waitFor(() => {
      expect(screen.getByTestId('is-connected')).toHaveTextContent('true')
    })

    act(() => {
      screen.getByTestId('mark-read').click()
    })

    expect(consoleErrorSpy).toHaveBeenCalledWith('Error marking message as read:', expect.any(Error))

    consoleErrorSpy.mockRestore()
  })
})

describe('useWebSocket', () => {
  it('should throw error when used outside WebSocketProvider', () => {
    const consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation()
    
    expect(() => {
      render(<TestComponent />)
    }).toThrow('useWebSocket must be used within a WebSocketProvider')
    
    consoleErrorSpy.mockRestore()
  })
})
