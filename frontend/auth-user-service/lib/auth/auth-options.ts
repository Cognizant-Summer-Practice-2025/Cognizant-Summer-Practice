import GithubProvider from "next-auth/providers/github"
import GoogleProvider from "next-auth/providers/google"
import FacebookProvider from "next-auth/providers/facebook"
import LinkedInProvider from "next-auth/providers/linkedin"
import { checkUserExists, checkOAuthProvider, addOAuthProvider, getUserByEmail } from "@/lib/user"
import { UserInjectionService } from "@/lib/services/user-injection-service"
import type { AuthOptions } from "next-auth"

declare module "next-auth" {
  interface Session {
    accessToken?: string
    userId?: string
  }
  interface JWT {
    accessToken?: string
    userId?: string
  }
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
          // OAuth provider already exists - inject user data and allow sign-in
          try {
            const userData = await getUserByEmail(user.email);
            if (userData) {
              await UserInjectionService.injectUser(userData);
              console.log('User injected on sign-in:', user.email);
            }
          } catch (error) {
            console.error('Error injecting user on sign-in:', error);
          }
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
            
            // Inject user data after adding OAuth provider
            try {
              const userData = await getUserByEmail(user.email);
              if (userData) {
                await UserInjectionService.injectUser(userData);
                console.log('User injected after adding OAuth provider:', user.email);
              }
            } catch (error) {
              console.error('Error injecting user after adding OAuth provider:', error);
            }
            
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
      
      // Always redirect to home page after successful sign-in
      return url.startsWith(baseUrl) ? url : `${baseUrl}/`;
    },
    
    async session({ session, token }) {
      if (session?.user?.email) {
        try {
          // Fetch full user data and inject into other services
          const userData = await getUserByEmail(session.user.email);
          if (userData) {
            // Inject user data into all other services
            await UserInjectionService.injectUser(userData);
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
        // Store the OAuth provider's access token from our backend
        try {
          const userData = await getUserByEmail(user.email!);
          if (userData) {
            // Get the access token from oauth_providers table (unauthenticated call during auth flow)
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
              }
            }
          }
        } catch (error) {
          console.error('Error fetching OAuth access token:', error);
        }
      }
      return token;
    },
  },
  session: {
    strategy: 'jwt',
  },
}

function getProviderNumber(provider: string): number {
  const providerMap: { [key: string]: number } = {
    'google': 0,    // Google
    'github': 1,    // GitHub  
    'linkedin': 2,  // LinkedIn
    'facebook': 3   // Facebook
  }
  return providerMap[provider.toLowerCase()] ?? 0
}

function getProviderName(provider: string): string {
  const providerMap: { [key: string]: string } = {
    'google': 'Google',
    'github': 'GitHub',
    'linkedin': 'LinkedIn',
    'facebook': 'Facebook'
  }
  return providerMap[provider.toLowerCase()] ?? 'Google'
}