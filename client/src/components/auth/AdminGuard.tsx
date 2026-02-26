import { useAuth } from 'react-oidc-context';
import { Navigate, Outlet, useLocation } from 'react-router-dom';

const AdminGuard = () => {
    const auth = useAuth();
    const location = useLocation();

    if (auth.isLoading) {
        return <div className="h-screen flex items-center justify-center text-gray-500">Checking permissions...</div>;
    }

    if (!auth.isAuthenticated) {
        return <Navigate to="/auth/login" state={{ from: location }} replace />;
    }

    // Check admin role from OIDC claims
    const roles = auth.user?.profile?.role;
    const roleArray: string[] = Array.isArray(roles) ? roles : roles ? [String(roles)] : [];
    const allowedRoles = ['admin', 'systemadmin', 'collegeadmin', 'employee'];
    const hasAccess = roleArray.some(r => allowedRoles.includes(r.toLowerCase()));

    if (!hasAccess) {
        return <Navigate to="/portal" replace />;
    }

    return <Outlet />;
};

export default AdminGuard;
