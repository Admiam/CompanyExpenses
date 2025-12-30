import { useEffect, useState, type ReactNode } from "react";
import { AUTH_CONFIG } from "@/lib/auth-config";
import type { User } from "@/types/auth-types";
import { AuthContext } from "./AuthContext";
import { apiProxy } from "@/lib/proxy/proxy";

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const checkAuth = async () => {
    try {
      const userData = await apiProxy.get<User>("/api/auth/user");
      setUser(userData);
      console.log("✅ User authenticated:", userData.email);
    } catch (error: any) {
      if (error?.response?.status === 401) {
        setUser(null);
      } else {
        console.error("❌ Auth check failed:", error);
        setUser(null);
      }
    } finally {
      setIsLoading(false);
    }
  };

  const login = () => {
    const returnUrl = encodeURIComponent(window.location.origin + "/dashboard");
    window.location.href = `${AUTH_CONFIG.authServerUrl}/Account/Login?returnUrl=${returnUrl}`;
  };

  const logout = async () => {
    try {
      // Call API logout endpoint
      await apiProxy.post("/api/auth/logout");
      setUser(null);

      // Redirect to auth server logout
      window.location.href = `${AUTH_CONFIG.authServerUrl}/Account/Logout`;
    } catch (error) {
      console.error("Logout failed:", error);
      // Force logout locally even if API call fails
      setUser(null);
      window.location.href = "/";
    }
  };

  useEffect(() => {
    checkAuth();
  }, []);

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: !!user,
        isLoading,
        login,
        logout,
        checkAuth,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}
