import { useQuery } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { Building2, Plus } from 'lucide-react';

const OrgManager = () => {
    const { data } = useQuery({
        queryKey: ['admin-branches'],
        queryFn: () => adminService.getBranches({ maxResultCount: 20 })
    });

    return (
        <div className="space-y-6">
            <div className="flex justify-between items-center">
                <h1 className="text-2xl font-bold text-white">Branch Management</h1>
                <button className="btn-primary flex items-center gap-2 px-4 py-2">
                    <Plus className="w-4 h-4" /> New Branch
                </button>
            </div>

            <div className="glass-panel overflow-hidden">
                <table className="w-full text-left text-sm">
                    <thead className="bg-white/5 text-gray-400 uppercase tracking-wider text-xs">
                        <tr>
                            <th className="px-6 py-4">Branch Name</th>
                            <th className="px-6 py-4">Code</th>
                            <th className="px-6 py-4">Address</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-white/5">


                        {data?.items.map((branch: any) => (
                            <tr key={branch.id} className="hover:bg-white/5 transition-colors">
                                <td className="px-6 py-4 text-white font-medium flex items-center gap-2">
                                    <Building2 className="w-4 h-4 text-indigo-400" />
                                    {branch.name}
                                </td>
                                <td className="px-6 py-4 text-gray-400 font-mono">{branch.code}</td>
                                <td className="px-6 py-4 text-gray-300">{branch.address || 'N/A'}</td>
                            </tr>
                        ))}
                        {data?.items?.length === 0 && <tr><td colSpan={3} className="px-6 py-8 text-center text-gray-500">No branches found.</td></tr>}
                    </tbody>
                </table>
            </div>
        </div>
    );
};

export default OrgManager;
