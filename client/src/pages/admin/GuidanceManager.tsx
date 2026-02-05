import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { AdvisingRequestStatus } from '../../types/guidance';
import { Check, X, CalendarClock } from 'lucide-react';
import clsx from 'clsx';

const GuidanceManager = () => {
    const queryClient = useQueryClient();

    const { data } = useQuery({
        queryKey: ['admin-advising'],
        queryFn: () => adminService.getAdvisingRequests({ maxResultCount: 20 })
    });

    const updateStatusMutation = useMutation({
        mutationFn: (variables: { id: string, status: AdvisingRequestStatus }) =>
            adminService.updateAdvisingStatus(variables.id, { status: variables.status }),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-advising'] });
        }
    });

    return (
        <div className="space-y-6">
            <h1 className="text-2xl font-bold text-white">Guidance & Advising</h1>

            <div className="glass-panel overflow-hidden">
                <table className="w-full text-left text-sm">
                    <thead className="bg-white/5 text-gray-400 uppercase tracking-wider text-xs">
                        <tr>
                            <th className="px-6 py-4">Request Time</th>
                            <th className="px-6 py-4">Subject</th>
                            <th className="px-6 py-4">Status</th>
                            <th className="px-6 py-4 text-right">Actions</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-white/5">


                        {data?.items.map((req: any) => (
                            <tr key={req.id} className="hover:bg-white/5 transition-colors">
                                <td className="px-6 py-4 text-gray-300 flex items-center gap-2">
                                    <CalendarClock className="w-4 h-4 text-gray-500" />
                                    {new Date(req.startTime).toLocaleString()}
                                </td>
                                <td className="px-6 py-4 text-white font-medium">{req.subject}</td>
                                <td className="px-6 py-4">
                                    <span className={clsx(
                                        "px-2 py-1 rounded text-xs font-bold uppercase",
                                        req.status === AdvisingRequestStatus.Approved ? "bg-emerald-500/20 text-emerald-500" :
                                            req.status === AdvisingRequestStatus.Rejected ? "bg-red-500/20 text-red-500" :
                                                "bg-amber-500/20 text-amber-500"
                                    )}>
                                        {req.status === AdvisingRequestStatus.Pending ? 'Pending' :
                                            req.status === AdvisingRequestStatus.Approved ? 'Approved' : 'Rejected'}
                                    </span>
                                </td>
                                <td className="px-6 py-4 text-right">
                                    {req.status === AdvisingRequestStatus.Pending && (
                                        <div className="flex justify-end gap-2">
                                            <button
                                                onClick={() => updateStatusMutation.mutate({ id: req.id, status: AdvisingRequestStatus.Approved })}
                                                className="p-1 rounded bg-emerald-500/20 text-emerald-400 hover:bg-emerald-500 hover:text-white"
                                            ><Check className="w-4 h-4" /></button>
                                            <button
                                                onClick={() => updateStatusMutation.mutate({ id: req.id, status: AdvisingRequestStatus.Rejected })}
                                                className="p-1 rounded bg-red-500/20 text-red-400 hover:bg-red-500 hover:text-white"
                                            ><X className="w-4 h-4" /></button>
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

export default GuidanceManager;
