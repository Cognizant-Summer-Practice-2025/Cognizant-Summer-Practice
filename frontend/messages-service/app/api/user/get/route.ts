import { NextRequest, NextResponse } from 'next/server';

interface ServiceUserData {
  id: string;
  email: string;
  username: string;
  firstName: string;
  lastName: string;
  professionalTitle?: string;
  bio?: string;
  location?: string;
  profileImage?: string;
  isActive: boolean;
  isAdmin: boolean;
  lastLoginAt?: string;
}

declare global {
  var messagesServiceUserStorage: Map<string, ServiceUserData>;
}

if (!global.messagesServiceUserStorage) {
  global.messagesServiceUserStorage = new Map();
}

/**
 * Get current user email from request parameters
 */
function getCurrentUserEmail(request: NextRequest): string | null {
  try {
    const url = new URL(request.url);
    
    const userEmail = url.searchParams.get('userEmail');
    if (userEmail) {
      console.log('Found userEmail parameter:', userEmail);
      return userEmail;
    }
    
    const userId = url.searchParams.get('userId');
    if (userId) {
      console.log('Found userId parameter:', userId);
      for (const [email, user] of global.messagesServiceUserStorage.entries()) {
        if (user.id === userId) {
          return email;
        }
      }
    }
    
    console.log('No user identification found in request');
    return null;
  } catch (error) {
    console.error('Error extracting user from request:', error);
    return null;
  }
}

/**
 * Get user data from this service with session-based identification
 */
export async function GET(request: NextRequest) {
  try {
    if (global.messagesServiceUserStorage.size === 0) {
      return NextResponse.json({ error: 'No user data found' }, { status: 404 });
    }

    const currentUserEmail = getCurrentUserEmail(request);
    let userData: ServiceUserData | null = null;

    if (currentUserEmail) {
      userData = global.messagesServiceUserStorage.get(currentUserEmail) || null;
    } else {
      return NextResponse.json({ error: 'No user identification provided' }, { status: 401 });
    }

    if (!userData) {
      return NextResponse.json({ error: 'User not found' }, { status: 404 });
    }

    const transformedUserData = {
      ...userData,
      avatarUrl: userData.profileImage,
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