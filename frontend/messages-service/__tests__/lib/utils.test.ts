import { cn } from '@/lib/utils'

describe('cn', () => {
	it('merges class names and resolves tailwind conflicts', () => {
		const result = cn('p-2', 'p-4', ['text-sm', { hidden: false }])
		expect(result).toBe('p-4 text-sm')
	})
}) 