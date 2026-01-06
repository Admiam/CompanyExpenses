import { SectionCards } from "@/components/section-cards";
import { ChartBarPie } from "@/components/chart-pie-bar.tsx";
import { MainLayout } from "@/components/layouts/MainLayout";
import { useState, useEffect } from "react";
import { expensesApi } from "@/lib/proxy/api";
import type { DashboardStats } from "@/lib/proxy/types";
import { toast } from "sonner";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Calendar, TrendingUp, Building2, Tag } from "lucide-react";
import { Skeleton } from "@/components/ui/skeleton";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { useTranslation } from "react-i18next";

export default function Dashboard() {
  const { t, i18n } = useTranslation();
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    loadDashboardData();
  }, []);

  const loadDashboardData = async () => {
    try {
      setIsLoading(true);
      const data = await expensesApi.getDashboardStats();
      setStats(data);
    } catch (error) {
      console.error("Failed to load dashboard data:", error);
      toast.error(t("dashboard.failedToLoadDashboard"));
    } finally {
      setIsLoading(false);
    }
  };

  const getLocale = () => (i18n.language === "cs" ? "cs-CZ" : "en-US");

  if (isLoading) {
    return (
      <MainLayout>
        <div className="flex flex-col gap-4 py-4 md:gap-6 md:py-6">
          <div className="grid grid-cols-1 gap-4 px-4 lg:px-6 @xl/main:grid-cols-2 @5xl/main:grid-cols-4">
            {[1, 2, 3, 4].map((i) => (
              <Card key={i}>
                <CardHeader>
                  <Skeleton className="h-4 w-32" />
                  <Skeleton className="h-8 w-40" />
                </CardHeader>
              </Card>
            ))}
          </div>
        </div>
      </MainLayout>
    );
  }

  if (!stats) {
    return (
      <MainLayout>
        <div className="flex items-center justify-center h-96">
          <p className="text-muted-foreground">{t("dashboard.failedToLoad")}</p>
        </div>
      </MainLayout>
    );
  }

  return (
    <MainLayout>
      <div className="flex flex-col gap-4 py-4 md:gap-6 md:py-6">
        {/* Statistiky */}
        <SectionCards stats={stats} />

        {/* Grafy */}
        <div className="px-4 lg:px-6">
          <ChartBarPie categoryData={stats.expensesByCategory} workplaceData={stats.expensesByWorkplace} />
        </div>

        {/* Detailní přehledy v tabulkách */}
        <div className="px-4 lg:px-6 grid gap-6 md:grid-cols-2">
          {/* Všechna pracoviště */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Building2 className="h-5 w-5" />
                {t("dashboard.expensesByWorkplace")}
              </CardTitle>
              <CardDescription>{t("dashboard.workplacesWithExpenses", { count: stats.expensesByWorkplace.length })}</CardDescription>
            </CardHeader>
            <CardContent>
              {stats.expensesByWorkplace.length === 0 ? (
                <p className="text-sm text-muted-foreground text-center py-8">{t("dashboard.noWorkplaceExpenses")}</p>
              ) : (
                <div className="max-h-[400px] overflow-y-auto">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>{t("nav.workplaces")}</TableHead>
                        <TableHead className="text-right">{t("common.count")}</TableHead>
                        <TableHead className="text-right">{t("common.total")}</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {stats.expensesByWorkplace.map((workplace) => (
                        <TableRow key={workplace.workplaceId}>
                          <TableCell className="font-medium">{workplace.workplaceName}</TableCell>
                          <TableCell className="text-right">{workplace.count}</TableCell>
                          <TableCell className="text-right font-semibold">
                            {workplace.total.toLocaleString(getLocale())} {t("common.currency")}
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </div>
              )}
            </CardContent>
          </Card>

          {/* Všechny kategorie */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Tag className="h-5 w-5" />
                {t("dashboard.expensesByCategory")}
              </CardTitle>
              <CardDescription>{t("dashboard.categoriesWithExpenses", { count: stats.expensesByCategory.length })}</CardDescription>
            </CardHeader>
            <CardContent>
              {stats.expensesByCategory.length === 0 ? (
                <p className="text-sm text-muted-foreground text-center py-8">{t("dashboard.noCategoryExpenses")}</p>
              ) : (
                <div className="max-h-[400px] overflow-y-auto">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>{t("expenses.category")}</TableHead>
                        <TableHead className="text-right">{t("common.count")}</TableHead>
                        <TableHead className="text-right">{t("common.total")}</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {stats.expensesByCategory.map((category) => (
                        <TableRow key={category.categoryId}>
                          <TableCell className="font-medium">{category.categoryName}</TableCell>
                          <TableCell className="text-right">{category.count}</TableCell>
                          <TableCell className="text-right font-semibold">
                            {category.total.toLocaleString(getLocale())} {t("common.currency")}
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </div>
              )}
            </CardContent>
          </Card>
        </div>

        {/* Poslední výdaje */}
        <div className="px-4 lg:px-6">
          <Card>
            <CardHeader>
              <CardTitle>{t("dashboard.recentExpenses")}</CardTitle>
              <CardDescription>{t("dashboard.recentExpensesDesc")}</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {stats.recentExpenses.length === 0 ? (
                  <p className="text-sm text-muted-foreground text-center py-4">{t("dashboard.noExpensesYet")}</p>
                ) : (
                  stats.recentExpenses.map((expense) => (
                    <div key={expense.id} className="flex items-center justify-between border-b pb-4 last:border-0 last:pb-0">
                      <div className="flex-1">
                        <div className="flex items-center gap-2">
                          <p className="font-medium">{expense.description}</p>
                          <Badge variant={expense.status === "Approved" ? "default" : expense.status === "Rejected" ? "destructive" : "secondary"}>
                            {expense.status === "Approved"
                              ? t("expenses.status.approved")
                              : expense.status === "Rejected"
                              ? t("expenses.status.rejected")
                              : t("expenses.status.pending")}
                          </Badge>
                        </div>
                        <div className="flex items-center gap-4 mt-1 text-sm text-muted-foreground">
                          {expense.categoryName && (
                            <span className="flex items-center gap-1">
                              <TrendingUp className="h-3 w-3" />
                              {expense.categoryName}
                            </span>
                          )}
                          {expense.workplaceName && <span>{expense.workplaceName}</span>}
                          <span className="flex items-center gap-1">
                            <Calendar className="h-3 w-3" />
                            {new Date(expense.expenseDate).toLocaleDateString(getLocale())}
                          </span>
                        </div>
                      </div>
                      <div className="text-right">
                        <p className="font-semibold text-lg">
                          {expense.amount.toLocaleString(getLocale())} {expense.currency}
                        </p>
                        <p className="text-xs text-muted-foreground">{new Date(expense.submittedAt).toLocaleDateString(getLocale())}</p>
                      </div>
                    </div>
                  ))
                )}
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </MainLayout>
  );
}
