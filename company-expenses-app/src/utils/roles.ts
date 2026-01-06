/**
 * Role management utilities
 */

export type UserRole = "Admin" | "Manager" | "User";

/**
 * Check if user has specific role
 */
export function hasRole(userRole: string | undefined, requiredRole: UserRole): boolean {
  if (!userRole) return false;
  return userRole === requiredRole;
}

/**
 * Check if user has any of the specified roles
 */
export function hasAnyRole(userRole: string | undefined, requiredRoles: UserRole[]): boolean {
  if (!userRole) return false;
  return requiredRoles.includes(userRole as UserRole);
}

/**
 * Check if user is admin
 */
export function isAdmin(userRole: string | undefined): boolean {
  return hasRole(userRole, "Admin");
}

/**
 * Check if user is manager or admin
 */
export function isManagerOrAdmin(userRole: string | undefined): boolean {
  return hasAnyRole(userRole, ["Admin", "Manager"]);
}

/**
 * Check if user can access workplaces page
 */
export function canAccessWorkplaces(userRole: string | undefined): boolean {
  return isManagerOrAdmin(userRole);
}

/**
 * Check if user can access users page
 */
export function canAccessUsers(userRole: string | undefined): boolean {
  return isManagerOrAdmin(userRole);
}

/**
 * Check if user can access categories page
 */
export function canAccessCategories(userRole: string | undefined): boolean {
  return isManagerOrAdmin(userRole);
}
