export const roleLabels = {
  admin: "Administrátor",
  manager: "Manažer",
  employee: "Zaměstnanec",
} as const;

export const roleColors = {
  admin: "bg-purple-500/10 text-purple-500",
  manager: "bg-blue-500/10 text-blue-500",
  employee: "bg-gray-500/10 text-gray-500",
} as const;

export type RoleType = keyof typeof roleLabels;
