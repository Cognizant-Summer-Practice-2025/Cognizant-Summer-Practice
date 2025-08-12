/**
 * Logger utility for the admin service
 * Following user preference to use loggers instead of console statements
 */
export class Logger {
  private static serviceName = 'admin-service';

  static info(message: string, ...args: any[]): void {
    console.log(`[${this.serviceName}] INFO: ${message}`, ...args);
  }

  static warn(message: string, ...args: any[]): void {
    console.warn(`[${this.serviceName}] WARN: ${message}`, ...args);
  }

  static error(message: string, error?: any, ...args: any[]): void {
    console.error(`[${this.serviceName}] ERROR: ${message}`, error, ...args);
  }

  static debug(message: string, ...args: any[]): void {
    if (process.env.NODE_ENV === 'development') {
      console.debug(`[${this.serviceName}] DEBUG: ${message}`, ...args);
    }
  }
}
