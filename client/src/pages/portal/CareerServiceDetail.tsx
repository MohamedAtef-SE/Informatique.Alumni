import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { careerService } from '../../services/careerService';
import { CareerPaymentMethod } from '../../types/career';
import { Clock, User, MapPin } from 'lucide-react';
import clsx from 'clsx';
import { Card, CardContent } from '../../components/ui/Card';
import { Button } from '../../components/ui/Button';
import FeedbackModal from '../../components/common/FeedbackModal';

interface ModalState {
    isOpen: boolean;
    variant: 'success' | 'warning' | 'info';
    title: string;
    message: string;
}

const CareerServiceDetail = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const [selectedTimeslot, setSelectedTimeslot] = useState<string | null>(null);
    const { t } = useTranslation();

    // Modal state for feedback
    const [modal, setModal] = useState<ModalState>({
        isOpen: false,
        variant: 'success',
        title: '',
        message: '',
    });

    const closeModal = () => setModal((prev) => ({ ...prev, isOpen: false }));

    const { data: service } = useQuery({
        queryKey: ['career-service', id],
        queryFn: () => careerService.getService(id!),
        enabled: !!id
    });

    const subscribeMutation = useMutation({
        mutationFn: () => careerService.subscribe(id!, selectedTimeslot!, CareerPaymentMethod.Cash),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['career-service', id] });
            setModal({
                isOpen: true,
                variant: 'success',
                title: t('career.detail.success_title', 'Registration Successful!'),
                message: t('career.detail.success_message', 'You have successfully registered for this session. We look forward to seeing you!'),
            });
            setSelectedTimeslot(null);
        },
        onError: (err: Error & { response?: { data?: { error?: { code?: string; message?: string } } } }) => {
            console.error(err);

            // Parse ABP error response for specific code
            const errorCode = err.response?.data?.error?.code;
            let title = t('career.detail.error_title', 'Registration Failed');
            let message = t('career.detail.error_message', 'An error occurred while processing your registration. Please try again.');
            let variant: 'warning' | 'info' = 'warning';

            // Handle specific error codes
            if (errorCode === 'Career:TimeOverlapDetected') {
                title = t('career.detail.already_registered_title', 'Already Registered');
                message = t('career.detail.already_registered_message', 'You already have a session booked at this time. Please choose a different timeslot.');
                variant = 'info';
            } else if (errorCode === 'Career:TimeslotFull') {
                title = t('career.detail.session_full_title', 'Session Full');
                message = t('career.detail.session_full_message', 'This session has reached maximum capacity. Please select another session.');
                variant = 'warning';
            } else if (errorCode === 'Career:MembershipNotActive') {
                title = t('career.detail.membership_required_title', 'Membership Required');
                message = t('career.detail.membership_required_message', 'An active membership is required to register for career services.');
                variant = 'warning';
            } else if (errorCode === 'Career:AlreadySubscribedToService') {
                title = t('career.detail.duplicate_subscription_title', 'Already Subscribed');
                message = t('career.detail.duplicate_subscription_message', 'You already have an active registration for this career service.');
                variant = 'info';
            }

            setModal({ isOpen: true, variant, title, message });
        }
    });


    if (!service) return <div className="text-center py-20 text-[var(--color-text-muted)]">{t('career.detail.not_found')}</div>;

    return (
        <>
            <div className="max-w-4xl mx-auto space-y-6 animate-fade-in">
                <button onClick={() => navigate(-1)} className="text-sm text-[var(--color-text-secondary)] hover:text-[var(--color-accent)] mb-4 transition-colors">
                    &larr; {t('career.detail.back_btn')}
                </button>

                <Card variant="default">
                    <CardContent className="p-8">
                        <div className="flex justify-between items-start">
                            <div>
                                <h1 className="text-3xl font-heading font-bold text-[var(--color-text-primary)] mb-2">{service.nameEn}</h1>
                                <div className="flex items-center gap-4 text-sm text-[var(--color-text-secondary)]">
                                    <span className="bg-slate-100 px-2 py-1 rounded text-[var(--color-text-primary)] border border-slate-200">{service.code}</span>
                                    <span>{service.hasFees ? t('career.paid_event') : t('career.free_event')}</span>
                                </div>
                            </div>

                            <div className="text-right">
                                <p className="text-2xl font-bold text-[var(--color-accent)]">
                                    {service.hasFees ? `${service.feeAmount} ${t('common.currency')}` : t('career.free')}
                                </p>
                            </div>
                        </div>

                        <p className="mt-8 text-[var(--color-text-secondary)] leading-relaxed whitespace-pre-wrap">{service.description}</p>

                        {service.mapUrl && (
                            <div className="mt-6">
                                <a href={service.mapUrl} target="_blank" rel="noopener noreferrer" className="text-[var(--color-accent)] hover:underline flex items-center gap-2 font-medium">
                                    <MapPin className="w-4 h-4" /> {t('career.detail.view_map')}
                                </a>
                            </div>
                        )}
                    </CardContent>
                </Card>

                {/* Timeslots */}
                <div className="space-y-4">
                    <h2 className="text-xl font-bold text-[var(--color-text-primary)]">{t('career.detail.sessions_title')}</h2>
                    {service.timeslots.length === 0 ? (
                        <p className="text-[var(--color-text-muted)]">{t('career.detail.no_sessions')}</p>
                    ) : (
                        <div className="grid gap-4">
                            {service.timeslots.map(slot => (
                                <div
                                    key={slot.id}
                                    onClick={() => setSelectedTimeslot(slot.id)}
                                    className={clsx(
                                        "p-4 rounded-xl border cursor-pointer transition-all flex justify-between items-center group",
                                        selectedTimeslot === slot.id
                                            ? "bg-[var(--color-accent-light)]/20 border-[var(--color-accent)] ring-1 ring-[var(--color-accent)]"
                                            : "bg-white border-[var(--color-border)] hover:border-[var(--color-accent)]/50 hover:shadow-sm"
                                    )}
                                >
                                    <div className="flex items-center gap-6">
                                        <div className="text-center w-16">
                                            <p className="text-xs text-[var(--color-text-muted)] uppercase">{new Date(slot.date).toLocaleString('default', { month: 'short' })}</p>
                                            <p className="text-xl font-bold text-[var(--color-text-primary)]">{new Date(slot.date).getDate()}</p>
                                        </div>

                                        <div>
                                            <div className="flex items-center gap-4 text-sm text-[var(--color-text-secondary)] mb-1">
                                                <span className="flex items-center gap-1"><Clock className="w-4 h-4" /> {slot.startTime} - {slot.endTime}</span>
                                                <span className="flex items-center gap-1"><User className="w-4 h-4" /> {slot.lecturerName}</span>
                                            </div>
                                            <p className="text-sm text-[var(--color-text-muted)] flex items-center gap-1">
                                                <MapPin className="w-3 h-3" /> {slot.room} - {slot.address}
                                            </p>
                                        </div>
                                    </div>

                                    <div className="text-right">
                                        <div className={clsx("text-sm font-bold", slot.currentCount >= slot.capacity ? "text-red-500" : "text-emerald-500")}>
                                            {slot.currentCount} / {slot.capacity}
                                        </div>
                                        <div className="text-xs text-[var(--color-text-muted)]">{t('career.detail.seats_taken')}</div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>

                {/* Action Bar */}
                <div className="fixed bottom-0 left-0 right-0 p-4 bg-white/90 backdrop-blur border-t border-[var(--color-border)] flex justify-end items-center gap-4 z-50 md:sticky md:bottom-auto md:bg-transparent md:border-none md:justify-end">
                    <Button
                        disabled={!selectedTimeslot || subscribeMutation.isPending}
                        onClick={() => subscribeMutation.mutate()}
                        className="px-8 py-6 text-lg shadow-lg shadow-blue-500/20"
                    >
                        {subscribeMutation.isPending ? t('career.detail.processing') : t('career.detail.confirm_btn')}
                    </Button>
                </div>
            </div>

            {/* Feedback Modal */}
            <FeedbackModal
                isOpen={modal.isOpen}
                variant={modal.variant}
                title={modal.title}
                message={modal.message}
                onClose={closeModal}
            />
        </>
    );
};

export default CareerServiceDetail;

