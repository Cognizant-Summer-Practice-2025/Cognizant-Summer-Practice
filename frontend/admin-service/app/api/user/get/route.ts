import { NextRequest, NextResponse } from 'next/server';
import { ServiceUserData } from '@/types/global';

// Initialize global storage if it doesn't exist
if (!global.adminServiceUserStorage) {
  global.adminServiceUserStorage = new Map();
}

/**
 * Get user data from this service
 */
export async function GET(request: NextRequest) {
  try {
    // Check if there's any user data stored
    if (global.adminServiceUserStorage.size === 0) {
      return NextResponse.json({ error: 'No user data found' }, { status: 404 });
    }

    // For now, return the first (and should be only) user
    // In a multi-user scenario, this would need session-based identification
    const userData: ServiceUserData = Array.from(global.adminServiceUserStorage.values())[0];

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