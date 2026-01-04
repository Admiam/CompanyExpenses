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
  WorkplaceLimit,
  CreateWorkplaceLimitRequest,
  UpdateWorkplaceLimitRequest,
  Role,
} from "./types";

export const authApi = {
  async login(credentials: LoginRequest): Promise<AuthResponse> {
    return apiProxy.post<AuthResponse>("/api/auth/login", credentials);
  },

  async register(data: RegisterRequest): Promise<AuthResponse> {
    return apiProxy.post<AuthResponse>("/api/auth/register", data);
  },

  async logout(): Promise<void> {
    return apiProxy.post("/api/auth/logout");
  },

  async getCurrentUser(): Promise<AuthResponse> {
    return apiProxy.get<AuthResponse>("/api/auth/me");
  },

  async refreshToken(): Promise<AuthResponse> {
    return apiProxy.post<AuthResponse>("/api/auth/refresh");
  },
};

export const expensesApi = {
  async getExpenses(page = 1, pageSize = 10): Promise<PaginatedResponse<Expense>> {
    return apiProxy.get<PaginatedResponse<Expense>>("/api/expenses", {
      params: { page, pageSize },
    });
  },

  async getExpense(id: string): Promise<Expense> {
    return apiProxy.get<Expense>(`/api/expenses/${id}`);
  },

  async createExpense(data: CreateExpenseRequest): Promise<Expense> {
    return apiProxy.post<Expense>("/api/expenses", data);
  },

  async updateExpense(id: string, data: UpdateExpenseRequest): Promise<Expense> {
    return apiProxy.put<Expense>(`/api/expenses/${id}`, data);
  },

  async deleteExpense(id: string): Promise<void> {
    return apiProxy.delete(`/api/expenses/${id}`);
  },

  async getExpensesByCategory(category: string): Promise<Expense[]> {
    return apiProxy.get<Expense[]>(`/api/expenses/category/${category}`);
  },

  async getStatistics(): Promise<Record<string, unknown>> {
    return apiProxy.get("/api/expenses/statistics");
  },
};

export const workplacesApi = {
  async getWorkplaces(): Promise<Workplace[]> {
    return apiProxy.get<Workplace[]>("/api/workplaces");
  },

  async getWorkplace(id: string): Promise<Workplace> {
    return apiProxy.get<Workplace>(`/api/workplaces/${id}`);
  },

  async createWorkplace(data: CreateWorkplaceRequest): Promise<Workplace> {
    return apiProxy.post<Workplace>("/api/workplaces", data);
  },

  async updateWorkplace(id: string, data: UpdateWorkplaceRequest): Promise<void> {
    return apiProxy.put(`/api/workplaces/${id}`, data);
  },

  async deleteWorkplace(id: string): Promise<void> {
    return apiProxy.delete(`/api/workplaces/${id}`);
  },

  async activateWorkplace(id: string): Promise<void> {
    return apiProxy.patch(`/api/workplaces/activate/${id}`);
  },

  async deactivateWorkplace(id: string): Promise<void> {
    return apiProxy.patch(`/api/workplaces/deactivate/${id}`);
  },
};

export const workplaceLimitsApi = {
  async getWorkplaceLimits(workplaceId: string): Promise<WorkplaceLimit[]> {
    return apiProxy.get<WorkplaceLimit[]>(`/api/workplacelimits/workplace/${workplaceId}`);
  },

  async getLimit(id: string): Promise<WorkplaceLimit> {
    return apiProxy.get<WorkplaceLimit>(`/api/workplacelimits/${id}`);
  },

  async createLimit(data: CreateWorkplaceLimitRequest): Promise<WorkplaceLimit> {
    return apiProxy.post<WorkplaceLimit>("/api/workplacelimits", data);
  },

  async updateLimit(id: string, data: UpdateWorkplaceLimitRequest): Promise<void> {
    return apiProxy.put(`/api/workplacelimits/${id}`, data);
  },

  async deleteLimit(id: string): Promise<void> {
    return apiProxy.delete(`/api/workplacelimits/${id}`);
  },
};

export const workplaceMembersApi = {
  async getAllMembers(): Promise<WorkplaceMember[]> {
    return apiProxy.get<WorkplaceMember[]>("/api/workplacemembers");
  },

  async getWorkplaceMembers(workplaceId: string): Promise<WorkplaceMember[]> {
    return apiProxy.get<WorkplaceMember[]>(`/api/workplacemembers/workplace/${workplaceId}`);
  },

  async getUserWorkplaces(userId: string): Promise<WorkplaceMember[]> {
    return apiProxy.get<WorkplaceMember[]>(`/api/workplacemembers/user/${userId}`);
  },

  async getMember(id: string): Promise<WorkplaceMember> {
    return apiProxy.get<WorkplaceMember>(`/api/workplacemembers/${id}`);
  },

  async addMember(data: CreateWorkplaceMemberRequest): Promise<WorkplaceMember> {
    return apiProxy.post<WorkplaceMember>("/api/workplacemembers", data);
  },

  async updateMember(id: string, data: UpdateWorkplaceMemberRequest): Promise<void> {
    return apiProxy.put(`/api/workplacemembers/${id}`, data);
  },

  async removeMember(id: string): Promise<void> {
    return apiProxy.delete(`/api/workplacemembers/${id}`);
  },

  async toggleManager(id: string, isManager: boolean): Promise<{ message: string }> {
    return apiProxy.patch<{ message: string }>(`/api/workplacemembers/${id}/manager`, isManager);
  },
};

export const invitationsApi = {
  async getInvitations(): Promise<Invitation[]> {
    return apiProxy.get<Invitation[]>("/api/invitations");
  },

  async getInvitation(id: string): Promise<Invitation> {
    return apiProxy.get<Invitation>(`/api/invitations/${id}`);
  },

  async verifyInvitation(token: string): Promise<Invitation> {
    return apiProxy.get<Invitation>(`/api/invitations/verify/${token}`);
  },

  async createInvitation(data: CreateInvitationRequest): Promise<Invitation> {
    return apiProxy.post<Invitation>("/api/invitations", data);
  },

  async acceptInvitation(id: string, data: AcceptInvitationRequest): Promise<{ message: string }> {
    return apiProxy.post<{ message: string }>(`/api/invitations/${id}/accept`, data);
  },

  async cancelInvitation(id: string): Promise<void> {
    return apiProxy.delete(`/api/invitations/${id}`);
  },

  async resendInvitation(id: string): Promise<Invitation> {
    return apiProxy.post<Invitation>(`/api/invitations/${id}/resend`);
  },
};

/***** Categories *****/
export const categoriesApi = {
  async getCategories(): Promise<ExpenseCategory[]> {
    return apiProxy.get<ExpenseCategory[]>("/api/expensecategories");
  },

  async getCategory(id: string): Promise<ExpenseCategory> {
    return apiProxy.get<ExpenseCategory>(`/api/expensecategories/${id}`);
  },

  async createCategory(data: CreateExpenseCategoryRequest): Promise<ExpenseCategory> {
    return apiProxy.post<ExpenseCategory>("/api/expensecategories", data);
  },

  async updateCategory(id: string, data: UpdateExpenseCategoryRequest): Promise<void> {
    return apiProxy.put(`/api/expensecategories/${id}`, data);
  },

  async deleteCategory(id: string): Promise<void> {
    return apiProxy.delete(`/api/expensecategories/${id}`);
  },
  async deactivateCategory(id: string): Promise<void> {
    return apiProxy.patch(`/api/expensecategories/deactivate/${id}`);
  },
  async activateCategory(id: string): Promise<void> {
    return apiProxy.patch(`/api/expensecategories/activate/${id}`);
  },
};

/***** Roles *****/
export const rolesApi = {
  async getRoles(): Promise<Role[]> {
    return apiProxy.get<Role[]>("/api/roles");
  },
};

export const api = {
  auth: authApi,
  expenses: expensesApi,
  workplaces: workplacesApi,
  workplaceMembers: workplaceMembersApi,
  workplaceLimits: workplaceLimitsApi,
  invitations: invitationsApi,
  categories: categoriesApi,
  roles: rolesApi,
};

export default api;
