import { NextResponse } from 'next/server';
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
 * Clear any locally injected user data for this service.
 * This endpoint is intended to be called by the browser on logout
 * to synchronously remove user data and avoid race conditions
 * where subsequent GETs might still see a user.
 */
export async function POST() {
  try {
    global.messagesServiceUserStorage.clear();
    global.messagesServiceSessionStorage.clear();

    // Create response and clear session cookie
    const response = NextResponse.json({ success: true });
    
    // Clear the session cookie
    response.cookies.set('ms_sid', '', {
      httpOnly: true,
      secure: process.env.NODE_ENV === 'production',
      sameSite: 'lax',
      maxAge: 0, // Expire immediately
      path: '/'
    });

    return response;
  } catch (error) {
    console.error('Error clearing local user storage:', error);
    return NextResponse.json({ success: false, error: 'Failed to clear local user storage' }, { status: 500 });
  }
}


