import { IconTrendingUp, IconTrendingDown, IconUsers, IconBuildingSkyscraper, IconAlertCircle } from "@tabler/icons-react";
import { useTranslation } from "react-i18next";

import { Badge } from "@/components/ui/badge";
import { Card, CardAction, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import type { DashboardStats } from "@/lib/proxy/types";

interface SectionCardsProps {
  stats: DashboardStats;
}

export function SectionCards({ stats }: SectionCardsProps) {
  const { t, i18n } = useTranslation();
  const getLocale = () => (i18n.language === "cs" ? "cs-CZ" : "en-US");

  return (
    <div className="grid grid-cols-1 gap-4 px-4 lg:px-6 @xl/main:grid-cols-2 @5xl/main:grid-cols-4">
      {/* Celkové výdaje */}
      <Card>
        <CardHeader>
          <CardDescription>{t("dashboard.totalExpenses")}</CardDescription>
          <CardTitle className="text-2xl font-semibold tabular-nums">
            {stats.totalExpenses.toLocaleString(getLocale())} {t("common.currency")}
          </CardTitle>
          <CardAction>
            <Badge variant="outline">
              <IconTrendingUp className="size-4" /> {t("common.total")}
            </Badge>
          </CardAction>
        </CardHeader>
        <CardFooter className="flex-col items-start gap-1.5 text-sm">
          <div className="flex gap-2 font-medium">{t("dashboard.approvedTotal")}</div>
          <div className="text-muted-foreground">{t("dashboard.allWorkplaces")}</div>
        </CardFooter>
      </Card>

      {/* Výdaje tento měsíc */}
      <Card>
        <CardHeader>
          <CardDescription>{t("dashboard.monthlyExpenses")}</CardDescription>
          <CardTitle className="text-2xl font-semibold tabular-nums">
            {stats.monthlyExpenses.toLocaleString(getLocale())} {t("common.currency")}
          </CardTitle>
          <CardAction>
            <Badge variant={stats.monthlyChange >= 0 ? "outline" : "secondary"}>
              {stats.monthlyChange >= 0 ? (
                <>
                  <IconTrendingUp className="size-4" /> +{stats.monthlyChange}%
                </>
              ) : (
                <>
                  <IconTrendingDown className="size-4" /> {stats.monthlyChange}%
                </>
              )}
            </Badge>
          </CardAction>
        </CardHeader>
        <CardFooter className="flex-col items-start gap-1.5 text-sm">
          <div className="flex gap-2 font-medium">{stats.monthlyChange >= 0 ? t("dashboard.monthlyGrowth") : t("dashboard.monthlyDecline")}</div>
          <div className="text-muted-foreground">{t("dashboard.monthComparison")}</div>
        </CardFooter>
      </Card>

      {/* Počet týmů */}
      <Card>
        <CardHeader>
          <CardDescription>{t("dashboard.teamsWorkplaces")}</CardDescription>
          <CardTitle className="text-2xl font-semibold tabular-nums">{stats.workplacesCount}</CardTitle>
          <CardAction>
            <Badge variant="outline">
              <IconBuildingSkyscraper className="size-4" />
            </Badge>
          </CardAction>
        </CardHeader>
        <CardFooter className="flex-col items-start gap-1.5 text-sm">
          <div className="flex gap-2 font-medium">{t("dashboard.activeWorkplaces")}</div>
          <div className="text-muted-foreground">{t("dashboard.budgetNote")}</div>
        </CardFooter>
      </Card>

      {/* Počet uživatelů a čekající výdaje */}
      <Card>
        <CardHeader>
          <CardDescription>{t("dashboard.users")}</CardDescription>
          <CardTitle className="text-2xl font-semibold tabular-nums">{stats.usersCount}</CardTitle>
          <CardAction>
            <Badge variant="outline">
              <IconUsers className="size-4" />
            </Badge>
          </CardAction>
        </CardHeader>
        <CardFooter className="flex-col items-start gap-1.5 text-sm">
          <div className="flex gap-2 font-medium items-center">
            {t("roles.employee")}
            {stats.pendingExpensesCount > 0 && (
              <Badge variant="destructive" className="ml-2">
                <IconAlertCircle className="size-3 mr-1" />
                {stats.pendingExpensesCount} {t("dashboard.pendingExpenses").toLowerCase()}
              </Badge>
            )}
          </div>
          <div className="text-muted-foreground">
            {stats.pendingExpensesCount > 0
              ? t("expenses.expensesCount", { count: stats.pendingExpensesCount }) + " " + t("expenses.status.pending").toLowerCase()
              : t("dashboard.noExpensesYet")}
          </div>
        </CardFooter>
      </Card>
    </div>
  );
}
