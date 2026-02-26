import { useAuth } from 'react-oidc-context';
import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Card, CardContent } from '../../components/ui/Card';
import { Button } from '../../components/ui/Button';

const Login = () => {
    const auth = useAuth();
    const { signinRedirect, isAuthenticated } = auth;
    const navigate = useNavigate();
    const { t } = useTranslation();

    useEffect(() => {
        if (isAuthenticated) {
            const roles = auth.user?.profile?.role;
            const roleArray: string[] = Array.isArray(roles) ? roles : roles ? [String(roles)] : [];
            const isAdmin = roleArray.some(r => r.toLowerCase() === 'admin');

            if (isAdmin) {
                navigate('/admin/dashboard');
            } else {
                navigate('/portal');
            }
        }
    }, [isAuthenticated, navigate, auth.user]);

    const handleLogin = () => {
        signinRedirect();
    };

    return (
        <div className="flex items-center justify-center min-h-[60vh] animate-fade-in">
            <Card variant="default" className="w-full max-w-md p-2 shadow-xl border-[var(--color-border)]">
                <CardContent className="p-8 text-center">
                    <div className="mb-6 flex justify-center">
                        <div className="w-16 h-16 bg-[var(--color-accent-light)]/20 rounded-full flex items-center justify-center">
                            <svg className="w-8 h-8 text-[var(--color-accent)]" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 16l-4-4m0 0l4-4m-4 4h14m-5 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h7a3 3 0 013 3v1" />
                            </svg>
                        </div>
                    </div>
                    <h1 className="text-3xl font-heading font-bold text-[var(--color-text-primary)] mb-4">{t('auth.login.welcome')}</h1>
                    <p className="text-[var(--color-text-secondary)] mb-8 leading-relaxed">
                        {t('auth.login.subtitle')}
                    </p>

                    <Button
                        onClick={handleLogin}
                        className="w-full shadow-lg shadow-blue-500/20 py-6 text-lg"
                    >
                        <span>{t('auth.login.btn_signin')}</span>
                    </Button>

                    <div className="mt-8 pt-6 border-t border-[var(--color-border)] text-sm text-[var(--color-text-muted)]">
                        <p className="mb-2">{t('auth.login.no_account')}</p>
                        <a href="/auth/register" className="text-[var(--color-accent)] font-bold hover:underline">{t('auth.login.claim_account')}</a>
                    </div>
                </CardContent>
            </Card>
        </div>
    );
};

export default Login;
