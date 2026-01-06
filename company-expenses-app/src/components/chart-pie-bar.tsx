"use client";

import { Bar, BarChart, CartesianGrid, XAxis, Pie, PieChart, Legend } from "recharts";
import type { CategoryExpense, WorkplaceExpense } from "@/lib/proxy/types";

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { type ChartConfig, ChartContainer, ChartTooltip, ChartTooltipContent, ChartLegend, ChartLegendContent } from "@/components/ui/chart";

interface ChartBarPieProps {
  categoryData: CategoryExpense[];
  workplaceData: WorkplaceExpense[];
}

const barConfig = {
  total: {
    label: "Výdaje",
    color: "var(--chart-1)",
  },
} satisfies ChartConfig;

const pieConfig = {
  value: {
    label: "Částka",
  },
} satisfies ChartConfig;

const chartColors = ["var(--chart-1)", "var(--chart-2)", "var(--chart-3)", "var(--chart-4)", "var(--chart-5)"];
const workplaceColors = ["hsl(var(--chart-1))", "hsl(var(--chart-2))", "hsl(var(--chart-3))", "hsl(var(--chart-4))", "hsl(var(--chart-5))"];

export function ChartBarPie({ categoryData, workplaceData }: ChartBarPieProps) {
  // Get all unique categories for the stacked bar chart
  const allCategories = new Map<string, { name: string; color: string }>();
  workplaceData.forEach((workplace) => {
    workplace.categories.forEach((cat) => {
      if (!allCategories.has(cat.categoryId)) {
        allCategories.set(cat.categoryId, {
          name: cat.categoryName,
          color: cat.categoryColor || chartColors[allCategories.size % chartColors.length],
        });
      }
    });
  });

  // Transform data for stacked bar chart
  const barData = workplaceData.slice(0, 10).map((workplace) => {
    const dataPoint: any = {
      name: workplace.workplaceName,
    };
    // Add each category as a separate property
    workplace.categories.forEach((cat) => {
      dataPoint[cat.categoryName] = cat.total;
    });
    return dataPoint;
  });

  // Create dynamic bar config for categories
  const dynamicBarConfig: ChartConfig = {};
  allCategories.forEach((category, categoryId) => {
    dynamicBarConfig[category.name] = {
      label: category.name,
      color: category.color,
    };
  });

  // Transform data for pie chart with colors from database
  const pieData = categoryData.map((item, index) => ({
    name: item.categoryName,
    value: item.total,
    fill: item.categoryColor || chartColors[index % chartColors.length],
  }));

  return (
    <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
      {/* Bar chart - takes 2/3 width */}
      <Card className="lg:col-span-2">
        <CardHeader>
          <CardTitle>Výdaje podle pracovišť</CardTitle>
          <CardDescription>Top 10 pracovišť podle výdajů (schválené výdaje tento rok)</CardDescription>
        </CardHeader>
        <CardContent>
          {barData.length === 0 ? (
            <div className="flex items-center justify-center h-[250px] text-sm text-muted-foreground">Zatím nejsou k dispozici žádná data</div>
          ) : (
            <ChartContainer config={dynamicBarConfig}>
              <BarChart data={barData}>
                <CartesianGrid vertical={false} />
                <XAxis dataKey="name" tickLine={false} tickMargin={10} axisLine={false} tick={{ fontSize: 12 }} />
                <ChartTooltip cursor={false} content={<ChartTooltipContent indicator="dashed" />} />
                <ChartLegend content={<ChartLegendContent />} />
                {Array.from(allCategories.values()).map((category) => (
                  <Bar key={category.name} dataKey={category.name} stackId="a" fill={category.color} radius={[0, 0, 4, 4]} />
                ))}
              </BarChart>
            </ChartContainer>
          )}
        </CardContent>
      </Card>

      {/* Pie chart - takes 1/3 width */}
      <Card className="flex flex-col">
        <CardHeader>
          <CardTitle>Výdaje podle kategorií</CardTitle>
          <CardDescription>Všechny kategorie (schválené výdaje tento rok)</CardDescription>
        </CardHeader>
        <CardContent className="flex-1 flex items-center justify-center pb-0">
          {pieData.length === 0 ? (
            <div className="flex items-center justify-center h-[300px] text-sm text-muted-foreground w-full">Zatím nejsou k dispozici žádná data</div>
          ) : (
            <ChartContainer config={pieConfig} className="mx-auto aspect-square max-h-[300px] w-full">
              <PieChart>
                <ChartTooltip cursor={false} content={<ChartTooltipContent hideLabel />} />
                <Pie data={pieData} dataKey="value" nameKey="name" />
              </PieChart>
            </ChartContainer>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
