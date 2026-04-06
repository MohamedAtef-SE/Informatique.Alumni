import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { FileDown, CheckCircle, XCircle, User, MapPin, GraduationCap, Eye, Briefcase, Phone, CreditCard, Building, Info } from 'lucide-react';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/Table';
import { Button } from '../../components/ui/Button';
import { toast } from 'sonner';
import { PageHeader } from '../../components/admin/PageHeader';
import { DataTableShell } from '../../components/admin/DataTableShell';
import { StatusBadge } from '../../components/admin/StatusBadge';
import { ConfirmDialog } from '../../components/ui/ConfirmDialog';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '../../components/ui/Dialog';
import { RegistrationStatus, type ActivityParticipantDto } from '../../types/events';

const ParticipantManager = () => {
    const queryClient = useQueryClient();
    const [filter, setFilter] = useState('');
    const [page, setPage] = useState(1);
    const [pageSize] = useState(10);
    
    const [selectedParticipant, setSelectedParticipant] = useState<ActivityParticipantDto | null>(null);
    const [confirmAction, setConfirmAction] = useState<{
        type: 'approve' | 'reject';
        participantId: string;
        participantName: string;
    } | null>(null);

    const { data, isLoading } = useQuery({
        queryKey: ['admin-event-participants', filter, page],
        queryFn: async () => {
            return await adminService.getEventParticipants({
                activityName: filter,
                skipCount: (page - 1) * pageSize,
                maxResultCount: pageSize,
                sorting: 'CreationTime DESC'
            });
        }
    });

    const approveMutation = useMutation({
        mutationFn: adminService.approveEventRegistration,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-event-participants'] });
            toast.success('Registration Approved!');
            setConfirmAction(null);
            setSelectedParticipant(null);
        },
        onError: () => toast.error('Failed to approve registration.')
    });

    const rejectMutation = useMutation({
        mutationFn: (id: string) => adminService.rejectEventRegistration(id, 'Rejected by Administrator'),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-event-participants'] });
            toast.success('Registration Rejected.');
            setConfirmAction(null);
            setSelectedParticipant(null);
        },
        onError: () => toast.error('Failed to reject registration.')
    });

    const handleExport = () => {
        if (!data?.items.length) return;
        
        const headers = ['Alumni Name', 'Mobile', 'Graduation Year', 'College', 'Job Title', 'Company', 'Event', 'Status', 'Applied Date'];
        const csvContent = [
            headers.join(','),
            ...data.items.map(p => [
                `"${p.alumniName}"`,
                `"${p.mobileNumber}"`,
                p.graduationYear,
                `"${p.collegeName}"`,
                `"${p.jobTitle || ''}"`,
                `"${p.company || ''}"`,
                `"${p.eventName}"`,
                getStatusLabel(p.status),
                new Date(p.creationTime).toLocaleDateString()
            ].join(','))
        ].join('\n');

        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        const link = document.createElement('a');
        link.href = URL.createObjectURL(blob);
        link.setAttribute('download', `Event_Participants_${new Date().toISOString().split('T')[0]}.csv`);
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        toast.info('Export started...');
    };

    const getStatusLabel = (status: RegistrationStatus) => {
        const s = status as any;
        switch (s) {
            case 0: return 'Pending';
            case 1: return 'Confirmed';
            case 2: return 'Cancelled';
            case 3: return 'Attended';
            default: return 'Unknown';
        }
    };

    const getStatusVariant = (status: RegistrationStatus): any => {
        const s = status as any;
        switch (s) {
            case 0: return 'warning';
            case 1: return 'success';
            case 2: return 'destructive';
            case 3: return 'info';
            default: return 'default';
        }
    };

    return (
        <div className="space-y-6 animate-in fade-in duration-500 pb-10">
            <PageHeader
                title="Event Registrations"
                description="Review and manage alumni participation requests for community events."
                action={
                    <Button variant="outline" onClick={handleExport} disabled={!data?.totalCount}>
                        <FileDown className="w-4 h-4 mr-2" /> Export Excel
                    </Button>
                }
            />

            <DataTableShell
                searchPlaceholder="Search by event or alumni name..."
                onSearch={setFilter}
                pagination={{
                    currentPage: page,
                    totalPages: Math.ceil((data?.totalCount || 0) / pageSize),
                    onPageChange: setPage
                }}
            >
                <div className="rounded-xl border bg-white shadow-sm overflow-hidden border-slate-200">
                    <Table>
                        <TableHeader>
                            <TableRow className="bg-slate-50 border-slate-200">
                                <TableHead className="text-slate-700 font-bold py-4">Alumni & Contact</TableHead>
                                <TableHead className="text-slate-700 font-bold">Professional</TableHead>
                                <TableHead className="text-slate-700 font-bold">Event Info</TableHead>
                                <TableHead className="text-slate-700 font-bold">Applied Date</TableHead>
                                <TableHead className="text-slate-700 font-bold">Status</TableHead>
                                <TableHead className="text-right text-slate-700 font-bold">Actions</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {isLoading ? (
                                <TableRow>
                                    <TableCell colSpan={6} className="h-48 text-center text-slate-400">
                                        <div className="flex flex-col items-center gap-2">
                                            <div className="w-8 h-8 border-2 border-red-600 border-t-transparent rounded-full animate-spin" />
                                            Loading participants...
                                        </div>
                                    </TableCell>
                                </TableRow>
                            ) : data?.items.length === 0 ? (
                                <TableRow>
                                    <TableCell colSpan={6} className="h-48 text-center text-slate-500">
                                        No registrations found matching your criteria.
                                    </TableCell>
                                </TableRow>
                            ) : (
                                data?.items.map((participant: ActivityParticipantDto) => (
                                    <TableRow key={participant.id} className="border-slate-100 hover:bg-slate-50/80 transition-colors group">
                                        <TableCell>
                                            <div className="flex items-center gap-3">
                                                <div className="w-10 h-10 rounded-full bg-slate-100 flex items-center justify-center border border-slate-200">
                                                    <User className="w-5 h-5 text-slate-600" />
                                                </div>
                                                <div>
                                                    <div className="font-bold text-slate-900 leading-tight">{participant.alumniName}</div>
                                                    <div className="text-xs text-slate-600 flex items-center mt-0.5">
                                                        <Phone className="w-3 h-3 mr-1 text-slate-400" />
                                                        {participant.mobileNumber}
                                                    </div>
                                                </div>
                                            </div>
                                        </TableCell>
                                        <TableCell>
                                            <div className="space-y-1">
                                                <div className="text-sm font-semibold text-slate-800 flex items-center gap-1.5">
                                                    <Briefcase className="w-3.5 h-3.5 text-slate-400" />
                                                    {participant.jobTitle || 'N/A'}
                                                </div>
                                                <div className="text-xs text-slate-600 flex items-center gap-1.5">
                                                    <Building className="w-3.5 h-3.5 text-slate-400" />
                                                    {participant.company || 'N/A'}
                                                </div>
                                            </div>
                                        </TableCell>
                                        <TableCell>
                                            <div className="space-y-1">
                                                <div className="font-bold text-slate-800">{participant.eventName}</div>
                                                <div className="flex items-center gap-3 text-xs text-slate-500">
                                                    <span className="flex items-center gap-1">
                                                        <MapPin className="w-3 h-3 text-red-500" />
                                                        {participant.location}
                                                    </span>
                                                </div>
                                            </div>
                                        </TableCell>
                                        <TableCell>
                                            <div className="text-sm text-slate-600 font-medium">
                                                {new Date(participant.creationTime).toLocaleDateString()}
                                            </div>
                                            <div className="text-[10px] text-slate-400">
                                                {new Date(participant.creationTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                                            </div>
                                        </TableCell>
                                        <TableCell>
                                            <StatusBadge variant={getStatusVariant(participant.status)}>
                                                {getStatusLabel(participant.status)}
                                            </StatusBadge>
                                            {(participant.paidAmount ?? 0) > 0 && (
                                                <div className="text-[10px] text-emerald-600 mt-1 font-bold flex items-center gap-1">
                                                    <CreditCard className="w-3 h-3" />
                                                    Paid: {participant.paidAmount} EGP
                                                </div>
                                            )}
                                        </TableCell>
                                        <TableCell className="text-right">
                                            <div className="flex justify-end gap-1.5 opacity-0 group-hover:opacity-100 transition-opacity">
                                                <Button
                                                    size="icon"
                                                    variant="ghost"
                                                    className="w-8 h-8 text-blue-600 hover:text-blue-700 hover:bg-blue-50 border border-transparent hover:border-blue-100"
                                                    onClick={() => setSelectedParticipant(participant)}
                                                    title="View Full Profile"
                                                >
                                                    <Eye className="w-4 h-4" />
                                                </Button>
                                                {participant.status === 0 && (
                                                    <>
                                                        <Button
                                                            size="icon"
                                                            variant="ghost"
                                                            className="w-8 h-8 text-emerald-600 hover:text-emerald-700 hover:bg-emerald-50 border border-transparent hover:border-emerald-100"
                                                            onClick={() => setConfirmAction({ 
                                                                type: 'approve', 
                                                                participantId: participant.id, 
                                                                participantName: participant.alumniName 
                                                            })}
                                                            title="Approve"
                                                        >
                                                            <CheckCircle className="w-4 h-4" />
                                                        </Button>
                                                        <Button
                                                            size="icon"
                                                            variant="ghost"
                                                            className="w-8 h-8 text-red-600 hover:text-red-700 hover:bg-red-50 border border-transparent hover:border-red-100"
                                                            onClick={() => setConfirmAction({ 
                                                                type: 'reject', 
                                                                participantId: participant.id, 
                                                                participantName: participant.alumniName 
                                                            })}
                                                            title="Reject"
                                                        >
                                                            <XCircle className="w-4 h-4" />
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
                </div>
            </DataTableShell>

            {/* Details Modal */}
            <Dialog open={!!selectedParticipant} onOpenChange={(open) => !open && setSelectedParticipant(null)}>
                <DialogContent className="max-w-3xl overflow-y-auto max-h-[90vh]">
                    <DialogHeader>
                        <DialogTitle className="text-2xl font-bold text-slate-800 border-b pb-4">
                            Participant Detailed Profile
                        </DialogTitle>
                    </DialogHeader>
                    {selectedParticipant && (
                        <div className="space-y-6 pt-4">
                            <div className="flex items-start justify-between bg-slate-50 p-4 rounded-xl border border-slate-100">
                                <div className="flex items-center gap-4">
                                    <div className="w-16 h-16 rounded-full bg-white flex items-center justify-center border shadow-sm">
                                        <User className="w-8 h-8 text-slate-400" />
                                    </div>
                                    <div>
                                        <h3 className="text-xl font-bold text-slate-900">{selectedParticipant.alumniName}</h3>
                                        <p className="text-slate-600 flex items-center gap-1.5 mt-1 font-medium">
                                            <GraduationCap className="w-4 h-4 text-red-500" />
                                            Medicine - Class of {selectedParticipant.graduationYear}
                                        </p>
                                    </div>
                                </div>
                                <StatusBadge variant={getStatusVariant(selectedParticipant.status)}>
                                    {getStatusLabel(selectedParticipant.status)}
                                </StatusBadge>
                            </div>

                            <div className="grid grid-cols-2 gap-6">
                                <div className="space-y-4">
                                    <section>
                                        <h4 className="text-sm font-bold text-slate-400 uppercase tracking-wider mb-2 flex items-center gap-2">
                                            <Briefcase className="w-4 h-4" /> Professional Experience
                                        </h4>
                                        <div className="bg-slate-50 p-3 rounded-lg border border-slate-100">
                                            <div className="font-bold text-slate-900 text-lg">{selectedParticipant.jobTitle || 'No job title provided'}</div>
                                            <div className="text-slate-700 font-medium">{selectedParticipant.company || 'No company info'}</div>
                                        </div>
                                    </section>

                                    <section>
                                        <h4 className="text-sm font-bold text-slate-400 uppercase tracking-wider mb-2 flex items-center gap-2">
                                            <Info className="w-4 h-4" /> Identity & Verification
                                        </h4>
                                        <div className="space-y-2">
                                            <div className="flex justify-between text-sm">
                                                <span className="text-slate-500 font-medium">National ID:</span>
                                                <span className="text-slate-900 font-bold tracking-widest">{selectedParticipant.nationalId || 'Not provided'}</span>
                                            </div>
                                            <div className="flex justify-between text-sm">
                                                <span className="text-slate-500 font-medium">Mobile Number:</span>
                                                <span className="text-slate-900 font-bold">{selectedParticipant.mobileNumber}</span>
                                            </div>
                                        </div>
                                    </section>
                                </div>

                                <div className="space-y-4">
                                    <section>
                                        <h4 className="text-sm font-bold text-slate-400 uppercase tracking-wider mb-2 flex items-center gap-2">
                                            <MapPin className="w-4 h-4" /> Contact Address
                                        </h4>
                                        <div className="bg-slate-50 p-3 rounded-lg border border-slate-100 min-h-[80px]">
                                            <p className="text-slate-700 text-sm leading-relaxed font-medium">
                                                {selectedParticipant.address || 'No residential address provided on file.'}
                                            </p>
                                        </div>
                                    </section>

                                    <section>
                                        <h4 className="text-sm font-bold text-slate-400 uppercase tracking-wider mb-2 flex items-center gap-2">
                                            <CreditCard className="w-4 h-4" /> Payment Details
                                        </h4>
                                        <div className="bg-emerald-50 p-3 rounded-lg border border-emerald-100">
                                            <div className="flex justify-between items-center">
                                                <span className="text-emerald-700 font-bold">Method:</span>
                                                <span className="text-emerald-900 font-bold capitalize">{selectedParticipant.paymentMethod || 'Manual/Admin'}</span>
                                            </div>
                                            <div className="flex justify-between items-center mt-1">
                                                <span className="text-emerald-700 font-bold">Amount Paid:</span>
                                                <span className="text-emerald-900 text-lg font-black">{selectedParticipant.paidAmount || 0} EGP</span>
                                            </div>
                                        </div>
                                    </section>
                                </div>
                            </div>

                            <div className="pt-6 border-t flex justify-between gap-4">
                                <Button variant="outline" onClick={() => setSelectedParticipant(null)}>
                                    Close Viewer
                                </Button>
                                {selectedParticipant.status === 0 && (
                                    <div className="flex gap-2">
                                        <Button 
                                            variant="destructive" 
                                            onClick={() => setConfirmAction({ 
                                                type: 'reject', 
                                                participantId: selectedParticipant.id, 
                                                participantName: selectedParticipant.alumniName 
                                            })}
                                        >
                                            Reject Request
                                        </Button>
                                        <Button 
                                            className="bg-emerald-600 hover:bg-emerald-700 border-none shadow-lg shadow-emerald-600/20"
                                            onClick={() => setConfirmAction({ 
                                                type: 'approve', 
                                                participantId: selectedParticipant.id, 
                                                participantName: selectedParticipant.alumniName 
                                            })}
                                        >
                                            Approve Registration
                                        </Button>
                                    </div>
                                )}
                            </div>
                        </div>
                    )}
                </DialogContent>
            </Dialog>

            <ConfirmDialog
                open={!!confirmAction}
                onOpenChange={(open) => !open && setConfirmAction(null)}
                title={confirmAction?.type === 'approve' ? 'Approve Registration' : 'Reject Registration'}
                description={`Are you sure you want to ${confirmAction?.type} the registration for ${confirmAction?.participantName}?`}
                confirmLabel={confirmAction?.type === 'approve' ? 'Approve' : 'Reject'}
                variant={confirmAction?.type === 'approve' ? 'default' : 'danger'}
                onConfirm={() => {
                    if (confirmAction?.type === 'approve') {
                        approveMutation.mutate(confirmAction.participantId);
                    } else {
                        rejectMutation.mutate(confirmAction!.participantId);
                    }
                }}
                isLoading={approveMutation.isPending || rejectMutation.isPending}
            />
        </div>
    );
};

export default ParticipantManager;
