/**
 * Common API types and interfaces
 */

// Generic API response wrapper
export interface ApiResponse<T> {
  data: T;
  message?: string;
  success: boolean;
}

// Paginated response
export interface PaginatedResponse<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// Error response
export interface ApiError {
  message: string;
  code?: string;
  errors?: Record<string, string[]>;
}

// Auth types
export interface LoginRequest {
  email: string;
  password: string;
  rememberMe?: boolean;
}

export interface RegisterRequest {
  email: string;
  password: string;
  confirmPassword: string;
}

export interface AuthResponse {
  token?: string;
  user: User;
}

export interface User {
  id: string;
  email: string;
  name?: string;
  role?: string;
}

// Expense types - TODO: Expand based on your backend models
export interface Expense {
  id: string;
  amount: number;
  description: string;
  category: string;
  date: string;
  userId: string;
}

export interface CreateExpenseRequest {
  amount: number;
  description: string;
  category: string;
  date: string;
}

export interface UpdateExpenseRequest extends Partial<CreateExpenseRequest> {
  id: string;
}
