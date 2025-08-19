import { NextResponse } from 'next/server';
import '@/types/global';

// Ensure global storage is initialized  
if (typeof global !== 'undefined' && !global.homePortfolioServiceUserStorage) {
  global.homePortfolioServiceUserStorage = new Map();
}

// Session-based storage for user data
if (typeof global !== 'undefined' && !global.homePortfolioServiceSessionStorage) {
  global.homePortfolioServiceSessionStorage = new Map();
}

/**
 * Clear any locally injected user data for this service.
 * This endpoint is intended to be called by the browser on logout
 * to synchronously remove user data and avoid race conditions
 * where subsequent GETs might still see a user.
 */
export async function POST() {
  try {
    global.homePortfolioServiceUserStorage.clear();
    global.homePortfolioServiceSessionStorage.clear();

    // Create response and clear session cookie
    const response = NextResponse.json({ success: true });
    
    // Clear the session cookie
    response.cookies.set('hp_sid', '', {
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


