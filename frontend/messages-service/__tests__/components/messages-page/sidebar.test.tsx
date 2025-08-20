import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import MessagesSidebar from '@/components/messages-page/sidebar/sidebar'

// Mock UserSearchModal component
jest.mock('@/components/messages-page/user-search-modal/user-search-modal', () => {
	return function MockUserSearchModal({ 
		visible, 
		onClose, 
		onUserSelect 
	}: {
		visible: boolean;
		onClose: () => void;
		onUserSelect: (user: { id: string; fullName: string; username?: string; professionalTitle?: string; avatarUrl?: string; isActive?: boolean; }) => void;
	}) {
		if (!visible) return null
		return (
			<div data-testid="user-search-modal">
				<button onClick={() => onUserSelect({ id: 'new-user', fullName: 'New User', username: 'newuser', isActive: true })}>
					Select User
				</button>
				<button onClick={onClose}>Close</button>
			</div>
		)
	}
})

const mockContacts = [
	{
		id: 'contact-1',
		name: 'Alice Johnson',
		avatar: '/alice.jpg',
		lastMessage: 'Hey, how are you?',
		timestamp: '2 min ago',
		isOnline: true,
		unreadCount: 2,
		userId: 'user-1',
		professionalTitle: 'Designer'
	},
	{
		id: 'contact-2',
		name: 'Bob Smith',
		avatar: '/bob.jpg',
		lastMessage: 'See you tomorrow!',
		timestamp: '1 hour ago',
		isOnline: false,
		unreadCount: 0,
		userId: 'user-2',
		professionalTitle: 'Developer'
	},
	{
		id: 'contact-3',
		name: 'Carol Wilson',
		avatar: '/carol.jpg',
		lastMessage: 'Thanks for the help',
		timestamp: '3 hours ago',
		isOnline: true,
		unreadCount: 1,
		userId: 'user-3'
	}
]

describe('MessagesSidebar', () => {
	const defaultProps = {
		contacts: mockContacts,
		selectedContact: null,
		onSelectContact: jest.fn(),
		onNewConversation: jest.fn()
	}

	beforeEach(() => {
		jest.clearAllMocks()
	})

	it('shows contact information correctly', () => {
		render(<MessagesSidebar {...defaultProps} />)
		
		// Check that contacts are rendered with their basic info
		expect(screen.getByText('Alice Johnson')).toBeInTheDocument()
		expect(screen.getByText('Bob Smith')).toBeInTheDocument()
		expect(screen.getByText('Carol Wilson')).toBeInTheDocument()
		
		// Check contact messages  
		expect(screen.getByText('Hey, how are you?')).toBeInTheDocument()
		expect(screen.getByText('See you tomorrow!')).toBeInTheDocument()
		expect(screen.getByText('Thanks for the help')).toBeInTheDocument()
		
		// Note: Professional titles don't appear to be rendered in this component
	})

	it('displays unread message counts', () => {
		render(<MessagesSidebar {...defaultProps} />)
		
		// Alice has 2 unread messages
		expect(screen.getByText('2')).toBeInTheDocument()
		// Carol has 1 unread message
		expect(screen.getByText('1')).toBeInTheDocument()
		// Bob has 0 unread messages, so no count should be shown
	})

	it('calls onSelectContact when contact is clicked', async () => {
		const user = userEvent.setup()
		const onSelectContact = jest.fn()
		
		render(
			<MessagesSidebar 
				{...defaultProps} 
				onSelectContact={onSelectContact}
			/>
		)
		
		const aliceContact = screen.getByText('Alice Johnson').closest('.contact-item')
		await user.click(aliceContact!)
		
		expect(onSelectContact).toHaveBeenCalledWith(mockContacts[0])
	})

	it('highlights selected contact', () => {
		render(
			<MessagesSidebar 
				{...defaultProps} 
				selectedContact={mockContacts[1]}
			/>
		)
		
		const bobContact = screen.getByText('Bob Smith').closest('.contact-item')
		expect(bobContact).toHaveClass('contact-item-active')
	})

	it('opens user search modal when new conversation button is clicked', async () => {
		const user = userEvent.setup()
		render(<MessagesSidebar {...defaultProps} />)
		
		const newButton = document.querySelector('.new-message-button')
		await user.click(newButton!)
		
		expect(screen.getByTestId('user-search-modal')).toBeInTheDocument()
	})

	it('calls onNewConversation when user is selected from search modal', async () => {
		const user = userEvent.setup()
		const onNewConversation = jest.fn()
		
		render(
			<MessagesSidebar 
				{...defaultProps} 
				onNewConversation={onNewConversation}
			/>
		)
		
		// Open search modal
		const newButton = document.querySelector('.new-message-button')
		await user.click(newButton!)
		
		// Select a user from the modal
		const selectUserButton = screen.getByText('Select User')
		await user.click(selectUserButton)
		
		expect(onNewConversation).toHaveBeenCalledWith({ id: 'new-user', fullName: 'New User', username: 'newuser', isActive: true })
	})

	it('handles empty contact list', () => {
		render(
			<MessagesSidebar 
				{...defaultProps} 
				contacts={[]}
			/>
		)
		
		// Should still render the new conversation button
		expect(document.querySelector('.new-message-button')).toBeInTheDocument()
		
		// Should show some empty state or just no contacts
		expect(screen.queryByText('Alice Johnson')).not.toBeInTheDocument()
	})

	it('displays timestamps correctly', () => {
		render(<MessagesSidebar {...defaultProps} />)
		
		expect(screen.getByText('2 min ago')).toBeInTheDocument()
		expect(screen.getByText('1 hour ago')).toBeInTheDocument()
		expect(screen.getByText('3 hours ago')).toBeInTheDocument()
	})

	it('closes search modal when close button is clicked', async () => {
		const user = userEvent.setup()
		render(<MessagesSidebar {...defaultProps} />)
		
		// Open search modal
		const newButton = document.querySelector('.new-message-button')
		await user.click(newButton!)
		
		expect(screen.getByTestId('user-search-modal')).toBeInTheDocument()
		
		// Close modal
		const closeButton = screen.getByText('Close')
		await user.click(closeButton)
		
		expect(screen.queryByTestId('user-search-modal')).not.toBeInTheDocument()
	})
}) 