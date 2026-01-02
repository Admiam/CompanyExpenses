import { useState, useEffect } from "react";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Switch } from "@/components/ui/switch";

interface WorkplaceFormModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  workplace?: {
    id: string;
    name: string;
    address: string;
    description?: string;
    monthlyBudget: number;
    isActive: boolean;
  } | null;
  onSave: (data: any) => void;
}

export function WorkplaceFormModal({ open, onOpenChange, workplace, onSave }: WorkplaceFormModalProps) {
  const [formData, setFormData] = useState({
    name: "",
    address: "",
    description: "",
    monthlyBudget: "",
    isActive: true,
  });

  useEffect(() => {
    if (workplace) {
      setFormData({
        name: workplace.name,
        address: workplace.address,
        description: workplace.description || "",
        monthlyBudget: workplace.monthlyBudget.toString(),
        isActive: workplace.isActive,
      });
    } else {
      setFormData({
        name: "",
        address: "",
        description: "",
        monthlyBudget: "",
        isActive: true,
      });
    }
  }, [workplace, open]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSave({
      ...formData,
      monthlyBudget: parseFloat(formData.monthlyBudget),
    });
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[525px]">
        <form onSubmit={handleSubmit}>
          <DialogHeader>
            <DialogTitle>{workplace ? "Upravit pracoviště" : "Nové pracoviště"}</DialogTitle>
            <DialogDescription>{workplace ? "Upravte údaje pracoviště" : "Vytvořte nové pracoviště"}</DialogDescription>
          </DialogHeader>

          <div className="grid gap-4 py-4">
            <div className="grid gap-2">
              <Label htmlFor="name">Název pracoviště</Label>
              <Input
                id="name"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                placeholder="např. Praha - Centrála"
                required
              />
            </div>

            <div className="grid gap-2">
              <Label htmlFor="address">Adresa</Label>
              <Input
                id="address"
                value={formData.address}
                onChange={(e) => setFormData({ ...formData, address: e.target.value })}
                placeholder="Ulice 123, Město"
                required
              />
            </div>

            <div className="grid gap-2">
              <Label htmlFor="description">Popis (volitelné)</Label>
              <Textarea
                id="description"
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                placeholder="Doplňující informace o pracovišti..."
                rows={3}
              />
            </div>

            <div className="grid gap-2">
              <Label htmlFor="budget">Měsíční rozpočet (Kč)</Label>
              <Input
                id="budget"
                type="number"
                step="1"
                value={formData.monthlyBudget}
                onChange={(e) => setFormData({ ...formData, monthlyBudget: e.target.value })}
                placeholder="50000"
                required
              />
            </div>

            <div className="flex items-center justify-between">
              <Label htmlFor="isActive">Aktivní pracoviště</Label>
              <Switch id="isActive" checked={formData.isActive} onCheckedChange={(checked) => setFormData({ ...formData, isActive: checked })} />
            </div>
          </div>

          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              Zrušit
            </Button>
            <Button type="submit">{workplace ? "Uložit změny" : "Vytvořit pracoviště"}</Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
