import { Outlet } from 'react-router-dom';

const AuthLayout = () => {
    return (
        <div className="min-h-screen flex items-center justify-center bg-[var(--color-background)] relative overflow-hidden">
            <div className="absolute inset-0 bg-gradient-to-br from-[var(--color-accent-light)]/30 to-slate-200/50 pointer-events-none" />
            <div className="absolute -top-40 -right-40 w-96 h-96 bg-[var(--color-accent)]/10 rounded-full blur-3xl pointer-events-none" />
            <div className="absolute bottom-0 left-0 w-full h-1/2 bg-gradient-to-t from-white to-transparent pointer-events-none" />

            <div className="relative z-10 w-full max-w-md p-4">
                <Outlet />
            </div>
        </div>
    );
};

export default AuthLayout;
