import '@testing-library/jest-dom'

// Mock Next.js router
jest.mock('next/router', () => ({
	useRouter() {
		return {
			route: '/',
			pathname: '/',
			query: {},
			asPath: '/',
			push: jest.fn(),
			pop: jest.fn(),
			reload: jest.fn(),
			back: jest.fn(),
			prefetch: jest.fn(),
			beforePopState: jest.fn(),
			events: {
				on: jest.fn(),
				off: jest.fn(),
				emit: jest.fn(),
			},
		}
	},
}))

// Mock Next.js navigation
jest.mock('next/navigation', () => ({
	useRouter() {
		return {
			push: jest.fn(),
			replace: jest.fn(),
			refresh: jest.fn(),
			back: jest.fn(),
			forward: jest.fn(),
			prefetch: jest.fn(),
		}
	},
	useSearchParams() {
		return new URLSearchParams()
	},
	usePathname() {
		return '/'
	},
}))

// Mock Next Auth
jest.mock('next-auth/react', () => ({
	useSession: jest.fn(() => ({
		data: null,
		status: 'unauthenticated',
	})),
	getSession: jest.fn(() => Promise.resolve(null)),
	signIn: jest.fn(),
	signOut: jest.fn(),
	SessionProvider: ({ children }) => children,
}))

// Mock fetch globally
// @ts-ignore
global.fetch = jest.fn()

// Mock window.location
// @ts-ignore
delete window.location
// @ts-ignore
window.location = {
	href: 'http://localhost:3000',
	search: '',
	origin: 'http://localhost:3000',
	pathname: '/',
	assign: jest.fn(),
	replace: jest.fn(),
	reload: jest.fn(),
}

// Mock localStorage
const localStorageMock = {
	getItem: jest.fn(),
	setItem: jest.fn(),
	removeItem: jest.fn(),
	clear: jest.fn(),
}
// @ts-ignore
global.localStorage = localStorageMock

// Mock sessionStorage
const sessionStorageMock = {
	getItem: jest.fn(),
	setItem: jest.fn(),
	removeItem: jest.fn(),
	clear: jest.fn(),
}
// @ts-ignore
global.sessionStorage = sessionStorageMock

// Mock ResizeObserver
// @ts-ignore
global.ResizeObserver = jest.fn().mockImplementation(() => ({
	observe: jest.fn(),
	unobserve: jest.fn(),
	disconnect: jest.fn(),
}))

// Mock IntersectionObserver
// @ts-ignore
global.IntersectionObserver = jest.fn().mockImplementation(() => ({
	observe: jest.fn(),
	unobserve: jest.fn(),
	disconnect: jest.fn(),
}))

// Suppress console errors during tests unless needed
const originalError = console.error
beforeAll(() => {
	// @ts-ignore
	console.error = (...args) => {
		if (
			typeof args[0] === 'string' &&
			args[0].includes('Warning: ReactDOM.render is no longer supported')
		) {
			return
		}
		// @ts-ignore
		originalError.call(console, ...args)
	}
})

afterAll(() => {
	// @ts-ignore
	console.error = originalError
}) 