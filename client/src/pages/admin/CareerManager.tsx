import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { Plus, Edit, Trash2, Briefcase } from 'lucide-react';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/Table';
import { Button } from '../../components/ui/Button';
import { toast } from 'sonner';
import { PageHeader } from '../../components/admin/PageHeader';
import { DataTableShell } from '../../components/admin/DataTableShell';
import { StatusBadge } from '../../components/admin/StatusBadge';
import { CreateCareerServiceModal } from '../../components/admin/career/CreateCareerServiceModal';
import { EditCareerServiceModal } from '../../components/admin/career/EditCareerServiceModal';
import { ConfirmDialog } from '../../components/ui/ConfirmDialog';
import type { CareerServiceDto } from '../../types/career';

const CareerManager = () => {
    const queryClient = useQueryClient();
    const [filter, setFilter] = useState('');
    const [page, setPage] = useState(1);
    const pageSize = 10;
    const [statusFilter, setStatusFilter] = useState<'all' | 'open' | 'closed'>('all');
    const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
    const [isEditModalOpen, setIsEditModalOpen] = useState(false);
    const [selectedService, setSelectedService] = useState<CareerServiceDto | null>(null);
    const [serviceToDelete, setServiceToDelete] = useState<{ id: string, name: string } | null>(null);

    const { data, isLoading } = useQuery({
        queryKey: ['admin-career', filter, page],
        queryFn: async () => {
            const result = await adminService.getCareerServices({
                filter,
                skipCount: (page - 1) * pageSize,
                maxResultCount: pageSize
            });
            return result;
        }
    });

    const deleteMutation = useMutation({
        mutationFn: adminService.deleteCareerService,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-career'] });
            toast.success('Service deleted.');
        },
        onError: () => {
            toast.error('Failed to delete service.');
        }
    });

    const handleDelete = (id: string, name: string) => {
        setServiceToDelete({ id, name });
    };

    const executeDelete = () => {
        if (serviceToDelete) {
            deleteMutation.mutate(serviceToDelete.id, {
                onSettled: () => setServiceToDelete(null)
            });
        }
    };

    const filteredItems = data?.items.filter(item => {
        const isOpen = new Date(item.lastSubscriptionDate) > new Date();
        if (statusFilter === 'all') return true;
        if (statusFilter === 'open') return isOpen;
        if (statusFilter === 'closed') return !isOpen;
        return true;
    });

    return (
        <div className="space-y-8 animate-in fade-in duration-500">
            <PageHeader
                title="Career Manager"
                description="Manage career workshops, training sessions, and services."
                action={
                    <Button className="shadow-neon" onClick={() => setIsCreateModalOpen(true)}>
                        <Plus className="w-4 h-4 mr-2" /> New Service
                    </Button>
                }
            />

            {/* Status Tabs */}
            <div className="flex space-x-2 border-b border-white/10 pb-4">
                {[
                    { label: 'All Services', value: 'all' },
                    { label: 'Open', value: 'open' },
                    { label: 'Closed', value: 'closed' }
                ].map((tab) => (
                    <Button
                        key={tab.label}
                        variant={statusFilter === tab.value ? 'default' : 'ghost'}
                        size="sm"
                        onClick={() => {
                            setStatusFilter(tab.value as any);
                            setPage(1);
                        }}
                    >
                        {tab.label}
                    </Button>
                ))}
            </div>

            <DataTableShell
                searchPlaceholder="Search workshops..."
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
                            <TableHead className="w-[30%]">Title</TableHead>
                            <TableHead>Code</TableHead>
                            <TableHead>Type</TableHead>
                            <TableHead>Location</TableHead>
                            <TableHead>Fee</TableHead>
                            <TableHead>Subscribers</TableHead>
                            <TableHead>Status</TableHead>
                            <TableHead className="text-right">Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {isLoading ? (
                            <TableRow>
                                <TableCell colSpan={5} className="h-32 text-center text-slate-400">Loading services...</TableCell>
                            </TableRow>
                        ) : filteredItems?.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={5} className="h-32 text-center text-slate-500">
                                    No services found.
                                </TableCell>
                            </TableRow>
                        ) : (
                            filteredItems?.map((item: CareerServiceDto) => {
                                const isOpen = new Date(item.lastSubscriptionDate) > new Date();
                                return (
                                    <TableRow key={item.id}>
                                        <TableCell>
                                            <div className="text-slate-900 dark:text-white font-medium flex items-center gap-2">
                                                <div className="p-1.5 rounded bg-slate-100 dark:bg-white/5 text-slate-500 dark:text-slate-400">
                                                    <Briefcase className="w-4 h-4" />
                                                </div>
                                                <div>
                                                    <div>{item.nameEn}</div>
                                                    <div className="text-xs text-slate-500">Deadline: {new Date(item.lastSubscriptionDate).toLocaleDateString()}</div>
                                                </div>
                                            </div>
                                        </TableCell>
                                        <TableCell>
                                            <code className="text-xs font-mono text-slate-600 dark:text-slate-400 bg-slate-100 dark:bg-white/5 px-2 py-1 rounded border border-slate-200 dark:border-white/10">
                                                {item.code}
                                            </code>
                                        </TableCell>
                                        <TableCell>
                                            <div className="text-sm text-slate-700 dark:text-slate-300">
                                                {item.serviceType?.nameEn || <span className="text-slate-400 italic">Not Assigned</span>}
                                            </div>
                                        </TableCell>
                                        <TableCell>
                                            <div className="text-sm text-slate-700 dark:text-slate-300">
                                                {item.branch?.nameEn || <span className="text-slate-400 italic">Online / Unassigned</span>}
                                            </div>
                                        </TableCell>
                                        <TableCell>
                                            <div className="text-sm">
                                                {item.hasFees ? (
                                                    <span className="text-slate-700 dark:text-slate-300">{item.feeAmount} EGP</span>
                                                ) : (
                                                    <span className="text-emerald-600 dark:text-emerald-400 bg-emerald-50 dark:bg-emerald-500/10 px-2 py-0.5 rounded text-xs font-medium">Free</span>
                                                )}
                                            </div>
                                        </TableCell>
                                        <TableCell>
                                            <div className="text-slate-700 dark:text-slate-300 font-mono">{item.subscribedCount}</div>
                                        </TableCell>
                                        <TableCell>
                                            <StatusBadge variant={isOpen ? 'success' : 'secondary'}>
                                                {isOpen ? 'Open' : 'Closed'}
                                            </StatusBadge>
                                        </TableCell>
                                        <TableCell className="text-right">
                                            <div className="flex justify-end gap-2">
                                                <Button size="icon" variant="ghost" className="text-slate-400 hover:text-slate-900 hover:bg-slate-100 dark:hover:text-white dark:hover:bg-white/10" title="Edit" onClick={() => { setSelectedService(item); setIsEditModalOpen(true); }}>
                                                    <Edit className="w-4 h-4" />
                                                </Button>
                                                <Button
                                                    size="icon"
                                                    variant="ghost"
                                                    className="text-red-400 hover:text-red-300 hover:bg-red-500/20"
                                                    onClick={(e) => { e.preventDefault(); e.stopPropagation(); handleDelete(item.id, item.nameEn); }}
                                                    disabled={deleteMutation.isPending && serviceToDelete?.id === item.id}
                                                    title="Delete"
                                                >
                                                    <Trash2 className="w-4 h-4" />
                                                </Button>
                                            </div>
                                        </TableCell>
                                    </TableRow>
                                );
                            })
                        )}
                    </TableBody>
                </Table>
            </DataTableShell>

            <CreateCareerServiceModal open={isCreateModalOpen} onOpenChange={setIsCreateModalOpen} />
            <EditCareerServiceModal open={isEditModalOpen} onOpenChange={setIsEditModalOpen} service={selectedService} />

            <ConfirmDialog
                open={!!serviceToDelete}
                onOpenChange={(open: boolean) => { if (!open) setServiceToDelete(null); }}
                title="Delete Service"
                description={`Are you sure you want to delete "${serviceToDelete?.name}"? This action cannot be undone.`}
                confirmLabel="Delete"
                variant="danger"
                onConfirm={executeDelete}
                isLoading={deleteMutation.isPending}
            />
        </div>
    );
};

export default CareerManager;
