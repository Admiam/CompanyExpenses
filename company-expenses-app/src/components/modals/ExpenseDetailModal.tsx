import { useEffect, useState } from "react";
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Calendar, CheckCircle2, XCircle, User, Clock, Edit2, Save, X, Upload, Image as ImageIcon, Trash2 } from "lucide-react";
import { expensesApi, categoriesApi } from "@/lib/proxy/api";

interface ExpenseApproval {
  id: string;
  action: string;
  actorEmail: string;
  note?: string;
  createdAt: string;
}

interface ExpenseAttachment {
  id: string;
  originalFileName: string;
  dataType: string;
  fileSize: number;
  base64Data: string;
  uploadedAt: string;
}

interface ExpenseDetail {
  id: string;
  description: string;
  amount: number;
  currency: string;
  expenseDate: string;
  status: "Pending" | "Approved" | "Rejected";
  workplaceId: string;
  categoryId: string;
  workplace?: { id: string; name: string };
  category?: { id: string; name: string };
  submittedAt: string;
  lastDecisionAt?: string;
  lastDecisionBy?: string;
  rejectionNote?: string;
  attachments: ExpenseAttachment[];
  approvals: ExpenseApproval[];
}

interface ExpenseDetailModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  expenseId: string | null;
}

const statusColors = {
  Pending: "bg-yellow-500/10 text-yellow-500",
  Approved: "bg-green-500/10 text-green-500",
  Rejected: "bg-red-500/10 text-red-500",
};

const statusLabels = {
  Pending: "Čeká na schválení",
  Approved: "Schváleno",
  Rejected: "Zamítnuto",
};

const actionColors = {
  Approved: "text-green-600",
  Rejected: "text-red-600",
  ReturnedForRevision: "text-orange-600",
};

const actionLabels = {
  Approved: "Schváleno",
  Rejected: "Zamítnuto",
  ReturnedForRevision: "Vráceno k revizi",
};

export function ExpenseDetailModal({ open, onOpenChange, expenseId }: ExpenseDetailModalProps) {
  const [expense, setExpense] = useState<ExpenseDetail | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [isEditingAmount, setIsEditingAmount] = useState(false);
  const [editedAmount, setEditedAmount] = useState<string>("");
  const [isSaving, setIsSaving] = useState(false);
  const [isEditingCategory, setIsEditingCategory] = useState(false);
  const [editedCategoryId, setEditedCategoryId] = useState<string>("");
  const [availableCategories, setAvailableCategories] = useState<Array<{ id: string; name: string }>>([]);
  const [isEditingAttachments, setIsEditingAttachments] = useState(false);
  const [newAttachments, setNewAttachments] = useState<File[]>([]);
  const [existingAttachments, setExistingAttachments] = useState<ExpenseAttachment[]>([]);
  const [attachmentsToDelete, setAttachmentsToDelete] = useState<string[]>([]);

  const loadExpenseDetail = async () => {
    if (!expenseId) return;

    try {
      setIsLoading(true);
      const data = await expensesApi.getExpense(expenseId);
      setExpense(data as any);
    } catch (error) {
      console.error("Failed to load expense detail:", error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleEditAmount = () => {
    if (expense) {
      setEditedAmount(expense.amount.toString());
      setIsEditingAmount(true);
    }
  };

  const handleCancelEdit = () => {
    setIsEditingAmount(false);
    setEditedAmount("");
  };

  const handleSaveAmount = async () => {
    if (!expense || !expenseId) return;

    const newAmount = parseFloat(editedAmount);
    if (isNaN(newAmount) || newAmount <= 0) {
      alert("Zadejte platnou částku");
      return;
    }

    try {
      setIsSaving(true);
      await expensesApi.updateExpenseAmount(expenseId, newAmount);
      setIsEditingAmount(false);
      await loadExpenseDetail();
    } catch (error) {
      console.error("Failed to update amount:", error);
      alert("Nepodařilo se aktualizovat částku");
    } finally {
      setIsSaving(false);
    }
  };

  const handleEditCategory = async () => {
    if (!expense) return;

    try {
      const categories = await categoriesApi.getCategoriesForWorkplace(expense.workplaceId);
      setAvailableCategories(categories as any);
      setEditedCategoryId(expense.categoryId);
      setIsEditingCategory(true);
    } catch (error) {
      console.error("Failed to load categories:", error);
      alert("Nepodařilo se načíst kategorie");
    }
  };

  const handleCancelCategoryEdit = () => {
    setIsEditingCategory(false);
    setEditedCategoryId("");
    setAvailableCategories([]);
  };

  const handleSaveCategory = async () => {
    if (!expense || !expenseId || !editedCategoryId) return;

    try {
      setIsSaving(true);
      await expensesApi.updateExpenseCategory(expenseId, editedCategoryId);
      setIsEditingCategory(false);
      await loadExpenseDetail();
    } catch (error) {
      console.error("Failed to update category:", error);
      alert("Nepodařilo se aktualizovat kategorii");
    } finally {
      setIsSaving(false);
    }
  };

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      setNewAttachments(Array.from(e.target.files));
    }
  };

  const handleRemoveNewAttachment = (index: number) => {
    setNewAttachments((prev) => prev.filter((_, i) => i !== index));
  };

  const handleEditAttachments = () => {
    if (expense?.attachments) {
      setExistingAttachments([...expense.attachments]);
    }
    setIsEditingAttachments(true);
    setNewAttachments([]);
    setAttachmentsToDelete([]);
  };

  const handleCancelAttachmentsEdit = () => {
    setIsEditingAttachments(false);
    setNewAttachments([]);
    setExistingAttachments([]);
    setAttachmentsToDelete([]);
  };

  const handleDeleteExistingAttachment = (attachmentId: string) => {
    setAttachmentsToDelete((prev) => [...prev, attachmentId]);
    setExistingAttachments((prev) => prev.filter((a) => a.id !== attachmentId));
  };

  const handleDownloadAttachment = (attachment: ExpenseAttachment) => {
    try {
      // Convert base64 to blob
      const byteCharacters = atob(attachment.base64Data);
      const byteNumbers = new Array(byteCharacters.length);
      for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
      }
      const byteArray = new Uint8Array(byteNumbers);
      const blob = new Blob([byteArray], { type: attachment.dataType });

      // Create download link
      const url = URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = attachment.originalFileName;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(url);
    } catch (error) {
      console.error("Failed to download attachment:", error);
      alert("Nepodařilo se stáhnout přílohu");
    }
  };

  const handleSaveAttachments = async () => {
    if (!expense || !expenseId) return;

    try {
      setIsSaving(true);

      // Convert new files to base64
      const newAttachmentsData = await Promise.all(
        newAttachments.map(async (file) => {
          const base64 = await new Promise<string>((resolve) => {
            const reader = new FileReader();
            reader.onloadend = () => {
              const result = reader.result as string;
              resolve(result.split(",")[1]);
            };
            reader.readAsDataURL(file);
          });

          return {
            fileName: file.name,
            fileType: file.type,
            base64Data: base64,
            originalFileSize: file.size,
          };
        })
      );

      // Combine existing (not deleted) and new attachments
      const allAttachments = [
        ...existingAttachments.map((a) => ({
          fileName: a.originalFileName,
          fileType: a.dataType,
          base64Data: a.base64Data,
          originalFileSize: a.fileSize,
        })),
        ...newAttachmentsData,
      ];

      await expensesApi.updateExpenseAttachments(expenseId, allAttachments);
      setIsEditingAttachments(false);
      setNewAttachments([]);
      setExistingAttachments([]);
      setAttachmentsToDelete([]);
      await loadExpenseDetail();
    } catch (error) {
      console.error("Failed to update attachments:", error);
      alert("Nepodařilo se aktualizovat přílohy");
    } finally {
      setIsSaving(false);
    }
  };

  useEffect(() => {
    if (open && expenseId) {
      loadExpenseDetail();
      setIsEditingAmount(false);
      setIsEditingCategory(false);
      setIsEditingAttachments(false);
      setNewAttachments([]);
      setExistingAttachments([]);
      setAttachmentsToDelete([]);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, expenseId]);

  if (!expense && !isLoading) return null;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[600px] max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Detail výdaje</DialogTitle>
          <DialogDescription>Kompletní informace o výdaji včetně historie schvalování</DialogDescription>
        </DialogHeader>

        {isLoading ? (
          <div className="flex justify-center py-8">
            <p className="text-muted-foreground">Načítání...</p>
          </div>
        ) : expense ? (
          <div className="space-y-6">
            {/* Basic Info */}
            <Card>
              <CardHeader className="pb-3">
                <div className="flex items-center justify-between">
                  <CardTitle className="text-lg">Základní informace</CardTitle>
                  <Badge variant="secondary" className={statusColors[expense.status]}>
                    {statusLabels[expense.status]}
                  </Badge>
                </div>
              </CardHeader>
              <CardContent className="space-y-3">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <p className="text-sm text-muted-foreground">Popis</p>
                    <p className="font-medium">{expense.description}</p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground mb-1">Částka</p>
                    {isEditingAmount ? (
                      <div className="flex items-center gap-2">
                        <Input type="number" value={editedAmount} onChange={(e) => setEditedAmount(e.target.value)} className="w-32" step="0.01" min="0" />
                        <span className="text-sm">{expense.currency}</span>
                        <Button size="sm" onClick={handleSaveAmount} disabled={isSaving} className="h-8 w-8 p-0">
                          <Save className="h-4 w-4" />
                        </Button>
                        <Button size="sm" variant="outline" onClick={handleCancelEdit} disabled={isSaving} className="h-8 w-8 p-0">
                          <X className="h-4 w-4" />
                        </Button>
                      </div>
                    ) : (
                      <div className="flex items-center gap-2">
                        <p className="font-medium text-lg">
                          {expense.amount.toLocaleString("cs-CZ")} {expense.currency}
                        </p>
                        {expense.status === "Pending" && (
                          <Button size="sm" variant="ghost" onClick={handleEditAmount} className="h-8 w-8 p-0">
                            <Edit2 className="h-4 w-4" />
                          </Button>
                        )}
                      </div>
                    )}
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground mb-1">Kategorie</p>
                    {isEditingCategory ? (
                      <div className="flex items-center gap-2">
                        <Select value={editedCategoryId} onValueChange={setEditedCategoryId}>
                          <SelectTrigger className="w-[200px]">
                            <SelectValue placeholder="Vyberte kategorii" />
                          </SelectTrigger>
                          <SelectContent>
                            {availableCategories.map((category) => (
                              <SelectItem key={category.id} value={category.id}>
                                {category.name}
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                        <Button size="sm" onClick={handleSaveCategory} disabled={isSaving || !editedCategoryId} className="h-8 w-8 p-0">
                          <Save className="h-4 w-4" />
                        </Button>
                        <Button size="sm" variant="outline" onClick={handleCancelCategoryEdit} disabled={isSaving} className="h-8 w-8 p-0">
                          <X className="h-4 w-4" />
                        </Button>
                      </div>
                    ) : (
                      <div className="flex items-center gap-2">
                        <p className="font-medium">{expense.category?.name || "N/A"}</p>
                        {expense.status === "Pending" && (
                          <Button size="sm" variant="ghost" onClick={handleEditCategory} className="h-8 w-8 p-0">
                            <Edit2 className="h-4 w-4" />
                          </Button>
                        )}
                      </div>
                    )}
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Pracoviště</p>
                    <p className="font-medium">{expense.workplace?.name || "N/A"}</p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Datum výdaje</p>
                    <p className="font-medium flex items-center gap-1">
                      <Calendar className="h-4 w-4" />
                      {new Date(expense.expenseDate).toLocaleDateString("cs-CZ")}
                    </p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Datum podání</p>
                    <p className="font-medium flex items-center gap-1">
                      <Clock className="h-4 w-4" />
                      {new Date(expense.submittedAt).toLocaleDateString("cs-CZ")}
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Rejection Note */}
            {expense.rejectionNote && (
              <Card className="border-red-200 bg-red-50/50">
                <CardHeader className="pb-3">
                  <CardTitle className="text-lg text-red-700 flex items-center gap-2">
                    <XCircle className="h-5 w-5" />
                    Důvod zamítnutí
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <p className="text-sm">{expense.rejectionNote}</p>
                </CardContent>
              </Card>
            )}

            {/* Decision Info */}
            {expense.lastDecisionAt && (
              <Card>
                <CardHeader className="pb-3">
                  <CardTitle className="text-lg">Poslední rozhodnutí</CardTitle>
                </CardHeader>
                <CardContent className="space-y-2">
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-muted-foreground">Rozhodl:</span>
                    <span className="font-medium flex items-center gap-1">
                      <User className="h-4 w-4" />
                      {expense.lastDecisionBy}
                    </span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-muted-foreground">Datum:</span>
                    <span className="font-medium">{new Date(expense.lastDecisionAt).toLocaleString("cs-CZ")}</span>
                  </div>
                </CardContent>
              </Card>
            )}

            {/* Attachments */}
            <Card>
              <CardHeader className="pb-3">
                <div className="flex items-center justify-between">
                  <CardTitle className="text-lg">Přílohy ({expense.attachments?.length || 0})</CardTitle>
                  {expense.status === "Pending" && !isEditingAttachments && (
                    <Button size="sm" variant="outline" onClick={handleEditAttachments} className="h-8">
                      <Edit2 className="h-4 w-4 mr-1" />
                      Upravit
                    </Button>
                  )}
                </div>
              </CardHeader>
              <CardContent>
                {isEditingAttachments ? (
                  <div className="space-y-4">
                    {/* Existing attachments */}
                    {existingAttachments.length > 0 && (
                      <div className="space-y-2">
                        <p className="text-sm font-medium">Současné přílohy ({existingAttachments.length}):</p>
                        <div className="grid grid-cols-2 gap-2">
                          {existingAttachments.map((attachment) => (
                            <div key={attachment.id} className="relative group">
                              <img
                                src={`data:${attachment.dataType};base64,${attachment.base64Data}`}
                                alt={attachment.originalFileName}
                                className="w-full h-32 object-cover rounded border"
                              />
                              <Button
                                size="sm"
                                variant="destructive"
                                onClick={() => handleDeleteExistingAttachment(attachment.id)}
                                className="absolute top-1 right-1 h-6 w-6 p-0 opacity-0 group-hover:opacity-100 transition-opacity"
                              >
                                <Trash2 className="h-3 w-3" />
                              </Button>
                              <p className="text-xs mt-1 truncate">{attachment.originalFileName}</p>
                            </div>
                          ))}
                        </div>
                      </div>
                    )}

                    {/* Upload new attachments */}
                    <div className="border-2 border-dashed rounded-lg p-4">
                      <label className="flex flex-col items-center cursor-pointer">
                        <Upload className="h-8 w-8 text-muted-foreground mb-2" />
                        <span className="text-sm text-muted-foreground">Klikněte pro přidání nových obrázků</span>
                        <input type="file" multiple accept="image/*" onChange={handleFileSelect} className="hidden" />
                      </label>
                    </div>
                    {newAttachments.length > 0 && (
                      <div className="space-y-2">
                        <p className="text-sm font-medium">Nově přidané soubory ({newAttachments.length}):</p>
                        <div className="grid grid-cols-2 gap-2">
                          {newAttachments.map((file, index) => (
                            <div key={index} className="relative group">
                              <img src={URL.createObjectURL(file)} alt={file.name} className="w-full h-32 object-cover rounded border" />
                              <Button
                                size="sm"
                                variant="destructive"
                                onClick={() => handleRemoveNewAttachment(index)}
                                className="absolute top-1 right-1 h-6 w-6 p-0 opacity-0 group-hover:opacity-100 transition-opacity"
                              >
                                <Trash2 className="h-3 w-3" />
                              </Button>
                              <p className="text-xs mt-1 truncate">{file.name}</p>
                            </div>
                          ))}
                        </div>
                      </div>
                    )}
                    <div className="flex gap-2 justify-end">
                      <Button size="sm" variant="outline" onClick={handleCancelAttachmentsEdit} disabled={isSaving}>
                        <X className="h-4 w-4 mr-1" />
                        Zrušit
                      </Button>
                      <Button size="sm" onClick={handleSaveAttachments} disabled={isSaving}>
                        <Save className="h-4 w-4 mr-1" />
                        Uložit
                      </Button>
                    </div>
                  </div>
                ) : (
                  <div>
                    {expense.attachments && expense.attachments.length > 0 ? (
                      <div className="grid grid-cols-2 gap-3">
                        {expense.attachments.map((attachment) => (
                          <div key={attachment.id} className="space-y-1">
                            <img
                              src={`data:${attachment.dataType};base64,${attachment.base64Data}`}
                              alt={attachment.originalFileName}
                              className="w-full h-32 object-cover rounded border cursor-pointer hover:opacity-80 transition-opacity"
                              onClick={() => handleDownloadAttachment(attachment)}
                            />
                            <p className="text-xs text-muted-foreground truncate">{attachment.originalFileName}</p>
                            <p className="text-xs text-muted-foreground">{(attachment.fileSize / 1024).toFixed(1)} KB</p>
                          </div>
                        ))}
                      </div>
                    ) : (
                      <div className="flex flex-col items-center justify-center py-8 text-muted-foreground">
                        <ImageIcon className="h-12 w-12 mb-2 opacity-20" />
                        <p className="text-sm">Žádné přílohy</p>
                      </div>
                    )}
                  </div>
                )}
              </CardContent>
            </Card>

            {/* Approval History */}
            {expense.approvals && expense.approvals.length > 0 && (
              <Card>
                <CardHeader className="pb-3">
                  <CardTitle className="text-lg">Historie schvalování</CardTitle>
                  <CardDescription>Chronologický přehled všech akcí</CardDescription>
                </CardHeader>
                <CardContent>
                  <div className="space-y-4">
                    {expense.approvals.map((approval, index) => (
                      <div key={approval.id}>
                        <div className="flex items-start gap-3">
                          <div className="mt-1">
                            {approval.action === "Approved" ? (
                              <CheckCircle2 className="h-5 w-5 text-green-600" />
                            ) : (
                              <XCircle className="h-5 w-5 text-red-600" />
                            )}
                          </div>
                          <div className="flex-1 space-y-1">
                            <div className="flex items-center justify-between">
                              <span className={`font-medium ${actionColors[approval.action as keyof typeof actionColors]}`}>
                                {actionLabels[approval.action as keyof typeof actionLabels]}
                              </span>
                              <span className="text-xs text-muted-foreground">{new Date(approval.createdAt).toLocaleString("cs-CZ")}</span>
                            </div>
                            <div className="flex items-center gap-1 text-sm text-muted-foreground">
                              <User className="h-3 w-3" />
                              {approval.actorEmail}
                            </div>
                            {approval.note && (
                              <div className="mt-2 rounded-md bg-muted p-3 text-sm">
                                <p className="font-medium mb-1">Poznámka:</p>
                                <p className="text-muted-foreground">{approval.note}</p>
                              </div>
                            )}
                          </div>
                        </div>
                        {index < expense.approvals.length - 1 && <Separator className="my-4" />}
                      </div>
                    ))}
                  </div>
                </CardContent>
              </Card>
            )}
          </div>
        ) : null}
      </DialogContent>
    </Dialog>
  );
}
