// src/types/auth.ts
export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
  role: "employee" | "manager";
}

export interface RegisterResponse {
  id: string;
  name: string;
  email: string;
  role: "employee" | "manager";
  token?: string;
}
