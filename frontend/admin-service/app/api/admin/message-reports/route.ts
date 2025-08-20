import { NextRequest, NextResponse } from 'next/server';

const MESSAGES_SERVICE_URL = process.env.MESSAGES_SERVICE_URL || 'http://localhost:5003';

export async function GET(request: NextRequest) {
  try {
    // Forward the request to the backend messages service
    const response = await fetch(`${MESSAGES_SERVICE_URL}/api/messages/admin/reports`, {
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
      console.error('Error fetching message reports:', errorText);
      return NextResponse.json(
        { error: 'Failed to fetch message reports' },
        { status: response.status }
      );
    }

    const messageReports = await response.json();
    return NextResponse.json(messageReports);
  } catch (error) {
    console.error('Error in message reports API:', error);
    return NextResponse.json(
      { error: 'Internal server error' },
      { status: 500 }
    );
  }
} 