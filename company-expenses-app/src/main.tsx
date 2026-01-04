import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import App from "./App.tsx";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import Dashboard from "@/pages/dashboard/Dashboard.tsx";
import ExpensesPage from "@/pages/expenses/ExpensesPage.tsx";
import WorkplacesPage from "@/pages/workplaces/WorkplacesPage.tsx";
import UsersPage from "@/pages/users/UsersPage.tsx";
import CategoriesPage from "@/pages/categories/CategoriesPage.tsx";
import LoginRedirect from "@/pages/auth/LoginRedirect.tsx";
import RegisterPage from "@/pages/RegisterPage.tsx";
import { ThemeProvider } from "@/components/theme-provider";
import { AuthProvider } from "@/auth";
import { ProtectedRoute } from "@/components/ProtectedRoute.tsx";
import { PublicRoute } from "@/components/PublicRoute.tsx";

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <ThemeProvider defaultTheme="dark" storageKey="vite-ui-theme">
      <BrowserRouter>
        <AuthProvider>
          <Routes>
            {/* Public routes - redirect to dashboard if authenticated */}
            <Route
              path="/"
              element={
                <PublicRoute>
                  <App />
                </PublicRoute>
              }
            />
            <Route
              path="/login"
              element={
                <PublicRoute>
                  <LoginRedirect />
                </PublicRoute>
              }
            />
            <Route
              path="/register"
              element={
                <PublicRoute>
                  <RegisterPage />
                </PublicRoute>
              }
            />

            {/* Protected routes - require authentication */}
            <Route
              path="/dashboard"
              element={
                <ProtectedRoute>
                  <Dashboard />
                </ProtectedRoute>
              }
            />
            <Route
              path="/expenses"
              element={
                <ProtectedRoute>
                  <ExpensesPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/workplaces"
              element={
                <ProtectedRoute>
                  <WorkplacesPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/users"
              element={
                <ProtectedRoute>
                  <UsersPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/categories"
              element={
                <ProtectedRoute>
                  <CategoriesPage />
                </ProtectedRoute>
              }
            />

            {/* Catch all - redirect to login */}
            <Route path="*" element={<LoginRedirect />} />
          </Routes>
        </AuthProvider>
      </BrowserRouter>
    </ThemeProvider>
  </StrictMode>
);
