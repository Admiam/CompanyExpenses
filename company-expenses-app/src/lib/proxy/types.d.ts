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

// Workplace types
export interface Workplace {
  id: string;
  name: string;
  description?: string;
  address?: string;
  isActive: boolean;
  createdAt: string;
  createdBy: string;
  members?: WorkplaceMember[];
  limits?: WorkplaceLimit[];
}

export interface WorkplaceLimit {
  id: string;
  workplaceId: string;
  categoryId: string;
  monthlyLimit: number;
  isActive: boolean;
}

export interface CreateWorkplaceRequest {
  name: string;
  description?: string;
  address?: string;
}

export type UpdateWorkplaceRequest = Partial<CreateWorkplaceRequest>;

// WorkplaceMember types
export interface WorkplaceMember {
  id: string;
  workplaceId: string;
  userId: string;
  positionName?: string;
  isManager: boolean;
  createdAt: string;
  createdBy: string;
  workplace?: Workplace;
}

export interface CreateWorkplaceMemberRequest {
  workplaceId: string;
  userId: string;
  positionName?: string;
  isManager?: boolean;
}

export interface UpdateWorkplaceMemberRequest {
  positionName?: string;
  isManager: boolean;
}

// Invitation types
export enum InvitationStatus {
  Pending = 0,
  Accepted = 1,
  Declined = 2,
  Expired = 3,
  Cancelled = 4,
}

export interface Invitation {
  id: string;
  email: string;
  invitedRoleId?: string;
  workplaceId?: string;
  token: string;
  expiresAt: string;
  acceptedAt?: string;
  invitedByUserId: string;
  status: InvitationStatus;
  createdAt: string;
  createdBy: string;
  workplace?: Workplace;
}

export interface CreateInvitationRequest {
  email: string;
  invitedRoleId?: string;
  workplaceId?: string;
}

export interface AcceptInvitationRequest {
  userId: string;
}

// ExpenseCategory types
export interface ExpenseCategory {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
  createdAt: string;
  createdBy: string;
}

export interface CreateExpenseCategoryRequest {
  name: string;
  description?: string;
}

export type UpdateExpenseCategoryRequest = Partial<CreateExpenseCategoryRequest>;
