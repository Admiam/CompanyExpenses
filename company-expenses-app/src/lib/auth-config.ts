export const AUTH_CONFIG = {
  authServerUrl: import.meta.env.VITE_AUTH_SERVER_URL || "http://localhost:7169",
  apiServerUrl: import.meta.env.VITE_API_BASE_URL || "http://localhost:5200",
  cookieName: ".AspNetCore.Identity.Application",
  loginUrl: "/Account/Login",
  logoutUrl: "/Account/Logout",
  registerUrl: "/Account/Register",
};

export const AUTH_ENDPOINTS = {
  login: `${AUTH_CONFIG.authServerUrl}${AUTH_CONFIG.loginUrl}`,
  logout: `${AUTH_CONFIG.authServerUrl}${AUTH_CONFIG.logoutUrl}`,
  register: `${AUTH_CONFIG.authServerUrl}${AUTH_CONFIG.registerUrl}`,
  userInfo: `${AUTH_CONFIG.apiServerUrl}/api/auth/user`,
  checkAuth: `${AUTH_CONFIG.apiServerUrl}/api/auth/check`,
};
