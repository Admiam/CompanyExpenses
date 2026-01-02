import { useState, useEffect } from "react";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";

interface ExpenseFormModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  expense?: {
    id: string;
    description: string;
    amount: number;
    expenseDate: string;
    categoryId: string;
    workplaceId: string;
    currency?: string;
  } | null;
  onSave: (data: any) => void;
}

export function ExpenseFormModal({ open, onOpenChange, expense, onSave }: ExpenseFormModalProps) {
  const [formData, setFormData] = useState({
    description: "",
    amount: "",
    expenseDate: new Date().toISOString().split("T")[0],
    categoryId: "",
    workplaceId: "",
    currency: "CZK",
  });

  // Mock data - nahraď API voláním
  const categories = [
    { id: "1", name: "Pohonné hmoty" },
    { id: "2", name: "Stravování" },
    { id: "3", name: "Kancelářské potřeby" },
  ];

  const workplaces = [
    { id: "1", name: "Praha - Centrála" },
    { id: "2", name: "Brno - Pobočka" },
  ];

  useEffect(() => {
    if (expense) {
      setFormData({
        description: expense.description || "",
        amount: expense.amount.toString(),
        expenseDate: expense.expenseDate,
        categoryId: expense.categoryId,
        workplaceId: expense.workplaceId,
        currency: expense.currency || "CZK",
      });
    } else {
      setFormData({
        description: "",
        amount: "",
        expenseDate: new Date().toISOString().split("T")[0],
        categoryId: "",
        workplaceId: "",
        currency: "CZK",
      });
    }
  }, [expense, open]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSave({
      ...formData,
      amount: parseFloat(formData.amount),
    });
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[525px]">
        <form onSubmit={handleSubmit}>
          <DialogHeader>
            <DialogTitle>{expense ? "Upravit výdaj" : "Nový výdaj"}</DialogTitle>
            <DialogDescription>{expense ? "Upravte údaje výdaje" : "Vytvořte nový výdaj"}</DialogDescription>
          </DialogHeader>

          <div className="grid gap-4 py-4">
            <div className="grid gap-2">
              <Label htmlFor="description">Popis výdaje</Label>
              <Input
                id="description"
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                placeholder="např. Tankování služebního vozu"
                required
              />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="grid gap-2">
                <Label htmlFor="amount">Částka (Kč)</Label>
                <Input
                  id="amount"
                  type="number"
                  step="0.01"
                  value={formData.amount}
                  onChange={(e) => setFormData({ ...formData, amount: e.target.value })}
                  placeholder="0.00"
                  required
                />
              </div>

              <div className="grid gap-2">
                <Label htmlFor="date">Datum</Label>
                <Input
                  id="date"
                  type="date"
                  value={formData.expenseDate}
                  onChange={(e) => setFormData({ ...formData, expenseDate: e.target.value })}
                  required
                />
              </div>
            </div>

            <div className="grid gap-2">
              <Label htmlFor="category">Kategorie</Label>
              <Select value={formData.categoryId} onValueChange={(value) => setFormData({ ...formData, categoryId: value })}>
                <SelectTrigger id="category">
                  <SelectValue placeholder="Vyberte kategorii" />
                </SelectTrigger>
                <SelectContent>
                  {categories.map((cat) => (
                    <SelectItem key={cat.id} value={cat.id}>
                      {cat.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="grid gap-2">
              <Label htmlFor="workplace">Pracoviště</Label>
              <Select value={formData.workplaceId} onValueChange={(value) => setFormData({ ...formData, workplaceId: value })}>
                <SelectTrigger id="workplace">
                  <SelectValue placeholder="Vyberte pracoviště" />
                </SelectTrigger>
                <SelectContent>
                  {workplaces.map((wp) => (
                    <SelectItem key={wp.id} value={wp.id}>
                      {wp.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>

          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              Zrušit
            </Button>
            <Button type="submit">{expense ? "Uložit změny" : "Vytvořit výdaj"}</Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
