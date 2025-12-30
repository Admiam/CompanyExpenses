import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import App from "./App.tsx";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import Dashboard from "@/pages/dashboard/Dashboard.tsx";
import LoginRedirect from "@/app/auth/LoginRedirect.tsx";
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

            {/* Protected routes - require authentication */}
            <Route
              path="/dashboard"
              element={
                <ProtectedRoute>
                  <Dashboard />
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
