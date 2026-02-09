import { MapPin, Phone, Tag, Pill, Building2, FlaskConical, Stethoscope, Eye, LayoutGrid } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { useState } from 'react';
import { Card, CardContent } from "../../ui/Card";
import { Button } from "../../ui/Button";
import { type MedicalPartnerDto, MedicalPartnerType } from "../../../types/health";

interface MedicalPartnerCardProps {
    partner: MedicalPartnerDto;
    onViewOffers: (partner: MedicalPartnerDto) => void;
}

export const MedicalPartnerCard = ({ partner, onViewOffers }: MedicalPartnerCardProps) => {
    const { t } = useTranslation();
    const [imgError, setImgError] = useState(false);

    const getPartnerTypeLabel = (type: number) => {
        switch (type) {
            case MedicalPartnerType.Pharmacy: return 'Pharmacy';
            case MedicalPartnerType.Hospital: return 'Hospital';
            case MedicalPartnerType.Lab: return 'Lab';
            case MedicalPartnerType.Clinic: return 'Clinic';
            default: return 'Medical';
        }
    };

    const getCategoryIcon = (partner: MedicalPartnerDto) => {
        if (partner.type === MedicalPartnerType.Pharmacy) return <Pill className="w-12 h-12" />;
        if (partner.type === MedicalPartnerType.Hospital) return <Building2 className="w-12 h-12" />;
        if (partner.type === MedicalPartnerType.Lab) return <FlaskConical className="w-12 h-12" />;
        if (partner.category?.toLowerCase().includes('dental')) return <Stethoscope className="w-12 h-12" />;
        if (partner.category?.toLowerCase().includes('optic')) return <Eye className="w-12 h-12" />;

        return <LayoutGrid className="w-12 h-12" />;
    };

    // Determine max discount if available
    const activeOffersCount = partner.offers?.filter(o => o.isActive).length || 0;

    return (
        <Card className="group overflow-hidden border-[var(--color-border)] hover:border-[var(--color-accent)]/50 transition-all duration-300 hover:shadow-lg bg-[var(--color-bg-card)]">
            <div className="relative h-32 bg-gradient-to-r from-[var(--color-bg-secondary)] to-[var(--color-bg-primary)] p-4 flex items-center justify-center">
                {!imgError && partner.logoUrl ? (
                    <img
                        src={partner.logoUrl}
                        alt={partner.name}
                        className="h-24 object-contain drop-shadow-md group-hover:scale-105 transition-transform duration-300"
                        onError={() => setImgError(true)}
                    />
                ) : (
                    <div className="flex flex-col items-center justify-center text-[var(--color-text-muted)]/40">
                        {getCategoryIcon(partner)}
                    </div>
                )}

                {/* Category Badge */}
                <div className="absolute top-3 right-3 bg-[var(--color-accent)]/10 text-[var(--color-accent)] border border-[var(--color-accent)]/20 px-2 py-0.5 rounded-full text-xs font-medium">
                    {partner.category || getPartnerTypeLabel(partner.type)}
                </div>
            </div>

            <CardContent className="p-5 space-y-4">
                <div>
                    <h3 className="text-lg font-bold text-[var(--color-text-primary)] group-hover:text-[var(--color-accent)] transition-colors">
                        {partner.name}
                    </h3>
                    {partner.description && (
                        <p className="text-xs text-[var(--color-text-muted)] line-clamp-2 mt-1">
                            {partner.description}
                        </p>
                    )}
                </div>

                <div className="space-y-2 text-sm text-[var(--color-text-secondary)]">
                    <div className="flex items-start gap-2">
                        <MapPin className="w-4 h-4 mt-0.5 text-[var(--color-text-muted)] shrink-0" />
                        <span className="line-clamp-1">{partner.city ? `${partner.city}, ${partner.region || ''}` : partner.address}</span>
                    </div>
                    {partner.hotlineNumber && (
                        <div className="flex items-center gap-2">
                            <Phone className="w-4 h-4 text-[var(--color-text-muted)] shrink-0" />
                            <span>{partner.hotlineNumber}</span>
                        </div>
                    )}
                </div>

                <div className="pt-4 mt-4 border-t border-[var(--color-border)]/50 flex items-center justify-between">
                    {activeOffersCount > 0 ? (
                        <div className="flex items-center gap-1.5 text-emerald-500 font-medium text-xs bg-emerald-500/10 px-2 py-1 rounded-full">
                            <Tag className="w-3.5 h-3.5" />
                            <span>{activeOffersCount} {t('services.health.offers', 'Offers Available')}</span>
                        </div>
                    ) : (
                        <span className="text-xs text-[var(--color-text-muted)]">No active offers</span>
                    )}

                    <Button
                        size="sm"
                        variant="outline"
                        className="ml-auto hover:bg-[var(--color-accent)] hover:text-white border-[var(--color-accent)]/30 text-[var(--color-accent)]"
                        onClick={() => onViewOffers(partner)}
                    >
                        {t('common.viewDetails', 'View Details')}
                    </Button>
                </div>
            </CardContent>
        </Card>
    );
};
