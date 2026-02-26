import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { MembershipRequestStatus } from '../../types/membership';
import type { AssociationRequestDto, EligibilityCheckDto } from '../../types/membership';
import { Check, X, FileText, AlertTriangle, CheckCircle2, XCircle, User } from 'lucide-react';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/Table';
import { Button } from '../../components/ui/Button';
import { Input } from '../../components/ui/Input';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '../../components/ui/Dialog';
import { toast } from 'sonner';
import { PageHeader } from '../../components/admin/PageHeader';
import { DataTableShell } from '../../components/admin/DataTableShell';
import { StatusBadge } from '../../components/admin/StatusBadge';

// ── Eligibility Indicator Component ──
const EligibilityIndicator = ({ summary, checks, onClick }: {
    summary: string;
    checks: EligibilityCheckDto[];
    onClick: () => void;
}) => {
    const failCount = checks.filter(c => c.status === 'Fail').length;
    const warnCount = checks.filter(c => c.status === 'Warning').length;
    const passCount = checks.filter(c => c.status === 'Pass').length;

    const config = summary === 'AllClear'
        ? { bg: 'bg-emerald-500/15', border: 'border-emerald-500/30', text: 'text-emerald-400', icon: <CheckCircle2 className="w-4 h-4" />, label: 'All Clear' }
        : summary === 'NeedsReview'
            ? { bg: 'bg-amber-500/15', border: 'border-amber-500/30', text: 'text-amber-400', icon: <AlertTriangle className="w-4 h-4" />, label: 'Review' }
            : { bg: 'bg-red-500/15', border: 'border-red-500/30', text: 'text-red-400', icon: <XCircle className="w-4 h-4" />, label: 'Issues' };

    return (
        <button
            onClick={onClick}
            className={`flex items-center gap-2 px-3 py-1.5 rounded-full text-xs font-medium ${config.bg} ${config.border} ${config.text} border transition-all hover:scale-105 cursor-pointer`}
            title={`${passCount} pass, ${warnCount} warnings, ${failCount} failures — click for details`}
        >
            {config.icon}
            <span>{config.label}</span>
            {failCount > 0 && (
                <span className="bg-red-500/30 px-1.5 py-0.5 rounded-full text-[10px]">{failCount}</span>
            )}
            {warnCount > 0 && (
                <span className="bg-amber-500/30 px-1.5 py-0.5 rounded-full text-[10px]">{warnCount}</span>
            )}
        </button>
    );
};

// ── Eligibility Detail Drawer ──
const EligibilityDrawer = ({ request, isOpen, onClose }: {
    request: AssociationRequestDto | null;
    isOpen: boolean;
    onClose: () => void;
}) => {
    if (!request) return null;

    const getCheckIcon = (status: string) => {
        switch (status) {
            case 'Pass': return <CheckCircle2 className="w-5 h-5 text-emerald-400 shrink-0" />;
            case 'Warning': return <AlertTriangle className="w-5 h-5 text-amber-400 shrink-0" />;
            case 'Fail': return <XCircle className="w-5 h-5 text-red-400 shrink-0" />;
            default: return null;
        }
    };

    const getCheckBg = (status: string) => {
        switch (status) {
            case 'Pass': return 'bg-emerald-500/5 border-emerald-500/20';
            case 'Warning': return 'bg-amber-500/5 border-amber-500/20';
            case 'Fail': return 'bg-red-500/5 border-red-500/20';
            default: return 'bg-slate-500/5 border-slate-500/20';
        }
    };

    return (
        <Dialog open={isOpen} onOpenChange={onClose}>
            <DialogContent className="sm:max-w-lg">
                <DialogHeader>
                    <DialogTitle className="flex items-center gap-3">
                        <div className="w-10 h-10 rounded-full bg-gradient-to-br from-indigo-500 to-purple-600 flex items-center justify-center text-white text-sm font-bold">
                            {request.alumniName?.charAt(0) || '?'}
                        </div>
                        <div>
                            <div>{request.alumniName || 'Unknown'}</div>
                            <div className="text-xs font-normal text-slate-400">
                                ID: {request.alumniNationalId} · {request.collegeName || 'N/A'}, {request.graduationYear || '—'}
                            </div>
                        </div>
                    </DialogTitle>
                    <DialogDescription>
                        Eligibility checks for membership request
                    </DialogDescription>
                </DialogHeader>

                <div className="space-y-3 py-4 max-h-[400px] overflow-y-auto">
                    {request.eligibilityChecks?.map((check, idx) => (
                        <div
                            key={idx}
                            className={`flex items-start gap-3 p-3 rounded-lg border ${getCheckBg(check.status)}`}
                        >
                            {getCheckIcon(check.status)}
                            <div className="flex-1 min-w-0">
                                <div className="text-sm font-medium text-slate-200">{check.checkName}</div>
                                <div className="text-xs text-slate-400 mt-0.5">{check.message}</div>
                            </div>
                            <span className={`text-[10px] font-bold px-2 py-0.5 rounded-full shrink-0 ${check.status === 'Pass' ? 'bg-emerald-500/20 text-emerald-400'
                                    : check.status === 'Warning' ? 'bg-amber-500/20 text-amber-400'
                                        : 'bg-red-500/20 text-red-400'
                                }`}>
                                {check.status.toUpperCase()}
                            </span>
                        </div>
                    ))}
                </div>

                {/* Request Details */}
                <div className="border-t border-slate-700/50 pt-4 space-y-2 text-sm text-slate-400">
                    <div className="flex justify-between">
                        <span>Subscription</span>
                        <span className="text-slate-200">{request.subscriptionFeeName}</span>
                    </div>
                    <div className="flex justify-between">
                        <span>Fee Amount</span>
                        <span className="text-slate-200 font-semibold">{(request.remainingAmount ?? 0).toLocaleString()} EGP</span>
                    </div>
                    <div className="flex justify-between">
                        <span>Delivery</span>
                        <span className="text-slate-200">{request.deliveryMethod === 2 ? 'Home Delivery' : 'Office Pickup'}</span>
                    </div>
                    <div className="flex justify-between">
                        <span>Request Date</span>
                        <span className="text-slate-200">{new Date(request.requestDate).toLocaleDateString()}</span>
                    </div>
                    {request.rejectionReason && (
                        <div className="flex justify-between">
                            <span>Rejection Reason</span>
                            <span className="text-red-400">{request.rejectionReason}</span>
                        </div>
                    )}
                </div>

                <DialogFooter>
                    <Button variant="outline" onClick={onClose}>Close</Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
};

// ── Main Component ──
const AlumniManager = () => {
    const queryClient = useQueryClient();
    const [filter, setFilter] = useState('');
    const [statusFilter, setStatusFilter] = useState<MembershipRequestStatus | undefined>(MembershipRequestStatus.Pending);
    const [page, setPage] = useState(1);
    const pageSize = 10;

    // Rejection Modal State
    const [isRejectModalOpen, setIsRejectModalOpen] = useState(false);
    const [rejectRequestId, setRejectRequestId] = useState<string | null>(null);
    const [rejectReason, setRejectReason] = useState('');

    // Eligibility Drawer State
    const [drawerRequest, setDrawerRequest] = useState<AssociationRequestDto | null>(null);
    const [isDrawerOpen, setIsDrawerOpen] = useState(false);

    const { data, isLoading } = useQuery({
        queryKey: ['admin-requests', filter, statusFilter, page],
        queryFn: () => adminService.getRequests({
            filter,
            status: statusFilter,
            sorting: 'requestDate desc',
            skipCount: (page - 1) * pageSize,
            maxResultCount: pageSize
        })
    });

    const approveMutation = useMutation({
        mutationFn: adminService.approveRequest,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-requests'] });
            toast.success('Request Approved!');
        },
        onError: () => {
            toast.error('Failed to approve request.');
        }
    });

    const rejectMutation = useMutation({
        mutationFn: (variables: { id: string, reason: string }) => adminService.rejectRequest(variables.id, variables.reason),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-requests'] });
            setIsRejectModalOpen(false);
            setRejectReason('');
            setRejectRequestId(null);
            toast.success('Request Rejected.');
        },
        onError: () => {
            toast.error('Failed to reject request.');
        }
    });

    const handleApprove = (id: string) => {
        if (confirm('Are you sure you want to approve this request?')) {
            approveMutation.mutate(id);
        }
    };

    const openRejectModal = (id: string) => {
        setRejectRequestId(id);
        setIsRejectModalOpen(true);
    };

    const handleConfirmReject = () => {
        if (rejectRequestId && rejectReason.trim()) {
            rejectMutation.mutate({ id: rejectRequestId, reason: rejectReason });
        } else {
            toast.error("Please provide a rejection reason.");
        }
    };

    const openDrawer = (req: AssociationRequestDto) => {
        setDrawerRequest(req);
        setIsDrawerOpen(true);
    };

    const getStatusVariant = (status: MembershipRequestStatus) => {
        switch (status) {
            case MembershipRequestStatus.Pending: return 'warning';
            case MembershipRequestStatus.Paid: return 'info';
            case MembershipRequestStatus.Approved: return 'success';
            case MembershipRequestStatus.Rejected: return 'destructive';
            case MembershipRequestStatus.InProgress: return 'default';
            case MembershipRequestStatus.ReadyForPickup: return 'success';
            case MembershipRequestStatus.OutForDelivery: return 'warning';
            case MembershipRequestStatus.Delivered: return 'success';
            default: return 'default';
        }
    };

    const getStatusLabel = (status: MembershipRequestStatus) => {
        switch (status) {
            case MembershipRequestStatus.Pending: return 'Pending';
            case MembershipRequestStatus.Paid: return 'Paid';
            case MembershipRequestStatus.Approved: return 'Approved';
            case MembershipRequestStatus.Rejected: return 'Rejected';
            case MembershipRequestStatus.InProgress: return 'In Progress';
            case MembershipRequestStatus.ReadyForPickup: return 'Ready for Pickup';
            case MembershipRequestStatus.OutForDelivery: return 'Out for Delivery';
            case MembershipRequestStatus.Delivered: return 'Delivered';
            default: return 'Unknown';
        }
    }

    return (
        <div className="space-y-8 animate-in fade-in duration-500">
            <PageHeader
                title="Membership Requests"
                description="Manage new alumni registration and membership requests."
            />

            {/* Status Tabs */}
            <div className="flex space-x-2 border-b border-white/10 pb-4 overflow-x-auto">
                {[
                    { label: 'Pending Reviews', value: MembershipRequestStatus.Pending },
                    { label: 'Approved', value: MembershipRequestStatus.Approved },
                    { label: 'Rejected', value: MembershipRequestStatus.Rejected },
                    { label: 'All Requests', value: undefined }
                ].map((tab) => (
                    <Button
                        key={tab.label}
                        variant={statusFilter === tab.value ? 'default' : 'ghost'}
                        size="sm"
                        onClick={() => {
                            setStatusFilter(tab.value);
                            setPage(1);
                        }}
                        className="whitespace-nowrap"
                    >
                        {tab.label}
                    </Button>
                ))}
            </div>

            <DataTableShell
                searchPlaceholder="Search by name, email, or mobile..."
                onSearch={(val) => {
                    setFilter(val);
                    setPage(1);
                }}
                pagination={{
                    currentPage: page,
                    totalPages: Math.ceil((data?.totalCount || 0) / pageSize),
                    onPageChange: setPage
                }}
            >
                <Table>
                    <TableHeader>
                        <TableRow className="hover:bg-transparent border-white/5">
                            <TableHead className="w-[220px] text-slate-700 dark:text-slate-300">Alumni</TableHead>
                            <TableHead className="text-slate-700 dark:text-slate-300">Subscription</TableHead>
                            <TableHead className="text-slate-700 dark:text-slate-300">Fee</TableHead>
                            <TableHead className="text-slate-700 dark:text-slate-300">Eligibility</TableHead>
                            <TableHead className="text-slate-700 dark:text-slate-300">Status</TableHead>
                            <TableHead className="text-right text-slate-700 dark:text-slate-300">Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {isLoading ? (
                            <TableRow>
                                <TableCell colSpan={6} className="h-32 text-center text-slate-400">
                                    Loading requests...
                                </TableCell>
                            </TableRow>
                        ) : data?.items.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={6} className="h-32 text-center text-slate-500">
                                    No requests found in this category.
                                </TableCell>
                            </TableRow>
                        ) : (
                            data?.items.map((req) => (
                                <TableRow key={req.id} className="hover:bg-slate-50 dark:hover:bg-slate-800/50">
                                    {/* Alumni Column */}
                                    <TableCell>
                                        <div className="flex items-center gap-3">
                                            <div className="w-9 h-9 rounded-full bg-gradient-to-br from-indigo-500 to-purple-600 flex items-center justify-center text-white text-xs font-bold shrink-0 overflow-hidden">
                                                {req.alumniPhotoUrl ? (
                                                    <img src={req.alumniPhotoUrl} alt="" className="w-full h-full object-cover" />
                                                ) : (
                                                    req.alumniName?.charAt(0) || <User className="w-4 h-4" />
                                                )}
                                            </div>
                                            <div className="min-w-0">
                                                <div className="font-medium text-slate-900 dark:text-white truncate">
                                                    {req.alumniName || 'Unknown'}
                                                </div>
                                                <div className="text-xs text-slate-500 truncate">
                                                    {req.alumniNationalId !== '—' ? `ID: ${req.alumniNationalId}` : ''}
                                                    {req.collegeName && ` · ${req.collegeName}`}
                                                    {req.graduationYear && ` '${String(req.graduationYear).slice(-2)}`}
                                                </div>
                                            </div>
                                        </div>
                                    </TableCell>

                                    {/* Subscription Column */}
                                    <TableCell>
                                        <span className="font-medium text-slate-900 dark:text-slate-300">
                                            {req.subscriptionFeeName}
                                        </span>
                                        {req.deliveryMethod === 2 && (
                                            <div className="text-xs text-amber-600 dark:text-amber-400 mt-1 flex items-center gap-1">
                                                <span>+ Courier</span>
                                            </div>
                                        )}
                                    </TableCell>

                                    {/* Fee Column */}
                                    <TableCell>
                                        <span className="font-bold text-slate-900 dark:text-white">
                                            {(req.remainingAmount ?? 0).toLocaleString()} EGP
                                        </span>
                                    </TableCell>

                                    {/* Eligibility Column */}
                                    <TableCell>
                                        <EligibilityIndicator
                                            summary={req.eligibilitySummary}
                                            checks={req.eligibilityChecks || []}
                                            onClick={() => openDrawer(req)}
                                        />
                                    </TableCell>

                                    {/* Status Column */}
                                    <TableCell>
                                        <StatusBadge variant={getStatusVariant(req.status)}>
                                            {getStatusLabel(req.status)}
                                        </StatusBadge>
                                    </TableCell>

                                    {/* Actions Column */}
                                    <TableCell className="text-right">
                                        <div className="flex justify-end gap-1">
                                            <Button
                                                size="icon"
                                                variant="ghost"
                                                title="View Details"
                                                onClick={() => openDrawer(req)}
                                            >
                                                <FileText className="w-4 h-4 text-slate-400" />
                                            </Button>

                                            {(req.status === MembershipRequestStatus.Pending || req.status === MembershipRequestStatus.Paid) && (
                                                <>
                                                    <Button
                                                        size="icon"
                                                        variant="ghost"
                                                        className="text-emerald-400 hover:text-emerald-300 hover:bg-emerald-500/20"
                                                        onClick={() => handleApprove(req.id)}
                                                        disabled={approveMutation.isPending}
                                                        title="Approve"
                                                    >
                                                        <Check className="w-4 h-4" />
                                                    </Button>
                                                    <Button
                                                        size="icon"
                                                        variant="ghost"
                                                        className="text-red-400 hover:text-red-300 hover:bg-red-500/20"
                                                        onClick={() => openRejectModal(req.id)}
                                                        disabled={rejectMutation.isPending}
                                                        title="Reject"
                                                    >
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

            {/* Eligibility Detail Drawer */}
            <EligibilityDrawer
                request={drawerRequest}
                isOpen={isDrawerOpen}
                onClose={() => setIsDrawerOpen(false)}
            />

            {/* Rejection Modal */}
            <Dialog open={isRejectModalOpen} onOpenChange={setIsRejectModalOpen}>
                <DialogContent>
                    <DialogHeader>
                        <DialogTitle>Reject Request</DialogTitle>
                        <DialogDescription>
                            Please provide a reason for rejecting this membership request. The user will be notified.
                        </DialogDescription>
                    </DialogHeader>
                    <div className="space-y-4 py-4">
                        <div className="space-y-2">
                            <label className="text-sm font-medium leading-none text-slate-200">
                                Rejection Reason
                            </label>
                            <Input
                                value={rejectReason}
                                onChange={(e) => setRejectReason(e.target.value)}
                                placeholder="e.g., Invalid ID card photo, Missing documents..."
                                autoFocus
                            />
                        </div>
                    </div>
                    <DialogFooter>
                        <Button variant="outline" onClick={() => setIsRejectModalOpen(false)}>
                            Cancel
                        </Button>
                        <Button
                            variant="destructive"
                            onClick={handleConfirmReject}
                            isLoading={rejectMutation.isPending}
                        >
                            Confirm Rejection
                        </Button>
                    </DialogFooter>
                </DialogContent>
            </Dialog>
        </div>
    );
};

export default AlumniManager;
