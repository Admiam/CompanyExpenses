import { API_CONFIG, FILE_CONFIG } from "@/lib/app-config";

const API_BASE_URL = API_CONFIG.baseUrl;

export interface ExpenseAttachment {
  id: string;
  expenseId: string;
  originalFileName: string;
  dataType: string;
  fileSize: number;
  uploadedByUserId: string;
  uploadedAt: string;
}

/**
 * Get all attachments for an expense
 */
export async function getExpenseAttachments(expenseId: string): Promise<ExpenseAttachment[]> {
  const response = await fetch(`${API_BASE_URL}/api/expenses/${expenseId}/expenseattachments`, {
    credentials: "include",
  });

  if (!response.ok) {
    throw new Error("Failed to fetch attachments");
  }

  return response.json();
}

/**
 * Upload an attachment for an expense
 */
export async function uploadExpenseAttachment(expenseId: string, file: File, userId?: string): Promise<ExpenseAttachment> {
  const formData = new FormData();
  formData.append("file", file);
  if (userId) {
    formData.append("userId", userId);
  }

  const response = await fetch(`${API_BASE_URL}/api/expenses/${expenseId}/expenseattachments`, {
    method: "POST",
    credentials: "include",
    body: formData,
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message || "Failed to upload attachment");
  }

  return response.json();
}

/**
 * Download an attachment
 */
export function getAttachmentDownloadUrl(expenseId: string, attachmentId: string): string {
  return `${API_BASE_URL}/api/expenses/${expenseId}/expenseattachments/${attachmentId}`;
}

/**
 * Delete an attachment
 */
export async function deleteExpenseAttachment(expenseId: string, attachmentId: string): Promise<void> {
  const response = await fetch(`${API_BASE_URL}/api/expenses/${expenseId}/expenseattachments/${attachmentId}`, {
    method: "DELETE",
    credentials: "include",
  });

  if (!response.ok) {
    throw new Error("Failed to delete attachment");
  }
}

/**
 * Format file size for display
 */
export function formatFileSize(bytes: number): string {
  if (bytes < 1024) return bytes + " B";
  if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + " KB";
  return (bytes / (1024 * 1024)).toFixed(1) + " MB";
}

/**
 * Validate if file is an allowed image type
 */
export function isValidImageFile(file: File): boolean {
  return FILE_CONFIG.allowedImageTypes.includes(file.type);
}

/**
 * Validate file size using configured maximum
 */
export function isValidFileSize(file: File, maxSizeBytes?: number): boolean {
  const limit = maxSizeBytes ?? FILE_CONFIG.maxFileSizeBytes;
  return file.size <= limit;
}
