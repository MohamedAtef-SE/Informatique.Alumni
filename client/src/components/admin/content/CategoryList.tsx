import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../../services/adminService';
import { Plus, Edit, Trash2 } from 'lucide-react';
import type { ArticleCategoryDto } from '../../../types/news';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../ui/Table';
import { Button } from '../../ui/Button';
import { DataTableShell } from '../../admin/DataTableShell';
import { StatusBadge } from '../../admin/StatusBadge';
import { CategoryModal } from './CategoryModal';
import { ConfirmDialog } from '../../ui/ConfirmDialog';
import { toast } from 'sonner';

export function CategoryList() {
    const queryClient = useQueryClient();
    const [page, setPage] = useState(1);
    const pageSize = 10;

    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingCategory, setEditingCategory] = useState<ArticleCategoryDto | null>(null);

    const [isDeleteConfirmOpen, setIsDeleteConfirmOpen] = useState(false);
    const [categoryToDelete, setCategoryToDelete] = useState<ArticleCategoryDto | null>(null);

    const { data, isLoading } = useQuery({
        queryKey: ['admin-content-categories', page],
        queryFn: () => adminService.getContentCategories({
            skipCount: (page - 1) * pageSize,
            maxResultCount: pageSize
        })
    });

    const deleteMutation = useMutation({
        mutationFn: adminService.deleteContentCategory,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-content-categories'] });
            queryClient.invalidateQueries({ queryKey: ['article-categories-lookup'] });
            toast.success('Category deleted');
            setIsDeleteConfirmOpen(false);
        },
        onError: () => {
            toast.error('Failed to delete category');
        }
    });

    const handleEditClick = (category: ArticleCategoryDto) => {
        setEditingCategory(category);
        setIsModalOpen(true);
    };

    const handleDeleteClick = (category: ArticleCategoryDto) => {
        setCategoryToDelete(category);
        setIsDeleteConfirmOpen(true);
    };

    return (
        <div className="space-y-4">
            <div className="flex justify-between items-center">
                <h2 className="text-xl font-semibold text-white">Categories</h2>
                <Button onClick={() => {
                    setEditingCategory(null);
                    setIsModalOpen(true);
                }} size="sm">
                    <Plus className="w-4 h-4 mr-2" /> Add Category
                </Button>
            </div>

            <DataTableShell
                pagination={{
                    currentPage: page,
                    totalPages: Math.ceil((data?.totalCount || 0) / pageSize),
                    onPageChange: setPage
                }}
            >
                <Table>
                    <TableHeader>
                        <TableRow>
                            <TableHead>Name (EN)</TableHead>
                            <TableHead>Name (AR)</TableHead>
                            <TableHead>Status</TableHead>
                            <TableHead className="text-right">Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {isLoading ? (
                            <TableRow>
                                <TableCell colSpan={4} className="h-32 text-center">
                                    <div className="flex flex-col items-center justify-center gap-3">
                                        <div className="relative flex h-10 w-10 items-center justify-center">
                                            <div className="absolute h-full w-full rounded-full border-2 border-accent/20 animate-ping" />
                                            <div className="h-5 w-5 rotate-45 rounded-sm bg-accent animate-pulse" />
                                        </div>
                                    </div>
                                </TableCell>
                            </TableRow>
                        ) : data?.items.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={4} className="h-32 text-center text-slate-500">No categories found.</TableCell>
                            </TableRow>
                        ) : (
                            data?.items.map((cat: ArticleCategoryDto) => (
                                <TableRow key={cat.id}>
                                    <TableCell className="font-medium text-slate-200">{cat.nameEn}</TableCell>
                                    <TableCell className="text-slate-300 font-arabic">{cat.nameAr}</TableCell>
                                    <TableCell>
                                        <StatusBadge variant={cat.isActive ? 'success' : 'secondary'}>
                                            {cat.isActive ? 'Active' : 'Inactive'}
                                        </StatusBadge>
                                    </TableCell>
                                    <TableCell className="text-right">
                                        <div className="flex justify-end gap-2">
                                            <Button
                                                size="icon"
                                                variant="ghost"
                                                className="text-slate-400 hover:text-white"
                                                onClick={() => handleEditClick(cat)}
                                            >
                                                <Edit className="w-4 h-4" />
                                            </Button>
                                            <Button
                                                size="icon"
                                                variant="ghost"
                                                className="text-red-400 hover:text-red-300"
                                                onClick={() => handleDeleteClick(cat)}
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

            <CategoryModal
                open={isModalOpen}
                onOpenChange={setIsModalOpen}
                category={editingCategory}
            />

            <ConfirmDialog
                open={isDeleteConfirmOpen}
                onOpenChange={setIsDeleteConfirmOpen}
                title="Delete Category"
                description={`Are you sure you want to delete "${categoryToDelete?.nameEn}"? This may affect posts assigned to this category.`}
                confirmLabel="Delete"
                variant="danger"
                onConfirm={() => categoryToDelete && deleteMutation.mutate(categoryToDelete.id)}
                isLoading={deleteMutation.isPending}
            />
        </div>
    );
}
