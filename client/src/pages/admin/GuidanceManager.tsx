import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { AdvisingRequestStatus, type GuidanceAdminDto } from '../../types/guidance';
import { Check, X, CalendarClock, User, MessageSquare } from 'lucide-react';
import { PageHeader } from '../../components/admin/PageHeader';
import { DataTableShell } from '../../components/admin/DataTableShell';
import { StatusBadge } from '../../components/admin/StatusBadge';
import { Button } from '../../components/ui/Button';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/Table';
import { toast } from 'sonner';
import { ReasonModal } from '../../components/admin/ReasonModal';
import { ApproveModal } from '../../components/admin/ApproveModal';

const GuidanceManager = () => {
    const queryClient = useQueryClient();
    const [page, setPage] = useState(1);
    const pageSize = 10;
    const [statusFilter, setStatusFilter] = useState<'all' | 'pending' | 'approved' | 'rejected' | 'completed'>('all');

    const [rejectId, setRejectId] = useState<string | null>(null);
    const [isRejectModalOpen, setIsRejectModalOpen] = useState(false);

    const [approveId, setApproveId] = useState<string | null>(null);
    const [isApproveModalOpen, setIsApproveModalOpen] = useState(false);

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

    const approveMutation = useMutation({
        mutationFn: (variables: { id: string, meetingLink: string }) => adminService.approveAdvisingRequest(variables.id, variables.meetingLink),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-advising'] });
            toast.success('Session request approved successfully');
            setIsApproveModalOpen(false);
            setApproveId(null);
        },
        onError: (error: any) => {
            console.error('[APPROVE ERROR]', error?.response?.status, error?.response?.data);
            toast.error('Failed to approve request');
        }
    });

    const rejectMutation = useMutation({
        mutationFn: (variables: { id: string, notes: string }) =>
            adminService.rejectAdvisingRequest(variables.id, variables.notes),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-advising'] });
            toast.success('Session request rejected successfully');
            setIsRejectModalOpen(false);
            setRejectId(null);
        },
        onError: () => toast.error('Failed to reject request')
    });

    const handleApproveInit = (id: string) => {
        setApproveId(id);
        setIsApproveModalOpen(true);
    };

    const confirmApprove = (meetingLink: string) => {
        if (approveId) {
            approveMutation.mutate({ id: approveId, meetingLink });
        }
    };

    const handleRejectInit = (id: string) => {
        setRejectId(id);
        setIsRejectModalOpen(true);
    };

    const confirmReject = (reason: string) => {
        if (rejectId) {
            rejectMutation.mutate({ id: rejectId, notes: reason });
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
            <div className="flex space-x-2 border-b border-slate-200 pb-4 overflow-x-auto">
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
                        <TableRow className="hover:bg-transparent border-slate-100">
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
                                <TableCell colSpan={5} className="h-32 text-center text-slate-500">Loading sessions...</TableCell>
                            </TableRow>
                        ) : items.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={5} className="h-32 text-center text-slate-500">
                                    No sessions found.
                                </TableCell>
                            </TableRow>
                        ) : (
                            items.map((req: GuidanceAdminDto) => (
                                <TableRow key={req.id}>
                                    <TableCell className="text-slate-700">
                                        <div className="flex flex-col gap-1">
                                            <div className="flex items-center gap-2 text-slate-900 font-medium">
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
                                        <div className="flex flex-col text-sm text-slate-700 space-y-3">
                                            {/* Alumni Info */}
                                            <div className="flex flex-col">
                                                <div className="flex items-center gap-1 text-slate-900 font-medium">
                                                    <User className="w-4 h-4 text-slate-400" />
                                                    Alumni: {req.alumniName}
                                                </div>
                                                <div className="text-xs text-slate-500 pl-5">
                                                    {req.alumniEmail || "No Email"}
                                                </div>
                                            </div>

                                            {/* Advisor Info */}
                                            <div className="flex flex-col">
                                                <div className="flex items-center gap-1 text-xs text-slate-700 font-medium">
                                                    <User className="w-3 h-3 text-slate-400" />
                                                    Advisor: {req.advisorName}
                                                </div>
                                                <div className="text-xs text-slate-500 pl-4">
                                                    {req.advisorEmail || "No Email"}
                                                </div>
                                            </div>
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <div className="flex flex-col">
                                            <span className="text-slate-900 font-medium">{req.subject}</span>
                                            {req.notes && (
                                                <div className="flex items-center gap-1 text-xs text-slate-500 mt-1 truncate max-w-[200px]" title={req.notes}>
                                                    <MessageSquare className="w-3 h-3" />
                                                    {req.notes}
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
                                                    <Button size="icon" variant="ghost" className="h-8 w-8 text-emerald-400 hover:bg-emerald-500/20" onClick={() => handleApproveInit(req.id)}>
                                                        <Check className="w-4 h-4" />
                                                    </Button>
                                                    <Button size="icon" variant="ghost" className="h-8 w-8 text-red-400 hover:bg-red-500/20" onClick={() => handleRejectInit(req.id)}>
                                                        <X className="w-4 h-4" />
                                                    </Button>
                                                </>
                                            )}
                                        </div>
                                    </TableCell>
                                </TableRow>
                            ))
                        )}
                    </TableBody>
                </Table>
            </DataTableShell>

            <ApproveModal
                open={isApproveModalOpen}
                onOpenChange={setIsApproveModalOpen}
                onConfirm={confirmApprove}
                isLoading={approveMutation.isPending}
            />

            <ReasonModal
                open={isRejectModalOpen}
                onOpenChange={setIsRejectModalOpen}
                onConfirm={confirmReject}
                isLoading={rejectMutation.isPending}
                title="Reject Session"
                description="Please provide a reason for rejecting this session request."
            />
        </div>
    );
};

export default GuidanceManager;
