export const SERVICES = {
  AUTH_USER_SERVICE: process.env.NEXT_PUBLIC_AUTH_USER_SERVICE || (() => { throw new Error('NEXT_PUBLIC_AUTH_USER_SERVICE not set'); })(),
  HOME_PORTFOLIO_SERVICE: process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE || (() => { throw new Error('NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE not set'); })(),
  MESSAGES_SERVICE: process.env.NEXT_PUBLIC_MESSAGES_SERVICE || (() => { throw new Error('NEXT_PUBLIC_MESSAGES_SERVICE not set'); })(),
  ADMIN_SERVICE: process.env.NEXT_PUBLIC_ADMIN_SERVICE || (() => { throw new Error('NEXT_PUBLIC_ADMIN_SERVICE not set'); })(),
} as const;

export function buildServiceUrl(service: keyof typeof SERVICES, path: string): string {
  const baseUrl = SERVICES[service];
  const cleanPath = path.startsWith('/') ? path.slice(1) : path;
  return `${baseUrl}/${cleanPath}`;
}

export function redirectToService(service: keyof typeof SERVICES, path: string): void {
  const url = buildServiceUrl(service, path);
  window.location.href = url;
}


