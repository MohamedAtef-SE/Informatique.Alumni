import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { servicesAppService } from '../../services/servicesService';
import { Newspaper, CreditCard, Award, Gift, QrCode, FileBadge, Building2, HeartPulse, Plus, X } from 'lucide-react';
import clsx from 'clsx';
import { useAuth } from 'react-oidc-context';
import ErrorModal from '../../components/common/ErrorModal';
import { Card, CardContent } from '../../components/ui/Card';
import { Button } from '../../components/ui/Button';

const ServicesLayout = () => {
    const [activeTab, setActiveTab] = useState<'news' | 'benefits' | 'membership' | 'certificates' | 'syndicates' | 'health'>('news');
    const [selectedNewsId, setSelectedNewsId] = useState<string | null>(null);
    const auth = useAuth();
    const { t, i18n } = useTranslation();

    // Queries
    const newsQuery = useQuery({ queryKey: ['news'], queryFn: servicesAppService.getPosts, enabled: activeTab === 'news' });
    const grantsQuery = useQuery({ queryKey: ['grants'], queryFn: servicesAppService.getGrants, enabled: activeTab === 'benefits' });
    const discountsQuery = useQuery({ queryKey: ['discounts'], queryFn: servicesAppService.getDiscounts, enabled: activeTab === 'benefits' });
    const cardQuery = useQuery({ queryKey: ['card'], queryFn: servicesAppService.getCard, enabled: activeTab === 'membership' && !!auth.user });

    return (
        <div className="space-y-8 animate-fade-in">
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
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 animate-slide-up">
                        {newsQuery.data?.items.map(post => (
                            <Card
                                key={post.id}
                                variant="default"
                                onClick={() => setSelectedNewsId(post.id)}
                                className="cursor-pointer group hover:border-[var(--color-accent)]/50 border-[var(--color-border)] hover:shadow-lg transition-all"
                            >
                                <CardContent className="p-6">
                                    <h3 className="text-xl font-bold text-[var(--color-text-primary)] mb-2 group-hover:text-[var(--color-accent)] transition-colors">{post.title}</h3>
                                    <p className="text-sm text-[var(--color-text-secondary)] line-clamp-3 leading-relaxed">{post.content}</p>
                                    <div className="mt-4 pt-4 border-t border-[var(--color-border)] flex justify-between text-xs text-[var(--color-text-muted)] group-hover:border-[var(--color-accent)]/20">
                                        <span>{new Date(post.creationTime || '').toLocaleDateString(i18n.language)}</span>
                                        <span className="text-[var(--color-accent)] font-medium">{post.category}</span>
                                    </div>
                                </CardContent>
                            </Card>
                        ))}
                        {newsQuery.data?.items.length === 0 && (
                            <div className="col-span-full py-20 text-center text-[var(--color-text-muted)] bg-slate-50 rounded-xl border border-dashed border-[var(--color-border)]">
                                {t('services.news.empty')}
                            </div>
                        )}
                    </div>
                )}

                {/* News Modal */}
                {activeTab === 'news' && selectedNewsId && (
                    <NewsModal id={selectedNewsId} onClose={() => setSelectedNewsId(null)} />
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
                                                <div className="bg-slate-100 border border-slate-200 p-2 rounded text-center font-mono text-sm text-[var(--color-text-primary)] select-all cursor-pointer hover:bg-slate-200 transition-colors">
                                                    {discount.promoCode}
                                                </div>
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
                                    <div className="w-20 h-20 bg-slate-200 rounded-lg overflow-hidden border-2 border-white/30 shadow-inner">
                                        {/* Photo Placeholder */}
                                        <div className="w-full h-full bg-slate-300 flex items-center justify-center text-xs text-slate-500 font-medium">{t('services.membership.photo_placeholder')}</div>
                                    </div>
                                    <div className="flex-1">
                                        <p className="text-xs text-blue-200 mb-0.5">{t('services.membership.name_label')}</p>
                                        <h4 className="font-bold text-white text-lg uppercase truncate drop-shadow-sm">{cardQuery.data?.alumniName || auth.user?.profile.name || 'Member Name'}</h4>
                                        <div className="flex justify-between mt-2">
                                            <div>
                                                <p className="text-[10px] text-blue-200">{t('services.membership.id_label')}</p>
                                                <p className="text-sm text-white font-mono">{cardQuery.data?.alumniNationalId || 'PENDING'}</p>
                                            </div>
                                            <div className="text-right">
                                                <p className="text-[10px] text-blue-200">{t('services.membership.degree_label')}</p>
                                                <p className="text-sm text-white font-mono">{cardQuery.data?.degree || 'N/A'}</p>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div className="mt-8 text-center space-y-4">
                            <Button className="w-full shadow-lg shadow-blue-500/20 py-6 text-lg">{t('services.membership.renew_btn')}</Button>
                            <button className="px-4 py-2 w-full rounded-lg hover:bg-slate-100 text-[var(--color-text-secondary)] text-sm transition-colors">{t('services.membership.lost_btn')}</button>
                        </div>
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
const CertificatesSection = () => {
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
            alert(t('services.certificates.success'));
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

    const handleRequest = () => {
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

        const payload = {
            items: [
                {
                    certificateDefinitionId: selectedDefId,
                    language: language
                }
            ],
            deliveryMethod: deliveryMethod,
            deliveryAddress: deliveryMethod === 2 ? requestAddress : null,
            targetBranchId: deliveryMethod === 1 ? selectedBranchId : null,
            userNotes: ""
        };

        requestMutation.mutate(payload);
    };

    return (
        <div className="space-y-6 animate-slide-up">
            <ErrorModal
                isOpen={errorState.isOpen}
                title={errorState.title}
                message={errorState.message}
                onClose={() => setErrorState(prev => ({ ...prev, isOpen: false }))}
            />

            <div className="flex justify-between items-center">
                <h2 className="text-xl font-bold text-[var(--color-text-primary)] flex items-center gap-2"><FileBadge className="w-5 h-5 text-[var(--color-accent)]" /> {t('services.certificates.my_certificates')}</h2>
                <Button
                    onClick={() => setIsModalOpen(true)}
                    className="flex items-center gap-2 shadow-sm"
                >
                    <Plus className="w-4 h-4" /> {t('services.certificates.request_new')}
                </Button>
            </div>

            {/* Request Modal */}
            {isModalOpen && (
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
                                disabled={requestMutation.isPending}
                                className="flex-1"
                            >
                                {requestMutation.isPending ? t('services.certificates.submitting') : t('services.certificates.submit')}
                            </Button>
                        </div>
                    </div>
                </div>
            )}

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {data?.items?.length === 0 && (
                    <div className="col-span-full text-center py-12 text-[var(--color-text-muted)] bg-slate-50 rounded-xl border border-dashed border-[var(--color-border)]">
                        <FileBadge className="w-12 h-12 mx-auto mb-2 opacity-20" />
                        <p>{t('services.certificates.no_requests')}</p>
                        <p className="text-xs mt-1">{t('services.certificates.click_to_start')}</p>
                    </div>
                )}

                {data?.items?.map((cert: any) => {
                    const certName = cert.items?.[0]?.certificateDefinitionName || 'Certificate Request';
                    return (
                        <div key={cert.id} className="bg-white border border-[var(--color-border)] rounded-xl p-4 flex justify-between items-center group hover:shadow-md transition-all hover:border-[var(--color-accent)]/30">
                            <div>
                                <h4 className="font-bold text-[var(--color-text-primary)] group-hover:text-[var(--color-accent)] transition-colors">{certName}</h4>
                                <div className="flex gap-2 text-xs text-[var(--color-text-muted)] mt-1">
                                    <span>{new Date(cert.creationTime).toLocaleDateString(i18n.language)}</span>
                                    <span>•</span>
                                    <span>{cert.deliveryMethod === 0 ? t('services.certificates.pickup') : t('services.certificates.home_delivery')}</span>
                                </div>
                            </div>
                            <div className={clsx(
                                "px-3 py-1 rounded-full text-xs font-bold uppercase tracking-wide border",
                                cert.status === 1 ? "bg-emerald-100 text-emerald-700 border-emerald-200" :
                                    cert.status === 2 ? "bg-red-100 text-red-700 border-red-200" :
                                        "bg-amber-100 text-amber-700 border-amber-200"
                            )}>
                                {cert.status === 0 ? t('services.certificates.status.pending') : cert.status === 1 ? t('services.certificates.status.ready') : t('services.certificates.status.rejected')}
                            </div>
                        </div>
                    );
                })}
            </div>
        </div>
    );
};

const SyndicatesSection = () => {
    const { t } = useTranslation();
    // Returns array of subscriptions
    const { data: applications } = useQuery({ queryKey: ['syndicate-status'], queryFn: servicesAppService.getSyndicateStatus });

    const activeApplication = applications && applications.length > 0 ? applications[0] : null;

    const getStatusLabel = (status: number) => {
        const statuses = ['Pending', 'Reviewing', 'SentToSyndicate', 'CardReady', 'Received', 'Rejected'];
        return statuses[status] || 'Unknown';
    };

    return (
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
                    <div className="bg-emerald-50 text-emerald-700 border border-emerald-200 p-4 rounded-lg font-bold">
                        {t('services.syndicates.status_label')}: {getStatusLabel(activeApplication.status)}
                    </div>
                ) : (
                    <Button className="w-full shadow-md shadow-blue-500/10">{t('services.syndicates.apply_btn')}</Button>
                )}
            </>
        </Card>
    );
};

const HealthSection = () => {
    const { t } = useTranslation();
    const { data } = useQuery({ queryKey: ['medical-partners'], queryFn: () => servicesAppService.getMedicalPartners({ maxResultCount: 10 }) });

    return (
        <div className="space-y-6 animate-slide-up">
            <h2 className="text-xl font-bold text-[var(--color-text-primary)] flex items-center gap-2"><HeartPulse className="w-5 h-5 text-[var(--color-accent)]" /> {t('services.health.title')}</h2>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                {data?.map((partner: any) => (
                    <Card key={partner.id} variant="default" className="border-[var(--color-border)] hover:border-[var(--color-accent)]/30 hover:shadow-md transition-all">
                        <CardContent className="p-6">
                            <h3 className="font-bold text-[var(--color-text-primary)]">{partner.name}</h3>
                            <p className="text-sm text-[var(--color-accent)] font-medium">{partner.type}</p>
                            <p className="text-xs text-[var(--color-text-muted)] mt-2">{partner.address}</p>
                            <div className="mt-4 pt-4 border-t border-[var(--color-border)] flex justify-between items-center">
                                <span className="text-xs text-[var(--color-text-muted)]">{t('services.health.discount')}</span>
                                <span className="font-bold text-emerald-600">{partner.discountRate}% {t('services.health.off')}</span>
                            </div>
                        </CardContent>
                    </Card>
                ))}
            </div>
        </div>
    );
};

const NewsModal = ({ id, onClose }: { id: string; onClose: () => void }) => {
    const { data: post, isLoading } = useQuery({
        queryKey: ['news', id],
        queryFn: () => servicesAppService.getPost(id)
    });

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4 animate-fade-in">
            <div className="bg-white w-full max-w-2xl max-h-[80vh] overflow-hidden flex flex-col shadow-2xl relative rounded-2xl border border-[var(--color-border)]">
                <button
                    onClick={onClose}
                    className="absolute top-4 right-4 p-2 text-[var(--color-text-muted)] hover:text-[var(--color-text-primary)] hover:bg-slate-100 rounded-full transition-colors z-10"
                >
                    <X className="w-6 h-6" />
                </button>

                {isLoading ? (
                    <div className="flex-1 flex items-center justify-center p-20">
                        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-[var(--color-accent)]"></div>
                    </div>
                ) : post ? (
                    <div className="overflow-y-auto custom-scrollbar">
                        <div className="p-8">
                            <div className="flex items-center gap-3 text-sm text-[var(--color-text-secondary)] mb-4">
                                <span className="text-[var(--color-accent)] font-bold px-2 py-1 rounded bg-[var(--color-accent-light)]/20">{post.category}</span>
                                <span>{new Date(post.creationTime || Date.now()).toLocaleDateString()}</span>
                            </div>
                            <h2 className="text-3xl font-heading font-bold text-[var(--color-text-primary)] mb-6 leading-tight">{post.title}</h2>
                            <div className="prose max-w-none text-[var(--color-text-secondary)]">
                                {/* Simple whitespace handling or dangerous HTML if trusted */}
                                <div dangerouslySetInnerHTML={{ __html: post.content }} />
                            </div>
                        </div>
                    </div>
                ) : (
                    <div className="p-10 text-center text-[var(--color-error)]">Failed to load article.</div>
                )}
            </div>
            <div className="absolute inset-0 -z-10" onClick={onClose}></div>
        </div>
    );
};

export default ServicesLayout;
