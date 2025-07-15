import NextAuth from "next-auth"
import GithubProvider from "next-auth/providers/github"
import GoogleProvider from "next-auth/providers/google"
import { checkUserExists, checkUserOAuthProvider, updateOAuthProvider, addOAuthProvider } from "@/lib/api"

export const authOptions = {
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
    async signIn({ user, account, profile }: any) {
      try {
        if (user.email) {
          const { exists, user: existingUser } = await checkUserExists(user.email);
          
          if (!exists) {
            // User doesn't exist, redirect to registration
            const params = new URLSearchParams({
              email: user.email,
              name: user.name || '',
              image: user.image || '',
              provider: account.provider,
              providerId: account.providerAccountId,
              accessToken: account.access_token || '',
              refreshToken: account.refresh_token || '',
              tokenExpiresAt: account.expires_at ? new Date(account.expires_at * 1000).toISOString() : ''
            });
            return `/register?${params.toString()}`;
          } else if (existingUser) {
            // User exists, check if they have this OAuth provider
            const providerMapping: { [key: string]: 'Google' | 'GitHub' | 'Facebook' | 'LinkedIn' } = {
              'google': 'Google',
              'github': 'GitHub', 
              'facebook': 'Facebook',
              'linkedin': 'LinkedIn'
            };
            
            const providerType = providerMapping[account.provider.toLowerCase()];
            if (!providerType) {
              console.alert(`Unsupported OAuth provider: ${account.provider}`);
              return false;
            }

            const { exists: providerExists, provider: existingProvider } = await checkUserOAuthProvider(existingUser.id, providerType);
            
            if (providerExists && existingProvider) {
              // Same provider exists, update tokens
              try {
                await updateOAuthProvider(existingProvider.id, {
                  accessToken: account.access_token,
                  refreshToken: account.refresh_token,
                  tokenExpiresAt: account.expires_at ? new Date(account.expires_at * 1000).toISOString() : undefined
                });
                console.log(`Updated OAuth tokens for provider ${providerType}`);
              } catch (error) {
                console.error('Error updating OAuth provider tokens:', error);
                return false;
              }
            } else {
              // Different provider, add new OAuth provider to user
              try {
                await addOAuthProvider({
                  userId: existingUser.id,
                  provider: providerType,
                  providerId: account.providerAccountId,
                  providerEmail: user.email,
                  accessToken: account.access_token || '',
                  refreshToken: account.refresh_token,
                  tokenExpiresAt: account.expires_at ? new Date(account.expires_at * 1000).toISOString() : undefined
                });
                console.log(`Added new OAuth provider ${providerType} to existing user`);
              } catch (error) {
                console.error('Error adding new OAuth provider:', error);
                return false;
              }
            }
          }
        }
        return true;
      } catch (error) {
        console.error('Error during sign-in callback:', error);
        return false;
      }
    },
    async redirect({ url, baseUrl }: { url: string; baseUrl: string }) {
      // If redirecting to registration, allow it
      if (url.includes('/register')) {
        return url;
      }
      // Otherwise, default behavior
      return url.startsWith(baseUrl) ? url : `${baseUrl}/profile`;
    },
    async session({ session, token }: any) {
      // Add OAuth provider info to session if available
      if (token.provider) {
        session.provider = token.provider;
        session.providerId = token.providerId;
        session.accessToken = token.accessToken;
      }
      return session;
    },
    async jwt({ token, user, account }: any) {
      // Store OAuth provider info in JWT token
      if (account) {
        token.provider = account.provider;
        token.providerId = account.providerAccountId;
        token.accessToken = account.access_token;
        token.refreshToken = account.refresh_token;
        token.tokenExpiresAt = account.expires_at;
      }
      return token;
    },
  },
}

const handler = NextAuth(authOptions)

export { handler as GET, handler as POST } 