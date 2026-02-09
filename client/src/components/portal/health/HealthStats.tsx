import { Heart, Activity, Percent, ShieldCheck } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { Card, CardContent } from "../../ui/Card";
import { useQuery } from '@tanstack/react-query';
import { servicesAppService } from '../../../services/servicesService';

export const HealthStats = () => {
    const { t } = useTranslation();

    const { data: statsData, isLoading } = useQuery({
        queryKey: ['healthStats'],
        queryFn: servicesAppService.getHealthStats,
        refetchOnWindowFocus: false
    });

    const stats = [
        {
            icon: Heart,
            label: t('services.health.stats.partners', 'Medical Partners'),
            value: isLoading ? "..." : `${statsData?.medicalPartnersCount || 0}+`,
            color: "text-rose-500",
            bg: "bg-rose-500/10"
        },
        {
            icon: Percent,
            label: t('services.health.stats.savings', 'Average Savings'),
            value: isLoading ? "..." : (statsData?.averageSavings || "20%"),
            color: "text-emerald-500",
            bg: "bg-emerald-500/10"
        },
        {
            icon: ShieldCheck,
            label: t('services.health.stats.verified', 'Verified Quality'),
            value: isLoading ? "..." : (statsData?.verifiedQuality || "100%"),
            color: "text-blue-500",
            bg: "bg-blue-500/10"
        },
        {
            icon: Activity,
            label: t('services.health.stats.active', 'Offers Active'),
            value: isLoading ? "..." : `${statsData?.activeOffersCount || 0}`,
            color: "text-violet-500",
            bg: "bg-violet-500/10"
        }
    ];

    return (
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
            {stats.map((stat, index) => (
                <Card key={index} className="border-[var(--color-border)] bg-[var(--color-bg-card)] shadow-sm hover:shadow-md transition-all">
                    <CardContent className="p-4 flex items-center gap-4">
                        <div className={`p-3 rounded-full ${stat.bg} ${stat.color}`}>
                            <stat.icon className="w-6 h-6" />
                        </div>
                        <div>
                            <p className="text-2xl font-bold text-[var(--color-text-primary)] min-h-[2rem] flex items-center">
                                {stat.value}
                            </p>
                            <p className="text-xs text-[var(--color-text-secondary)] font-medium">{stat.label}</p>
                        </div>
                    </CardContent>
                </Card>
            ))}
        </div>
    );
};
