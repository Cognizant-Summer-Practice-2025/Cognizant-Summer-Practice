export interface ImageUploadRequest {
  imageFile: File;
  subfolder: string;
}

export interface ImageUploadProgress {
  uploadedBytes: number;
  totalBytes: number;
  percentage: number;
  status: 'uploading' | 'completed' | 'error' | 'cancelled';
}

export interface ImageMetadata {
  filename: string;
  size: number;
  type: string;
  lastModified: number;
  dimensions?: {
    width: number;
    height: number;
  };
}

export interface UploadedImage {
  id: string;
  path: string;
  filename: string;
  size: number;
  uploadedAt: Date;
  subfolder: string;
  url: string; 
}

// Configuration interfaces
export interface ImageUploadConfig {
  maxSizeInMB: number;
  allowedTypes: string[];
  allowedSubfolders: string[];
  apiBaseUrl: string;
}

// Hook state interface for React components
export interface UseImageUploadState {
  isUploading: boolean;
  progress: ImageUploadProgress | null;
  error: string | null;
  uploadedImage: UploadedImage | null;
  uploadImage: (file: File, subfolder: string) => Promise<void>;
  deleteImage: (imagePath: string) => Promise<void>;
  reset: () => void;
}

// Validation result interface
export interface ImageValidationResult {
  isValid: boolean;
  error?: string;
  warnings?: string[];
} 