export const FILE_CONFIG = {
  maxFileSizeBytes: Number(import.meta.env.VITE_MAX_FILE_SIZE_BYTES) || 10_485_760,
  get maxFileSizeMB(): number {
    return this.maxFileSizeBytes / (1024 * 1024);
  },
  allowedImageTypes: (import.meta.env.VITE_ALLOWED_IMAGE_TYPES || "image/jpeg,image/jpg,image/png,image/gif").split(","),
  get allowedImageAccept(): string {
    return this.allowedImageTypes.join(",");
  },
};

export const UI_CONFIG = {
  registrationRedirectDelay: Number(import.meta.env.VITE_REGISTRATION_REDIRECT_DELAY) || 2000,
  toastDuration: Number(import.meta.env.VITE_TOAST_DURATION) || 5000,
};

export const API_CONFIG = {
  baseUrl: import.meta.env.VITE_API_BASE_URL || "https://localhost:7200",
  timeout: Number(import.meta.env.VITE_API_TIMEOUT) || 30000,
};
