"use client";

import { useState, useRef } from "react";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { X, Image as ImageIcon } from "lucide-react";
import { validateImageFile, generateImagePreview, createImagePreviewUrl } from "@/lib/image";

interface ImageUploadProps {
  label?: string;
  value?: string;
  onFileSelect: (file: File | null) => void;
  error?: string;
  disabled?: boolean;
  preview?: boolean;
  className?: string;
  accept?: string;
  maxSizeInMB?: number;
}

export function ImageUpload({
  label = "Upload Image",
  value,
  onFileSelect,
  error,
  disabled = false,
  preview = true,
  className = "",
  accept = "image/*",
  maxSizeInMB = 5
}: ImageUploadProps) {
  const [uploadError, setUploadError] = useState<string | null>(null);
  const [previewUrl, setPreviewUrl] = useState<string | null>(value || null);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleFileSelect = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) {
      setSelectedFile(null);
      setPreviewUrl(value || null);
      onFileSelect(null);
      return;
    }

    // Validate file
    const validation = validateImageFile(file, maxSizeInMB);
    if (!validation.isValid) {
      setUploadError(validation.error || "Invalid file");
      setSelectedFile(null);
      setPreviewUrl(value || null);
      onFileSelect(null);
      return;
    }

    setSelectedFile(file);
    setUploadError(null);
    onFileSelect(file);

    // Generate preview only 
    if (preview) {
      try {
        const previewDataUrl = await generateImagePreview(file);
        setPreviewUrl(previewDataUrl);
      } catch (err) {
        console.error("Error generating preview:", err);
      }
    }
  };

  const handleRemove = () => {
    setPreviewUrl(value || null);
    setSelectedFile(null);
    onFileSelect(null);
    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
  };

  const handleButtonClick = () => {
    fileInputRef.current?.click();
  };

  return (
    <div className={`space-y-3 ${className}`}>
      {label && (
        <Label className="text-sm font-medium text-slate-700">
          {label}
        </Label>
      )}

      <div className="flex flex-col space-y-3">
        {/* File Input - Hidden */}
        <input
          ref={fileInputRef}
          type="file"
          accept={accept}
          onChange={handleFileSelect}
          disabled={disabled}
          className="hidden"
        />

        {/* Preview Area */}
        {preview && previewUrl && (
          <div className="relative w-full max-w-xs">
            <div className="relative aspect-video w-full rounded-lg border border-slate-200 overflow-hidden bg-slate-50">
              <img
                src={createImagePreviewUrl(previewUrl)}
                alt="Preview"
                className="w-full h-full object-cover"
                onError={(e) => {
                  // Fallback to placeholder if image fails to load
                  e.currentTarget.src = 'https://placehold.co/400x300?text=Image+Error';
                }}
              />
              <button
                type="button"
                onClick={handleRemove}
                disabled={disabled}
                className="absolute top-2 right-2 p-1 bg-red-500 text-white rounded-full hover:bg-red-600 transition-colors"
              >
                <X className="w-4 h-4" />
              </button>
            </div>
          </div>
        )}

        {/* Upload Controls */}
        <div className="flex flex-col sm:flex-row gap-2">
          <Button
            type="button"
            variant="outline"
            onClick={handleButtonClick}
            disabled={disabled}
            className="flex items-center gap-2"
          >
            <ImageIcon className="w-4 h-4" />
            {selectedFile ? "Change Image" : "Select Image"}
          </Button>
        </div>

        {/* Selected File Info */}
        {selectedFile && (
          <div className="text-xs text-slate-600">
            Selected: {selectedFile.name} ({(selectedFile.size / (1024 * 1024)).toFixed(2)}MB)
          </div>
        )}

        {/* Error Display */}
        {(uploadError || error) && (
          <div className="text-sm text-red-600">
            {uploadError || error}
          </div>
        )}

        {/* Instructions */}
        <div className="text-xs text-slate-500">
          Supported formats: JPG, PNG, GIF, WebP (max {maxSizeInMB}MB)
        </div>
      </div>
    </div>
  );
} 