import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { Plus, Edit, Trash2, Tags } from 'lucide-react';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/Table';
import { Button } from '../../components/ui/Button';
import { toast } from 'sonner';
import { PageHeader } from '../../components/admin/PageHeader';
import { DataTableShell } from '../../components/admin/DataTableShell';
import { StatusBadge } from '../../components/admin/StatusBadge';
import { CreateCareerServiceTypeModal } from '../../components/admin/career/CreateCareerServiceTypeModal';
import { EditCareerServiceTypeModal } from '../../components/admin/career/EditCareerServiceTypeModal';
import { ConfirmDialog } from '../../components/ui/ConfirmDialog';
import type { CareerServiceTypeDto } from '../../types/career';

const CareerServiceTypeManager = () => {
    const queryClient = useQueryClient();
    const [filter, setFilter] = useState('');
    const [page, setPage] = useState(1);
    const pageSize = 10;
    const [isActiveFilter, setIsActiveFilter] = useState<'all' | 'active' | 'inactive'>('all');
    const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
    const [isEditModalOpen, setIsEditModalOpen] = useState(false);
    const [selectedType, setSelectedType] = useState<CareerServiceTypeDto | null>(null);
    const [typeToDelete, setTypeToDelete] = useState<{ id: string, name: string } | null>(null);

    const { data, isLoading } = useQuery({
        queryKey: ['admin-career-types', filter, page, isActiveFilter],
        queryFn: async () => {
            const result = await adminService.getCareerServiceTypes({
                sorting: 'nameEn asc',
                skipCount: (page - 1) * pageSize,
                maxResultCount: pageSize,
                isActive: isActiveFilter === 'all' ? undefined : isActiveFilter === 'active'
            });
            return result;
        }
    });

    const deleteMutation = useMutation({
        mutationFn: adminService.deleteCareerServiceType,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-career-types'] });
            toast.success('Category deleted successfully.');
        },
        onError: () => {
            toast.error('Failed to delete category.');
        }
    });

    const handleDelete = (id: string, name: string) => {
        setTypeToDelete({ id, name });
    };

    const executeDelete = () => {
        if (typeToDelete) {
            deleteMutation.mutate(typeToDelete.id, {
                onSettled: () => setTypeToDelete(null)
            });
        }
    };

    return (
        <div className="space-y-8 animate-in fade-in duration-500">
            <PageHeader
                title="Career Categories"
                description="Manage service types like Workshops, Job Fairs, Mock Interviews, etc."
                action={
                    <Button className="shadow-neon" onClick={() => setIsCreateModalOpen(true)}>
                        <Plus className="w-4 h-4 mr-2" /> New Category
                    </Button>
                }
            />

            {/* Filter Tabs */}
            <div className="flex space-x-2 border-b border-white/10 pb-4">
                {[
                    { label: 'All', value: 'all' },
                    { label: 'Active', value: 'active' },
                    { label: 'Inactive', value: 'inactive' }
                ].map((tab) => (
                    <Button
                        key={tab.label}
                        variant={isActiveFilter === tab.value ? 'default' : 'ghost'}
                        size="sm"
                        onClick={() => {
                            setIsActiveFilter(tab.value as any);
                            setPage(1);
                        }}
                    >
                        {tab.label}
                    </Button>
                ))}
            </div>

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
                            <TableHead className="w-[40%]">Name (English)</TableHead>
                            <TableHead className="w-[40%]">Name (Arabic)</TableHead>
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
                            data?.items.map((item: CareerServiceTypeDto) => (
                                <TableRow key={item.id}>
                                    <TableCell>
                                        <div className="text-slate-900 dark:text-white font-medium flex items-center gap-2">
                                            <div className="p-1.5 rounded bg-slate-100 dark:bg-white/5 text-slate-500 dark:text-slate-400">
                                                <Tags className="w-4 h-4" />
                                            </div>
                                            <span>{item.nameEn}</span>
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <div className="text-slate-700 dark:text-slate-300 font-arabic text-right dir-rtl">
                                            {item.nameAr}
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <StatusBadge variant={item.isActive ? 'success' : 'secondary'}>
                                            {item.isActive ? 'Active' : 'Inactive'}
                                        </StatusBadge>
                                    </TableCell>
                                    <TableCell className="text-right">
                                        <div className="flex justify-end gap-2">
                                            <Button 
                                                size="icon" 
                                                variant="ghost" 
                                                className="text-slate-400 hover:text-slate-900 hover:bg-slate-100 dark:hover:text-white dark:hover:bg-white/10" 
                                                title="Edit" 
                                                onClick={() => { setSelectedType(item); setIsEditModalOpen(true); }}
                                            >
                                                <Edit className="w-4 h-4" />
                                            </Button>
                                            <Button
                                                size="icon"
                                                variant="ghost"
                                                className="text-red-400 hover:text-red-300 hover:bg-red-500/20"
                                                onClick={() => handleDelete(item.id, item.nameEn)}
                                                disabled={deleteMutation.isPending && typeToDelete?.id === item.id}
                                                title="Delete"
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

            <CreateCareerServiceTypeModal open={isCreateModalOpen} onOpenChange={setIsCreateModalOpen} />
            <EditCareerServiceTypeModal open={isEditModalOpen} onOpenChange={setIsEditModalOpen} careerType={selectedType} />

            <ConfirmDialog
                open={!!typeToDelete}
                onOpenChange={(open: boolean) => { if (!open) setTypeToDelete(null); }}
                title="Delete Category"
                description={`Are you sure you want to delete "${typeToDelete?.name}"? This will affect services currently assigned to this category.`}
                confirmLabel="Delete"
                variant="danger"
                onConfirm={executeDelete}
                isLoading={deleteMutation.isPending}
            />
        </div>
    );
};

export default CareerServiceTypeManager;
