import { useEffect, useState } from "react";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Loader2, AlertTriangle, Receipt, DollarSign } from "lucide-react";
import { categoriesApi } from "@/lib/proxy/api";
import type { CategoryDependencies } from "@/lib/proxy/types";

interface CategoryDeleteModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  categoryId: string | null;
  categoryName: string;
  onConfirm: () => void;
}

export function CategoryDeleteModal({ open, onOpenChange, categoryId, categoryName, onConfirm }: CategoryDeleteModalProps) {
  const [dependencies, setDependencies] = useState<CategoryDependencies | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);

  useEffect(() => {
    if (open && categoryId) {
      loadDependencies();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, categoryId]);

  const loadDependencies = async () => {
    if (!categoryId) return;

    try {
      setIsLoading(true);
      const data = await categoriesApi.getCategoryDependencies(categoryId);
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
          <DialogTitle>Smazat kategorii</DialogTitle>
          <DialogDescription>Opravdu chcete smazat kategorii "{categoryName}"?</DialogDescription>
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
                  <AlertDescription>Kategorii nelze smazat, protože má závislosti. Nejprve odeberte následující položky:</AlertDescription>
                </Alert>
              )}

              <div className="space-y-3">
                {dependencies.expensesCount > 0 && (
                  <div className="flex items-center gap-3 p-3 bg-muted rounded-lg">
                    <Receipt className="h-5 w-5 text-muted-foreground" />
                    <div className="flex-1">
                      <div className="font-medium">Výdaje</div>
                      <div className="text-sm text-muted-foreground">
                        {dependencies.expensesCount} výdaj{dependencies.expensesCount === 1 ? "" : dependencies.expensesCount < 5 ? "e" : "ů"} používá tuto
                        kategorii
                      </div>
                    </div>
                    <div className="text-2xl font-bold text-destructive">{dependencies.expensesCount}</div>
                  </div>
                )}

                {dependencies.limitsCount > 0 && (
                  <div className="flex items-center gap-3 p-3 bg-muted rounded-lg">
                    <DollarSign className="h-5 w-5 text-muted-foreground" />
                    <div className="flex-1">
                      <div className="font-medium">Limity rozpočtu</div>
                      <div className="text-sm text-muted-foreground">
                        {dependencies.limitsCount} limit{dependencies.limitsCount === 1 ? "" : dependencies.limitsCount < 5 ? "y" : "ů"} nakonfigurován
                        {dependencies.limitsCount === 1 ? "" : dependencies.limitsCount < 5 ? "y" : "o"}
                      </div>
                    </div>
                    <div className="text-2xl font-bold text-destructive">{dependencies.limitsCount}</div>
                  </div>
                )}

                {dependencies.canDelete && (
                  <Alert>
                    <AlertDescription>Tato kategorie nemá žádné závislosti a může být bezpečně smazána.</AlertDescription>
                  </Alert>
                )}
              </div>

              {!dependencies.canDelete && (
                <Alert>
                  <AlertDescription className="text-sm">
                    <strong>Pro smazání této kategorie:</strong>
                    <ol className="mt-2 ml-4 list-decimal space-y-1">
                      {dependencies.expensesCount > 0 && (
                        <li>
                          Změňte kategorii u všech {dependencies.expensesCount} výdaj{dependencies.expensesCount === 1 ? "u" : "ů"} nebo je smažte
                        </li>
                      )}
                      {dependencies.limitsCount > 0 && (
                        <li>
                          Smažte všechny {dependencies.limitsCount} limit{dependencies.limitsCount === 1 ? "" : dependencies.limitsCount < 5 ? "y" : "ů"}{" "}
                          rozpočtu
                        </li>
                      )}
                    </ol>
                    <p className="mt-2">
                      <strong>Alternativa:</strong> Můžete kategorii deaktivovat místo smazání. Deaktivovaná kategorie zůstane u existujících výdajů, ale nebude
                      dostupná pro nové výdaje.
                    </p>
                  </AlertDescription>
                </Alert>
              )}
            </div>
          ) : (
            <Alert variant="destructive">
              <AlertTriangle className="h-4 w-4" />
              <AlertDescription>Nepodařilo se načíst informace o závislostech</AlertDescription>
            </Alert>
          )}
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={handleClose} disabled={isDeleting}>
            Zrušit
          </Button>
          <Button variant="destructive" onClick={handleDelete} disabled={!dependencies?.canDelete || isDeleting}>
            {isDeleting ? (
              <>
                <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                Mazání...
              </>
            ) : (
              "Smazat kategorii"
            )}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
