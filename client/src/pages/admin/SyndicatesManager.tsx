import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { SyndicateStatus } from '../../types/syndicates';
import { Check, X } from 'lucide-react';
import clsx from 'clsx';

const SyndicatesManager = () => {
    const queryClient = useQueryClient();
    const [filter] = useState('');

    const { data } = useQuery({
        queryKey: ['admin-syndicates', filter],
        queryFn: () => adminService.getSyndicateRequests({ filter, maxResultCount: 20 })
    });

    const updateStatusMutation = useMutation({
        mutationFn: (variables: { id: string, status: SyndicateStatus }) =>
            adminService.updateSyndicateRequestStatus(variables.id, variables.status),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-syndicates'] });
            alert('Status Updated');
        }
    });

    const handleAction = (id: string, status: SyndicateStatus) => {
        if (confirm(`Change status to ${status === SyndicateStatus.Approved ? 'Approved' : 'Rejected'}?`)) {
            updateStatusMutation.mutate({ id, status });
        }
    };

    return (
        <div className="space-y-6">
            <h1 className="text-2xl font-bold text-white">Syndicate Applications</h1>

            <div className="glass-panel overflow-hidden">
                <table className="w-full text-left text-sm">
                    <thead className="bg-white/5 text-gray-400 uppercase tracking-wider text-xs">
                        <tr>
                            <th className="px-6 py-4">Request Date</th>
                            <th className="px-6 py-4">Alumni</th>
                            <th className="px-6 py-4">Status</th>
                            <th className="px-6 py-4 text-right">Actions</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-white/5">


                        {data?.items.map((req: any) => (
                            <tr key={req.id} className="hover:bg-white/5 transition-colors">
                                <td className="px-6 py-4 text-gray-300">
                                    {new Date(req.creationTime).toLocaleDateString()}
                                </td>
                                <td className="px-6 py-4 text-white font-medium">Alumni ID: {req.alumniId}</td>
                                <td className="px-6 py-4">
                                    <span className={clsx(
                                        "px-2 py-1 rounded text-xs font-bold uppercase",
                                        req.status === SyndicateStatus.Approved ? "bg-emerald-500/20 text-emerald-500" :
                                            req.status === SyndicateStatus.Rejected ? "bg-red-500/20 text-red-500" :
                                                "bg-amber-500/20 text-amber-500"
                                    )}>
                                        {req.status === SyndicateStatus.Pending ? 'Pending' :
                                            req.status === SyndicateStatus.Approved ? 'Approved' :
                                                req.status === SyndicateStatus.Rejected ? 'Rejected' : 'More Info'}
                                    </span>
                                </td>
                                <td className="px-6 py-4 text-right">
                                    {req.status === SyndicateStatus.Pending && (
                                        <div className="flex justify-end gap-2">
                                            <button onClick={() => handleAction(req.id, SyndicateStatus.Approved)} className="p-1 rounded bg-emerald-500/20 text-emerald-400 hover:bg-emerald-500 hover:text-white"><Check className="w-4 h-4" /></button>
                                            <button onClick={() => handleAction(req.id, SyndicateStatus.Rejected)} className="p-1 rounded bg-red-500/20 text-red-400 hover:bg-red-500 hover:text-white"><X className="w-4 h-4" /></button>
                                        </div>
                                    )}
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
};

export default SyndicatesManager;
