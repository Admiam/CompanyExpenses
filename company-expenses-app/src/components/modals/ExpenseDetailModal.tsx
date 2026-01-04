import { useEffect, useState } from "react";
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { Calendar, CheckCircle2, XCircle, User, Clock } from "lucide-react";
import { expensesApi } from "@/lib/proxy/api";

interface ExpenseApproval {
  id: string;
  action: string;
  actorEmail: string;
  note?: string;
  createdAt: string;
}

interface ExpenseDetail {
  id: string;
  description: string;
  amount: number;
  currency: string;
  expenseDate: string;
  status: "Pending" | "Approved" | "Rejected";
  workplace?: { id: string; name: string };
  category?: { id: string; name: string };
  submittedAt: string;
  lastDecisionAt?: string;
  lastDecisionBy?: string;
  rejectionNote?: string;
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

  useEffect(() => {
    if (open && expenseId) {
      loadExpenseDetail();
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
                    <p className="text-sm text-muted-foreground">Částka</p>
                    <p className="font-medium text-lg">
                      {expense.amount.toLocaleString("cs-CZ")} {expense.currency}
                    </p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Kategorie</p>
                    <p className="font-medium">{expense.category?.name || "N/A"}</p>
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
