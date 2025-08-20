import { NextRequest, NextResponse } from 'next/server';

const USER_SERVICE_URL = process.env.USER_SERVICE_URL || 'http://localhost:5002';

export async function GET(request: NextRequest) {
  try {
    // Forward the request to the backend user service
    const response = await fetch(`${USER_SERVICE_URL}/api/users/admin/reports`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        // Forward any authorization headers if present
        ...(request.headers.get('authorization') && {
          'authorization': request.headers.get('authorization')!
        })
      },
    });

    if (!response.ok) {
      const errorText = await response.text();
      console.error('Error fetching user reports:', errorText);
      return NextResponse.json(
        { error: 'Failed to fetch user reports' },
        { status: response.status }
      );
    }

    const userReports = await response.json();
    return NextResponse.json(userReports);
  } catch (error) {
    console.error('Error in user reports API:', error);
    return NextResponse.json(
      { error: 'Internal server error' },
      { status: 500 }
    );
  }
} 