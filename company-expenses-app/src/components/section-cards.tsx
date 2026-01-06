import { IconTrendingUp, IconTrendingDown, IconUsers, IconBuildingSkyscraper, IconAlertCircle } from "@tabler/icons-react";

import { Badge } from "@/components/ui/badge";
import { Card, CardAction, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import type { DashboardStats } from "@/lib/proxy/types";

interface SectionCardsProps {
  stats: DashboardStats;
}

export function SectionCards({ stats }: SectionCardsProps) {
  return (
    <div className="grid grid-cols-1 gap-4 px-4 lg:px-6 @xl/main:grid-cols-2 @5xl/main:grid-cols-4">
      {/* Celkové výdaje */}
      <Card>
        <CardHeader>
          <CardDescription>Celkové výdaje (schváleno)</CardDescription>
          <CardTitle className="text-2xl font-semibold tabular-nums">{stats.totalExpenses.toLocaleString("cs-CZ")} Kč</CardTitle>
          <CardAction>
            <Badge variant="outline">
              <IconTrendingUp className="size-4" /> Celkem
            </Badge>
          </CardAction>
        </CardHeader>
        <CardFooter className="flex-col items-start gap-1.5 text-sm">
          <div className="flex gap-2 font-medium">Schválené výdaje celkem</div>
          <div className="text-muted-foreground">Všechna pracoviště</div>
        </CardFooter>
      </Card>

      {/* Výdaje tento měsíc */}
      <Card>
        <CardHeader>
          <CardDescription>Výdaje tento měsíc</CardDescription>
          <CardTitle className="text-2xl font-semibold tabular-nums">{stats.monthlyExpenses.toLocaleString("cs-CZ")} Kč</CardTitle>
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
          <div className="flex gap-2 font-medium">{stats.monthlyChange >= 0 ? "Růst oproti minulému měsíci" : "Pokles oproti minulému měsíci"}</div>
          <div className="text-muted-foreground">Porovnání měsíc/měsíc</div>
        </CardFooter>
      </Card>

      {/* Počet týmů */}
      <Card>
        <CardHeader>
          <CardDescription>Týmy / pracoviště</CardDescription>
          <CardTitle className="text-2xl font-semibold tabular-nums">{stats.workplacesCount}</CardTitle>
          <CardAction>
            <Badge variant="outline">
              <IconBuildingSkyscraper className="size-4" />
            </Badge>
          </CardAction>
        </CardHeader>
        <CardFooter className="flex-col items-start gap-1.5 text-sm">
          <div className="flex gap-2 font-medium">Aktivní pracoviště</div>
          <div className="text-muted-foreground">Rozpočet lze nastavit pro každý tým</div>
        </CardFooter>
      </Card>

      {/* Počet uživatelů a čekající výdaje */}
      <Card>
        <CardHeader>
          <CardDescription>Uživatelé</CardDescription>
          <CardTitle className="text-2xl font-semibold tabular-nums">{stats.usersCount}</CardTitle>
          <CardAction>
            <Badge variant="outline">
              <IconUsers className="size-4" />
            </Badge>
          </CardAction>
        </CardHeader>
        <CardFooter className="flex-col items-start gap-1.5 text-sm">
          <div className="flex gap-2 font-medium items-center">
            Registrovaní zaměstnanci
            {stats.pendingExpensesCount > 0 && (
              <Badge variant="destructive" className="ml-2">
                <IconAlertCircle className="size-3 mr-1" />
                {stats.pendingExpensesCount} čeká
              </Badge>
            )}
          </div>
          <div className="text-muted-foreground">
            {stats.pendingExpensesCount > 0 ? `${stats.pendingExpensesCount} výdajů čeká na schválení` : "Žádné výdaje nečekají na schválení"}
          </div>
        </CardFooter>
      </Card>
    </div>
  );
}
