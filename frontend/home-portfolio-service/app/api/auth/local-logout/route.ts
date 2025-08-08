import { NextResponse } from 'next/server';

// Reference to the same storage used in inject/remove
declare global {
  // eslint-disable-next-line no-var
  var homePortfolioServiceUserStorage: Map<string, any>;
}

if (!global.homePortfolioServiceUserStorage) {
  global.homePortfolioServiceUserStorage = new Map();
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
    return NextResponse.json({ success: true });
  } catch (error) {
    console.error('Error clearing local user storage:', error);
    return NextResponse.json({ success: false, error: 'Failed to clear local user storage' }, { status: 500 });
  }
}


