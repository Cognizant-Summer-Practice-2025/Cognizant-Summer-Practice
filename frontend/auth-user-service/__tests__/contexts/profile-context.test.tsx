import React from 'react'
import { render, screen, act } from '@testing-library/react'
import { ProfileProvider, useProfile } from '@/lib/contexts/profile-context'

// Test component to access context
const TestComponent = () => {
  const { activeTab, setActiveTab } = useProfile()
  
  return (
    <div>
      <div data-testid="active-tab">{activeTab}</div>
      <button data-testid="set-tab-1" onClick={() => setActiveTab('tab1')}>
        Set Tab 1
      </button>
      <button data-testid="set-tab-2" onClick={() => setActiveTab('tab2')}>
        Set Tab 2
      </button>
      <button data-testid="reset-tab" onClick={() => setActiveTab('basic-info')}>
        Reset Tab
      </button>
    </div>
  )
}

const renderWithProvider = (children: React.ReactNode) => {
  return render(
    <ProfileProvider>
      {children}
    </ProfileProvider>
  )
}

describe('ProfileProvider', () => {
  it('should provide initial state with default active tab', () => {
    renderWithProvider(<TestComponent />)

    expect(screen.getByTestId('active-tab')).toHaveTextContent('basic-info')
  })

  it('should update active tab when setActiveTab is called', () => {
    renderWithProvider(<TestComponent />)

    expect(screen.getByTestId('active-tab')).toHaveTextContent('basic-info')

    act(() => {
      screen.getByTestId('set-tab-1').click()
    })

    expect(screen.getByTestId('active-tab')).toHaveTextContent('tab1')
  })

  it('should update active tab multiple times', () => {
    renderWithProvider(<TestComponent />)

    // Set to tab1
    act(() => {
      screen.getByTestId('set-tab-1').click()
    })

    expect(screen.getByTestId('active-tab')).toHaveTextContent('tab1')

    // Set to tab2
    act(() => {
      screen.getByTestId('set-tab-2').click()
    })

    expect(screen.getByTestId('active-tab')).toHaveTextContent('tab2')

    // Reset to basic-info
    act(() => {
      screen.getByTestId('reset-tab').click()
    })

    expect(screen.getByTestId('active-tab')).toHaveTextContent('basic-info')
  })

  it('should handle empty string tab name', () => {
    const TestComponentWithEmptyTab = () => {
      const { activeTab, setActiveTab } = useProfile()
      
      return (
        <div>
          <div data-testid="active-tab">{activeTab || 'empty'}</div>
          <button data-testid="set-empty-tab" onClick={() => setActiveTab('')}>
            Set Empty Tab
          </button>
        </div>
      )
    }

    renderWithProvider(<TestComponentWithEmptyTab />)

    act(() => {
      screen.getByTestId('set-empty-tab').click()
    })

    expect(screen.getByTestId('active-tab')).toHaveTextContent('empty')
  })

  it('should handle special characters in tab names', () => {
    const TestComponentWithSpecialChars = () => {
      const { activeTab, setActiveTab } = useProfile()
      
      return (
        <div>
          <div data-testid="active-tab">{activeTab}</div>
          <button 
            data-testid="set-special-tab" 
            onClick={() => setActiveTab('tab-with-dashes_and_underscores.and.dots')}
          >
            Set Special Tab
          </button>
        </div>
      )
    }

    renderWithProvider(<TestComponentWithSpecialChars />)

    act(() => {
      screen.getByTestId('set-special-tab').click()
    })

    expect(screen.getByTestId('active-tab')).toHaveTextContent('tab-with-dashes_and_underscores.and.dots')
  })

  it('should maintain state across multiple children', () => {
    const FirstChild = () => {
      const { activeTab, setActiveTab } = useProfile()
      return (
        <div>
          <div data-testid="first-child-tab">{activeTab}</div>
          <button data-testid="first-child-set" onClick={() => setActiveTab('from-first')}>
            Set from First
          </button>
        </div>
      )
    }

    const SecondChild = () => {
      const { activeTab, setActiveTab } = useProfile()
      return (
        <div>
          <div data-testid="second-child-tab">{activeTab}</div>
          <button data-testid="second-child-set" onClick={() => setActiveTab('from-second')}>
            Set from Second
          </button>
        </div>
      )
    }

    render(
      <ProfileProvider>
        <FirstChild />
        <SecondChild />
      </ProfileProvider>
    )

    // Both children should show the same initial state
    expect(screen.getByTestId('first-child-tab')).toHaveTextContent('basic-info')
    expect(screen.getByTestId('second-child-tab')).toHaveTextContent('basic-info')

    // Update from first child
    act(() => {
      screen.getByTestId('first-child-set').click()
    })

    // Both should reflect the change
    expect(screen.getByTestId('first-child-tab')).toHaveTextContent('from-first')
    expect(screen.getByTestId('second-child-tab')).toHaveTextContent('from-first')

    // Update from second child
    act(() => {
      screen.getByTestId('second-child-set').click()
    })

    // Both should reflect the new change
    expect(screen.getByTestId('first-child-tab')).toHaveTextContent('from-second')
    expect(screen.getByTestId('second-child-tab')).toHaveTextContent('from-second')
  })

  it('should provide stable function references', () => {
    let setActiveTabRef: any

    const TestComponentWithRef = () => {
      const { setActiveTab } = useProfile()
      
      // Store reference to function
      if (!setActiveTabRef) {
        setActiveTabRef = setActiveTab
      }
      
      return (
        <div>
          <div data-testid="function-stable">
            {setActiveTab === setActiveTabRef ? 'stable' : 'unstable'}
          </div>
        </div>
      )
    }

    const { rerender } = renderWithProvider(<TestComponentWithRef />)

    expect(screen.getByTestId('function-stable')).toHaveTextContent('stable')

    // Force a re-render
    rerender(
      <ProfileProvider>
        <TestComponentWithRef />
      </ProfileProvider>
    )

    expect(screen.getByTestId('function-stable')).toHaveTextContent('stable')
  })
})

describe('useProfile', () => {
  it('should work with context default values when used outside provider', () => {
    // Note: The context provides default values, so it won't throw an error
    // but the setActiveTab function will be a no-op
    
    render(<TestComponent />)

    expect(screen.getByTestId('active-tab')).toHaveTextContent('basic-info')

    // The button click should work but won't change anything since it's the default no-op function
    act(() => {
      screen.getByTestId('set-tab-1').click()
    })

    // Should still show basic-info since the default setActiveTab does nothing
    expect(screen.getByTestId('active-tab')).toHaveTextContent('basic-info')
  })
})
