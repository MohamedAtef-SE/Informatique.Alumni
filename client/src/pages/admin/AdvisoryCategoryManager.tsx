import { useState } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from 'react-oidc-context';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { LucideLightbulb, Trash2, Edit2, Plus, CheckCircle2, XCircle } from 'lucide-react';
import { PageHeader } from '../../components/admin/PageHeader';
import { DataTableShell } from '../../components/admin/DataTableShell';
import { Button } from '../../components/ui/Button';
import { AdvisoryCategoryModal } from '../../components/admin/guidance/AdvisoryCategoryModal';
import { ConfirmDialog } from '../../components/ui/ConfirmDialog';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/Table';
import { toast } from 'sonner';
import { advisoryCategoryService } from '../../services/advisoryCategoryService';
import type { AdvisoryCategoryDto } from '../../types/guidance';

const AdvisoryCategoryManager = () => {
    const queryClient = useQueryClient();
    const [page, setPage] = useState(1);
    const pageSize = 10;
    const [filter, setFilter] = useState('');
    const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
    const [categoryToDelete, setCategoryToDelete] = useState<string | null>(null);
    const [categoryToEdit, setCategoryToEdit] = useState<AdvisoryCategoryDto | null>(null);

    // Auth Check
    const auth = useAuth();
    const roles = auth.user?.profile?.role;
    const roleArray = Array.isArray(roles) ? roles : roles ? [String(roles)] : [];
    const isSuperAdmin = roleArray.some(r => ['admin', 'systemadmin'].includes(r.toLowerCase()));

    if (!isSuperAdmin) {
        return <Navigate to="/admin/dashboard" replace />;
    }

    const { data, isLoading } = useQuery({
        queryKey: ['admin-advisory-categories', page, filter],
        queryFn: () => advisoryCategoryService.getList({
            skipCount: (page - 1) * pageSize,
            maxResultCount: pageSize,
            sorting: 'nameEn',
            filter
        })
    });

    const deleteMutation = useMutation({
        mutationFn: advisoryCategoryService.delete,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-advisory-categories'] });
            toast.success('Category deleted successfully');
        },
        onError: () => toast.error('Failed to delete category')
    });

    const handleDelete = (id: string) => {
        setCategoryToDelete(id);
    };

    const confirmDelete = () => {
        if (categoryToDelete) {
            deleteMutation.mutate(categoryToDelete, {
                onSettled: () => setCategoryToDelete(null)
            });
        }
    };

    return (
        <div className="space-y-8 animate-in fade-in duration-500">
            <PageHeader
                title="Advisory Categories"
                description="Manage professional specializations for alumni advisors."
                action={
                    <Button className="shadow-neon" onClick={() => setIsCreateModalOpen(true)}>
                        <Plus className="w-4 h-4 mr-2" /> New Category
                    </Button>
                }
            />

            <DataTableShell
                searchPlaceholder="Search categories..."
                onSearch={setFilter}
                pagination={{
                    currentPage: page,
                    totalPages: Math.ceil((data?.totalCount || 0) / pageSize),
                    onPageChange: setPage
                }}
            >
                <Table>
                    <TableHeader>
                        <TableRow className="hover:bg-transparent border-white/5">
                            <TableHead>Category Name (EN)</TableHead>
                            <TableHead>Category Name (AR)</TableHead>
                            <TableHead>Status</TableHead>
                            <TableHead className="text-right">Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {isLoading ? (
                            <TableRow>
                                <TableCell colSpan={4} className="h-32 text-center text-slate-400">Loading categories...</TableCell>
                            </TableRow>
                        ) : data?.items.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={4} className="h-32 text-center text-slate-500">
                                    No categories found.
                                </TableCell>
                            </TableRow>
                        ) : (
                            data?.items.map((cat: AdvisoryCategoryDto) => (
                                <TableRow key={cat.id} className="group">
                                    <TableCell className="font-medium text-slate-900 dark:text-white">
                                        <div className="flex items-center gap-2">
                                            <LucideLightbulb className="w-4 h-4 text-indigo-500" />
                                            <span>{cat.nameEn}</span>
                                        </div>
                                    </TableCell>
                                    <TableCell className="text-slate-600 dark:text-slate-300 font-arabic">
                                        {cat.nameAr}
                                    </TableCell>
                                    <TableCell>
                                        {cat.isActive ? (
                                            <div className="flex items-center gap-1.5 text-emerald-500 text-sm">
                                                <CheckCircle2 className="w-4 h-4" />
                                                <span>Active</span>
                                            </div>
                                        ) : (
                                            <div className="flex items-center gap-1.5 text-slate-400 text-sm">
                                                <XCircle className="w-4 h-4" />
                                                <span>Inactive</span>
                                            </div>
                                        )}
                                    </TableCell>
                                    <TableCell className="text-right">
                                        <div className="flex items-center justify-end gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                                            <Button
                                                size="icon"
                                                variant="ghost"
                                                className="text-blue-400 hover:text-blue-300 hover:bg-blue-500/20"
                                                onClick={() => setCategoryToEdit(cat)}
                                            >
                                                <Edit2 className="w-4 h-4" />
                                            </Button>
                                            <Button
                                                size="icon"
                                                variant="ghost"
                                                className="text-red-400 hover:text-red-300 hover:bg-red-500/20"
                                                onClick={() => handleDelete(cat.id)}
                                            >
                                                <Trash2 className="w-4 h-4" />
                                            </Button>
                                        </div>
                                    </TableCell>
                                </TableRow>
                            ))
                        )}
                    </TableBody>
                </Table>
            </DataTableShell>

            <AdvisoryCategoryModal
                open={isCreateModalOpen || !!categoryToEdit}
                onOpenChange={(open: boolean) => {
                    if (!open) {
                        setIsCreateModalOpen(false);
                        setCategoryToEdit(null);
                    }
                }}
                category={categoryToEdit}
            />

            <ConfirmDialog
                open={!!categoryToDelete}
                onOpenChange={(open) => !open && setCategoryToDelete(null)}
                title="Delete Category"
                description="Are you sure you want to delete this advisory category? Professional advisor specializations using this category will be preserved but the category will no longer be available for new applications."
                confirmLabel="Delete"
                variant="danger"
                onConfirm={confirmDelete}
                isLoading={deleteMutation.isPending}
            />
        </div>
    );
};

export default AdvisoryCategoryManager;
