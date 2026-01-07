"use client";

import { Bar, BarChart, CartesianGrid, XAxis, Pie, PieChart } from "recharts";
import type { CategoryExpense, WorkplaceExpense } from "@/lib/proxy/types";
import { useTranslation } from "react-i18next";

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { type ChartConfig, ChartContainer, ChartTooltip, ChartTooltipContent, ChartLegend, ChartLegendContent } from "@/components/ui/chart";

interface ChartBarPieProps {
  categoryData: CategoryExpense[];
  workplaceData: WorkplaceExpense[];
}

const chartColors = ["var(--chart-1)", "var(--chart-2)", "var(--chart-3)", "var(--chart-4)", "var(--chart-5)"];

export function ChartBarPie({ categoryData, workplaceData }: ChartBarPieProps) {
  const { t } = useTranslation();

  const pieConfig = {
    value: {
      label: t("common.amount"),
    },
  } satisfies ChartConfig;

  const safeWorkplaceData = workplaceData ?? [];
  const safeCategoryData = categoryData ?? [];

  const allCategories = new Map<string, { name: string; color: string }>();
  safeWorkplaceData.forEach((workplace) => {
    (workplace.categories ?? []).forEach((cat) => {
      if (!allCategories.has(cat.categoryId)) {
        allCategories.set(cat.categoryId, {
          name: cat.categoryName,
          color: cat.categoryColor || chartColors[allCategories.size % chartColors.length],
        });
      }
    });
  });

  const barData = safeWorkplaceData.slice(0, 10).map((workplace) => {
    const dataPoint: any = {
      name: workplace.workplaceName,
    };
    (workplace.categories ?? []).forEach((cat) => {
      dataPoint[cat.categoryName] = cat.total;
    });
    return dataPoint;
  });

  const dynamicBarConfig: ChartConfig = {};
  allCategories.forEach((category) => {
    dynamicBarConfig[category.name] = {
      label: category.name,
      color: category.color,
    };
  });

  const pieData = safeCategoryData.map((item, index) => ({
    name: item.categoryName,
    value: item.total,
    fill: item.categoryColor || chartColors[index % chartColors.length],
  }));

  return (
    <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
      <Card className="lg:col-span-2">
        <CardHeader>
          <CardTitle>{t("charts.expensesByWorkplace")}</CardTitle>
          <CardDescription>{t("dashboard.topWorkplaces")}</CardDescription>
        </CardHeader>
        <CardContent>
          {barData.length === 0 ? (
            <div className="flex items-center justify-center h-[250px] text-sm text-muted-foreground">{t("charts.noData")}</div>
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

      <Card className="flex flex-col">
        <CardHeader>
          <CardTitle>{t("charts.expensesByCategory")}</CardTitle>
          <CardDescription>{t("dashboard.expensesByCategories")}</CardDescription>
        </CardHeader>
        <CardContent className="flex-1 flex items-center justify-center pb-0">
          {pieData.length === 0 ? (
            <div className="flex items-center justify-center h-[300px] text-sm text-muted-foreground w-full">{t("charts.noData")}</div>
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
