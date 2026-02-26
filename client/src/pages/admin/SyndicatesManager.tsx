import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { SyndicateStatus } from '../../types/syndicates';
import { PlayCircle, Package, CheckCircle2, X, FileText, DollarSign, Building2, User, ArrowRight, CreditCard, Truck } from 'lucide-react';
import { PageHeader } from '../../components/admin/PageHeader';
import { DataTableShell } from '../../components/admin/DataTableShell';
import { StatusBadge } from '../../components/admin/StatusBadge';
import { Button } from '../../components/ui/Button';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/Table';
import { toast } from 'sonner';
import { ReasonModal } from '../../components/admin/ReasonModal';
import { ConfirmDialog } from '../../components/ui/ConfirmDialog';
import { ReviewSyndicateModal } from '../../components/admin/syndicates/ReviewSyndicateModal';

/**
 * Syndicate Subscription Lifecycle:
 * Draft(-1) → [alumni pays] → Pending(0) → [admin: Start Review] → Reviewing(1)
 *   → [admin: Mark Ready] → CardReady(3) → [admin: Mark Received] → Received(5)
 *   At any point: [admin: Reject with reason] → Rejected(4)
 */

const statusLabel = (status: number) => {
    switch (status) {
        case SyndicateStatus.Draft: return 'Draft';
        case SyndicateStatus.Pending: return 'Pending';
        case SyndicateStatus.Reviewing: return 'Reviewing';
        case SyndicateStatus.SentToSyndicate: return 'Sent to Syndicate';
        case SyndicateStatus.CardReady: return 'Card Ready';
        case SyndicateStatus.Rejected: return 'Rejected';
        case SyndicateStatus.Received: return 'Received';
        default: return 'Unknown';
    }
};

const statusVariant = (status: number): 'warning' | 'success' | 'destructive' | 'default' => {
    switch (status) {
        case SyndicateStatus.Draft: return 'default';
        case SyndicateStatus.Pending: return 'warning';
        case SyndicateStatus.Reviewing: return 'default';
        case SyndicateStatus.SentToSyndicate: return 'default';
        case SyndicateStatus.CardReady: return 'success';
        case SyndicateStatus.Rejected: return 'destructive';
        case SyndicateStatus.Received: return 'success';
        default: return 'default';
    }
};

const SyndicatesManager = () => {
    const queryClient = useQueryClient();
    const [page, setPage] = useState(1);
    const pageSize = 10;
    const [statusFilter, setStatusFilter] = useState<number | undefined>(undefined);

    // Action confirm dialog
    const [confirmAction, setConfirmAction] = useState<{
        open: boolean;
        title: string;
        description: string;
        onConfirm: () => void;
    }>({ open: false, title: '', description: '', onConfirm: () => { } });

    // Reject modal
    const [rejectId, setRejectId] = useState<string | null>(null);
    const [isRejectModalOpen, setIsRejectModalOpen] = useState(false);

    // Review Modal
    const [selectedSubscription, setSelectedSubscription] = useState<any | null>(null);

    const { data, isLoading } = useQuery({
        queryKey: ['admin-syndicates', statusFilter, page],
        queryFn: () => adminService.getSyndicateRequests({
            skipCount: (page - 1) * pageSize,
            maxResultCount: pageSize,
            statusFilter: statusFilter
        })
    });

    const invalidate = () => queryClient.invalidateQueries({ queryKey: ['admin-syndicates'] });

    const markInProgressMutation = useMutation({
        mutationFn: (id: string) => adminService.markSyndicateInProgress(id),
        onSuccess: () => { invalidate(); toast.success('Application approved and processing started.'); setSelectedSubscription(null); },
        onError: (err: any) => toast.error(err?.response?.data?.error?.message || 'Failed to update status.')
    });

    const markReadyMutation = useMutation({
        mutationFn: (id: string) => adminService.markSyndicateReadyForPickup(id),
        onSuccess: () => { invalidate(); toast.success('Card marked as Ready for Pickup.'); setConfirmAction(a => ({ ...a, open: false })); },
        onError: (err: any) => toast.error(err?.response?.data?.error?.message || 'Failed to update status.')
    });

    const markReceivedMutation = useMutation({
        mutationFn: (id: string) => adminService.markSyndicateReceived(id),
        onSuccess: () => { invalidate(); toast.success('Marked as Received.'); setConfirmAction(a => ({ ...a, open: false })); },
        onError: (err: any) => toast.error(err?.response?.data?.error?.message || 'Failed to update status.')
    });

    const rejectMutation = useMutation({
        mutationFn: (vars: { id: string; reason: string }) => adminService.rejectSyndicate(vars.id, vars.reason),
        onSuccess: () => { invalidate(); toast.success('Application rejected.'); setIsRejectModalOpen(false); setRejectId(null); setSelectedSubscription(null); },
        onError: () => toast.error('Failed to reject application.')
    });

    const handleReviewClick = (req: any) => {
        setSelectedSubscription(req);
    };

    const showConfirm = (title: string, description: string, onConfirm: () => void) => {
        setConfirmAction({ open: true, title, description, onConfirm });
    };

    const handleRejectClick = (id: string) => {
        setRejectId(id);
        setIsRejectModalOpen(true);
    };

    /** Render the correct action button(s) based on current status */
    const renderActions = (req: any) => {
        const status = req.status as number;

        switch (status) {
            case SyndicateStatus.Pending:
                return (
                    <div className="flex justify-end gap-1">
                        <Button
                            size="sm"
                            variant="ghost"
                            className="text-blue-600 hover:text-blue-700 hover:bg-blue-50 gap-1 text-xs"
                            onClick={() => handleReviewClick(req)}
                        >
                            <PlayCircle className="w-3.5 h-3.5" />
                            Review
                        </Button>
                        <Button
                            size="sm"
                            variant="ghost"
                            className="text-red-600 hover:text-red-700 hover:bg-red-50 gap-1 text-xs"
                            onClick={() => handleRejectClick(req.id)}
                        >
                            <X className="w-3.5 h-3.5" />
                            Reject
                        </Button>
                    </div>
                );

            case SyndicateStatus.Reviewing:
                return (
                    <div className="flex justify-end gap-1">
                        <Button
                            size="sm"
                            variant="ghost"
                            className="text-green-600 hover:text-green-700 hover:bg-green-50 gap-1 text-xs"
                            onClick={() => showConfirm(
                                'Mark Card Ready',
                                'Mark this subscription card as ready for pickup? The alumni will be notified.',
                                () => markReadyMutation.mutate(req.id)
                            )}
                        >
                            <Package className="w-3.5 h-3.5" />
                            Card Ready
                        </Button>
                        <Button
                            size="sm"
                            variant="ghost"
                            className="text-red-600 hover:text-red-700 hover:bg-red-50 gap-1 text-xs"
                            onClick={() => handleRejectClick(req.id)}
                        >
                            <X className="w-3.5 h-3.5" />
                            Reject
                        </Button>
                    </div>
                );

            case SyndicateStatus.CardReady:
                return (
                    <div className="flex justify-end gap-1">
                        <Button
                            size="sm"
                            variant="ghost"
                            className="text-emerald-600 hover:text-emerald-700 hover:bg-emerald-50 gap-1 text-xs"
                            onClick={() => showConfirm(
                                'Mark as Received',
                                'Confirm that the alumni has received their syndicate card?',
                                () => markReceivedMutation.mutate(req.id)
                            )}
                        >
                            <CheckCircle2 className="w-3.5 h-3.5" />
                            Received
                        </Button>
                    </div>
                );

            default:
                return <span className="text-xs text-slate-400">—</span>;
        }
    };

    const tabs = [
        { label: 'All', value: undefined },
        { label: 'Pending', value: SyndicateStatus.Pending },
        { label: 'Reviewing', value: SyndicateStatus.Reviewing },
        { label: 'Card Ready', value: SyndicateStatus.CardReady },
        { label: 'Received', value: SyndicateStatus.Received },
        { label: 'Rejected', value: SyndicateStatus.Rejected }
    ];

    return (
        <div className="space-y-8 animate-in fade-in duration-500">
            <PageHeader
                title="Syndicate Applications"
                description="Manage alumni syndicate subscription requests through the approval workflow."
            />

            {/* Workflow Visual */}
            <div className="flex items-center gap-2 text-xs text-slate-500 bg-slate-50 dark:bg-white/5 rounded-lg px-4 py-2.5 border border-slate-200 dark:border-white/10">
                <span className="font-medium text-slate-700 dark:text-slate-300">Workflow:</span>
                <span className="text-amber-600 font-medium">Pending</span>
                <ArrowRight className="w-3 h-3" />
                <span className="text-blue-600 font-medium">Reviewing</span>
                <ArrowRight className="w-3 h-3" />
                <span className="text-green-600 font-medium">Card Ready</span>
                <ArrowRight className="w-3 h-3" />
                <span className="text-emerald-600 font-medium">Received</span>
                <span className="mx-2">|</span>
                <span className="text-red-500 font-medium">Rejected</span>
                <span className="text-slate-400">(at any stage)</span>
            </div>

            {/* Status Tabs */}
            <div className="flex flex-wrap gap-2 border-b border-slate-200 dark:border-white/10 pb-4">
                {tabs.map((tab) => (
                    <Button
                        key={tab.label}
                        variant={statusFilter === tab.value ? 'default' : 'ghost'}
                        size="sm"
                        onClick={() => {
                            setStatusFilter(tab.value);
                            setPage(1);
                        }}
                    >
                        {tab.label}
                    </Button>
                ))}
            </div>

            <DataTableShell
                searchPlaceholder="Search applications..."
                onSearch={() => { }}
                pagination={{
                    currentPage: page,
                    totalPages: Math.ceil((data?.totalCount || 0) / pageSize),
                    onPageChange: setPage
                }}
            >
                <Table>
                    <TableHeader>
                        <TableRow className="hover:bg-transparent">
                            <TableHead>Date</TableHead>
                            <TableHead>Syndicate</TableHead>
                            <TableHead>Alumni</TableHead>
                            <TableHead>Fee</TableHead>
                            <TableHead>Payment</TableHead>
                            <TableHead>Delivery</TableHead>
                            <TableHead>Status</TableHead>
                            <TableHead>Notes</TableHead>
                            <TableHead className="text-right">Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {isLoading ? (
                            <TableRow>
                                <TableCell colSpan={9} className="h-32 text-center text-slate-500">Loading requests...</TableCell>
                            </TableRow>
                        ) : data?.items?.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={9} className="h-32 text-center text-slate-500">
                                    No applications found.
                                </TableCell>
                            </TableRow>
                        ) : (
                            data?.items?.map((req: any) => (
                                <TableRow key={req.id}>
                                    <TableCell className="text-slate-700 dark:text-slate-300 whitespace-nowrap">
                                        <div className="flex items-center gap-2">
                                            <FileText className="w-4 h-4 text-slate-400" />
                                            {new Date(req.creationTime).toLocaleDateString()}
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <div className="flex items-center gap-2">
                                            <Building2 className="w-4 h-4 text-blue-500" />
                                            <span className="text-slate-900 dark:text-white font-semibold">{req.syndicateName || '—'}</span>
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <div className="flex items-center gap-2">
                                            <User className="w-4 h-4 text-slate-400" />
                                            <div className="flex flex-col">
                                                <span className="text-slate-900 dark:text-white font-medium">{req.alumniName}</span>
                                            </div>
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <div className="flex items-center gap-1 text-slate-700 dark:text-slate-300 font-medium">
                                            <DollarSign className="w-3.5 h-3.5 text-green-600" />
                                            <span>{req.feeAmount?.toFixed(2) ?? '0.00'}</span>
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <div className="flex items-center gap-1.5">
                                            <CreditCard className={`w-3.5 h-3.5 ${req.paymentStatus === 1 ? 'text-green-600' : 'text-red-500'}`} />
                                            <StatusBadge variant={req.paymentStatus === 1 ? 'success' : 'destructive'}>
                                                {req.paymentStatus === 1 ? 'Paid' : req.paymentStatus === 2 ? 'Pending' : 'Not Paid'}
                                            </StatusBadge>
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <div className="flex items-center gap-1.5 text-sm text-slate-600 dark:text-slate-300">
                                            <Truck className="w-3.5 h-3.5 text-slate-400" />
                                            <span>{req.deliveryMethod === 0 ? 'Office Pickup' : req.deliveryMethod === 1 ? 'Delivery' : '—'}</span>
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <StatusBadge variant={statusVariant(req.status)}>
                                            {statusLabel(req.status)}
                                        </StatusBadge>
                                    </TableCell>
                                    <TableCell>
                                        <span className="text-sm text-slate-500 dark:text-slate-400 truncate max-w-[150px] block" title={req.adminNotes}>
                                            {req.adminNotes || '—'}
                                        </span>
                                    </TableCell>
                                    <TableCell className="text-right">
                                        {renderActions(req)}
                                    </TableCell>
                                </TableRow>
                            ))
                        )}
                    </TableBody>
                </Table>
            </DataTableShell>

            {/* Action Confirmation */}
            <ConfirmDialog
                open={confirmAction.open}
                onOpenChange={(open) => setConfirmAction(a => ({ ...a, open }))}
                title={confirmAction.title}
                description={confirmAction.description}
                onConfirm={confirmAction.onConfirm}
                confirmLabel="Confirm"
                isLoading={markInProgressMutation.isPending || markReadyMutation.isPending || markReceivedMutation.isPending}
            />

            {/* Reject Modal */}
            <ReasonModal
                open={isRejectModalOpen}
                onOpenChange={setIsRejectModalOpen}
                onConfirm={(reason) => {
                    if (rejectId) rejectMutation.mutate({ id: rejectId, reason });
                }}
                isLoading={rejectMutation.isPending}
                title="Reject Application"
                description="Please provide a reason for rejecting this syndicate application."
            />

            {/* Review Modal */}
            <ReviewSyndicateModal
                open={!!selectedSubscription}
                onOpenChange={(open) => !open && setSelectedSubscription(null)}
                subscription={selectedSubscription}
                onApprove={(id) => markInProgressMutation.mutate(id)}
                onReject={(id) => {
                    setRejectId(id);
                    setIsRejectModalOpen(true);
                }}
                isApproving={markInProgressMutation.isPending}
                isRejecting={rejectMutation.isPending}
            />
        </div>
    );
};

export default SyndicatesManager;
