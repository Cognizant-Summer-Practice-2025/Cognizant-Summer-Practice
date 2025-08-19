const API_BASE_URL = process.env.NEXT_PUBLIC_PORTFOLIO_API_URL!;

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

async function handleApiResponse<T>(response: Response): Promise<T> {
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
  return response.json();
}

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

  const response = await fetch(`${API_BASE_URL}/api/ImageUpload/upload`, {
    method: 'POST',
    body: formData,
  });

  return handleApiResponse<ImageUploadResponse>(response);
}

/**
 * Delete an image by its path
 * @param imagePath - The path of the image to delete
 * @returns Promise with success message
 */
export async function deleteImage(imagePath: string): Promise<{ message: string }> {
  const response = await fetch(`${API_BASE_URL}/api/ImageUpload/delete?imagePath=${encodeURIComponent(imagePath)}`, {
    method: 'DELETE',
  });

  return handleApiResponse<{ message: string }>(response);
}

/**
 * Get the list of supported subfolders for image uploads
 * @returns Promise with the list of supported subfolders
 */
export async function getSupportedSubfolders(): Promise<SupportedSubfoldersResponse> {
  const response = await fetch(`${API_BASE_URL}/api/ImageUpload/supported-subfolders`);
  return handleApiResponse<SupportedSubfoldersResponse>(response);
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
  const response = await fetch(`${API_BASE_URL}/api/ImageUpload/health`);
  return handleApiResponse<{ status: string; service: string; timestamp: string }>(response);
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

 