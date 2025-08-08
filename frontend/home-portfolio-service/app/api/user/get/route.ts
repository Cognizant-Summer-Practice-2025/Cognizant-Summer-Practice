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
    // If a forced local logout flag is present, always return 404 and clear storage
    const { searchParams } = new URL(request.url);
    if (searchParams.get('localLogout') === '1') {
      global.homePortfolioServiceUserStorage.clear();
      return NextResponse.json({ error: 'No user data found' }, { status: 404 });
    }

    // Check if there's any user data stored
    if (global.homePortfolioServiceUserStorage.size === 0) {
      return NextResponse.json({ error: 'No user data found' }, { status: 404 });
    }

    // For now, return the first (and should be only) user
    // In a multi-user scenario, this would need session-based identification
    const userData = Array.from(global.homePortfolioServiceUserStorage.values())[0];

    // If accessToken exists but is an empty string, treat as missing and return 404 to force re-injection
    if (userData && typeof userData.accessToken === 'string' && userData.accessToken.trim() === '') {
      return NextResponse.json({ error: 'No user data found' }, { status: 404 });
    }

    if (!userData) {
      return NextResponse.json({ error: 'User not found' }, { status: 404 });
    }

    // Transform profileImage to avatarUrl for compatibility with User interface
    const transformedUserData = {
      ...userData,
      avatarUrl: userData.profileImage,
      // Remove profileImage to avoid confusion
      profileImage: undefined
    };

    return NextResponse.json(transformedUserData);
  } catch (error) {
    console.error('Error getting user data:', error);
    return NextResponse.json(
      { error: 'Failed to get user data' },
      { status: 500 }
    );
  }
}