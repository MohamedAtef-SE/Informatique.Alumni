import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { CertificateRequestStatus, CertificateLanguage, type CertificateRequestDto } from '../../types/certificates';
import { FileBadge, Check, Truck, Package, Loader2, CreditCard, User, Mail, Hash } from 'lucide-react';
import { PageHeader } from '../../components/admin/PageHeader';
import { DataTableShell } from '../../components/admin/DataTableShell';
import { StatusBadge } from '../../components/admin/StatusBadge';
import { Button } from '../../components/ui/Button';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/Table';
import { toast } from 'sonner';
import { ReasonModal } from '../../components/admin/ReasonModal';
import { ReviewCertificateModal } from '../../components/admin/certificates/ReviewCertificateModal';

const CertificatesManager = () => {
    const queryClient = useQueryClient();
    const [page, setPage] = useState(1);
    const pageSize = 10;
    const [statusFilter, setStatusFilter] = useState<'all' | 'pending' | 'processing' | 'ready' | 'delivered'>('all');

    const [rejectId, setRejectId] = useState<string | null>(null);
    const [isRejectModalOpen, setIsRejectModalOpen] = useState(false);

    // Review Modal
    const [selectedRequest, setSelectedRequest] = useState<CertificateRequestDto | null>(null);

    const { data, isLoading } = useQuery({
        queryKey: ['admin-certificates', statusFilter, page],
        queryFn: async () => {
            let statusParam: number | undefined = undefined;
            if (statusFilter === 'pending') statusParam = CertificateRequestStatus.PendingPayment;
            else if (statusFilter === 'processing') statusParam = CertificateRequestStatus.Processing;
            else if (statusFilter === 'ready') statusParam = CertificateRequestStatus.ReadyForPickup;
            else if (statusFilter === 'delivered') statusParam = CertificateRequestStatus.Delivered;

            const result = await adminService.getCertificateRequests({
                skipCount: (page - 1) * pageSize,
                maxResultCount: pageSize,
                status: statusParam
            });
            return result;
        }
    });

    const updateStatusMutation = useMutation({
        mutationFn: (variables: { id: string, status: CertificateRequestStatus, note?: string }) =>
            adminService.updateCertificateStatus(variables.id, { newStatus: variables.status, note: variables.note }),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-certificates'] });
            toast.success('Status updated successfully');
            setIsRejectModalOpen(false);
            setRejectId(null);
        },
        onError: () => toast.error('Failed to update status')
    });

    const handleStatusChange = (id: string, newStatus: CertificateRequestStatus) => {
        if (newStatus === CertificateRequestStatus.Rejected) {
            setRejectId(id);
            setIsRejectModalOpen(true);
            return;
        }

        const action = newStatus === CertificateRequestStatus.Processing ? 'Start processing' :
            newStatus === CertificateRequestStatus.ReadyForPickup ? 'Mark as Ready' :
                newStatus === CertificateRequestStatus.Delivered ? 'Mark as Delivered' : 'Update';

        if (confirm(`${action} for this request?`)) {
            updateStatusMutation.mutate({ id, status: newStatus });
        }
    };

    const confirmReject = (reason: string) => {
        if (rejectId) {
            updateStatusMutation.mutate({ id: rejectId, status: CertificateRequestStatus.Rejected, note: reason });
        }
    };

    // Use data items directly since backend handles filtering and pagination
    const displayItems = data?.items || [];

    return (
        <div className="space-y-8 animate-in fade-in duration-500">
            <PageHeader
                title="Certificate Requests"
                description="Manage official document requests and status workflow."
            />

            {/* Status Tabs */}
            <div className="flex space-x-2 border-b border-white/10 pb-4 overflow-x-auto">
                {[
                    { label: 'All', value: 'all' },
                    { label: 'Pending Payment', value: 'pending' },
                    { label: 'Processing', value: 'processing' },
                    { label: 'Ready for Pickup', value: 'ready' },
                    { label: 'Delivered', value: 'delivered' }
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
                searchPlaceholder="Search requests..."
                onSearch={() => { }}
                pagination={{
                    currentPage: page,
                    totalPages: Math.ceil((data?.totalCount || 0) / pageSize),
                    onPageChange: setPage
                }}
            >
                <Table>
                    <TableHeader>
                        <TableRow className="hover:bg-slate-50/50">
                            <TableHead>Req ID / Date</TableHead>
                            <TableHead>Requester Info</TableHead>
                            <TableHead>Request Details</TableHead>
                            <TableHead>Delivery & Branch</TableHead>
                            <TableHead>Payment</TableHead>
                            <TableHead>Status</TableHead>
                            <TableHead className="text-right">Workflow Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {isLoading ? (
                            <TableRow>
                                <TableCell colSpan={5} className="h-32 text-center text-slate-400">Loading requests...</TableCell>
                            </TableRow>
                        ) : displayItems.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={7} className="h-32 text-center text-slate-500">
                                    No requests found in this category.
                                </TableCell>
                            </TableRow>
                        ) : (
                            displayItems.map((req: CertificateRequestDto) => (
                                <TableRow key={req.id}>
                                    <TableCell className="align-top">
                                        <div className="flex flex-col">
                                            <span className="font-semibold text-slate-900 flex items-center gap-1"><Hash className="w-3.5 h-3.5 text-slate-400" />{req.id.substring(0, 8).toUpperCase()}</span>
                                            <span className="text-sm text-slate-600 mt-1">{req.creationTime ? new Date(req.creationTime).toLocaleDateString() : '-'}</span>
                                            <span className="text-xs text-slate-500">{req.creationTime ? new Date(req.creationTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) : ''}</span>
                                        </div>
                                    </TableCell>
                                    <TableCell className="align-top">
                                        <div className="flex flex-col">
                                            <span className="text-slate-900 font-semibold flex items-center gap-1.5">
                                                <User className="w-4 h-4 text-primary" />
                                                {req.alumniName && req.alumniName !== 'Unknown' ? req.alumniName : (req.alumniEmail ? req.alumniEmail.split('@')[0] : 'Alumni')}
                                            </span>
                                            {req.studentId && (
                                                <span className="text-xs text-slate-500 flex items-center gap-1 mt-1 truncate max-w-[180px]" title={req.studentId}>
                                                    <Hash className="w-3 h-3 text-slate-400" />
                                                    ID: {req.studentId.length > 8 ? req.studentId.substring(0, 8).toUpperCase() : req.studentId}
                                                </span>
                                            )}
                                            <span className="text-xs text-slate-500 flex items-center gap-1 truncate max-w-[180px] mt-0.5" title={req.alumniEmail}>
                                                <Mail className="w-3 h-3 text-slate-400" />
                                                {req.alumniEmail || '-'}
                                            </span>
                                        </div>
                                    </TableCell>
                                    <TableCell className="align-top">
                                        <div className="flex flex-col gap-2">
                                            {req.items?.map(i => (
                                                <div key={i.id} className="text-xs flex flex-col p-2.5 bg-slate-50 rounded-lg border border-slate-200">
                                                    <span className="text-slate-900 font-medium flex items-center gap-1.5"><FileBadge className="w-4 h-4 text-primary" />{i.certificateDefinitionName}</span>
                                                    {i.qualificationName && <span className="text-slate-600 ml-5 mt-0.5">• {i.qualificationName}</span>}
                                                    <div className="flex gap-2 ml-5 mt-1.5">
                                                        <span className="px-2 py-0.5 rounded-md bg-indigo-50 text-indigo-600 text-[11px] font-medium border border-indigo-100">
                                                            {i.language === CertificateLanguage.Arabic ? 'Arabic' : 'English'}
                                                        </span>
                                                        <span className="px-2 py-0.5 rounded-md bg-slate-100 text-slate-600 text-[11px] font-medium border border-slate-200">
                                                            EGP {i.fee}
                                                        </span>
                                                    </div>
                                                </div>
                                            ))}
                                            {req.userNotes && (
                                                <div className="text-xs text-amber-600 mt-1 italic line-clamp-2" title={req.userNotes}>Note: {req.userNotes}</div>
                                            )}
                                        </div>
                                    </TableCell>
                                    <TableCell className="align-top">
                                        <div className="flex flex-col text-sm text-slate-600">
                                            <span className="flex items-center gap-1.5 font-medium text-slate-900 mb-1">
                                                {req.deliveryMethod === 2 ? <Truck className="w-4 h-4 text-primary" /> : <Package className="w-4 h-4 text-indigo-500" />}
                                                {req.deliveryMethod === 2 ? 'Courier' : 'Pickup Office'}
                                            </span>
                                            {req.deliveryMethod === 2 && req.deliveryAddress && (
                                                <span className="text-xs text-slate-500 line-clamp-2 max-w-[150px]" title={req.deliveryAddress}>
                                                    {req.deliveryAddress}
                                                </span>
                                            )}
                                            {req.deliveryMethod !== 2 && req.targetBranchName && (
                                                <span className="text-xs text-slate-600 font-medium bg-slate-100 py-1 px-2 rounded-md w-fit border border-slate-200 mt-1" title={req.targetBranchName}>
                                                    {req.targetBranchName}
                                                </span>
                                            )}
                                        </div>
                                    </TableCell>
                                    <TableCell className="align-top">
                                        <div className="flex flex-col gap-1.5">
                                            <div className="text-sm font-semibold text-slate-900 flex items-center gap-1.5">
                                                <CreditCard className="w-4 h-4 text-emerald-500" />
                                                EGP {req.totalFees}
                                            </div>
                                            {req.remainingAmount > 0 ? (
                                                <span className="text-xs font-medium text-amber-700 bg-amber-50 border border-amber-200 px-2 py-1 rounded-md w-fit">
                                                    Unpaid: EGP {req.remainingAmount}
                                                </span>
                                            ) : (
                                                <span className="text-xs font-medium text-emerald-700 bg-emerald-50 border border-emerald-200 px-2 py-1 rounded-md w-fit flex items-center gap-1">
                                                    <Check className="w-3 h-3" /> Fully Paid
                                                </span>
                                            )}
                                        </div>
                                    </TableCell>
                                    <TableCell className="align-top">
                                        <StatusBadge variant={
                                            req.status === CertificateRequestStatus.Delivered ? 'success' :
                                                req.status === CertificateRequestStatus.Rejected ? 'destructive' :
                                                    req.status === CertificateRequestStatus.ReadyForPickup ? 'info' :
                                                        req.status === CertificateRequestStatus.Processing ? 'warning' : 'default'
                                        }>
                                            {Object.keys(CertificateRequestStatus).find(k => CertificateRequestStatus[k as keyof typeof CertificateRequestStatus] === req.status) || 'Unknown'}
                                        </StatusBadge>
                                    </TableCell>
                                    <TableCell className="text-right align-top">
                                        <div className="flex justify-end gap-2 items-center">
                                            {req.status === CertificateRequestStatus.PendingPayment && (
                                                <Button size="sm" variant="ghost" className="h-8 text-blue-600 hover:text-blue-700 hover:bg-blue-50" onClick={() => setSelectedRequest(req)}>
                                                    <Loader2 className="w-4 h-4 mr-1" /> Review
                                                </Button>
                                            )}
                                            {req.status === CertificateRequestStatus.Processing && (
                                                <Button size="sm" variant="outline" className="h-8 bg-blue-50 border-blue-200 text-blue-600 hover:bg-blue-100 shadow-sm" onClick={() => handleStatusChange(req.id, CertificateRequestStatus.ReadyForPickup)}>
                                                    <Package className="w-3 h-3 mr-1" /> Ready
                                                </Button>
                                            )}
                                            {req.status === CertificateRequestStatus.ReadyForPickup && (
                                                <Button size="sm" variant="outline" className="h-8 bg-emerald-50 border-emerald-200 text-emerald-600 hover:bg-emerald-100 shadow-sm" onClick={() => handleStatusChange(req.id, CertificateRequestStatus.Delivered)}>
                                                    <Check className="w-3 h-3 mr-1" /> Delivered
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
                title="Reject Request"
                description="Please provide a reason for rejecting this certificate request."
            />

            <ReviewCertificateModal
                open={!!selectedRequest}
                onOpenChange={(open) => !open && setSelectedRequest(null)}
                request={selectedRequest}
                onApprove={(id) => {
                    handleStatusChange(id, CertificateRequestStatus.Processing);
                    setSelectedRequest(null);
                }}
                onReject={(id) => {
                    setRejectId(id);
                    setIsRejectModalOpen(true);
                }}
                isApproving={updateStatusMutation.isPending}
                isRejecting={updateStatusMutation.isPending}
            />
        </div>
    );
};

export default CertificatesManager;
