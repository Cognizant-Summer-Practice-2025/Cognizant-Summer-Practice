export const SERVICES = {
  AUTH_USER_SERVICE: process.env.NEXT_PUBLIC_AUTH_USER_SERVICE || '',
  HOME_PORTFOLIO_SERVICE: process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE || '',
  MESSAGES_SERVICE: process.env.NEXT_PUBLIC_MESSAGES_SERVICE || '',
  ADMIN_SERVICE: process.env.NEXT_PUBLIC_ADMIN_SERVICE || '',
} as const;

export function buildServiceUrl(service: keyof typeof SERVICES, path: string): string {
  const baseUrl = SERVICES[service];
  const cleanPath = path.startsWith('/') ? path.slice(1) : path;
  if (!baseUrl) {
    if (typeof window !== 'undefined') {
      // Avoid crashing builds; this will no-op at runtime if misconfigured
      console.error(`[admin][config] Missing base URL for ${service}`);
      return `/${cleanPath}`;
    }
    return `/${cleanPath}`;
  }
  return `${baseUrl}/${cleanPath}`;
}

export function redirectToService(service: keyof typeof SERVICES, path: string): void {
  const url = buildServiceUrl(service, path);
  window.location.href = url;
}


