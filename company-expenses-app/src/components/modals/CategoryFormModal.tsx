import { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";

interface CategoryFormModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  category?: {
    id: string;
    name: string;
    color: string;
    isActive: boolean;
  } | null;
  onSave: (data: any) => void;
}

export function CategoryFormModal({ open, onOpenChange, category, onSave }: CategoryFormModalProps) {
  const { t } = useTranslation();
  const [formData, setFormData] = useState({
    name: "",
    color: "#3b82f6",
    isActive: true,
  });

  useEffect(() => {
    if (category) {
      setFormData({
        name: category.name,
        color: category.color,
        isActive: category.isActive,
      });
    } else {
      setFormData({
        name: "",
        color: "#3b82f6",
        isActive: true,
      });
    }
  }, [category, open]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSave(formData);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[425px]">
        <form onSubmit={handleSubmit}>
          <DialogHeader>
            <DialogTitle>{category ? t("categories.editCategory") : t("categories.newCategory")}</DialogTitle>
            <DialogDescription>{category ? t("categories.editDescription") : t("categories.newDescription")}</DialogDescription>
          </DialogHeader>

          <div className="grid gap-4 py-4">
            <div className="grid gap-2">
              <Label htmlFor="name">{t("categories.name")}</Label>
              <Input
                id="name"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                placeholder={t("categories.namePlaceholder")}
                required
              />
            </div>

            <div className="grid gap-2">
              <Label htmlFor="color">{t("categories.color")}</Label>
              <div className="flex gap-2 items-center">
                <Input
                  id="color"
                  type="color"
                  value={formData.color}
                  onChange={(e) => setFormData({ ...formData, color: e.target.value })}
                  className="w-20 h-10"
                />
                <Input
                  type="text"
                  value={formData.color}
                  onChange={(e) => setFormData({ ...formData, color: e.target.value })}
                  placeholder="#3b82f6"
                  className="flex-1"
                />
              </div>
            </div>

            <div className="flex items-center justify-between">
              <Label htmlFor="isActive">{t("common.active")}</Label>
              <Switch id="isActive" checked={formData.isActive} onCheckedChange={(checked) => setFormData({ ...formData, isActive: checked })} />
            </div>
          </div>

          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              {t("common.cancel")}
            </Button>
            <Button type="submit">{category ? t("common.save") : t("common.create")}</Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
