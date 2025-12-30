import { Navigate, useLocation } from "react-router-dom";
import { useAuth } from "@/auth/useAuth";

interface ProtectedRouteProps {
  children: React.ReactNode;
}

export function ProtectedRoute({ children }: ProtectedRouteProps) {
  const { isAuthenticated, isLoading } = useAuth();
  const location = useLocation();

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-gray-900 dark:border-white"></div>
      </div>
    );
  }

  if (!isAuthenticated) {
    // Uložit současnou cestu pro redirect po přihlášení
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  return <>{children}</>;
}
