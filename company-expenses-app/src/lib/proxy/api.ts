/**
 * API Service - High-level API methods
 * Use this file to define your API endpoints
 */

import { apiProxy } from "./proxy";
import type {
  LoginRequest,
  RegisterRequest,
  AuthResponse,
  Expense,
  CreateExpenseRequest,
  UpdateExpenseRequest,
  PaginatedResponse,
  Workplace,
  CreateWorkplaceRequest,
  UpdateWorkplaceRequest,
  WorkplaceMember,
  CreateWorkplaceMemberRequest,
  UpdateWorkplaceMemberRequest,
  Invitation,
  CreateInvitationRequest,
  AcceptInvitationRequest,
  ExpenseCategory,
  CreateExpenseCategoryRequest,
  UpdateExpenseCategoryRequest,
} from "./types";

/**
 * Authentication API
 */
export const authApi = {
  /**
   * Login user
   */
  async login(credentials: LoginRequest): Promise<AuthResponse> {
    return apiProxy.post<AuthResponse>("/api/auth/login", credentials);
  },

  /**
   * Register new user
   */
  async register(data: RegisterRequest): Promise<AuthResponse> {
    return apiProxy.post<AuthResponse>("/api/auth/register", data);
  },

  /**
   * Logout user
   */
  async logout(): Promise<void> {
    return apiProxy.post("/api/auth/logout");
  },

  /**
   * Get current user
   */
  async getCurrentUser(): Promise<AuthResponse> {
    return apiProxy.get<AuthResponse>("/api/auth/me");
  },

  /**
   * Refresh authentication token
   */
  async refreshToken(): Promise<AuthResponse> {
    return apiProxy.post<AuthResponse>("/api/auth/refresh");
  },
};

/**
 * Expenses API
 */
export const expensesApi = {
  /**
   * Get all expenses (paginated)
   */
  async getExpenses(page = 1, pageSize = 10): Promise<PaginatedResponse<Expense>> {
    return apiProxy.get<PaginatedResponse<Expense>>("/api/expenses", {
      params: { page, pageSize },
    });
  },

  /**
   * Get single expense by ID
   */
  async getExpense(id: string): Promise<Expense> {
    return apiProxy.get<Expense>(`/api/expenses/${id}`);
  },

  /**
   * Create new expense
   */
  async createExpense(data: CreateExpenseRequest): Promise<Expense> {
    return apiProxy.post<Expense>("/api/expenses", data);
  },

  /**
   * Update existing expense
   */
  async updateExpense(id: string, data: UpdateExpenseRequest): Promise<Expense> {
    return apiProxy.put<Expense>(`/api/expenses/${id}`, data);
  },

  /**
   * Delete expense
   */
  async deleteExpense(id: string): Promise<void> {
    return apiProxy.delete(`/api/expenses/${id}`);
  },

  /**
   * Get expenses by category
   */
  async getExpensesByCategory(category: string): Promise<Expense[]> {
    return apiProxy.get<Expense[]>(`/api/expenses/category/${category}`);
  },

  /**
   * Get expense statistics
   */
  async getStatistics(): Promise<Record<string, unknown>> {
    return apiProxy.get("/api/expenses/statistics");
  },
};

/**
 * Workplaces API
 */
export const workplacesApi = {
  /**
   * Get all workplaces
   */
  async getWorkplaces(): Promise<Workplace[]> {
    return apiProxy.get<Workplace[]>("/api/workplaces");
  },

  /**
   * Get single workplace by ID
   */
  async getWorkplace(id: string): Promise<Workplace> {
    return apiProxy.get<Workplace>(`/api/workplaces/${id}`);
  },

  /**
   * Create new workplace
   */
  async createWorkplace(data: CreateWorkplaceRequest): Promise<Workplace> {
    return apiProxy.post<Workplace>("/api/workplaces", data);
  },

  /**
   * Update existing workplace
   */
  async updateWorkplace(id: string, data: UpdateWorkplaceRequest): Promise<void> {
    return apiProxy.put(`/api/workplaces/${id}`, data);
  },

  /**
   * Delete workplace (soft delete - sets IsActive to false)
   */
  async deleteWorkplace(id: string): Promise<void> {
    return apiProxy.delete(`/api/workplaces/${id}`);
  },
};

/**
 * Workplace Members API
 */
export const workplaceMembersApi = {
  /**
   * Get all members of all workplaces
   */
  async getAllMembers(): Promise<WorkplaceMember[]> {
    return apiProxy.get<WorkplaceMember[]>("/api/workplacemembers");
  },

  /**
   * Get members of a specific workplace
   */
  async getWorkplaceMembers(workplaceId: string): Promise<WorkplaceMember[]> {
    return apiProxy.get<WorkplaceMember[]>(`/api/workplacemembers/workplace/${workplaceId}`);
  },

  /**
   * Get workplaces where user is a member
   */
  async getUserWorkplaces(userId: string): Promise<WorkplaceMember[]> {
    return apiProxy.get<WorkplaceMember[]>(`/api/workplacemembers/user/${userId}`);
  },

  /**
   * Get single member by ID
   */
  async getMember(id: string): Promise<WorkplaceMember> {
    return apiProxy.get<WorkplaceMember>(`/api/workplacemembers/${id}`);
  },

  /**
   * Add member to workplace
   */
  async addMember(data: CreateWorkplaceMemberRequest): Promise<WorkplaceMember> {
    return apiProxy.post<WorkplaceMember>("/api/workplacemembers", data);
  },

  /**
   * Update member
   */
  async updateMember(id: string, data: UpdateWorkplaceMemberRequest): Promise<void> {
    return apiProxy.put(`/api/workplacemembers/${id}`, data);
  },

  /**
   * Remove member from workplace
   */
  async removeMember(id: string): Promise<void> {
    return apiProxy.delete(`/api/workplacemembers/${id}`);
  },

  /**
   * Toggle manager status
   */
  async toggleManager(id: string, isManager: boolean): Promise<{ message: string }> {
    return apiProxy.patch<{ message: string }>(`/api/workplacemembers/${id}/manager`, isManager);
  },
};

/**
 * Invitations API
 */
export const invitationsApi = {
  /**
   * Get all invitations
   */
  async getInvitations(): Promise<Invitation[]> {
    return apiProxy.get<Invitation[]>("/api/invitations");
  },

  /**
   * Get single invitation by ID
   */
  async getInvitation(id: string): Promise<Invitation> {
    return apiProxy.get<Invitation>(`/api/invitations/${id}`);
  },

  /**
   * Verify invitation by token
   */
  async verifyInvitation(token: string): Promise<Invitation> {
    return apiProxy.get<Invitation>(`/api/invitations/verify/${token}`);
  },

  /**
   * Create new invitation
   */
  async createInvitation(data: CreateInvitationRequest): Promise<Invitation> {
    return apiProxy.post<Invitation>("/api/invitations", data);
  },

  /**
   * Accept invitation
   */
  async acceptInvitation(id: string, data: AcceptInvitationRequest): Promise<{ message: string }> {
    return apiProxy.post<{ message: string }>(`/api/invitations/${id}/accept`, data);
  },

  /**
   * Cancel invitation
   */
  async cancelInvitation(id: string): Promise<void> {
    return apiProxy.delete(`/api/invitations/${id}`);
  },

  /**
   * Resend invitation
   */
  async resendInvitation(id: string): Promise<Invitation> {
    return apiProxy.post<Invitation>(`/api/invitations/${id}/resend`);
  },
};

/**
 * Expense Categories API
 */
export const categoriesApi = {
  /**
   * Get all active categories
   */
  async getCategories(): Promise<ExpenseCategory[]> {
    return apiProxy.get<ExpenseCategory[]>("/api/expensecategories");
  },

  /**
   * Get single category by ID
   */
  async getCategory(id: string): Promise<ExpenseCategory> {
    return apiProxy.get<ExpenseCategory>(`/api/expensecategories/${id}`);
  },

  /**
   * Create new category
   */
  async createCategory(data: CreateExpenseCategoryRequest): Promise<ExpenseCategory> {
    return apiProxy.post<ExpenseCategory>("/api/expensecategories", data);
  },

  /**
   * Update existing category
   */
  async updateCategory(id: string, data: UpdateExpenseCategoryRequest): Promise<void> {
    return apiProxy.put(`/api/expensecategories/${id}`, data);
  },

  /**
   * Delete category (soft delete)
   */
  async deleteCategory(id: string): Promise<void> {
    return apiProxy.delete(`/api/expensecategories/${id}`);
  },
};

/**
 * Export all API methods
 */
export const api = {
  auth: authApi,
  expenses: expensesApi,
  workplaces: workplacesApi,
  workplaceMembers: workplaceMembersApi,
  invitations: invitationsApi,
  categories: categoriesApi,
};

export default api;
