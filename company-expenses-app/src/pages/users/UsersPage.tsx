import { MainLayout } from "@/components/layouts/MainLayout";
import { Button } from "@/components/ui/button";
import { Plus, Mail, Shield, User, CheckCircle, XCircle, Loader2, RotateCw } from "lucide-react";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { invitationsApi } from "@/lib/proxy/api";
// import { type Invitation, InvitationStatus } from "./types";
import { useEffect, useState } from "react";
import { toast } from "sonner";
import { UserInviteModal } from "@/components/modals/UserInviteModal";

const InvitationStatus = {
  Pending: 0,
  Accepted: 1,
  Declined: 2,
  Expired: 3,
  Cancelled: 4,
} as const;

type InvitationStatus = (typeof InvitationStatus)[keyof typeof InvitationStatus];

export interface Workplace {
  id: string;
  name: string;
  description?: string;
  address?: string;
  isActive: boolean;
}

export interface Invitation {
  id: string;
  email: string;
  invitedRoleId?: string;
  workplaceId?: string;
  token: string;
  expiresAt: string;
  acceptedAt?: string;
  invitedByUserId: string;
  status: InvitationStatus;
  createdAt: string;
  createdBy: string;
  workplace?: Workplace;
}

// Mock data
const mockUsers = [
  {
    id: "1",
    name: "Jan Novák",
    email: "jan.novak@firma.cz",
    role: "admin",
    workplace: "Praha - Centrála",
    status: "active",
    expenseCount: 12,
    totalExpenses: 45000,
  },
  {
    id: "2",
    name: "Marie Svobodová",
    email: "marie.svobodova@firma.cz",
    role: "manager",
    workplace: "Brno - Pobočka",
    status: "active",
    expenseCount: 8,
    totalExpenses: 28000,
  },
  {
    id: "3",
    name: "Petr Dvořák",
    email: "petr.dvorak@firma.cz",
    role: "employee",
    workplace: "Praha - Centrála",
    status: "active",
    expenseCount: 5,
    totalExpenses: 12500,
  },
];

const roleLabels = {
  admin: "Administrátor",
  manager: "Manažer",
  employee: "Zaměstnanec",
};

const roleColors = {
  admin: "bg-purple-500/10 text-purple-500",
  manager: "bg-blue-500/10 text-blue-500",
  employee: "bg-gray-500/10 text-gray-500",
};

const getStatusLabel = (status: InvitationStatus): string => {
  switch (status) {
    case InvitationStatus.Pending:
      return "Pending";
    case InvitationStatus.Accepted:
      return "Accepted";
    case InvitationStatus.Declined:
      return "Declined";
    case InvitationStatus.Expired:
      return "Expired";
    case InvitationStatus.Cancelled:
      return "Cancelled";
    default:
      return "Unknown";
  }
};

const getStatusIcon = (status: InvitationStatus) => {
  switch (status) {
    case InvitationStatus.Pending:
      return <Mail className="h-4 w-4 text-yellow-500" />;
    case InvitationStatus.Accepted:
      return <CheckCircle className="h-4 w-4 text-green-500" />;
    case InvitationStatus.Declined:
    case InvitationStatus.Cancelled:
      return <XCircle className="h-4 w-4 text-red-500" />;
    case InvitationStatus.Expired:
      return <XCircle className="h-4 w-4 text-gray-500" />;
    default:
      return null;
  }
};

export default function UsersPage() {
  const [invitations, setInvitations] = useState<Invitation[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isInviteModalOpen, setIsInviteModalOpen] = useState(false);

  useEffect(() => {
    loadInvitations();
  }, []);

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

  const handleInvitationCreated = () => {
    setIsInviteModalOpen(false);
    loadInvitations();
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
              <div className="text-2xl font-bold">{mockUsers.length}</div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Administrators</CardTitle>
              <Shield className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{mockUsers.filter((u) => u.role === "admin").length}</div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Managers</CardTitle>
              <Shield className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{mockUsers.filter((u) => u.role === "manager").length}</div>
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
        <Tabs defaultValue="users" className="space-y-4">
          <TabsList>
            <TabsTrigger value="users">Active Users</TabsTrigger>
            <TabsTrigger value="invitations">
              Invitations
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
                    {mockUsers.map((user) => (
                      <TableRow key={user.id} className="cursor-pointer hover:bg-muted/50">
                        <TableCell>
                          <div className="flex items-center gap-3">
                            <Avatar className="h-8 w-8">
                              <AvatarFallback>
                                {user.name
                                  .split(" ")
                                  .map((n) => n[0])
                                  .join("")}
                              </AvatarFallback>
                            </Avatar>
                            <div>
                              <div className="font-medium">{user.name}</div>
                              <div className="text-sm text-muted-foreground">{user.email}</div>
                            </div>
                          </div>
                        </TableCell>
                        <TableCell>
                          <Badge variant="secondary" className={roleColors[user.role as keyof typeof roleColors]}>
                            {roleLabels[user.role as keyof typeof roleLabels]}
                          </Badge>
                        </TableCell>
                        <TableCell>{user.workplace}</TableCell>
                        <TableCell className="text-right">{user.expenseCount}</TableCell>
                        <TableCell className="text-right">{user.totalExpenses.toLocaleString("cs-CZ")} Kč</TableCell>
                        <TableCell className="text-right">
                          <Button variant="ghost" size="sm">
                            Detail
                          </Button>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
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
                          <TableCell>{invitation.workplace?.name || "N/A"}</TableCell>
                          <TableCell>{new Date(invitation.createdAt).toLocaleDateString()}</TableCell>
                          <TableCell>{new Date(invitation.expiresAt).toLocaleDateString()}</TableCell>
                          <TableCell>
                            <div className="flex items-center gap-2">
                              {getStatusIcon(invitation.status)}
                              <span className="text-sm">{getStatusLabel(invitation.status)}</span>
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
      </div>
    </MainLayout>
  );
}
