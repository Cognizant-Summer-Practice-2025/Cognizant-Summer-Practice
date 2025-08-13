import { NextResponse } from 'next/server';
import { Logger } from '@/lib/logger';
import '@/types/global';

// Ensure global storage is initialized  
if (typeof global !== 'undefined' && !global.adminServiceUserStorage) {
  global.adminServiceUserStorage = new Map();
}

/**
 * Clear any locally injected user data for this service.
 * This endpoint is intended to be called by the browser on logout
 * to synchronously remove user data and avoid race conditions
 * where subsequent GETs might still see a user.
 */
export async function POST() {
  try {
    global.adminServiceUserStorage.clear();
    return NextResponse.json({ success: true });
  } catch (error) {
    Logger.error('Error clearing local user storage', error);
    return NextResponse.json({ success: false, error: 'Failed to clear local user storage' }, { status: 500 });
  }
}
