import { Outlet, useLocation } from 'react-router-dom';
import { useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { cn } from '../../utils/cn';
import Navbar from './Navbar';
import GlobalLoader from '../ui/GlobalLoader';
import { Toaster } from 'sonner';

const MainLayout = () => {
    const { i18n } = useTranslation();
    const location = useLocation();
    const isHome = location.pathname === '/portal';

    useEffect(() => {
        const dir = i18n.dir();
        document.documentElement.dir = dir;
        document.documentElement.lang = i18n.language;
    }, [i18n.language]);

    // Navbar handles its own state mostly, simplified layout
    return (
        <div className="min-h-screen relative overflow-x-hidden bg-[var(--color-background)]">
            {/* Background elements */}
            <div className="fixed inset-0 bg-[radial-gradient(circle_at_50%_-20%,var(--color-accent-light),transparent)] opacity-30 pointer-events-none" />

            <GlobalLoader />
            <Toaster position="top-right" theme="light" richColors />

            <Navbar />

            {/* Main Content Area */}
            {/* Added pt-28 to account for fixed navbar height */}
            <main className="min-h-screen relative flex flex-col pt-28">
                <div className={cn(
                    "w-full mx-auto space-y-8 animate-fade-in",
                    // Apply standard padding for all routes EXCEPT the portal home (dashboard)
                    isHome
                        ? "p-0 max-w-none"
                        : "p-4 lg:p-8 max-w-[1600px]"
                )}>
                    <Outlet />
                </div>
            </main>
        </div>
    );
};

export default MainLayout;
