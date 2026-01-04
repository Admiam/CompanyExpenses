import { useState, useEffect } from "react";
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Loader2, Plus, Trash2, Calendar } from "lucide-react";
import { workplaceLimitsApi, categoriesApi } from "@/lib/proxy/api";
import type { WorkplaceLimit, ExpenseCategory } from "@/lib/proxy/types";
import { toast } from "sonner";

interface WorkplaceLimitModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  workplaceId: string;
  workplaceName: string;
  onLimitsUpdated?: () => void;
}

export function WorkplaceLimitModal({ open, onOpenChange, workplaceId, workplaceName, onLimitsUpdated }: WorkplaceLimitModalProps) {
  const [limits, setLimits] = useState<WorkplaceLimit[]>([]);
  const [categories, setCategories] = useState<ExpenseCategory[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isAddingNew, setIsAddingNew] = useState(false);
  const [newLimit, setNewLimit] = useState({
    categoryId: "",
    periodFrom: "",
    periodTo: "",
    limitAmount: "",
    currency: "CZK",
  });

  useEffect(() => {
    if (open) {
      loadData();
    }
  }, [open, workplaceId]);

  const loadData = async () => {
    try {
      setIsLoading(true);
      const [limitsData, categoriesData] = await Promise.all([workplaceLimitsApi.getWorkplaceLimits(workplaceId), categoriesApi.getCategories()]);
      setLimits(limitsData);
      setCategories(categoriesData.filter((c) => c.isActive));
    } catch (error) {
      console.error("Failed to load limits:", error);
      toast.error("Failed to load limits");
    } finally {
      setIsLoading(false);
    }
  };

  const handleAddLimit = async () => {
    if (!newLimit.periodFrom || !newLimit.periodTo || !newLimit.limitAmount) {
      toast.error("Please fill all required fields");
      return;
    }

    try {
      await workplaceLimitsApi.createLimit({
        workplaceId,
        categoryId: newLimit.categoryId || undefined,
        periodFrom: newLimit.periodFrom,
        periodTo: newLimit.periodTo,
        limitAmount: parseFloat(newLimit.limitAmount),
        currency: newLimit.currency,
      });
      toast.success("Limit added");
      setIsAddingNew(false);
      setNewLimit({
        categoryId: "",
        periodFrom: "",
        periodTo: "",
        limitAmount: "",
        currency: "CZK",
      });
      loadData();
      onLimitsUpdated?.();
    } catch (error: any) {
      console.error("Failed to add limit:", error);
      toast.error(error?.response?.data?.message || "Failed to add limit");
    }
  };

  const handleDeleteLimit = async (id: string) => {
    if (!confirm("Are you sure you want to delete this limit?")) {
      return;
    }

    try {
      await workplaceLimitsApi.deleteLimit(id);
      toast.success("Limit deleted");
      loadData();
      onLimitsUpdated?.();
    } catch (error: any) {
      console.error("Failed to delete limit:", error);
      toast.error(error?.response?.data?.message || "Failed to delete limit");
    }
  };

  const getCategoryName = (categoryId?: string) => {
    if (!categoryId) return "General (All categories)";
    const category = categories.find((c) => c.id === categoryId);
    return category?.name || "Unknown";
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[800px] max-h-[80vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Budget Limits - {workplaceName}</DialogTitle>
          <DialogDescription>Manage budget limits for this workplace. You can set general limits or category-specific limits.</DialogDescription>
        </DialogHeader>

        <div className="space-y-4">
          {isLoading ? (
            <div className="flex justify-center py-8">
              <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
            </div>
          ) : (
            <>
              <div className="flex justify-between items-center">
                <h3 className="text-lg font-medium">Current Limits</h3>
                <Button size="sm" onClick={() => setIsAddingNew(!isAddingNew)}>
                  <Plus className="mr-2 h-4 w-4" />
                  Add Limit
                </Button>
              </div>

              {isAddingNew && (
                <div className="border rounded-lg p-4 space-y-4 bg-muted/50">
                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-2">
                      <Label>Category (optional)</Label>
                      <Select
                        value={newLimit.categoryId || "general"}
                        onValueChange={(value) => setNewLimit({ ...newLimit, categoryId: value === "general" ? "" : value })}
                      >
                        <SelectTrigger>
                          <SelectValue placeholder="General (All categories)" />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="general">General (All categories)</SelectItem>
                          {categories.map((category) => (
                            <SelectItem key={category.id} value={category.id}>
                              {category.name}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </div>

                    <div className="space-y-2">
                      <Label>Amount *</Label>
                      <Input
                        type="number"
                        step="0.01"
                        value={newLimit.limitAmount}
                        onChange={(e) => setNewLimit({ ...newLimit, limitAmount: e.target.value })}
                        placeholder="50000"
                        required
                      />
                    </div>
                  </div>

                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-2">
                      <Label>Period From *</Label>
                      <Input type="date" value={newLimit.periodFrom} onChange={(e) => setNewLimit({ ...newLimit, periodFrom: e.target.value })} required />
                    </div>

                    <div className="space-y-2">
                      <Label>Period To *</Label>
                      <Input type="date" value={newLimit.periodTo} onChange={(e) => setNewLimit({ ...newLimit, periodTo: e.target.value })} required />
                    </div>
                  </div>

                  <div className="flex justify-end gap-2">
                    <Button variant="outline" size="sm" onClick={() => setIsAddingNew(false)}>
                      Cancel
                    </Button>
                    <Button size="sm" onClick={handleAddLimit}>
                      Add Limit
                    </Button>
                  </div>
                </div>
              )}

              {limits.length === 0 ? (
                <div className="text-center py-8 text-muted-foreground">No limits set. Add your first limit to start tracking budgets.</div>
              ) : (
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Category</TableHead>
                      <TableHead>Amount</TableHead>
                      <TableHead>Period</TableHead>
                      <TableHead className="text-right">Actions</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {limits.map((limit) => (
                      <TableRow key={limit.id}>
                        <TableCell className="font-medium">{getCategoryName(limit.categoryId)}</TableCell>
                        <TableCell>
                          {limit.limitAmount.toLocaleString()} {limit.currency}
                        </TableCell>
                        <TableCell>
                          <div className="flex items-center gap-1 text-sm text-muted-foreground">
                            <Calendar className="h-3 w-3" />
                            {new Date(limit.periodFrom).toLocaleDateString()} - {new Date(limit.periodTo).toLocaleDateString()}
                          </div>
                        </TableCell>
                        <TableCell className="text-right">
                          <Button variant="ghost" size="sm" onClick={() => handleDeleteLimit(limit.id)}>
                            <Trash2 className="h-4 w-4 text-destructive" />
                          </Button>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              )}
            </>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
}
