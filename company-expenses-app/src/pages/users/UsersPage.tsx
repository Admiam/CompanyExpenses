import { MainLayout } from "@/components/layouts/MainLayout";
import { Button } from "@/components/ui/button";
import { Plus, Shield, User, Loader2, RotateCw, Mail, Trash2 } from "lucide-react";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { invitationsApi, workplaceMembersApi } from "@/lib/proxy/api";
import { useEffect, useState } from "react";
import { toast } from "sonner";
import { UserInviteModal } from "@/components/modals/UserInviteModal";
import { UserDetailModal } from "@/components/modals/UserDetailModal";
import { InvitationStatus, roleLabels, roleColors } from "@/constants";
import type { Invitation, UserWithStats } from "@/lib/proxy/types";
import { getInvitationStatusLabel, getInvitationStatusIcon } from "@/utils";

export default function UsersPage() {
  const [users, setUsers] = useState<UserWithStats[]>([]);
  const [inactiveUsers, setInactiveUsers] = useState<UserWithStats[]>([]);
  const [invitations, setInvitations] = useState<Invitation[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isUsersLoading, setIsUsersLoading] = useState(true);
  const [isInactiveUsersLoading, setIsInactiveUsersLoading] = useState(false);
  const [isInviteModalOpen, setIsInviteModalOpen] = useState(false);
  const [isDetailModalOpen, setIsDetailModalOpen] = useState(false);
  const [selectedUserId, setSelectedUserId] = useState<string | null>(null);

  useEffect(() => {
    loadInvitations();
    loadUsers();
  }, []);

  const loadUsers = async () => {
    try {
      setIsUsersLoading(true);
      const data = await workplaceMembersApi.getUsersWithStats();
      setUsers(data);
    } catch (error) {
      console.error("Failed to load users:", error);
      toast.error("Failed to load users");
    } finally {
      setIsUsersLoading(false);
    }
  };

  const loadInactiveUsers = async () => {
    try {
      setIsInactiveUsersLoading(true);
      const data = await workplaceMembersApi.getInactiveUsers();
      setInactiveUsers(data);
    } catch (error) {
      console.error("Failed to load inactive users:", error);
      toast.error("Failed to load inactive users");
    } finally {
      setIsInactiveUsersLoading(false);
    }
  };

  const loadInvitations = async () => {
    try {
      setIsLoading(true);
      const data = await invitationsApi.getInvitations();
      setInvitations(data);
    } catch (error) {
      console.error("Failed to load invitations:", error);
      toast.error("Failed to load invitations");
    } finally {
      setIsLoading(false);
    }
  };

  const handleCancelInvitation = async (id: string) => {
    try {
      await invitationsApi.cancelInvitation(id);
      toast.success("Invitation cancelled");
      loadInvitations();
    } catch (error) {
      console.error("Failed to cancel invitation:", error);
      toast.error("Failed to cancel invitation");
    }
  };

  const handleResendInvitation = async (id: string) => {
    try {
      await invitationsApi.resendInvitation(id);
      toast.success("Invitation resent");
      loadInvitations();
    } catch (error) {
      console.error("Failed to resend invitation:", error);
      toast.error("Failed to resend invitation");
    }
  };

  const handleDeleteInvitation = async (id: string) => {
    if (!confirm("Are you sure you want to permanently delete this invitation?")) {
      return;
    }
    try {
      await invitationsApi.deleteInvitation(id);
      toast.success("Invitation deleted");
      loadInvitations();
    } catch (error) {
      console.error("Failed to delete invitation:", error);
      toast.error("Failed to delete invitation");
    }
  };

  const handleInvitationCreated = () => {
    setIsInviteModalOpen(false);
    loadInvitations();
  };

  const handleRowClick = (userId: string) => {
    setSelectedUserId(userId);
    setIsDetailModalOpen(true);
  };

  const handleUserDeleted = () => {
    loadUsers();
    loadInactiveUsers();
  };

  return (
    <MainLayout>
      <div className="flex flex-col gap-4 py-4 px-4 md:gap-6 md:py-6 lg:px-6">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold tracking-tight">Users</h1>
            <p className="text-muted-foreground">User and invitation management</p>
          </div>
          <Button onClick={() => setIsInviteModalOpen(true)}>
            <Plus className="mr-2 h-4 w-4" />
            Invite User
          </Button>
        </div>

        {/* Stats */}
        <div className="grid gap-4 md:grid-cols-4">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Total Users</CardTitle>
              <User className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{users.length}</div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Administrators</CardTitle>
              <Shield className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{users.filter((u) => u.role === "admin").length}</div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Managers</CardTitle>
              <Shield className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{users.filter((u) => u.role === "manager").length}</div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Pending Invitations</CardTitle>
              <Mail className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{invitations.filter((i) => i.status === InvitationStatus.Pending).length}</div>
            </CardContent>
          </Card>
        </div>

        {/* Tabs */}
        <Tabs
          defaultValue="users"
          className="space-y-4"
          onValueChange={(value) => {
            if (value === "inactive" && inactiveUsers.length === 0) {
              loadInactiveUsers();
            }
          }}
        >
          <TabsList>
            <TabsTrigger value="users">Aktivní uživatelé</TabsTrigger>
            <TabsTrigger value="inactive">
              Neaktivní uživatelé
              {inactiveUsers.length > 0 && (
                <Badge variant="secondary" className="ml-2">
                  {inactiveUsers.length}
                </Badge>
              )}
            </TabsTrigger>
            <TabsTrigger value="invitations">
              Pozvánky
              {invitations.filter((i) => i.status === InvitationStatus.Pending).length > 0 && (
                <Badge variant="secondary" className="ml-2">
                  {invitations.filter((i) => i.status === InvitationStatus.Pending).length}
                </Badge>
              )}
            </TabsTrigger>
          </TabsList>

          {/* Users Tab */}
          <TabsContent value="users" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle>Seznam uživatelů</CardTitle>
                <CardDescription>Přehled všech aktivních uživatelů v systému</CardDescription>
              </CardHeader>
              <CardContent>
                {isUsersLoading ? (
                  <div className="flex justify-center py-8">
                    <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
                  </div>
                ) : users.length === 0 ? (
                  <div className="text-center py-8 text-muted-foreground">Žádní uživatelé nenalezeni</div>
                ) : (
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Uživatel</TableHead>
                        <TableHead>Role</TableHead>
                        <TableHead>Pracoviště</TableHead>
                        <TableHead className="text-right">Počet výdajů</TableHead>
                        <TableHead className="text-right">Celkem výdajů</TableHead>
                        <TableHead className="text-right">Akce</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {users.map((user) => (
                        <TableRow key={user.id} className="cursor-pointer hover:bg-muted/50" onClick={() => handleRowClick(user.id)}>
                          <TableCell>
                            <div className="flex items-center gap-3">
                              <Avatar className="h-8 w-8">
                                <AvatarFallback>
                                  {user.name
                                    ?.split(" ")
                                    .map((n) => n[0])
                                    .join("") || user.email.substring(0, 2).toUpperCase()}
                                </AvatarFallback>
                              </Avatar>
                              <div>
                                <div className="font-medium">{user.name || user.email}</div>
                                <div className="text-sm text-muted-foreground">{user.email}</div>
                              </div>
                            </div>
                          </TableCell>
                          <TableCell>
                            <Badge variant="secondary" className={roleColors[user.role as keyof typeof roleColors]}>
                              {roleLabels[user.role as keyof typeof roleLabels]}
                            </Badge>
                          </TableCell>
                          <TableCell>
                            {user.workplace && user.workplace !== "N/A" ? (
                              user.workplace
                            ) : (
                              <span className="text-muted-foreground italic">Všechna pracoviště</span>
                            )}
                          </TableCell>
                          <TableCell className="text-right">{user.expenseCount}</TableCell>
                          <TableCell className="text-right">{user.totalExpenses.toLocaleString("cs-CZ")} Kč</TableCell>
                          <TableCell className="text-right" onClick={(e) => e.stopPropagation()}>
                            <Button variant="ghost" size="sm" onClick={() => handleRowClick(user.id)}>
                              Detail
                            </Button>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                )}
              </CardContent>
            </Card>
          </TabsContent>

          {/* Inactive Users Tab */}
          <TabsContent value="inactive" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle>Neaktivní uživatelé</CardTitle>
                <CardDescription>Přehled deaktivovaných uživatelů v systému</CardDescription>
              </CardHeader>
              <CardContent>
                {isInactiveUsersLoading ? (
                  <div className="flex justify-center py-8">
                    <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
                  </div>
                ) : inactiveUsers.length === 0 ? (
                  <div className="text-center py-8 text-muted-foreground">Žádní neaktivní uživatelé</div>
                ) : (
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Uživatel</TableHead>
                        <TableHead>Role</TableHead>
                        <TableHead>Pracoviště</TableHead>
                        <TableHead className="text-right">Počet výdajů</TableHead>
                        <TableHead className="text-right">Celkem výdajů</TableHead>
                        <TableHead className="text-right">Akce</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {inactiveUsers.map((user) => (
                        <TableRow key={user.id} className="cursor-pointer hover:bg-muted/50 opacity-60" onClick={() => handleRowClick(user.id)}>
                          <TableCell>
                            <div className="flex items-center gap-3">
                              <Avatar className="h-8 w-8">
                                <AvatarFallback>
                                  {user.name
                                    ?.split(" ")
                                    .map((n) => n[0])
                                    .join("") || user.email.substring(0, 2).toUpperCase()}
                                </AvatarFallback>
                              </Avatar>
                              <div>
                                <div className="font-medium">{user.name || user.email}</div>
                                <div className="text-sm text-muted-foreground">{user.email}</div>
                              </div>
                            </div>
                          </TableCell>
                          <TableCell>
                            <Badge variant="secondary" className={roleColors[user.role as keyof typeof roleColors]}>
                              {roleLabels[user.role as keyof typeof roleLabels]}
                            </Badge>
                          </TableCell>
                          <TableCell>
                            {user.workplace && user.workplace !== "N/A" ? (
                              user.workplace
                            ) : (
                              <span className="text-muted-foreground italic">Všechna pracoviště</span>
                            )}
                          </TableCell>
                          <TableCell className="text-right">{user.expenseCount}</TableCell>
                          <TableCell className="text-right">{user.totalExpenses.toLocaleString("cs-CZ")} Kč</TableCell>
                          <TableCell className="text-right" onClick={(e) => e.stopPropagation()}>
                            <Button variant="ghost" size="sm" onClick={() => handleRowClick(user.id)}>
                              Detail
                            </Button>
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
                <CardTitle>Invitations</CardTitle>
                <CardDescription>Overview of sent invitations</CardDescription>
              </CardHeader>
              <CardContent>
                {isLoading ? (
                  <div className="flex justify-center py-8">
                    <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
                  </div>
                ) : invitations.length === 0 ? (
                  <div className="text-center py-8 text-muted-foreground">No invitations found</div>
                ) : (
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Email</TableHead>
                        <TableHead>Workplace</TableHead>
                        <TableHead>Created</TableHead>
                        <TableHead>Expires</TableHead>
                        <TableHead>Status</TableHead>
                        <TableHead className="text-right">Actions</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {invitations.map((invitation) => (
                        <TableRow key={invitation.id}>
                          <TableCell className="font-medium">{invitation.email}</TableCell>
                          <TableCell>
                            {invitation.workplace?.name && invitation.workplace.name !== "N/A" ? (
                              invitation.workplace.name
                            ) : (
                              <span className="text-muted-foreground italic">Všechna pracoviště</span>
                            )}
                          </TableCell>
                          <TableCell>{new Date(invitation.createdAt).toLocaleDateString()}</TableCell>
                          <TableCell>{new Date(invitation.expiresAt).toLocaleDateString()}</TableCell>
                          <TableCell>
                            <div className="flex items-center gap-2">
                              {getInvitationStatusIcon(invitation.status)}
                              <span className="text-sm">{getInvitationStatusLabel(invitation.status)}</span>
                            </div>
                          </TableCell>
                          <TableCell className="text-right">
                            <div className="flex justify-end gap-2">
                              {invitation.status === InvitationStatus.Pending && (
                                <>
                                  <Button variant="ghost" size="sm" onClick={() => handleResendInvitation(invitation.id)}>
                                    <RotateCw className="h-4 w-4 mr-1" />
                                    Resend
                                  </Button>
                                  <Button variant="ghost" size="sm" onClick={() => handleCancelInvitation(invitation.id)}>
                                    Cancel
                                  </Button>
                                </>
                              )}
                              <Button
                                variant="ghost"
                                size="sm"
                                onClick={() => handleDeleteInvitation(invitation.id)}
                                className="text-destructive hover:text-destructive"
                              >
                                <Trash2 className="h-4 w-4" />
                              </Button>
                            </div>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                )}
              </CardContent>
            </Card>
          </TabsContent>
        </Tabs>

        <UserInviteModal open={isInviteModalOpen} onOpenChange={setIsInviteModalOpen} onSuccess={handleInvitationCreated} />
        <UserDetailModal open={isDetailModalOpen} onOpenChange={setIsDetailModalOpen} userId={selectedUserId} onUserDeleted={handleUserDeleted} />
      </div>
    </MainLayout>
  );
}
