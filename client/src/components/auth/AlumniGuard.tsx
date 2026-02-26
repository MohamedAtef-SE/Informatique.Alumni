import React from 'react';
import { useAuth } from 'react-oidc-context';
import { Navigate, Outlet, useLocation } from 'react-router-dom';

/**
 * AlumniGuard — Protects /portal routes.
 *
 * • If user is NOT authenticated → redirect to login.
 * • If user IS authenticated AND has the "alumni" role → render children.
 * • If user IS authenticated but does NOT have "alumni" role → show a
 *   friendly modal with a 5-second countdown, then auto-logout.
 */
const AlumniGuard: React.FC = () => {
    const auth = useAuth();
    const location = useLocation();


    // ------- role extraction -------
    const roles = auth.user?.profile?.role;
    const roleArray: string[] = Array.isArray(roles) ? roles : roles ? [roles] : [];
    const isAlumni = roleArray.some(r => r.toLowerCase() === 'alumni');
    const isAdmin = roleArray.some(r => r.toLowerCase() === 'admin');

    // ------- loading state -------
    if (auth.isLoading) {
        return (
            <div className="h-screen flex items-center justify-center text-gray-500">
                Checking permissions...
            </div>
        );
    }

    // ------- not authenticated -------
    if (!auth.isAuthenticated) {
        return <Navigate to="/auth/login" state={{ from: location }} replace />;
    }

    // ------- admin users → redirect to admin dashboard -------
    if (isAdmin) {
        return <Navigate to="/admin/dashboard" replace />;
    }

    // ------- non-alumni user → redirect to login -------
    if (!isAlumni) {
        return <Navigate to="/auth/login" replace />;
    }

    // ------- alumni user → allow access -------
    return <Outlet />;
};

export default AlumniGuard;
