import { useAuth } from 'react-oidc-context';
import { Navigate, Outlet, useLocation } from 'react-router-dom';

const AdminGuard = () => {
    const auth = useAuth();
    const location = useLocation();

    if (auth.isLoading) {
        return <div className="h-screen flex items-center justify-center text-gray-500">Checking permissions...</div>;
    }

    if (!auth.isAuthenticated) {
        return <Navigate to="/login" state={{ from: location }} replace />;
    }

    // TODO: Implement actual Role Check here. 
    // For now, we assume anyone who can login to this specific route via direct link is authorized OR we just check a claim.
    // const isAdmin = auth.user?.profile?.role === 'admin'; 
    // if (!isAdmin) return <Navigate to="/portal/dashboard" replace />;

    // For Development/Demo purposes, we allow access.
    return <Outlet />;
};

export default AdminGuard;
