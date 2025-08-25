import { NextRequest, NextResponse } from 'next/server';
import { getServerSession } from 'next-auth';
import { authOptions } from '@/lib/auth/auth-options';

export async function POST(request: NextRequest) {
  try {
    console.log('🔍 [DEBUG] Starting create-checkout-session API route');
    
    const session = await getServerSession(authOptions);
    console.log('🔍 [DEBUG] Session retrieved:', {
      hasSession: !!session,
      hasAccessToken: !!session?.accessToken,
      accessTokenLength: session?.accessToken?.length || 0
    });

    if (!session?.accessToken) {
      console.log('❌ [DEBUG] No session or access token found');
      return NextResponse.json({ error: 'Unauthorized' }, { status: 401 });
    }

    const { successUrl, cancelUrl } = await request.json();
    console.log('🔍 [DEBUG] Request body:', { successUrl, cancelUrl });

    if (!successUrl || !cancelUrl) {
      console.log('❌ [DEBUG] Missing success or cancel URL');
      return NextResponse.json(
        { error: 'Success and cancel URLs are required' }, 
        { status: 400 }
      );
    }

    // Call the backend API to create checkout session
    const backendUrl = process.env.NEXT_PUBLIC_USER_API_URL || 'http://localhost:5200';
    const fullBackendUrl = `${backendUrl}/api/premiumsubscription/create-checkout-session`;
    
    console.log('🔍 [DEBUG] Calling backend:', {
      backendUrl,
      fullBackendUrl,
      accessToken: session.accessToken.substring(0, 20) + '...'
    });

    const response = await fetch(fullBackendUrl, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${session.accessToken}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        successUrl,
        cancelUrl,
      }),
    });

    console.log('🔍 [DEBUG] Backend response:', {
      status: response.status,
      statusText: response.statusText,
      ok: response.ok
    });

    if (!response.ok) {
      const errorText = await response.text();
      console.error('❌ [DEBUG] Backend error response:', errorText);
      throw new Error(`Backend error: ${response.status} ${response.statusText} - ${errorText}`);
    }

    const data = await response.json();
    console.log('✅ [DEBUG] Successfully created checkout session:', data);
    return NextResponse.json(data);
  } catch (error) {
    console.error('❌ [DEBUG] Error in create-checkout-session:', error);
    return NextResponse.json(
      { error: error instanceof Error ? error.message : 'Internal server error' }, 
      { status: 500 }
    );
  }
}
