import { useState } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from 'react-oidc-context';
import { useQuery, useMutation } from '@tanstack/react-query';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { adminService } from '../../services/adminService';
import { CommunicationChannel } from '../../types/communications';
import { Send, Mail, MessageSquare, Building2, Users, Calculator, ClipboardList } from 'lucide-react';
import { PageHeader } from '../../components/admin/PageHeader';
import { Button } from '../../components/ui/Button';
import { Input } from '../../components/ui/Input';
import { toast } from 'sonner';
import clsx from 'clsx';
import type { BranchDto } from '../../types/organization';
import CommunicationLogsModal from '../../components/admin/CommunicationLogsModal';

const schema = z.object({
    channel: z.nativeEnum(CommunicationChannel),
    subject: z.string().optional(),
    body: z.string().min(1, "Message body is required"),
    filter: z.object({
        branchId: z.string().optional(),
        graduationYear: z.string().optional(),
        graduationSemester: z.string().optional(),
        membershipStatus: z.string().optional()
    })
}).superRefine((data, ctx) => {
    if (data.channel === CommunicationChannel.Email && (!data.subject || data.subject.trim() === '')) {
        ctx.addIssue({
            code: z.ZodIssueCode.custom,
            message: "Subject is required for Email",
            path: ["subject"]
        });
    }
});

type FormData = z.infer<typeof schema>;

const CommunicationCenter = () => {
    const auth = useAuth();
    const roles = auth.user?.profile?.role;
    const roleArray = Array.isArray(roles) ? roles : roles ? [String(roles)] : [];
    const isSuperAdmin = roleArray.some(r => ['admin', 'systemadmin'].includes(r.toLowerCase()));

    const [recipientCount, setRecipientCount] = useState<number | null>(null);
    const [showLogs, setShowLogs] = useState(false);

    const { control, handleSubmit, watch, reset, formState: { errors } } = useForm<FormData>({
        resolver: zodResolver(schema),
        defaultValues: {
            channel: CommunicationChannel.Email,
            subject: '',
            body: '',
            filter: {
                branchId: '',
                graduationYear: '',
                graduationSemester: '',
                membershipStatus: '0' // All
            }
        }
    });

    const channel = watch('channel');
    const filter = watch('filter');

    const { data: branches } = useQuery({
        queryKey: ['admin-branches-list'],
        queryFn: () => adminService.getBranches({ maxResultCount: 100 })
    });

    const { data: graduationYears } = useQuery<number[]>({
        queryKey: ['distinct-graduation-years'],
        queryFn: () => adminService.getDistinctGraduationYears(),
        staleTime: 5 * 60 * 1000 // cache for 5 minutes
    });

    const countMutation = useMutation({
        mutationFn: (filterData: any) => adminService.getRecipientsCount(filterData),
        onSuccess: (count) => {
            setRecipientCount(count);
            toast.success(`Found ${count} recipients matching criteria.`);
        },
        onError: () => toast.error('Failed to calculate recipients.')
    });

    const sendMutation = useMutation({
        mutationFn: (data: any) => adminService.sendMessage(data),
        onSuccess: () => {
            toast.success('Message queued successfully!');
            setRecipientCount(null);
            reset();
        },
        onError: () => toast.error('Failed to send message.')
    });

    const buildFilterDto = (filterData: any) => {
        return {
            branchId: filterData.branchId || undefined,
            graduationYear: filterData.graduationYear ? parseInt(filterData.graduationYear) : undefined,
            graduationSemester: filterData.graduationSemester ? parseInt(filterData.graduationSemester) : undefined,
            membershipStatus: filterData.membershipStatus ? parseInt(filterData.membershipStatus) : 0
        };
    };

    const handlePreviewCount = () => {
        const payload = buildFilterDto(filter);
        countMutation.mutate(payload);
    };

    const onSubmit = (data: FormData) => {
        const payload = {
            channel: data.channel,
            subject: data.subject || '',
            body: data.body,
            filter: buildFilterDto(data.filter)
        };
        sendMutation.mutate(payload);
    };

    if (!isSuperAdmin) {
        return <Navigate to="/admin/dashboard" replace />;
    }

    return (
        <div className="space-y-8 animate-in fade-in duration-500">
            <div style={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', flexWrap: 'wrap', gap: 12 }}>
                <PageHeader
                    title="Communication Center"
                    description="Broadcast messages to alumni across different channels. Target your audience with precision."
                />
                <button
                    id="view-delivery-logs-btn"
                    type="button"
                    onClick={() => setShowLogs(true)}
                    style={{
                        display: 'inline-flex', alignItems: 'center', gap: 8,
                        padding: '10px 18px', borderRadius: 10, cursor: 'pointer',
                        background: 'rgba(99,102,241,0.15)', border: '1px solid rgba(99,102,241,0.35)',
                        color: '#818cf8', fontSize: 13, fontWeight: 600,
                        transition: 'all 0.2s', whiteSpace: 'nowrap'
                    }}
                    onMouseEnter={e => { (e.currentTarget as HTMLButtonElement).style.background = 'rgba(99,102,241,0.3)'; }}
                    onMouseLeave={e => { (e.currentTarget as HTMLButtonElement).style.background = 'rgba(99,102,241,0.15)'; }}
                >
                    <ClipboardList size={16} />
                    View Delivery Logs
                </button>
            </div>

            <form onSubmit={handleSubmit(onSubmit)} className="grid grid-cols-1 md:grid-cols-3 gap-8">
                <div className="md:col-span-2 space-y-6">
                    <div className="bg-white dark:bg-slate-900 rounded-2xl shadow-sm border border-slate-200 dark:border-slate-800 p-6 md:p-8 space-y-6 relative overflow-hidden transition-all duration-300 hover:shadow-md">
                        <div className="absolute top-0 left-0 w-full h-1 bg-gradient-to-r from-sky-500 to-indigo-500"></div>

                        <div>
                            <label className="block text-sm font-semibold text-slate-700 dark:text-slate-300 mb-3">Communication Channel</label>
                            <Controller
                                name="channel"
                                control={control}
                                render={({ field }) => (
                                    <div className="flex gap-4">
                                        <button
                                            type="button"
                                            onClick={() => field.onChange(CommunicationChannel.Email)}
                                            className={clsx(
                                                "flex items-center gap-2 px-5 py-2.5 rounded-xl border transition-all duration-200 font-medium",
                                                field.value === CommunicationChannel.Email
                                                    ? "border-sky-500 bg-sky-50 dark:bg-sky-500/10 text-sky-600 dark:text-sky-400 shadow-sm"
                                                    : "border-slate-200 dark:border-slate-700 text-slate-500 dark:text-slate-400 hover:border-slate-300 hover:bg-slate-50 dark:hover:bg-slate-800"
                                            )}
                                        >
                                            <Mail className="w-4 h-4" /> Email
                                        </button>
                                        <button
                                            type="button"
                                            onClick={() => field.onChange(CommunicationChannel.Sms)}
                                            className={clsx(
                                                "flex items-center gap-2 px-5 py-2.5 rounded-xl border transition-all duration-200 font-medium",
                                                field.value === CommunicationChannel.Sms
                                                    ? "border-emerald-500 bg-emerald-50 dark:bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 shadow-sm"
                                                    : "border-slate-200 dark:border-slate-700 text-slate-500 dark:text-slate-400 hover:border-slate-300 hover:bg-slate-50 dark:hover:bg-slate-800"
                                            )}
                                        >
                                            <MessageSquare className="w-4 h-4" /> SMS
                                        </button>
                                    </div>
                                )}
                            />
                        </div>

                        {channel === CommunicationChannel.Email && (
                            <div className="space-y-2 animate-in fade-in slide-in-from-top-2 duration-300">
                                <label className="block text-sm font-semibold text-slate-700 dark:text-slate-300">Subject</label>
                                <Controller
                                    name="subject"
                                    control={control}
                                    render={({ field }) => (
                                        <Input
                                            {...field}
                                            error={errors.subject?.message}
                                            placeholder="Enter an engaging email subject..."
                                            className="bg-slate-50 dark:bg-slate-900 border-slate-200 dark:border-slate-700"
                                        />
                                    )}
                                />
                            </div>
                        )}

                        <div className="space-y-2">
                            <label className="block text-sm font-semibold text-slate-700 dark:text-slate-300">Message Content</label>
                            <Controller
                                name="body"
                                control={control}
                                render={({ field }) => (
                                    <textarea
                                        {...field}
                                        className={clsx(
                                            "w-full h-64 bg-slate-50 dark:bg-slate-900/50 border rounded-xl px-4 py-3 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-offset-1 dark:focus:ring-offset-slate-900 resize-none transition-colors",
                                            errors.body
                                                ? "border-red-300 dark:border-red-500/50 focus:border-red-500 focus:ring-red-500/20"
                                                : "border-slate-200 dark:border-slate-700 focus:border-indigo-500 focus:ring-indigo-500/20"
                                        )}
                                        placeholder="Type your message here..."
                                    />
                                )}
                            />
                            {errors.body && <p className="text-red-500 text-xs mt-1 font-medium">{errors.body.message}</p>}
                        </div>

                        <div className="flex justify-between items-center pt-6 border-t border-slate-100 dark:border-slate-800">
                            <div className="text-sm font-medium h-6">
                                {recipientCount !== null ? (
                                    <span className="flex items-center gap-2 text-indigo-600 dark:text-indigo-400 animate-in fade-in zoom-in duration-300">
                                        <Users className="w-4 h-4" /> Validated Reach: {recipientCount} Recipients
                                    </span>
                                ) : (
                                    <span className="text-slate-400 dark:text-slate-500 flex items-center gap-2">
                                        <Calculator className="w-4 h-4" /> Calculate reach before sending
                                    </span>
                                )}
                            </div>
                            <Button
                                type="submit"
                                isLoading={sendMutation.isPending}
                                className="bg-gradient-to-r from-indigo-500 to-sky-500 hover:from-indigo-600 hover:to-sky-600 text-white shadow-md hover:shadow-lg transition-all"
                            >
                                <Send className="w-4 h-4 mr-2" /> Dispatch Broadcast
                            </Button>
                        </div>
                    </div>
                </div>

                <div className="space-y-6">
                    <div className="bg-white dark:bg-slate-900 rounded-2xl shadow-sm border border-slate-200 dark:border-slate-800 p-6 space-y-6 relative overflow-hidden transition-all duration-300 hover:shadow-md sticky top-6">
                        <div className="absolute top-0 left-0 w-full h-1 bg-gradient-to-r from-emerald-400 to-teal-500"></div>

                        <div className="flex justify-between items-center bg-slate-50 dark:bg-slate-800/50 -mx-6 -mt-6 px-6 py-4 border-b border-slate-100 dark:border-slate-800 mb-6">
                            <h3 className="font-bold text-slate-800 dark:text-white flex items-center gap-2">
                                <Building2 className="w-4 h-4 text-emerald-500" /> Target Audience
                            </h3>
                            <Button
                                type="button"
                                variant="outline"
                                size="sm"
                                onClick={handlePreviewCount}
                                isLoading={countMutation.isPending}
                                className="border-indigo-200 dark:border-indigo-500/30 text-indigo-600 dark:text-indigo-400 hover:bg-indigo-50 dark:hover:bg-indigo-500/10 h-8 text-xs font-semibold px-3"
                            >
                                <Calculator className="w-3 h-3 mr-1.5" /> Measure
                            </Button>
                        </div>

                        <div className="space-y-5">
                            <div className="space-y-1.5">
                                <label className="text-xs font-bold text-slate-500 uppercase tracking-wider">Branch</label>
                                <Controller
                                    name="filter.branchId"
                                    control={control}
                                    render={({ field }) => (
                                        <select
                                            {...field}
                                            className="w-full bg-slate-50 dark:bg-slate-900/50 border border-slate-200 dark:border-slate-700 rounded-lg p-2.5 text-sm text-slate-800 dark:text-white focus:outline-none focus:border-emerald-500  transition-colors"
                                        >
                                            <option value="">All Branches</option>
                                            {branches?.items.map((branch: BranchDto) => (
                                                <option key={branch.id} value={branch.id}>{branch.name}</option>
                                            ))}
                                        </select>
                                    )}
                                />
                            </div>

                            <div className="space-y-1.5">
                                <label className="text-xs font-bold text-slate-500 uppercase tracking-wider">Graduation Year</label>
                                <Controller
                                    name="filter.graduationYear"
                                    control={control}
                                    render={({ field }) => (
                                        <select
                                            {...field}
                                            className="w-full bg-slate-50 dark:bg-slate-900/50 border border-slate-200 dark:border-slate-700 rounded-lg p-2.5 text-sm text-slate-800 dark:text-white focus:outline-none focus:border-emerald-500 transition-colors"
                                        >
                                            <option value="">Any Year</option>
                                            {(graduationYears ?? []).map((year) => (
                                                <option key={year} value={String(year)}>{year}</option>
                                            ))}
                                        </select>
                                    )}
                                />
                            </div>

                            <div className="space-y-1.5">
                                <label className="text-xs font-bold text-slate-500 uppercase tracking-wider">Graduation Semester</label>
                                <Controller
                                    name="filter.graduationSemester"
                                    control={control}
                                    render={({ field }) => (
                                        <select
                                            {...field}
                                            className="w-full bg-slate-50 dark:bg-slate-900/50 border border-slate-200 dark:border-slate-700 rounded-lg p-2.5 text-sm text-slate-800 dark:text-white focus:outline-none focus:border-emerald-500 transition-colors"
                                        >
                                            <option value="">Any Semester</option>
                                            <option value="1">Fall (1)</option>
                                            <option value="2">Spring (2)</option>
                                            <option value="3">Summer (3)</option>
                                        </select>
                                    )}
                                />
                            </div>

                            <div className="space-y-1.5">
                                <label className="text-xs font-bold text-slate-500 uppercase tracking-wider">Membership Status</label>
                                <Controller
                                    name="filter.membershipStatus"
                                    control={control}
                                    render={({ field }) => (
                                        <select
                                            {...field}
                                            className="w-full bg-slate-50 dark:bg-slate-900/50 border border-slate-200 dark:border-slate-700 rounded-lg p-2.5 text-sm text-slate-800 dark:text-white focus:outline-none focus:border-emerald-500 transition-colors"
                                        >
                                            <option value="0">All Alumni</option>
                                            <option value="1">Active Members Only</option>
                                            <option value="2">Inactive Members Only</option>
                                        </select>
                                    )}
                                />
                            </div>

                            <div className="pt-4 mt-2 border-t border-slate-100 dark:border-slate-800">
                                <div className="bg-emerald-50 dark:bg-emerald-500/10 rounded-lg p-3 border border-emerald-100 dark:border-emerald-500/20">
                                    <div className="text-xs text-emerald-800 dark:text-emerald-300 leading-relaxed flex items-start gap-2">
                                        <div className="w-1.5 h-1.5 rounded-full bg-emerald-500 mt-1.5 shrink-0" />
                                        <span>Leave fields empty to broadcast universally. Use "Measure" to validate your audience accurately beforehand.</span>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </form>

            {/* Delivery Logs Modal */}
            <CommunicationLogsModal isOpen={showLogs} onClose={() => setShowLogs(false)} />
        </div>
    );
};

export default CommunicationCenter;
