import { NextRequest, NextResponse } from 'next/server';

// Reference to the same storage used in inject/remove
declare global {
  var messagesServiceUserStorage: Map<string, any>;
}

if (!global.messagesServiceUserStorage) {
  global.messagesServiceUserStorage = new Map();
}

/**
 * Get user data from this service
 */
export async function GET(request: NextRequest) {
  try {
    const { searchParams } = new URL(request.url);
    const email = searchParams.get('email');

    if (!email) {
      return NextResponse.json({ error: 'User email is required' }, { status: 400 });
    }

    // Get user data from storage
    const userData = global.messagesServiceUserStorage.get(email);

    if (!userData) {
      return NextResponse.json({ error: 'User not found' }, { status: 404 });
    }

    return NextResponse.json({ 
      success: true, 
      user: userData
    });
  } catch (error) {
    console.error('Error getting user data:', error);
    return NextResponse.json(
      { error: 'Failed to get user data' },
      { status: 500 }
    );
  }
}