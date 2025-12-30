import {
  IconTrendingUp,
  IconTrendingDown,
  IconUsers,
  IconBuildingSkyscraper,
} from "@tabler/icons-react"

import { Badge } from "@/components/ui/badge"
import {
  Card,
  CardAction,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"
import type {DashboardStats} from "@/lib/dummy-data"

interface SectionCardsProps {
  stats: DashboardStats
}

export function SectionCards({ stats }: SectionCardsProps) {
  return (
      <div className="grid grid-cols-1 gap-4 px-4 lg:px-6 @xl/main:grid-cols-2 @5xl/main:grid-cols-4">
        {/* Celkové výdaje */}
        <Card>
          <CardHeader>
            <CardDescription>Celkové výdaje</CardDescription>
            <CardTitle className="text-2xl font-semibold tabular-nums">
              {stats.totalExpenses.toLocaleString("cs-CZ")} Kč
            </CardTitle>
            <CardAction>
              <Badge variant="outline">
                <IconTrendingUp className="size-4" /> +8%
              </Badge>
            </CardAction>
          </CardHeader>
          <CardFooter className="flex-col items-start gap-1.5 text-sm">
            <div className="flex gap-2 font-medium">
              Meziroční nárůst <IconTrendingUp className="size-4" />
            </div>
            <div className="text-muted-foreground">Vs. loňský rok</div>
          </CardFooter>
        </Card>

        {/* Výdaje tento měsíc */}
        <Card>
          <CardHeader>
            <CardDescription>Výdaje tento měsíc</CardDescription>
            <CardTitle className="text-2xl font-semibold tabular-nums">
              {stats.monthlyExpenses.toLocaleString("cs-CZ")} Kč
            </CardTitle>
            <CardAction>
              <Badge variant="outline">
                {stats.monthlyChange >= 0 ? (
                    <>
                      <IconTrendingUp className="size-4" /> {stats.monthlyChange}%
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
            <div className="flex gap-2 font-medium">
              {stats.monthlyChange >= 0 ? "Růst oproti minulému měsíci" : "Pokles oproti minulému měsíci"}
            </div>
            <div className="text-muted-foreground">Porovnání měsíc/měsíc</div>
          </CardFooter>
        </Card>

        {/* Počet týmů */}
        <Card>
          <CardHeader>
            <CardDescription>Týmy / pracoviště</CardDescription>
            <CardTitle className="text-2xl font-semibold tabular-nums">
              {stats.teams}
            </CardTitle>
            <CardAction>
              <Badge variant="outline">
                <IconBuildingSkyscraper className="size-4" />
              </Badge>
            </CardAction>
          </CardHeader>
          <CardFooter className="flex-col items-start gap-1.5 text-sm">
            <div className="flex gap-2 font-medium">Aktivní pracoviště</div>
            <div className="text-muted-foreground">
              Rozpočet lze nastavit pro každý tým
            </div>
          </CardFooter>
        </Card>

        {/* Počet uživatelů */}
        <Card>
          <CardHeader>
            <CardDescription>Uživatelé</CardDescription>
            <CardTitle className="text-2xl font-semibold tabular-nums">
              {stats.users}
            </CardTitle>
            <CardAction>
              <Badge variant="outline">
                <IconUsers className="size-4" />
              </Badge>
            </CardAction>
          </CardHeader>
          <CardFooter className="flex-col items-start gap-1.5 text-sm">
            <div className="flex gap-2 font-medium">Registrovaní zaměstnanci</div>
            <div className="text-muted-foreground">
              Správa přístupů v sekci Uživatelé
            </div>
          </CardFooter>
        </Card>
      </div>
  )
}
