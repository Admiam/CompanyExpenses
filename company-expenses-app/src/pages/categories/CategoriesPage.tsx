import { MainLayout } from "@/components/layouts/MainLayout";
import { Button } from "@/components/ui/button";
import { Plus, Trash2, Loader2, Eye, EyeOff } from "lucide-react";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { useState, useEffect } from "react";
import { CategoryFormModal } from "@/components/modals/CategoryFormModal";
import { categoriesApi } from "@/lib/proxy/api";
import type { ExpenseCategory } from "@/lib/proxy/types";
import { toast } from "sonner";

export default function CategoriesPage() {
  const [categories, setCategories] = useState<ExpenseCategory[]>([]);
  const [activeCategories, setActiveCategories] = useState<ExpenseCategory[]>([]);
  const [inactiveCategories, setInactiveCategories] = useState<ExpenseCategory[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingCategory, setEditingCategory] = useState<ExpenseCategory | null>(null);

  useEffect(() => {
    loadCategories();
  }, []);

  const loadCategories = async () => {
    try {
      setIsLoading(true);
      const data = await categoriesApi.getCategories();
      setCategories(data);
      setActiveCategories(data.filter((c) => c.isActive));
      setInactiveCategories(data.filter((c) => !c.isActive));
    } catch (error) {
      console.error("Failed to load categories:", error);
      toast.error("Failed to load categories");
    } finally {
      setIsLoading(false);
    }
  };

  const handleCreate = () => {
    setEditingCategory(null);
    setIsModalOpen(true);
  };

  const handleEdit = (category: ExpenseCategory) => {
    setEditingCategory({ ...category, color: (category as any).color || "#000000" } as ExpenseCategory);
    setIsModalOpen(true);
  };

  const handleSave = async (data: any) => {
    try {
      if (editingCategory) {
        const updateData = {
          id: editingCategory.id,
          name: data.name,
          color: data.color,
          isActive: data.isActive,
        };
        console.log("Updating category:", updateData);
        await categoriesApi.updateCategory(editingCategory.id, updateData);
        toast.success("Category updated");
      } else {
        console.log("Creating category:", data);
        await categoriesApi.createCategory(data);
        toast.success("Category created");
      }
      setIsModalOpen(false);
      loadCategories();
    } catch (error: any) {
      console.error("Failed to save category:", error);
      toast.error(error?.response?.data?.message || "Failed to save category");
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm("Are you sure you want to delete this category?")) {
      return;
    }

    try {
      await categoriesApi.deleteCategory(id);
      toast.success("Category deleted");
      loadCategories();
    } catch (error: any) {
      console.error("Failed to delete category:", error);
      toast.error(error?.response?.data?.message || "Failed to delete category");
    }
  };

  const handleDeactivate = async (id: string) => {
    if (!confirm("Are you sure you want to deactivate this category?")) {
      return;
    }

    try {
      await categoriesApi.deactivateCategory(id);
      toast.success("Category deactivated");
      loadCategories();
    } catch (error: any) {
      console.error("Failed to deactivate category:", error);
      toast.error(error?.response?.data?.message || "Failed to deactivate category");
    }
  };

  const handleActivate = async (id: string) => {
    if (!confirm("Are you sure you want to activate this category?")) {
      return;
    }

    try {
      await categoriesApi.activateCategory(id);
      toast.success("Category activated");
      loadCategories();
    } catch (error: any) {
      console.error("Failed to activate category:", error);
      toast.error(error?.response?.data?.message || "Failed to activate category");
    }
  };

  return (
    <MainLayout>
      <div className="flex flex-col gap-4 py-4 px-4 md:gap-6 md:py-6 lg:px-6">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold tracking-tight">Expense Categories</h1>
            <p className="text-muted-foreground">Manage categories for expense tracking</p>
          </div>
          <Button onClick={handleCreate}>
            <Plus className="mr-2 h-4 w-4" />
            New Category
          </Button>
        </div>

        <div className="grid gap-4 md:grid-cols-3">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Total Categories</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{categories.length}</div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Active Categories</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{categories.filter((c) => c.isActive).length}</div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Inactive Categories</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{categories.filter((c) => !c.isActive).length}</div>
            </CardContent>
          </Card>
        </div>

        <Card>
          <CardHeader>
            <CardTitle>Categories List</CardTitle>
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <div className="flex justify-center py-8">
                <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
              </div>
            ) : activeCategories.length === 0 ? (
              <div className="text-center py-8 text-muted-foreground">No categories found. Create your first category.</div>
            ) : (
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead className="w-16">Color</TableHead>
                    <TableHead className="w-32">Name</TableHead>
                    <TableHead className="w-32">Status</TableHead>
                    <TableHead className="w-32 text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {activeCategories.map((category) => (
                    <TableRow key={category.id} className="cursor-pointer" onClick={() => handleEdit(category)}>
                      <TableCell>
                        <div className="w-6 h-6 rounded-full" style={{ backgroundColor: (category as any).color || "#000000" }} />
                      </TableCell>
                      <TableCell className="font-medium">{category.name}</TableCell>
                      <TableCell>
                        <Badge variant="secondary" className={category.isActive ? "bg-green-500/10 text-green-500" : "bg-gray-500/10 text-gray-500"}>
                          {category.isActive ? "Active" : "Inactive"}
                        </Badge>
                      </TableCell>
                      <TableCell className="text-right">
                        <div className="flex justify-end gap-2">
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={(e) => {
                              e.stopPropagation();
                              handleDeactivate(category.id);
                            }}
                          >
                            <Eye className="h-4 w-4" />
                          </Button>
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={(e) => {
                              e.stopPropagation();
                              handleDelete(category.id);
                            }}
                          >
                            <Trash2 className="h-4 w-4 text-destructive" />
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

        <Card>
          <CardHeader>
            <CardTitle>Inactive Categories</CardTitle>
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <div className="flex justify-center py-8">
                <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
              </div>
            ) : inactiveCategories.length === 0 ? (
              <div className="text-center py-8 text-muted-foreground">No categories found. Create your first category.</div>
            ) : (
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead className="w-16">Color</TableHead>
                    <TableHead className="w-32">Name</TableHead>
                    <TableHead className="w-32">Status</TableHead>
                    <TableHead className="w-32 text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {inactiveCategories.map((category) => (
                    <TableRow key={category.id} className="cursor-pointer" onClick={() => handleEdit(category)}>
                      <TableCell>
                        <div className="w-6 h-6 rounded-full" style={{ backgroundColor: (category as any).color || "#000000" }} />
                      </TableCell>
                      <TableCell className="font-medium">{category.name}</TableCell>
                      <TableCell>
                        <Badge variant="secondary" className={category.isActive ? "bg-green-500/10 text-green-500" : "bg-gray-500/10 text-gray-500"}>
                          {category.isActive ? "Active" : "Inactive"}
                        </Badge>
                      </TableCell>
                      <TableCell className="text-right">
                        <div className="flex justify-end gap-2">
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={(e) => {
                              e.stopPropagation();
                              handleActivate(category.id);
                            }}
                          >
                            <EyeOff className="h-4 w-4" />
                          </Button>
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={(e) => {
                              e.stopPropagation();
                              handleDelete(category.id);
                            }}
                          >
                            <Trash2 className="h-4 w-4 text-destructive" />
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

        <CategoryFormModal
          open={isModalOpen}
          onOpenChange={setIsModalOpen}
          category={editingCategory ? { ...editingCategory, color: (editingCategory as any).color || "#000000" } : null}
          onSave={handleSave}
        />
      </div>
    </MainLayout>
  );
}
