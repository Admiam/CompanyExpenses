import { MainLayout } from "@/components/layouts/MainLayout";
import { Button } from "@/components/ui/button";
import { Plus, Filter, Download } from "lucide-react";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { useState } from "react";
import { ExpenseFormModal } from "@/components/modals/ExpenseFormModal";

// Mock data - později nahraď API voláním
const mockExpenses = [
  {
    id: "1",
    description: "Nákup kancelářských potřeb",
    amount: 2500,
    date: "2024-12-15",
    status: "pending",
    employee: "Jan Novák",
    workplace: "Praha - Centrála",
  },
  {
    id: "2",
    description: "Firemní oběd s klientem",
    amount: 1200,
    date: "2024-12-14",
    status: "approved",
    employee: "Marie Svobodová",
    workplace: "Brno - Pobočka",
  },
  {
    id: "3",
    description: "Tankování služebního vozu",
    amount: 1800,
    date: "2024-12-13",
    status: "rejected",
    employee: "Petr Dvořák",
    workplace: "Praha - Centrála",
  },
];

const statusColors = {
  pending: "bg-yellow-500/10 text-yellow-500 hover:bg-yellow-500/20",
  approved: "bg-green-500/10 text-green-500 hover:bg-green-500/20",
  rejected: "bg-red-500/10 text-red-500 hover:bg-red-500/20",
};

const statusLabels = {
  pending: "Čeká na schválení",
  approved: "Schváleno",
  rejected: "Zamítnuto",
};

export default function ExpensesPage() {
  const [isModalOpen, setIsModalOpen] = useState(false);
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

  const handleCreate = () => {
    setEditingExpense(null);
    setIsModalOpen(true);
  };

  const handleSave = (data: any) => {
    console.log("Saving expense:", data);
    // TODO: API call to POST /api/Expenses
    setIsModalOpen(false);
  };

  return (
    <MainLayout>
      <div className="flex flex-col gap-4 py-4 px-4 md:gap-6 md:py-6 lg:px-6">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold tracking-tight">Výdaje</h1>
            <p className="text-muted-foreground">Správa a sledování výdajů</p>
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
              Nový výdaj
            </Button>
          </div>
        </div>

        {/* Stats Cards */}
        <div className="grid gap-4 md:grid-cols-3">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Celkem výdajů</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">5 500 Kč</div>
              <p className="text-xs text-muted-foreground">Za tento měsíc</p>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Čeká na schválení</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">2 500 Kč</div>
              <p className="text-xs text-muted-foreground">1 výdaj</p>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Schváleno</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">1 200 Kč</div>
              <p className="text-xs text-muted-foreground">1 výdaj</p>
            </CardContent>
          </Card>
        </div>

        {/* Expenses Table */}
        <Card>
          <CardHeader>
            <CardTitle>Seznam výdajů</CardTitle>
            <CardDescription>Přehled všech výdajů s možností rozkliknout detail</CardDescription>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Popis</TableHead>
                  <TableHead>Zaměstnanec</TableHead>
                  <TableHead>Pracoviště</TableHead>
                  <TableHead>Datum</TableHead>
                  <TableHead className="text-right">Částka</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="text-right">Akce</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {mockExpenses.map((expense) => (
                  <TableRow key={expense.id} className="cursor-pointer hover:bg-muted/50">
                    <TableCell className="font-medium">{expense.description}</TableCell>
                    <TableCell>{expense.employee}</TableCell>
                    <TableCell>{expense.workplace}</TableCell>
                    <TableCell>{new Date(expense.date).toLocaleDateString("cs-CZ")}</TableCell>
                    <TableCell className="text-right">{expense.amount.toLocaleString("cs-CZ")} Kč</TableCell>
                    <TableCell>
                      <Badge variant="secondary" className={statusColors[expense.status as keyof typeof statusColors]}>
                        {statusLabels[expense.status as keyof typeof statusLabels]}
                      </Badge>
                    </TableCell>
                    <TableCell className="text-right">
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() =>
                          setEditingExpense({
                            id: expense.id,
                            description: expense.description,
                            amount: expense.amount,
                            expenseDate: expense.date,
                            categoryId: "mock-category", // replace with real categoryId if available
                            workplaceId: "mock-workplace", // replace with real workplaceId if available
                          })
                        }
                      >
                        Detail
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>

        {/* Modal */}
        <ExpenseFormModal open={isModalOpen} onOpenChange={setIsModalOpen} expense={editingExpense} onSave={handleSave} />
      </div>
    </MainLayout>
  );
}
