import GithubProvider from "next-auth/providers/github"
import GoogleProvider from "next-auth/providers/google"
import { checkUserExists, checkOAuthProvider, addOAuthProvider } from "@/lib/user"
import type { AuthOptions } from "next-auth"

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
          // OAuth provider already exists - allow sign-in
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
      
      // Always redirect to home page after successful sign-in
      return url.startsWith(baseUrl) ? url : `${baseUrl}/`;
    },
    
    async session({ session }) {
      return session;
    },
    
    async jwt({ token }) {
      return token;
    },
  },
}