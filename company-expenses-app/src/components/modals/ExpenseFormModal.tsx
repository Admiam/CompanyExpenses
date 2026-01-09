import { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Upload, X, Image as ImageIcon } from "lucide-react";
import { categoriesApi, workplacesApi, workplaceMembersApi } from "@/lib/proxy/api";
import type { ExpenseCategory, Workplace } from "@/lib/proxy/types";
import { useAuth } from "@/auth/useAuth";
import { isAdmin } from "@/utils/roles";
import { FILE_CONFIG } from "@/lib/app-config";

interface ExpenseAttachment {
  id: string;
  originalFileName: string;
  dataType: string;
  fileSize: number;
  uploadedAt: string;
}

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
  const { t } = useTranslation();
  const { user } = useAuth();
  const userIsAdmin = isAdmin(user?.role);

  const [formData, setFormData] = useState({
    description: "",
    amount: "",
    expenseDate: new Date().toISOString().split("T")[0],
    categoryId: "",
    workplaceId: "",
    currency: "CZK",
  });

  const [selectedFiles, setSelectedFiles] = useState<File[]>([]);
  const [filePreviews, setFilePreviews] = useState<{ file: File; preview: string }[]>([]);
  const [existingAttachments, setExistingAttachments] = useState<ExpenseAttachment[]>([]);
  const [categories, setCategories] = useState<ExpenseCategory[]>([]);
  const [workplaces, setWorkplaces] = useState<Workplace[]>([]);

  useEffect(() => {
    const loadData = async () => {
      try {
        const categoriesData = await categoriesApi.getCategories();
        setCategories(categoriesData.filter((c) => c.isActive));

        if (userIsAdmin) {
          const workplacesData = await workplacesApi.getWorkplaces();
          setWorkplaces(workplacesData.filter((w) => w.isActive));
        } else if (user?.id) {
          // API returns Workplace[] directly for this user
          const userWorkplaces = await workplaceMembersApi.getUserWorkplaces(user.id);

          if (userWorkplaces && userWorkplaces.length > 0) {
            const activeWorkplaces = userWorkplaces.filter((w) => w.isActive);
            setWorkplaces(activeWorkplaces);

            if (activeWorkplaces.length > 0 && !expense) {
              setFormData((prev) => ({ ...prev, workplaceId: activeWorkplaces[0].id }));
            }
          } else {
            setWorkplaces([]);
          }
        }
      } catch (error) {
        console.error("Failed to load categories and workplaces:", error);
      }
    };

    if (open && user) {
      loadData();
    }
  }, [open, userIsAdmin, user?.id, expense, user]);

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
      setSelectedFiles([]);
      setFilePreviews([]);
      setExistingAttachments([]);
    }
  }, [expense, open]);

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(e.target.files || []);

    const validFiles: File[] = [];

    for (const file of files) {
      if (file.size > FILE_CONFIG.maxFileSizeBytes) {
        alert(`${file.name}: Soubor je příliš velký. Maximální velikost je ${FILE_CONFIG.maxFileSizeMB} MB.`);
        continue;
      }

      if (!FILE_CONFIG.allowedImageTypes.includes(file.type)) {
        alert(`${file.name}: Nepodporovaný typ souboru. Povolené jsou pouze obrázky (JPEG, PNG, GIF).`);
        continue;
      }

      validFiles.push(file);
    }

    if (validFiles.length === 0) return;

    setSelectedFiles((prev) => [...prev, ...validFiles]);

    validFiles.forEach((file) => {
      if (file.type.startsWith("image/")) {
        const reader = new FileReader();
        reader.onloadend = () => {
          setFilePreviews((prev) => [...prev, { file, preview: reader.result as string }]);
        };
        reader.readAsDataURL(file);
      }
    });

    e.target.value = "";
  };

  const handleRemoveFile = (fileToRemove: File) => {
    setSelectedFiles((prev) => prev.filter((f) => f !== fileToRemove));
    setFilePreviews((prev) => prev.filter((p) => p.file !== fileToRemove));
  };

  const formatFileSize = (bytes: number): string => {
    if (bytes < 1024) return bytes + " B";
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + " KB";
    return (bytes / (1024 * 1024)).toFixed(1) + " MB";
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!formData.description.trim()) {
      alert("Popis výdaje je povinný");
      return;
    }
    if (!formData.amount || parseFloat(formData.amount) <= 0) {
      alert("Částka musí být větší než 0");
      return;
    }
    if (!formData.categoryId) {
      alert("Kategorie je povinná");
      return;
    }

    let workplaceId = formData.workplaceId;
    if (!userIsAdmin) {
      if (workplaces.length > 0) {
        workplaceId = workplaces[0].id;
      } else {
        alert("Nemáte přiřazené žádné pracoviště. Kontaktujte administrátora.");
        return;
      }
    } else {
      if (!workplaceId) {
        alert("Pracoviště je povinné");
        return;
      }
    }

    const attachments = await Promise.all(
      selectedFiles.map(async (file) => {
        const base64 = await fileToBase64(file);
        return {
          originalFileName: file.name,
          dataType: file.type,
          base64Data: base64.split(",")[1], // Remove data:image/jpeg;base64, prefix
          originalFileSize: file.size,
        };
      })
    );

    onSave({
      description: formData.description,
      amount: parseFloat(formData.amount),
      currency: formData.currency,
      expenseDate: formData.expenseDate,
      categoryId: formData.categoryId,
      workplaceId: workplaceId,
      attachments,
    });
  };

  const fileToBase64 = (file: File): Promise<string> => {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.readAsDataURL(file);
      reader.onload = () => resolve(reader.result as string);
      reader.onerror = (error) => reject(error);
    });
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[525px] max-h-[90vh] overflow-hidden flex flex-col p-0">
        <form onSubmit={handleSubmit} className="flex flex-col h-full overflow-hidden">
          <DialogHeader className="px-6 pt-6 flex-shrink-0">
            <DialogTitle>{expense ? t("expenses.editExpense") : t("expenses.newExpense")}</DialogTitle>
            <DialogDescription>{expense ? t("expenses.editExpense") : t("expenses.newExpense")}</DialogDescription>
          </DialogHeader>

          <div className="grid gap-4 py-4 px-6 overflow-y-auto flex-1">
            <div className="grid gap-2">
              <Label htmlFor="description">
                {t("common.description")} <span className="text-red-500">*</span>
              </Label>
              <Input
                id="description"
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                placeholder={t("common.description")}
                required
              />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="grid gap-2">
                <Label htmlFor="amount">
                  {t("common.amount")} ({t("common.currency")}) <span className="text-red-500">*</span>
                </Label>
                <Input
                  id="amount"
                  type="number"
                  step="0.01"
                  min="0.01"
                  value={formData.amount}
                  onChange={(e) => setFormData({ ...formData, amount: e.target.value })}
                  placeholder="0.00"
                  required
                />
              </div>

              <div className="grid gap-2">
                <Label htmlFor="date">{t("expenses.expenseDate")}</Label>
                <Input
                  id="date"
                  type="date"
                  value={formData.expenseDate}
                  onChange={(e) => setFormData({ ...formData, expenseDate: e.target.value })}
                  required
                />
              </div>
            </div>

            {userIsAdmin ? (
              <div className="grid grid-cols-2 gap-4">
                <div className="grid gap-2">
                  <Label htmlFor="category">
                    {t("expenses.category")} <span className="text-red-500">*</span>
                  </Label>
                  <Select value={formData.categoryId} onValueChange={(value) => setFormData({ ...formData, categoryId: value })} required>
                    <SelectTrigger id="category">
                      <SelectValue placeholder={t("invitations.selectWorkplace")} />
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
                  <Label htmlFor="workplace">
                    {t("expenses.workplace")} <span className="text-red-500">*</span>
                  </Label>
                  <Select value={formData.workplaceId} onValueChange={(value) => setFormData({ ...formData, workplaceId: value })} required>
                    <SelectTrigger id="workplace">
                      <SelectValue placeholder={t("invitations.selectWorkplace")} />
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
            ) : (
              <div className="grid gap-2">
                <Label htmlFor="category">
                  {t("expenses.category")} <span className="text-red-500">*</span>
                </Label>
                <Select value={formData.categoryId} onValueChange={(value) => setFormData({ ...formData, categoryId: value })} required>
                  <SelectTrigger id="category">
                    <SelectValue placeholder={t("invitations.selectWorkplace")} />
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
            )}

            <div className="grid gap-2">
              <Label htmlFor="receipt">{t("expenses.attachments")}</Label>

              <div className="space-y-3">
                {existingAttachments.length > 0 && (
                  <div className="space-y-2">
                    {existingAttachments.map((attachment) => (
                      <div key={attachment.id} className="border border-gray-300 rounded-lg p-3 flex items-center gap-3">
                        <div className="flex-shrink-0">
                          <ImageIcon className="h-5 w-5 text-gray-400" />
                        </div>
                        <div className="flex-1 min-w-0">
                          <p className="text-sm font-medium text-gray-900 truncate">{attachment.originalFileName}</p>
                          <p className="text-xs text-gray-500">{formatFileSize(attachment.fileSize)}</p>
                        </div>
                      </div>
                    ))}
                  </div>
                )}

                {filePreviews.length > 0 && (
                  <div className="grid grid-cols-2 gap-2">
                    {filePreviews.map((item, index) => (
                      <div key={index} className="border border-gray-300 rounded-lg p-2 relative group">
                        <div className="aspect-square relative">
                          <img src={item.preview} alt={item.file.name} className="w-full h-full object-cover rounded" />
                          <Button
                            type="button"
                            variant="destructive"
                            size="sm"
                            onClick={() => handleRemoveFile(item.file)}
                            className="absolute top-1 right-1 h-6 w-6 p-0 opacity-0 group-hover:opacity-100 transition-opacity"
                          >
                            <X className="h-4 w-4" />
                          </Button>
                        </div>
                        <p className="text-xs text-gray-600 truncate mt-1">{item.file.name}</p>
                        <p className="text-xs text-gray-400">{formatFileSize(item.file.size)}</p>
                      </div>
                    ))}
                  </div>
                )}

                <div className="border-2 border-dashed border-gray-300 rounded-lg p-4 hover:border-gray-400 transition-colors">
                  <label htmlFor="receipt" className="cursor-pointer flex flex-col items-center gap-2">
                    <Upload className="h-6 w-6 text-gray-400" />
                    <span className="text-sm text-gray-600">{selectedFiles.length > 0 ? t("expenses.addAttachment") : t("expenses.dropFilesHere")}</span>
                    <span className="text-xs text-gray-500">{t("expenses.maxFileSize")}</span>
                  </label>
                  <input id="receipt" type="file" multiple className="hidden" accept={FILE_CONFIG.allowedImageAccept} onChange={handleFileSelect} />
                </div>
              </div>
            </div>
          </div>

          <DialogFooter className="flex-shrink-0 px-6 pb-6 pt-4 border-t">
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              {t("common.cancel")}
            </Button>
            <Button type="submit">{expense ? t("common.save") : t("common.create")}</Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
