import { useEffect, useState, useCallback } from "react";
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Mail, Calendar, TrendingUp, CheckCircle2, XCircle, Users, Loader2, Building2, Shield, Trash2, AlertTriangle } from "lucide-react";
import { workplaceMembersApi } from "@/lib/proxy/api";
import type { UserDetail } from "@/lib/proxy/types";
import { roleLabels, roleColors } from "@/constants";
import { toast } from "sonner";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";

interface UserDetailModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  userId: string | null;
  onUserDeleted?: () => void;
}

const statusColors = {
  Pending: "bg-yellow-500/10 text-yellow-500",
  Approved: "bg-green-500/10 text-green-500",
  Rejected: "bg-red-500/10 text-red-500",
  Paid: "bg-blue-500/10 text-blue-500",
};

const statusLabels = {
  Pending: "Čeká na schválení",
  Approved: "Schváleno",
  Rejected: "Zamítnuto",
  Paid: "Vyplaceno",
};

const actionColors = {
  Approve: "text-green-600",
  Reject: "text-red-600",
};

const actionLabels = {
  Approve: "Schválil",
  Reject: "Zamítl",
};

const invitationStatusColors = {
  Pending: "bg-yellow-500/10 text-yellow-500",
  Accepted: "bg-green-500/10 text-green-500",
  Expired: "bg-gray-500/10 text-gray-500",
  Cancelled: "bg-red-500/10 text-red-500",
};

const invitationStatusLabels = {
  Pending: "Čeká",
  Accepted: "Přijato",
  Expired: "Vypršelo",
  Cancelled: "Zrušeno",
};

export function UserDetailModal({ open, onOpenChange, userId, onUserDeleted }: UserDetailModalProps) {
  const [userDetail, setUserDetail] = useState<UserDetail | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);

  const loadUserDetail = useCallback(async () => {
    if (!userId) return;

    try {
      setIsLoading(true);
      const data = await workplaceMembersApi.getUserDetail(userId);
      setUserDetail(data);
    } catch (error) {
      console.error("Failed to load user detail:", error);
    } finally {
      setIsLoading(false);
    }
  }, [userId]);

  useEffect(() => {
    if (open && userId) {
      loadUserDetail();
    }
  }, [open, userId, loadUserDetail]);

  const handleDeleteUser = async () => {
    if (!userId) return;

    try {
      setIsDeleting(true);
      await workplaceMembersApi.deleteUser(userId);
      toast.success("Uživatel byl úspěšně odstraněn");
      setShowDeleteDialog(false);
      onOpenChange(false);
      if (onUserDeleted) {
        onUserDeleted();
      }
    } catch (error) {
      console.error("Failed to delete user:", error);
      toast.error("Nepodařilo se odstranit uživatele");
    } finally {
      setIsDeleting(false);
    }
  };

  if (!open) return null;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-h-[90vh] overflow-y-auto" style={{ maxWidth: "55vw" }}>
        <DialogHeader>
          <div className="flex items-center justify-between">
            <div>
              <DialogTitle>Detail uživatele</DialogTitle>
              <DialogDescription>Kompletní přehled aktivit a statistik uživatele</DialogDescription>
            </div>
            <Button variant="destructive" size="sm" onClick={() => setShowDeleteDialog(true)} disabled={isDeleting}>
              <Trash2 className="h-4 w-4 mr-2" />
              Odstranit uživatele
            </Button>
          </div>
        </DialogHeader>

        {isLoading ? (
          <div className="flex justify-center items-center py-12">
            <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
          </div>
        ) : userDetail ? (
          <div className="space-y-6">
            {/* User Info Card */}
            <Card>
              <CardHeader>
                <div className="flex items-start gap-4">
                  <Avatar className="h-16 w-16">
                    <AvatarFallback className="text-xl">
                      {userDetail.name
                        ?.split(" ")
                        .map((n) => n[0])
                        .join("") || userDetail.email.substring(0, 2).toUpperCase()}
                    </AvatarFallback>
                  </Avatar>
                  <div className="flex-1">
                    <div className="flex items-center gap-3">
                      <CardTitle className="text-2xl">{userDetail.name || userDetail.email}</CardTitle>
                      <Badge variant="secondary" className={roleColors[userDetail.role as keyof typeof roleColors]}>
                        {roleLabels[userDetail.role as keyof typeof roleLabels]}
                      </Badge>
                    </div>
                    <CardDescription className="mt-2 flex items-center gap-2">
                      <Mail className="h-4 w-4" />
                      {userDetail.email}
                    </CardDescription>
                    <div className="mt-2 flex items-center gap-2 text-sm text-muted-foreground">
                      <Calendar className="h-4 w-4" />
                      Registrován: {new Date(userDetail.createdAt).toLocaleDateString("cs-CZ")}
                    </div>
                  </div>
                </div>
              </CardHeader>
            </Card>

            {/* Stats Overview */}
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-2">
              <Card>
                <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                  <CardTitle className="text-sm font-medium">Celkem výdajů</CardTitle>
                  <TrendingUp className="h-4 w-4 text-muted-foreground" />
                </CardHeader>
                <CardContent>
                  <div className="text-2xl font-bold">{userDetail.expenseStats.total.toLocaleString("cs-CZ")} Kč</div>
                  <p className="text-xs text-muted-foreground">{userDetail.expenseStats.count} výdajů</p>
                </CardContent>
              </Card>

              <Card>
                <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                  <CardTitle className="text-sm font-medium">Pracoviště</CardTitle>
                  <Building2 className="h-4 w-4 text-muted-foreground" />
                </CardHeader>
                <CardContent>
                  <div className="text-2xl font-bold">{userDetail.memberships.length}</div>
                  <p className="text-xs text-muted-foreground">{userDetail.memberships.filter((m) => m.isManager).length} jako manager</p>
                </CardContent>
              </Card>

              <Card>
                <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                  <CardTitle className="text-sm font-medium">Schválení</CardTitle>
                  <CheckCircle2 className="h-4 w-4 text-muted-foreground" />
                </CardHeader>
                <CardContent>
                  <div className="text-2xl font-bold">{userDetail.approvalStats.count}</div>
                  <p className="text-xs text-muted-foreground">
                    {userDetail.approvalStats.approved} schváleno, {userDetail.approvalStats.rejected} zamítnuto
                  </p>
                </CardContent>
              </Card>

              <Card>
                <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                  <CardTitle className="text-sm font-medium">Pozvánky</CardTitle>
                  <Users className="h-4 w-4 text-muted-foreground" />
                </CardHeader>
                <CardContent>
                  <div className="text-2xl font-bold">{userDetail.invitationStats.count}</div>
                  <p className="text-xs text-muted-foreground">
                    {userDetail.invitationStats.accepted} přijato, {userDetail.invitationStats.pending} čeká
                  </p>
                </CardContent>
              </Card>
            </div>

            {/* Tabs with detailed data */}
            <Tabs defaultValue="expenses" className="space-y-4">
              <TabsList className="grid w-full grid-cols-4">
                <TabsTrigger value="expenses">Výdaje ({userDetail.expenses.length})</TabsTrigger>
                <TabsTrigger value="memberships">Pracoviště ({userDetail.memberships.length})</TabsTrigger>
                <TabsTrigger value="approvals">Schválení ({userDetail.approvals.length})</TabsTrigger>
                <TabsTrigger value="invitations">Pozvánky ({userDetail.invitations.length})</TabsTrigger>
              </TabsList>

              {/* Expenses Tab */}
              <TabsContent value="expenses" className="space-y-4">
                <Card>
                  <CardHeader>
                    <CardTitle>Historie výdajů</CardTitle>
                    <CardDescription>Všechny výdaje vytvořené tímto uživatelem</CardDescription>
                  </CardHeader>
                  <CardContent>
                    {userDetail.expenses.length === 0 ? (
                      <div className="text-center py-8 text-muted-foreground">Žádné výdaje</div>
                    ) : (
                      <>
                        {/* Expense Stats by Status */}
                        <div className="mb-4 grid gap-4 md:grid-cols-3">
                          {userDetail.expenseStats.byStatus.map((stat) => (
                            <Card key={stat.status}>
                              <CardContent className="pt-6">
                                <div className="flex items-center justify-between">
                                  <div>
                                    <p className="text-sm font-medium text-muted-foreground">
                                      {statusLabels[stat.status as keyof typeof statusLabels] || stat.status}
                                    </p>
                                    <p className="text-2xl font-bold">{stat.total.toLocaleString("cs-CZ")} Kč</p>
                                    <p className="text-xs text-muted-foreground">{stat.count} výdajů</p>
                                  </div>
                                  <Badge className={statusColors[stat.status as keyof typeof statusColors]}>{stat.count}</Badge>
                                </div>
                              </CardContent>
                            </Card>
                          ))}
                        </div>

                        <Separator className="my-4" />

                        <Table>
                          <TableHeader>
                            <TableRow>
                              <TableHead>Datum</TableHead>
                              <TableHead>Popis</TableHead>
                              <TableHead>Kategorie</TableHead>
                              <TableHead>Pracoviště</TableHead>
                              <TableHead className="text-right">Částka</TableHead>
                              <TableHead>Status</TableHead>
                            </TableRow>
                          </TableHeader>
                          <TableBody>
                            {userDetail.expenses.map((expense) => (
                              <TableRow key={expense.id}>
                                <TableCell>{new Date(expense.expenseDate).toLocaleDateString("cs-CZ")}</TableCell>
                                <TableCell>
                                  <div className="max-w-xs truncate">{expense.description || "—"}</div>
                                </TableCell>
                                <TableCell>{expense.categoryName}</TableCell>
                                <TableCell>{expense.workplaceName}</TableCell>
                                <TableCell className="text-right font-medium">
                                  {expense.amount.toLocaleString("cs-CZ")} {expense.currency}
                                </TableCell>
                                <TableCell>
                                  <Badge className={statusColors[expense.status as keyof typeof statusColors]}>
                                    {statusLabels[expense.status as keyof typeof statusLabels]}
                                  </Badge>
                                </TableCell>
                              </TableRow>
                            ))}
                          </TableBody>
                        </Table>
                      </>
                    )}
                  </CardContent>
                </Card>
              </TabsContent>

              {/* Memberships Tab */}
              <TabsContent value="memberships" className="space-y-4">
                <Card>
                  <CardHeader>
                    <CardTitle>Členství na pracovištích</CardTitle>
                    <CardDescription>Pracoviště, na kterých je uživatel členem</CardDescription>
                  </CardHeader>
                  <CardContent>
                    {userDetail.memberships.length === 0 ? (
                      <div className="text-center py-8 text-muted-foreground">Žádná členství</div>
                    ) : (
                      <Table>
                        <TableHeader>
                          <TableRow>
                            <TableHead>Pracoviště</TableHead>
                            <TableHead>Pozice</TableHead>
                            <TableHead>Role</TableHead>
                            <TableHead>Připojen</TableHead>
                          </TableRow>
                        </TableHeader>
                        <TableBody>
                          {userDetail.memberships.map((membership) => (
                            <TableRow key={membership.id}>
                              <TableCell className="font-medium">{membership.workplaceName}</TableCell>
                              <TableCell>{membership.positionName || "—"}</TableCell>
                              <TableCell>
                                {membership.isManager ? (
                                  <Badge variant="secondary" className="bg-purple-500/10 text-purple-500">
                                    <Shield className="h-3 w-3 mr-1" />
                                    Manager
                                  </Badge>
                                ) : (
                                  <Badge variant="outline">Člen</Badge>
                                )}
                              </TableCell>
                              <TableCell>{new Date(membership.joinedAt).toLocaleDateString("cs-CZ")}</TableCell>
                            </TableRow>
                          ))}
                        </TableBody>
                      </Table>
                    )}
                  </CardContent>
                </Card>
              </TabsContent>

              {/* Approvals Tab */}
              <TabsContent value="approvals" className="space-y-4">
                <Card>
                  <CardHeader>
                    <CardTitle>Historie schvalování</CardTitle>
                    <CardDescription>Výdaje schválené nebo zamítnuté tímto uživatelem</CardDescription>
                  </CardHeader>
                  <CardContent>
                    {userDetail.approvals.length === 0 ? (
                      <div className="text-center py-8 text-muted-foreground">Žádná schválení</div>
                    ) : (
                      <Table>
                        <TableHeader>
                          <TableRow>
                            <TableHead>Datum</TableHead>
                            <TableHead>Akce</TableHead>
                            <TableHead>Výdaj</TableHead>
                            <TableHead>Kategorie</TableHead>
                            <TableHead className="text-right">Částka</TableHead>
                            <TableHead>Poznámka</TableHead>
                          </TableRow>
                        </TableHeader>
                        <TableBody>
                          {userDetail.approvals.map((approval) => (
                            <TableRow key={approval.id}>
                              <TableCell>{new Date(approval.createdAt).toLocaleDateString("cs-CZ")}</TableCell>
                              <TableCell>
                                <Badge variant="outline" className={actionColors[approval.action as keyof typeof actionColors]}>
                                  {approval.action === "Approve" ? <CheckCircle2 className="h-3 w-3 mr-1" /> : <XCircle className="h-3 w-3 mr-1" />}
                                  {actionLabels[approval.action as keyof typeof actionLabels] || approval.action}
                                </Badge>
                              </TableCell>
                              <TableCell>
                                <div className="max-w-xs truncate">{approval.expenseDescription || "—"}</div>
                              </TableCell>
                              <TableCell>{approval.categoryName}</TableCell>
                              <TableCell className="text-right font-medium">
                                {approval.expenseAmount.toLocaleString("cs-CZ")} {approval.expenseCurrency}
                              </TableCell>
                              <TableCell>
                                <div className="max-w-xs truncate">{approval.note || "—"}</div>
                              </TableCell>
                            </TableRow>
                          ))}
                        </TableBody>
                      </Table>
                    )}
                  </CardContent>
                </Card>
              </TabsContent>

              {/* Invitations Tab */}
              <TabsContent value="invitations" className="space-y-4">
                <Card>
                  <CardHeader>
                    <CardTitle>Odeslané pozvánky</CardTitle>
                    <CardDescription>Pozvánky vytvořené tímto uživatelem</CardDescription>
                  </CardHeader>
                  <CardContent>
                    {userDetail.invitations.length === 0 ? (
                      <div className="text-center py-8 text-muted-foreground">Žádné pozvánky</div>
                    ) : (
                      <Table>
                        <TableHeader>
                          <TableRow>
                            <TableHead>Email</TableHead>
                            <TableHead>Pracoviště</TableHead>
                            <TableHead>Status</TableHead>
                            <TableHead>Vytvořeno</TableHead>
                            <TableHead>Vyprší</TableHead>
                            <TableHead>Přijato</TableHead>
                          </TableRow>
                        </TableHeader>
                        <TableBody>
                          {userDetail.invitations.map((invitation) => (
                            <TableRow key={invitation.id}>
                              <TableCell className="font-medium">{invitation.email}</TableCell>
                              <TableCell>{invitation.workplaceName || "—"}</TableCell>
                              <TableCell>
                                <Badge className={invitationStatusColors[invitation.status as keyof typeof invitationStatusColors]}>
                                  {invitationStatusLabels[invitation.status as keyof typeof invitationStatusLabels] || invitation.status}
                                </Badge>
                              </TableCell>
                              <TableCell>{new Date(invitation.createdAt).toLocaleDateString("cs-CZ")}</TableCell>
                              <TableCell>{new Date(invitation.expiresAt).toLocaleDateString("cs-CZ")}</TableCell>
                              <TableCell>{invitation.acceptedAt ? new Date(invitation.acceptedAt).toLocaleDateString("cs-CZ") : "—"}</TableCell>
                            </TableRow>
                          ))}
                        </TableBody>
                      </Table>
                    )}
                  </CardContent>
                </Card>
              </TabsContent>
            </Tabs>
          </div>
        ) : (
          <div className="text-center py-8 text-muted-foreground">Nepodařilo se načíst detail uživatele</div>
        )}
      </DialogContent>

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={showDeleteDialog} onOpenChange={setShowDeleteDialog}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle className="flex items-center gap-2">
              <AlertTriangle className="h-5 w-5 text-destructive" />
              Odstranit uživatele?
            </AlertDialogTitle>
            <AlertDialogDescription className="space-y-2">
              <p>Tato akce je nevratná a odstraní:</p>
              <ul className="list-disc list-inside ml-4 space-y-1">
                <li>Všechna členství na pracovištích</li>
                <li>Všechny výdaje (budou označeny jako smazané)</li>
                <li>Všechna schválení výdajů</li>
                <li>Všechny pozvánky (budou zrušeny)</li>
                <li>Uživatelský účet z Identity systému</li>
              </ul>
              <p className="font-semibold mt-4">Opravdu chcete pokračovat?</p>
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel disabled={isDeleting}>Zrušit</AlertDialogCancel>
            <AlertDialogAction onClick={handleDeleteUser} disabled={isDeleting} className="bg-destructive text-destructive-foreground hover:bg-destructive/90">
              {isDeleting ? (
                <>
                  <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                  Odstraňuji...
                </>
              ) : (
                <>
                  <Trash2 className="h-4 w-4 mr-2" />
                  Ano, odstranit
                </>
              )}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </Dialog>
  );
}
