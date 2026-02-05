
import { useTranslation } from 'react-i18next';
import { Card, CardContent } from '../../components/ui/Card';
import { Button } from '../../components/ui/Button';

const Register = () => {

    const { t } = useTranslation();

    return (
        <div className="flex items-center justify-center min-h-[60vh] animate-fade-in">
            <Card variant="default" className="w-full max-w-md p-2 shadow-xl border-[var(--color-border)]">
                <CardContent className="p-8 text-center">
                    <div className="mb-6 flex justify-center">
                        <div className="w-16 h-16 bg-[var(--color-accent-light)]/20 rounded-full flex items-center justify-center">
                            <svg className="w-8 h-8 text-[var(--color-accent)]" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z" />
                            </svg>
                        </div>
                    </div>
                    <h1 className="text-3xl font-heading font-bold text-[var(--color-text-primary)] mb-4">{t('auth.register.title')}</h1>
                    <p className="text-[var(--color-text-secondary)] mb-8 leading-relaxed">
                        {t('auth.register.subtitle')}
                    </p>

                    <Button
                        onClick={() => window.location.href = 'mailto:alumni-support@university.edu'}
                        className="w-full shadow-lg shadow-blue-500/20 py-6 text-lg"
                    >
                        <span>{t('auth.register.contact_support')}</span>
                    </Button>

                    <div className="mt-8 pt-6 border-t border-[var(--color-border)] text-sm text-[var(--color-text-muted)]">
                        <p className="mb-2">{t('auth.register.has_account')}</p>
                        <a href="/auth/login" className="text-[var(--color-accent)] font-bold hover:underline">{t('auth.register.login')}</a>
                    </div>
                </CardContent>
            </Card>
        </div>
    );
};

export default Register;
