import {
    AreaChart,
    Area,
    XAxis,
    YAxis,
    CartesianGrid,
    Tooltip,
    ResponsiveContainer,
    PieChart,
    Pie,
    Cell
} from 'recharts';
import type { DashboardStatsDto } from '../../types/admin';

interface DashboardChartsProps {
    stats: DashboardStatsDto | null;
    loading: boolean;
}

const COLORS = ['#2D96D7', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#ec4899'];

export const RegistrationTrendChart = ({ stats, loading }: DashboardChartsProps) => {
    if (loading) {
        return <div className="h-[300px] w-full bg-slate-50 animate-pulse rounded-lg" />;
    }

    if (!stats?.monthlyRegistrations?.length) {
        return (
            <div className="h-[300px] flex items-center justify-center text-slate-400 bg-slate-50 rounded-lg border border-dashed border-slate-200">
                Not enough data to display trends
            </div>
        );
    }

    return (
        <div className="h-[300px] w-full min-w-0">
            <ResponsiveContainer width="100%" height="100%">
                <AreaChart
                    data={stats.monthlyRegistrations}
                    margin={{ top: 10, right: 10, left: 0, bottom: 0 }}
                >
                    <defs>
                        <linearGradient id="colorCount" x1="0" y1="0" x2="0" y2="1">
                            <stop offset="5%" stopColor="#2D96D7" stopOpacity={0.3} />
                            <stop offset="95%" stopColor="#2D96D7" stopOpacity={0} />
                        </linearGradient>
                    </defs>
                    <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#e2e8f0" />
                    <XAxis
                        dataKey="month"
                        axisLine={false}
                        tickLine={false}
                        tick={{ fill: '#64748b', fontSize: 12 }}
                        dy={10}
                    />
                    <YAxis
                        axisLine={false}
                        tickLine={false}
                        tick={{ fill: '#64748b', fontSize: 12 }}
                    />
                    <Tooltip
                        contentStyle={{
                            backgroundColor: '#fff',
                            borderRadius: '8px',
                            border: '1px solid #e2e8f0',
                            boxShadow: '0 4px 6px -1px rgba(0, 0, 0, 0.1)'
                        }}
                        cursor={{ stroke: '#94a3b8', strokeWidth: 1 }}
                    />
                    <Area
                        type="monotone"
                        dataKey="count"
                        stroke="#2D96D7"
                        strokeWidth={3}
                        fillOpacity={1}
                        fill="url(#colorCount)"
                    />
                </AreaChart>
            </ResponsiveContainer>
        </div>
    );
};

export const CollegeDistributionChart = ({ stats, loading }: DashboardChartsProps) => {
    if (loading) {
        return <div className="h-[300px] w-full bg-slate-50 animate-pulse rounded-lg" />;
    }

    if (!stats?.alumniByCollege?.length) {
        return (
            <div className="h-[300px] flex items-center justify-center text-slate-400 bg-slate-50 rounded-lg border border-dashed border-slate-200">
                No college data available
            </div>
        );
    }

    // Filter out zero values for better pie chart
    const data = stats.alumniByCollege.filter(item => item.count > 0);

    if (data.length === 0) {
        return (
            <div className="h-[300px] flex items-center justify-center text-slate-400 bg-slate-50 rounded-lg border border-dashed border-slate-200">
                No alumni assigned to colleges yet
            </div>
        );
    }

    return (
        <div className="flex flex-col h-[400px]">
            {/* Chart Area - Top Half */}
            <div className="flex-none h-[220px] w-full min-w-0 relative">
                <ResponsiveContainer width="100%" height="100%">
                    <PieChart>
                        <Pie
                            data={data}
                            cx="50%"
                            cy="50%"
                            innerRadius={60}
                            outerRadius={90}
                            paddingAngle={2}
                            dataKey="count"
                            nameKey="label"
                            strokeWidth={0}
                        >
                            {data.map((_, index) => (
                                <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                            ))}
                        </Pie>
                        <Tooltip
                            contentStyle={{
                                backgroundColor: '#fff',
                                borderRadius: '8px',
                                border: '1px solid #e2e8f0',
                                boxShadow: '0 4px 6px -1px rgba(0, 0, 0, 0.1)'
                            }}
                        />
                    </PieChart>
                </ResponsiveContainer>
            </div>

            {/* Legend List - Bottom Half (Scrollable) */}
            <div className="flex-1 overflow-y-auto px-2 custom-scrollbar border-t border-slate-100 pt-2">
                <div className="space-y-2">
                    {data.map((item, index) => (
                        <div key={index} className="flex items-center justify-between p-2 rounded-md hover:bg-slate-50 transition-colors group">
                            <div className="flex items-center gap-3 min-w-0">
                                <span
                                    className="w-3 h-3 rounded-full flex-shrink-0 ring-2 ring-offset-2 ring-transparent group-hover:ring-slate-200 transition-all"
                                    style={{ backgroundColor: COLORS[index % COLORS.length] }}
                                />
                                <div className="flex flex-col min-w-0">
                                    <span className="text-sm font-medium text-slate-700 truncate" title={item.label}>
                                        {item.label}
                                    </span>
                                </div>
                            </div>
                            <div className="flex flex-col items-end flex-shrink-0 ml-4">
                                <span className="text-sm font-bold text-slate-900">{item.count}</span>
                                <span className="text-xs text-slate-500 font-medium bg-slate-100 px-1.5 rounded-full">
                                    {((item.count / (stats.totalAlumni || 1)) * 100).toFixed(1)}%
                                </span>
                            </div>
                        </div>
                    ))}
                </div>
            </div>
        </div>
    );
};
