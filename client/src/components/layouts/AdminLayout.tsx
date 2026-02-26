import { Outlet, NavLink } from 'react-router-dom';
import { useAuth } from 'react-oidc-context';
import { LayoutDashboard, Users, Calendar, FileText, LogOut, Briefcase, Gift, Image, Landmark, HeartPulse, FileBadge, Building2, Plane, BookOpen, Mail } from 'lucide-react';
import clsx from 'clsx';

const adminNavItems = [
    { icon: LayoutDashboard, label: 'Dashboard', to: '/admin/dashboard' },
    { icon: Users, label: 'Alumni Manager', to: '/admin/alumni' },
    { icon: Calendar, label: 'Events Manager', to: '/admin/events' },
    { icon: FileText, label: 'Content (CMS)', to: '/admin/content' },
    { icon: Briefcase, label: 'Career & Jobs', to: '/admin/career' },
    { icon: Gift, label: 'Benefits', to: '/admin/benefits' },
    { icon: Image, label: 'Gallery', to: '/admin/gallery' },
    { icon: Landmark, label: 'Syndicates', to: '/admin/syndicates' },
    { icon: HeartPulse, label: 'Health', to: '/admin/health' },
    { icon: FileBadge, label: 'Certificates', to: '/admin/certificates' },
    { icon: Building2, label: 'Organization', to: '/admin/organization' },
    { icon: Plane, label: 'Trips', to: '/admin/trips' },
    { icon: BookOpen, label: 'Guidance', to: '/admin/guidance' },
    { icon: Mail, label: 'Communication', to: '/admin/communication' },
];

import GlobalLoader from '../ui/GlobalLoader';
import { Toaster } from 'sonner';

const AdminLayout = () => {
    const auth = useAuth();

    const handleLogout = async () => {
        await auth.signoutRedirect();
    };

    return (
        <div className="flex h-screen bg-bg-dark text-white font-sans selection:bg-accent selection:text-primary overflow-hidden">
            <GlobalLoader />
            <Toaster position="top-right" theme="light" richColors />
            {/* Admin Sidebar */}
            <aside className="w-[260px] flex flex-col border-r border-white/10 bg-primary">
                <div className="h-16 flex items-center px-6 border-b border-white/10 gap-3">
                    <div className="w-10 h-10 flex items-center justify-center">
                        <img src="/logo.png" alt="Informatique Alumni" className="w-full h-full object-contain" />
                    </div>
                    <span className="font-bold tracking-tight text-lg">Admin<span className="text-red-500">Portal</span></span>
                </div>

                <nav className="flex-1 py-6 px-3 space-y-1 overflow-y-auto custom-scrollbar">
                    <div className="px-3 mb-2 text-xs font-semibold text-gray-500 uppercase tracking-wider">Management</div>
                    {adminNavItems.filter(item => {
                        // Check roles
                        const roles = auth.user?.profile?.role;
                        const roleArray = Array.isArray(roles) ? roles : roles ? [String(roles)] : [];
                        const isSuperAdmin = roleArray.some(r => ['admin', 'systemadmin'].includes(r.toLowerCase()));

                        // Restricted items
                        if (item.label === 'Organization' && !isSuperAdmin) return false;
                        if (item.label === 'Communication' && !isSuperAdmin) return false;

                        return true;
                    }).map((item) => (
                        <NavLink
                            key={item.to}
                            to={item.to}
                            className={({ isActive }) => clsx(
                                'flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-all group',
                                isActive
                                    ? 'bg-red-500/10 text-red-500 border border-red-500/20'
                                    : 'text-gray-400 hover:text-white hover:bg-white/5'
                            )}
                        >
                            <item.icon className="w-4 h-4" />
                            {item.label}
                        </NavLink>
                    ))}
                </nav>

                <div className="p-4 border-t border-white/10">
                    <button
                        onClick={handleLogout}
                        className="flex items-center gap-3 px-3 py-2 text-sm text-gray-400 hover:text-white transition-colors w-full"
                    >
                        <LogOut className="w-4 h-4" /> Sign Out
                    </button>
                </div>
            </aside>

            {/* Main Content */}
            <main className="flex-1 min-w-0 relative flex flex-col overflow-hidden">
                {/* Background Details - Isolated to prevent scrollbars */}
                <div className="absolute inset-0 z-0 pointer-events-none overflow-hidden">
                    <div className="absolute top-[-20%] right-[-10%] w-[600px] h-[600px] bg-red-900/20 rounded-full blur-[120px]" />
                    <div className="absolute bottom-[-20%] left-[-10%] w-[500px] h-[500px] bg-primary/40 rounded-full blur-[100px]" />
                </div>

                {/* Scrollable Content Area */}
                <div className="relative z-10 flex-1 overflow-auto p-8">
                    <Outlet />
                </div>
            </main>
        </div>
    );
};

export default AdminLayout;
