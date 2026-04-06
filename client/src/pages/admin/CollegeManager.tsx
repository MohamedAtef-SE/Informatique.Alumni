import { useState } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from 'react-oidc-context';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { GraduationCap, Trash2, Edit2, Plus, Building2 } from 'lucide-react';
import { PageHeader } from '../../components/admin/PageHeader';
import { DataTableShell } from '../../components/admin/DataTableShell';
import { Button } from '../../components/ui/Button';
import { CollegeModal } from '../../components/admin/organization/CollegeModal';
import { ConfirmDialog } from '../../components/ui/ConfirmDialog';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/Table';
import { toast } from 'sonner';
import type { CollegeDto } from '../../types/organization';

const CollegeManager = () => {
    const queryClient = useQueryClient();
    const [page, setPage] = useState(1);
    const pageSize = 10;
    const [filter, setFilter] = useState('');
    const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
    const [collegeToDelete, setCollegeToDelete] = useState<string | null>(null);
    const [collegeToEdit, setCollegeToEdit] = useState<CollegeDto | null>(null);

    // Auth Check
    const auth = useAuth();
    const roles = auth.user?.profile?.role;
    const roleArray = Array.isArray(roles) ? roles : roles ? [String(roles)] : [];
    const isSuperAdmin = roleArray.some(r => ['admin', 'systemadmin'].includes(r.toLowerCase()));

    if (!isSuperAdmin) {
        return <Navigate to="/admin/dashboard" replace />;
    }

    const { data: branchesData } = useQuery({
        queryKey: ['admin-branches-all'],
        queryFn: () => adminService.getBranches({ skipCount: 0, maxResultCount: 1000 })
    });
    
    const branchesMap = new Map((branchesData?.items || []).map(b => [b.id, b.name]));

    const { data, isLoading } = useQuery({
        queryKey: ['admin-colleges', page, filter],
        queryFn: () => adminService.getColleges({
            skipCount: (page - 1) * pageSize,
            maxResultCount: pageSize,
            filter
        })
    });

    const deleteMutation = useMutation({
        mutationFn: adminService.deleteCollege,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-colleges'] });
            toast.success('College deleted successfully');
        },
        onError: () => toast.error('Failed to delete college')
    });

    const handleDelete = (id: string) => {
        setCollegeToDelete(id);
    };

    const confirmDelete = () => {
        if (collegeToDelete) {
            deleteMutation.mutate(collegeToDelete, {
                onSettled: () => setCollegeToDelete(null)
            });
        }
    };

    return (
        <div className="space-y-8 animate-in fade-in duration-500">
            <PageHeader
                title="Colleges Management"
                description="Manage colleges and faculties within the university."
                action={
                    <Button className="shadow-neon" onClick={() => setIsCreateModalOpen(true)}>
                        <Plus className="w-4 h-4 mr-2" /> New College
                    </Button>
                }
            />

            <DataTableShell
                searchPlaceholder="Search colleges..."
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
                            <TableHead>College Name</TableHead>
                            <TableHead>Branch</TableHead>
                            <TableHead>External ID</TableHead>
                            <TableHead className="text-right">Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {isLoading ? (
                            <TableRow>
                                <TableCell colSpan={4} className="h-32 text-center text-slate-400">Loading colleges...</TableCell>
                            </TableRow>
                        ) : data?.items.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={4} className="h-32 text-center text-slate-500">
                                    No colleges found.
                                </TableCell>
                            </TableRow>
                        ) : (
                            data?.items.map((college: CollegeDto) => (
                                <TableRow key={college.id} className="group">
                                    <TableCell className="font-medium text-slate-900 dark:text-white">
                                        <div className="flex items-center gap-2">
                                            <GraduationCap className="w-4 h-4 text-indigo-500" />
                                            <span>{college.name}</span>
                                        </div>
                                    </TableCell>
                                    <TableCell className="text-slate-600 dark:text-slate-300">
                                        {college.branchId ? (
                                            <div className="flex items-center gap-2">
                                                <Building2 className="w-4 h-4 text-slate-400" />
                                                <span>{branchesMap.get(college.branchId) || 'Unknown Branch'}</span>
                                            </div>
                                        ) : (
                                            <span className="text-slate-500 italic">No branch linked</span>
                                        )}
                                    </TableCell>
                                    <TableCell className="text-slate-500 dark:text-slate-400 font-mono text-xs">
                                        {college.externalId || <span className="opacity-50">-</span>}
                                    </TableCell>
                                    <TableCell className="text-right">
                                        <div className="flex items-center justify-end gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                                            <Button
                                                size="icon"
                                                variant="ghost"
                                                className="text-blue-400 hover:text-blue-300 hover:bg-blue-500/20"
                                                onClick={() => setCollegeToEdit(college)}
                                            >
                                                <Edit2 className="w-4 h-4" />
                                            </Button>
                                            <Button
                                                size="icon"
                                                variant="ghost"
                                                className="text-red-400 hover:text-red-300 hover:bg-red-500/20"
                                                onClick={() => handleDelete(college.id)}
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

            <CollegeModal
                open={isCreateModalOpen || !!collegeToEdit}
                onOpenChange={(open: boolean) => {
                    if (!open) {
                        setIsCreateModalOpen(false);
                        setCollegeToEdit(null);
                    }
                }}
                college={collegeToEdit}
            />

            <ConfirmDialog
                open={!!collegeToDelete}
                onOpenChange={(open) => !open && setCollegeToDelete(null)}
                title="Delete College"
                description="Are you sure you want to delete this college? This action cannot be undone."
                confirmLabel="Delete"
                variant="danger"
                onConfirm={confirmDelete}
                isLoading={deleteMutation.isPending}
            />
        </div>
    );
};

export default CollegeManager;
