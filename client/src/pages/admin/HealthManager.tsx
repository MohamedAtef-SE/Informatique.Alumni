import { useQuery } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { HeartPulse, Plus } from 'lucide-react';

const HealthManager = () => {
    const { data } = useQuery({
        queryKey: ['admin-health-offers'],
        queryFn: () => adminService.getHealthcareOffers({ maxResultCount: 20 })
    });

    return (
        <div className="space-y-6">
            <div className="flex justify-between items-center">
                <h1 className="text-2xl font-bold text-white">Healthcare Offers</h1>
                <button className="btn-primary flex items-center gap-2 px-4 py-2">
                    <Plus className="w-4 h-4" /> New Offer
                </button>
            </div>

            <div className="glass-panel overflow-hidden">
                <table className="w-full text-left text-sm">
                    <thead className="bg-white/5 text-gray-400 uppercase tracking-wider text-xs">
                        <tr>
                            <th className="px-6 py-4">Title</th>
                            <th className="px-6 py-4">Description</th>
                            <th className="px-6 py-4">Code</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-white/5">


                        {data?.items.map((offer: any) => (
                            <tr key={offer.id} className="hover:bg-white/5 transition-colors">
                                <td className="px-6 py-4 text-white font-medium flex items-center gap-2">
                                    <HeartPulse className="w-4 h-4 text-pink-500" />
                                    {offer.title}
                                </td>
                                <td className="px-6 py-4 text-gray-300">{offer.description}</td>
                                <td className="px-6 py-4 text-gray-400 font-mono">{offer.discountCode || '-'}</td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
};

export default HealthManager;
