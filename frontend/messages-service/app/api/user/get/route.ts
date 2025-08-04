import { NextRequest, NextResponse } from 'next/server';

// Reference to the same storage used in inject/remove
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
 * Similar to how auth-user-service identifies users, but using the SSO session data
 */
function getCurrentUserEmail(request: NextRequest): string | null {
  try {
    const url = new URL(request.url);
    
    // Priority 1: userEmail parameter (most reliable)
    const userEmail = url.searchParams.get('userEmail');
    if (userEmail) {
      console.log('Found userEmail parameter:', userEmail);
      return userEmail;
    }
    
    // Priority 2: userId parameter (lookup by ID)
    const userId = url.searchParams.get('userId');
    if (userId) {
      console.log('Found userId parameter:', userId);
      // Find user by ID in storage
      for (const [email, user] of global.messagesServiceUserStorage.entries()) {
        if (user.id === userId) {
          console.log('Found user by ID:', email);
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
 * Following the same pattern as auth-user-service but for our SSO architecture
 */
export async function GET(request: NextRequest) {
  try {
    console.log('=== GET /api/user/get called ===');
    
    // Check if there's any user data stored
    if (global.messagesServiceUserStorage.size === 0) {
      console.log('No users in storage');
      return NextResponse.json({ error: 'No user data found' }, { status: 404 });
    }

    console.log('Users in storage:', Array.from(global.messagesServiceUserStorage.keys()));

    // Get current user email from request (session-based identification)
    const currentUserEmail = getCurrentUserEmail(request);
    
    let userData: ServiceUserData | null = null;

    if (currentUserEmail) {
      // Session-based identification: find the specific user by email
      userData = global.messagesServiceUserStorage.get(currentUserEmail) || null;
      
      if (userData) {
        console.log(`✅ Found user data for: ${currentUserEmail} (ID: ${userData.id})`);
      } else {
        console.log(`❌ User ${currentUserEmail} not found in storage`);
      }
    } else {
      console.log('⚠️ No user identification provided, using fallback');
    }

    // Fallback: if no session info or user not found, return the first user (backward compatibility)
    if (!userData) {
      console.warn('Falling back to first user. This should not happen in production with proper session identification.');
      userData = Array.from(global.messagesServiceUserStorage.values())[0];
      
      if (userData) {
        console.log(`Fallback: returning first user: ${userData.email} (ID: ${userData.id})`);
      }
    }

    if (!userData) {
      console.log('No user data found at all');
      return NextResponse.json({ error: 'User not found' }, { status: 404 });
    }

    // Transform profileImage to avatarUrl for compatibility with User interface
    const transformedUserData = {
      ...userData,
      avatarUrl: userData.profileImage,
      // Remove profileImage to avoid confusion
      profileImage: undefined
    };

    console.log(`✅ Returning user data for: ${userData.email} (ID: ${userData.id})`);
    return NextResponse.json(transformedUserData);
  } catch (error) {
    console.error('Error getting user data:', error);
    return NextResponse.json(
      { error: 'Failed to get user data' },
      { status: 500 }
    );
  }
}