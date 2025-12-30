// import { createContext, useContext, useState, useEffect, type ReactNode } from "react";
// import { AUTH_CONFIG } from "@/lib/auth-config";
// import type { User, AuthContextType } from "@/types/auth-types";

// // Context není exportován - použij useAuth hook
// const AuthContext = createContext<AuthContextType | undefined>(undefined);

// // Export pouze komponenty - pro Fast Refresh
// export function AuthProvider({ children }: { children: ReactNode }) {
//   const [user, setUser] = useState<User | null>(null);
//   const [isLoading, setIsLoading] = useState(true);

//   const checkAuth = async () => {
//     try {
//       // Zkontroluj, jestli máme cookie s autentizací
//       const response = await fetch(`${AUTH_CONFIG.apiServerUrl}/api/auth/user`, {
//         credentials: "include", // Důležité pro cookies
//         headers: {
//           Accept: "application/json",
//         },
//       });

//       if (response.ok) {
//         const userData = await response.json();
//         setUser(userData);
//       } else {
//         setUser(null);
//       }
//     } catch (error) {
//       console.error("Auth check failed:", error);
//       setUser(null);
//     } finally {
//       setIsLoading(false);
//     }
//   };

//   const login = () => {
//     // Redirect na Auth server login
//     const returnUrl = encodeURIComponent(window.location.origin + "/dashboard");
//     window.location.href = `${AUTH_CONFIG.authServerUrl}/Account/Login?returnUrl=${returnUrl}`;
//   };

//   const logout = async () => {
//     try {
//       // Logout na Auth serveru
//       await fetch(`${AUTH_CONFIG.authServerUrl}/Account/Logout`, {
//         method: "POST",
//         credentials: "include",
//       });

//       setUser(null);
//       window.location.href = "/";
//     } catch (error) {
//       console.error("Logout failed:", error);
//     }
//   };

//   useEffect(() => {
//     checkAuth();
//   }, []);

//   return (
//     <AuthContext.Provider
//       value={{
//         user,
//         isAuthenticated: !!user,
//         isLoading,
//         login,
//         logout,
//         checkAuth,
//       }}
//     >
//       {children}
//     </AuthContext.Provider>
//   );
// }

// // Hook pro přístup k AuthContext
// export function useAuth(): AuthContextType {
//   const context = useContext(AuthContext);
//   if (context === undefined) {
//     throw new Error("useAuth must be used within an AuthProvider");
//   }
//   return context;
// }
