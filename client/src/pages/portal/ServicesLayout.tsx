import { useState } from 'react';
import { MembershipGuard } from '../../components/common/MembershipGuard';
import { toast } from 'sonner';
import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { servicesAppService } from '../../services/servicesService';
import { alumniService } from '../../services/alumniService';
import { fileService } from '../../services/fileService';
import { Newspaper, CreditCard, Award, Gift, QrCode, FileBadge, Building2, HeartPulse, Plus, Upload, Loader2 } from 'lucide-react';
import clsx from 'clsx';
import { useAuth } from 'react-oidc-context';
import ErrorModal from '../../components/common/ErrorModal';
import { Card, CardContent } from '../../components/ui/Card';
import { Button } from '../../components/ui/Button';

import FeaturedNews from '../../components/portal/news/FeaturedNews';
import NewsCard from '../../components/portal/news/NewsCard';
import ArticleView from '../../components/portal/news/ArticleView';

import { PartnerFilterBar } from '../../components/portal/health/PartnerFilterBar';
import { MedicalPartnerCard } from '../../components/portal/health/MedicalPartnerCard';
import { HealthStats } from '../../components/portal/health/HealthStats';
import { useDebounce } from '../../hooks/useDebounce';
import type { MedicalPartnerDto } from '../../types/health';
import { OfferDetailsModal } from '../../components/portal/health/OfferDetailsModal';

const ServicesLayout = () => {
    const [activeTab, setActiveTab] = useState<'news' | 'benefits' | 'membership' | 'certificates' | 'syndicates' | 'health'>('news');
    const [selectedNewsId, setSelectedNewsId] = useState<string | null>(null);
    const auth = useAuth();
    const { t } = useTranslation();

    // Queries
    const newsQuery = useQuery({ queryKey: ['news'], queryFn: () => servicesAppService.getPosts(), enabled: activeTab === 'news' });
    const featuredNewsQuery = useQuery({ queryKey: ['news-featured'], queryFn: () => servicesAppService.getPosts({ isFeatured: true, maxResultCount: 1 }), enabled: activeTab === 'news' });
    const grantsQuery = useQuery({ queryKey: ['grants'], queryFn: servicesAppService.getGrants, enabled: activeTab === 'benefits' });
    const discountsQuery = useQuery({ queryKey: ['discounts'], queryFn: servicesAppService.getDiscounts, enabled: activeTab === 'benefits' });
    const cardQuery = useQuery({ queryKey: ['my-card'], queryFn: servicesAppService.getCard, enabled: activeTab === 'membership' });

    // Membership Action States
    const [showRenewModal, setShowRenewModal] = useState(false);
    const [showLostModal, setShowLostModal] = useState(false);

    // ... other queries

    const featuredPost = featuredNewsQuery.data?.items?.[0];
    const newsItems = newsQuery.data?.items || [];
    // Filter out featured post from regular list if it exists to avoid duplication
    const regularNews = featuredPost ? newsItems.filter(p => p.id !== featuredPost.id) : newsItems;

    return (
        <div className="space-y-8 animate-fade-in">
            {/* ... Header & Tabs ... */}
            <div>
                <h1 className="text-3xl font-heading font-bold text-[var(--color-text-primary)]">{t('services.title')}</h1>
                <p className="text-[var(--color-text-secondary)] mt-1">{t('services.subtitle')}</p>
            </div>

            {/* Tabs */}
            <div className="flex gap-4 border-b border-[var(--color-border)] overflow-x-auto pb-1">
                {[
                    { id: 'news', icon: Newspaper, label: t('services.tabs.news') },
                    { id: 'benefits', icon: Gift, label: t('services.tabs.benefits') },
                    { id: 'membership', icon: CreditCard, label: t('services.tabs.membership') },
                    { id: 'certificates', icon: FileBadge, label: t('services.tabs.certificates') },
                    { id: 'syndicates', icon: Building2, label: t('services.tabs.syndicates') },
                    { id: 'health', icon: HeartPulse, label: t('services.tabs.health') },
                ].map((tab) => (
                    <button
                        key={tab.id}
                        onClick={() => setActiveTab(tab.id as any)}
                        className={clsx(
                            "px-4 py-3 flex items-center gap-2 border-b-2 transition-all whitespace-nowrap text-sm font-medium",
                            activeTab === tab.id
                                ? "border-[var(--color-accent)] text-[var(--color-accent)] bg-[var(--color-accent-light)]/20 rounded-t-lg"
                                : "border-transparent text-[var(--color-text-secondary)] hover:text-[var(--color-text-primary)] hover:bg-slate-100 rounded-t-lg"
                        )}
                    >
                        <tab.icon className="w-4 h-4" /> {tab.label}
                    </button>
                ))}
            </div>

            {/* Content */}
            <div className="min-h-[400px]">
                {activeTab === 'news' && (
                    <div className="animate-slide-up space-y-12">
                        {/* Featured News Hero */}
                        {featuredPost && (
                            <MembershipGuard>
                                <FeaturedNews
                                    post={featuredPost}
                                    onClick={() => setSelectedNewsId(featuredPost.id)}
                                />
                            </MembershipGuard>
                        )}

                        {/* Recent News Grid */}
                        <div>
                            {featuredPost && <h3 className="text-2xl font-bold text-[var(--color-text-primary)] mb-6">Recent News</h3>}

                            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
                                {regularNews.map(post => (
                                    <MembershipGuard key={post.id}>
                                        <NewsCard
                                            post={post}
                                            onClick={() => setSelectedNewsId(post.id)}
                                        />
                                    </MembershipGuard>
                                ))}
                            </div>
                        </div>

                        {newsItems.length === 0 && !featuredNewsQuery.isLoading && (
                            <div className="col-span-full py-20 text-center text-[var(--color-text-muted)] bg-slate-50 rounded-xl border border-dashed border-[var(--color-border)]">
                                {t('services.news.empty')}
                            </div>
                        )}
                    </div>
                )}

                {/* Article View Modal */}
                {activeTab === 'news' && selectedNewsId && (
                    <ArticleView id={selectedNewsId} onClose={() => setSelectedNewsId(null)} />
                )}

                {activeTab === 'benefits' && (
                    <div className="space-y-12 animate-slide-up">
                        {/* Grants */}
                        <div>
                            <h2 className="text-xl font-bold text-[var(--color-text-primary)] mb-4 flex items-center gap-2"><Award className="w-5 h-5 text-[var(--color-accent)]" /> {t('services.benefits.grants_title')}</h2>
                            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                                {grantsQuery.data?.items.map(grant => (
                                    <Card key={grant.id} variant="default" className="text-center border-[var(--color-border)] hover:shadow-md transition-all">
                                        <CardContent className="p-6">
                                            <div className="text-3xl font-bold text-[var(--color-accent)] mb-2">{grant.percentage}%</div>
                                            <h3 className="font-bold text-[var(--color-text-primary)]">{grant.nameEn}</h3>
                                            <p className="text-xs text-[var(--color-text-muted)] mt-1">{grant.type}</p>
                                        </CardContent>
                                    </Card>
                                ))}
                            </div>
                        </div>

                        {/* Discounts */}
                        <div>
                            <h2 className="text-xl font-bold text-[var(--color-text-primary)] mb-4 flex items-center gap-2"><Gift className="w-5 h-5 text-[var(--color-accent)]" /> {t('services.benefits.discounts_title')}</h2>
                            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                                {discountsQuery.data?.items.map(discount => (
                                    <Card key={discount.id} variant="default" className="relative overflow-hidden group border-[var(--color-border)] hover:border-[var(--color-accent)]/30 hover:shadow-lg transition-all">
                                        <div className="absolute top-0 right-0 bg-[var(--color-error)] text-white text-xs font-bold px-2 py-1 rounded-bl shadow-sm">
                                            -{discount.discountPercentage}%
                                        </div>
                                        <CardContent className="p-6">
                                            <h3 className="font-bold text-[var(--color-text-primary)] text-lg mb-1">{discount.providerName}</h3>
                                            <p className="text-sm text-[var(--color-accent)] mb-2 font-medium">{discount.title}</p>
                                            <p className="text-xs text-[var(--color-text-secondary)] mb-4 leading-relaxed">{discount.description}</p>
                                            {discount.promoCode && (
                                                <MembershipGuard>
                                                    <div
                                                        className="bg-slate-100 border border-slate-200 p-2 rounded text-center font-mono text-sm text-[var(--color-text-primary)] cursor-pointer hover:bg-slate-200 transition-colors relative overflow-hidden group/code"
                                                        onClick={() => {
                                                            navigator.clipboard.writeText(discount.promoCode);
                                                            toast.success(t('common.copied', 'Copied to clipboard'));
                                                        }}
                                                    >
                                                        <span className="blur-[4px] group-hover/code:blur-0 transition-all duration-300 select-none">
                                                            {discount.promoCode}
                                                        </span>
                                                        <div className="absolute inset-0 flex items-center justify-center text-xs text-slate-500 font-medium group-hover/code:hidden">
                                                            Click to View
                                                        </div>
                                                    </div>
                                                </MembershipGuard>
                                            )}
                                        </CardContent>
                                    </Card>
                                ))}
                            </div>
                        </div>
                    </div>
                )}

                {activeTab === 'membership' && (
                    <div className="max-w-md mx-auto animate-fade-in">
                        <div className="relative aspect-[1.58] rounded-2xl shadow-2xl overflow-hidden transform hover:scale-[1.02] transition-transform duration-500">
                            {/* Card Background - Keeping dark/premium look for the physical card */}
                            <div className="absolute inset-0 bg-gradient-to-br from-[#1e3a8a] to-[#0f172a] z-0"></div>
                            <div className="absolute inset-0 bg-[url('/noise.png')] opacity-20 z-0"></div>

                            {/* Card Content */}
                            <div className="relative z-10 p-6 flex flex-col h-full justify-between">
                                <div className="flex justify-between items-start">
                                    <div>
                                        <h3 className="font-bold text-white text-lg">{t('services.membership.card_title')}</h3>
                                        <p className="text-xs text-blue-200 uppercase tracking-widest">{t('services.membership.card_subtitle')}</p>
                                    </div>
                                    <QrCode className="w-12 h-12 text-white/90" />
                                </div>

                                <div className="flex items-end gap-4">
                                    <div className="w-20 h-20 bg-slate-200 rounded-lg overflow-hidden border-2 border-white/30 shadow-inner relative">
                                        {cardQuery.data?.alumniPhotoUrl ? (
                                            <img
                                                src={alumniService.getPhotoUrl(cardQuery.data.alumniPhotoUrl)}
                                                alt="Member"
                                                className="w-full h-full object-cover"
                                                onError={(e) => {
                                                    // Fallback if image fails to load
                                                    e.currentTarget.style.display = 'none';
                                                    e.currentTarget.nextElementSibling?.classList.remove('hidden');
                                                }}
                                            />
                                        ) : null}
                                        {/* Photo Placeholder / Fallback */}
                                        <div className={clsx(
                                            "w-full h-full bg-slate-300 flex items-center justify-center text-xs text-slate-500 font-medium absolute inset-0",
                                            cardQuery.data?.alumniPhotoUrl ? "hidden" : ""
                                        )}>
                                            {t('services.membership.photo_placeholder')}
                                        </div>
                                    </div>
                                    <div className="flex-1">
                                        <p className="text-xs text-blue-200 mb-0.5">{t('services.membership.name_label')}</p>
                                        <h4 className="font-bold text-white text-lg uppercase truncate drop-shadow-sm">{cardQuery.data?.alumniName || auth.user?.profile.name || 'Member Name'}</h4>
                                        <div className="flex justify-between mt-2">
                                            <div>
                                                <p className="text-[10px] text-blue-200">{t('services.membership.id_label')}</p>
                                                <p className="text-sm text-white font-mono">{cardQuery.data?.alumniNationalId || 'PENDING'}</p>
                                            </div>
                                            <div className="text-right flex flex-col items-end gap-1">
                                                <div>
                                                    <p className="text-[10px] text-blue-200 uppercase tracking-wider">{t('services.membership.degree_label')}</p>
                                                    <p className="text-sm text-white font-mono font-bold leading-tight">{cardQuery.data?.degree || 'N/A'}</p>
                                                </div>
                                                <div className="mt-1">
                                                    <p className="text-[9px] text-blue-200 uppercase tracking-wider">{t('services.membership.college_label', 'College')}</p>
                                                    <p className="text-xs text-white/90 font-mono leading-tight truncate max-w-[140px]" title={cardQuery.data?.collegeName}>{cardQuery.data?.collegeName || 'N/A'}</p>
                                                </div>
                                                <div>
                                                    <p className="text-[9px] text-blue-200 uppercase tracking-wider">{t('services.membership.year_label', 'Class Of')}</p>
                                                    <p className="text-xs text-white/90 font-mono leading-tight">{cardQuery.data?.gradYear || 'N/A'}</p>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div className="mt-8 text-center space-y-4">
                            <Button onClick={() => setShowRenewModal(true)} className="w-full shadow-lg shadow-blue-500/20 py-6 text-lg">{t('services.membership.renew_btn')}</Button>
                            <button onClick={() => setShowLostModal(true)} className="px-4 py-2 w-full rounded-lg hover:bg-slate-100 text-[var(--color-text-secondary)] text-sm transition-colors">{t('services.membership.lost_btn')}</button>
                        </div>

                        {/* Renew Membership Modal */}
                        {showRenewModal && (
                            <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4 animate-fade-in">
                                <div className="bg-white border border-[var(--color-border)] rounded-xl p-6 w-full max-w-md space-y-4 shadow-2xl">
                                    <h3 className="text-xl font-bold text-[var(--color-text-primary)]">{t('services.membership.renew_title', 'Renew Your Membership')}</h3>
                                    <p className="text-[var(--color-text-secondary)]">
                                        {t('services.membership.renew_desc', 'To renew your alumni membership, please visit your nearest branch office or contact us at support@alumni.edu to complete the renewal process and payment.')}
                                    </p>
                                    <div className="flex gap-3 pt-4">
                                        <button
                                            onClick={() => setShowRenewModal(false)}
                                            className="flex-1 px-4 py-2 rounded-lg bg-slate-100 hover:bg-slate-200 text-[var(--color-text-secondary)] transition-colors font-medium"
                                        >
                                            {t('common.close', 'Close')}
                                        </button>
                                    </div>
                                </div>
                            </div>
                        )}

                        {/* Report Lost Card Modal */}
                        {showLostModal && (
                            <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4 animate-fade-in">
                                <div className="bg-white border border-[var(--color-border)] rounded-xl p-6 w-full max-w-md space-y-4 shadow-2xl">
                                    <h3 className="text-xl font-bold text-[var(--color-text-primary)]">{t('services.membership.lost_title', 'Report Lost Card')}</h3>
                                    <p className="text-[var(--color-text-secondary)]">
                                        {t('services.membership.lost_desc', 'If you have lost your membership card, please visit the nearest branch office with a valid ID to request a replacement. A small fee may apply.')}
                                    </p>
                                    <div className="flex gap-3 pt-4">
                                        <button
                                            onClick={() => setShowLostModal(false)}
                                            className="flex-1 px-4 py-2 rounded-lg bg-slate-100 hover:bg-slate-200 text-[var(--color-text-secondary)] transition-colors font-medium"
                                        >
                                            {t('common.close', 'Close')}
                                        </button>
                                    </div>
                                </div>
                            </div>
                        )}
                    </div>
                )}

                {activeTab === 'certificates' && (
                    <CertificatesSection />
                )}

                {activeTab === 'syndicates' && (
                    <SyndicatesSection />
                )}

                {activeTab === 'health' && (
                    <HealthSection />
                )}
            </div>
        </div>
    );
};

// Sub-components
function CertificatesSection() {
    const queryClient = useQueryClient();
    const { t, i18n } = useTranslation();
    const { data } = useQuery({ queryKey: ['my-certificates'], queryFn: servicesAppService.getCertificates });
    const { data: definitions } = useQuery({ queryKey: ['certificate-definitions'], queryFn: servicesAppService.getCertificateDefinitions });
    const { data: branches } = useQuery({ queryKey: ['branches'], queryFn: servicesAppService.getBranches });

    const [isModalOpen, setIsModalOpen] = useState(false);
    const [selectedDefId, setSelectedDefId] = useState<string>('');
    const [selectedBranchId, setSelectedBranchId] = useState<string>('');
    const [language, setLanguage] = useState(2); // 2: English, 1: Arabic (Backend Enums)
    const [deliveryMethod, setDeliveryMethod] = useState(1); // 1: Office Pickup, 2: Home Delivery (Backend Enums)
    const [requestAddress, setRequestAddress] = useState('');
    const [selectedFile, setSelectedFile] = useState<File | null>(null);
    const [isUploading, setIsUploading] = useState(false);

    // Error Modal State
    const [errorState, setErrorState] = useState<{ isOpen: boolean; title: string; message: string }>({
        isOpen: false,
        title: '',
        message: ''
    });

    const requestMutation = useMutation({
        mutationFn: servicesAppService.requestCertificate,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['my-certificates'] });
            setIsModalOpen(false);
            toast.success(t('services.certificates.success'));
        },
        onError: (err: any) => {
            console.error("Certificate Request Failed");
            const errorBody = err.response?.data?.error;
            let errorCode = errorBody?.code;
            const errorMessage = errorBody?.message || err.message || "";

            if (!errorCode && errorMessage.includes("Alumni:Certificate:")) {
                const match = errorMessage.match(/(Alumni:Certificate:\d{3})/);
                if (match) {
                    errorCode = match[1];
                }
            }

            let title = t('common.error');
            let userMessage = t('services.certificates.error_generic');

            if (errorCode === "Alumni:Certificate:003" || errorMessage.includes("MembershipExpired")) {
                title = t('services.certificates.error_membership_title');
                userMessage = t('services.certificates.error_membership_msg');
            } else if (errorCode === "Alumni:Certificate:005") {
                title = t('services.certificates.error_address_title');
                userMessage = t('services.certificates.error_address_msg');
            } else if (errorCode === "Alumni:Certificate:006") {
                title = t('services.certificates.error_branch_title');
                userMessage = t('services.certificates.error_branch_msg');
            } else {
                userMessage = errorMessage;
            }

            setErrorState({
                isOpen: true,
                title: title,
                message: userMessage
            });
        }
    });



    const handleRequest = async () => {
        if (!selectedDefId) {
            setErrorState({ isOpen: true, title: t('services.certificates.missing_info'), message: t('services.certificates.select_type_error') });
            return;
        }

        if (deliveryMethod === 1 && !selectedBranchId) {
            setErrorState({ isOpen: true, title: t('services.certificates.missing_info'), message: t('services.certificates.select_branch_error') });
            return;
        }

        if (deliveryMethod === 2 && !requestAddress.trim()) {
            setErrorState({ isOpen: true, title: t('services.certificates.missing_info'), message: t('services.certificates.enter_address_error') });
            return;
        }

        let attachmentUrl = null;
        if (selectedFile) {
            try {
                setIsUploading(true);
                attachmentUrl = await fileService.upload(selectedFile);
            } catch (error) {
                console.error("File upload failed", error);
                setIsUploading(false);
                setErrorState({ isOpen: true, title: t('common.error'), message: t('services.certificates.upload_error', 'File upload failed. Please try again.') });
                return;
            } finally {
                setIsUploading(false);
            }
        }

        const payload = {
            items: [
                {
                    certificateDefinitionId: selectedDefId,
                    language: language,
                    attachmentUrl: attachmentUrl
                }
            ],
            deliveryMethod: deliveryMethod,
            deliveryAddress: deliveryMethod === 2 ? requestAddress : null,
            targetBranchId: deliveryMethod === 1 ? selectedBranchId : null,
            userNotes: ""
        };

        requestMutation.mutate(payload);
    };

    const getStatusInfo = (status: number) => {
        switch (status) {
            case 1: // Draft
            case 2: // PendingPayment
            case 3: // Processing
                return { label: t('services.certificates.status.pending'), className: "bg-amber-100 text-amber-700 border-amber-200" };
            case 4: // ReadyForPickup
            case 5: // OutForDelivery
            case 6: // Delivered (Using ready style for now or add new)
                return { label: t('services.certificates.status.ready'), className: "bg-emerald-100 text-emerald-700 border-emerald-200" };
            case 7: // Rejected
                return { label: t('services.certificates.status.rejected'), className: "bg-red-100 text-red-700 border-red-200" };
            default:
                return { label: t('services.certificates.status.pending'), className: "bg-gray-100 text-gray-700 border-gray-200" };
        }
    };

    return (
        <div className="space-y-6 animate-slide-up">
            <ErrorModal
                isOpen={errorState.isOpen}
                title={errorState.title}
                message={errorState.message}
                onClose={() => setErrorState(prev => ({ ...prev, isOpen: false }))}
            />

            <h2 className="text-xl font-bold text-[var(--color-text-primary)] flex items-center gap-2"><FileBadge className="w-5 h-5 text-[var(--color-accent)]" /> {t('services.certificates.my_certificates')}</h2>
            {definitions?.isEligible !== false ? (
                <Button
                    onClick={() => setIsModalOpen(true)}
                    className="flex items-center gap-2 shadow-sm"
                >
                    <Plus className="w-4 h-4" /> {t('services.certificates.request_new')}
                </Button>
            ) : null}


            {/* Ineligibility Banner */}
            {
                definitions?.isEligible === false && (
                    <div className="bg-amber-50 border border-amber-200 rounded-xl p-6 flex flex-col md:flex-row items-center gap-4 text-center md:text-left animate-fade-in">
                        <div className="bg-amber-100 p-3 rounded-full">
                            <CreditCard className="w-6 h-6 text-amber-600" />
                        </div>
                        <div className="flex-1">
                            <h3 className="font-bold text-amber-800 text-lg">{t('services.membership.required_title', 'Active Membership Required')}</h3>
                            <p className="text-amber-700/80 text-sm mt-1">
                                {definitions.ineligibilityReason || t('services.membership.required_desc', 'You need an active membership to request certificates. Please renew your membership to access this service.')}
                            </p>
                        </div>
                        <Button
                            variant="outline"
                            className="border-amber-300 text-amber-800 hover:bg-amber-100"
                            onClick={() => document.getElementById('tab-membership')?.click()} // Hacky navigation or just let them find it
                        >
                            {t('services.membership.renew_btn')}
                        </Button>
                    </div>
                )
            }

            {/* Request Modal */}
            {
                isModalOpen && (
                    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4 animate-fade-in">
                        <div className="bg-white border border-[var(--color-border)] rounded-xl p-6 w-full max-w-md space-y-4 shadow-2xl">
                            <h3 className="text-xl font-bold text-[var(--color-text-primary)]">{t('services.certificates.modal_title')}</h3>

                            <div className="space-y-3">
                                <div>
                                    <label className="text-xs text-[var(--color-text-secondary)] block mb-1">{t('services.certificates.type_label')}</label>
                                    <select
                                        className="w-full bg-slate-50 border border-[var(--color-border)] rounded-lg px-3 py-2 text-[var(--color-text-primary)] focus:border-[var(--color-accent)] outline-none"
                                        value={selectedDefId}
                                        onChange={(e) => setSelectedDefId(e.target.value)}
                                    >
                                        <option value="">{t('services.certificates.select_type')}</option>
                                        {definitions?.items?.map((def: any) => (
                                            <option key={def.id} value={def.id}>{def.nameEn} ({def.fee} {t('common.currency')})</option>
                                        ))}
                                    </select>
                                </div>

                                <div>
                                    <label className="text-xs text-[var(--color-text-secondary)] block mb-1">{t('services.certificates.language_label')}</label>
                                    <div className="flex gap-4">
                                        <label className="flex items-center gap-2 text-[var(--color-text-primary)] cursor-pointer text-sm">
                                            <input type="radio" name="lang" checked={language === 2} onChange={() => setLanguage(2)} className="accent-[var(--color-accent)]" /> {t('services.certificates.lang_en')}
                                        </label>
                                        <label className="flex items-center gap-2 text-[var(--color-text-primary)] cursor-pointer text-sm">
                                            <input type="radio" name="lang" checked={language === 1} onChange={() => setLanguage(1)} className="accent-[var(--color-accent)]" /> {t('services.certificates.lang_ar')}
                                        </label>
                                    </div>
                                </div>

                                <div>
                                    <label className="text-xs text-[var(--color-text-secondary)] block mb-1">{t('services.certificates.delivery_label')}</label>
                                    <div className="flex gap-4">
                                        <label className="flex items-center gap-2 text-[var(--color-text-primary)] cursor-pointer text-sm">
                                            <input type="radio" name="delivery" checked={deliveryMethod === 1} onChange={() => setDeliveryMethod(1)} className="accent-[var(--color-accent)]" /> {t('services.certificates.pickup')}
                                        </label>
                                        <label className="flex items-center gap-2 text-[var(--color-text-primary)] cursor-pointer text-sm">
                                            <input type="radio" name="delivery" checked={deliveryMethod === 2} onChange={() => setDeliveryMethod(2)} className="accent-[var(--color-accent)]" /> {t('services.certificates.home_delivery')}
                                        </label>
                                    </div>
                                </div>

                                {deliveryMethod === 1 && (
                                    <div>
                                        <label className="text-xs text-[var(--color-text-secondary)] block mb-1">{t('services.certificates.branch_label')}</label>
                                        <select
                                            className="w-full bg-slate-50 border border-[var(--color-border)] rounded-lg px-3 py-2 text-[var(--color-text-primary)] focus:border-[var(--color-accent)] outline-none"
                                            value={selectedBranchId}
                                            onChange={(e) => setSelectedBranchId(e.target.value)}
                                        >
                                            <option value="">{t('services.certificates.select_branch')}</option>
                                            {branches?.map((branch: any) => (
                                                <option key={branch.id} value={branch.id}>{branch.name}</option>
                                            ))}
                                        </select>
                                    </div>
                                )}

                                {deliveryMethod === 2 && (
                                    <div>
                                        <label className="text-xs text-[var(--color-text-secondary)] block mb-1">{t('services.certificates.address_label')}</label>
                                        <textarea
                                            className="w-full bg-slate-50 border border-[var(--color-border)] rounded-lg px-3 py-2 text-[var(--color-text-primary)] focus:border-[var(--color-accent)] outline-none resize-none h-24"
                                            placeholder={t('services.certificates.address_placeholder')}
                                            value={requestAddress}
                                            onChange={(e) => setRequestAddress(e.target.value)}
                                        />
                                    </div>

                                )}

                                <div>
                                    <label className="text-xs text-[var(--color-text-secondary)] block mb-1">{t('services.certificates.attachment_label', 'Supporting Document (ID/Passport - Optional)')}</label>
                                    <div className="border border-dashed border-[var(--color-border)] rounded-lg p-4 text-center hover:bg-slate-50 transition-colors relative">
                                        <input
                                            type="file"
                                            accept=".pdf,.jpg,.jpeg,.png"
                                            className="absolute inset-0 w-full h-full opacity-0 cursor-pointer"
                                            onChange={(e) => {
                                                if (e.target.files && e.target.files[0]) {
                                                    setSelectedFile(e.target.files[0]);
                                                }
                                            }}
                                        />
                                        <div className="flex flex-col items-center gap-1 pointer-events-none">
                                            <Upload className="w-5 h-5 text-[var(--color-accent)]" />
                                            <span className="text-sm text-[var(--color-text-primary)] font-medium">
                                                {selectedFile ? selectedFile.name : t('services.certificates.upload_placeholder', 'Click to upload file')}
                                            </span>
                                            <span className="text-xs text-[var(--color-text-muted)]">
                                                PDF, JPG, PNG (Max 5MB)
                                            </span>
                                        </div>
                                    </div>
                                </div>

                                <div className="flex gap-3 pt-4">
                                    <button
                                        onClick={() => setIsModalOpen(false)}
                                        className="flex-1 px-4 py-2 rounded-lg bg-slate-100 hover:bg-slate-200 text-[var(--color-text-secondary)] transition-colors font-medium"
                                    >
                                        {t('common.cancel')}
                                    </button>
                                    <Button
                                        onClick={handleRequest}
                                        disabled={requestMutation.isPending || isUploading}
                                        className="flex-1"
                                    >
                                        {isUploading ? (
                                            <span className="flex items-center gap-2"><Loader2 className="w-4 h-4 animate-spin" /> {t('common.uploading', 'Uploading...')}</span>
                                        ) : requestMutation.isPending ? (
                                            t('services.certificates.submitting')
                                        ) : (
                                            t('services.certificates.submit')
                                        )}
                                    </Button>
                                </div>
                            </div>
                        </div>
                    </div>
                )
            }

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {data?.items?.length === 0 && (
                    <div className="col-span-full text-center py-12 text-[var(--color-text-muted)] bg-slate-50 rounded-xl border border-dashed border-[var(--color-border)]">
                        <FileBadge className="w-12 h-12 mx-auto mb-2 opacity-20" />
                        <p>{t('services.certificates.no_requests')}</p>
                        <p className="text-xs mt-1">{t('services.certificates.click_to_start')}</p>
                    </div>
                )}

                {data?.items?.map((cert: any) => {
                    const certName = cert.items?.[0]?.certificateDefinitionName || t('services.certificates.certificate_request');
                    const statusInfo = getStatusInfo(cert.status);

                    return (
                        <div key={cert.id} className="bg-white border border-[var(--color-border)] rounded-xl p-4 flex justify-between items-center group hover:shadow-md transition-all hover:border-[var(--color-accent)]/30">
                            <div>
                                <h4 className="font-bold text-[var(--color-text-primary)] group-hover:text-[var(--color-accent)] transition-colors">{certName}</h4>
                                <div className="flex gap-2 text-xs text-[var(--color-text-muted)] mt-1">
                                    <span>{new Date(cert.creationTime).toLocaleDateString(i18n.language)}</span>
                                    <span>•</span>
                                    <span>{cert.deliveryMethod === 1 ? t('services.certificates.pickup') : t('services.certificates.home_delivery')}</span>
                                </div>
                            </div>
                            <div className={clsx(
                                "px-3 py-1 rounded-full text-xs font-bold uppercase tracking-wide border",
                                statusInfo.className
                            )}>
                                {statusInfo.label}
                            </div>
                        </div>
                    );
                })}
            </div>
        </div>
    );
};

import SyndicateApplicationWizard from '../../components/portal/syndicates/SyndicateApplicationWizard';

function SyndicatesSection() {
    const { t } = useTranslation();
    const queryClient = useQueryClient();
    // Returns array of subscriptions
    const { data: applications } = useQuery({ queryKey: ['syndicate-status'], queryFn: servicesAppService.getSyndicateStatus });
    const [isWizardOpen, setIsWizardOpen] = useState(false);

    const activeApplication = applications && applications.length > 0 ? applications[0] : null;

    const getStatusLabel = (status: number) => {
        switch (status) {
            case -1: return t('services.syndicates.status.draft');
            case 0: return t('services.syndicates.status.pending');
            case 1: return t('services.syndicates.status.reviewing');
            case 2: return t('services.syndicates.status.sent_to_syndicate');
            case 3: return t('services.syndicates.status.card_ready');
            case 4: return t('services.syndicates.status.rejected');
            case 5: return t('services.syndicates.status.received');
            default: return t('services.syndicates.status.unknown');
        }
    };

    return (
        <>
            <Card variant="default" className="max-w-xl mx-auto p-8 text-center space-y-4 border-[var(--color-border)]">
                <Building2 className="w-16 h-16 text-[var(--color-accent)] mx-auto opacity-80" />
                <h2 className="text-2xl font-bold text-[var(--color-text-primary)]">
                    {activeApplication?.syndicateName || t('services.syndicates.title')}
                </h2>

                {/* Content */}
                <>
                    <p className="text-[var(--color-text-secondary)]">
                        {t('services.syndicates.description')}
                    </p>

                    {activeApplication ? (
                        <div className="space-y-3">
                            <div className={clsx(
                                "p-4 rounded-lg font-bold border",
                                activeApplication.status === -1 ? "bg-gray-50 text-gray-700 border-gray-200" :
                                    activeApplication.status === 4 ? "bg-red-50 text-red-700 border-red-200" :
                                        "bg-emerald-50 text-emerald-700 border-emerald-200"
                            )}>
                                {t('services.syndicates.status_label')}: {getStatusLabel(activeApplication.status)}
                            </div>

                            {/* Resume Draft */}
                            {activeApplication.status === -1 && (
                                <Button
                                    onClick={() => setIsWizardOpen(true)}
                                    className="w-full"
                                    variant="outline"
                                >
                                    {t('common.resume', 'Resume Application')}
                                </Button>
                            )}

                            {/* Upload Documents for Pending/Reviewing */}
                            {(activeApplication.status === 0 || activeApplication.status === 1) && (
                                <Button
                                    onClick={() => setIsWizardOpen(true)}
                                    className="w-full"
                                    variant="outline"
                                >
                                    <Upload className="w-4 h-4 mr-2" />
                                    {t('services.syndicates.upload_docs')}
                                </Button>
                            )}
                        </div>
                    ) : (
                        <MembershipGuard>
                            <Button
                                onClick={() => setIsWizardOpen(true)}
                                className="w-full shadow-md shadow-blue-500/10"
                            >
                                {t('services.syndicates.apply_btn')}
                            </Button>
                        </MembershipGuard>
                    )}
                </>
            </Card>

            {isWizardOpen && (
                <SyndicateApplicationWizard
                    existingSubscription={activeApplication}
                    onClose={() => setIsWizardOpen(false)}
                    onSuccess={() => {
                        setIsWizardOpen(false);
                        queryClient.invalidateQueries({ queryKey: ['syndicate-status'] });
                    }}
                />
            )}
        </>
    );
};

function HealthSection() {
    const { t } = useTranslation();
    const [filterText, setFilterText] = useState('');
    const [category, setCategory] = useState('');
    const [city, setCity] = useState('');
    const [selectedPartner, setSelectedPartner] = useState<MedicalPartnerDto | null>(null);

    const debouncedFilterText = useDebounce(filterText, 800);

    const queryInput = {
        filterText: debouncedFilterText,
        category: category === 'all' ? undefined : category,
        city: city === 'all' ? undefined : city,
        maxResultCount: 50
    };

    const { data, isLoading } = useQuery({
        queryKey: ['medical-partners', queryInput],
        queryFn: () => servicesAppService.getMedicalPartners(queryInput)
    });

    const handleClearFilters = () => {
        setFilterText('');
        setCategory('');
        setCity('');
    };

    return (
        <div className="space-y-8 animate-slide-up">
            <HealthStats />

            <div className="space-y-6">
                <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                    <h2 className="text-xl font-bold text-[var(--color-text-primary)] flex items-center gap-2">
                        <HeartPulse className="w-5 h-5 text-[var(--color-accent)]" />
                        {t('services.health.title')}
                    </h2>
                </div>

                <PartnerFilterBar
                    filterText={filterText}
                    setFilterText={setFilterText}
                    category={category}
                    setCategory={setCategory}
                    city={city}
                    setCity={setCity}
                    onClear={handleClearFilters}
                />

                {isLoading ? (
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                        {[1, 2, 3].map(i => (
                            <div key={i} className="h-64 bg-slate-100 rounded-xl animate-pulse"></div>
                        ))}
                    </div>
                ) : (
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                        {data?.length === 0 && (
                            <div className="col-span-full text-center py-12 text-[var(--color-text-muted)] bg-slate-50 rounded-xl border border-dashed border-[var(--color-border)]">
                                <HeartPulse className="w-12 h-12 mx-auto mb-2 opacity-20" />
                                <p>No medical partners found matching your criteria.</p>
                                <Button
                                    variant="link"
                                    onClick={handleClearFilters}
                                    className="mt-2 text-[var(--color-accent)]"
                                >
                                    Clear Filters
                                </Button>
                            </div>
                        )}
                        {data?.map((partner: MedicalPartnerDto) => (
                            <MedicalPartnerCard
                                key={partner.id}
                                partner={partner}
                                onViewOffers={(p) => setSelectedPartner(p)}
                            />
                        ))}
                    </div>
                )}
            </div>

            <OfferDetailsModal
                isOpen={!!selectedPartner}
                onClose={() => setSelectedPartner(null)}
                partner={selectedPartner}
            />
        </div>
    );
};



export default ServicesLayout;
