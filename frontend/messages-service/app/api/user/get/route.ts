import { NextRequest, NextResponse } from 'next/server';
import '@/types/global';

// Reference to the same storage used in inject/remove - using global declaration from types/global.ts

if (!global.messagesServiceUserStorage) {
  global.messagesServiceUserStorage = new Map();
}

// Session-based storage for user data
if (!global.messagesServiceSessionStorage) {
  global.messagesServiceSessionStorage = new Map();
}

/**
 * Get user data from this service
 */
export async function GET(request: NextRequest) {
  try {
    // If a forced local logout flag is present, always return 404 and clear storage
    const { searchParams } = new URL(request.url);
    if (searchParams.get('localLogout') === '1') {
      global.messagesServiceUserStorage.clear();
      global.messagesServiceSessionStorage.clear();
      return NextResponse.json({ error: 'No user data found' }, { status: 404 });
    }

    // Read session cookie to get user data for this specific session
    const sessionId = request.cookies.get('ms_sid')?.value;
    
    if (!sessionId) {
      return NextResponse.json({ error: 'No session found. Please log in.' }, { status: 401 });
    }

    // Get user data for this session
    const userData = global.messagesServiceSessionStorage.get(sessionId);
    
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
      avatarUrl: userData.profileImage || undefined,
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