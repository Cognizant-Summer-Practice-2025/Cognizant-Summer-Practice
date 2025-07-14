import NextAuth from "next-auth"
import GithubProvider from "next-auth/providers/github"
import GoogleProvider from "next-auth/providers/google"
import { checkUserExists } from "@/lib/api"

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
          const { exists } = await checkUserExists(user.email);
          
          if (!exists) {
            // Store user info in session for registration flow
            return `/register?email=${encodeURIComponent(user.email)}&name=${encodeURIComponent(user.name || '')}&image=${encodeURIComponent(user.image || '')}`;
          }
        }
        return true;
      } catch (error) {
        console.error('Error during sign-in callback:', error);
        return true; // Allow sign-in even if check fails
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
      return session;
    },
    async jwt({ token, user }: any) {
      return token;
    },
  },
}

const handler = NextAuth(authOptions)

export { handler as GET, handler as POST } 