import { AuthenticatedApiClient } from '../api/authenticated-client';

// Create authenticated API client for portfolio service (where image upload is handled)
const portfolioClient = AuthenticatedApiClient.createPortfolioClient();

export interface ImageUploadResponse {
  imagePath: string;
  fileName: string;
  subfolder: string;
  fileSize: number;
  uploadedAt: string;
  message: string;
}

export interface ImageUploadErrorResponse {
  error: string;
  message: string;
  supportedFormats: string[];
  supportedSubfolders: string[];
}

export interface SupportedSubfoldersResponse {
  subfolders: string[];
}

// Helper function to handle API responses - no longer needed as AuthenticatedApiClient handles this

/**
 * Upload an image file to the specified subfolder
 * @param imageFile - The image file to upload
 * @param subfolder - The subfolder where the image should be saved
 * @returns Promise with the upload response containing the image path
 */
export async function uploadImage(
  imageFile: File,
  subfolder: string
): Promise<ImageUploadResponse> {
  const formData = new FormData();
  formData.append('imageFile', imageFile);
  formData.append('subfolder', subfolder);

  try {
    // For FormData uploads, we need to use a custom approach since AuthenticatedApiClient expects JSON
    const session = await import('next-auth/react').then(m => m.getSession());
    const accessToken = session?.accessToken;
    
    if (!accessToken) {
      throw new Error('No access token available. Please sign in.');
    }

    const response = await fetch(`${portfolioClient['baseUrl']}/api/ImageUpload/upload`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${accessToken}`,
      },
      body: formData,
    });

    if (!response.ok) {
      let errorData: ImageUploadErrorResponse;
      try {
        errorData = await response.json();
      } catch {
        errorData = {
          error: 'UnknownError',
          message: `HTTP error! status: ${response.status}`,
          supportedFormats: [],
          supportedSubfolders: []
        };
      }
      throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
    }

    return await response.json();
  } catch (error) {
    console.error('📤 API: Error uploading image:', error);
    throw error;
  }
}

/**
 * Delete an image by its path
 * @param imagePath - The path of the image to delete
 * @returns Promise with success message
 */
export async function deleteImage(imagePath: string): Promise<{ message: string }> {
  try {
    return await portfolioClient.delete<{ message: string }>(`/api/ImageUpload/delete?imagePath=${encodeURIComponent(imagePath)}`, true);
  } catch (error) {
    console.error('📤 API: Error deleting image:', error);
    throw error;
  }
}

/**
 * Get the list of supported subfolders for image uploads
 * @returns Promise with the list of supported subfolders
 */
export async function getSupportedSubfolders(): Promise<SupportedSubfoldersResponse> {
  return await portfolioClient.get<SupportedSubfoldersResponse>('/api/ImageUpload/supported-subfolders', false);
}

/**
 * Check the health status of the image upload service
 * @returns Promise with health status
 */
export async function checkImageUploadHealth(): Promise<{
  status: string;
  service: string;
  timestamp: string;
}> {
  return await portfolioClient.get<{ status: string; service: string; timestamp: string }>('/api/ImageUpload/health', false);
}

/**
 * Validate image file before upload
 * @param file - The file to validate
 * @param maxSizeInMB - Maximum file size in MB (default 5MB)
 * @returns Object with validation result and error message if invalid
 */
export function validateImageFile(
  file: File,
  maxSizeInMB: number = 5
): { isValid: boolean; error?: string } {
  const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
  const maxSizeInBytes = maxSizeInMB * 1024 * 1024;

  if (!file) {
    return { isValid: false, error: 'No file selected' };
  }

  if (!allowedTypes.includes(file.type)) {
    return {
      isValid: false,
      error: `File type ${file.type} is not allowed. Supported types: ${allowedTypes.join(', ')}`
    };
  }

  if (file.size > maxSizeInBytes) {
    return {
      isValid: false,
      error: `File size (${(file.size / (1024 * 1024)).toFixed(2)}MB) exceeds maximum allowed size of ${maxSizeInMB}MB`
    };
  }

  return { isValid: true };
}

/**
 * Get file extension from filename
 * @param filename - The filename to extract extension from
 * @returns The file extension (e.g., '.jpg')
 */
export function getFileExtension(filename: string): string {
  return filename.toLowerCase().substring(filename.lastIndexOf('.'));
}

/**
 * Generate a preview URL for a selected file
 * @param file - The file to generate preview for
 * @returns Promise with the preview URL
 */
export function generateImagePreview(file: File): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = (e) => {
      resolve(e.target?.result as string);
    };
    reader.onerror = reject;
    reader.readAsDataURL(file);
  });
}

 