import { useEffect, useState } from "react";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Loader2, AlertTriangle, Users, DollarSign, Mail, Receipt } from "lucide-react";
import { workplacesApi } from "@/lib/proxy/api";
import type { WorkplaceDependencies } from "@/lib/proxy/types";

interface WorkplaceDeleteModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  workplaceId: string | null;
  workplaceName: string;
  onConfirm: () => void;
}

export function WorkplaceDeleteModal({ open, onOpenChange, workplaceId, workplaceName, onConfirm }: WorkplaceDeleteModalProps) {
  const [dependencies, setDependencies] = useState<WorkplaceDependencies | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);

  useEffect(() => {
    if (open && workplaceId) {
      loadDependencies();
    }
  }, [open, workplaceId]);

  const loadDependencies = async () => {
    if (!workplaceId) return;

    try {
      setIsLoading(true);
      const data = await workplacesApi.getWorkplaceDependencies(workplaceId);
      setDependencies(data);
    } catch (error) {
      console.error("Failed to load dependencies:", error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleDelete = async () => {
    if (!dependencies?.canDelete) return;

    setIsDeleting(true);
    onConfirm();
  };

  const handleClose = () => {
    if (!isDeleting) {
      onOpenChange(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>Delete Workplace</DialogTitle>
          <DialogDescription>Are you sure you want to delete workplace "{workplaceName}"?</DialogDescription>
        </DialogHeader>

        <div className="py-4">
          {isLoading ? (
            <div className="flex justify-center py-8">
              <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
            </div>
          ) : dependencies ? (
            <div className="space-y-4">
              {!dependencies.canDelete && (
                <Alert variant="destructive">
                  <AlertTriangle className="h-4 w-4" />
                  <AlertDescription>Cannot delete this workplace because it has dependencies. Please remove the following items first:</AlertDescription>
                </Alert>
              )}

              <div className="space-y-3">
                {dependencies.membersCount > 0 && (
                  <div className="flex items-center gap-3 p-3 bg-muted rounded-lg">
                    <Users className="h-5 w-5 text-muted-foreground" />
                    <div className="flex-1">
                      <div className="font-medium">Members</div>
                      <div className="text-sm text-muted-foreground">
                        {dependencies.membersCount} member{dependencies.membersCount !== 1 ? "s" : ""} assigned
                      </div>
                    </div>
                    <div className="text-2xl font-bold text-destructive">{dependencies.membersCount}</div>
                  </div>
                )}

                {dependencies.limitsCount > 0 && (
                  <div className="flex items-center gap-3 p-3 bg-muted rounded-lg">
                    <DollarSign className="h-5 w-5 text-muted-foreground" />
                    <div className="flex-1">
                      <div className="font-medium">Budget Limits</div>
                      <div className="text-sm text-muted-foreground">
                        {dependencies.limitsCount} limit{dependencies.limitsCount !== 1 ? "s" : ""} configured
                      </div>
                    </div>
                    <div className="text-2xl font-bold text-destructive">{dependencies.limitsCount}</div>
                  </div>
                )}

                {dependencies.invitationsCount > 0 && (
                  <div className="flex items-center gap-3 p-3 bg-muted rounded-lg">
                    <Mail className="h-5 w-5 text-muted-foreground" />
                    <div className="flex-1">
                      <div className="font-medium">Invitations</div>
                      <div className="text-sm text-muted-foreground">
                        {dependencies.invitationsCount} invitation{dependencies.invitationsCount !== 1 ? "s" : ""} sent
                      </div>
                    </div>
                    <div className="text-2xl font-bold text-destructive">{dependencies.invitationsCount}</div>
                  </div>
                )}

                {dependencies.expensesCount > 0 && (
                  <div className="flex items-center gap-3 p-3 bg-muted rounded-lg">
                    <Receipt className="h-5 w-5 text-muted-foreground" />
                    <div className="flex-1">
                      <div className="font-medium">Expenses</div>
                      <div className="text-sm text-muted-foreground">
                        {dependencies.expensesCount} expense{dependencies.expensesCount !== 1 ? "s" : ""} recorded
                      </div>
                    </div>
                    <div className="text-2xl font-bold text-destructive">{dependencies.expensesCount}</div>
                  </div>
                )}

                {dependencies.canDelete && (
                  <Alert>
                    <AlertDescription>This workplace has no dependencies and can be safely deleted.</AlertDescription>
                  </Alert>
                )}
              </div>

              {!dependencies.canDelete && (
                <Alert>
                  <AlertDescription className="text-sm">
                    <strong>To delete this workplace:</strong>
                    <ol className="mt-2 ml-4 list-decimal space-y-1">
                      {dependencies.membersCount > 0 && (
                        <li>
                          Remove or reassign all {dependencies.membersCount} member{dependencies.membersCount !== 1 ? "s" : ""}
                        </li>
                      )}
                      {dependencies.limitsCount > 0 && (
                        <li>
                          Delete all {dependencies.limitsCount} budget limit{dependencies.limitsCount !== 1 ? "s" : ""}
                        </li>
                      )}
                      {dependencies.invitationsCount > 0 && (
                        <li>
                          Delete all {dependencies.invitationsCount} invitation{dependencies.invitationsCount !== 1 ? "s" : ""}
                        </li>
                      )}
                      {dependencies.expensesCount > 0 && (
                        <li>
                          Delete or reassign all {dependencies.expensesCount} expense{dependencies.expensesCount !== 1 ? "s" : ""}
                        </li>
                      )}
                    </ol>
                  </AlertDescription>
                </Alert>
              )}
            </div>
          ) : null}
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={handleClose} disabled={isDeleting}>
            Cancel
          </Button>
          <Button variant="destructive" onClick={handleDelete} disabled={!dependencies?.canDelete || isDeleting}>
            {isDeleting && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            Delete Workplace
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
