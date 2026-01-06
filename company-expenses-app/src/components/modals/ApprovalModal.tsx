import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { CheckCircle2, XCircle } from "lucide-react";

interface ApprovalModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  expense: {
    id: string;
    description: string;
    amount: number;
    currency: string;
  } | null;
  action: "approve" | "reject";
  onConfirm: (expenseId: string, action: "approve" | "reject", note?: string) => void;
}

export function ApprovalModal({ open, onOpenChange, expense, action, onConfirm }: ApprovalModalProps) {
  const { t } = useTranslation();
  const [note, setNote] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleConfirm = async () => {
    if (!expense) return;

    // For rejection, note is required
    if (action === "reject" && !note.trim()) {
      return;
    }

    setIsSubmitting(true);
    try {
      await onConfirm(expense.id, action, note.trim() || undefined);
      setNote("");
      onOpenChange(false);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancel = () => {
    setNote("");
    onOpenChange(false);
  };

  if (!expense) return null;

  const isApprove = action === "approve";

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            {isApprove ? (
              <>
                <CheckCircle2 className="h-5 w-5 text-green-600" />
                {t("approval.approveTitle")}
              </>
            ) : (
              <>
                <XCircle className="h-5 w-5 text-red-600" />
                {t("approval.rejectTitle")}
              </>
            )}
          </DialogTitle>
          <DialogDescription>{isApprove ? t("approval.approveConfirm") : t("approval.rejectConfirm")}</DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-4">
          {/* Expense details */}
          <div className="rounded-lg border p-4 space-y-2">
            <div className="flex justify-between">
              <span className="text-sm text-muted-foreground">{t("common.description")}:</span>
              <span className="text-sm font-medium">{expense.description}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-sm text-muted-foreground">{t("common.amount")}:</span>
              <span className="text-sm font-medium">
                {expense.amount.toLocaleString("cs-CZ")} {expense.currency}
              </span>
            </div>
          </div>

          {/* Note input */}
          <div className="space-y-2">
            <Label htmlFor="note">{isApprove ? t("approval.noteOptional") : t("approval.rejectionReason")}</Label>
            <Textarea
              id="note"
              value={note}
              onChange={(e) => setNote(e.target.value)}
              placeholder={isApprove ? t("approval.notePlaceholder") : t("approval.rejectionPlaceholder")}
              rows={4}
              required={!isApprove}
            />
            {!isApprove && !note.trim() && <p className="text-sm text-red-600">{t("approval.rejectionRequired")}</p>}
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={handleCancel} disabled={isSubmitting}>
            {t("common.cancel")}
          </Button>
          <Button onClick={handleConfirm} disabled={isSubmitting || (!isApprove && !note.trim())} variant={isApprove ? "default" : "destructive"}>
            {isSubmitting ? t("common.processing") : isApprove ? t("common.approve") : t("common.reject")}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
