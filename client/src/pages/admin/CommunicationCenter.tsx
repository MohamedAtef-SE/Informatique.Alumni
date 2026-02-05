import { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { CommunicationChannel } from '../../types/communications';
import { Send, Mail, MessageSquare } from 'lucide-react';
import clsx from 'clsx';

const CommunicationCenter = () => {
    // const auth = useAuth();
    // const branchId = auth.user?.profile?.sub; 
    // In real app, we need to fetch user's branch ID properly.
    // For now, hardcoding a placeholder or using user profile ID if they are admin.
    // Ideally admin selects branch if super admin.

    const [subject, setSubject] = useState('');
    const [body, setBody] = useState('');
    const [channel, setChannel] = useState<CommunicationChannel>(CommunicationChannel.Email);

    const sendMutation = useMutation({
        mutationFn: (data: any) => adminService.sendMessage(data),
        onSuccess: () => {
            alert('Message queued successfully!');
            setSubject('');
            setBody('');
        },
        onError: () => alert('Failed to send message.')
    });

    const handleSend = () => {
        // Mock Filter: Send to ALL in current branch (placeholder ID)
        if (!body) return;

        sendMutation.mutate({
            channel,
            subject,
            body,
            filter: {
                branchId: "3fa85f64-5717-4562-b3fc-2c963f66afa6" // Placeholder GUID, user input needed in real UI
            }
        });
    };

    return (
        <div className="max-w-4xl mx-auto space-y-6">
            <h1 className="text-2xl font-bold text-white">Communication Center</h1>

            <div className="glass-panel p-6 space-y-6">
                <div>
                    <label className="block text-sm font-medium text-gray-400 mb-2">Channel</label>
                    <div className="flex gap-4">
                        <button
                            onClick={() => setChannel(CommunicationChannel.Email)}
                            className={clsx("flex items-center gap-2 px-4 py-2 rounded-lg border transition-colors", channel === CommunicationChannel.Email ? "border-[var(--color-primary)] bg-[var(--color-primary)]/10 text-white" : "border-white/10 text-gray-400 hover:border-white/20")}
                        >
                            <Mail className="w-4 h-4" /> Email
                        </button>
                        <button
                            onClick={() => setChannel(CommunicationChannel.Sms)}
                            className={clsx("flex items-center gap-2 px-4 py-2 rounded-lg border transition-colors", channel === CommunicationChannel.Sms ? "border-[var(--color-primary)] bg-[var(--color-primary)]/10 text-white" : "border-white/10 text-gray-400 hover:border-white/20")}
                        >
                            <MessageSquare className="w-4 h-4" /> SMS
                        </button>
                    </div>
                </div>

                {channel === CommunicationChannel.Email && (
                    <div>
                        <label className="block text-sm font-medium text-gray-400 mb-2">Subject</label>
                        <input
                            type="text"
                            className="w-full bg-black/20 border border-white/10 rounded-lg px-4 py-2 text-white focus:outline-none focus:border-[var(--color-primary)]"
                            value={subject}
                            onChange={(e) => setSubject(e.target.value)}
                        />
                    </div>
                )}

                <div>
                    <label className="block text-sm font-medium text-gray-400 mb-2">Message Body</label>
                    <textarea
                        className="w-full h-40 bg-black/20 border border-white/10 rounded-lg px-4 py-2 text-white focus:outline-none focus:border-[var(--color-primary)] resize-none"
                        value={body}
                        onChange={(e) => setBody(e.target.value)}
                    />
                </div>

                <div className="pt-4 border-t border-white/10 flex justify-between items-center">
                    <p className="text-xs text-gray-500">Target: All Alumni in your Branch (Default)</p>
                    <button
                        onClick={handleSend}
                        disabled={sendMutation.isPending}
                        className="btn-primary flex items-center gap-2 px-6 py-2"
                    >
                        {sendMutation.isPending ? 'Sending...' : <><Send className="w-4 h-4" /> Send Broadcast</>}
                    </button>
                </div>
            </div>
        </div>
    );
};

export default CommunicationCenter;
