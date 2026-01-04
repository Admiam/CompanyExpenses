import { useState, useEffect } from "react";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { invitationsApi, workplacesApi, rolesApi } from "@/lib/proxy/api";
import type { Workplace, Role } from "@/lib/proxy/types";
import { toast } from "sonner";
import { Loader2 } from "lucide-react";

interface UserInviteModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSuccess?: () => void;
}

export function UserInviteModal({ open, onOpenChange, onSuccess }: UserInviteModalProps) {
  const [formData, setFormData] = useState({
    email: "",
    invitedRoleId: "",
    workplaceId: "",
  });
  const [workplaces, setWorkplaces] = useState<Workplace[]>([]);
  const [roles, setRoles] = useState<Role[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [isSending, setIsSending] = useState(false);

  useEffect(() => {
    if (open) {
      setFormData({
        email: "",
        invitedRoleId: "",
        workplaceId: "",
      });
      loadData();
    }
  }, [open]);

  const loadData = async () => {
    try {
      setIsLoading(true);
      const [workplacesData, rolesData] = await Promise.all([workplacesApi.getWorkplaces(), rolesApi.getRoles()]);
      setWorkplaces(workplacesData);
      setRoles(rolesData);
    } catch (error) {
      console.error("Failed to load data:", error);
      toast.error("Failed to load data");
    } finally {
      setIsLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      setIsSending(true);
      await invitationsApi.createInvitation({
        email: formData.email,
        invitedRoleId: formData.invitedRoleId || undefined,
        workplaceId: formData.workplaceId || undefined,
      });

      toast.success("Invitation sent successfully");
      onOpenChange(false);
      onSuccess?.();
    } catch (error: any) {
      console.error("Failed to send invitation:", error);
      toast.error(error?.response?.data?.message || "Failed to send invitation");
    } finally {
      setIsSending(false);
    }
  };

  return (
    <div>
      <Dialog open={open} onOpenChange={onOpenChange}>
        <DialogContent className="sm:max-w-[425px]">
          <form onSubmit={handleSubmit}>
            <DialogHeader>
              <DialogTitle>Invite User</DialogTitle>
              <DialogDescription>Send an invitation to a new user</DialogDescription>
            </DialogHeader>

            <div className="grid gap-4 py-4">
              <div className="grid gap-2">
                <Label htmlFor="email">Email</Label>
                <Input
                  id="email"
                  type="email"
                  value={formData.email}
                  onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                  placeholder="user@company.com"
                  required
                  disabled={isSending}
                />
              </div>

              <div className="grid gap-2">
                <Label htmlFor="role">Role (Optional)</Label>
                <Select
                  value={formData.invitedRoleId}
                  onValueChange={(value) => setFormData({ ...formData, invitedRoleId: value })}
                  disabled={isLoading || isSending}
                >
                  <SelectTrigger id="role">
                    <SelectValue placeholder={isLoading ? "Loading..." : "Select role"} />
                  </SelectTrigger>
                  <SelectContent>
                    {roles.map((role) => (
                      <SelectItem key={role.id} value={role.id}>
                        {role.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div className="grid gap-2">
                <Label htmlFor="workplace">Workplace (Optional)</Label>
                <Select
                  value={formData.workplaceId}
                  onValueChange={(value) => setFormData({ ...formData, workplaceId: value })}
                  disabled={isLoading || isSending}
                >
                  <SelectTrigger id="workplace">
                    <SelectValue placeholder={isLoading ? "Loading..." : "Select workplace"} />
                  </SelectTrigger>
                  <SelectContent>
                    {workplaces.map((wp) => (
                      <SelectItem key={wp.id} value={wp.id}>
                        {wp.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>

            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => onOpenChange(false)} disabled={isSending}>
                Cancel
              </Button>
              <Button type="submit" disabled={isSending}>
                {isSending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                Send Invitation
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
      <DialogFooter>
        <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
          Zrušit
        </Button>
        <Button type="submit">Odeslat pozvánku</Button>
      </DialogFooter>
    </div>
  );
}
//   </div>
// </div>

//         </form>
//       </DialogContent>
//     </Dialog>
//   );
// }
