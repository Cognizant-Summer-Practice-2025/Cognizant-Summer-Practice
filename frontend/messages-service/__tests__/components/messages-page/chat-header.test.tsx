import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import ChatHeader from '@/components/messages-page/chat-header/chat-header'

// Mock the external dependencies
jest.mock('@/lib/portfolio/api', () => ({
	getPortfoliosByUserId: jest.fn().mockResolvedValue([
		{ id: 'portfolio-1', title: 'Test Portfolio' }
	])
}))

jest.mock('@/lib/config/services', () => ({
	redirectToService: jest.fn()
}))

jest.mock('@/lib/user/api', () => ({
	reportUser: jest.fn()
}))

jest.mock('@/lib/contexts/user-context', () => ({
	useUser: () => ({
		user: { id: 'test-user-123' }
	})
}))

// Mock ReportModal component
jest.mock('@/components/messages-page/message-menu/report-modal', () => {
	return function MockReportModal({ isOpen, onClose, onSubmit }: any) {
		if (!isOpen) return null
		return (
			<div data-testid="report-modal">
				<button onClick={() => onSubmit('spam')}>Submit Report</button>
				<button onClick={onClose}>Cancel</button>
			</div>
		)
	}
})

const mockContact = {
	id: 'contact-123',
	name: 'John Doe',
	avatar: '/avatar.jpg',
	lastMessage: 'Hello there!',
	timestamp: '2 min ago',
	userId: 'user-456',
	professionalTitle: 'Software Developer'
}

describe('ChatHeader', () => {
	it('renders contact information', () => {
		render(<ChatHeader selectedContact={mockContact} />)
		
		expect(screen.getByText('John Doe')).toBeInTheDocument()
		expect(screen.getByText('Software Developer')).toBeInTheDocument()
		
		const avatar = screen.getByRole('img', { name: /john doe/i })
		expect(avatar).toBeInTheDocument()
		expect(avatar).toHaveAttribute('alt', 'John Doe')
	})

	it('shows back button in mobile mode', () => {
		const onBackToSidebar = jest.fn()
		render(
			<ChatHeader 
				selectedContact={mockContact} 
				onBackToSidebar={onBackToSidebar}
				isMobile={true}
			/>
		)
		
		expect(document.querySelector('.back-button')).toBeInTheDocument()
	})

	it('calls onBackToSidebar when back button is clicked', async () => {
		const user = userEvent.setup()
		const onBackToSidebar = jest.fn()
		
		render(
			<ChatHeader 
				selectedContact={mockContact} 
				onBackToSidebar={onBackToSidebar}
				isMobile={true}
			/>
		)
		
		const backButton = document.querySelector('.back-button')
		await user.click(backButton!)
		
		expect(onBackToSidebar).toHaveBeenCalledTimes(1)
	})

	it('opens dropdown menu when more options clicked', async () => {
		const user = userEvent.setup()
		render(<ChatHeader selectedContact={mockContact} />)
		
		const moreButton = document.querySelector('.more-options-btn')
		await user.click(moreButton!)
		
		// Check that the dropdown opened by looking for the report option
		await screen.findByText(/report user/i)
		expect(screen.getByText(/report user/i)).toBeInTheDocument()
	})

	it('opens report modal when report user is clicked', async () => {
		const user = userEvent.setup()
		render(<ChatHeader selectedContact={mockContact} />)
		
		// Open dropdown menu
		const moreButton = document.querySelector('.more-options-btn')
		await user.click(moreButton!)
		
		// Click report user
		const reportButton = await screen.findByText(/report user/i)
		await user.click(reportButton)
		
		expect(screen.getByTestId('report-modal')).toBeInTheDocument()
	})

	it('handles contact without professional title', () => {
		const contactWithoutTitle = {
			...mockContact,
			professionalTitle: undefined
		}
		
		render(<ChatHeader selectedContact={contactWithoutTitle} />)
		
		expect(screen.getByText('John Doe')).toBeInTheDocument()
		expect(screen.queryByText('Software Developer')).not.toBeInTheDocument()
	})

	it('does not show back button in desktop mode', () => {
		render(
			<ChatHeader 
				selectedContact={mockContact} 
				isMobile={false}
			/>
		)
		
		expect(document.querySelector('.back-button')).not.toBeInTheDocument()
	})
}) 