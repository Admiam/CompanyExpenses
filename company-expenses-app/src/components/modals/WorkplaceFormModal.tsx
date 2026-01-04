import { useState, useEffect } from "react";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { Loader2 } from "lucide-react";

interface WorkplaceFormData {
  name: string;
  code: string;
  isActive: boolean;
}

interface WorkplaceFormModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  workplace?: {
    id: string;
    name: string;
    code?: string;
    isActive: boolean;
  } | null;
  onSave: (data: WorkplaceFormData) => Promise<void>;
}

export function WorkplaceFormModal({ open, onOpenChange, workplace, onSave }: WorkplaceFormModalProps) {
  const [formData, setFormData] = useState<WorkplaceFormData>({
    name: "",
    code: "",
    isActive: true,
  });
  const [isSaving, setIsSaving] = useState(false);

  useEffect(() => {
    if (workplace) {
      setFormData({
        name: workplace.name || "",
        code: workplace.code || "",
        isActive: workplace.isActive ?? true,
      });
    } else {
      setFormData({
        name: "",
        code: "",
        isActive: true,
      });
    }
  }, [workplace, open]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSaving(true);
    try {
      await onSave(formData);
      onOpenChange(false);
    } catch (error) {
      console.error("Failed to save workplace:", error);
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <form onSubmit={handleSubmit}>
          <DialogHeader>
            <DialogTitle>{workplace ? "Edit Workplace" : "Create New Workplace"}</DialogTitle>
            <DialogDescription>{workplace ? "Update workplace information" : "Add a new workplace to your organization"}</DialogDescription>
          </DialogHeader>

          <div className="grid gap-4 py-4">
            <div className="grid gap-2">
              <Label htmlFor="name">Name *</Label>
              <Input
                id="name"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                placeholder="e.g., Prague - Headquarters"
                required
              />
            </div>

            <div className="grid gap-2">
              <Label htmlFor="code">Code</Label>
              <Input
                id="code"
                value={formData.code}
                onChange={(e) => setFormData({ ...formData, code: e.target.value })}
                placeholder="e.g., PRG-HQ (optional)"
              />
            </div>

            <div className="flex items-center justify-between">
              <Label htmlFor="isActive">Active</Label>
              <Switch id="isActive" checked={formData.isActive} onCheckedChange={(checked) => setFormData({ ...formData, isActive: checked })} />
            </div>
          </div>

          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)} disabled={isSaving}>
              Cancel
            </Button>
            <Button type="submit" disabled={isSaving}>
              {isSaving && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              {workplace ? "Update" : "Create"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
