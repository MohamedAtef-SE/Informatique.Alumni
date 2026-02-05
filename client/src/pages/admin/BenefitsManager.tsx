import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { Plus, Edit, Trash2, Gift, Award } from 'lucide-react';
import clsx from 'clsx';

const BenefitsManager = () => {
    const [activeTab, setActiveTab] = useState<'grants' | 'discounts'>('grants');

    const grantsQuery = useQuery({
        queryKey: ['admin-grants'],
        queryFn: () => adminService.getGrants({ maxResultCount: 20 }),
        enabled: activeTab === 'grants'
    });

    const discountsQuery = useQuery({
        queryKey: ['admin-discounts'],
        queryFn: () => adminService.getDiscounts({ maxResultCount: 20 }),
        enabled: activeTab === 'discounts'
    });

    return (
        <div className="space-y-6">
            <div className="flex justify-between items-center">
                <h1 className="text-2xl font-bold text-white">Benefits Manager</h1>

                <div className="flex gap-4">
                    <div className="flex rounded-lg bg-white/10 p-1">
                        <button
                            onClick={() => setActiveTab('grants')}
                            className={clsx("px-4 py-1.5 rounded text-sm font-medium transition-colors", activeTab === 'grants' ? "bg-primary text-white shadow" : "text-gray-400 hover:text-white")}
                        >
                            Grants
                        </button>
                        <button
                            onClick={() => setActiveTab('discounts')}
                            className={clsx("px-4 py-1.5 rounded text-sm font-medium transition-colors", activeTab === 'discounts' ? "bg-primary text-white shadow" : "text-gray-400 hover:text-white")}
                        >
                            Discounts
                        </button>
                    </div>

                    <button className="btn-primary flex items-center gap-2 px-4 py-2">
                        <Plus className="w-4 h-4" /> New {activeTab === 'grants' ? 'Grant' : 'Discount'}
                    </button>
                </div>
            </div>

            <div className="glass-panel overflow-hidden">
                <table className="w-full text-left text-sm">
                    <thead className="bg-white/5 text-gray-400 uppercase tracking-wider text-xs">
                        <tr>
                            <th className="px-6 py-4">Title</th>
                            <th className="px-6 py-4">Details</th>
                            <th className="px-6 py-4 text-right">Actions</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-white/5">


                        {activeTab === 'grants' && grantsQuery.data?.items.map((item: any) => (
                            <tr key={item.id} className="hover:bg-white/5 transition-colors">
                                <td className="px-6 py-4">
                                    <div className="text-white font-medium flex items-center gap-2">
                                        <Award className="w-4 h-4 text-amber-500" />
                                        {item.nameEn}
                                    </div>
                                </td>
                                <td className="px-6 py-4 text-gray-300">{item.percentage}% Off</td>
                                <td className="px-6 py-4 text-right flex justify-end gap-2">
                                    <button className="p-1 rounded bg-white/10 text-gray-300 hover:bg-white/20 hover:text-white transition-colors" title="Edit"><Edit className="w-4 h-4" /></button>
                                    <button className="p-1 rounded bg-red-500/20 text-red-400 hover:bg-red-500 hover:text-white transition-colors" title="Delete"><Trash2 className="w-4 h-4" /></button>
                                </td>
                            </tr>
                        ))}

                        {activeTab === 'discounts' && discountsQuery.data?.items.map((item: any) => (
                            <tr key={item.id} className="hover:bg-white/5 transition-colors">
                                <td className="px-6 py-4">
                                    <div className="text-white font-medium flex items-center gap-2">
                                        <Gift className="w-4 h-4 text-purple-500" />
                                        {item.title}
                                    </div>
                                    <div className="text-xs text-gray-500">{item.providerName}</div>
                                </td>
                                <td className="px-6 py-4 text-gray-300">
                                    {item.discountPercentage}%
                                    {item.promoCode && <span className="ml-2 px-2 py-0.5 rounded bg-white/10 text-xs font-mono">{item.promoCode}</span>}
                                </td>
                                <td className="px-6 py-4 text-right flex justify-end gap-2">
                                    <button className="p-1 rounded bg-white/10 text-gray-300 hover:bg-white/20 hover:text-white transition-colors" title="Edit"><Edit className="w-4 h-4" /></button>
                                    <button className="p-1 rounded bg-red-500/20 text-red-400 hover:bg-red-500 hover:text-white transition-colors" title="Delete"><Trash2 className="w-4 h-4" /></button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
};

export default BenefitsManager;
