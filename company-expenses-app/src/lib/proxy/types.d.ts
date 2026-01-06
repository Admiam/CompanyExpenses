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

// Expense types
export interface Expense {
  id: string;
  description: string;
  amount: number;
  currency: string;
  expenseDate: string;
  status: "Pending" | "Approved" | "Rejected";
  employeeUserId: string;
  workplaceId: string;
  categoryId: string;
  workplace?: { id: string; name: string };
  category?: { id: string; name: string };
  submittedAt: string;
  createdAt: string;
}

export interface ExpenseAttachmentUpload {
  originalFileName: string;
  dataType: string;
  base64Data: string;
  originalFileSize: number;
}

export interface CreateExpenseRequest {
  description: string;
  amount: number;
  currency?: string;
  expenseDate: string;
  categoryId: string;
  workplaceId: string;
  attachments?: ExpenseAttachmentUpload[];
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
  categoryId?: string;
  periodFrom: string;
  periodTo: string;
  limitAmount: number;
  currency: string;
  createdAt: string;
  createdBy: string;
  category?: ExpenseCategory;
}

export interface CreateWorkplaceLimitRequest {
  workplaceId: string;
  categoryId?: string;
  periodFrom: string;
  periodTo: string;
  limitAmount: number;
  currency?: string;
}

export interface UpdateWorkplaceLimitRequest {
  id: string;
  workplaceId: string;
  categoryId?: string;
  periodFrom: string;
  periodTo: string;
  limitAmount: number;
  currency?: string;
}

export interface CreateWorkplaceRequest {
  name: string;
  code?: string;
  isActive?: boolean;
}

export interface UpdateWorkplaceRequest {
  id: string;
  name: string;
  code?: string;
  isActive: boolean;
}

export interface WorkplaceDependencies {
  workplaceId: string;
  membersCount: number;
  limitsCount: number;
  invitationsCount: number;
  expensesCount: number;
  canDelete: boolean;
}

export interface CategoryDependencies {
  categoryId: string;
  expensesCount: number;
  limitsCount: number;
  canDelete: boolean;
}

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

// User with statistics
export interface UserWithStats {
  id: string;
  name: string;
  email: string;
  role: "admin" | "manager" | "employee";
  workplace: string;
  workplaceId?: string;
  isActive: boolean;
  status: string;
  expenseCount: number;
  totalExpenses: number;
}

// User detail types
export interface UserMembership {
  id: string;
  workplaceId: string;
  workplaceName: string;
  positionName?: string;
  isManager: boolean;
  joinedAt: string;
}

export interface UserExpense {
  id: string;
  amount: number;
  currency: string;
  expenseDate: string;
  description?: string;
  status: string;
  categoryName: string;
  workplaceName: string;
  submittedAt: string;
}

export interface UserApproval {
  id: string;
  expenseId: string;
  action: string;
  note?: string;
  createdAt: string;
  expenseAmount: number;
  expenseCurrency: string;
  expenseDescription?: string;
  categoryName: string;
}

export interface UserInvitation {
  id: string;
  email: string;
  workplaceName?: string;
  status: string;
  createdAt: string;
  expiresAt: string;
  acceptedAt?: string;
}

export interface ExpenseStatusStat {
  status: string;
  count: number;
  total: number;
}

export interface UserDetail {
  id: string;
  name: string;
  email: string;
  role: string;
  isActive: boolean;
  createdAt: string;
  memberships: UserMembership[];
  expenses: UserExpense[];
  expenseStats: {
    total: number;
    count: number;
    byStatus: ExpenseStatusStat[];
  };
  approvals: UserApproval[];
  approvalStats: {
    count: number;
    approved: number;
    rejected: number;
  };
  invitations: UserInvitation[];
  invitationStats: {
    count: number;
    pending: number;
    accepted: number;
    expired: number;
  };
}

// Invitation types
import type { InvitationStatusType } from "@/constants/invitation";

export interface Invitation {
  id: string;
  email: string;
  invitedRoleId?: string;
  workplaceId?: string;
  token: string;
  expiresAt: string;
  acceptedAt?: string;
  invitedByUserId: string;
  status: InvitationStatusType;
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

// Role types
export interface Role {
  id: string;
  name: string;
}

// Dashboard statistics types
export interface DashboardStats {
  totalExpenses: number;
  monthlyExpenses: number;
  monthlyChange: number;
  workplacesCount: number;
  usersCount: number;
  pendingExpensesCount: number;
  expensesByCategory: CategoryExpense[];
  expensesByWorkplace: WorkplaceExpense[];
  recentExpenses: RecentExpense[];
}

export interface CategoryExpense {
  categoryId: string;
  categoryName: string;
  categoryColor?: string;
  total: number;
  count: number;
}

export interface WorkplaceExpense {
  workplaceId: string;
  workplaceName: string;
  total: number;
  count: number;
  categories: {
    categoryId: string;
    categoryName: string;
    categoryColor?: string;
    total: number;
  }[];
}

export interface RecentExpense {
  id: string;
  description: string;
  amount: number;
  currency: string;
  expenseDate: string;
  status: string;
  employeeUserId: string;
  categoryName?: string;
  workplaceName?: string;
  submittedAt: string;
}
