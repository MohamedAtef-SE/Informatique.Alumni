import { Tag, MapPin, Phone, Globe, Copy, Check, Pill, Building2, FlaskConical, Stethoscope, Eye, LayoutGrid } from "lucide-react";
import { useTranslation } from "react-i18next";
import { useState } from "react";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "../../ui/Dialog";
import { type MedicalPartnerDto, MedicalPartnerType } from "../../../types/health";
import { toast } from "sonner";
import clsx from "clsx";

interface OfferDetailsModalProps {
    isOpen: boolean;
    onClose: () => void;
    partner: MedicalPartnerDto | null;
}

export const OfferDetailsModal = ({ isOpen, onClose, partner }: OfferDetailsModalProps) => {
    const { t } = useTranslation();
    const [copiedCode, setCopiedCode] = useState<string | null>(null);
    const [imgError, setImgError] = useState(false);

    if (!partner) return null;

    const copyToClipboard = (code: string) => {
        navigator.clipboard.writeText(code);
        setCopiedCode(code);
        toast.success(t('common.copied', 'Code copied to clipboard'));
        setTimeout(() => setCopiedCode(null), 2000);
    };

    const getCategoryIcon = () => {
        const iconProps = { className: "w-10 h-10" };
        if (partner.type === MedicalPartnerType.Pharmacy) return <Pill {...iconProps} />;
        if (partner.type === MedicalPartnerType.Hospital) return <Building2 {...iconProps} />;
        if (partner.type === MedicalPartnerType.Lab) return <FlaskConical {...iconProps} />;
        if (partner.category?.toLowerCase().includes('dental')) return <Stethoscope {...iconProps} />;
        if (partner.category?.toLowerCase().includes('optic')) return <Eye {...iconProps} />;

        return <LayoutGrid {...iconProps} />;
    };

    return (
        <Dialog open={isOpen} onOpenChange={onClose}>
            {/* 
                CRITICAL FIX for UI Contrast:
                - Enforcing bg-white context to break out of any parent dark theme issues.
                - Using standard slate-900 for text to ensuring max readability.
                - Removed reliance on var(--color-bg-card) which was causing the dark-on-dark issue.
            */}
            <DialogContent className="sm:max-w-[750px] p-0 gap-0 bg-white border border-slate-200 shadow-2xl overflow-hidden block">

                {/* Header Banner */}
                <div className="relative h-32 w-full bg-gradient-to-r from-slate-50 to-slate-100 border-b border-slate-200">
                    <div className="absolute top-0 right-0 p-4 opacity-5">
                        {getCategoryIcon()}
                    </div>
                </div>

                {/* Main Content Container with negative margin for overlap effect */}
                <div className="px-8 pb-8 -mt-16 relative z-10">

                    {/* Partner Identity Card */}
                    <div className="flex flex-col md:flex-row gap-6 items-start">
                        {/* Logo Box */}
                        <div className="h-32 w-32 shrink-0 rounded-xl bg-white shadow-lg p-2 border border-slate-100 flex items-center justify-center overflow-hidden">
                            {!imgError && partner.logoUrl ? (
                                <img
                                    src={partner.logoUrl}
                                    alt={partner.name}
                                    className="h-full w-full object-contain"
                                    onError={() => setImgError(true)}
                                />
                            ) : (
                                <div className="text-blue-600 opacity-90">
                                    {getCategoryIcon()}
                                </div>
                            )}
                        </div>

                        {/* Title & Badges */}
                        <div className="pt-16 md:pt-20 flex-1">
                            <DialogHeader>
                                <DialogTitle className="text-2xl md:text-3xl font-bold text-slate-900 mb-2 leading-tight">
                                    {partner.name}
                                </DialogTitle>
                                <div className="flex flex-wrap items-center gap-2 text-sm text-slate-500">
                                    <span className="bg-blue-50 text-blue-700 px-3 py-1 rounded-full font-semibold text-xs border border-blue-100 uppercase tracking-wide">
                                        {partner.category || 'Medical Partner'}
                                    </span>
                                    {partner.city && (
                                        <div className="flex items-center gap-1 text-slate-600">
                                            <MapPin className="w-3 h-3" />
                                            <span>{partner.city}</span>
                                        </div>
                                    )}
                                </div>
                            </DialogHeader>
                        </div>
                    </div>

                    {/* Content Grid */}
                    <div className="grid grid-cols-1 md:grid-cols-12 gap-8 mt-8">

                        {/* Left Column: Contact Details (4 cols) */}
                        <div className="md:col-span-4 space-y-6">
                            <div>
                                <h4 className="font-bold text-slate-900 border-b-2 border-slate-100 pb-2 mb-4 uppercase text-xs tracking-wider flex items-center gap-2">
                                    <Building2 className="w-4 h-4 text-slate-400" />
                                    {t('common.contact_info', 'Contact Info')}
                                </h4>

                                <ul className="space-y-4 text-sm">
                                    <li className="flex gap-3 items-start group">
                                        <div className="mt-0.5 bg-slate-100 p-1.5 rounded-md group-hover:bg-blue-50 transition-colors">
                                            <MapPin className="w-4 h-4 text-slate-500 group-hover:text-blue-600" />
                                        </div>
                                        <div>
                                            <p className="font-semibold text-slate-700 mb-0.5 text-xs uppercase text-slate-400">Address</p>
                                            <p className="text-slate-600 leading-snug">
                                                {partner.city ? `${partner.city}, ${partner.region}` : partner.address}
                                            </p>
                                        </div>
                                    </li>

                                    {partner.hotlineNumber && (
                                        <li className="flex gap-3 items-start group">
                                            <div className="mt-0.5 bg-slate-100 p-1.5 rounded-md group-hover:bg-blue-50 transition-colors">
                                                <Phone className="w-4 h-4 text-slate-500 group-hover:text-blue-600" />
                                            </div>
                                            <div>
                                                <p className="font-semibold text-slate-700 mb-0.5 text-xs uppercase text-slate-400">Hotline</p>
                                                <p className="text-slate-900 font-bold font-mono tracking-tight text-lg">
                                                    {partner.hotlineNumber}
                                                </p>
                                            </div>
                                        </li>
                                    )}

                                    {partner.website && (
                                        <li className="flex gap-3 items-start group">
                                            <div className="mt-0.5 bg-slate-100 p-1.5 rounded-md group-hover:bg-blue-50 transition-colors">
                                                <Globe className="w-4 h-4 text-slate-500 group-hover:text-blue-600" />
                                            </div>
                                            <div className="overflow-hidden">
                                                <p className="font-semibold text-slate-700 mb-0.5 text-xs uppercase text-slate-400">Website</p>
                                                <a
                                                    href={partner.website}
                                                    target="_blank"
                                                    rel="noopener noreferrer"
                                                    className="text-blue-600 hover:text-blue-800 font-medium truncate block transition-colors"
                                                >
                                                    {partner.website.replace(/^https?:\/\//, '')}
                                                </a>
                                            </div>
                                        </li>
                                    )}
                                </ul>
                            </div>
                        </div>

                        {/* Right Column: Offers (8 cols) */}
                        <div className="md:col-span-8">
                            <h4 className="font-bold text-slate-900 border-b-2 border-slate-100 pb-2 mb-4 uppercase text-xs tracking-wider flex items-center gap-2">
                                <Tag className="w-4 h-4 text-emerald-600" />
                                {t('services.health.activeOffers', 'Active Offers')}
                            </h4>

                            {partner.offers && partner.offers.length > 0 ? (
                                <div className="space-y-4">
                                    {partner.offers.filter(o => o.isActive).map((offer) => (
                                        <div
                                            key={offer.id}
                                            className="group relative bg-slate-50 rounded-xl border border-slate-200 p-0 overflow-hidden hover:shadow-md hover:border-blue-300 transition-all duration-300"
                                        >
                                            {/* Ribbon/Accent on left */}
                                            <div className="absolute left-0 top-0 bottom-0 w-1.5 bg-blue-500 group-hover:bg-blue-600 transition-colors"></div>

                                            <div className="p-5 pl-7 flex flex-col sm:flex-row gap-4 justify-between items-start sm:items-center">
                                                <div className="flex-1">
                                                    <h5 className="font-bold text-slate-800 text-lg mb-1 group-hover:text-blue-700 transition-colors">
                                                        {offer.title}
                                                    </h5>
                                                    <p className="text-slate-600 text-sm leading-relaxed">
                                                        {offer.description}
                                                    </p>
                                                </div>

                                                {offer.discountCode && (
                                                    <div className="flex items-center gap-2 bg-white p-1.5 pr-2 rounded-lg border border-slate-200 shadow-sm shrink-0">
                                                        <div className="bg-slate-100 text-slate-500 px-2 py-1.5 rounded text-[10px] font-bold uppercase tracking-wider">
                                                            Code
                                                        </div>
                                                        <div className="text-slate-900 font-mono font-bold text-base px-1">
                                                            {offer.discountCode}
                                                        </div>
                                                        <div className="h-6 w-px bg-slate-200 mx-1"></div>
                                                        <button
                                                            onClick={() => copyToClipboard(offer.discountCode!)}
                                                            className={clsx(
                                                                "p-1.5 rounded-md transition-all hover:bg-slate-100 active:scale-95",
                                                                copiedCode === offer.discountCode ? "text-emerald-600" : "text-slate-400 hover:text-blue-600"
                                                            )}
                                                            title="Copy Code"
                                                        >
                                                            {copiedCode === offer.discountCode ? (
                                                                <Check className="w-4 h-4" />
                                                            ) : (
                                                                <Copy className="w-4 h-4" />
                                                            )}
                                                        </button>
                                                    </div>
                                                )}
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            ) : (
                                <div className="flex flex-col items-center justify-center py-12 px-4 text-center text-slate-400 bg-slate-50 rounded-xl border-2 border-dashed border-slate-200">
                                    <div className="bg-white p-4 rounded-full shadow-sm mb-3">
                                        <Tag className="w-8 h-8 text-slate-300" />
                                    </div>
                                    <p className="font-medium text-slate-600">No active offers available</p>
                                    <p className="text-xs mt-1">Check back later for new deals from {partner.name}</p>
                                </div>
                            )}
                        </div>
                    </div>
                </div>
            </DialogContent>
        </Dialog>
    );
};
