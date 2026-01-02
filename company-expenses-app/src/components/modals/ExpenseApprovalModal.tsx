import { useState } from "react";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { CheckCircle, XCircle } from "lucide-react";

interface ExpenseApprovalModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  expense: {
    id: string;
    description: string;
    amount: number;
    employee: string;
    date: string;
  } | null;
  onApprove: (expenseId: string, note: string) => void;
  onReject: (expenseId: string, note: string) => void;
}

export function ExpenseApprovalModal({ open, onOpenChange, expense, onApprove, onReject }: ExpenseApprovalModalProps) {
  const [note, setNote] = useState("");
  const [isApproving, setIsApproving] = useState(false);

  const handleApprove = () => {
    if (expense) {
      onApprove(expense.id, note);
      setNote("");
      setIsApproving(false);
    }
  };

  const handleReject = () => {
    if (expense) {
      onReject(expense.id, note);
      setNote("");
      setIsApproving(false);
    }
  };

  if (!expense) return null;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[525px]">
        <DialogHeader>
          <DialogTitle>Schválení výdaje</DialogTitle>
          <DialogDescription>Rozhodněte o schválení nebo zamítnutí výdaje</DialogDescription>
        </DialogHeader>

        {/* Expense Details */}
        <div className="grid gap-4 py-4">
          <div className="grid gap-2">
            <div className="flex justify-between">
              <span className="text-sm font-medium text-muted-foreground">Popis:</span>
              <span className="text-sm font-medium">{expense.description}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-sm font-medium text-muted-foreground">Částka:</span>
              <span className="text-sm font-bold">{expense.amount.toLocaleString("cs-CZ")} Kč</span>
            </div>
            <div className="flex justify-between">
              <span className="text-sm font-medium text-muted-foreground">Zaměstnanec:</span>
              <span className="text-sm font-medium">{expense.employee}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-sm font-medium text-muted-foreground">Datum:</span>
              <span className="text-sm font-medium">{new Date(expense.date).toLocaleDateString("cs-CZ")}</span>
            </div>
          </div>

          <div className="grid gap-2">
            <Label htmlFor="note">Poznámka (volitelné)</Label>
            <Textarea id="note" value={note} onChange={(e) => setNote(e.target.value)} placeholder="Důvod schválení nebo zamítnutí..." rows={3} />
          </div>
        </div>

        <DialogFooter className="gap-2">
          <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
            Zrušit
          </Button>
          <Button type="button" variant="destructive" onClick={handleReject}>
            <XCircle className="mr-2 h-4 w-4" />
            Zamítnout
          </Button>
          <Button type="button" onClick={handleApprove} className="bg-green-600 hover:bg-green-700">
            <CheckCircle className="mr-2 h-4 w-4" />
            Schválit
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
