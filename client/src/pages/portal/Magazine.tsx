import { useQuery } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { magazineService } from '../../services/magazineService';
import { BookOpen, Download } from 'lucide-react';
import { Card } from '../../components/ui/Card';
import { Button } from '../../components/ui/Button';
import { MembershipGuard } from '../../components/common/MembershipGuard';

const Magazine = () => {
    const { t, i18n } = useTranslation();
    const { data } = useQuery({
        queryKey: ['magazines'],
        queryFn: () => magazineService.getList({ maxResultCount: 20 })
    });

    const handleDownload = async (id: string, title: string) => {
        try {
            const blob = await magazineService.download(id);
            const url = window.URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = url;
            link.download = `${title}.pdf`;
            document.body.appendChild(link);
            link.click();
            link.remove();
        } catch (error) {
            console.error("Failed to download issue", error);
            alert(t('magazine.download_error'));
        }
    };

    return (
        <div className="space-y-6 animate-fade-in">
            <div>
                <h1 className="text-3xl font-heading font-bold text-[var(--color-text-primary)]">{t('magazine.title')}</h1>
                <p className="text-[var(--color-text-secondary)] mt-1">{t('magazine.subtitle')}</p>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 animate-slide-up">
                {data?.items.map((issue) => (
                    <Card key={issue.id} variant="default" className="overflow-hidden flex flex-col group hover:border-[var(--color-accent)]/30 transition-all border-[var(--color-border)] hover:shadow-lg">
                        <div className="h-48 bg-slate-100 flex items-center justify-center relative border-b border-[var(--color-border)]">
                            <BookOpen className="w-16 h-16 text-slate-300 group-hover:text-[var(--color-accent)] transition-colors duration-300" />
                            <div className="absolute bottom-0 left-0 right-0 bg-white/90 backdrop-blur p-2 text-center text-xs text-[var(--color-text-secondary)] border-t border-[var(--color-border)]">
                                {t('magazine.published')}: {new Date(issue.issueDate).toLocaleDateString(i18n.language)}
                            </div>
                        </div>
                        <div className="p-6 flex flex-col flex-1">
                            <h3 className="text-xl font-bold text-[var(--color-text-primary)] mb-4 group-hover:text-[var(--color-accent)] transition-colors line-clamp-2">
                                {issue.title}
                            </h3>
                            <div className="mt-auto">
                                <MembershipGuard onActive={() => handleDownload(issue.id, issue.title)}>
                                    <Button
                                        variant="outline"
                                        className="w-full flex items-center justify-center gap-2 group-hover:bg-[var(--color-accent)] group-hover:text-white group-hover:border-[var(--color-accent)] transition-all"
                                    >
                                        <Download className="w-4 h-4" /> {t('magazine.download_pdf')}
                                    </Button>
                                </MembershipGuard>
                            </div>
                        </div>
                    </Card>
                ))}

                {data?.items.length === 0 && (
                    <div className="col-span-full text-center py-20 text-[var(--color-text-muted)] bg-slate-50 rounded-xl border border-dashed border-[var(--color-border)]">
                        {t('magazine.no_issues')}
                    </div>
                )}
            </div>
        </div>
    );
};

export default Magazine;

