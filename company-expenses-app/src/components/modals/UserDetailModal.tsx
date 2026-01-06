import { useEffect, useState, useCallback } from "react";
import { useTranslation } from "react-i18next";
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Checkbox } from "@/components/ui/checkbox";
import { Mail, Calendar, TrendingUp, CheckCircle2, XCircle, Users, Loader2, Building2, Shield, UserX, UserCheck } from "lucide-react";
import { workplaceMembersApi, rolesApi, workplacesApi } from "@/lib/proxy/api";
import type { UserDetail, Role, Workplace } from "@/lib/proxy/types";
import { roleLabels, roleColors } from "@/constants";
import { toast } from "sonner";
import { useAuth } from "@/auth/useAuth";
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
  const { t } = useTranslation();
  const { user: currentUser } = useAuth();
  const [userDetail, setUserDetail] = useState<UserDetail | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [isDeactivating, setIsDeactivating] = useState(false);
  const [showDeactivateDialog, setShowDeactivateDialog] = useState(false);
  const [roles, setRoles] = useState<Role[]>([]);
  const [isChangingRole, setIsChangingRole] = useState(false);
  const [workplaces, setWorkplaces] = useState<Workplace[]>([]);
  const [isSavingWorkplaces, setIsSavingWorkplaces] = useState(false);
  const [selectedWorkplaceIds, setSelectedWorkplaceIds] = useState<Set<string>>(new Set());

  // Role hierarchy: admin > manager > user
  const getRoleLevel = (role: string): number => {
    const roleLower = role.toLowerCase();
    if (roleLower === "admin") return 3;
    if (roleLower === "manager") return 2;
    if (roleLower === "user") return 1;
    return 0;
  };

  // Check if current user can edit target user
  const canEditUser = (): boolean => {
    if (!currentUser || !userDetail) return false;
    const currentUserLevel = getRoleLevel(currentUser.role);
    const targetUserLevel = getRoleLevel(userDetail.role);
    return currentUserLevel > targetUserLevel;
  };

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

  const loadRoles = useCallback(async () => {
    try {
      const data = await rolesApi.getRoles();
      setRoles(data);
    } catch (error) {
      console.error("Failed to load roles:", error);
    }
  }, []);

  const loadWorkplaces = useCallback(async () => {
    try {
      const data = await workplacesApi.getWorkplaces();
      setWorkplaces(data.filter((w) => w.isActive));
    } catch (error) {
      console.error("Failed to load workplaces:", error);
    }
  }, []);

  useEffect(() => {
    if (open && userId) {
      loadUserDetail();
      loadRoles();
      loadWorkplaces();
    }
  }, [open, userId, loadUserDetail, loadRoles, loadWorkplaces]);

  useEffect(() => {
    if (userDetail) {
      // Initialize selected workplaces based on user's current memberships
      const memberWorkplaceIds = new Set(userDetail.memberships?.map((m) => m.workplaceId) ?? []);
      setSelectedWorkplaceIds(memberWorkplaceIds);
    }
  }, [userDetail]);

  const handleDeactivateUser = async () => {
    if (!userId || !userDetail) return;

    try {
      setIsDeactivating(true);
      if (userDetail.isActive) {
        await workplaceMembersApi.deactivateUser(userId);
        toast.success("Uživatel byl úspěšně deaktivován");
      } else {
        await workplaceMembersApi.reactivateUser(userId);
        toast.success("Uživatel byl úspěšně aktivován");
      }
      setShowDeactivateDialog(false);
      onOpenChange(false);
      if (onUserDeleted) {
        onUserDeleted();
      }
    } catch (error) {
      console.error("Failed to deactivate/reactivate user:", error);
      toast.error("Nepodařilo se změnit stav uživatele");
    } finally {
      setIsDeactivating(false);
    }
  };

  const handleRoleChange = async (roleId: string) => {
    if (!userId) return;

    try {
      setIsChangingRole(true);
      await workplaceMembersApi.changeUserRole(userId, roleId);
      toast.success("Role uživatele byla úspěšně změněna");
      // Reload user detail to show updated role
      await loadUserDetail();
    } catch (error) {
      console.error("Failed to change user role:", error);
      toast.error("Nepodařilo se změnit roli uživatele");
    } finally {
      setIsChangingRole(false);
    }
  };

  const handleWorkplaceToggle = (workplaceId: string) => {
    setSelectedWorkplaceIds((prev) => {
      const newSet = new Set(prev);
      if (newSet.has(workplaceId)) {
        newSet.delete(workplaceId);
      } else {
        newSet.add(workplaceId);
      }
      return newSet;
    });
  };

  const handleSaveWorkplaces = async () => {
    if (!userId || !userDetail) return;

    try {
      setIsSavingWorkplaces(true);

      // Current memberships
      const currentWorkplaceIds = new Set(userDetail.memberships?.map((m) => m.workplaceId) ?? []);

      // Find workplaces to add (selected but not in current memberships)
      const toAdd = Array.from(selectedWorkplaceIds).filter((id) => !currentWorkplaceIds.has(id));

      // Find workplaces to remove (in current memberships but not selected)
      const toRemove = (userDetail.memberships ?? []).filter((m) => !selectedWorkplaceIds.has(m.workplaceId));

      // Add new memberships
      for (const workplaceId of toAdd) {
        await workplaceMembersApi.addUserToWorkplace(userId, workplaceId);
      }

      // Remove old memberships
      for (const membership of toRemove) {
        await workplaceMembersApi.removeMember(membership.id);
      }

      toast.success("Členství na pracovištích byla úspěšně aktualizována");
      // Reload user detail to show updated memberships
      await loadUserDetail();
    } catch (error: any) {
      console.error("Failed to save workplace memberships:", error);
      const errorMessage = error?.response?.data?.message || "Nepodařilo se aktualizovat členství";
      toast.error(errorMessage);
    } finally {
      setIsSavingWorkplaces(false);
    }
  };

  if (!open) return null;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-h-[90vh] overflow-y-auto" style={{ maxWidth: "55vw" }}>
        <DialogHeader>
          <div className="flex items-center justify-between">
            <div>
              <DialogTitle>{t("users.title")}</DialogTitle>
              <DialogDescription>{t("users.subtitle")}</DialogDescription>
            </div>
            {userDetail && (
              <Button
                variant={userDetail.isActive ? "outline" : "default"}
                size="sm"
                onClick={() => setShowDeactivateDialog(true)}
                disabled={isDeactivating || !canEditUser()}
              >
                {userDetail.isActive ? (
                  <>
                    <UserX className="h-4 w-4 mr-2" />
                    {t("workplaces.deactivate")}
                  </>
                ) : (
                  <>
                    <UserCheck className="h-4 w-4 mr-2" />
                    {t("workplaces.activate")}
                  </>
                )}
              </Button>
            )}
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
                    <div className="mt-4">
                      <label className="text-sm font-medium mb-2 block">{t("users.role")}</label>
                      {!canEditUser() && <p className="text-xs text-muted-foreground mb-2">{t("errors.forbidden")}</p>}
                      <Select
                        value={roles.find((r) => r.name.toLowerCase() === userDetail.role)?.id || ""}
                        onValueChange={handleRoleChange}
                        disabled={isChangingRole || !canEditUser()}
                      >
                        <SelectTrigger className="w-[200px]">
                          <SelectValue placeholder={t("invitations.selectRole")} />
                        </SelectTrigger>
                        <SelectContent>
                          {roles
                            .filter((role) => {
                              if (!currentUser) return false;
                              const currentUserLevel = getRoleLevel(currentUser.role);
                              const roleLevel = getRoleLevel(role.name);
                              return roleLevel < currentUserLevel;
                            })
                            .map((role) => (
                              <SelectItem key={role.id} value={role.id}>
                                {roleLabels[role.name.toLowerCase() as keyof typeof roleLabels] || role.name}
                              </SelectItem>
                            ))}
                        </SelectContent>
                      </Select>
                    </div>
                    <div className="mt-4">
                      <div className="flex items-center justify-between mb-2">
                        <label className="text-sm font-medium">{t("users.workplace")}</label>
                        <Button onClick={handleSaveWorkplaces} disabled={isSavingWorkplaces || !canEditUser()} size="sm" variant="secondary">
                          {isSavingWorkplaces ? (
                            <>
                              <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                              {t("common.loading")}
                            </>
                          ) : (
                            t("common.save")
                          )}
                        </Button>
                      </div>
                      {!canEditUser() && <p className="text-xs text-muted-foreground mb-2">{t("errors.forbidden")}</p>}
                      <div className="border rounded-md p-3 max-h-[200px] overflow-y-auto space-y-2">
                        {workplaces.length === 0 ? (
                          <p className="text-sm text-muted-foreground">{t("workplaces.noWorkplaces")}</p>
                        ) : (
                          workplaces.map((workplace) => (
                            <div key={workplace.id} className="flex items-center space-x-2">
                              <Checkbox
                                id={`workplace-${workplace.id}`}
                                checked={selectedWorkplaceIds.has(workplace.id)}
                                onCheckedChange={() => handleWorkplaceToggle(workplace.id)}
                                disabled={isSavingWorkplaces || !canEditUser()}
                              />
                              <label
                                htmlFor={`workplace-${workplace.id}`}
                                className="text-sm font-normal leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70 cursor-pointer"
                              >
                                {workplace.name}
                              </label>
                            </div>
                          ))
                        )}
                      </div>
                    </div>
                  </div>
                </div>
              </CardHeader>
            </Card>

            {/* Stats Overview */}
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-2">
              <Card>
                <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                  <CardTitle className="text-sm font-medium">{t("users.totalExpenses")}</CardTitle>
                  <TrendingUp className="h-4 w-4 text-muted-foreground" />
                </CardHeader>
                <CardContent>
                  <div className="text-2xl font-bold">{(userDetail.expenseStats?.total ?? 0).toLocaleString("cs-CZ")} Kč</div>
                  <p className="text-xs text-muted-foreground">{userDetail.expenseStats?.count ?? 0} výdajů</p>
                </CardContent>
              </Card>

              <Card>
                <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                  <CardTitle className="text-sm font-medium">{t("nav.workplaces")}</CardTitle>
                  <Building2 className="h-4 w-4 text-muted-foreground" />
                </CardHeader>
                <CardContent>
                  <div className="text-2xl font-bold">{userDetail.memberships?.length ?? 0}</div>
                  <p className="text-xs text-muted-foreground">{userDetail.memberships?.filter((m) => m.isManager).length ?? 0} jako manager</p>
                </CardContent>
              </Card>

              <Card>
                <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                  <CardTitle className="text-sm font-medium">{t("expenses.approve")}</CardTitle>
                  <CheckCircle2 className="h-4 w-4 text-muted-foreground" />
                </CardHeader>
                <CardContent>
                  <div className="text-2xl font-bold">{userDetail.approvalStats?.count ?? 0}</div>
                  <p className="text-xs text-muted-foreground">
                    {userDetail.approvalStats?.approved ?? 0} schváleno, {userDetail.approvalStats?.rejected ?? 0} zamítnuto
                  </p>
                </CardContent>
              </Card>

              <Card>
                <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                  <CardTitle className="text-sm font-medium">{t("users.invitations")}</CardTitle>
                  <Users className="h-4 w-4 text-muted-foreground" />
                </CardHeader>
                <CardContent>
                  <div className="text-2xl font-bold">{userDetail.invitationStats?.count ?? 0}</div>
                  <p className="text-xs text-muted-foreground">
                    {userDetail.invitationStats?.accepted ?? 0} přijato, {userDetail.invitationStats?.pending ?? 0} čeká
                  </p>
                </CardContent>
              </Card>
            </div>

            {/* Tabs with detailed data */}
            <Tabs defaultValue="expenses" className="space-y-4">
              <TabsList className="grid w-full grid-cols-4">
                <TabsTrigger value="expenses">Výdaje ({userDetail.expenses?.length ?? 0})</TabsTrigger>
                <TabsTrigger value="memberships">Pracoviště ({userDetail.memberships?.length ?? 0})</TabsTrigger>
                <TabsTrigger value="approvals">Schválení ({userDetail.approvals?.length ?? 0})</TabsTrigger>
                <TabsTrigger value="invitations">Pozvánky ({userDetail.invitations?.length ?? 0})</TabsTrigger>
              </TabsList>

              {/* Expenses Tab */}
              <TabsContent value="expenses" className="space-y-4">
                <Card>
                  <CardHeader>
                    <CardTitle>Historie výdajů</CardTitle>
                    <CardDescription>Všechny výdaje vytvořené tímto uživatelem</CardDescription>
                  </CardHeader>
                  <CardContent>
                    {(userDetail.expenses?.length ?? 0) === 0 ? (
                      <div className="text-center py-8 text-muted-foreground">Žádné výdaje</div>
                    ) : (
                      <>
                        {/* Expense Stats by Status */}
                        <div className="mb-4 grid gap-4 md:grid-cols-3">
                          {(userDetail.expenseStats?.byStatus ?? []).map((stat) => (
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
                            {(userDetail.expenses ?? []).map((expense) => (
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
                    {(userDetail.memberships?.length ?? 0) === 0 ? (
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
                          {(userDetail.memberships ?? []).map((membership) => (
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
                    {(userDetail.approvals?.length ?? 0) === 0 ? (
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
                          {(userDetail.approvals ?? []).map((approval) => (
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
                                {approval.expenseAmount?.toLocaleString("cs-CZ") ?? 0} {approval.expenseCurrency ?? "CZK"}
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
                    {(userDetail.invitations?.length ?? 0) === 0 ? (
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
                          {(userDetail.invitations ?? []).map((invitation) => (
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
          <div className="text-center py-8 text-muted-foreground">{t("users.loadError")}</div>
        )}
      </DialogContent>

      {/* Deactivate/Reactivate Confirmation Dialog */}
      <AlertDialog open={showDeactivateDialog} onOpenChange={setShowDeactivateDialog}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle className="flex items-center gap-2">
              {userDetail?.isActive ? <UserX className="h-5 w-5" /> : <UserCheck className="h-5 w-5" />}
              {userDetail?.isActive ? t("workplaces.deactivate") + "?" : t("workplaces.activate") + "?"}
            </AlertDialogTitle>
            <AlertDialogDescription className="space-y-2">
              {userDetail?.isActive ? (
                <>
                  <p>Deaktivací uživatele:</p>
                  <ul className="list-disc list-inside ml-4 space-y-1">
                    <li>Uživatel se nebude moci přihlásit</li>
                    <li>Všechna data zůstanou zachována</li>
                    <li>Uživatele můžete kdykoliv znovu aktivovat</li>
                  </ul>
                </>
              ) : (
                <>
                  <p>Aktivací uživatele:</p>
                  <ul className="list-disc list-inside ml-4 space-y-1">
                    <li>Uživatel se bude moci znovu přihlásit</li>
                    <li>Přístup ke všem datům bude obnoven</li>
                  </ul>
                </>
              )}
              <p className="font-semibold mt-4">Chcete pokračovat?</p>
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel disabled={isDeactivating}>{t("common.cancel")}</AlertDialogCancel>
            <AlertDialogAction onClick={handleDeactivateUser} disabled={isDeactivating}>
              {isDeactivating ? (
                <>
                  <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                  {t("common.loading")}
                </>
              ) : userDetail?.isActive ? (
                <>
                  <UserX className="h-4 w-4 mr-2" />
                  {t("common.yes")}, {t("workplaces.deactivate").toLowerCase()}
                </>
              ) : (
                <>
                  <UserCheck className="h-4 w-4 mr-2" />
                  {t("common.yes")}, {t("workplaces.activate").toLowerCase()}
                </>
              )}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </Dialog>
  );
}
