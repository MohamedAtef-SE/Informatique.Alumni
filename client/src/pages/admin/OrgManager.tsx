import { useState } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from 'react-oidc-context';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { Building2, Plus, MapPin, Trash2, Hash } from 'lucide-react';
import { PageHeader } from '../../components/admin/PageHeader';
import { DataTableShell } from '../../components/admin/DataTableShell';
import { Button } from '../../components/ui/Button';
import { CreateBranchModal } from '../../components/admin/organization/CreateBranchModal';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/Table';
import { toast } from 'sonner';
import type { BranchDto } from '../../types/organization';

const OrgManager = () => {
    const queryClient = useQueryClient();
    const [page, setPage] = useState(1);
    const pageSize = 10;
    const [filter, setFilter] = useState('');
    const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);

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
        if (confirm('Are you sure you want to delete this branch?')) {
            deleteMutation.mutate(id);
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
                            <TableHead>Address</TableHead>
                            <TableHead className="text-right">Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {isLoading ? (
                            <TableRow>
                                <TableCell colSpan={4} className="h-32 text-center text-slate-400">Loading branches...</TableCell>
                            </TableRow>
                        ) : data?.items.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={4} className="h-32 text-center text-slate-500">
                                    No branches found.
                                </TableCell>
                            </TableRow>
                        ) : (
                            data?.items.map((branch: BranchDto) => (
                                <TableRow key={branch.id} className="group">
                                    <TableCell className="font-medium text-white">
                                        <div className="flex items-center gap-2">
                                            <Building2 className="w-4 h-4 text-indigo-400" />
                                            {branch.name}
                                        </div>
                                    </TableCell>
                                    <TableCell className="text-slate-400 font-mono text-xs">
                                        <div className="flex items-center gap-1">
                                            <Hash className="w-3 h-3" />
                                            {branch.code}
                                        </div>
                                    </TableCell>
                                    <TableCell className="text-slate-300">
                                        <div className="flex items-center gap-1">
                                            <MapPin className="w-3 h-3 text-slate-500" />
                                            {branch.address || '-'}
                                        </div>
                                    </TableCell>
                                    <TableCell className="text-right">
                                        <Button
                                            size="icon"
                                            variant="ghost"
                                            className="text-red-400 hover:text-red-300 hover:bg-red-500/20 opacity-0 group-hover:opacity-100 transition-opacity"
                                            onClick={() => handleDelete(branch.id)}
                                        >
                                            <Trash2 className="w-4 h-4" />
                                        </Button>
                                    </TableCell>
                                </TableRow>
                            ))
                        )}
                    </TableBody>
                </Table>
            </DataTableShell>

            <CreateBranchModal open={isCreateModalOpen} onOpenChange={setIsCreateModalOpen} />
        </div>
    );
};

export default OrgManager;
