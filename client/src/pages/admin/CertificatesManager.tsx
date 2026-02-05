import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { CertificateRequestStatus } from '../../types/certificates';
import { FileBadge } from 'lucide-react';
import clsx from 'clsx';

const CertificatesManager = () => {
    const queryClient = useQueryClient();
    const [filter] = useState('');

    const { data } = useQuery({
        queryKey: ['admin-certificates', filter],
        queryFn: () => adminService.getCertificateRequests({ filter, maxResultCount: 20 })
    });

    const updateStatusMutation = useMutation({
        mutationFn: (variables: { id: string, status: CertificateRequestStatus }) =>
            adminService.updateCertificateStatus(variables.id, { newStatus: variables.status }),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-certificates'] });
            alert('Status Updated');
        }
    });

    return (
        <div className="space-y-6">
            <h1 className="text-2xl font-bold text-white">Certificate Requests</h1>

            <div className="glass-panel overflow-hidden">
                <table className="w-full text-left text-sm">
                    <thead className="bg-white/5 text-gray-400 uppercase tracking-wider text-xs">
                        <tr>
                            <th className="px-6 py-4">Request Date</th>
                            <th className="px-6 py-4">Details</th>
                            <th className="px-6 py-4">Total Fees</th>
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
                                <td className="px-6 py-4">
                                    <div className="text-white font-medium flex items-center gap-2">
                                        <FileBadge className="w-4 h-4 text-blue-400" />
                                        {req.items?.length || 0} Certificates
                                    </div>
                                    <div className="text-xs text-gray-500">{req.deliveryMethod === 1 ? 'Courier' : 'Pickup'}</div>
                                </td>
                                <td className="px-6 py-4 text-white font-mono">{req.totalFees} EGP</td>
                                <td className="px-6 py-4">
                                    <span className={clsx(
                                        "px-2 py-1 rounded text-xs font-bold uppercase",
                                        req.status === CertificateRequestStatus.Delivered ? "bg-emerald-500/20 text-emerald-500" :
                                            req.status === CertificateRequestStatus.Pending ? "bg-amber-500/20 text-amber-500" :
                                                "bg-blue-500/20 text-blue-500"
                                    )}>
                                        {req.status === CertificateRequestStatus.Pending ? 'Pending' : 'Processed'}
                                    </span>
                                </td>
                                <td className="px-6 py-4 text-right">
                                    {req.status === CertificateRequestStatus.Pending && (
                                        <button
                                            onClick={() => updateStatusMutation.mutate({ id: req.id, status: CertificateRequestStatus.Processing })}
                                            className="text-xs bg-blue-600 hover:bg-blue-700 text-white px-3 py-1 rounded"
                                        >
                                            Processing
                                        </button>
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

export default CertificatesManager;
