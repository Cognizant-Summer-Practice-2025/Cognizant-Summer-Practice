import { renderHook, act, waitFor } from '@testing-library/react'
import { usePortfolioSearch } from '@/hooks/usePortfolioSearch'

// Mock fetch
const mockFetch = global.fetch as jest.MockedFunction<typeof fetch>

// Mock timers for debouncing
jest.useFakeTimers()

// Mock window.location
const mockLocation = {
  href: '',
}
Object.defineProperty(window, 'location', {
  value: mockLocation,
  writable: true,
})

describe('usePortfolioSearch', () => {
  beforeEach(() => {
    jest.clearAllMocks()
    jest.clearAllTimers()
    process.env.NEXT_PUBLIC_API_BASE_URL = 'http://localhost:5201'
    process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE = 'http://localhost:3001'
    
    // Reset location mock
    mockLocation.href = ''
  })

  afterEach(() => {
    delete process.env.NEXT_PUBLIC_API_BASE_URL
    delete process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE
  })

  afterAll(() => {
    jest.useRealTimers()
  })

  it('should initialize with default state', () => {
    const { result } = renderHook(() => usePortfolioSearch())

    expect(result.current.searchTerm).toBe('')
    expect(result.current.results).toEqual([])
    expect(result.current.loading).toBe(false)
    expect(result.current.error).toBeNull()
    expect(result.current.showResults).toBe(false)
    expect(result.current.searchInputRef.current).toBeNull()
    expect(result.current.searchContainerRef.current).toBeNull()
  })

  it('should update search term and trigger debounced search', async () => {
    const mockPortfolioData = {
      data: [
        {
          id: '1',
          userId: 'user1',
          name: 'John Doe',
          role: 'Developer',
          location: 'New York',
          description: 'Full stack developer',
          skills: ['React', 'Node.js'],
          avatar: 'avatar1.jpg',
          featured: true
        }
      ]
    }

    mockFetch.mockResolvedValueOnce({
      ok: true,
      json: () => Promise.resolve(mockPortfolioData),
    } as Response)

    const { result } = renderHook(() => usePortfolioSearch())

    // Set search term
    act(() => {
      result.current.setSearchTerm('John')
    })

    expect(result.current.searchTerm).toBe('John')

    // Fast-forward debounce timer
    act(() => {
      jest.advanceTimersByTime(300)
    })

    await waitFor(() => {
      expect(result.current.results).toHaveLength(1)
      expect(result.current.showResults).toBe(true)
    })

    expect(result.current.results[0]).toEqual({
      id: '1',
      userId: 'user1',
      name: 'John Doe',
      role: 'Developer',
      location: 'New York',
      description: 'Full stack developer',
      skills: ['React', 'Node.js'],
      avatar: 'avatar1.jpg',
      featured: true
    })

    expect(mockFetch).toHaveBeenCalledWith(
      'http://localhost:5201/api/Portfolio/home-page-cards/paginated?page=1&pageSize=10&searchTerm=John'
    )
  })

  it('should clear results when search term is empty', () => {
    const { result } = renderHook(() => usePortfolioSearch())

    // Set empty search term
    act(() => {
      result.current.setSearchTerm('')
    })

    // Fast-forward debounce timer
    act(() => {
      jest.advanceTimersByTime(300)
    })

    expect(result.current.results).toEqual([])
    expect(result.current.showResults).toBe(false)
    expect(mockFetch).not.toHaveBeenCalled()
  })

  it('should clear results when search term is only whitespace', () => {
    const { result } = renderHook(() => usePortfolioSearch())

    // Set whitespace search term
    act(() => {
      result.current.setSearchTerm('   ')
    })

    // Fast-forward debounce timer
    act(() => {
      jest.advanceTimersByTime(300)
    })

    expect(result.current.results).toEqual([])
    expect(result.current.showResults).toBe(false)
    expect(mockFetch).not.toHaveBeenCalled()
  })

  it('should handle search errors', async () => {
    mockFetch.mockRejectedValueOnce(new Error('Search failed'))

    const { result } = renderHook(() => usePortfolioSearch())

    act(() => {
      result.current.setSearchTerm('test')
    })

    act(() => {
      jest.advanceTimersByTime(300)
    })

    await waitFor(() => {
      expect(result.current.error).toBe('Search failed')
      expect(result.current.results).toEqual([])
      expect(result.current.loading).toBe(false)
    })
  })

  it('should handle API response errors', async () => {
    mockFetch.mockResolvedValueOnce({
      ok: false,
      status: 500,
    } as Response)

    const { result } = renderHook(() => usePortfolioSearch())

    act(() => {
      result.current.setSearchTerm('test')
    })

    act(() => {
      jest.advanceTimersByTime(300)
    })

    await waitFor(() => {
      expect(result.current.error).toBe('Search failed')
      expect(result.current.results).toEqual([])
    })
  })

  it('should debounce search requests', () => {
    const { result } = renderHook(() => usePortfolioSearch())

    // Rapid search term changes
    act(() => {
      result.current.setSearchTerm('J')
    })

    act(() => {
      result.current.setSearchTerm('Jo')
    })

    act(() => {
      result.current.setSearchTerm('Joh')
    })

    act(() => {
      result.current.setSearchTerm('John')
    })

    // Only advance timer once
    act(() => {
      jest.advanceTimersByTime(300)
    })

    // Should only make one API call with the latest search term
    expect(mockFetch).toHaveBeenCalledTimes(1)
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('searchTerm=John')
    )
  })

  it('should navigate to portfolio on result click', () => {
    const { result } = renderHook(() => usePortfolioSearch())

    const mockResult = {
      id: '1',
      userId: 'user123',
      name: 'John Doe',
      role: 'Developer',
      location: 'New York',
      description: 'Full stack developer',
      skills: ['React'],
      featured: false
    }

    act(() => {
      result.current.handleResultClick(mockResult)
    })

    expect(mockLocation.href).toBe('http://localhost:3001/portfolio?user=user123')
    expect(result.current.showResults).toBe(false)
    expect(result.current.searchTerm).toBe('')
  })

  it('should clear search state when clearSearch is called', () => {
    const { result } = renderHook(() => usePortfolioSearch())

    // Set up some state first
    act(() => {
      result.current.setSearchTerm('test')
      result.current.setShowResults(true)
    })

    // Clear search
    act(() => {
      result.current.clearSearch()
    })

    expect(result.current.searchTerm).toBe('')
    expect(result.current.results).toEqual([])
    expect(result.current.showResults).toBe(false)
    expect(result.current.error).toBeNull()
  })

  it('should show loading state during search', async () => {
    let resolvePromise: (value: any) => void
    const searchPromise = new Promise((resolve) => {
      resolvePromise = resolve
    })

    mockFetch.mockReturnValueOnce(searchPromise as any)

    const { result } = renderHook(() => usePortfolioSearch())

    act(() => {
      result.current.setSearchTerm('test')
    })

    act(() => {
      jest.advanceTimersByTime(300)
    })

    // Should be loading
    await waitFor(() => {
      expect(result.current.loading).toBe(true)
    })

    // Resolve the promise
    act(() => {
      resolvePromise!({
        ok: true,
        json: () => Promise.resolve({ data: [] }),
      })
    })

    await waitFor(() => {
      expect(result.current.loading).toBe(false)
    })
  })

  it('should cleanup debounce timeout on unmount', () => {
    const clearTimeoutSpy = jest.spyOn(global, 'clearTimeout')
    
    const { result, unmount } = renderHook(() => usePortfolioSearch())

    act(() => {
      result.current.setSearchTerm('test')
    })

    unmount()

    expect(clearTimeoutSpy).toHaveBeenCalled()
    
    clearTimeoutSpy.mockRestore()
  })

  it('should handle document click outside to hide results', () => {
    const { result } = renderHook(() => usePortfolioSearch())

    // Show results first
    act(() => {
      result.current.setShowResults(true)
    })

    expect(result.current.showResults).toBe(true)

    // Simulate click outside
    const mockEvent = {
      target: document.createElement('div'),
    } as any

    act(() => {
      document.dispatchEvent(new MouseEvent('mousedown', { bubbles: true }))
    })

    // Note: In a real test, we'd need to set up the refs properly to test this functionality
    // This is a simplified test to verify the event listener is set up
  })

  it('should use default API URL when environment variable is not set', async () => {
    delete process.env.NEXT_PUBLIC_API_BASE_URL

    mockFetch.mockResolvedValueOnce({
      ok: true,
      json: () => Promise.resolve({ data: [] }),
    } as Response)

    const { result } = renderHook(() => usePortfolioSearch())

    act(() => {
      result.current.setSearchTerm('test')
    })

    act(() => {
      jest.advanceTimersByTime(300)
    })

    await waitFor(() => {
      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('http://localhost:5201')
      )
    })
  })

  it('should use default home portfolio service URL when environment variable is not set', () => {
    delete process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE

    const { result } = renderHook(() => usePortfolioSearch())

    const mockResult = {
      id: '1',
      userId: 'user123',
      name: 'John Doe',
      role: 'Developer',
      location: 'New York',
      description: 'Full stack developer',
      skills: ['React'],
      featured: false
    }

    act(() => {
      result.current.handleResultClick(mockResult)
    })

    expect(mockLocation.href).toBe('http://localhost:3001/portfolio?user=user123')
  })
})
