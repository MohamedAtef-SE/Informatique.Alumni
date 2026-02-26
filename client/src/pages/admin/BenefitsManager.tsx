import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { Plus, Edit, Trash2, Gift, Award } from 'lucide-react';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/Table';
import { Button } from '../../components/ui/Button';
import { toast } from 'sonner';
import { PageHeader } from '../../components/admin/PageHeader';
import { DataTableShell } from '../../components/admin/DataTableShell';
import { CreateGrantModal } from '../../components/admin/benefits/CreateGrantModal';
import { CreateDiscountModal } from '../../components/admin/benefits/CreateDiscountModal';
import { EditGrantModal } from '../../components/admin/benefits/EditGrantModal';
import { EditDiscountModal } from '../../components/admin/benefits/EditDiscountModal';
import { ConfirmDialog } from '../../components/ui/ConfirmDialog';
import type { AcademicGrantDto, CommercialDiscountDto } from '../../types/benefits';

const BenefitsManager = () => {
    const queryClient = useQueryClient();
    const [activeTab, setActiveTab] = useState<'grants' | 'discounts'>('grants');
    const [page, setPage] = useState(1);
    const pageSize = 10;
    const [isGrantModalOpen, setIsGrantModalOpen] = useState(false);
    const [isDiscountModalOpen, setIsDiscountModalOpen] = useState(false);
    const [isEditGrantModalOpen, setIsEditGrantModalOpen] = useState(false);
    const [isEditDiscountModalOpen, setIsEditDiscountModalOpen] = useState(false);
    const [selectedGrant, setSelectedGrant] = useState<AcademicGrantDto | null>(null);
    const [selectedDiscount, setSelectedDiscount] = useState<CommercialDiscountDto | null>(null);
    const [isDeleteConfirmOpen, setIsDeleteConfirmOpen] = useState(false);
    const [itemToDelete, setItemToDelete] = useState<{ id: string, title: string, type: 'grants' | 'discounts' } | null>(null);

    const grantsQuery = useQuery({
        queryKey: ['admin-grants', page],
        queryFn: async () => {
            const result = await adminService.getGrants({
                skipCount: (page - 1) * pageSize,
                maxResultCount: pageSize
            });
            return result;
        },
        enabled: activeTab === 'grants'
    });

    const discountsQuery = useQuery({
        queryKey: ['admin-discounts', page],
        queryFn: async () => {
            const result = await adminService.getDiscounts({
                skipCount: (page - 1) * pageSize,
                maxResultCount: pageSize
            });
            return result;
        },
        enabled: activeTab === 'discounts'
    });

    const deleteGrantMutation = useMutation({
        mutationFn: adminService.deleteGrant,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-grants'] });
            toast.success('Grant deleted.');
            setIsDeleteConfirmOpen(false);
            setItemToDelete(null);
        },
        onError: () => {
            toast.error('Failed to delete grant.');
            setIsDeleteConfirmOpen(false);
        }
    });

    const deleteDiscountMutation = useMutation({
        mutationFn: adminService.deleteDiscount,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-discounts'] });
            toast.success('Discount deleted.');
            setIsDeleteConfirmOpen(false);
            setItemToDelete(null);
        },
        onError: () => {
            toast.error('Failed to delete discount.');
            setIsDeleteConfirmOpen(false);
        }
    });

    const handleDeleteClick = (id: string, title: string, type: 'grants' | 'discounts') => {
        setItemToDelete({ id, title, type });
        setIsDeleteConfirmOpen(true);
    };

    const confirmDelete = () => {
        if (!itemToDelete) return;
        if (itemToDelete.type === 'grants') {
            deleteGrantMutation.mutate(itemToDelete.id);
        } else {
            deleteDiscountMutation.mutate(itemToDelete.id);
        }
    };

    const currentData = activeTab === 'grants' ? grantsQuery.data : discountsQuery.data;
    const isLoading = activeTab === 'grants' ? grantsQuery.isLoading : discountsQuery.isLoading;

    return (
        <div className="space-y-8 animate-in fade-in duration-500">
            <PageHeader
                title="Benefits Manager"
                description="Manage academic grants and commercial discounts/offers."
                action={
                    <div className="flex gap-2">
                        <Button
                            variant="outline"
                            className={activeTab === 'grants' ? 'bg-primary/20 text-primary border-primary/50' : ''}
                            onClick={() => { setActiveTab('grants'); setPage(1); }}
                        >
                            <Award className="w-4 h-4 mr-2" /> Grants
                        </Button>
                        <Button
                            variant="outline"
                            className={activeTab === 'discounts' ? 'bg-primary/20 text-primary border-primary/50' : ''}
                            onClick={() => { setActiveTab('discounts'); setPage(1); }}
                        >
                            <Gift className="w-4 h-4 mr-2" /> Discounts
                        </Button>
                        <div className="w-px bg-slate-200 dark:bg-white/10 mx-2 h-8" />
                        <Button className="shadow-neon" onClick={() => activeTab === 'grants' ? setIsGrantModalOpen(true) : setIsDiscountModalOpen(true)}>
                            <Plus className="w-4 h-4 mr-2" /> New {activeTab === 'grants' ? 'Grant' : 'Discount'}
                        </Button>
                    </div>
                }
            />

            <DataTableShell
                searchPlaceholder="Search benefits..."
                onSearch={() => { }} // Search not implemented in backend for these yet
                pagination={{
                    currentPage: page,
                    totalPages: Math.ceil((currentData?.totalCount || 0) / pageSize),
                    onPageChange: setPage
                }}
            >
                <Table>
                    <TableHeader>
                        <TableRow className="hover:bg-slate-50 dark:hover:bg-white/5 border-slate-200 dark:border-white/5">
                            <TableHead className="w-[35%] text-slate-500 dark:text-slate-400">Title</TableHead>
                            <TableHead className="text-slate-500 dark:text-slate-400">Details</TableHead>
                            {activeTab === 'discounts' && <TableHead className="text-slate-500 dark:text-slate-400">Promo Code</TableHead>}
                            <TableHead className="text-right text-slate-500 dark:text-slate-400">Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {isLoading ? (
                            <TableRow>
                                <TableCell colSpan={activeTab === 'discounts' ? 4 : 3} className="h-32 text-center text-slate-400">Loading...</TableCell>
                            </TableRow>
                        ) : currentData?.items.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={activeTab === 'discounts' ? 4 : 3} className="h-32 text-center text-slate-500">
                                    No items found.
                                </TableCell>
                            </TableRow>
                        ) : activeTab === 'grants' ? (
                            (currentData?.items as AcademicGrantDto[])?.map((item) => (
                                <TableRow key={item.id}>
                                    <TableCell>
                                        <div className="text-slate-900 dark:text-white font-medium flex items-center gap-2">
                                            <Award className="w-4 h-4 text-amber-500" />
                                            {item.nameEn}
                                        </div>
                                    </TableCell>
                                    <TableCell className="text-slate-600 dark:text-slate-300">
                                        {item.percentage}% Coverage ({item.type})
                                    </TableCell>
                                    <TableCell className="text-right">
                                        <div className="flex justify-end gap-2">
                                            <Button
                                                size="icon"
                                                variant="ghost"
                                                className="text-slate-400 hover:text-slate-900 dark:hover:text-white"
                                                title="Edit"
                                                onClick={() => {
                                                    setSelectedGrant(item);
                                                    setIsEditGrantModalOpen(true);
                                                }}
                                            >
                                                <Edit className="w-4 h-4" />
                                            </Button>
                                            <Button
                                                size="icon"
                                                variant="ghost"
                                                className="text-red-400 hover:text-red-300 hover:bg-red-500/20"
                                                onClick={() => handleDeleteClick(item.id, item.nameEn, 'grants')}
                                                title="Delete"
                                            >
                                                <Trash2 className="w-4 h-4" />
                                            </Button>
                                        </div>
                                    </TableCell>
                                </TableRow>
                            ))
                        ) : (
                            (currentData?.items as CommercialDiscountDto[])?.map((item) => (
                                <TableRow key={item.id}>
                                    <TableCell>
                                        <div className="text-slate-900 dark:text-white font-medium flex items-center gap-2">
                                            <Gift className="w-4 h-4 text-purple-500" />
                                            <div>
                                                <div>{item.title}</div>
                                                <div className="text-xs text-slate-500">{item.providerName}</div>
                                            </div>
                                        </div>
                                    </TableCell>
                                    <TableCell className="text-slate-600 dark:text-slate-300">
                                        {item.discountPercentage}% Off
                                    </TableCell>
                                    <TableCell>
                                        {item.promoCode ? (
                                            <span className="px-2 py-0.5 rounded bg-slate-100 dark:bg-white/10 text-xs font-mono text-accent">{item.promoCode}</span>
                                        ) : (
                                            <span className="text-slate-400 text-xs italic">None</span>
                                        )}
                                    </TableCell>
                                    <TableCell className="text-right">
                                        <div className="flex justify-end gap-2">
                                            <Button
                                                size="icon"
                                                variant="ghost"
                                                className="text-slate-400 hover:text-slate-900 dark:hover:text-white"
                                                title="Edit"
                                                onClick={() => {
                                                    setSelectedDiscount(item);
                                                    setIsEditDiscountModalOpen(true);
                                                }}
                                            >
                                                <Edit className="w-4 h-4" />
                                            </Button>
                                            <Button
                                                size="icon"
                                                variant="ghost"
                                                className="text-red-400 hover:text-red-300 hover:bg-red-500/20"
                                                onClick={() => handleDeleteClick(item.id, item.title, 'discounts')}
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

            <CreateGrantModal open={isGrantModalOpen} onOpenChange={setIsGrantModalOpen} />
            <CreateDiscountModal open={isDiscountModalOpen} onOpenChange={setIsDiscountModalOpen} />
            <EditGrantModal open={isEditGrantModalOpen} onOpenChange={setIsEditGrantModalOpen} grant={selectedGrant} />
            <EditDiscountModal open={isEditDiscountModalOpen} onOpenChange={setIsEditDiscountModalOpen} discount={selectedDiscount} />

            <ConfirmDialog
                open={isDeleteConfirmOpen}
                onOpenChange={setIsDeleteConfirmOpen}
                title={`Delete ${itemToDelete?.type === 'grants' ? 'Grant' : 'Discount'}`}
                description={`Are you sure you want to delete "${itemToDelete?.title}"? This action cannot be undone.`}
                onConfirm={confirmDelete}
                variant="danger"
                confirmLabel="Delete"
                isLoading={deleteGrantMutation.isPending || deleteDiscountMutation.isPending}
            />
        </div>
    );
};

export default BenefitsManager;
