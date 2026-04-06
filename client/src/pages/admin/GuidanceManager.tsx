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
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '../../components/ui/Card';
import { Label } from '../../components/ui/Label';
import { Input } from '../../components/ui/Input';
import { Settings, ListFilter, Save, Building } from 'lucide-react';
import { cn } from '../../utils/cn';

const GuidanceManager = () => {
    const queryClient = useQueryClient();
    const [page, setPage] = useState(1);
    const pageSize = 10;
    const [statusFilter, setStatusFilter] = useState<'all' | 'pending' | 'approved' | 'rejected' | 'completed'>('all');

    const [rejectId, setRejectId] = useState<string | null>(null);
    const [isRejectModalOpen, setIsRejectModalOpen] = useState(false);

    const [approveId, setApproveId] = useState<string | null>(null);
    const [isApproveModalOpen, setIsApproveModalOpen] = useState(false);
    
    // Tabs & Settings State
    const [activeTab, setActiveTab] = useState<'requests' | 'settings'>('requests');
    const [selectedBranchId, setSelectedBranchId] = useState<string | null>(null);
    const [isSavingSettings, setIsSavingSettings] = useState(false);

    // Form Stats
    const [startTime, setStartTime] = useState('09:00');
    const [endTime, setEndTime] = useState('17:00');
    const [duration, setDuration] = useState(30);
    const [activeDays, setActiveDays] = useState<number[]>([]); // 0=Sunday, 1=Monday...

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
        }),
        enabled: activeTab === 'requests'
    });

    // Fetch Branches for settings
    const { data: branchesData } = useQuery({
        queryKey: ['admin-branches'],
        queryFn: () => adminService.getBranches({ maxResultCount: 100 }),
        enabled: activeTab === 'settings'
    });

    // Fetch and sync Rule when branch changes
    const { isLoading: isLoadingRule } = useQuery({
        queryKey: ['admin-guidance-rule', selectedBranchId],
        queryFn: async () => {
            if (!selectedBranchId) return null;
            const res = await adminService.getGuidanceRule(selectedBranchId);
            
            // Sync form state
            if (res) {
                setStartTime(res.startTime.substring(0, 5));
                setEndTime(res.endTime.substring(0, 5));
                setDuration(res.sessionDurationMinutes);
                setActiveDays(res.weekDays || []);
            }
            return res;
        },
        enabled: activeTab === 'settings' && !!selectedBranchId
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

    const handleSaveSettings = async () => {
        if (!selectedBranchId) {
            toast.error("Please select a branch first");
            return;
        }

        setIsSavingSettings(true);
        try {
            await adminService.updateGuidanceRule({
                branchId: selectedBranchId,
                startTime: startTime + ":00",
                endTime: endTime + ":00",
                sessionDurationMinutes: duration,
                weekDays: activeDays
            });
            toast.success("Office hours updated successfully");
            queryClient.invalidateQueries({ queryKey: ['admin-guidance-rule', selectedBranchId] });
        } catch (error) {
            toast.error("Failed to update office hours");
        } finally {
            setIsSavingSettings(false);
        }
    };

    const toggleDay = (dayIndex: number) => {
        setActiveDays(prev => 
            prev.includes(dayIndex) 
                ? prev.filter(d => d !== dayIndex) 
                : [...prev, dayIndex].sort((a,b) => a - b)
        );
    };

    const days = [
        { label: 'Sun', value: 0 },
        { label: 'Mon', value: 1 },
        { label: 'Tue', value: 2 },
        { label: 'Wed', value: 3 },
        { label: 'Thu', value: 4 },
        { label: 'Fri', value: 5 },
        { label: 'Sat', value: 6 },
    ];

    const items = data?.items || [];

    return (
        <div className="space-y-8 animate-in fade-in duration-500">
            <PageHeader
                title="Guidance & Advising"
                description="Manage mentorship and career guidance sessions."
            />

            {/* Main Tabs */}
            <div className="flex items-center gap-1 p-1 bg-slate-100 rounded-lg w-fit">
                <Button
                    variant={activeTab === 'requests' ? 'default' : 'ghost'}
                    size="sm"
                    className="flex items-center gap-2"
                    onClick={() => setActiveTab('requests')}
                >
                    <ListFilter className="w-4 h-4" />
                    Session Requests
                </Button>
                <Button
                    variant={activeTab === 'settings' ? 'default' : 'ghost'}
                    size="sm"
                    className="flex items-center gap-2"
                    onClick={() => setActiveTab('settings')}
                >
                    <Settings className="w-4 h-4" />
                    Office Hours
                </Button>
            </div>

            {activeTab === 'requests' ? (
                <>
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
            </>
            ) : (
                <div className="grid grid-cols-1 lg:grid-cols-3 gap-8 animate-in slide-in-from-bottom-4 duration-500">
                    {/* Branch Selection */}
                    <Card className="lg:col-span-1 border-slate-200 shadow-sm">
                        <CardHeader>
                            <CardTitle className="text-lg flex items-center gap-2">
                                <Building className="w-5 h-5 text-primary" />
                                Branch Rules
                            </CardTitle>
                            <CardDescription>Select a branch to configure its advising availability.</CardDescription>
                        </CardHeader>
                        <CardContent className="space-y-2">
                           {branchesData?.items?.map(branch => (
                               <button
                                 key={branch.id}
                                 onClick={() => setSelectedBranchId(branch.id)}
                                 className={cn(
                                     "w-full text-left px-4 py-3 rounded-lg transition-all border",
                                     selectedBranchId === branch.id 
                                        ? "bg-[var(--color-accent-light)]/10 border-[var(--color-accent)] text-[var(--color-accent)] font-bold shadow-sm" 
                                        : "bg-white border-slate-100 text-slate-600 hover:border-slate-200"
                                 )}
                               >
                                   {branch.name}
                               </button>
                           ))}
                        </CardContent>
                    </Card>

                    {/* Rule Form */}
                    <Card className="lg:col-span-2 border-slate-200 shadow-sm relative overflow-hidden">
                        {isLoadingRule && (
                            <div className="absolute inset-0 bg-white/60 backdrop-blur-[2px] z-10 flex items-center justify-center">
                                <div className="flex items-center gap-2 text-slate-500">
                                    <div className="w-4 h-4 rounded-full border-2 border-primary border-t-transparent animate-spin" />
                                    Loading rules...
                                </div>
                            </div>
                        )}
                        <CardHeader>
                            <CardTitle className="text-lg flex items-center gap-2">
                                <CalendarClock className="w-5 h-5 text-primary" />
                                Time & Availability
                            </CardTitle>
                            <CardDescription>Define the window of time when sessions can be booked.</CardDescription>
                        </CardHeader>
                        <CardContent className="space-y-8">
                            {!selectedBranchId ? (
                                <div className="h-64 flex flex-col items-center justify-center text-slate-400 gap-2 border-2 border-dashed border-slate-100 rounded-xl">
                                    <Building className="w-12 h-12 opacity-20" />
                                    <p>Select a branch to see details</p>
                                </div>
                            ) : (
                                <>
                                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                                        <div className="space-y-2">
                                            <Label>Opening Time</Label>
                                            <Input type="time" value={startTime} onChange={e => setStartTime(e.target.value)} />
                                        </div>
                                        <div className="space-y-2">
                                            <Label>Closing Time</Label>
                                            <Input type="time" value={endTime} onChange={e => setEndTime(e.target.value)} />
                                        </div>
                                    </div>

                                    <div className="space-y-3">
                                        <Label>Session Duration (Minutes)</Label>
                                        <div className="flex gap-2">
                                            {[15, 30, 45, 60].map(m => (
                                                <Button 
                                                    key={m} 
                                                    variant={duration === m ? 'default' : 'outline'}
                                                    className="flex-1"
                                                    onClick={() => setDuration(m)}
                                                >
                                                    {m}m
                                                </Button>
                                            ))}
                                        </div>
                                    </div>

                                    <div className="space-y-4">
                                        <div className="flex flex-col gap-1">
                                            <Label className="text-sm font-semibold text-slate-900">Weekly Schedule</Label>
                                            <p className="text-xs text-slate-500">Select the days this branch is open for guidance sessions.</p>
                                        </div>
                                        <div className="flex flex-wrap gap-2.5 py-1">
                                            {days.map(day => (
                                                <button
                                                    key={day.value}
                                                    type="button"
                                                    onClick={() => toggleDay(day.value)}
                                                    className={cn(
                                                        "h-12 w-12 flex items-center justify-center rounded-xl text-xs font-bold transition-all duration-200 border-2",
                                                        activeDays.includes(day.value)
                                                            ? "bg-[var(--color-accent)] border-[var(--color-accent)] text-white shadow-lg shadow-[var(--color-accent)]/20 scale-105"
                                                            : "bg-white border-slate-100 text-slate-400 hover:border-slate-300 hover:text-slate-600"
                                                    )}
                                                >
                                                    {day.label.toUpperCase()}
                                                </button>
                                            ))}
                                        </div>
                                        <p className="text-[10px] text-slate-400 italic">
                                            Tip: Selected days will be clickable in the Alumni booking calendar.
                                        </p>
                                    </div>

                                    <div className="pt-4 border-t border-slate-100 flex justify-end">
                                        <Button 
                                            onClick={handleSaveSettings} 
                                            disabled={isSavingSettings}
                                            className="min-w-[140px] gap-2"
                                        >
                                            {isSavingSettings ? (
                                                <div className="w-4 h-4 rounded-full border-2 border-white border-t-transparent animate-spin" />
                                            ) : (
                                                <Save className="w-4 h-4" />
                                            )}
                                            {isSavingSettings ? 'Saving...' : 'Save Settings'}
                                        </Button>
                                    </div>
                                </>
                            )}
                        </CardContent>
                    </Card>
                </div>
            )}

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
