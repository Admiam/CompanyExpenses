// Auth configuration - values loaded from environment variables
export const AUTH_CONFIG = {
  // Auth server URL (Blazor Identity) - from environment variable
  authServerUrl: import.meta.env.VITE_AUTH_SERVER_URL || "https://localhost:7169",

  // API server URL - from environment variable
  apiServerUrl: import.meta.env.VITE_API_BASE_URL || "https://localhost:7200",

  // Cookie settings
  cookieName: ".AspNetCore.Identity.Application",

  // Redirect URLs
  loginUrl: "/Account/Login",
  logoutUrl: "/Account/Logout",
  registerUrl: "/Account/Register",
};

export const AUTH_ENDPOINTS = {
  // Auth server endpoints
  login: `${AUTH_CONFIG.authServerUrl}${AUTH_CONFIG.loginUrl}`,
  logout: `${AUTH_CONFIG.authServerUrl}${AUTH_CONFIG.logoutUrl}`,
  register: `${AUTH_CONFIG.authServerUrl}${AUTH_CONFIG.registerUrl}`,

  // API endpoints for user info
  userInfo: `${AUTH_CONFIG.apiServerUrl}/api/auth/user`,
  checkAuth: `${AUTH_CONFIG.apiServerUrl}/api/auth/check`,
};
