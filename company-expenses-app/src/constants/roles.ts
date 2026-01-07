export const roleLabels = {
  admin: "roles.admin",
  manager: "roles.manager",
  employee: "roles.employee",
} as const;

export const roleColors = {
  admin: "bg-purple-500/10 text-purple-500",
  manager: "bg-blue-500/10 text-blue-500",
  employee: "bg-gray-500/10 text-gray-500",
} as const;

export type RoleType = keyof typeof roleLabels;

export const getRoleLabel = (role: RoleType | string, t: (key: string) => string): string => {
  const key = roleLabels[role as RoleType];
  return key ? t(key) : role;
};
