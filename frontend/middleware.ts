import { NextRequest, NextResponse } from "next/server";
import { getToken } from "next-auth/jwt";

const protectedRoutes = ["/profile", "/publish"];

export default async function middleware(request: NextRequest) {
    const token = await getToken({ 
        req: request, 
        secret: process.env.AUTH_SECRET 
    });
    
    const isProtectedRoute = protectedRoutes.some(route => 
        request.nextUrl.pathname.startsWith(route)
    );

    if (isProtectedRoute && !token) {
        return NextResponse.redirect(new URL("/login", request.url));
    }

    return NextResponse.next();
}

export const config = {
    matcher: ["/profile/:path*", "/publish/:path*"]
}; 