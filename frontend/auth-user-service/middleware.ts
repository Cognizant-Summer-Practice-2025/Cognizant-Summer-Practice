import { NextRequest, NextResponse } from "next/server";
import { getToken } from "next-auth/jwt";

const protectedRoutes = ["/profile", "/publish", "/ai"];

// Define allowed origins for CORS
const allowedOrigins = [
  process.env.NEXT_PUBLIC_AUTH_USER_SERVICE || 'http://localhost:3000',
  process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE || 'http://localhost:3001',
  process.env.NEXT_PUBLIC_MESSAGES_SERVICE || 'http://localhost:3002',
  process.env.NEXT_PUBLIC_ADMIN_SERVICE || 'http://localhost:3003',
  // Add any other services here
];

export default async function middleware(request: NextRequest) {
    const origin = request.headers.get('origin');
    
    // Handle CORS for API routes
    if (request.nextUrl.pathname.startsWith('/api/')) {
        // Handle preflight OPTIONS request
        if (request.method === 'OPTIONS') {
            return new NextResponse(null, {
                status: 200,
                headers: {
                    'Access-Control-Allow-Origin': origin && allowedOrigins.includes(origin) ? origin : allowedOrigins[0],
                    'Access-Control-Allow-Methods': 'GET, POST, PUT, DELETE, OPTIONS',
                    'Access-Control-Allow-Headers': 'Content-Type, Authorization, X-Service-Auth',
                    'Access-Control-Allow-Credentials': 'true',
                },
            });
        }

        // For actual requests, add CORS headers
        const response = NextResponse.next();
        
        if (origin && allowedOrigins.includes(origin)) {
            response.headers.set('Access-Control-Allow-Origin', origin);
        } else {
            response.headers.set('Access-Control-Allow-Origin', allowedOrigins[0]);
        }
        
        response.headers.set('Access-Control-Allow-Methods', 'GET, POST, PUT, DELETE, OPTIONS');
        response.headers.set('Access-Control-Allow-Headers', 'Content-Type, Authorization, X-Service-Auth');
        response.headers.set('Access-Control-Allow-Credentials', 'true');
        
        return response;
    }

    // Handle protected routes authentication
    const token = await getToken({ 
        req: request, 
        secret: process.env.AUTH_SECRET 
    });
    
    const isProtectedRoute = protectedRoutes.some(route => 
        request.nextUrl.pathname.startsWith(route)
    );

    if (isProtectedRoute && !token) {
        // Preserve the current URL when redirecting to login
        const loginUrl = new URL("/login", request.url);
        loginUrl.searchParams.set("callbackUrl", request.url);
        return NextResponse.redirect(loginUrl);
    }

    return NextResponse.next();
}

export const config = {
    matcher: [
        "/profile/:path*", 
        "/publish/:path*",
        "/ai/:path*",
        "/api/:path*"  // Include API routes for CORS handling
    ]
};