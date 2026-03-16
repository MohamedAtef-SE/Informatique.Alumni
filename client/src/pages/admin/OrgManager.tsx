import { useState } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from 'react-oidc-context';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { Building2, Plus, MapPin, Trash2, Hash, Edit2, Mail, Phone, Link } from 'lucide-react';
import { PageHeader } from '../../components/admin/PageHeader';
import { DataTableShell } from '../../components/admin/DataTableShell';
import { Button } from '../../components/ui/Button';
import { BranchModal } from '../../components/admin/organization/BranchModal';
import { ConfirmDialog } from '../../components/ui/ConfirmDialog';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/Table';
import { toast } from 'sonner';
import type { BranchDto } from '../../types/organization';

const OrgManager = () => {
    const queryClient = useQueryClient();
    const [page, setPage] = useState(1);
    const pageSize = 10;
    const [filter, setFilter] = useState('');
    const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
    const [branchToDelete, setBranchToDelete] = useState<string | null>(null);
    const [branchToEdit, setBranchToEdit] = useState<BranchDto | null>(null);

    // Auth Check
    const auth = useAuth();
    const roles = auth.user?.profile?.role;
    const roleArray = Array.isArray(roles) ? roles : roles ? [String(roles)] : [];
    const isSuperAdmin = roleArray.some(r => ['admin', 'systemadmin'].includes(r.toLowerCase()));

    if (!isSuperAdmin) {
        return <Navigate to="/admin/dashboard" replace />;
    }

    const { data, isLoading } = useQuery({
        queryKey: ['admin-branches', page, filter],
        queryFn: () => adminService.getBranches({
            skipCount: (page - 1) * pageSize,
            maxResultCount: pageSize,
            filter
        })
    });

    const deleteMutation = useMutation({
        mutationFn: adminService.deleteBranch,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-branches'] });
            toast.success('Branch deleted successfully');
        },
        onError: () => toast.error('Failed to delete branch')
    });

    const handleDelete = (id: string) => {
        setBranchToDelete(id);
    };

    const confirmDelete = () => {
        if (branchToDelete) {
            deleteMutation.mutate(branchToDelete, {
                onSettled: () => setBranchToDelete(null)
            });
        }
    };

    return (
        <div className="space-y-8 animate-in fade-in duration-500">
            <PageHeader
                title="Branch Management"
                description="Manage organization branches and locations."
                action={
                    <Button className="shadow-neon" onClick={() => setIsCreateModalOpen(true)}>
                        <Plus className="w-4 h-4 mr-2" /> New Branch
                    </Button>
                }
            />

            <DataTableShell
                searchPlaceholder="Search branches..."
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
                            <TableHead>Branch Name</TableHead>
                            <TableHead>Code</TableHead>
                            <TableHead>Contact Info</TableHead>
                            <TableHead>Address & Maps</TableHead>
                            <TableHead className="text-right">Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {isLoading ? (
                            <TableRow>
                                <TableCell colSpan={5} className="h-32 text-center text-slate-400">Loading branches...</TableCell>
                            </TableRow>
                        ) : data?.items.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={5} className="h-32 text-center text-slate-500">
                                    No branches found.
                                </TableCell>
                            </TableRow>
                        ) : (
                            data?.items.map((branch: BranchDto) => (
                                <TableRow key={branch.id} className="group">
                                    <TableCell className="font-medium text-slate-900 dark:text-white">
                                        <div className="flex items-center gap-2">
                                            <Building2 className="w-4 h-4 text-indigo-500" />
                                            <div className="flex flex-col">
                                                <span>{branch.name}</span>
                                                {(branch.linkedInPage || branch.facebookPage) &&
                                                    <div className="flex items-center gap-1 mt-1 text-slate-400">
                                                        <Link className="w-3 h-3" />
                                                        <span className="text-[10px] uppercase">Social links active</span>
                                                    </div>
                                                }
                                            </div>
                                        </div>
                                    </TableCell>
                                    <TableCell className="text-slate-500 dark:text-slate-400 font-mono text-xs">
                                        <div className="flex items-center gap-1">
                                            <Hash className="w-3 h-3" />
                                            {branch.code}
                                        </div>
                                    </TableCell>
                                    <TableCell className="text-slate-600 dark:text-slate-300">
                                        <div className="flex flex-col gap-1 text-xs">
                                            {branch.email ? (
                                                <div className="flex items-center gap-1 truncate max-w-[150px]" title={branch.email}>
                                                    <Mail className="w-3 h-3 text-slate-400 shrink-0" />
                                                    {branch.email}
                                                </div>
                                            ) : (
                                                <span className="text-slate-500 italic">No email</span>
                                            )}
                                            {branch.phoneNumber && (
                                                <div className="flex items-center gap-1">
                                                    <Phone className="w-3 h-3 text-slate-400 shrink-0" />
                                                    {branch.phoneNumber}
                                                </div>
                                            )}
                                        </div>
                                    </TableCell>
                                    <TableCell className="text-slate-600 dark:text-slate-300">
                                        <div className="flex flex-col gap-1 text-xs">
                                            <div className="flex items-start gap-1">
                                                <MapPin className="w-3 h-3 text-slate-400 shrink-0 mt-0.5" />
                                                <span className="truncate max-w-[200px]" title={branch.address || ''}>
                                                    {branch.address || 'Address not set'}
                                                </span>
                                            </div>
                                            {branch.latitude && branch.longitude && (
                                                <div className="text-[10px] text-slate-400 ml-4 font-mono">
                                                    {branch.latitude.toFixed(4)}, {branch.longitude.toFixed(4)}
                                                </div>
                                            )}
                                        </div>
                                    </TableCell>
                                    <TableCell className="text-right">
                                        <div className="flex items-center justify-end gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                                            <Button
                                                size="icon"
                                                variant="ghost"
                                                className="text-blue-400 hover:text-blue-300 hover:bg-blue-500/20"
                                                onClick={() => setBranchToEdit(branch)}
                                            >
                                                <Edit2 className="w-4 h-4" />
                                            </Button>
                                            <Button
                                                size="icon"
                                                variant="ghost"
                                                className="text-red-400 hover:text-red-300 hover:bg-red-500/20"
                                                onClick={() => handleDelete(branch.id)}
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

            <BranchModal
                open={isCreateModalOpen || !!branchToEdit}
                onOpenChange={(open) => {
                    if (!open) {
                        setIsCreateModalOpen(false);
                        setBranchToEdit(null);
                    }
                }}
                branch={branchToEdit}
            />

            <ConfirmDialog
                open={!!branchToDelete}
                onOpenChange={(open) => !open && setBranchToDelete(null)}
                title="Delete Branch"
                description="Are you sure you want to delete this branch? This action cannot be undone."
                confirmLabel="Delete"
                variant="danger"
                onConfirm={confirmDelete}
                isLoading={deleteMutation.isPending}
            />
        </div>
    );
};

export default OrgManager;
