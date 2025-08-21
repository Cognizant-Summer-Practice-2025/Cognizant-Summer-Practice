/**
 * Service URLs configuration
 * Centralized configuration for microservice endpoints
 */

export const SERVICES = {
  AUTH_USER_SERVICE: process.env.NEXT_PUBLIC_AUTH_USER_SERVICE!,
  HOME_PORTFOLIO_SERVICE: process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE!,
  MESSAGES_SERVICE: process.env.NEXT_PUBLIC_MESSAGES_SERVICE!,
  ADMIN_SERVICE: process.env.NEXT_PUBLIC_ADMIN_SERVICE!,
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