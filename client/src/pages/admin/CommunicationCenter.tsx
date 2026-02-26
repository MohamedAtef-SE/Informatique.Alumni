import { useState } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from 'react-oidc-context';
import { useQuery, useMutation } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { CommunicationChannel } from '../../types/communications';
import { Send, Mail, MessageSquare, Building2 } from 'lucide-react';
import { PageHeader } from '../../components/admin/PageHeader';
import { Button } from '../../components/ui/Button';
import { Input } from '../../components/ui/Input';
import { toast } from 'sonner';
import clsx from 'clsx';
import type { BranchDto } from '../../types/organization';

const CommunicationCenter = () => {
    const [subject, setSubject] = useState('');
    const [body, setBody] = useState('');
    const [channel, setChannel] = useState<number>(CommunicationChannel.Email);
    const [selectedBranchId, setSelectedBranchId] = useState<string>('');

    // Auth Check
    const auth = useAuth();
    const roles = auth.user?.profile?.role;
    const roleArray = Array.isArray(roles) ? roles : roles ? [String(roles)] : [];
    const isSuperAdmin = roleArray.some(r => ['admin', 'systemadmin'].includes(r.toLowerCase()));

    if (!isSuperAdmin) {
        return <Navigate to="/admin/dashboard" replace />;
    }

    const { data: branches } = useQuery({
        queryKey: ['admin-branches-list'],
        queryFn: () => adminService.getBranches({ maxResultCount: 100 })
    });

    const sendMutation = useMutation({
        mutationFn: (data: any) => adminService.sendMessage(data),
        onSuccess: () => {
            toast.success('Message queued successfully!');
            setSubject('');
            setBody('');
        },
        onError: () => toast.error('Failed to send message.')
    });

    const handleSend = () => {
        if (!body) {
            toast.error("Message body is required");
            return;
        }
        if (channel === CommunicationChannel.Email && !subject) {
            toast.error("Subject is required for Email");
            return;
        }

        sendMutation.mutate({
            channel,
            subject,
            body,
            filter: {
                branchId: selectedBranchId || undefined
            }
        });
    };

    return (
        <div className="space-y-8 animate-in fade-in duration-500">
            <PageHeader
                title="Communication Center"
                description="Broadcast messages to alumni across different channels."
            />

            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                <div className="md:col-span-2 space-y-6">
                    <div className="glass-panel p-6 space-y-6">
                        <div>
                            <label className="block text-sm font-medium text-slate-300 mb-2">Channel</label>
                            <div className="flex gap-4">
                                <button
                                    onClick={() => setChannel(CommunicationChannel.Email)}
                                    className={clsx(
                                        "flex items-center gap-2 px-4 py-2 rounded-lg border transition-all duration-200",
                                        channel === CommunicationChannel.Email
                                            ? "border-sky-500 bg-sky-500/10 text-sky-400 shadow-[0_0_10px_rgba(14,165,233,0.3)]"
                                            : "border-white/10 text-slate-400 hover:border-white/20 hover:text-slate-200"
                                    )}
                                >
                                    <Mail className="w-4 h-4" /> Email
                                </button>
                                <button
                                    onClick={() => setChannel(CommunicationChannel.Sms)}
                                    className={clsx(
                                        "flex items-center gap-2 px-4 py-2 rounded-lg border transition-all duration-200",
                                        channel === CommunicationChannel.Sms
                                            ? "border-emerald-500 bg-emerald-500/10 text-emerald-400 shadow-[0_0_10px_rgba(16,185,129,0.3)]"
                                            : "border-white/10 text-slate-400 hover:border-white/20 hover:text-slate-200"
                                    )}
                                >
                                    <MessageSquare className="w-4 h-4" /> SMS
                                </button>
                            </div>
                        </div>

                        {channel === CommunicationChannel.Email && (
                            <div className="space-y-2">
                                <label className="block text-sm font-medium text-slate-300">Subject</label>
                                <Input
                                    value={subject}
                                    onChange={(e) => setSubject(e.target.value)}
                                    placeholder="Enter email subject..."
                                />
                            </div>
                        )}

                        <div className="space-y-2">
                            <label className="block text-sm font-medium text-slate-300">Message Body</label>
                            <textarea
                                className="w-full h-64 bg-slate-950/50 border border-white/10 rounded-lg px-4 py-3 text-white focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 resize-none transition-colors"
                                value={body}
                                onChange={(e) => setBody(e.target.value)}
                                placeholder="Type your message here..."
                            />
                        </div>

                        <div className="flex justify-end pt-4 border-t border-white/10">
                            <Button
                                onClick={handleSend}
                                isLoading={sendMutation.isPending}
                                className="shadow-neon-indigo"
                            >
                                <Send className="w-4 h-4 mr-2" /> Send Broadcast
                            </Button>
                        </div>
                    </div>
                </div>

                <div className="space-y-6">
                    <div className="glass-panel p-6 space-y-4">
                        <h3 className="font-semibold text-white flex items-center gap-2">
                            <Building2 className="w-4 h-4 text-indigo-400" /> Target Audience
                        </h3>

                        <div className="space-y-3">
                            <label className="text-sm text-slate-400">Branch Filter</label>
                            <select
                                className="w-full bg-slate-950/50 border border-white/10 rounded-md p-2 text-sm text-white focus:outline-none focus:border-indigo-500"
                                value={selectedBranchId}
                                onChange={(e) => setSelectedBranchId(e.target.value)}
                            >
                                <option value="">All Branches</option>
                                {branches?.items.map((branch: BranchDto) => (
                                    <option key={branch.id} value={branch.id}>{branch.name}</option>
                                ))}
                            </select>
                            <p className="text-xs text-slate-500">
                                Leave select as "All Branches" to broadcast to everyone.
                            </p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default CommunicationCenter;
