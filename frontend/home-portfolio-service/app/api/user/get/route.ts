import { NextRequest, NextResponse } from 'next/server';

// Reference to the same storage used in inject/remove
declare global {
  var homePortfolioServiceUserStorage: Map<string, any>;
}

if (!global.homePortfolioServiceUserStorage) {
  global.homePortfolioServiceUserStorage = new Map();
}

/**
 * Get user data from this service
 */
export async function GET(request: NextRequest) {
  try {
    // Check if there's any user data stored
    if (global.homePortfolioServiceUserStorage.size === 0) {
      return NextResponse.json({ error: 'No user data found' }, { status: 404 });
    }

    // For now, return the first (and should be only) user
    // In a multi-user scenario, this would need session-based identification
    const userData = Array.from(global.homePortfolioServiceUserStorage.values())[0];

    if (!userData) {
      return NextResponse.json({ error: 'User not found' }, { status: 404 });
    }

    return NextResponse.json(userData);
  } catch (error) {
    console.error('Error getting user data:', error);
    return NextResponse.json(
      { error: 'Failed to get user data' },
      { status: 500 }
    );
  }
}