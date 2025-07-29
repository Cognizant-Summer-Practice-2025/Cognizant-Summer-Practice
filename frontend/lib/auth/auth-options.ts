import GithubProvider from "next-auth/providers/github"
import GoogleProvider from "next-auth/providers/google"
import { checkUserExists, checkOAuthProvider, addOAuthProvider, getUserByEmail } from "@/lib/user"
import type { AuthOptions } from "next-auth"

// Extend the Session type to include custom properties
declare module "next-auth" {
  interface Session {
  provider?: string;
  providerId?: string;
  accessToken?: string;
  userData?: unknown;
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
  ],
  pages: {
    signIn: '/login',
  },
  callbacks: {
    async signIn({ user, account }) {
      try {
        if (user.email) {
          const { exists, user: existingUser } = await checkUserExists(user.email);
          
          if (!exists) {
            // User doesn't exist, redirect to registration
            const params = new URLSearchParams({
              email: user.email,
              name: user.name || '',
              avatar: user.image || '',
              provider: account?.provider || '',
              providerId: account?.providerAccountId || ''
            });
            
            // Redirect to registration page with pre-filled data
            throw new Error(`/register?${params.toString()}`);
          }
          
          // Check if this OAuth provider is already linked to any user
          if (account?.provider && account?.providerAccountId && existingUser) {
            const { exists: providerExists, user: providerUser } = await checkOAuthProvider(account.provider, account.providerAccountId);
            
            if (!providerExists) {
              // Add this OAuth provider to the existing user
              try {
                const providerMapping: { [key: string]: 'Google' | 'GitHub' | 'Facebook' | 'LinkedIn' } = {
                  'google': 'Google',
                  'github': 'GitHub',
                  'facebook': 'Facebook',
                  'linkedin': 'LinkedIn'
                };
                
                const providerType = providerMapping[account.provider.toLowerCase()];
                if (!providerType) {
                  console.error(`Unsupported OAuth provider: ${account.provider}`);
                  return false;
                }

                await addOAuthProvider({
                  userId: existingUser.id,
                  provider: providerType,
                  providerId: account.providerAccountId,
                  providerEmail: user.email,
                  accessToken: account.access_token || '',
                  refreshToken: account.refresh_token,
                  tokenExpiresAt: account.expires_at ? new Date(account.expires_at * 1000).toISOString() : undefined
                });
                console.log(`Added ${account.provider} provider to existing user ${user.email}`);
              } catch (error) {
                console.error('Error adding OAuth provider:', error);
                return false;
              }
            } else if (providerUser && providerUser.id === existingUser.id) {
              // Same user, update existing provider tokens if needed
              try {
                // We would need the provider ID from the database to update, so skip update for now
                console.log(`OAuth provider ${account.provider} already linked to user ${user.email}`);
              } catch (error) {
                console.error('Error updating OAuth provider:', error);
                // Don't fail sign-in if update fails
              }
            } else if (providerUser && providerUser.id !== existingUser.id) {
              // OAuth provider is linked to a different user
              console.error(`OAuth provider ${account.provider} is already linked to a different user`);
              return false;
            }
          }
          
          return true;
        }
        
        return false;
      } catch (error) {
        if (error instanceof Error && error.message.startsWith('/register')) {
          // This is a redirect to registration page
          throw error;
        }
        console.error('Error in signIn callback:', error);
        return false;
      }
    },
    async redirect({ url, baseUrl }) {
      // If redirecting to registration, allow it
      if (url.includes('/register')) {
        return url;
      }
      // Always redirect to home page (root) instead of profile
      return url.startsWith(baseUrl) ? url : `${baseUrl}/`;
    },
    async session({ session, token }) {
      // Add OAuth provider info to session if available
      if (token.provider) {
        session.provider = token.provider as string;
        session.providerId = token.providerId as string;
        session.accessToken = token.accessToken as string;
      }
      
      // Add full user data to session if available in token
      if (token.userData) {
        session.userData = token.userData;
      }
      
      return session;
    },
    async jwt({ token, user, account }) {
      // Store OAuth provider info in JWT token
      if (account) {
        token.provider = account.provider;
        token.providerId = account.providerAccountId;
        token.accessToken = account.access_token;
        token.refreshToken = account.refresh_token;
        token.tokenExpiresAt = account.expires_at;
      }
      
      // Fetch and store user data in token for easy access
      if (user?.email && !token.userData) {
        try {
          const userData = await getUserByEmail(user.email);
          if (userData) {
            token.userData = userData;
          }
        } catch (error) {
          console.error('Error fetching user data for token:', error);
        }
      }
      
      return token;
    },
  },
}
