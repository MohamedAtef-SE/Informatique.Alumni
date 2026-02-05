import { useParams, useNavigate } from 'react-router-dom';
import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { eventsService } from '../../services/eventsService';
import { RegistrationStatus } from '../../types/events';
import { Calendar, MapPin, Clock, FileText, CheckCircle } from 'lucide-react';
import clsx from 'clsx';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/Card';
import { Button } from '../../components/ui/Button';
import ErrorModal from '../../components/common/ErrorModal';

const EventDetail = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const { t } = useTranslation();

    // Error Modal State
    const [errorState, setErrorState] = useState<{ isOpen: boolean; title: string; message: string }>({
        isOpen: false,
        title: '',
        message: ''
    });

    const { data: event } = useQuery({
        queryKey: ['event', id],
        queryFn: () => eventsService.get(id!),
        enabled: !!id
    });

    const registerMutation = useMutation({
        mutationFn: () => eventsService.register(id!),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['event', id] });
            // Show success modal
            setErrorState({
                isOpen: true,
                title: t('event_detail.register_success_title', 'Registration Successful!'),
                message: t('event_detail.register_success_msg', 'You have been successfully registered for this event. Check your email for confirmation.')
            });
        },
        onError: (err: any) => {
            console.error(err);
            const errorBody = err.response?.data?.error;
            const errorMessage = errorBody?.message || err.message || t('event_detail.register_error');

            let title = t('common.error', 'Error');
            let userMessage = errorMessage;

            // Handle specific error cases
            if (errorMessage.includes('already registered')) {
                title = t('event_detail.already_registered_title', 'Already Registered');
                userMessage = t('event_detail.already_registered_msg', 'You are already registered for this event. Check your registrations in your profile.');
            } else if (errorMessage.includes('membership') || errorMessage.includes('MembershipExpired')) {
                title = t('event_detail.membership_required_title', 'Membership Required');
                userMessage = t('event_detail.membership_required_msg', 'You need an active membership to register for events. Please renew your membership.');
            } else if (errorMessage.includes('capacity') || errorMessage.includes('full')) {
                title = t('event_detail.event_full_title', 'Event Full');
                userMessage = t('event_detail.event_full_msg', 'Sorry, this event has reached maximum capacity.');
            }

            setErrorState({
                isOpen: true,
                title: title,
                message: userMessage
            });
        }
    });


    if (!event) return <div className="text-center py-20 text-[var(--color-text-muted)]">{t('event_detail.not_found')}</div>;

    const isRegistered = !!event.myRegistration;

    return (
        <div className="max-w-5xl mx-auto space-y-8 animate-fade-in">
            {/* Error/Success Modal */}
            <ErrorModal
                isOpen={errorState.isOpen}
                title={errorState.title}
                message={errorState.message}
                onClose={() => setErrorState(prev => ({ ...prev, isOpen: false }))}
            />

            <button onClick={() => navigate(-1)} className="text-sm text-[var(--color-text-secondary)] hover:text-[var(--color-accent)] mb-6 transition-colors">
                &larr; {t('event_detail.back_btn')}
            </button>

            {/* Hero Section */}
            <div className="relative rounded-2xl overflow-hidden bg-white border border-[var(--color-border)] shadow-md mb-8">
                <div className="h-48 bg-gradient-to-r from-blue-50 to-indigo-50 absolute top-0 left-0 w-full"></div>

                <div className="relative p-8 md:p-12 flex flex-col md:flex-row items-end gap-8 h-full min-h-[250px] justify-end">
                    <div className="flex-1">
                        <span className={clsx(
                            "px-3 py-1 rounded-full text-xs font-bold uppercase",
                            event.isPublished ? "bg-emerald-100 text-emerald-700" : "bg-amber-100 text-amber-700"
                        )}>
                            {event.isPublished ? t('event_detail.status.open') : t('event_detail.status.closed')}
                        </span>

                        <h1 className="text-4xl font-heading font-bold text-[var(--color-text-primary)] mt-4 mb-2">{event.nameEn}</h1>
                        <div className="flex flex-wrap gap-6 text-[var(--color-text-secondary)] mt-4">
                            <div className="flex items-center gap-2">
                                <Calendar className="w-5 h-5 text-[var(--color-accent)]" />
                                <span>{new Date(event.lastSubscriptionDate).toLocaleDateString()}</span>
                            </div>
                            <div className="flex items-center gap-2">
                                <MapPin className="w-5 h-5 text-[var(--color-accent)]" />
                                <span className="font-semibold">{event.location}</span>
                            </div>
                            <div className="flex items-center gap-2">
                                <span className="font-bold text-[var(--color-accent)] text-lg">
                                    {event.hasFees ? `${event.feeAmount} ${t('common.currency')}` : t('events.free_entry')}
                                </span>
                            </div>
                        </div>
                    </div>

                    <div className="w-full md:w-auto">
                        {isRegistered ? (
                            <div className="bg-emerald-50 border border-emerald-200 rounded-xl p-4 text-center min-w-[200px]">
                                <CheckCircle className="w-8 h-8 text-emerald-500 mx-auto mb-2" />
                                <p className="text-emerald-700 font-bold">{t('event_detail.registered')}</p>
                                <p className="text-xs text-emerald-600 mt-1">{t('event_detail.status_label')}: {Object.keys(RegistrationStatus).find(key => RegistrationStatus[key as keyof typeof RegistrationStatus] === event.myRegistration?.status)}</p>
                            </div>
                        ) : (
                            <Button
                                disabled={registerMutation.isPending}
                                onClick={() => registerMutation.mutate()}
                                className="w-full md:w-auto px-8 py-6 text-lg shadow-lg shadow-blue-500/20"
                            >
                                {registerMutation.isPending ? t('event_detail.register_processing') : t('event_detail.register_btn')}
                            </Button>
                        )}
                    </div>
                </div>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
                {/* Main Content */}
                <div className="lg:col-span-2 space-y-8">
                    <Card variant="default">
                        <CardHeader>
                            <CardTitle>{t('event_detail.about_title')}</CardTitle>
                        </CardHeader>
                        <CardContent>
                            <div className="prose max-w-none text-[var(--color-text-secondary)]">
                                <p className="whitespace-pre-wrap leading-relaxed">{event.description}</p>
                            </div>
                        </CardContent>
                    </Card>

                    {/* Agenda */}
                    <Card variant="default">
                        <CardContent className="p-8">
                            <div className="flex items-center justify-between mb-6">
                                <h2 className="text-xl font-bold text-[var(--color-text-primary)]">{t('event_detail.agenda_title')}</h2>
                                <button className="text-sm text-[var(--color-accent)] flex items-center gap-2 hover:underline font-medium">
                                    <FileText className="w-4 h-4" /> {t('event_detail.download_pdf')}
                                </button>
                            </div>

                            <div className="space-y-6 relative before:absolute before:left-[19px] before:top-2 before:bottom-2 before:w-0.5 before:bg-[var(--color-border)] rtl:before:right-[19px] rtl:before:left-auto">
                                {!event.agenda?.length ? (
                                    <p className="pl-10 rtl:pl-0 rtl:pr-10 text-[var(--color-text-muted)]">{t('event_detail.agenda_empty')}</p>
                                ) : (
                                    event.agenda.map((item) => (
                                        <div key={item.id} className="relative pl-10 rtl:pl-0 rtl:pr-10">
                                            <div className="absolute left-3 top-1.5 w-3 h-3 rounded-full bg-white border-2 border-[var(--color-accent)] z-10 shadow-sm rtl:right-3 rtl:left-auto"></div>
                                            <div className="bg-slate-50 border border-[var(--color-border)] rounded-lg p-4 hover:border-[var(--color-accent)]/50 transition-all">
                                                <div className="flex flex-col md:flex-row md:items-center justify-between gap-2 mb-2">
                                                    <h3 className="font-bold text-[var(--color-text-primary)] text-lg">{item.activityName}</h3>
                                                    <div className="flex items-center gap-2 text-sm text-[var(--color-accent)] font-mono bg-[var(--color-accent-light)] px-2 py-1 rounded">
                                                        <Clock className="w-3 h-3" />
                                                        {item.startTime} - {item.endTime}
                                                    </div>
                                                </div>
                                                {item.description && <p className="text-sm text-[var(--color-text-secondary)]">{item.description}</p>}
                                                {item.place && <p className="text-xs text-[var(--color-text-muted)] mt-2 flex items-center gap-1"><MapPin className="w-3 h-3" /> {item.place}</p>}
                                            </div>
                                        </div>
                                    ))
                                )}
                            </div>
                        </CardContent>
                    </Card>
                </div>

                {/* Sidebar Info */}
                <div className="space-y-6">
                    {/* Map Card */}
                    {event.googleMapUrl && (
                        <Card variant="default" className="overflow-hidden h-64 relative group border-[var(--color-border)]">
                            <iframe
                                width="100%"
                                height="100%"
                                style={{ border: 0 }}
                                loading="lazy"
                                allowFullScreen
                                src={event.googleMapUrl.replace('http:', 'https:')}
                                title="Event Location"
                                className="filter grayscale group-hover:grayscale-0 transition-all duration-500"
                            ></iframe>
                        </Card>
                    )}

                    {/* Sponsors/Companies */}
                    {event.participatingCompanies?.length > 0 && (
                        <Card variant="default">
                            <CardContent className="p-6">
                                <h3 className="text-lg font-bold text-[var(--color-text-primary)] mb-4">{t('event_detail.companies_title')}</h3>
                                <div className="flex flex-wrap gap-4">
                                    {event.participatingCompanies.map(pc => (
                                        <div key={pc.id} className="bg-slate-50 border border-[var(--color-border)] p-2 rounded-lg" title={pc.company?.nameEn}>
                                            <div className="w-12 h-12 flex items-center justify-center text-[var(--color-text-secondary)] font-bold text-xs">
                                                {pc.company?.nameEn.substring(0, 2)}
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            </CardContent>
                        </Card>
                    )}
                </div>
            </div>
        </div>
    );
};

export default EventDetail;
