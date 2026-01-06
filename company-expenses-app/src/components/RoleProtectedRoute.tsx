import { Navigate } from "react-router-dom";
import { useAuth } from "@/auth/useAuth";
import { hasAnyRole, type UserRole } from "@/utils/roles";

interface RoleProtectedRouteProps {
  children: React.ReactNode;
  requiredRoles: UserRole[];
}

/**
 * Component that protects routes based on user roles
 * Redirects to dashboard if user doesn't have required role
 */
export function RoleProtectedRoute({ children, requiredRoles }: RoleProtectedRouteProps) {
  const { user, isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-gray-900 dark:border-white"></div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  // Check if user has required role
  if (!hasAnyRole(user?.role, requiredRoles)) {
    // User is authenticated but doesn't have permission
    // Redirect to dashboard with error message
    return <Navigate to="/dashboard" replace />;
  }

  return <>{children}</>;
}
