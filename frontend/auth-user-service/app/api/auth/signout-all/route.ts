import { NextRequest, NextResponse } from 'next/server';
import { getServerSession } from 'next-auth';
import { authOptions } from '@/lib/auth/auth-options';
import { UserInjectionService } from '@/lib/services/user-injection-service';

/**
 * Custom sign-out endpoint that removes user data from all services
 */
export async function POST(request: NextRequest) {
  try {
    // Get current session
    const session = await getServerSession(authOptions);
    
    if (!session?.user?.email) {
      return NextResponse.json({ error: 'No active session found' }, { status: 401 });
    }

    // Remove user data from all other services
    await UserInjectionService.removeUser(session.user.email);

    // Return success response - client should then call NextAuth signOut
    return NextResponse.json({ 
      success: true, 
      message: 'User data removed from all services'
    });
  } catch (error) {
    console.error('Error in custom sign-out:', error);
    return NextResponse.json(
      { error: 'Failed to sign out from all services' },
      { status: 500 }
    );
  }
}