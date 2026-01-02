import { MainLayout } from "@/components/layouts/MainLayout";
import { Button } from "@/components/ui/button";
import { Plus, MapPin, Users } from "lucide-react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";

// Mock data
const mockWorkplaces = [
  {
    id: "1",
    name: "Praha - Centrála",
    address: "Václavské náměstí 1, Praha 1",
    memberCount: 25,
    monthlyBudget: 50000,
    currentExpenses: 32500,
    manager: "Jan Novák",
  },
  {
    id: "2",
    name: "Brno - Pobočka",
    address: "Masarykova 123, Brno",
    memberCount: 15,
    monthlyBudget: 30000,
    currentExpenses: 18200,
    manager: "Marie Svobodová",
  },
  {
    id: "3",
    name: "Ostrava - Sklad",
    address: "Průmyslová 45, Ostrava",
    memberCount: 8,
    monthlyBudget: 20000,
    currentExpenses: 12800,
    manager: "Petr Dvořák",
  },
];

export default function WorkplacesPage() {
  return (
    <MainLayout>
      <div className="flex flex-col gap-4 py-4 px-4 md:gap-6 md:py-6 lg:px-6">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold tracking-tight">Pracoviště</h1>
            <p className="text-muted-foreground">Správa pracovišť a jejich rozpočtů</p>
          </div>
          <Button>
            <Plus className="mr-2 h-4 w-4" />
            Nové pracoviště
          </Button>
        </div>

        {/* Workplaces Grid */}
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {mockWorkplaces.map((workplace) => {
            const budgetUsed = (workplace.currentExpenses / workplace.monthlyBudget) * 100;
            const budgetColor = budgetUsed > 80 ? "text-red-500" : budgetUsed > 60 ? "text-yellow-500" : "text-green-500";

            return (
              <Card key={workplace.id} className="cursor-pointer hover:bg-muted/50 transition-colors">
                <CardHeader>
                  <div className="flex items-start justify-between">
                    <div className="flex items-center gap-2">
                      <MapPin className="h-5 w-5 text-muted-foreground" />
                      <CardTitle className="text-lg">{workplace.name}</CardTitle>
                    </div>
                    <Badge variant="outline">
                      <Users className="mr-1 h-3 w-3" />
                      {workplace.memberCount}
                    </Badge>
                  </div>
                  <CardDescription>{workplace.address}</CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div>
                    <div className="flex items-center justify-between text-sm mb-1">
                      <span className="text-muted-foreground">Rozpočet</span>
                      <span className={budgetColor + " font-medium"}>{budgetUsed.toFixed(0)}%</span>
                    </div>
                    <div className="w-full bg-secondary h-2 rounded-full overflow-hidden">
                      <div
                        className={`h-full ${budgetUsed > 80 ? "bg-red-500" : budgetUsed > 60 ? "bg-yellow-500" : "bg-green-500"}`}
                        style={{ width: `${Math.min(budgetUsed, 100)}%` }}
                      />
                    </div>
                    <div className="flex items-center justify-between text-xs text-muted-foreground mt-1">
                      <span>{workplace.currentExpenses.toLocaleString("cs-CZ")} Kč</span>
                      <span>{workplace.monthlyBudget.toLocaleString("cs-CZ")} Kč</span>
                    </div>
                  </div>

                  <div className="pt-2 border-t">
                    <div className="text-sm">
                      <span className="text-muted-foreground">Manažer: </span>
                      <span className="font-medium">{workplace.manager}</span>
                    </div>
                  </div>

                  <div className="flex gap-2">
                    <Button variant="outline" size="sm" className="flex-1">
                      Detail
                    </Button>
                    <Button variant="ghost" size="sm" className="flex-1">
                      Upravit
                    </Button>
                  </div>
                </CardContent>
              </Card>
            );
          })}
        </div>

        {/* Summary Stats */}
        <div className="grid gap-4 md:grid-cols-3">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Celkem pracovišť</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{mockWorkplaces.length}</div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Celkový rozpočet</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{mockWorkplaces.reduce((sum, w) => sum + w.monthlyBudget, 0).toLocaleString("cs-CZ")} Kč</div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Celkem zaměstnanců</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{mockWorkplaces.reduce((sum, w) => sum + w.memberCount, 0)}</div>
            </CardContent>
          </Card>
        </div>
      </div>
    </MainLayout>
  );
}
