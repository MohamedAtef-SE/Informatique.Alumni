import { useState } from 'react';
import { useQuery, keepPreviousData } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { alumniService } from '../../services/alumniService';
import type { AlumniSearchFilterDto } from '../../types/alumni';
import { Filter } from 'lucide-react';
import AlumniCard from '../../components/portal/AlumniCard';
import { Input } from '../../components/ui/Input';
import { Button } from '../../components/ui/Button';

const Directory = () => {
    const { t } = useTranslation();
    const [filters, setFilters] = useState<AlumniSearchFilterDto>({
        filter: '',
        maxResultCount: 12,
        skipCount: 0
    });

    const { data, isError } = useQuery({
        queryKey: ['alumni', filters],
        queryFn: () => alumniService.getList(filters),
        placeholderData: keepPreviousData
    });

    const items = data?.items ?? [];

    return (
        <div className="space-y-8 animate-fade-in">
            {/* Header & Controls */}
            <div className="flex flex-col md:flex-row md:items-end justify-between gap-4 border-b border-[var(--color-border)] pb-6">
                <div>
                    <h1 className="text-3xl font-heading font-bold text-[var(--color-text-primary)]">
                        {t('directory.title')}
                    </h1>
                    <p className="text-[var(--color-text-secondary)] mt-2">{t('directory.subtitle', { count: data?.totalCount ?? 0 })}</p>
                </div>

                <div className="flex gap-3 w-full md:w-auto">
                    <div className="w-full md:w-72">
                        <Input
                            placeholder={t('directory.search_placeholder')}
                            value={filters.filter || ''}
                            onChange={(e) => setFilters(prev => ({ ...prev, filter: e.target.value, skipCount: 0 }))}
                            className="bg-white border-[var(--color-border)]"
                        />
                    </div>
                    <Button variant="outline" size="icon" className="shrink-0 bg-white">
                        <Filter className="w-5 h-5 text-[var(--color-text-muted)]" />
                    </Button>
                </div>
            </div>

            {/* Grid */}
            {isError ? (
                <div className="text-center py-20 text-[var(--color-error)] bg-[var(--color-error-light)] rounded-xl border border-[var(--color-error)]/20">
                    {t('directory.error')}
                </div>
            ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 animate-slide-up">
                    {items.map((alumni) => (
                        <AlumniCard key={alumni.id} alumni={alumni} />
                    ))}

                    {items.length === 0 && (
                        <div className="col-span-full text-center py-20 text-[var(--color-text-muted)] bg-slate-50 rounded-xl border border-dashed border-[var(--color-border)]">
                            {t('directory.no_results')}
                        </div>
                    )}
                </div>
            )}

            {/* Pagination (Basic) */}
            <div className="flex justify-center pt-8 gap-4">
                <Button
                    variant="ghost"
                    disabled={filters.skipCount === 0}
                    onClick={() => setFilters(prev => ({ ...prev, skipCount: (prev.skipCount || 0) - 12 }))}
                >
                    {t('directory.pagination.previous')}
                </Button>
                <span className="text-[var(--color-text-secondary)] text-sm flex items-center font-medium">
                    {t('directory.pagination.page', { page: (filters.skipCount || 0) / 12 + 1 })}
                </span>
                <Button
                    variant="ghost"
                    disabled={(filters.skipCount || 0) + 12 >= (data?.totalCount || 0)}
                    onClick={() => setFilters(prev => ({ ...prev, skipCount: (prev.skipCount || 0) + 12 }))}
                >
                    {t('directory.pagination.next')}
                </Button>
            </div>
        </div>
    );
};

export default Directory;
