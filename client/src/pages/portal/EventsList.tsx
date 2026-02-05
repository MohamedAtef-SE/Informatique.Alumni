import { useQuery } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { eventsService } from '../../services/eventsService';
import { MapPin, ArrowRight, Calendar, Plane } from 'lucide-react';
import { Link } from 'react-router-dom';
import { useState } from 'react';
import clsx from 'clsx';
import { Card, CardContent } from '../../components/ui/Card';
import { Button } from '../../components/ui/Button';

const EventsList = () => {
    const [activeTab, setActiveTab] = useState<'events' | 'trips'>('events');
    const { t } = useTranslation();

    return (
        <div className="space-y-8 animate-fade-in">
            <div>
                <h1 className="text-3xl font-heading font-bold text-[var(--color-text-primary)]">{t('events.title')}</h1>
                <p className="text-[var(--color-text-secondary)]">{t('events.subtitle')}</p>
            </div>

            {/* Tabs */}
            <div className="flex gap-4 border-b border-[var(--color-border)]">
                <button
                    onClick={() => setActiveTab('events')}
                    className={clsx("px-4 py-3 flex items-center gap-2 border-b-2 transition-colors font-medium", activeTab === 'events' ? "border-[var(--color-accent)] text-[var(--color-accent)]" : "border-transparent text-[var(--color-text-secondary)] hover:text-[var(--color-text-primary)]")}
                >
                    <Calendar className="w-4 h-4" /> {t('events.tabs.events')}
                </button>
                <button
                    onClick={() => setActiveTab('trips')}
                    className={clsx("px-4 py-3 flex items-center gap-2 border-b-2 transition-colors font-medium", activeTab === 'trips' ? "border-[var(--color-accent)] text-[var(--color-accent)]" : "border-transparent text-[var(--color-text-secondary)] hover:text-[var(--color-text-primary)]")}
                >
                    <Plane className="w-4 h-4" /> {t('events.tabs.trips')}
                </button>
            </div>

            {activeTab === 'events' ? <EventsTab /> : <TripsTab />}
        </div>
    );
};

const EventsTab = () => {
    const { t, i18n } = useTranslation();
    const { data } = useQuery({
        queryKey: ['events'],
        queryFn: () => eventsService.getList({ maxResultCount: 20 })
    });

    return (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8 animate-slide-up">
            {data?.items.map((event) => (
                <Card key={event.id} variant="default" className="overflow-hidden flex flex-col group hover:border-[var(--color-accent)]/50 transition-all relative border-[var(--color-border)]">
                    {/* Image Placeholder */}
                    <div className="h-48 bg-gradient-to-br from-indigo-50 to-slate-100 relative p-4 flex flex-col justify-end border-b border-[var(--color-border)]">
                        <div className="absolute top-4 right-4 bg-white/90 backdrop-blur px-3 py-1 rounded-lg border border-[var(--color-border)] text-xs font-bold text-[var(--color-text-primary)] shadow-sm">
                            {event.hasFees ? `${event.feeAmount} ${t('common.currency')}` : t('events.free_entry')}
                        </div>
                        <div className="absolute top-4 left-4 bg-[var(--color-accent)] text-white w-12 h-14 rounded-lg flex flex-col items-center justify-center font-bold shadow-lg">
                            <span className="text-xs uppercase">{new Date(event.lastSubscriptionDate).toLocaleString(i18n.language, { month: 'short' })}</span>
                            <span className="text-xl">{new Date(event.lastSubscriptionDate).getDate()}</span>
                        </div>
                    </div>

                    <CardContent className="p-6 flex flex-col flex-1">
                        <div className="flex items-start justify-between mb-4">
                            <div>
                                <h3 className="text-xl font-bold text-[var(--color-text-primary)] mb-1 group-hover:text-[var(--color-accent)] transition-colors line-clamp-2">
                                    {event.nameEn}
                                </h3>
                                <div className="flex items-center gap-2 text-xs text-[var(--color-text-muted)] uppercase tracking-wider">
                                    <span className="px-1.5 py-0.5 rounded bg-slate-100 border border-slate-200">{event.code}</span>
                                </div>
                            </div>
                        </div>

                        <p className="text-[var(--color-text-secondary)] text-sm line-clamp-3 mb-6 flex-1 leading-relaxed">
                            {event.description}
                        </p>

                        <div className="space-y-3 pt-4 border-t border-[var(--color-border)]">
                            <div className="flex items-center gap-2 text-sm text-[var(--color-text-secondary)]">
                                <MapPin className="w-4 h-4 text-[var(--color-accent)]" />
                                <span className="truncate">{event.location}</span>
                            </div>
                        </div>

                        <Link
                            to={`/portal/events/${event.id}`}
                            className="mt-6 w-full"
                        >
                            <Button variant="outline" className="w-full text-sm group-hover:bg-[var(--color-accent)] group-hover:text-white transition-all">
                                {t('events.details_btn')} <ArrowRight className="w-4 h-4 ml-2 rtl:rotate-180" />
                            </Button>
                        </Link>
                    </CardContent>
                </Card>
            ))}
            {data?.items.length === 0 && (
                <div className="col-span-full py-20 text-center text-[var(--color-text-muted)] bg-slate-50 rounded-xl border border-dashed border-[var(--color-border)]">
                    {t('events.no_events')}
                </div>
            )}
        </div>
    );
};

const TripsTab = () => {
    const { t } = useTranslation();
    const { data } = useQuery({ queryKey: ['trips'], queryFn: () => eventsService.getTrips({ maxResultCount: 20 }) });

    return (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8 animate-slide-up">
            {data?.items.map((trip: any) => (
                <Card key={trip.id} variant="default" className="overflow-hidden flex flex-col group hover:border-emerald-500/50 transition-all relative border-[var(--color-border)]">
                    <div className="h-48 bg-gradient-to-br from-emerald-50 to-teal-50 relative p-4 flex flex-col justify-end border-b border-[var(--color-border)]">
                        <div className="absolute top-4 right-4 bg-white/90 backdrop-blur px-3 py-1 rounded-lg border border-[var(--color-border)] text-xs font-bold text-[var(--color-text-primary)] shadow-sm">
                            {trip.fees} {t('common.currency')}
                        </div>
                        <div className="absolute top-4 left-4 bg-emerald-500 text-white w-12 h-14 rounded-lg flex flex-col items-center justify-center font-bold shadow-lg">
                            <span className="text-xs uppercase">{t('events.days')}</span>
                            <span className="text-xl">{trip.days}</span>
                        </div>
                    </div>

                    <CardContent className="p-6 flex flex-col flex-1">
                        <h3 className="text-xl font-bold text-[var(--color-text-primary)] mb-2 group-hover:text-emerald-600 transition-colors">{trip.title}</h3>
                        <p className="text-[var(--color-text-secondary)] text-sm line-clamp-3 mb-6 flex-1 leading-relaxed">
                            {trip.description}
                        </p>
                        <div className="space-y-3 pt-4 border-t border-[var(--color-border)]">
                            <div className="flex items-center gap-2 text-sm text-[var(--color-text-secondary)]">
                                <MapPin className="w-4 h-4 text-emerald-500" />
                                <span className="truncate">{trip.destination}</span>
                            </div>
                            <div className="flex items-center gap-2 text-sm text-[var(--color-text-secondary)]">
                                <Calendar className="w-4 h-4 text-emerald-500" />
                                <span>{new Date(trip.dateFrom).toLocaleDateString()}</span>
                            </div>
                        </div>
                        <Button className="mt-6 w-full bg-emerald-600 hover:bg-emerald-700 text-white shadow-emerald-500/20">
                            {t('events.trip_details_btn')}
                        </Button>
                    </CardContent>
                </Card>
            ))}
            {data?.items.length === 0 && (
                <div className="col-span-full py-20 text-center text-[var(--color-text-muted)] bg-slate-50 rounded-xl border border-dashed border-[var(--color-border)]">
                    {t('events.no_trips')}
                </div>
            )}
        </div>
    );
};

export default EventsList;
