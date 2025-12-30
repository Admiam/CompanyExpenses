"use client"

import * as React from "react"
import { Area, AreaChart, CartesianGrid, XAxis } from "recharts"

import { useIsMobile } from "@/hooks/use-mobile"
import {
  Card,
  CardAction,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"
import {
  type ChartConfig,
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
} from "@/components/ui/chart"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import {
  ToggleGroup,
  ToggleGroupItem,
} from "@/components/ui/toggle-group"

// Dummy výdaje podle kategorií
const chartData = [
  { date: "2024-05-01", kancelar: 1200, cestovne: 800, zabava: 500 },
  { date: "2024-05-02", kancelar: 900, cestovne: 1200, zabava: 200 },
  { date: "2024-05-03", kancelar: 1500, cestovne: 700, zabava: 600 },
  { date: "2024-05-04", kancelar: 1100, cestovne: 900, zabava: 400 },
  { date: "2024-05-05", kancelar: 1800, cestovne: 600, zabava: 1000 },
  { date: "2024-05-06", kancelar: 2000, cestovne: 1500, zabava: 800 },
  { date: "2024-05-07", kancelar: 1700, cestovne: 1100, zabava: 700 },
]

// Konfigurace grafu
const chartConfig = {
  kancelar: {
    label: "Kancelář",
    color: "var(--chart-1)",
  },
  cestovne: {
    label: "Cestovné",
    color: "var(--chart-2)",
  },
  zabava: {
    label: "Zábava",
    color: "var(--chart-3)",
  },
} satisfies ChartConfig

export function ChartAreaInteractive() {
  const isMobile = useIsMobile()
  const [timeRange, setTimeRange] = React.useState("7d")

  React.useEffect(() => {
    if (isMobile) {
      setTimeRange("7d")
    }
  }, [isMobile])

  return (
      <Card className="@container/card">
        <CardHeader>
          <CardTitle>Výdaje v čase</CardTitle>
          <CardDescription>
            Přehled podle kategorií za poslední období
          </CardDescription>
          <CardAction>
            <ToggleGroup
                type="single"
                value={timeRange}
                onValueChange={setTimeRange}
                variant="outline"
                className="hidden *:data-[slot=toggle-group-item]:!px-4 @[767px]/card:flex"
            >
              <ToggleGroupItem value="30d">Posledních 30 dní</ToggleGroupItem>
              <ToggleGroupItem value="7d">Posledních 7 dní</ToggleGroupItem>
            </ToggleGroup>
            <Select value={timeRange} onValueChange={setTimeRange}>
              <SelectTrigger
                  className="flex w-40 @[767px]/card:hidden"
                  size="sm"
                  aria-label="Vyber období"
              >
                <SelectValue placeholder="Vyber období" />
              </SelectTrigger>
              <SelectContent className="rounded-xl">
                <SelectItem value="30d">Posledních 30 dní</SelectItem>
                <SelectItem value="7d">Posledních 7 dní</SelectItem>
              </SelectContent>
            </Select>
          </CardAction>
        </CardHeader>
        <CardContent className="px-2 pt-4 sm:px-6 sm:pt-6">
          <ChartContainer
              config={chartConfig}
              className="aspect-auto h-[250px] w-full"
          >
            <AreaChart data={chartData}>
              <CartesianGrid vertical={false} />
              <XAxis
                  dataKey="date"
                  tickLine={false}
                  axisLine={false}
                  tickMargin={8}
                  minTickGap={32}
                  tickFormatter={(value) =>
                      new Date(value).toLocaleDateString("cs-CZ", {
                        month: "short",
                        day: "numeric",
                      })
                  }
              />
              <ChartTooltip
                  cursor={false}
                  content={
                    <ChartTooltipContent
                        labelFormatter={(value) =>
                            new Date(value).toLocaleDateString("cs-CZ", {
                              month: "short",
                              day: "numeric",
                            })
                        }
                        indicator="dot"
                    />
                  }
              />
              <Area
                  dataKey="kancelar"
                  type="natural"
                  fill="var(--chart-1)"
                  stroke="var(--chart-1)"
                  stackId="a"
              />
              <Area
                  dataKey="cestovne"
                  type="natural"
                  fill="var(--chart-2)"
                  stroke="var(--chart-2)"
                  stackId="a"
              />
              <Area
                  dataKey="zabava"
                  type="natural"
                  fill="var(--chart-3)"
                  stroke="var(--chart-3)"
                  stackId="a"
              />
            </AreaChart>
          </ChartContainer>
        </CardContent>
      </Card>
  )
}
