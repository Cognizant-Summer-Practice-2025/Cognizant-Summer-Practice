import { NextRequest, NextResponse } from 'next/server';

// Global storage for signout signals
declare global {
  var authServiceSignoutSignals: Set<string>;
}

// Initialize global storage if it doesn't exist
if (!global.authServiceSignoutSignals) {
  global.authServiceSignoutSignals = new Set();
}

/**
 * Check if there's a signout signal for a specific user
 */
export async function POST(request: NextRequest) {
  try {
    const { email } = await request.json();

    if (!email) {
      return NextResponse.json({ error: 'Email is required' }, { status: 400 });
    }

    // Check if there's a signout signal for this user
    const shouldSignOut = global.authServiceSignoutSignals.has(email);

    if (shouldSignOut) {
      // Clear the signal after detecting it
      global.authServiceSignoutSignals.delete(email);
      console.log(`Signout signal detected and cleared for user: ${email}`);
    }

    return NextResponse.json({ 
      shouldSignOut,
      message: shouldSignOut ? 'Signout required' : 'No signout signal'
    });
  } catch (error) {
    console.error('Error checking signout signal:', error);
    return NextResponse.json(
      { error: 'Failed to check signout signal' },
      { status: 500 }
    );
  }
}