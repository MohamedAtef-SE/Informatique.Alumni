import { useState, useEffect } from 'react';
import { NavLink, Link, useLocation } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { useAuth } from 'react-oidc-context';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import {
    LayoutDashboard,
    Calendar,
    Briefcase,
    FileText,
    UserCircle,
    Image,
    BookOpen,
    Menu,
    X,
    LogOut,
    GraduationCap,
    Search,
    Bell,
    ChevronDown
} from 'lucide-react';
import { cn } from '../../utils/cn';
import { Button } from '../ui/Button';
import { alumniService } from '../../services/alumniService';

const navItems = [
    { icon: LayoutDashboard, label: 'Home', to: '/portal' },
    { icon: UserCircle, label: 'My Profile', to: '/portal/profile', mobileOnly: true },
    { icon: Calendar, label: 'Events', to: '/portal/events' },
    { icon: GraduationCap, label: 'Directory', to: '/portal/directory' },
    { icon: Briefcase, label: 'Career', to: '/portal/career' },
    { icon: FileText, label: 'Services', to: '/portal/services' },
    { icon: Image, label: 'Gallery', to: '/portal/gallery' },
    { icon: BookOpen, label: 'Magazine', to: '/portal/magazine' },
];

const Navbar = () => {
    const [scrolled, setScrolled] = useState(false);
    const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
    const [userMenuOpen, setUserMenuOpen] = useState(false);
    const auth = useAuth();
    const user = auth.user?.profile;
    const location = useLocation();

    // Fetch profile to get photo URL
    const { data: profile } = useQuery({
        queryKey: ['my-profile'],
        queryFn: () => alumniService.getMyProfile(),
        enabled: !!auth.isAuthenticated,
    });

    useEffect(() => {
        const handleScroll = () => setScrolled(window.scrollY > 20);
        window.addEventListener('scroll', handleScroll);
        return () => window.removeEventListener('scroll', handleScroll);
    }, []);

    const getInitials = (name?: string) => {
        if (!name) return 'U';
        return name.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase();
    };

    const handleSignOut = async () => {
        try {
            await auth.removeUser();
            const returnUrl = encodeURIComponent(window.location.origin + '/auth/login');
            window.location.href = `https://localhost:44386/Account/Logout?returnUrl=${returnUrl}`;
        } catch (error) {
            console.error('Sign out error:', error);
            window.location.href = '/auth/login';
        }
    };

    const { i18n, t } = useTranslation();

    const changeLanguage = (lng: string) => {
        i18n.changeLanguage(lng);
    };

    return (
        <>
            <motion.nav
                initial={{ y: -100 }}
                animate={{ y: 0 }}
                className={cn(
                    "fixed top-0 left-0 right-0 z-50 transition-all duration-300 border-b",
                    scrolled
                        ? "bg-white/95 backdrop-blur-xl border-[var(--color-border)] h-20 shadow-md"
                        : "bg-white/80 backdrop-blur-sm border-transparent h-28"
                )}
            >
                <div className="container mx-auto px-4 h-full flex items-center justify-between">
                    {/* Brand */}
                    <Link to="/portal" className="flex items-center gap-3 group">
                        <motion.div
                            whileHover={{ rotate: 5 }}
                            transition={{ duration: 0.3 }}
                            className={cn(
                                "relative flex items-center justify-center transition-all duration-300",
                                scrolled ? "w-16 h-16" : "w-24 h-24"
                            )}
                        >
                            <img src="/logo.png" alt="Informatique Alumni" className="w-full h-full object-contain" />
                        </motion.div>
                    </Link>

                    {/* Desktop Navigation */}
                    <div className="hidden lg:flex items-center gap-1">
                        {navItems.map((item) => {
                            if (item.mobileOnly) return null;
                            const isActive = location.pathname.startsWith(item.to);
                            return (
                                <Link key={item.to} to={item.to}>
                                    <motion.div
                                        className="relative px-4 py-2"
                                        whileHover="hover"
                                        initial="initial"
                                    >
                                        <span className={cn(
                                            "relative z-10 text-sm font-medium transition-colors",
                                            isActive ? "text-[var(--color-accent)]" : "text-[var(--color-text-secondary)] hover:text-[var(--color-text-primary)]"
                                        )}>
                                            {/* We use a simple key mapping here or update navItems to use translation keys directly */}
                                            {t(`nav.${item.label.toLowerCase().replace(' ', '_')}`, item.label)}
                                        </span>
                                        {isActive && (
                                            <motion.div
                                                layoutId="activeNav"
                                                className="absolute inset-0 bg-[var(--color-accent-light)] rounded-lg -z-0"
                                                transition={{ type: "spring", stiffness: 300, damping: 30 }}
                                            />
                                        )}
                                        <motion.div
                                            variants={{
                                                initial: { scaleX: 0, opacity: 0 },
                                                hover: { scaleX: 1, opacity: 1 }
                                            }}
                                            transition={{ duration: 0.3 }}
                                            className="absolute bottom-0 left-0 right-0 h-0.5 bg-[var(--color-accent)] rounded-full mx-4"
                                        />
                                    </motion.div>
                                </Link>
                            );
                        })}
                    </div>

                    {/* Right Actions */}
                    <div className="hidden lg:flex items-center gap-4">
                        {/* Language Switcher */}
                        <div className="flex bg-slate-100 rounded-lg p-1">
                            <button
                                onClick={() => changeLanguage('en')}
                                className={cn(
                                    "px-2 py-1 text-xs font-bold rounded-md transition-all",
                                    i18n.language === 'en' ? "bg-white text-[var(--color-accent)] shadow-sm" : "text-[var(--color-text-muted)] hover:text-[var(--color-text-primary)]"
                                )}
                            >
                                EN
                            </button>
                            <button
                                onClick={() => changeLanguage('ar')}
                                className={cn(
                                    "px-2 py-1 text-xs font-bold rounded-md transition-all",
                                    i18n.language === 'ar' ? "bg-white text-[var(--color-accent)] shadow-sm" : "text-[var(--color-text-muted)] hover:text-[var(--color-text-primary)]"
                                )}
                            >
                                AR
                            </button>
                        </div>

                        <div className="relative group">
                            <Search className="w-5 h-5 text-[var(--color-text-secondary)] group-hover:text-[var(--color-accent)] transition-colors cursor-pointer" />
                        </div>

                        <div className="relative group">
                            <Bell className="w-5 h-5 text-[var(--color-text-secondary)] group-hover:text-[var(--color-accent)] transition-colors cursor-pointer" />
                            <span className="absolute -top-1 -right-1 w-2 h-2 bg-[var(--color-error)] rounded-full animate-pulse" />
                        </div>

                        <div className="w-px h-6 bg-[var(--color-border)] mx-2" />

                        {/* User Dropdown */}
                        <div className="relative">
                            <button
                                onClick={() => setUserMenuOpen(!userMenuOpen)}
                                className="flex items-center gap-2 hover:bg-[var(--color-secondary)] p-1 rounded-full pr-3 transition-colors border border-transparent hover:border-[var(--color-border)]"
                            >
                                {profile?.photoUrl ? (
                                    <img
                                        src={alumniService.getPhotoUrl(profile.photoUrl)}
                                        alt={user?.name || 'Profile'}
                                        className="w-8 h-8 rounded-full object-cover shadow-md ring-2 ring-[var(--color-border)]"
                                        onError={(e) => {
                                            e.currentTarget.style.display = 'none';
                                            const fallback = e.currentTarget.nextElementSibling as HTMLElement;
                                            if (fallback) fallback.style.display = 'flex';
                                        }}
                                    />
                                ) : null}
                                <div className={cn(
                                    "w-8 h-8 rounded-full bg-[var(--color-accent)] flex items-center justify-center text-xs font-bold text-white shadow-md",
                                    profile?.photoUrl && "hidden"
                                )}>
                                    {getInitials(user?.name)}
                                </div>
                                <ChevronDown className={cn("w-4 h-4 text-[var(--color-text-secondary)] transition-transform", userMenuOpen ? "rotate-180" : "")} />
                            </button>

                            <AnimatePresence>
                                {userMenuOpen && (
                                    <motion.div
                                        initial={{ opacity: 0, y: 10, scale: 0.95 }}
                                        animate={{ opacity: 1, y: 0, scale: 1 }}
                                        exit={{ opacity: 0, y: 10, scale: 0.95 }}
                                        className="absolute right-0 top-full mt-2 w-56 bg-white border border-[var(--color-border)] rounded-xl shadow-lg overflow-hidden py-1 ltr:right-0 rtl:left-0 rtl:right-auto"
                                    >
                                        <div className="px-4 py-3 border-b border-[var(--color-border)]">
                                            <p className="text-sm font-semibold text-[var(--color-text-primary)] truncate">{user?.name}</p>
                                            <p className="text-xs text-[var(--color-text-secondary)] truncate">{user?.email}</p>
                                        </div>
                                        <Link to="/portal/profile" className="flex items-center gap-2 px-4 py-2.5 text-sm text-[var(--color-text-secondary)] hover:bg-[var(--color-secondary)] hover:text-[var(--color-text-primary)] transition-colors">
                                            <UserCircle className="w-4 h-4" /> {t('nav.profile', 'My Profile')}
                                        </Link>
                                        <button
                                            onClick={handleSignOut}
                                            className="w-full flex items-center gap-2 px-4 py-2.5 text-sm text-[var(--color-error)] hover:bg-[var(--color-error-light)] transition-colors"
                                        >
                                            <LogOut className="w-4 h-4" /> {t('nav.sign_out', 'Sign Out')}
                                        </button>
                                    </motion.div>
                                )}
                            </AnimatePresence>
                        </div>
                    </div>

                    {/* Mobile Menu Button */}
                    <button
                        onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
                        className="lg:hidden p-2 text-[var(--color-text-secondary)] hover:text-[var(--color-accent)]"
                    >
                        {mobileMenuOpen ? <X className="w-6 h-6" /> : <Menu className="w-6 h-6" />}
                    </button>
                </div>
            </motion.nav>

            {/* Mobile Navigation Drawer */}
            <AnimatePresence>
                {mobileMenuOpen && (
                    <>
                        <motion.div
                            initial={{ opacity: 0 }}
                            animate={{ opacity: 1 }}
                            exit={{ opacity: 0 }}
                            onClick={() => setMobileMenuOpen(false)}
                            className="fixed inset-0 bg-black/30 backdrop-blur-sm z-40 lg:hidden"
                        />
                        <motion.div
                            initial={{ x: '100%' }}
                            animate={{ x: 0 }}
                            exit={{ x: '100%' }}
                            transition={{ type: "spring", damping: 25, stiffness: 200 }}
                            className="fixed top-0 right-0 bottom-0 w-[280px] bg-white border-l border-[var(--color-border)] z-50 lg:hidden flex flex-col shadow-2xl"
                        >
                            <div className="p-6 border-b border-[var(--color-border)] flex justify-between items-center">
                                <span className="text-lg font-bold text-[var(--color-text-primary)]">Menu</span>
                                {/* Mobile Language Switcher */}
                                <div className="flex bg-slate-100 rounded-lg p-1">
                                    <button
                                        onClick={() => changeLanguage('en')}
                                        className={cn(
                                            "px-2 py-1 text-xs font-bold rounded-md transition-all",
                                            i18n.language === 'en' ? "bg-white text-[var(--color-accent)] shadow-sm" : "text-[var(--color-text-muted)] hover:text-[var(--color-text-primary)]"
                                        )}
                                    >
                                        EN
                                    </button>
                                    <button
                                        onClick={() => changeLanguage('ar')}
                                        className={cn(
                                            "px-2 py-1 text-xs font-bold rounded-md transition-all",
                                            i18n.language === 'ar' ? "bg-white text-[var(--color-accent)] shadow-sm" : "text-[var(--color-text-muted)] hover:text-[var(--color-text-primary)]"
                                        )}
                                    >
                                        AR
                                    </button>
                                </div>
                            </div>
                            <div className="flex-1 overflow-y-auto py-4 px-3 space-y-1">
                                {navItems.map((item) => (
                                    <NavLink
                                        key={item.to}
                                        to={item.to}
                                        onClick={() => setMobileMenuOpen(false)}
                                        className={({ isActive }) => cn(
                                            "flex items-center gap-3 px-4 py-3 rounded-lg text-sm font-medium transition-colors",
                                            isActive
                                                ? "bg-[var(--color-accent-light)] text-[var(--color-accent)] border border-[var(--color-accent)]/20"
                                                : "text-[var(--color-text-secondary)] hover:bg-[var(--color-secondary)] hover:text-[var(--color-text-primary)]"
                                        )}
                                    >
                                        <item.icon className="w-5 h-5" />
                                        {t(`nav.${item.label.toLowerCase().replace(' ', '_')}`, item.label)}
                                    </NavLink>
                                ))}
                            </div>
                            <div className="p-4 border-t border-[var(--color-border)]">
                                <Button
                                    onClick={handleSignOut}
                                    className="w-full bg-[var(--color-error-light)] hover:bg-[var(--color-error)] hover:text-white text-[var(--color-error)] border-none"
                                >
                                    <LogOut className="w-4 h-4 mr-2" /> {t('nav.sign_out', 'Sign Out')}
                                </Button>
                            </div>
                        </motion.div>
                    </>
                )}
            </AnimatePresence>
        </>
    );
};

export default Navbar;
