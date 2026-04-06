import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { MembershipRequestStatus } from '../../types/membership';
import { AdvisoryStatus } from '../../types/admin';
import type { AssociationRequestDto, EligibilityCheckDto } from '../../types/membership';
import { Check, X, FileText, AlertTriangle, CheckCircle2, XCircle, User, Star, ShieldCheck, Facebook, Linkedin } from 'lucide-react';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/Table';
import { Button } from '../../components/ui/Button';
import { Input } from '../../components/ui/Input';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '../../components/ui/Dialog';
import { toast } from 'sonner';
import { PageHeader } from '../../components/admin/PageHeader';
import { DataTableShell } from '../../components/admin/DataTableShell';
import { StatusBadge } from '../../components/admin/StatusBadge';
import { AlumniImportModal } from '../../components/admin/AlumniImportModal';
import { DownloadCloud } from 'lucide-react';

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

// ── Advisor Review Modal ──
const AdvisorReviewModal = ({ alumni, isOpen, onClose, onApprove, onReject }: {
    alumni: any | null;
    isOpen: boolean;
    onClose: () => void;
    onApprove: (id: string) => void;
    onReject: (id: string, reason: string) => void;
}) => {
    const [rejectionReason, setRejectionReason] = useState('');
    const [isRejecting, setIsRejecting] = useState(false);

    if (!alumni) return null;

    return (
        <Dialog open={isOpen} onOpenChange={onClose}>
            <DialogContent className="sm:max-w-xl bg-slate-900 border-slate-800 text-slate-100">
                <DialogHeader>
                    <DialogTitle className="flex items-center gap-2 text-xl">
                        <ShieldCheck className="w-6 h-6 text-indigo-400" />
                        Review Advisor Application
                    </DialogTitle>
                    <DialogDescription className="text-slate-400">
                        Review professional background and expertise for <strong>{alumni.fullName}</strong>.
                    </DialogDescription>
                </DialogHeader>

                <div className="space-y-6 py-4">
                    {/* Bio Section */}
                    <div className="space-y-2">
                        <label className="text-sm font-semibold text-indigo-300 uppercase tracking-wider">Mentorship bio</label>
                        <div className="p-4 rounded-xl bg-slate-800/50 border border-slate-700 text-slate-200 text-sm leading-relaxed whitespace-pre-wrap">
                            {alumni.advisoryBio || "No bio provided."}
                        </div>
                    </div>

                    {/* Stats Row */}
                    <div className="grid grid-cols-2 gap-4">
                        <div className="p-3 rounded-lg bg-slate-800/30 border border-slate-700/50">
                            <div className="text-[10px] text-slate-500 uppercase font-bold mb-1">Experience</div>
                            <div className="text-lg font-semibold text-slate-100">{alumni.advisoryExperienceYears || 0} Years</div>
                        </div>
                        <div className="p-3 rounded-lg bg-slate-800/30 border border-slate-700/50">
                            <div className="text-[10px] text-slate-500 uppercase font-bold mb-1">Main Industry</div>
                            <div className="text-sm font-semibold text-indigo-400 font-mono truncate" title={alumni.expertiseNames?.join(', ')}>
                                {alumni.expertiseNames?.length > 0 ? alumni.expertiseNames.join(', ') : 'None Selected'}
                            </div>
                        </div>
                    </div>

                    {/* Social Links Verification */}
                    {(alumni.facebookUrl || alumni.linkedinUrl) && (
                        <div className="flex gap-4">
                            {alumni.facebookUrl && (
                                <a 
                                    href={alumni.facebookUrl.startsWith('http') ? alumni.facebookUrl : `https://${alumni.facebookUrl}`} 
                                    target="_blank" 
                                    rel="noreferrer"
                                    className="flex-1 flex items-center justify-center gap-3 p-3 rounded-xl bg-blue-600/10 border border-blue-600/20 text-blue-400 hover:bg-blue-600/20 transition-all group"
                                >
                                    <Facebook className="w-5 h-5 group-hover:scale-110 transition-transform" />
                                    <span className="text-xs font-semibold">Facebook Profile</span>
                                </a>
                            )}
                            {alumni.linkedinUrl && (
                                <a 
                                    href={alumni.linkedinUrl.startsWith('http') ? alumni.linkedinUrl : `https://${alumni.linkedinUrl}`} 
                                    target="_blank" 
                                    rel="noreferrer"
                                    className="flex-1 flex items-center justify-center gap-3 p-3 rounded-xl bg-[#0077B5]/10 border border-[#0077B5]/20 text-[#0077B5] hover:bg-[#0077B5]/20 transition-all group"
                                >
                                    <Linkedin className="w-5 h-5 group-hover:scale-110 transition-transform" />
                                    <span className="text-xs font-semibold">LinkedIn Profile</span>
                                </a>
                            )}
                        </div>
                    )}

                    {isRejecting && (
                        <div className="space-y-2 animate-in fade-in slide-in-from-top-1">
                            <label className="text-sm font-medium text-red-400">Rejection Reason</label>
                            <textarea
                                value={rejectionReason}
                                onChange={(e) => setRejectionReason(e.target.value)}
                                className="w-full p-3 rounded-lg bg-slate-800 border-red-500/30 focus:border-red-500 text-sm h-24 outline-none"
                                placeholder="Explain why the application was rejected..."
                            />
                        </div>
                    )}
                </div>

                <DialogFooter className="flex gap-2 sm:justify-between border-t border-slate-800 pt-6">
                    {isRejecting ? (
                        <>
                            <Button variant="ghost" onClick={() => setIsRejecting(false)}>Back</Button>
                            <Button 
                                variant="destructive" 
                                disabled={!rejectionReason.trim()}
                                onClick={() => onReject(alumni.id, rejectionReason)}
                            >
                                Confirm Rejection
                            </Button>
                        </>
                    ) : (
                        <>
                            <Button variant="outline" className="border-red-500/50 text-red-400 hover:bg-red-500/10" onClick={() => setIsRejecting(true)}>Reject</Button>
                            <div className="flex gap-2">
                                <Button variant="ghost" onClick={onClose}>Cancel</Button>
                                <Button className="bg-indigo-600 hover:bg-indigo-500" onClick={() => onApprove(alumni.id)}>Approve as Advisor</Button>
                            </div>
                        </>
                    )}
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
};

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
    const [view, setView] = useState<'requests' | 'profiles'>('requests');
    const [page, setPage] = useState(1);
    const pageSize = 10;

    // Rejection Modal State
    const [isRejectModalOpen, setIsRejectModalOpen] = useState(false);
    const [rejectRequestId, setRejectRequestId] = useState<string | null>(null);
    const [rejectReason, setRejectReason] = useState('');

    // Advisor Review State
    const [reviewAlumni, setReviewAlumni] = useState<any | null>(null);

    // Eligibility Drawer State
    const [drawerRequest, setDrawerRequest] = useState<AssociationRequestDto | null>(null);
    const [isDrawerOpen, setIsDrawerOpen] = useState(false);

    // Import Modal State
    const [isImportModalOpen, setIsImportModalOpen] = useState(false);

    const { data: requestsData, isLoading: isRequestsLoading } = useQuery({
        queryKey: ['admin-requests', filter, statusFilter, page],
        queryFn: () => adminService.getRequests({
            filter,
            status: statusFilter,
            sorting: 'requestDate desc',
            skipCount: (page - 1) * pageSize,
            maxResultCount: pageSize
        }),
        enabled: view === 'requests'
    });

    const { data: profilesData, isLoading: isProfilesLoading } = useQuery({
        queryKey: ['admin-profiles', filter, page],
        queryFn: () => adminService.getAlumniProfiles({
            filter,
            skipCount: (page - 1) * pageSize,
            maxResultCount: pageSize
        }),
        enabled: view === 'profiles'
    });

    const data = view === 'requests' ? requestsData : profilesData;
    const isLoading = view === 'requests' ? isRequestsLoading : isProfilesLoading;

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

    const toggleAdvisorMutation = useMutation({
        mutationFn: adminService.toggleAlumniAdvisor,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-profiles'] });
            toast.success('Advisor status updated!');
        },
        onError: () => {
            toast.error('Failed to update advisor status.');
        }
    });

    const approveAdvisorMutation = useMutation({
        mutationFn: adminService.approveAlumniAdvisor,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-profiles'] });
            toast.success('Advisor approved successfully!');
            setReviewAlumni(null);
        },
        onError: () => {
            toast.error('Failed to approve advisor.');
        }
    });

    const rejectAdvisorMutation = useMutation({
        mutationFn: ({ id, reason }: { id: string, reason: string }) => adminService.rejectAlumniAdvisor(id, reason),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-profiles'] });
            toast.success('Advisor application rejected');
            setReviewAlumni(null);
        },
        onError: () => {
            toast.error('Failed to reject advisor.');
        }
    });

    const toggleVipMutation = useMutation({
        mutationFn: adminService.toggleAlumniVip,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-profiles'] });
            toast.success('VIP status updated!');
        },
        onError: () => {
            toast.error('Failed to update VIP status.');
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

    const openReview = async (id: string) => {
        try {
            const fullProfile = await adminService.getAlumniProfile(id);
            setReviewAlumni(fullProfile);
        } catch (error) {
            toast.error('Failed to load profile details');
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
            <div className="flex justify-between items-center pr-4">
                <PageHeader
                    title={view === 'requests' ? "Membership Requests" : "Registered Alumni"}
                    description={view === 'requests' ? "Manage new alumni registration and membership requests." : "Manage active alumni profiles and special statuses."}
                />
                <div className="flex gap-2">
                    <Button 
                        variant={view === 'requests' ? 'default' : 'outline'}
                        onClick={() => { setView('requests'); setPage(1); }}
                        className="gap-2"
                    >
                        Requests
                    </Button>
                    <Button 
                        variant={view === 'profiles' ? 'default' : 'outline'}
                        onClick={() => { setView('profiles'); setPage(1); }}
                        className="gap-2"
                    >
                        Active Alumni
                    </Button>
                    <Button 
                        onClick={() => setIsImportModalOpen(true)}
                        className="bg-indigo-600 hover:bg-indigo-700 text-white gap-2 ml-4"
                    >
                        <DownloadCloud className="w-4 h-4" />
                        Bulk Import
                    </Button>
                </div>
            </div>

            {/* Status Tabs (only for requests) */}
            {view === 'requests' && (
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
            )}

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
                            {view === 'requests' ? (
                                <>
                                    <TableHead className="text-slate-700 dark:text-slate-300">Subscription</TableHead>
                                    <TableHead className="text-slate-700 dark:text-slate-300">Fee</TableHead>
                                    <TableHead className="text-slate-700 dark:text-slate-300">Eligibility</TableHead>
                                </>
                            ) : (
                                <>
                                    <TableHead className="text-slate-700 dark:text-slate-300">Contact</TableHead>
                                    <TableHead className="text-slate-700 dark:text-slate-300 text-center">VIP</TableHead>
                                    <TableHead className="text-slate-700 dark:text-slate-300 text-center">Advisor</TableHead>
                                </>
                            )}
                            <TableHead className="text-slate-700 dark:text-slate-300">Status</TableHead>
                            <TableHead className="text-right text-slate-700 dark:text-slate-300">Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {isLoading ? (
                            <TableRow>
                                <TableCell colSpan={6} className="h-32 text-center text-slate-400">
                                    Loading {view === 'requests' ? 'requests' : 'alumni'}...
                                </TableCell>
                            </TableRow>
                        ) : (data?.items?.length ?? 0) === 0 ? (
                            <TableRow>
                                <TableCell colSpan={6} className="h-32 text-center text-slate-500">
                                    No {view === 'requests' ? 'requests' : 'alumni'} found.
                                </TableCell>
                            </TableRow>
                        ) : (
                            data?.items.map((item: any) => (
                                <TableRow key={item.id} className="hover:bg-slate-50 dark:hover:bg-slate-800/50">
                                    {/* Alumni Column */}
                                    <TableCell>
                                        <div className="flex items-center gap-3">
                                            <div className="w-9 h-9 rounded-full bg-gradient-to-br from-indigo-500 to-purple-600 flex items-center justify-center text-white text-xs font-bold shrink-0 overflow-hidden">
                                                {item.alumniPhotoUrl || item.photoUrl ? (
                                                    <img src={item.alumniPhotoUrl || item.photoUrl} alt="" className="w-full h-full object-cover" />
                                                ) : (
                                                    (item.alumniName || item.fullName)?.charAt(0) || <User className="w-4 h-4" />
                                                )}
                                            </div>
                                            <div className="min-w-0">
                                                <div className="font-medium text-slate-900 dark:text-white truncate">
                                                    {item.alumniName || item.fullName || 'Unknown'}
                                                </div>
                                                <div className="text-xs text-slate-500 truncate">
                                                    {item.alumniNationalId || item.nationalId ? `ID: ${item.alumniNationalId || item.nationalId}` : ''}
                                                </div>
                                            </div>
                                        </div>
                                    </TableCell>

                                    {view === 'requests' ? (
                                        <>
                                            <TableCell>
                                                <span className="font-medium text-slate-900 dark:text-slate-300">
                                                    {item.subscriptionFeeName}
                                                </span>
                                            </TableCell>
                                            <TableCell>
                                                <span className="font-bold text-slate-900 dark:text-white">
                                                    {(item.remainingAmount ?? 0).toLocaleString()} EGP
                                                </span>
                                            </TableCell>
                                            <TableCell>
                                                <EligibilityIndicator
                                                    summary={item.eligibilitySummary}
                                                    checks={item.eligibilityChecks || []}
                                                    onClick={() => openDrawer(item)}
                                                />
                                            </TableCell>
                                        </>
                                    ) : (
                                        <>
                                            <TableCell>
                                                <div className="text-xs space-y-0.5">
                                                    <div className="text-slate-300 truncate max-w-[200px]">{item.email}</div>
                                                    <div className="text-slate-500">{item.mobileNumber}</div>
                                                </div>
                                            </TableCell>
                                            <TableCell className="text-center">
                                                <button 
                                                    onClick={() => toggleVipMutation.mutate(item.id)}
                                                    className={`p-1.5 rounded-full transition-colors cursor-pointer ${item.isVip ? 'text-amber-400 bg-amber-400/10' : 'text-slate-600 hover:text-slate-400 hover:bg-slate-700/30'}`}
                                                    title={item.isVip ? "Remove VIP" : "Mark as VIP"}
                                                >
                                                    <Star className={`w-5 h-5 ${item.isVip ? 'fill-current' : ''}`} />
                                                </button>
                                            </TableCell>
                                            <TableCell className="text-center">
                                                {item.advisoryStatus === AdvisoryStatus.Requested ? (
                                                    <Button 
                                                        size="sm" 
                                                        variant="outline" 
                                                        className="h-8 border-indigo-500/50 text-indigo-400 hover:bg-indigo-500/10"
                                                        onClick={() => openReview(item.id)}
                                                    >
                                                        Review
                                                    </Button>
                                                ) : (
                                                    <button 
                                                        onClick={() => toggleAdvisorMutation.mutate(item.id)}
                                                        className={`p-1.5 rounded-full transition-colors cursor-pointer ${item.advisoryStatus === AdvisoryStatus.Approved ? 'text-indigo-400 bg-indigo-400/10' : 'text-slate-600 hover:text-slate-400 hover:bg-slate-700/30'}`}
                                                        title={item.advisoryStatus === AdvisoryStatus.Approved ? "Demote from Advisor" : "Promote to Advisor"}
                                                    >
                                                        <ShieldCheck className={`w-5 h-5 ${item.advisoryStatus === AdvisoryStatus.Approved ? 'fill-current' : ''}`} />
                                                    </button>
                                                )}
                                            </TableCell>
                                        </>
                                    )}

                                    <TableCell>
                                        {view === 'requests' ? (
                                            <StatusBadge variant={getStatusVariant(item.status)}>
                                                {getStatusLabel(item.status)}
                                            </StatusBadge>
                                        ) : (
                                            <StatusBadge variant="success">Active</StatusBadge>
                                        )}
                                    </TableCell>

                                    <TableCell className="text-right">
                                        <div className="flex justify-end gap-1">
                                            {view === 'requests' && (
                                                <Button
                                                    size="icon"
                                                    variant="ghost"
                                                    title="View Details"
                                                    onClick={() => openDrawer(item)}
                                                >
                                                    <FileText className="w-4 h-4 text-slate-400" />
                                                </Button>
                                            )}

                                            {view === 'requests' && (item.status === MembershipRequestStatus.Pending || item.status === MembershipRequestStatus.Paid) && (
                                                <>
                                                    <Button
                                                        size="icon"
                                                        variant="ghost"
                                                        className="text-emerald-400 hover:text-emerald-300 hover:bg-emerald-500/20"
                                                        onClick={() => handleApprove(item.id)}
                                                        disabled={approveMutation.isPending}
                                                        title="Approve"
                                                    >
                                                        <Check className="w-4 h-4" />
                                                    </Button>
                                                    <Button
                                                        size="icon"
                                                        variant="ghost"
                                                        className="text-red-400 hover:text-red-300 hover:bg-red-500/20"
                                                        onClick={() => openRejectModal(item.id)}
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

            {/* Import Modal */}
            <AlumniImportModal
                isOpen={isImportModalOpen}
                onClose={() => setIsImportModalOpen(false)}
                onSuccess={() => {
                    queryClient.invalidateQueries({ queryKey: ['admin-requests'] });
                }}
            />
            <AdvisorReviewModal 
                alumni={reviewAlumni}
                isOpen={!!reviewAlumni}
                onClose={() => setReviewAlumni(null)}
                onApprove={(id) => approveAdvisorMutation.mutate(id)}
                onReject={(id, reason) => rejectAdvisorMutation.mutate({ id, reason })}
            />
        </div>
    );
};

export default AlumniManager;
