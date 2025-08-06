import { User } from '@/lib/user/interfaces';

interface ServiceUserData {
  id: string;
  email: string;
  username: string;
  firstName?: string;
  lastName?: string;
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
 * Service for managing user injection/removal across microservices
 */
export class UserInjectionService {
  private static baseUrl = process.env.NEXT_PUBLIC_AUTH_USER_SERVICE || 'http://localhost:3000';

  /**
   * Inject user data into all other services
   */
  static async injectUser(user: User, accessToken?: string): Promise<void> {
    try {
      const userData: ServiceUserData = {
        id: user.id,
        email: user.email,
        username: user.username,
        firstName: user.firstName,
        lastName: user.lastName,
        professionalTitle: user.professionalTitle,
        bio: user.bio,
        location: user.location,
        profileImage: user.avatarUrl,
        isActive: user.isActive,
        isAdmin: user.isAdmin || false,
        lastLoginAt: user.lastLoginAt,
        accessToken,
      };

      const response = await fetch(`${this.baseUrl}/api/services/user-injection`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          action: 'inject',
          userData,
        }),
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(`Failed to inject user: ${errorData.error || response.statusText}`);
      }

      const result = await response.json();
      console.log('User injection results:', result.results);
    } catch (error) {
      console.error('Error injecting user across services:', error);
      // Don't throw error to prevent login failure if services are down
    }
  }

  /**
   * Remove user data from all other services
   */
  static async removeUser(userEmail: string): Promise<void> {
    try {
      const response = await fetch(`${this.baseUrl}/api/services/user-injection`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          action: 'remove',
          userEmail,
        }),
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(`Failed to remove user: ${errorData.error || response.statusText}`);
      }

      const result = await response.json();
      console.log('User removal results:', result.results);
    } catch (error) {
      console.error('Error removing user across services:', error);
      // Don't throw error to prevent logout failure if services are down
    }
  }
}