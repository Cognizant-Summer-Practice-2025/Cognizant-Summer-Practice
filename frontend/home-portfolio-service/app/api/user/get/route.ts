import { NextRequest, NextResponse } from 'next/server';
import type { ServiceUserData } from '@/types/global';

// Ensure global storage is initialized
if (typeof global !== 'undefined' && !global.homePortfolioServiceUserStorage) {
  global.homePortfolioServiceUserStorage = new Map<string, ServiceUserData>();
}

// Session-based storage for user data
if (typeof global !== 'undefined' && !global.homePortfolioServiceSessionStorage) {
  global.homePortfolioServiceSessionStorage = new Map<string, ServiceUserData>();
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
      global.homePortfolioServiceSessionStorage.clear();
      return NextResponse.json({ error: 'No user data found' }, { status: 404 });
    }

    // Read session cookie to get user data for this specific session
    const sessionId = request.cookies.get('hp_sid')?.value;
    
    if (!sessionId) {
      return NextResponse.json({ error: 'No session found. Please log in.' }, { status: 401 });
    }

    // Get user data for this session
    const userData: ServiceUserData | undefined = global.homePortfolioServiceSessionStorage.get(sessionId);
    
    if (!userData) {
      return NextResponse.json({ error: 'Session expired. Please log in again.' }, { status: 401 });
    }

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