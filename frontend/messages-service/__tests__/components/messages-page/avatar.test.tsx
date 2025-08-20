import { render, screen } from '@testing-library/react'
import Avatar from '@/components/messages-page/avatar/avatar'

describe('Avatar', () => {
	it('renders avatar container with proper data-slot', () => {
		render(
			<Avatar 
				src="/test-avatar.jpg" 
				alt="John Doe" 
			/>
		)
		
		const avatarContainer = document.querySelector('[data-slot="avatar"]')
		expect(avatarContainer).toBeInTheDocument()
	})

	it('displays fallback text when image fails to load or as default', () => {
		render(
			<Avatar 
				src="/broken-image.jpg" 
				alt="Jane Smith" 
				fallback="JS"
			/>
		)
		
		// Should show the fallback text
		expect(screen.getByText('JS')).toBeInTheDocument()
	})

	it('uses first letter of alt text as fallback when no fallback provided', () => {
		render(
			<Avatar 
				src="/broken-image.jpg" 
				alt="Bob Wilson" 
			/>
		)
		
		// Should show first letter of alt text as fallback
		expect(screen.getByText('B')).toBeInTheDocument()
	})

	it('applies custom className to avatar container', () => {
		render(
			<Avatar 
				src="/test-avatar.jpg" 
				alt="Test User" 
				className="custom-avatar"
			/>
		)
		
		const avatarContainer = document.querySelector('[data-slot="avatar"]')
		expect(avatarContainer).toHaveClass('custom-avatar')
	})

	it('handles empty alt text gracefully', () => {
		render(
			<Avatar 
				src="/test-avatar.jpg" 
				alt="" 
				fallback="NA"
			/>
		)
		
		expect(screen.getByText('NA')).toBeInTheDocument()
	})

	it('renders fallback when alt text is provided but image may not load', () => {
		render(
			<Avatar 
				src="/may-not-exist.jpg" 
				alt="Test User" 
			/>
		)
		
		// Should show first letter of alt text
		expect(screen.getByText('T')).toBeInTheDocument()
	})
}) 