"use client"

import { Bar, BarChart, CartesianGrid, XAxis, Pie, PieChart } from "recharts"

import {
    Card,
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

// Dummy data for bar chart (expenses by team)
const barData = [
    { team: "Marketing", amount: 25000 },
    { team: "Vývoj", amount: 40000 },
    { team: "Prodej", amount: 32000 },
    { team: "HR", amount: 12000 },
]

// Dummy data for pie chart (expenses by category)
const pieData = [
    { category: "Kancelář", value: 18000, fill: "var(--chart-1)" },
    { category: "Cestovné", value: 25000, fill: "var(--chart-2)" },
    { category: "Zábava", value: 12000, fill: "var(--chart-3)" },
    { category: "Jiné", value: 8000, fill: "var(--chart-4)" },
]

const barConfig = {
    amount: {
        label: "Výdaje",
        color: "var(--chart-1)",
    },
} satisfies ChartConfig

const pieConfig = {
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
    jine: {
        label: "Jiné",
        color: "var(--chart-4)",
    },
} satisfies ChartConfig

export function ChartBarPie() {
    return (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Bar chart - takes 2/3 width */}
            <Card className="lg:col-span-2">
                <CardHeader>
                    <CardTitle>Výdaje podle týmů</CardTitle>
                    <CardDescription>Porovnání týmů / pracovišť</CardDescription>
                </CardHeader>
                <CardContent>
                    <ChartContainer config={barConfig}>
                        <BarChart data={barData}>
                            <CartesianGrid vertical={false} />
                            <XAxis
                                dataKey="team"
                                tickLine={false}
                                tickMargin={10}
                                axisLine={false}
                            />
                            <ChartTooltip
                                cursor={false}
                                content={<ChartTooltipContent indicator="dashed" />}
                            />
                            <Bar dataKey="amount" fill="var(--chart-1)" radius={6} />
                        </BarChart>
                    </ChartContainer>
                </CardContent>
            </Card>

            {/* Pie chart - takes 1/3 width */}
            <Card>
                <CardHeader>
                    <CardTitle>Výdaje podle kategorií</CardTitle>
                    <CardDescription>Rozpad výdajů</CardDescription>
                </CardHeader>
                <CardContent>
                    <ChartContainer
                        config={pieConfig}
                        className="mx-auto aspect-square max-h-[250px]"
                    >
                        <PieChart>
                            <ChartTooltip
                                cursor={false}
                                content={<ChartTooltipContent hideLabel />}
                            />
                            <Pie data={pieData} dataKey="value" nameKey="category" />
                        </PieChart>
                    </ChartContainer>
                </CardContent>
            </Card>
        </div>
    )
}
