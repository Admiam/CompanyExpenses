import { MainLayout } from "@/components/layouts/MainLayout";
import { Button } from "@/components/ui/button";
import { Plus, MapPin, Users, Settings } from "lucide-react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { useState, useEffect } from "react";
import { workplacesApi, workplaceLimitsApi } from "@/lib/proxy/api";
import type { Workplace } from "@/lib/proxy/types";
import { toast } from "sonner";
import { WorkplaceFormModal } from "@/components/modals/WorkplaceFormModal";
import { WorkplaceLimitModal } from "@/components/modals/WorkplaceLimitModal";

interface WorkplaceWithLimits extends Workplace {
  code?: string;
  totalLimit: number;
  currentExpenses: number;
}

export default function WorkplacesPage() {
  const [workplaces, setWorkplaces] = useState<WorkplaceWithLimits[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isLimitModalOpen, setIsLimitModalOpen] = useState(false);
  const [editingWorkplace, setEditingWorkplace] = useState<Workplace | null>(null);
  const [selectedWorkplace, setSelectedWorkplace] = useState<Workplace | null>(null);

  useEffect(() => {
    loadWorkplaces();
  }, []);

  const loadWorkplaces = async () => {
    try {
      setIsLoading(true);
      const data = await workplacesApi.getWorkplaces();

      // Load limits for each workplace
      const workplacesWithLimits = await Promise.all(
        data.map(async (workplace) => {
          try {
            const limits = await workplaceLimitsApi.getWorkplaceLimits(workplace.id);
            const currentDate = new Date();

            // Calculate total limit for current period
            const activeLimits = limits.filter((limit) => {
              const from = new Date(limit.periodFrom);
              const to = new Date(limit.periodTo);
              return currentDate >= from && currentDate <= to && !limit.categoryId; // General limits only
            });

            const totalLimit = activeLimits.reduce((sum, limit) => sum + limit.limitAmount, 0);

            // TODO: Get actual expenses from API
            const currentExpenses = Math.random() * totalLimit * 0.8; // Mock data

            return {
              ...workplace,
              totalLimit,
              currentExpenses,
            };
          } catch (error) {
            console.error(`Failed to load limits for ${workplace.name}:`, error);
            return {
              ...workplace,
              totalLimit: 0,
              currentExpenses: 0,
            };
          }
        })
      );

      setWorkplaces(workplacesWithLimits);
    } catch (error) {
      console.error("Failed to load workplaces:", error);
      toast.error("Failed to load workplaces");
    } finally {
      setIsLoading(false);
    }
  };

  const handleCreate = () => {
    setEditingWorkplace(null);
    setIsModalOpen(true);
  };

  const handleEdit = (workplace: Workplace) => {
    setEditingWorkplace(workplace);
    setIsModalOpen(true);
  };

  const handleSave = async (data: any) => {
    try {
      if (editingWorkplace) {
        const updateData = {
          id: editingWorkplace.id,
          name: data.name,
          code: data.code,
          isActive: data.isActive,
        };
        await workplacesApi.updateWorkplace(editingWorkplace.id, updateData);
        toast.success("Workplace updated");
      } else {
        await workplacesApi.createWorkplace(data);
        toast.success("Workplace created");
      }
      setIsModalOpen(false);
      loadWorkplaces();
    } catch (error: any) {
      console.error("Failed to save workplace:", error);
      toast.error(error?.response?.data?.message || "Failed to save workplace");
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm("Are you sure you want to delete this workplace?")) {
      return;
    }

    try {
      await workplacesApi.deleteWorkplace(id);
      toast.success("Workplace deleted");
      loadWorkplaces();
    } catch (error: any) {
      console.error("Failed to delete workplace:", error);
      toast.error(error?.response?.data?.message || "Failed to delete workplace");
    }
  };

  const handleViewDetail = (workplace: Workplace) => {
    setSelectedWorkplace(workplace);
    setIsLimitModalOpen(true);
  };

  const handleLimitsUpdated = () => {
    loadWorkplaces(); // Reload to refresh budget calculations
  };
  return (
    <MainLayout>
      <div className="flex flex-col gap-4 py-4 px-4 md:gap-6 md:py-6 lg:px-6">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold tracking-tight">Workplaces</h1>
            <p className="text-muted-foreground">Manage workplaces and their budgets</p>
          </div>
          <Button onClick={handleCreate}>
            <Plus className="mr-2 h-4 w-4" />
            New Workplace
          </Button>
        </div>

        {/* Workplaces Grid */}
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {workplaces
            .filter((w) => w.isActive)
            .map((workplace) => {
              const monthlyBudget = workplace.totalLimit || 0;
              const currentExpenses = workplace.currentExpenses || 0;
              const memberCount = 10; // TODO: Get from members API

              const budgetUsed = monthlyBudget > 0 ? (currentExpenses / monthlyBudget) * 100 : 0;
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
                        {memberCount}
                      </Badge>
                    </div>
                    <CardDescription>{workplace.code || "No code"}</CardDescription>
                  </CardHeader>
                  <CardContent className="space-y-4">
                    {monthlyBudget > 0 ? (
                      <div>
                        <div className="flex items-center justify-between text-sm mb-1">
                          <span className="text-muted-foreground">Budget</span>
                          <span className={budgetColor + " font-medium"}>{budgetUsed.toFixed(0)}%</span>
                        </div>
                        <div className="w-full bg-secondary h-2 rounded-full overflow-hidden">
                          <div
                            className={`h-full ${budgetUsed > 80 ? "bg-red-500" : budgetUsed > 60 ? "bg-yellow-500" : "bg-green-500"}`}
                            style={{ width: `${Math.min(budgetUsed, 100)}%` }}
                          />
                        </div>
                        <div className="flex items-center justify-between text-xs text-muted-foreground mt-1">
                          <span>{currentExpenses.toLocaleString()} CZK</span>
                          <span>{monthlyBudget.toLocaleString()} CZK</span>
                        </div>
                      </div>
                    ) : (
                      <div className="text-center py-4 text-sm text-muted-foreground">No budget limit set</div>
                    )}

                    <div className="flex gap-2">
                      <Button variant="outline" size="sm" className="flex-1" onClick={() => handleViewDetail(workplace)}>
                        <Settings className="mr-2 h-4 w-4" />
                        Limits
                      </Button>
                      <Button variant="ghost" size="sm" className="flex-1" onClick={() => handleEdit(workplace)}>
                        Edit
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
              <CardTitle className="text-sm font-medium">Total Workplaces</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{workplaces.length}</div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Active Workplaces</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{workplaces.filter((w) => w.isActive).length}</div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Inactive Workplaces</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{workplaces.filter((w) => !w.isActive).length}</div>
            </CardContent>
          </Card>
        </div>

        <WorkplaceFormModal open={isModalOpen} onOpenChange={setIsModalOpen} workplace={editingWorkplace} onSave={handleSave} />

        {selectedWorkplace && (
          <WorkplaceLimitModal
            open={isLimitModalOpen}
            onOpenChange={setIsLimitModalOpen}
            workplaceId={selectedWorkplace.id}
            workplaceName={selectedWorkplace.name}
            onLimitsUpdated={handleLimitsUpdated}
          />
        )}
      </div>
    </MainLayout>
  );
}
