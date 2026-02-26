import { useAuth } from 'react-oidc-context';
import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

const Callback = () => {
    const auth = useAuth();
    const navigate = useNavigate();

    useEffect(() => {
        if (auth.isAuthenticated) {
            const roles = auth.user?.profile?.role;
            const roleArray: string[] = Array.isArray(roles) ? roles : roles ? [String(roles)] : [];
            const isAdmin = roleArray.some(r => r.toLowerCase() === 'admin');

            if (isAdmin) {
                navigate('/admin/dashboard');
            } else {
                navigate('/portal');
            }
        }
        if (auth.error) {
            console.error(auth.error);
        }
    }, [auth.isAuthenticated, auth.error, navigate, auth.user]);

    return (
        <div className="flex items-center justify-center h-screen bg-[var(--color-bg-dark)]">
            <div className="text-center">
                {auth.error ? (
                    <div className="text-red-500">
                        <h2 className="text-2xl font-bold mb-2">Login Error</h2>
                        <p>{auth.error.message}</p>
                        <button onClick={() => navigate('/auth/login')} className="mt-4 btn-primary">Back to Login</button>
                    </div>
                ) : (
                    <div className="text-[var(--color-accent)]">
                        <h2 className="text-2xl animate-pulse">Authenticating...</h2>
                    </div>
                )}
            </div>
        </div>
    );
};

export default Callback;
