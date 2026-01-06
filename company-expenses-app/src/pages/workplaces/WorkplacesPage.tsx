import { MainLayout } from "@/components/layouts/MainLayout";
import { Button } from "@/components/ui/button";
import { Plus, MapPin, Users, Settings, TrendingUp, Calendar, CheckCircle, Trash2 } from "lucide-react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { useState, useEffect } from "react";
import { workplacesApi, workplaceLimitsApi, expensesApi } from "@/lib/proxy/api";
import type { Workplace } from "@/lib/proxy/types";
import { toast } from "sonner";
import { WorkplaceFormModal } from "@/components/modals/WorkplaceFormModal";
import { WorkplaceLimitModal } from "@/components/modals/WorkplaceLimitModal";
import { WorkplaceDeleteModal } from "@/components/modals/WorkplaceDeleteModal";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";

interface WorkplaceWithStats extends Workplace {
  code?: string;
  totalLimit: number;
  currentExpenses: number;
  memberCount: number;
  activeExpensesCount: number;
}

export default function WorkplacesPage() {
  const [workplaces, setWorkplaces] = useState<WorkplaceWithStats[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isLimitModalOpen, setIsLimitModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [editingWorkplace, setEditingWorkplace] = useState<Workplace | null>(null);
  const [selectedWorkplace, setSelectedWorkplace] = useState<Workplace | null>(null);
  const [deletingWorkplace, setDeletingWorkplace] = useState<{ id: string; name: string } | null>(null);

  useEffect(() => {
    loadWorkplaces();
  }, []);

  const loadWorkplaces = async () => {
    try {
      setIsLoading(true);
      const data = await workplacesApi.getWorkplaces();

      // Load limits and expenses for each workplace
      const workplacesWithStats = await Promise.all(
        data.map(async (workplace) => {
          try {
            // Get limits
            const limits = await workplaceLimitsApi.getWorkplaceLimits(workplace.id);
            const currentDate = new Date();

            // Calculate total limit for current period (ALL categories combined)
            const activeLimits = limits.filter((limit) => {
              const from = new Date(limit.periodFrom);
              const to = new Date(limit.periodTo);
              return currentDate >= from && currentDate <= to;
            });

            // Sum all category limits
            const totalLimit = activeLimits.reduce((sum, limit) => sum + limit.limitAmount, 0);

            // Get actual expenses for this workplace
            const expenses = await expensesApi.getExpenses();
            const workplaceExpenses = expenses.filter((e) => e.workplaceId === workplace.id);

            // Calculate current expenses (for active limits period, all categories)
            const currentExpenses = workplaceExpenses
              .filter((expense) => {
                const expenseDate = new Date(expense.expenseDate);
                return activeLimits.some((limit) => {
                  const from = new Date(limit.periodFrom);
                  const to = new Date(limit.periodTo);
                  return expenseDate >= from && expenseDate <= to;
                });
              })
              .reduce((sum, expense) => sum + expense.amount, 0);

            // Count active (pending) expenses
            const activeExpensesCount = workplaceExpenses.filter((e) => e.status === "Pending").length;

            return {
              ...workplace,
              totalLimit,
              currentExpenses,
              memberCount: workplace.members?.length || 0,
              activeExpensesCount,
            };
          } catch (error) {
            console.error(`Failed to load stats for ${workplace.name}:`, error);
            return {
              ...workplace,
              totalLimit: 0,
              currentExpenses: 0,
              memberCount: workplace.members?.length || 0,
              activeExpensesCount: 0,
            };
          }
        })
      );

      setWorkplaces(workplacesWithStats);
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
          isActive: data.isActive ?? true,
        };
        await workplacesApi.updateWorkplace(editingWorkplace.id, updateData);
        toast.success("Workplace updated");
      } else {
        await workplacesApi.createWorkplace({
          name: data.name,
          code: data.code,
          isActive: true,
        });
        toast.success("Workplace created");
      }
      setIsModalOpen(false);
      loadWorkplaces();
    } catch (error: any) {
      console.error("Failed to save workplace:", error);
      toast.error(error?.response?.data?.message || "Failed to save workplace");
    }
  };

  const handleDelete = (id: string) => {
    const workplace = workplaces.find((w) => w.id === id);
    if (!workplace) return;

    setDeletingWorkplace({ id: workplace.id, name: workplace.name });
    setIsDeleteModalOpen(true);
  };

  const handleConfirmDelete = async () => {
    if (!deletingWorkplace) return;

    try {
      await workplacesApi.deleteWorkplace(deletingWorkplace.id);
      toast.success("Workplace deleted");
      setIsDeleteModalOpen(false);
      setDeletingWorkplace(null);
      loadWorkplaces();
    } catch (error: any) {
      console.error("Failed to delete workplace:", error);
      toast.error(error?.response?.data?.message || "Failed to delete workplace");
      setIsDeleteModalOpen(false);
      setDeletingWorkplace(null);
    }
  };

  const handleViewDetail = (workplace: Workplace) => {
    setSelectedWorkplace(workplace);
    setIsLimitModalOpen(true);
  };

  const handleLimitsUpdated = () => {
    loadWorkplaces(); // Reload to refresh budget calculations
  };

  const handleActivate = async (id: string) => {
    try {
      const workplace = workplaces.find((w) => w.id === id);
      if (!workplace) return;

      await workplacesApi.updateWorkplace(id, {
        id: workplace.id,
        name: workplace.name,
        code: workplace.code,
        isActive: true,
      });
      toast.success("Workplace activated");
      loadWorkplaces();
    } catch (error: any) {
      console.error("Failed to activate workplace:", error);
      toast.error(error?.response?.data?.message || "Failed to activate workplace");
    }
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

        {/* Workplaces Grid */}
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {isLoading ? (
            <div className="col-span-full text-center py-8 text-muted-foreground">Loading workplaces...</div>
          ) : workplaces.filter((w) => w.isActive).length === 0 ? (
            <div className="col-span-full text-center py-8 text-muted-foreground">No active workplaces</div>
          ) : (
            workplaces
              .filter((w) => w.isActive)
              .map((workplace) => {
                const monthlyBudget = workplace.totalLimit || 0;
                const currentExpenses = workplace.currentExpenses || 0;
                const memberCount = workplace.memberCount;
                const activeExpensesCount = workplace.activeExpensesCount;

                const budgetUsed = monthlyBudget > 0 ? (currentExpenses / monthlyBudget) * 100 : 0;
                const budgetColor = budgetUsed > 90 ? "text-red-600" : budgetUsed > 75 ? "text-orange-500" : "text-green-600";
                const budgetBg = budgetUsed > 90 ? "bg-red-500" : budgetUsed > 75 ? "bg-orange-500" : "bg-green-500";

                return (
                  <Card key={workplace.id} className="hover:shadow-md transition-shadow">
                    <CardHeader className="pb-3">
                      <div className="flex items-start justify-between gap-2">
                        <div className="flex items-center gap-2 flex-1 min-w-0">
                          <MapPin className="h-5 w-5 text-muted-foreground shrink-0" />
                          <div className="min-w-0 flex-1">
                            <CardTitle className="text-lg truncate">{workplace.name}</CardTitle>
                            <CardDescription className="text-xs mt-1">{workplace.code || "No code assigned"}</CardDescription>
                          </div>
                        </div>
                        <Badge variant="secondary" className="shrink-0">
                          <Users className="mr-1 h-3 w-3" />
                          {memberCount}
                        </Badge>
                      </div>
                    </CardHeader>
                    <CardContent className="space-y-4">
                      {/* Budget Info */}
                      {monthlyBudget > 0 ? (
                        <div className="space-y-2">
                          <div className="flex items-center justify-between text-sm">
                            <span className="text-muted-foreground">Current Period Budget</span>
                            <span className={`${budgetColor} font-semibold`}>{budgetUsed.toFixed(1)}%</span>
                          </div>
                          <div className="w-full bg-secondary h-2.5 rounded-full overflow-hidden">
                            <div className={`h-full ${budgetBg} transition-all`} style={{ width: `${Math.min(budgetUsed, 100)}%` }} />
                          </div>
                          <div className="flex items-center justify-between text-xs">
                            <span className="text-muted-foreground">{currentExpenses.toLocaleString("cs-CZ")} CZK</span>
                            <span className="text-muted-foreground">{monthlyBudget.toLocaleString("cs-CZ")} CZK</span>
                          </div>
                          <div className="text-xs text-muted-foreground">Remaining: {(monthlyBudget - currentExpenses).toLocaleString("cs-CZ")} CZK</div>
                        </div>
                      ) : (
                        <div className="text-center py-3 text-sm text-muted-foreground bg-muted/30 rounded-md">No budget limit set for current period</div>
                      )}

                      {/* Stats Row */}
                      <div className="flex items-center gap-4 text-xs text-muted-foreground border-t pt-3">
                        <div className="flex items-center gap-1">
                          <TrendingUp className="h-3.5 w-3.5" />
                          <span>{activeExpensesCount} pending</span>
                        </div>
                        <div className="flex items-center gap-1">
                          <Calendar className="h-3.5 w-3.5" />
                          <span>Since {new Date(workplace.createdAt).toLocaleDateString("cs-CZ")}</span>
                        </div>
                      </div>

                      {/* Actions */}
                      <div className="flex gap-2 pt-2">
                        <Button variant="outline" size="sm" className="flex-1" onClick={() => handleViewDetail(workplace)}>
                          <Settings className="mr-2 h-4 w-4" />
                          Manage Limits
                        </Button>
                        <Button variant="ghost" size="sm" onClick={() => handleEdit(workplace)}>
                          Edit
                        </Button>
                        <Button variant="ghost" size="sm" onClick={() => handleDelete(workplace.id)} className="text-destructive hover:text-destructive">
                          Delete
                        </Button>
                      </div>
                    </CardContent>
                  </Card>
                );
              })
          )}
        </div>

        {/* Inactive Workplaces Table */}
        {workplaces.filter((w) => !w.isActive).length > 0 && (
          <div className="space-y-4">
            <div>
              <h2 className="text-2xl font-bold tracking-tight">Inactive Workplaces</h2>
              <p className="text-muted-foreground">Deactivated workplaces that can be restored or deleted</p>
            </div>
            <Card>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Name</TableHead>
                    <TableHead>Code</TableHead>
                    <TableHead>Members</TableHead>
                    <TableHead>Created</TableHead>
                    <TableHead className="text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {workplaces
                    .filter((w) => !w.isActive)
                    .map((workplace) => (
                      <TableRow key={workplace.id}>
                        <TableCell className="font-medium">{workplace.name}</TableCell>
                        <TableCell>{workplace.code || "-"}</TableCell>
                        <TableCell>
                          <Badge variant="secondary" className="shrink-0">
                            <Users className="mr-1 h-3 w-3" />
                            {workplace.memberCount}
                          </Badge>
                        </TableCell>
                        <TableCell>{new Date(workplace.createdAt).toLocaleDateString("cs-CZ")}</TableCell>
                        <TableCell className="text-right">
                          <div className="flex justify-end gap-2">
                            <Button variant="outline" size="sm" onClick={() => handleActivate(workplace.id)}>
                              <CheckCircle className="mr-2 h-4 w-4" />
                              Activate
                            </Button>
                            <Button variant="destructive" size="sm" onClick={() => handleDelete(workplace.id)}>
                              <Trash2 className="mr-2 h-4 w-4" />
                              Delete
                            </Button>
                          </div>
                        </TableCell>
                      </TableRow>
                    ))}
                </TableBody>
              </Table>
            </Card>
          </div>
        )}

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

        <WorkplaceDeleteModal
          open={isDeleteModalOpen}
          onOpenChange={setIsDeleteModalOpen}
          workplaceId={deletingWorkplace?.id || null}
          workplaceName={deletingWorkplace?.name || ""}
          onConfirm={handleConfirmDelete}
        />
      </div>
    </MainLayout>
  );
}
