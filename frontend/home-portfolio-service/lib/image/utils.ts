const API_BASE_URL = process.env.NEXT_PUBLIC_PORTFOLIO_API_URL ?? '';

/**
 * Convert a server image path to a full API URL
 * @param imagePath - Path like "server/portfolio/projects/uuid.jpg"
 * @returns Full API URL like "https://backend-portfolio.lemongrass-88207da5.northeurope.azurecontainerapps.io/api/Image/projects/uuid.jpg"
 */
export function getImageUrl(imagePath: string): string {
  if (!imagePath) return '';
  
  // If it's already a full URL, return as is
  if (imagePath.startsWith('http://') || imagePath.startsWith('https://')) {
    return imagePath;
  }
  
  // If it's a data URL (for previews), return as is
  if (imagePath.startsWith('data:')) {
    return imagePath;
  }
  
  // Parse the server path format: "server/portfolio/{subfolder}/{filename}"
  const pathParts = imagePath.split('/');
  
  if (pathParts.length >= 3 && pathParts[0] === 'server' && pathParts[1] === 'portfolio') {
    const subfolder = pathParts[2];
    const filename = pathParts.slice(3).join('/'); // In case filename has slashes
    
    if (!API_BASE_URL) {
      throw new Error('Portfolio API URL is not configured');
    }
    return `${API_BASE_URL}/api/Image/${subfolder}/${filename}`;
  }
  
  // If path format is unexpected, return the original path
  console.warn('Unexpected image path format:', imagePath);
  return imagePath;
}

/**
 * Get a safe image URL with fallback
 * @param imagePath - Image path or URL
 * @param fallbackUrl - Fallback URL if image path is invalid
 * @returns Safe image URL
 */
export function getSafeImageUrl(imagePath?: string, fallbackUrl?: string): string {
  if (!imagePath) {
    return fallbackUrl || 'https://placehold.co/400x300?text=No+Image';
  }
  
  return getImageUrl(imagePath);
}

/**
 * Check if an image path is a server path that needs conversion
 * @param imagePath - Image path to check
 * @returns True if it's a server path
 */
export function isServerImagePath(imagePath: string): boolean {
  return imagePath.startsWith('server/portfolio/');
}

/**
 * Extract subfolder from server image path
 * @param imagePath - Server image path
 * @returns Subfolder name or null
 */
export function getSubfolderFromPath(imagePath: string): string | null {
  const pathParts = imagePath.split('/');
  
  if (pathParts.length >= 3 && pathParts[0] === 'server' && pathParts[1] === 'portfolio') {
    return pathParts[2];
  }
  
  return null;
}

/**
 * Extract filename from server image path
 * @param imagePath - Server image path
 * @returns Filename or null
 */
export function getFilenameFromPath(imagePath: string): string | null {
  const pathParts = imagePath.split('/');
  
  if (pathParts.length >= 4 && pathParts[0] === 'server' && pathParts[1] === 'portfolio') {
    return pathParts.slice(3).join('/');
  }
  
  return null;
}

/**
 * Create a preview URL for displaying images in components
 * @param imagePath - Image path from server or preview data URL
 * @returns URL suitable for img src attribute
 */
export function createImagePreviewUrl(imagePath: string): string {
  // For data URLs (file previews), return as is
  if (imagePath.startsWith('data:')) {
    return imagePath;
  }
  
  // For server paths, convert to API URL
  return getImageUrl(imagePath);
}

/**
 * Validate if an image URL is accessible
 * @param imageUrl - Image URL to validate
 * @returns Promise that resolves to true if image is accessible
 */
export async function validateImageUrl(imageUrl: string): Promise<boolean> {
  try {
    const response = await fetch(imageUrl, { method: 'HEAD' });
    return response.ok;
  } catch {
    return false;
  }
} 