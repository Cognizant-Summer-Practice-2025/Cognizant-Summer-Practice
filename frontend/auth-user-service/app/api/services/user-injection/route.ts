import { NextRequest, NextResponse } from 'next/server';
import { SERVICES } from '@/lib/config/services';

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
  accessToken?: string;
}

/**
 * Inject user data into all other services
 */
async function injectUserToServices(userData: ServiceUserData) {
  const services = [
    { name: 'ADMIN_SERVICE', url: SERVICES.ADMIN_SERVICE },
    { name: 'HOME_PORTFOLIO_SERVICE', url: SERVICES.HOME_PORTFOLIO_SERVICE },
    { name: 'MESSAGES_SERVICE', url: SERVICES.MESSAGES_SERVICE },
  ];

  const results = await Promise.allSettled(
    services.map(async (service) => {
      try {
        const response = await fetch(`${service.url}/api/user/inject`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'X-Service-Auth': process.env.SERVICE_AUTH_SECRET || 'default-secret',
          },
          body: JSON.stringify(userData),
        });

        if (!response.ok) {
          const errorText = await response.text().catch(() => 'Unknown error');
          console.error(`Failed to inject user to ${service.name}:`, {
            status: response.status,
            statusText: response.statusText,
            url: `${service.url}/api/user/inject`,
            error: errorText
          });
          throw new Error(`Failed to inject user to ${service.name}: ${response.statusText} - ${errorText}`);
        }

        return { service: service.name, success: true };
      } catch (error) {
        console.error(`Error injecting user to ${service.name}:`, error);
        return { service: service.name, success: false, error: error instanceof Error ? error.message : String(error) };
      }
    })
  );

  return results;
}

/**
 * Remove user data from all services (including self)
 */
async function removeUserFromServices(userEmail: string) {
  const services = [
    { name: 'AUTH_USER_SERVICE', url: SERVICES.AUTH_USER_SERVICE },
    { name: 'ADMIN_SERVICE', url: SERVICES.ADMIN_SERVICE },
    { name: 'HOME_PORTFOLIO_SERVICE', url: SERVICES.HOME_PORTFOLIO_SERVICE },
    { name: 'MESSAGES_SERVICE', url: SERVICES.MESSAGES_SERVICE },
  ];

  const results = await Promise.allSettled(
    services.map(async (service) => {
      try {
        const response = await fetch(`${service.url}/api/user/remove`, {
          method: 'DELETE',
          headers: {
            'Content-Type': 'application/json',
            'X-Service-Auth': process.env.SERVICE_AUTH_SECRET || 'default-secret',
          },
          body: JSON.stringify({ email: userEmail }),
        });

        if (!response.ok) {
          const errorText = await response.text().catch(() => 'Unknown error');
          console.error(`Failed to remove user from ${service.name}:`, {
            status: response.status,
            statusText: response.statusText,
            url: `${service.url}/api/user/remove`,
            error: errorText
          });
          throw new Error(`Failed to remove user from ${service.name}: ${response.statusText} - ${errorText}`);
        }

        return { service: service.name, success: true };
      } catch (error) {
        console.error(`Error removing user from ${service.name}:`, error);
        return { service: service.name, success: false, error: error instanceof Error ? error.message : String(error) };
      }
    })
  );

  return results;
}

export async function POST(request: NextRequest) {
  try {
    const { action, userData, userEmail } = await request.json();

    if (action === 'inject') {
      if (!userData) {
        return NextResponse.json({ error: 'User data is required for injection' }, { status: 400 });
      }

      const results = await injectUserToServices(userData);
      return NextResponse.json({ success: true, results });
    }

    if (action === 'remove') {
      if (!userEmail) {
        return NextResponse.json({ error: 'User email is required for removal' }, { status: 400 });
      }

      const results = await removeUserFromServices(userEmail);
      return NextResponse.json({ success: true, results });
    }

    return NextResponse.json({ error: 'Invalid action. Use "inject" or "remove"' }, { status: 400 });
  } catch (error) {
    console.error('Error in user injection API:', error);
    return NextResponse.json(
      { error: 'Internal server error' },
      { status: 500 }
    );
  }
}