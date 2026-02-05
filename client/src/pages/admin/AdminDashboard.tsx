import { Users, Calendar, Eye, Activity } from 'lucide-react';

const StatCard = ({ title, value, icon: Icon, color }: any) => (
    <div className="glass-panel p-6 flex items-center gap-4">
        <div className={`w-12 h-12 rounded-lg flex items-center justify-center ${color}`}>
            <Icon className="w-6 h-6 text-white" />
        </div>
        <div>
            <p className="text-sm text-gray-400">{title}</p>
            <p className="text-2xl font-bold text-white">{value}</p>
        </div>
    </div>
);

const AdminDashboard = () => {
    return (
        <div className="space-y-8">
            <div>
                <h1 className="text-3xl font-bold text-white">Admin Dashboard</h1>
                <p className="text-gray-400">Overview of system activity and requests.</p>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
                <StatCard title="Total Alumni" value="1,240" icon={Users} color="bg-blue-600" />
                <StatCard title="Pending Approvals" value="12" icon={Activity} color="bg-amber-600" />
                <StatCard title="Active Events" value="3" icon={Calendar} color="bg-emerald-600" />
                <StatCard title="Page Views" value="45.2k" icon={Eye} color="bg-purple-600" />
            </div>

            <div className="glass-panel p-8 text-center py-20 text-gray-500">
                Chart placeholders would go here (New Registrations, Event Popularity, etc.)
            </div>
        </div>
    );
};

export default AdminDashboard;
