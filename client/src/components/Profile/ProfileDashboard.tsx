import React from 'react';
import { Card, CardContent } from '../ui/Card';
import { Users, Search, BarChart3, Lock } from 'lucide-react';
import { useTranslation } from 'react-i18next';

interface ProfileDashboardProps {
    viewCount: number;
}

const ProfileDashboard: React.FC<ProfileDashboardProps> = ({ viewCount }) => {
    const { t } = useTranslation();

    return (
        <Card className="overflow-hidden border-slate-200 border shadow-sm">
            <CardContent className="p-6">
                <div className="flex items-center justify-between mb-4">
                    <h3 className="text-lg font-bold text-slate-900">Your Dashboard</h3>
                    <span className="flex items-center gap-1 text-[10px] text-slate-400 font-bold uppercase tracking-widest">
                        <Lock className="w-3 h-3" /> Private to you
                    </span>
                </div>
                
                <div className="grid grid-cols-2 gap-4">
                    <div className="p-4 rounded-2xl bg-blue-50 border border-blue-100 group hover:bg-blue-600 hover:text-white transition-all duration-300 cursor-pointer">
                        <div className="flex flex-col">
                            <span className="text-2xl font-black mb-1">{viewCount}</span>
                            <span className="text-xs font-bold text-blue-600 group-hover:text-blue-100 transition-colors uppercase tracking-tight">Profile views</span>
                            <div className="mt-2 flex items-center gap-1 text-[10px] opacity-60">
                                <Users className="w-3 h-3" /> +12% this week
                            </div>
                        </div>
                    </div>

                    <div className="p-4 rounded-2xl bg-slate-50 border border-slate-100 group hover:bg-slate-900 hover:text-white transition-all duration-300 cursor-pointer">
                        <div className="flex flex-col">
                            <span className="text-2xl font-black mb-1">24</span>
                            <span className="text-xs font-bold text-slate-600 group-hover:text-slate-100 transition-colors uppercase tracking-tight">Search results</span>
                            <div className="mt-2 flex items-center gap-1 text-[10px] opacity-60">
                                <Search className="w-3 h-3" /> 4 new appearances
                            </div>
                        </div>
                    </div>
                </div>

                <div className="mt-6 pt-6 border-t border-slate-100 flex items-center justify-between text-blue-600 hover:text-blue-700 cursor-pointer group">
                    <span className="text-xs font-bold uppercase tracking-widest">View all analytics</span>
                    <BarChart3 className="w-4 h-4 group-hover:translate-x-1 transition-transform" />
                </div>
            </CardContent>
        </Card>
    );
};

export default ProfileDashboard;
