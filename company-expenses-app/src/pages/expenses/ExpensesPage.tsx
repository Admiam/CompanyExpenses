import { MainLayout } from "@/components/layouts/MainLayout";
import { Button } from "@/components/ui/button";
import { Plus, Filter, Download, Check, X, Edit } from "lucide-react";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { useState, useEffect } from "react";
import { ExpenseFormModal } from "@/components/modals/ExpenseFormModal";
import { ApprovalModal } from "@/components/modals/ApprovalModal";
import { ExpenseDetailModal } from "@/components/modals/ExpenseDetailModal";
import { expensesApi } from "@/lib/proxy/api";
import type { CreateExpenseRequest } from "@/lib/proxy/types";
import { toast } from "sonner";
import { useAuth } from "@/auth/useAuth";
import { isManagerOrAdmin } from "@/utils/roles";
import { useTranslation } from "react-i18next";

interface Expense {
  id: string;
  description: string;
  amount: number;
  currency: string;
  expenseDate: string;
  status: "Pending" | "Approved" | "Rejected";
  employeeUserId: string;
  workplaceId: string;
  categoryId: string;
  workplace?: { id: string; name: string };
  category?: { id: string; name: string };
  attachments?: any[];
}

const statusColors = {
  Pending: "bg-yellow-500/10 text-yellow-500 hover:bg-yellow-500/20",
  Approved: "bg-green-500/10 text-green-500 hover:bg-green-500/20",
  Rejected: "bg-red-500/10 text-red-500 hover:bg-red-500/20",
};

export default function ExpensesPage() {
  const { t, i18n } = useTranslation();
  const getLocale = () => (i18n.language === "cs" ? "cs-CZ" : "en-US");
  const { user } = useAuth();
  const canApprove = isManagerOrAdmin(user?.role);

  const statusLabels = {
    Pending: t("expenses.status.pending"),
    Approved: t("expenses.status.approved"),
    Rejected: t("expenses.status.rejected"),
  };

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [expenses, setExpenses] = useState<Expense[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [approvalModalOpen, setApprovalModalOpen] = useState(false);
  const [approvalAction, setApprovalAction] = useState<"approve" | "reject">("approve");
  const [selectedExpense, setSelectedExpense] = useState<Expense | null>(null);
  const [detailModalOpen, setDetailModalOpen] = useState(false);
  const [detailExpenseId, setDetailExpenseId] = useState<string | null>(null);

  type ExpenseFormType = {
    id: string;
    description: string;
    amount: number;
    expenseDate: string;
    categoryId: string;
    workplaceId: string;
    currency?: string;
  };

  const [editingExpense, setEditingExpense] = useState<ExpenseFormType | null>(null);

  const loadExpenses = async () => {
    try {
      setIsLoading(true);
      const response = await expensesApi.getExpenses();
      setExpenses(response);
    } catch (error) {
      console.error("Failed to load expenses:", error);
      toast.error(t("expenses.loadError"));
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    loadExpenses();
  }, []);

  const handleCreate = () => {
    setEditingExpense(null);
    setIsModalOpen(true);
  };

  const handleSave = async (data: CreateExpenseRequest) => {
    try {
      console.log("Creating expense:", data);
      await expensesApi.createExpense(data);

      toast.success(t("expenses.createSuccess"));

      setIsModalOpen(false);
      loadExpenses();
    } catch (error) {
      console.error("Failed to create expense:", error);
      toast.error(t("expenses.createError"));
    }
  };

  const handleApprovalAction = (expense: Expense, action: "approve" | "reject") => {
    setSelectedExpense(expense);
    setApprovalAction(action);
    setApprovalModalOpen(true);
  };

  const handleApprovalConfirm = async (expenseId: string, action: "approve" | "reject", note?: string) => {
    try {
      if (action === "approve") {
        await expensesApi.approveExpense(expenseId, note);
        toast.success(t("expenses.approveSuccess"));
      } else {
        if (!note) {
          toast.error(t("approval.rejectionReasonRequired"));
          return;
        }
        await expensesApi.rejectExpense(expenseId, note);
        toast.success(t("expenses.rejectSuccess"));
      }

      loadExpenses();
      setApprovalModalOpen(false);
    } catch (error) {
      console.error("Failed to process approval:", error);
      toast.error(action === "approve" ? t("expenses.approveError") : t("expenses.rejectError"));
    }
  };

  const handleShowDetail = (expenseId: string) => {
    setDetailExpenseId(expenseId);
    setDetailModalOpen(true);
  };

  const handleRowClick = (expenseId: string, event: React.MouseEvent) => {
    const target = event.target as HTMLElement;
    if (target.closest("button")) {
      return;
    }
    handleShowDetail(expenseId);
  };

  const totalAmount = expenses.reduce((sum, exp) => sum + exp.amount, 0);
  const pendingExpenses = expenses.filter((exp) => exp.status === "Pending");
  const pendingAmount = pendingExpenses.reduce((sum, exp) => sum + exp.amount, 0);
  const approvedExpenses = expenses.filter((exp) => exp.status === "Approved");
  const approvedAmount = approvedExpenses.reduce((sum, exp) => sum + exp.amount, 0);

  return (
    <MainLayout>
      <div className="flex flex-col gap-4 py-4 px-4 md:gap-6 md:py-6 lg:px-6">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold tracking-tight">{t("expenses.title")}</h1>
            <p className="text-muted-foreground">{t("expenses.subtitle")}</p>
          </div>
          <div className="flex gap-2">
            <Button variant="outline" size="icon">
              <Filter className="h-4 w-4" />
            </Button>
            <Button variant="outline" size="icon">
              <Download className="h-4 w-4" />
            </Button>
            <Button onClick={handleCreate}>
              <Plus className="mr-2 h-4 w-4" />
              {t("expenses.newExpense")}
            </Button>
          </div>
        </div>

        {/* Stats Cards */}
        <div className="grid gap-4 md:grid-cols-3">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">{t("expenses.totalExpenses")}</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">
                {totalAmount.toLocaleString(getLocale())} {t("common.currency")}
              </div>
              <p className="text-xs text-muted-foreground">{t("expenses.expensesCount", { count: expenses.length })}</p>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">{t("expenses.pendingAmount")}</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">
                {pendingAmount.toLocaleString(getLocale())} {t("common.currency")}
              </div>
              <p className="text-xs text-muted-foreground">{t("expenses.expensesCount", { count: pendingExpenses.length })}</p>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">{t("expenses.approvedAmount")}</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">
                {approvedAmount.toLocaleString(getLocale())} {t("common.currency")}
              </div>
              <p className="text-xs text-muted-foreground">{t("expenses.expensesCount", { count: approvedExpenses.length })}</p>
            </CardContent>
          </Card>
        </div>

        {/* Expenses Table */}
        <Card>
          <CardHeader>
            <CardTitle>{t("expenses.expenseList")}</CardTitle>
            <CardDescription>{t("expenses.expenseListDesc")}</CardDescription>
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <div className="flex justify-center py-8">
                <p className="text-muted-foreground">{t("expenses.loadingExpenses")}</p>
              </div>
            ) : expenses.length === 0 ? (
              <div className="flex justify-center py-8">
                <p className="text-muted-foreground">{t("expenses.noExpenses")}</p>
              </div>
            ) : (
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>{t("common.description")}</TableHead>
                    <TableHead>{t("expenses.category")}</TableHead>
                    <TableHead>{t("expenses.workplace")}</TableHead>
                    <TableHead>{t("common.date")}</TableHead>
                    <TableHead className="text-right">{t("common.amount")}</TableHead>
                    <TableHead>{t("common.status")}</TableHead>
                    <TableHead className="text-right">{t("common.actions")}</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {expenses.map((expense) => (
                    <TableRow key={expense.id} className="cursor-pointer hover:bg-muted/50" onClick={(e) => handleRowClick(expense.id, e)}>
                      <TableCell className="font-medium">{expense.description || t("expenses.noDescription")}</TableCell>
                      <TableCell>{expense.category?.name || "N/A"}</TableCell>
                      <TableCell>{expense.workplace?.name || "N/A"}</TableCell>
                      <TableCell>{new Date(expense.expenseDate).toLocaleDateString(getLocale())}</TableCell>
                      <TableCell className="text-right">
                        {expense.amount.toLocaleString(getLocale())} {expense.currency}
                      </TableCell>
                      <TableCell>
                        <Badge variant="secondary" className={statusColors[expense.status]}>
                          {statusLabels[expense.status]}
                        </Badge>
                      </TableCell>
                      <TableCell className="text-right">
                        <div className="flex gap-1 justify-end">
                          {expense.status === "Pending" && (
                            <>
                              {canApprove && (
                                <>
                                  <Button
                                    variant="ghost"
                                    size="sm"
                                    onClick={() => handleApprovalAction(expense, "approve")}
                                    className="h-8 w-8 p-0 text-green-600 hover:text-green-700 hover:bg-green-50"
                                    title={t("expenses.approve")}
                                  >
                                    <Check className="h-4 w-4" />
                                  </Button>
                                  <Button
                                    variant="ghost"
                                    size="sm"
                                    onClick={() => handleApprovalAction(expense, "reject")}
                                    className="h-8 w-8 p-0 text-red-600 hover:text-red-700 hover:bg-red-50"
                                    title={t("expenses.reject")}
                                  >
                                    <X className="h-4 w-4" />
                                  </Button>
                                </>
                              )}
                              <Button
                                variant="ghost"
                                size="sm"
                                onClick={(e) => {
                                  e.stopPropagation();
                                  handleShowDetail(expense.id);
                                }}
                                className="h-8 w-8 p-0"
                                title={t("common.edit")}
                              >
                                <Edit className="h-4 w-4" />
                              </Button>
                            </>
                          )}
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            )}
          </CardContent>
        </Card>

        {/* Modals */}
        <ExpenseFormModal open={isModalOpen} onOpenChange={setIsModalOpen} expense={editingExpense} onSave={handleSave} />
        <ApprovalModal
          open={approvalModalOpen}
          onOpenChange={setApprovalModalOpen}
          expense={selectedExpense}
          action={approvalAction}
          onConfirm={handleApprovalConfirm}
        />
        <ExpenseDetailModal open={detailModalOpen} onOpenChange={setDetailModalOpen} expenseId={detailExpenseId} />
      </div>
    </MainLayout>
  );
}
