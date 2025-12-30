export interface DashboardStats {
    totalExpenses: number
    monthlyExpenses: number
    monthlyChange: number // percent vs last month
    teams: number
    users: number
}

export const dummyDashboardStats: DashboardStats = {
    totalExpenses: 1250000,
    monthlyExpenses: 95300,
    monthlyChange: -5,
    teams: 12,
    users: 128,
}

