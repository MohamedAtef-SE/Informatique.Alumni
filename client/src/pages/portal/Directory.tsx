import { useState } from 'react';
import { useQuery, keepPreviousData } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { alumniService } from '../../services/alumniService';
import type { AlumniSearchFilterDto } from '../../types/alumni';
import { Filter } from 'lucide-react';
import AlumniCard from '../../components/portal/AlumniCard';
import AlumniCardSkeleton from '../../components/portal/AlumniCardSkeleton';
import { Input } from '../../components/ui/Input';
import { Button } from '../../components/ui/Button';
import { Search, UserX } from 'lucide-react';

const Directory = () => {
    const { t } = useTranslation();
    const [filters, setFilters] = useState<AlumniSearchFilterDto>({
        filter: '',
        maxResultCount: 12,
        skipCount: 0
    });

    const { data, isError, isLoading } = useQuery({
        queryKey: ['alumni', filters],
        queryFn: () => alumniService.getList(filters),
        placeholderData: keepPreviousData
    });

    const items = data?.items ?? [];

    return (
        <div className="space-y-8 animate-fade-in">
            {/* Header & Controls */}
            <div className="relative group/filters">
                <div className="absolute -inset-0.5 bg-gradient-to-r from-[var(--color-accent)]/20 to-blue-500/20 rounded-2xl blur-xl opacity-0 group-hover/filters:opacity-100 transition-opacity duration-700 pointer-events-none"></div>
                <div className="relative flex flex-col md:flex-row md:items-center justify-between gap-6 bg-white border border-[var(--color-border)] p-6 rounded-2xl shadow-xl overflow-hidden">
                    <div className="relative z-10">
                        <h1 className="text-4xl font-heading font-black tracking-tight text-[var(--color-text-primary)]">
                            {t('directory.title')}
                        </h1>
                        <p className="text-[var(--color-text-secondary)] mt-2 flex items-center gap-2 font-medium">
                            <span className="w-2 h-2 rounded-full bg-[var(--color-accent)] animate-pulse" />
                            {t('directory.subtitle', { count: data?.totalCount ?? 0 })}
                        </p>
                    </div>

                    <div className="flex gap-4 w-full md:w-auto relative z-10">
                        <div className="relative w-full md:w-80 group/search">
                            <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-[var(--color-text-muted)] group-focus-within/search:text-[var(--color-accent)] transition-colors" />
                            <Input
                                placeholder={t('directory.search_placeholder')}
                                value={filters.filter || ''}
                                onChange={(e) => setFilters(prev => ({ ...prev, filter: e.target.value, skipCount: 0 }))}
                                className="pl-11 bg-[var(--color-secondary)]/50 border-[var(--color-border)] focus:border-[var(--color-accent)]/50 focus:ring-[var(--color-accent)]/10 h-12 rounded-xl transition-all"
                            />
                        </div>
                        <Button variant="outline" size="icon" className="shrink-0 h-12 w-12 rounded-xl border-[var(--color-border)] bg-[var(--color-secondary)]/50 hover:bg-[var(--color-secondary)] transition-all">
                            <Filter className="w-5 h-5 text-[var(--color-text-muted)]" />
                        </Button>
                    </div>

                    {/* Decorative backdrop shape */}
                    <div className="absolute -right-12 -top-12 w-32 h-32 bg-[var(--color-accent)]/5 rounded-full blur-3xl pointer-events-none" />
                </div>
            </div>

            {/* Grid */}
            {isError ? (
                <div className="text-center py-20 text-[var(--color-error)] bg-[var(--color-error-light)] rounded-xl border border-[var(--color-error)]/20">
                    {t('directory.error')}
                </div>
            ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-8 animate-in slide-in-from-bottom-8 duration-700">
                    {isLoading ? (
                        Array.from({ length: 8 }).map((_, i) => (
                            <AlumniCardSkeleton key={i} />
                        ))
                    ) : items.length === 0 ? (
                        <div className="col-span-full flex flex-col items-center justify-center py-32 bg-white/[0.01] rounded-3xl border border-dashed border-white/5 animate-in fade-in zoom-in duration-500">
                            <div className="w-20 h-20 rounded-full bg-slate-100/50 flex items-center justify-center mb-6 shadow-inner">
                                <UserX className="w-10 h-10 text-slate-400" />
                            </div>
                            <h3 className="text-xl font-bold text-[var(--color-text-primary)] mb-2">
                                {t('directory.no_results')}
                            </h3>
                            <p className="text-[var(--color-text-secondary)] max-w-sm text-center">
                                We couldn't find any alumni matching your current filter. Try adjusting your search or clearing filters.
                            </p>
                        </div>
                    ) : (
                        items.map((alumni) => (
                            <AlumniCard key={alumni.id} alumni={alumni} />
                        ))
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
