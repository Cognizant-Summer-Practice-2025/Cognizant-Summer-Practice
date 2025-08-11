/**
 * Service URLs configuration
 * Centralized configuration for microservice endpoints
 */

export const SERVICES = {
  AUTH_USER_SERVICE: process.env.NEXT_PUBLIC_AUTH_USER_SERVICE || process.env.AUTH_USER_SERVICE || 'http://localhost:3000',
  HOME_PORTFOLIO_SERVICE: process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE || process.env.HOME_PORTFOLIO_SERVICE || 'http://localhost:3001',
  MESSAGES_SERVICE: process.env.NEXT_PUBLIC_MESSAGES_SERVICE || process.env.MESSAGES_SERVICE || 'http://localhost:3002',
  ADMIN_SERVICE: process.env.NEXT_PUBLIC_ADMIN_SERVICE || process.env.ADMIN_SERVICE || 'http://localhost:3003',
} as const;

/**
 * Helper function to build URLs for external service redirects
 */
export function buildServiceUrl(service: keyof typeof SERVICES, path: string): string {
  const baseUrl = SERVICES[service];
  // Remove leading slash from path if present to avoid double slashes
  const cleanPath = path.startsWith('/') ? path.slice(1) : path;
  return `${baseUrl}/${cleanPath}`;
}

/**
 * Helper function to redirect to external service
 */
export function redirectToService(service: keyof typeof SERVICES, path: string): void {
  const url = buildServiceUrl(service, path);
  window.location.href = url;
} 