import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { HeartPulse, Plus, Trash2, MapPin, Phone, Edit2 } from 'lucide-react';
import { PageHeader } from '../../components/admin/PageHeader';
import { DataTableShell } from '../../components/admin/DataTableShell';
import { Button } from '../../components/ui/Button';
import { CreateMedicalPartnerModal } from '../../components/admin/health/CreateMedicalPartnerModal';
import { EditMedicalPartnerModal } from '../../components/admin/health/EditMedicalPartnerModal';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/Table';
import { toast } from 'sonner';
import { MedicalPartnerType, type MedicalPartnerDto } from '../../types/health';

const HealthManager = () => {
    const queryClient = useQueryClient();
    const [page, setPage] = useState(1);
    const pageSize = 10;
    const [filter, setFilter] = useState('');
    const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
    const [editingPartner, setEditingPartner] = useState<MedicalPartnerDto | null>(null);

    const { data, isLoading } = useQuery({
        queryKey: ['admin-medical-partners', page, filter],
        queryFn: () => adminService.getMedicalPartners({
            skipCount: (page - 1) * pageSize,
            maxResultCount: pageSize,
            filter
        })
    });

    const items = data?.items || (Array.isArray(data) ? data : []);
    const totalCount = data?.totalCount || items.length;

    const deleteMutation = useMutation({
        mutationFn: adminService.deleteMedicalPartner,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-medical-partners'] });
            toast.success('Partner deleted successfuly');
        },
        onError: () => toast.error('Failed to delete partner')
    });

    const handleDelete = (id: string) => {
        if (confirm('Are you sure you want to delete this partner?')) {
            deleteMutation.mutate(id);
        }
    };

    const getTypeName = (type: number) => {
        return Object.keys(MedicalPartnerType).find(key => MedicalPartnerType[key as keyof typeof MedicalPartnerType] === type) || 'Unknown';
    };

    return (
        <div className="space-y-8 animate-in fade-in duration-500">
            <PageHeader
                title="Medical Partners"
                description="Manage hospitals, clinics, and medical providers."
                action={
                    <Button className="shadow-neon" onClick={() => setIsCreateModalOpen(true)}>
                        <Plus className="w-4 h-4 mr-2" /> New Partner
                    </Button>
                }
            />

            <DataTableShell
                searchPlaceholder="Search partners..."
                onSearch={setFilter}
                pagination={{
                    currentPage: page,
                    totalPages: Math.ceil(totalCount / pageSize) || 1,
                    onPageChange: setPage
                }}
            >
                <Table>
                    <TableHeader>
                        <TableRow className="hover:bg-transparent border-slate-200 dark:border-white/5">
                            <TableHead>Name</TableHead>
                            <TableHead>Type</TableHead>
                            <TableHead>Location</TableHead>
                            <TableHead>Contact</TableHead>
                            <TableHead className="text-right">Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {isLoading ? (
                            <TableRow>
                                <TableCell colSpan={5} className="h-32 text-center text-slate-400">Loading partners...</TableCell>
                            </TableRow>
                        ) : items.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={5} className="h-32 text-center text-slate-500">
                                    No medical partners found.
                                </TableCell>
                            </TableRow>
                        ) : (
                            items.map((partner: MedicalPartnerDto) => (
                                <TableRow key={partner.id} className="group">
                                    <TableCell className="font-medium text-slate-900 dark:text-white">
                                        <div className="flex items-center gap-2">
                                            <HeartPulse className="w-4 h-4 text-emerald-500" />
                                            {partner.name}
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <span className="px-2 py-1 rounded-full bg-slate-100 dark:bg-slate-800 text-xs text-slate-700 dark:text-slate-300 border border-slate-200 dark:border-white/5">
                                            {getTypeName(partner.type)}
                                        </span>
                                    </TableCell>
                                    <TableCell className="text-slate-500 dark:text-slate-400">
                                        <div className="flex items-center gap-1 text-xs">
                                            <MapPin className="w-3 h-3" />
                                            {partner.city ? `${partner.city}, ${partner.address}` : partner.address}
                                        </div>
                                    </TableCell>
                                    <TableCell className="text-slate-500 dark:text-slate-400">
                                        <div className="flex items-center gap-1 text-xs">
                                            <Phone className="w-3 h-3" />
                                            {partner.contactNumber}
                                        </div>
                                    </TableCell>
                                    <TableCell className="text-right">
                                        <div className="flex justify-end gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
                                            <Button
                                                size="icon"
                                                variant="ghost"
                                                className="text-slate-500 hover:text-blue-600 hover:bg-blue-50 dark:hover:bg-blue-500/20"
                                                onClick={() => setEditingPartner(partner)}
                                            >
                                                <Edit2 className="w-4 h-4" />
                                            </Button>
                                            <Button
                                                size="icon"
                                                variant="ghost"
                                                className="text-red-500 hover:text-red-600 hover:bg-red-50 dark:text-red-400 dark:hover:text-red-300 dark:hover:bg-red-500/20"
                                                onClick={() => handleDelete(partner.id)}
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

            <CreateMedicalPartnerModal open={isCreateModalOpen} onOpenChange={setIsCreateModalOpen} />
            <EditMedicalPartnerModal open={!!editingPartner} onOpenChange={(open) => !open && setEditingPartner(null)} partner={editingPartner} />
        </div>
    );
};

export default HealthManager;
