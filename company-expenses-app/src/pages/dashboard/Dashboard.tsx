import { SectionCards } from "@/components/section-cards";
import { dummyDashboardStats } from "@/lib/dummy-data";
import { ChartBarPie } from "@/components/chart-pie-bar.tsx";
import { MainLayout } from "@/components/layouts/MainLayout";

export default function Dashboard() {
  return (
    <MainLayout>
      <div className="flex flex-col gap-4 py-4 md:gap-6 md:py-6">
        {/* Statistiky */}
        <SectionCards stats={dummyDashboardStats} />

        {/* Grafy */}
        <div className="px-4 lg:px-6">
          <ChartBarPie />
        </div>
      </div>
    </MainLayout>
  );
}
