import { AppSidebar } from "@/components/app-sidebar"
import { ChartAreaInteractive } from "@/components/chart-area-interactive"
import { DataTable } from "@/components/data-table"
import { SectionCards } from "@/components/section-cards"
import { SiteHeader } from "@/components/site-header"
import {
    SidebarInset,
    SidebarProvider,
} from "@/components/ui/sidebar"
import { dummyDashboardStats } from "@/lib/dummy-data"
import { ThemeProvider } from "@/components/theme-provider"

import rawData from "./data.json"
import * as React from "react";
import {ChartBarPie} from "@/components/chart-pie-bar.tsx";

export default function Dashboard() {

    const data = rawData.map((item, index) => ({
        id: item.id,
        employee: item.reviewer === "Assign reviewer" ? "Unassigned" : item.reviewer,
        description: `${item.header} (${item.type})`,
        amount: Number(item.target.replace(/[^\d]/g, "")), // strip "Kč"
        date: new Date(2024, 11, index + 1).toISOString(), // fake date for demo
        status:
            item.status === "Done"
                ? "Approved"
                : item.status === "In Process"
                    ? "Pending"
                    : item.status === "Planned"
                        ? "Pending"
                        : "Rejected", // fallback
    }))

    return (
        <ThemeProvider defaultTheme="dark" storageKey="vite-ui-theme">
            <SidebarProvider
                style={
                    {
                        "--sidebar-width": "calc(var(--spacing) * 72)",
                        "--header-height": "calc(var(--spacing) * 12)",
                    } as React.CSSProperties
                }
            >
                <AppSidebar variant="inset" />
                <SidebarInset>
                    <SiteHeader />
                    <div className="flex flex-1 flex-col">
                        <div className="@container/main flex flex-1 flex-col gap-2">
                            <div className="flex flex-col gap-4 py-4 md:gap-6 md:py-6">
                                <SectionCards stats={dummyDashboardStats} />
                                <div className="px-4 lg:px-6">
                                    <ChartBarPie />
                                </div>
                                <div className="px-4 lg:px-6">
                                    <ChartAreaInteractive />
                                </div>
                                <DataTable data={data} />
                            </div>
                        </div>
                    </div>
                </SidebarInset>
            </SidebarProvider>
        </ThemeProvider>

    )
}
