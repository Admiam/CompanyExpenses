// Application configuration - values loaded from environment variables

/**
 * File upload configuration
 */
export const FILE_CONFIG = {
  // Maximum file size in bytes (default 10 MB)
  maxFileSizeBytes: Number(import.meta.env.VITE_MAX_FILE_SIZE_BYTES) || 10_485_760,

  // Maximum file size in MB (for display purposes)
  get maxFileSizeMB(): number {
    return this.maxFileSizeBytes / (1024 * 1024);
  },

  // Allowed file types for attachments
  allowedImageTypes: (import.meta.env.VITE_ALLOWED_IMAGE_TYPES || "image/jpeg,image/jpg,image/png,image/gif").split(","),

  // Allowed file extensions (for accept attribute)
  get allowedImageAccept(): string {
    return this.allowedImageTypes.join(",");
  },
};

/**
 * UI/UX configuration
 */
export const UI_CONFIG = {
  // Redirect delay after successful registration (in ms)
  registrationRedirectDelay: Number(import.meta.env.VITE_REGISTRATION_REDIRECT_DELAY) || 2000,

  // Toast notification duration (in ms)
  toastDuration: Number(import.meta.env.VITE_TOAST_DURATION) || 5000,
};

/**
 * API configuration
 */
export const API_CONFIG = {
  // API base URL
  baseUrl: import.meta.env.VITE_API_BASE_URL || "https://localhost:7200",

  // Request timeout (in ms)
  timeout: Number(import.meta.env.VITE_API_TIMEOUT) || 30000,
};
