import { useState } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from 'react-oidc-context';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { BookOpen, GraduationCap, Trash2, Edit2, Plus } from 'lucide-react';
import { PageHeader } from '../../components/admin/PageHeader';
import { DataTableShell } from '../../components/admin/DataTableShell';
import { Button } from '../../components/ui/Button';
import { MajorModal } from '../../components/admin/organization/MajorModal';
import { ConfirmDialog } from '../../components/ui/ConfirmDialog';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/Table';
import { toast } from 'sonner';
import type { MajorDto } from '../../types/organization';

const MajorManager = () => {
    const queryClient = useQueryClient();
    const [page, setPage] = useState(1);
    const pageSize = 10;
    const [filter, setFilter] = useState('');
    const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
    const [majorToDelete, setMajorToDelete] = useState<string | null>(null);
    const [majorToEdit, setMajorToEdit] = useState<MajorDto | null>(null);

    // Auth Check
    const auth = useAuth();
    const roles = auth.user?.profile?.role;
    const roleArray = Array.isArray(roles) ? roles : roles ? [String(roles)] : [];
    const isSuperAdmin = roleArray.some(r => ['admin', 'systemadmin'].includes(r.toLowerCase()));

    if (!isSuperAdmin) {
        return <Navigate to="/admin/dashboard" replace />;
    }

    const { data: collegesData } = useQuery({
        queryKey: ['admin-colleges-all'],
        queryFn: () => adminService.getColleges({ skipCount: 0, maxResultCount: 1000 })
    });
    
    const collegesMap = new Map((collegesData?.items || []).map(c => [c.id, c.name]));

    const { data, isLoading } = useQuery({
        queryKey: ['admin-majors', page, filter],
        queryFn: () => adminService.getMajors({
            skipCount: (page - 1) * pageSize,
            maxResultCount: pageSize,
            filter
        })
    });

    const deleteMutation = useMutation({
        mutationFn: adminService.deleteMajor,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-majors'] });
            toast.success('Major deleted successfully');
        },
        onError: () => toast.error('Failed to delete major')
    });

    const handleDelete = (id: string) => {
        setMajorToDelete(id);
    };

    const confirmDelete = () => {
        if (majorToDelete) {
            deleteMutation.mutate(majorToDelete, {
                onSettled: () => setMajorToDelete(null)
            });
        }
    };

    return (
        <div className="space-y-8 animate-in fade-in duration-500">
            <PageHeader
                title="Majors Management"
                description="Manage academic majors and specializations."
                action={
                    <Button className="shadow-neon" onClick={() => setIsCreateModalOpen(true)}>
                        <Plus className="w-4 h-4 mr-2" /> New Major
                    </Button>
                }
            />

            <DataTableShell
                searchPlaceholder="Search majors..."
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
                            <TableHead>Major Name</TableHead>
                            <TableHead>Parent College</TableHead>
                            <TableHead className="text-right">Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {isLoading ? (
                            <TableRow>
                                <TableCell colSpan={3} className="h-32 text-center text-slate-400">Loading majors...</TableCell>
                            </TableRow>
                        ) : data?.items.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={3} className="h-32 text-center text-slate-500">
                                    No majors found.
                                </TableCell>
                            </TableRow>
                        ) : (
                            data?.items.map((major: MajorDto) => (
                                <TableRow key={major.id} className="group">
                                    <TableCell className="font-medium text-slate-900 dark:text-white">
                                        <div className="flex items-center gap-2">
                                            <BookOpen className="w-4 h-4 text-indigo-500" />
                                            <span>{major.name}</span>
                                        </div>
                                    </TableCell>
                                    <TableCell className="text-slate-600 dark:text-slate-300">
                                        <div className="flex items-center gap-2">
                                            <GraduationCap className="w-4 h-4 text-slate-400" />
                                            <span>{collegesMap.get(major.collegeId) || 'Unknown College'}</span>
                                        </div>
                                    </TableCell>
                                    <TableCell className="text-right">
                                        <div className="flex items-center justify-end gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                                            <Button
                                                size="icon"
                                                variant="ghost"
                                                className="text-blue-400 hover:text-blue-300 hover:bg-blue-500/20"
                                                onClick={() => setMajorToEdit(major)}
                                            >
                                                <Edit2 className="w-4 h-4" />
                                            </Button>
                                            <Button
                                                size="icon"
                                                variant="ghost"
                                                className="text-red-400 hover:text-red-300 hover:bg-red-500/20"
                                                onClick={() => handleDelete(major.id)}
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

            <MajorModal
                open={isCreateModalOpen || !!majorToEdit}
                onOpenChange={(open: boolean) => {
                    if (!open) {
                        setIsCreateModalOpen(false);
                        setMajorToEdit(null);
                    }
                }}
                major={majorToEdit}
            />

            <ConfirmDialog
                open={!!majorToDelete}
                onOpenChange={(open) => !open && setMajorToDelete(null)}
                title="Delete Major"
                description="Are you sure you want to delete this major?  This action cannot be undone."
                confirmLabel="Delete"
                variant="danger"
                onConfirm={confirmDelete}
                isLoading={deleteMutation.isPending}
            />
        </div>
    );
};

export default MajorManager;
