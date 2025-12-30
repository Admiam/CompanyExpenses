/**
 * API Service - High-level API methods
 * Use this file to define your API endpoints
 */

import { apiProxy } from "./proxy";
import type { LoginRequest, RegisterRequest, AuthResponse, Expense, CreateExpenseRequest, UpdateExpenseRequest, PaginatedResponse } from "./types";

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
 * Export all API methods
 */
export const api = {
  auth: authApi,
  expenses: expensesApi,
};

export default api;
