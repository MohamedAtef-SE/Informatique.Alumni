import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { MembershipRequestStatus } from '../../types/membership';
import { Check, X, Search, Filter } from 'lucide-react';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/Table';
import { Button } from '../../components/ui/Button';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/Card';
import { cn } from '../../utils/cn';
import { toast } from 'sonner';

const AlumniManager = () => {
    const queryClient = useQueryClient();
    const [filter, setFilter] = useState('');
    const [statusFilter, setStatusFilter] = useState<MembershipRequestStatus | undefined>(MembershipRequestStatus.Pending);

    const { data } = useQuery({
        queryKey: ['admin-requests', filter, statusFilter],
        queryFn: () => adminService.getRequests({
            filter,
            status: statusFilter,
            sorting: 'requestDate desc',
            maxResultCount: 20
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

    const handleReject = (id: string) => {
        const reason = prompt('Enter rejection reason:');
        if (reason) {
            rejectMutation.mutate({ id, reason });
        }
    };

    const statusColors: Record<number, string> = {
        [MembershipRequestStatus.Pending]: 'bg-amber-500/20 text-amber-500 border-amber-500/20',
        [MembershipRequestStatus.Approved]: 'bg-emerald-500/20 text-emerald-500 border-emerald-500/20',
        [MembershipRequestStatus.Rejected]: 'bg-red-500/20 text-red-500 border-red-500/20',
        [MembershipRequestStatus.Paid]: 'bg-blue-500/20 text-blue-500 border-blue-500/20',
    };

    const statusLabels: Record<number, string> = {
        [MembershipRequestStatus.Pending]: 'Pending',
        [MembershipRequestStatus.Approved]: 'Approved',
        [MembershipRequestStatus.Rejected]: 'Rejected',
        [MembershipRequestStatus.Paid]: 'Paid',
    };

    return (
        <div className="space-y-8 animate-fade-in">
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
                <div>
                    <h1 className="text-3xl font-heading font-bold text-white">
                        Membership Requests
                    </h1>
                    <p className="text-slate-400 mt-1">Manage new alumni registration and membership requests.</p>
                </div>
            </div>

            <Card variant="glass">
                <CardHeader className="flex flex-col md:flex-row gap-4 justify-between items-start md:items-center border-b border-white/5 pb-6">
                    <CardTitle>Requests List</CardTitle>
                    <div className="flex gap-3 w-full md:w-auto">
                        <div className="relative group w-full md:w-64">
                            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-500 group-focus-within:text-accent transition-colors" />
                            <input
                                type="text"
                                placeholder="Search by name..."
                                className="w-full bg-slate-900/50 border border-white/10 rounded-lg pl-10 pr-4 py-2 text-sm text-white focus:outline-none focus:border-accent focus:ring-1 focus:ring-accent transition-all"
                                value={filter}
                                onChange={(e) => setFilter(e.target.value)}
                            />
                        </div>
                        <div className="relative min-w-[140px]">
                            <Filter className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-500" />
                            <select
                                className="w-full pl-10 pr-4 py-2 bg-slate-900/50 border border-white/10 rounded-lg text-sm text-white focus:outline-none focus:border-accent appearance-none cursor-pointer"
                                value={statusFilter}
                                onChange={(e) => setStatusFilter(e.target.value ? Number(e.target.value) as MembershipRequestStatus : undefined)}
                            >
                                <option value="">All Status</option>
                                <option value={MembershipRequestStatus.Pending}>Pending</option>
                                <option value={MembershipRequestStatus.Approved}>Approved</option>
                                <option value={MembershipRequestStatus.Rejected}>Rejected</option>
                            </select>
                        </div>
                    </div>
                </CardHeader>
                <CardContent className="p-0">
                    <Table>
                        <TableHeader>
                            <TableRow className="hover:bg-transparent border-white/5">
                                <TableHead className="w-[200px]">Request Date</TableHead>
                                <TableHead>Fee Type</TableHead>
                                <TableHead>Amount</TableHead>
                                <TableHead>Status</TableHead>
                                <TableHead className="text-right">Actions</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {data?.items.length === 0 ? (
                                <TableRow>
                                    <TableCell colSpan={5} className="h-32 text-center text-slate-500">
                                        No requests found matching your criteria.
                                    </TableCell>
                                </TableRow>
                            ) : (
                                data?.items.map((req) => (
                                    <TableRow key={req.id}>
                                        <TableCell>
                                            <div className="font-medium text-white">
                                                {new Date(req.requestDate).toLocaleDateString()}
                                            </div>
                                            <div className="text-xs text-slate-500">
                                                {new Date(req.requestDate).toLocaleTimeString()}
                                            </div>
                                        </TableCell>
                                        <TableCell>
                                            <span className="font-medium text-slate-300">{req.subscriptionFeeName}</span>
                                        </TableCell>
                                        <TableCell>
                                            <span className="font-bold text-white">{req.remainingAmount} EGP</span>
                                        </TableCell>
                                        <TableCell>
                                            <span className={cn(
                                                "px-2.5 py-1 rounded-full text-xs font-bold border",
                                                statusColors[req.status] || 'bg-gray-500/20 text-gray-500 border-gray-500/20'
                                            )}>
                                                {statusLabels[req.status] || 'Unknown'}
                                            </span>
                                        </TableCell>
                                        <TableCell className="text-right">
                                            {req.status === MembershipRequestStatus.Pending && (
                                                <div className="flex justify-end gap-2">
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
                                                        onClick={() => handleReject(req.id)}
                                                        disabled={rejectMutation.isPending}
                                                        title="Reject"
                                                    >
                                                        <X className="w-4 h-4" />
                                                    </Button>
                                                </div>
                                            )}
                                        </TableCell>
                                    </TableRow>
                                ))
                            )}
                        </TableBody>
                    </Table>
                </CardContent>
            </Card>
        </div>
    );
};

export default AlumniManager;
