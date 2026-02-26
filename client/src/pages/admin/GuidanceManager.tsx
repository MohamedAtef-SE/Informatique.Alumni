import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { AdvisingRequestStatus, type AdvisingRequestDto } from '../../types/guidance';
import { Check, X, CalendarClock, User, MessageSquare } from 'lucide-react';
import { PageHeader } from '../../components/admin/PageHeader';
import { DataTableShell } from '../../components/admin/DataTableShell';
import { StatusBadge } from '../../components/admin/StatusBadge';
import { Button } from '../../components/ui/Button';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/Table';
import { toast } from 'sonner';
import { ReasonModal } from '../../components/admin/ReasonModal';

const GuidanceManager = () => {
    const queryClient = useQueryClient();
    const [page, setPage] = useState(1);
    const pageSize = 10;
    const [statusFilter, setStatusFilter] = useState<'all' | 'pending' | 'approved' | 'rejected' | 'completed'>('all');

    const [rejectId, setRejectId] = useState<string | null>(null);
    const [isRejectModalOpen, setIsRejectModalOpen] = useState(false);

    const { data, isLoading } = useQuery({
        queryKey: ['admin-advising', statusFilter, page],
        queryFn: () => adminService.getAdvisingRequests({
            skipCount: (page - 1) * pageSize,
            maxResultCount: pageSize,
            status: statusFilter === 'all' ? undefined :
                statusFilter === 'pending' ? AdvisingRequestStatus.Pending :
                    statusFilter === 'approved' ? AdvisingRequestStatus.Approved :
                        statusFilter === 'rejected' ? AdvisingRequestStatus.Rejected :
                            AdvisingRequestStatus.Completed
        })
    });

    const updateStatusMutation = useMutation({
        mutationFn: (variables: { id: string, status: AdvisingRequestStatus, notes?: string }) =>
            adminService.updateAdvisingStatus(variables.id, { status: variables.status, notes: variables.notes }),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-advising'] });
            toast.success('Status updated successfully');
            setIsRejectModalOpen(false);
            setRejectId(null);
        },
        onError: () => toast.error('Failed to update status')
    });

    const handleStatusChange = (id: string, newStatus: AdvisingRequestStatus) => {
        if (newStatus === AdvisingRequestStatus.Rejected) {
            setRejectId(id);
            setIsRejectModalOpen(true);
            return;
        }

        const action = newStatus === AdvisingRequestStatus.Approved ? 'Approve' :
            newStatus === AdvisingRequestStatus.Completed ? 'Mark as Completed' : 'Update';

        if (confirm(`${action} this request?`)) {
            updateStatusMutation.mutate({ id, status: newStatus });
        }
    };

    const confirmReject = (reason: string) => {
        if (rejectId) {
            updateStatusMutation.mutate({ id: rejectId, status: AdvisingRequestStatus.Rejected, notes: reason });
        }
    };

    const items = data?.items || [];

    return (
        <div className="space-y-8 animate-in fade-in duration-500">
            <PageHeader
                title="Guidance & Advising"
                description="Manage mentorship and career guidance sessions."
            />

            {/* Status Tabs */}
            <div className="flex space-x-2 border-b border-white/10 pb-4 overflow-x-auto">
                {[
                    { label: 'All', value: 'all' },
                    { label: 'Pending', value: 'pending' },
                    { label: 'Approved', value: 'approved' },
                    { label: 'Rejected', value: 'rejected' },
                    { label: 'Completed', value: 'completed' }
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
                searchPlaceholder="Search sessions..."
                onSearch={() => { }}
                pagination={{
                    currentPage: page,
                    totalPages: Math.ceil((data?.totalCount || 0) / pageSize),
                    onPageChange: setPage
                }}
            >
                <Table>
                    <TableHeader>
                        <TableRow className="hover:bg-transparent border-white/5">
                            <TableHead>Time & Date</TableHead>
                            <TableHead>Alumni/Advisor</TableHead>
                            <TableHead>Subject</TableHead>
                            <TableHead>Status</TableHead>
                            <TableHead className="text-right">Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {isLoading ? (
                            <TableRow>
                                <TableCell colSpan={5} className="h-32 text-center text-slate-400">Loading sessions...</TableCell>
                            </TableRow>
                        ) : items.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={5} className="h-32 text-center text-slate-500">
                                    No sessions found.
                                </TableCell>
                            </TableRow>
                        ) : (
                            items.map((req: AdvisingRequestDto) => (
                                <TableRow key={req.id}>
                                    <TableCell className="text-slate-300">
                                        <div className="flex flex-col gap-1">
                                            <div className="flex items-center gap-2 text-white font-medium">
                                                <CalendarClock className="w-4 h-4 text-primary" />
                                                {new Date(req.startTime).toLocaleDateString()}
                                            </div>
                                            <span className="text-xs text-slate-500 pl-6">
                                                {new Date(req.startTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })} -
                                                {new Date(req.endTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                                            </span>
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <div className="flex flex-col text-sm text-slate-300">
                                            <div className="flex items-center gap-1 text-white">
                                                <User className="w-3 h-3 text-slate-500" />
                                                Alumni: {req.alumniId /* Should resolve name ideally */}
                                            </div>
                                            <div className="flex items-center gap-1 text-xs text-slate-500 mt-1">
                                                <User className="w-3 h-3" />
                                                Advisor: {req.advisorId /* Should resolve name ideally */}
                                            </div>
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <div className="flex flex-col">
                                            <span className="text-white font-medium">{req.subject}</span>
                                            {req.description && (
                                                <div className="flex items-center gap-1 text-xs text-slate-400 mt-1 truncate max-w-[200px]" title={req.description}>
                                                    <MessageSquare className="w-3 h-3" />
                                                    {req.description}
                                                </div>
                                            )}
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <StatusBadge variant={
                                            req.status === AdvisingRequestStatus.Completed ? 'success' :
                                                req.status === AdvisingRequestStatus.Rejected ? 'destructive' :
                                                    req.status === AdvisingRequestStatus.Approved ? 'info' :
                                                        req.status === AdvisingRequestStatus.Cancelled ? 'secondary' : 'warning'
                                        }>
                                            {Object.keys(AdvisingRequestStatus).find(k => AdvisingRequestStatus[k as keyof typeof AdvisingRequestStatus] === req.status)}
                                        </StatusBadge>
                                    </TableCell>
                                    <TableCell className="text-right">
                                        <div className="flex justify-end gap-2">
                                            {req.status === AdvisingRequestStatus.Pending && (
                                                <>
                                                    <Button size="icon" variant="ghost" className="h-8 w-8 text-emerald-400 hover:bg-emerald-500/20" onClick={() => handleStatusChange(req.id, AdvisingRequestStatus.Approved)}>
                                                        <Check className="w-4 h-4" />
                                                    </Button>
                                                    <Button size="icon" variant="ghost" className="h-8 w-8 text-red-400 hover:bg-red-500/20" onClick={() => handleStatusChange(req.id, AdvisingRequestStatus.Rejected)}>
                                                        <X className="w-4 h-4" />
                                                    </Button>
                                                </>
                                            )}
                                            {req.status === AdvisingRequestStatus.Approved && (
                                                <Button size="sm" variant="outline" className="h-8" onClick={() => handleStatusChange(req.id, AdvisingRequestStatus.Completed)}>
                                                    Mark Completed
                                                </Button>
                                            )}
                                        </div>
                                    </TableCell>
                                </TableRow>
                            ))
                        )}
                    </TableBody>
                </Table>
            </DataTableShell>

            <ReasonModal
                open={isRejectModalOpen}
                onOpenChange={setIsRejectModalOpen}
                onConfirm={confirmReject}
                isLoading={updateStatusMutation.isPending}
                title="Reject Session"
                description="Please provide a reason for rejecting this session request."
            />
        </div>
    );
};

export default GuidanceManager;
