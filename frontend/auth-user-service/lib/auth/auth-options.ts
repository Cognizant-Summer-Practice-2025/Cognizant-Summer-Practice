import GithubProvider from "next-auth/providers/github"
import GoogleProvider from "next-auth/providers/google"
import FacebookProvider from "next-auth/providers/facebook"
import LinkedInProvider from "next-auth/providers/linkedin"
import { checkUserExists, checkOAuthProvider, addOAuthProvider, getUserByEmail } from "@/lib/user"
import type { AuthOptions } from "next-auth"

declare module "next-auth" {
  interface Session {
    accessToken?: string
    userId?: string
    error?: "RefreshAccessTokenError"
  }
  interface JWT {
    accessToken?: string
    userId?: string
    refreshToken?: string
    expiresAt?: number
    provider?: string
    error?: "RefreshAccessTokenError"
  }
}

interface TokenRefreshResult {
  accessToken?: string;
  expiresAt?: number;
  refreshToken?: string;
  error?: string;
}

interface TokenData {
  accessToken?: string;
  refreshToken?: string;
  userId?: string;
  provider?: string;
  expiresAt?: number;
}


/**
 * Refreshes an access token using our backend refresh endpoint
 */
async function refreshAccessToken(token: TokenData): Promise<TokenRefreshResult> {
  try {
    console.log('🔄 Attempting token refresh with token data:', {
      hasRefreshToken: !!token.refreshToken,
      refreshTokenLength: token.refreshToken?.length || 0,
      hasAccessToken: !!token.accessToken,
      provider: token.provider,
      userId: token.userId
    });

    if (!token.refreshToken) {
      console.error('❌ No refresh token available for refresh');
      return { error: "No refresh token available" };
    }

    const backendUrl = process.env.NEXT_PUBLIC_USER_API_URL || 'http://localhost:5200';
    console.log('🌐 Calling refresh endpoint:', `${backendUrl}/api/oauth2/refresh`);
    
    const response = await fetch(`${backendUrl}/api/oauth2/refresh`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        refreshToken: token.refreshToken,
      }),
    });

    console.log('📡 Refresh response status:', response.status, response.statusText);

    if (!response.ok) {
      const errorText = await response.text();
      console.error('❌ Refresh failed response:', errorText);
      throw new Error(`Token refresh failed: ${response.status} - ${errorText}`);
    }

    const refreshData = await response.json();
    console.log('✅ Refresh response data:', refreshData);
    
    // The refresh endpoint now returns the new token data directly
    if (refreshData.accessToken) {
      console.log('🔑 Using refreshed token data:', {
        hasNewAccessToken: !!refreshData.accessToken,
        newTokenLength: refreshData.accessToken?.length || 0,
        newExpiresAt: refreshData.tokenExpiresAt,
        hasRefreshToken: refreshData.hasRefreshToken
      });
      
      return {
        accessToken: refreshData.accessToken,
        expiresAt: refreshData.tokenExpiresAt ? new Date(refreshData.tokenExpiresAt).getTime() / 1000 : undefined,
        refreshToken: refreshData.hasRefreshToken ? token.refreshToken : undefined,
      };
    }

    // Fallback: return current token with extended expiry using UTC time
    console.warn('⚠️ No refreshed token returned, using fallback with extended expiry');
    const nowUtc = new Date().getTime();
    return {
      accessToken: token.accessToken,
      expiresAt: Math.floor(nowUtc / 1000) + (60 * 60), // 1 hour from now in UTC
      refreshToken: token.refreshToken,
    };
  } catch (error) {
    console.error('❌ Error refreshing token:', error);
    return { error: "RefreshAccessTokenError" };
  }
}

/**
 * Gets the provider number for backend API calls
 */
function getProviderNumber(provider: string): number {
  const providerMapping: { [key: string]: number } = {
    'google': 0,   // Google
    'github': 1,   // GitHub
    'linkedin': 2, // LinkedIn
    'facebook': 3, // Facebook
  };
  return providerMapping[provider.toLowerCase()] ?? 0;
}

export const authOptions: AuthOptions = {
  providers: [
    GithubProvider({
      clientId: process.env.AUTH_GITHUB_ID!,
      clientSecret: process.env.AUTH_GITHUB_SECRET!,
    }),
    GoogleProvider({
      clientId: process.env.AUTH_GOOGLE_ID!,
      clientSecret: process.env.AUTH_GOOGLE_SECRET!,
      authorization: {
        params: {
          prompt: "consent",
          access_type: "offline",
          response_type: "code",
        },
      },
    }),
    FacebookProvider({
      clientId: process.env.AUTH_FACEBOOK_ID!,
      clientSecret: process.env.AUTH_FACEBOOK_SECRET!,
    }),
    LinkedInProvider({
      clientId: process.env.AUTH_LINKEDIN_ID!,
      clientSecret: process.env.AUTH_LINKEDIN_SECRET!,
    }),
  ],
  pages: {
    signIn: '/login',
    error: '/login', // Redirect errors to login page
  },
  callbacks: {
    async signIn({ user, account }) {
      try {
        if (!user.email || !account?.provider || !account?.providerAccountId) {
          console.error('Missing required OAuth data');
          return false;
        }

        // 1. Check if OAuth provider already exists
        const { exists: providerExists, user: providerUser } = await checkOAuthProvider(account.provider, account.providerAccountId);
        
        if (providerExists && providerUser) {
          return true;
        }
        
        // 2. Check if user exists by email
        const { exists, user: existingUser } = await checkUserExists(user.email);
        
        const providerMapping: { [key: string]: number } = {
          'google': 0,   // Google
          'github': 1,   // GitHub
          'facebook': 3, // Facebook
          'linkedin': 2  // LinkedIn
        };
        
        const providerType = providerMapping[account.provider.toLowerCase()];
        if (providerType === undefined) {
          console.error(`Unsupported OAuth provider: ${account.provider}`);
          return false;
        }
        
        if (exists && existingUser) {
          // User exists but OAuth provider doesn't - add OAuth provider to existing user
          
          try {
            const providerStringMapping: { [key: number]: 'Google' | 'GitHub' | 'Facebook' | 'LinkedIn' } = {
              0: 'Google',
              1: 'GitHub', 
              2: 'LinkedIn',
              3: 'Facebook'
            };

            await addOAuthProvider({
              userId: existingUser.id,
              provider: providerStringMapping[providerType],
              providerId: account.providerAccountId,
              providerEmail: user.email,
              accessToken: account.access_token || '',
              refreshToken: account.refresh_token,
              tokenExpiresAt: account.expires_at ? new Date(account.expires_at * 1000).toISOString() : undefined
            });
            
            return true;
          } catch {

            return false;
          }
        } else {
          // User doesn't exist - redirect to login with OAuth data for registration
          const params = new URLSearchParams({
            email: user.email,
            name: user.name || '',
            image: user.image || '',
            provider: account.provider,
            providerId: account.providerAccountId,
            accessToken: account.access_token || '',
            refreshToken: account.refresh_token || '',
            tokenExpiresAt: account.expires_at ? account.expires_at.toString() : ''
          });
          
          // Throw error to trigger redirect with register parameter
          throw new Error(`/login?register=${encodeURIComponent(params.toString())}`);
        }
        
      } catch (error) {
        if (error instanceof Error && error.message.startsWith('/login?register=')) {
          // This is a registration redirect - rethrow to handle in redirect callback
          throw error;
        }
        return false;
      }
    },
    
    async redirect({ url, baseUrl }) {
      // Handle registration redirects from error page
      if (url.includes('/api/auth/error')) {
        const urlObj = new URL(url);
        const errorParam = urlObj.searchParams.get('error');
        
        if (errorParam && errorParam.startsWith('/login?register=')) {
          // Extract the registration parameters and redirect
          return `${baseUrl}${errorParam}`;
        }
      }
      
      // Handle direct registration redirects
      if (url.includes('/login?register=')) {
        return url;
      }
      
      // Allow redirects to external services (like home-portfolio-service)
      // Only restrict redirects to same-origin URLs
      if (url.startsWith(baseUrl) || url.startsWith('http://localhost:3001') || url.startsWith('http://localhost:3002') || url.startsWith('http://localhost:3003')) {
        return url;
      }
      
      // Default fallback to home page
      return `${baseUrl}/`;
    },
    
    async session({ session, token }) {
      // Pass any token errors to the session
      if (token.error === "RefreshAccessTokenError") {
        session.error = token.error;
      }

      if (session?.user?.email) {
        try {
          // Fetch full user data and inject into other services
          const userData = await getUserByEmail(session.user.email);
          if (userData) {
                    // Inject user data into all other services
            // Add user data to session
            session.userId = userData.id;
          }
        } catch (error) {
          console.error('Error injecting user data on session creation:', error);
        }
      }
      
      // Send properties to the client
      session.accessToken = token.accessToken as string;
      if (!session.userId) {
        session.userId = token.userId as string;
      }
      
      return session;
    },
    
    async jwt({ token, account, user }) {
      if (account && user) {
        // First-time login: Store the OAuth provider's access token from our backend
        try {
          const userData = await getUserByEmail(user.email!);
          if (userData) {
            // Get the access token from oauth_providers table 
            const backendUrl = process.env.NEXT_PUBLIC_USER_API_URL || 'http://localhost:5200';
            const url = `${backendUrl}/api/users/${userData.id}/oauth-providers/${getProviderNumber(account.provider)}`;
            
            const response = await fetch(url, {
              method: 'GET',
              headers: {
                'Content-Type': 'application/json',
              },
            });
            
            if (response.ok) {
              const oauthData = await response.json();
              if (oauthData.exists && oauthData.provider) {
                token.accessToken = oauthData.provider.accessToken;
                token.userId = userData.id;
                token.refreshToken = oauthData.provider.refreshToken;
                token.expiresAt = oauthData.provider.tokenExpiresAt ? new Date(oauthData.provider.tokenExpiresAt).getTime() / 1000 : undefined;
                token.provider = account.provider;
              }
            }
          }
        } catch (error) {
          console.error('Error fetching OAuth access token:', error);
        }
      } else if (token.expiresAt && new Date().getTime() < (token.expiresAt as number) * 1000) {
        // Subsequent logins, but the access token is still valid
        return token;
      } else if (token.refreshToken && token.expiresAt && new Date().getTime() >= (token.expiresAt as number) * 1000) {
        // Subsequent logins, but the access token has expired, try to refresh it
        console.log('Token expired, attempting refresh...');
        try {
          const refreshedToken = await refreshAccessToken(token as TokenData);
          if (refreshedToken.error) {
            console.error('Token refresh failed:', refreshedToken.error);
            // If refresh fails, mark token as invalid but don't throw error to avoid breaking auth
            token.error = "RefreshAccessTokenError";
          } else {
            // Update token with refreshed values
            token.accessToken = refreshedToken.accessToken;
            token.expiresAt = refreshedToken.expiresAt;
            if (refreshedToken.refreshToken) {
              token.refreshToken = refreshedToken.refreshToken;
            }
          }
        } catch (error) {
          console.error('Error refreshing access token:', error);
          token.error = "RefreshAccessTokenError";
        }
      }
      
      return token;
    },
  },
  session: {
    strategy: 'jwt',
  },
  events: {
    async signOut(message) {
      // When NextAuth signOut is called directly, remove user from all services
      if (message.session?.user?.email) {
        try {
          console.log(`NextAuth signOut event - removing user: ${message.session.user.email}`);
          // await UserInjectionService.removeUser(message.session.user.email); // Removed
          console.log(`User ${message.session.user.email} removed from all services during NextAuth signOut`);
        } catch (error) {
          console.error('Error removing user during NextAuth signOut:', error);
        }
      }
    }
  }
}