import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { BookOpen, Plus, Trash2, Download, FileText, Calendar } from 'lucide-react';
import { PageHeader } from '../../components/admin/PageHeader';
import { DataTableShell } from '../../components/admin/DataTableShell';
import { Button } from '../../components/ui/Button';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/Table';
import { toast } from 'sonner';
import { CreateMagazineModal } from '../../components/admin/magazine/CreateMagazineModal';
import type { MagazineIssueDto } from '../../types/news';

const MagazineManager = () => {
    const queryClient = useQueryClient();
    const [page, setPage] = useState(1);
    const pageSize = 10;
    const [filter, setFilter] = useState('');
    const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);

    const { data, isLoading } = useQuery({
        queryKey: ['admin-magazines', page, filter],
        queryFn: () => adminService.getMagazines({
            skipCount: (page - 1) * pageSize,
            maxResultCount: pageSize,
            filter
        })
    });

    const deleteMutation = useMutation({
        mutationFn: adminService.deleteMagazine,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-magazines'] });
            toast.success('Magazine issue deleted successfully');
        },
        onError: () => toast.error('Failed to delete magazine issue')
    });

    const handleDelete = (id: string, title: string) => {
        if (window.confirm(`Are you sure you want to delete "${title}"?`)) {
            deleteMutation.mutate(id);
        }
    };

    const items = data?.items || [];
    const totalCount = data?.totalCount || 0;

    return (
        <div className="space-y-8 animate-in fade-in duration-500">
            <PageHeader
                title="Magazine Manager"
                description="Manage alumni magazine issues and PDF uploads."
                action={
                    <Button className="shadow-neon" onClick={() => setIsCreateModalOpen(true)}>
                        <Plus className="w-4 h-4 mr-2" /> New Issue
                    </Button>
                }
            />

            <DataTableShell
                searchPlaceholder="Search issues..."
                onSearch={setFilter}
                pagination={{
                    currentPage: page,
                    totalPages: Math.ceil(totalCount / pageSize) || 1,
                    onPageChange: setPage
                }}
            >
                <Table>
                    <TableHeader>
                        <TableRow className="hover:bg-transparent border-white/5">
                            <TableHead className="text-slate-200">Issue Title</TableHead>
                            <TableHead className="text-slate-200">Publish Date</TableHead>
                            <TableHead className="text-slate-200">Upload Date</TableHead>
                            <TableHead className="text-right text-slate-200">Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {isLoading ? (
                            <TableRow>
                                <TableCell colSpan={4} className="h-32 text-center text-slate-400">Loading issues...</TableCell>
                            </TableRow>
                        ) : items.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={4} className="h-32 text-center text-slate-500 font-medium">
                                    No magazine issues found.
                                </TableCell>
                            </TableRow>
                        ) : (
                            items.map((issue: MagazineIssueDto) => (
                                <TableRow key={issue.id} className="group border-white/5 hover:bg-white/[0.02] transition-colors">
                                    <TableCell>
                                        <div className="flex items-center gap-3">
                                            <div className="w-10 h-10 rounded-lg bg-red-500/10 flex items-center justify-center text-red-500 border border-red-500/20">
                                                <FileText className="w-5 h-5" />
                                            </div>
                                            <div className="font-medium text-slate-200">{issue.title}</div>
                                        </div>
                                    </TableCell>
                                    <TableCell className="text-slate-400">
                                        <div className="flex items-center gap-2">
                                            <Calendar className="w-3.5 h-3.5" />
                                            {new Date(issue.publishDate).toLocaleDateString()}
                                        </div>
                                    </TableCell>
                                    <TableCell className="text-slate-500 text-xs font-mono lowercase">
                                        {issue.creationTime ? new Date(issue.creationTime).toLocaleDateString() : 'Unknown'}
                                    </TableCell>
                                    <TableCell className="text-right">
                                        <div className="flex justify-end gap-2 opacity-0 group-hover:opacity-100 transition-all duration-200">
                                            <Button
                                                size="icon"
                                                variant="ghost"
                                                className="text-slate-400 hover:text-white hover:bg-white/10"
                                                onClick={() => window.open(issue.pdfUrl, '_blank')}
                                                title="View PDF"
                                            >
                                                <Download className="w-4 h-4" />
                                            </Button>
                                            <Button
                                                size="icon"
                                                variant="ghost"
                                                className="text-red-400 hover:text-red-300 hover:bg-red-500/20"
                                                onClick={() => handleDelete(issue.id, issue.title)}
                                                disabled={deleteMutation.isPending}
                                                title="Delete Issue"
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

            <CreateMagazineModal open={isCreateModalOpen} onOpenChange={setIsCreateModalOpen} />
        </div>
    );
};

export default MagazineManager;
