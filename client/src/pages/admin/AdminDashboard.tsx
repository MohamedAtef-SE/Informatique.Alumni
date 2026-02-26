import { useEffect, useState } from 'react';
import { adminService } from '../../services/adminService';
import type { DashboardStatsDto } from '../../types/admin';
import { Users, Calendar, Activity, UserPlus, Briefcase, UserCheck } from 'lucide-react';
import { PageHeader } from '../../components/admin/PageHeader';
import { Button } from '../../components/ui/Button';
import { RegistrationTrendChart, CollegeDistributionChart } from '../../components/admin/DashboardCharts';



const StatCard = ({ title, value, icon: Icon, color, loading }: any) => (
    <div className="glass-panel p-6 flex items-center gap-4 border border-slate-200 hover:border-slate-300 transition-all bg-white shadow-sm">
        <div className={`w-12 h-12 rounded-lg flex items-center justify-center ${color}`}>
            <Icon className="w-6 h-6 text-white" />
        </div>
        <div>
            <p className="text-sm font-medium text-slate-500">{title}</p>
            {loading ? (
                <div className="h-8 w-24 bg-slate-100 animate-pulse rounded mt-1" />
            ) : (
                <p className="text-2xl font-bold text-slate-900">{value}</p>
            )}
        </div>
    </div>
);

const AdminDashboard = () => {
    const [stats, setStats] = useState<DashboardStatsDto | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        loadStats();
    }, []);

    const loadStats = async () => {
        try {
            setLoading(true);
            const data = await adminService.getDashboardStats();
            setStats(data);
        } catch (error) {
            console.error('Failed to load dashboard stats', error);
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="space-y-8 animate-in fade-in duration-500">
            <PageHeader
                title="Admin Dashboard"
                description="Overview of system activity and key performance indicators."
                action={<Button onClick={loadStats} variant="outline" size="sm">Refresh Data</Button>}
            />

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                <StatCard
                    title="Total Alumni"
                    value={stats?.totalAlumni?.toLocaleString() ?? 0}
                    icon={Users}
                    color="bg-blue-600 shadow-lg shadow-blue-900/20"
                    loading={loading}
                />
                <StatCard
                    title="Pending Requests"
                    value={stats?.pendingAlumni?.toLocaleString() ?? 0}
                    icon={UserPlus}
                    color="bg-amber-500 shadow-lg shadow-amber-900/20"
                    loading={loading}
                />
                <StatCard
                    title="Active Alumni"
                    value={stats?.activeAlumni?.toLocaleString() ?? 0}
                    icon={UserCheck}
                    color="bg-green-600 shadow-lg shadow-green-900/20"
                    loading={loading}
                />
                <StatCard
                    title="Total Revenue"
                    value={`EGP ${stats?.totalRevenue?.toLocaleString() ?? 0}`}
                    icon={Activity}
                    color="bg-emerald-600 shadow-lg shadow-emerald-900/20"
                    loading={loading}
                />
                <StatCard
                    title="Active Jobs"
                    value={stats?.activeJobs?.toLocaleString() ?? 0}
                    icon={Briefcase}
                    color="bg-purple-600 shadow-lg shadow-purple-900/20"
                    loading={loading}
                />
                <StatCard
                    title="Active Events"
                    value={stats?.upcomingEvents?.toLocaleString() ?? 0}
                    icon={Calendar}
                    color="bg-cyan-600 shadow-lg shadow-cyan-900/20"
                    loading={loading}
                />
            </div>

            {/* Main Content Grid */}
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">

                {/* Left Column (Charts) */}
                <div className="lg:col-span-2 space-y-6">
                    <div className="glass-panel p-6 border border-slate-200 bg-white shadow-sm">
                        <h3 className="text-lg font-semibold text-slate-900 mb-6">Alumni Growth Trend</h3>
                        <RegistrationTrendChart stats={stats} loading={loading} />
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div className="glass-panel p-6 border border-slate-200 bg-white shadow-sm">
                            <h3 className="text-lg font-semibold text-slate-900 mb-4">Top Employers</h3>
                            {loading ? (
                                <div className="space-y-3">
                                    {[1, 2, 3].map(i => <div key={i} className="h-6 bg-slate-100 rounded animate-pulse" />)}
                                </div>
                            ) : (
                                <div className="space-y-3">
                                    {stats?.topEmployers?.map((emp, i) => (
                                        <div key={i} className="flex items-center justify-between text-sm">
                                            <span className="text-slate-700 font-medium truncate max-w-[180px]" title={emp.label}>{emp.label}</span>
                                            <span className="text-slate-500 bg-slate-100 px-2 py-0.5 rounded-full text-xs">{emp.count}</span>
                                        </div>
                                    ))}
                                    {(!stats?.topEmployers?.length) && <p className="text-sm text-slate-400">No data available</p>}
                                </div>
                            )}
                        </div>

                        <div className="glass-panel p-6 border border-slate-200 bg-white shadow-sm">
                            <h3 className="text-lg font-semibold text-slate-900 mb-4">Top Locations</h3>
                            {loading ? (
                                <div className="space-y-3">
                                    {[1, 2, 3].map(i => <div key={i} className="h-6 bg-slate-100 rounded animate-pulse" />)}
                                </div>
                            ) : (
                                <div className="space-y-3">
                                    {stats?.topLocations?.map((loc, i) => (
                                        <div key={i} className="flex items-center justify-between text-sm">
                                            <span className="text-slate-700 font-medium truncate max-w-[180px]" title={loc.label}>{loc.label}</span>
                                            <span className="text-slate-500 bg-slate-100 px-2 py-0.5 rounded-full text-xs">{loc.count}</span>
                                        </div>
                                    ))}
                                    {(!stats?.topLocations?.length) && <p className="text-sm text-slate-400">No data available</p>}
                                </div>
                            )}
                        </div>
                    </div>
                </div>

                {/* Right Column (Distribution & Activity) */}
                <div className="space-y-6">
                    <div className="glass-panel p-6 border border-slate-200 bg-white shadow-sm">
                        <h3 className="text-lg font-semibold text-slate-900 mb-6">Alumni By College</h3>
                        <CollegeDistributionChart stats={stats} loading={loading} />
                    </div>

                    <div className="glass-panel p-6 border border-slate-200 bg-white shadow-sm h-fit">
                        <h3 className="text-lg font-semibold text-slate-900 mb-4 flex items-center gap-2">
                            <Activity className="w-5 h-5 text-blue-600" />
                            Recent Activity
                        </h3>
                        <div className="space-y-4">
                            {loading ? (
                                [1, 2, 3, 4].map(i => (
                                    <div key={i} className="flex gap-3 animate-pulse">
                                        <div className="w-2 h-2 mt-2 rounded-full bg-slate-200" />
                                        <div className="flex-1 space-y-2">
                                            <div className="h-4 bg-slate-100 rounded w-3/4" />
                                            <div className="h-3 bg-slate-50 rounded w-1/2" />
                                        </div>
                                    </div>
                                ))
                            ) : (
                                stats?.recentActivities?.map((activity, i) => (
                                    <div key={i} className="relative pl-4 border-l border-slate-200 pb-1 last:pb-0">
                                        <div className={`absolute left-[-5px] top-1.5 w-2.5 h-2.5 rounded-full border-2 border-white ${activity.type === 'User' ? 'bg-blue-500' :
                                            activity.type === 'Event' ? 'bg-emerald-500' : 'bg-purple-500'
                                            }`} />
                                        <p className="text-sm font-medium text-slate-800">{activity.description}</p>
                                        <p className="text-xs text-slate-400 mt-0.5">
                                            {new Date(activity.time).toLocaleDateString()} • {new Date(activity.time).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                                        </p>
                                    </div>
                                ))
                            )}
                            {(!loading && !stats?.recentActivities?.length) && (
                                <p className="text-sm text-slate-400 italic">No recent activity</p>
                            )}
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default AdminDashboard;
