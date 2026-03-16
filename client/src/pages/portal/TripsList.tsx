import { useState, useEffect } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { motion } from 'framer-motion';
import { Plane, MapPin, Calendar, Users, Clock, Tag, Globe } from 'lucide-react';
import { alumniService } from '../../services/alumniService';
import { useCurrencyStore, formatCurrency, detectCurrencyFromLocale } from '../../stores/useCurrencyStore';
import type { AlumniTripDto } from '../../types/trips';
import type { CurrencyDto } from '../../types/lookups';
import { Button } from '../../components/ui/Button';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogFooter } from '../../components/ui/Dialog';
import { toast } from 'sonner';

const TripsList = () => {
    const { selectedCurrency, setSelectedCurrency } = useCurrencyStore();
    const [bookingTrip, setBookingTrip] = useState<AlumniTripDto | null>(null);

    // Load currencies from backend lookup
    const { data: currencies = [] } = useQuery<CurrencyDto[]>({
        queryKey: ['portal-currencies'],
        queryFn: alumniService.getCurrencies,
        staleTime: 5 * 60 * 1000,
    });

    // Auto-detect locale currency on load; re-hydrate persisted selection
    useEffect(() => {
        if (!currencies.length) return;
        if (!selectedCurrency) {
            const detected = detectCurrencyFromLocale(currencies);
            setSelectedCurrency(detected ?? currencies.find(c => c.isBase) ?? currencies[0]);
        } else {
            // Ensure we have the full object (persist only stores code)
            const full = currencies.find(c => c.code === selectedCurrency.code);
            if (full && full.exchangeRateFromUSD !== selectedCurrency.exchangeRateFromUSD) {
                setSelectedCurrency(full);
            }
        }
    }, [currencies]);

    const { data, isLoading } = useQuery({
        queryKey: ['portal-active-trips'],
        queryFn: () => alumniService.getActiveTrips({ maxResultCount: 50 })
    });

    const trips = data?.items ?? [];

    return (
        <div className="min-h-screen bg-[var(--color-background)] py-12 px-4">
            <div className="container mx-auto max-w-6xl">
                {/* Header */}
                <motion.div
                    initial={{ opacity: 0, y: 24 }}
                    animate={{ opacity: 1, y: 0 }}
                    className="mb-12"
                >
                    <div className="flex flex-col sm:flex-row sm:items-end justify-between gap-4 mb-4">
                        <div>
                            <h1 className="text-4xl md:text-5xl font-black text-[var(--color-text-primary)] tracking-tight">
                                Alumni <span className="text-gradient">Trips</span>
                            </h1>
                            <p className="text-[var(--color-text-secondary)] mt-2">
                                Exclusive travel experiences for our alumni community
                            </p>
                        </div>

                        {/* Currency selector */}
                        {currencies.length > 0 && (
                            <div className="flex items-center gap-2 bg-white border border-[var(--color-border)] rounded-xl px-3 py-2 shadow-sm">
                                <Globe className="w-4 h-4 text-[var(--color-accent)]" />
                                <select
                                    value={selectedCurrency?.code ?? 'USD'}
                                    onChange={(e) => {
                                        const c = currencies.find(x => x.code === e.target.value);
                                        if (c) setSelectedCurrency(c);
                                    }}
                                    className="text-sm font-medium text-[var(--color-text-primary)] bg-transparent focus:outline-none cursor-pointer"
                                >
                                    {currencies.map(c => (
                                        <option key={c.code} value={c.code}>
                                            {c.flagEmoji} {c.code}
                                        </option>
                                    ))}
                                </select>
                            </div>
                        )}
                    </div>

                    {selectedCurrency && !selectedCurrency.isBase && (
                        <p className="text-xs text-[var(--color-text-muted)] mt-1">
                            Prices shown in {selectedCurrency.name} ({selectedCurrency.symbol}) · Rates updated daily from ECB
                        </p>
                    )}
                </motion.div>

                {/* Trip Cards Grid */}
                {isLoading ? (
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
                        {[...Array(6)].map((_, i) => (
                            <div key={i} className="h-80 bg-slate-100 animate-pulse rounded-3xl" />
                        ))}
                    </div>
                ) : trips.length === 0 ? (
                    <div className="text-center py-32">
                        <Plane className="w-16 h-16 text-[var(--color-text-muted)] mx-auto mb-4 opacity-30" />
                        <h3 className="text-xl font-bold text-[var(--color-text-secondary)]">No upcoming trips</h3>
                        <p className="text-[var(--color-text-muted)] mt-2">Check back soon for new travel experiences.</p>
                    </div>
                ) : (
                    <motion.div
                        className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8"
                        initial="hidden"
                        animate="visible"
                        variants={{ visible: { transition: { staggerChildren: 0.1 } } }}
                    >
                        {trips.map((trip) => (
                            <TripCard
                                key={trip.id}
                                trip={trip}
                                currency={selectedCurrency}
                                onBook={() => setBookingTrip(trip)}
                            />
                        ))}
                    </motion.div>
                )}
            </div>

            {/* Booking Modal */}
            {bookingTrip && (
                <BookTripModal
                    trip={bookingTrip}
                    currency={selectedCurrency}
                    open={!!bookingTrip}
                    onOpenChange={(open) => { if (!open) setBookingTrip(null); }}
                />
            )}
        </div>
    );
};

// ── Trip Card ──────────────────────────────────────────────────────────────────

interface TripCardProps {
    trip: AlumniTripDto;
    currency: CurrencyDto | null;
    onBook: () => void;
}

const TripCard = ({ trip, currency, onBook }: TripCardProps) => {
    const daysUntil = trip.startDate
        ? Math.ceil((new Date(trip.startDate).getTime() - Date.now()) / 86400000)
        : null;

    return (
        <motion.div
            variants={{ hidden: { opacity: 0, y: 30 }, visible: { opacity: 1, y: 0 } }}
            whileHover={{ y: -8, transition: { duration: 0.2 } }}
            className="group bg-white rounded-3xl border border-[var(--color-border)] shadow-sm hover:shadow-2xl transition-shadow duration-300 overflow-hidden flex flex-col"
        >
            {/* Image / Gradient Header */}
            <div className="h-48 bg-gradient-to-br from-sky-400/20 via-indigo-300/20 to-purple-400/20 relative flex items-center justify-center">
                <Plane className="w-20 h-20 text-sky-400/60" />
                {daysUntil !== null && daysUntil <= 30 && (
                    <span className="absolute top-4 right-4 bg-orange-500 text-white text-xs font-bold px-3 py-1 rounded-full">
                        {daysUntil}d left
                    </span>
                )}
                <span className="absolute top-4 left-4 bg-white/80 backdrop-blur text-[var(--color-text-primary)] text-xs font-bold px-3 py-1 rounded-full border border-[var(--color-border)]">
                    <Tag className="inline w-3 h-3 mr-1 text-[var(--color-accent)]" />
                    {formatCurrency(trip.pricePerPerson ?? 0, currency)} / person
                </span>
            </div>

            {/* Content */}
            <div className="p-6 flex flex-col flex-1">
                <h3 className="text-xl font-bold text-[var(--color-text-primary)] group-hover:text-[var(--color-accent)] transition-colors mb-1">
                    {trip.title}
                </h3>
                <div className="flex items-center gap-1.5 text-sm text-[var(--color-text-secondary)] mb-3">
                    <MapPin className="w-4 h-4 text-[var(--color-accent)]" />
                    {trip.destination}
                </div>

                {trip.description && (
                    <p className="text-sm text-[var(--color-text-secondary)] line-clamp-2 mb-4">{trip.description}</p>
                )}

                <div className="mt-auto space-y-2 text-xs text-[var(--color-text-muted)] border-t border-[var(--color-border)] pt-4">
                    {trip.startDate && (
                        <div className="flex items-center gap-2">
                            <Calendar className="w-3.5 h-3.5 text-[var(--color-accent)]" />
                            {new Date(trip.startDate).toLocaleDateString(undefined, { day: 'numeric', month: 'long', year: 'numeric' })}
                        </div>
                    )}
                    {trip.endDate && (
                        <div className="flex items-center gap-2">
                            <Clock className="w-3.5 h-3.5" />
                            Until {new Date(trip.endDate).toLocaleDateString(undefined, { day: 'numeric', month: 'long' })}
                        </div>
                    )}
                    {trip.maxCapacity && (
                        <div className="flex items-center gap-2">
                            <Users className="w-3.5 h-3.5" />
                            Max {trip.maxCapacity} participants
                        </div>
                    )}
                </div>

                <Button
                    className="w-full mt-4"
                    onClick={onBook}
                >
                    Book This Trip
                </Button>
            </div>
        </motion.div>
    );
};

// ── Book Trip Modal ────────────────────────────────────────────────────────────

interface BookTripModalProps {
    trip: AlumniTripDto;
    currency: CurrencyDto | null;
    open: boolean;
    onOpenChange: (open: boolean) => void;
}

const BookTripModal = ({ trip, currency, open, onOpenChange }: BookTripModalProps) => {
    const queryClient = useQueryClient();
    const [guestCount, setGuestCount] = useState(0);

    const totalParticipants = 1 + guestCount;
    const pricePerPersonUSD = trip.pricePerPerson ?? 0;
    const totalUSD = pricePerPersonUSD * totalParticipants;

    const mutation = useMutation({
        mutationFn: () => alumniService.requestTrip(trip.id, guestCount),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['portal-active-trips'] });
            toast.success('Booking request submitted! The team will contact you shortly.');
            onOpenChange(false);
        },
        onError: (err: unknown) => {
            const error = err as import('axios').AxiosError<{ error: { message: string } }>;
            toast.error(error?.response?.data?.error?.message ?? 'Booking failed. Please try again.');
        }
    });

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="max-w-md">
                <DialogHeader>
                    <DialogTitle className="flex items-center gap-2">
                        <Plane className="w-5 h-5 text-[var(--color-accent)]" />
                        Book — {trip.title}
                    </DialogTitle>
                    <DialogDescription>
                        Submit a booking request. The alumni team will confirm availability and contact you for payment.
                    </DialogDescription>
                </DialogHeader>

                <div className="space-y-5 py-2">
                    {/* Destination & dates */}
                    <div className="bg-slate-50 rounded-2xl p-4 space-y-2 text-sm text-[var(--color-text-secondary)]">
                        <div className="flex items-center gap-2">
                            <MapPin className="w-4 h-4 text-[var(--color-accent)]" />
                            <span className="font-medium text-[var(--color-text-primary)]">{trip.destination}</span>
                        </div>
                        {trip.startDate && (
                            <div className="flex items-center gap-2">
                                <Calendar className="w-4 h-4" />
                                {new Date(trip.startDate).toLocaleDateString()} – {trip.endDate ? new Date(trip.endDate).toLocaleDateString() : '—'}
                            </div>
                        )}
                    </div>

                    {/* Guest count */}
                    <div>
                        <label className="block text-sm font-medium text-[var(--color-text-primary)] mb-2">
                            Additional guests (family / companions)
                        </label>
                        <div className="flex items-center gap-3">
                            <button
                                type="button"
                                onClick={() => setGuestCount(Math.max(0, guestCount - 1))}
                                className="w-9 h-9 rounded-full border border-[var(--color-border)] flex items-center justify-center text-lg font-bold text-[var(--color-text-primary)] hover:border-[var(--color-accent)] transition-colors"
                            >−</button>
                            <span className="w-8 text-center text-xl font-bold text-[var(--color-text-primary)]">{guestCount}</span>
                            <button
                                type="button"
                                onClick={() => setGuestCount(guestCount + 1)}
                                className="w-9 h-9 rounded-full border border-[var(--color-border)] flex items-center justify-center text-lg font-bold text-[var(--color-text-primary)] hover:border-[var(--color-accent)] transition-colors"
                            >+</button>
                            <span className="text-sm text-[var(--color-text-muted)] ml-1">= {totalParticipants} person{totalParticipants > 1 ? 's' : ''}</span>
                        </div>
                    </div>

                    {/* Price breakdown */}
                    <div className="rounded-2xl border border-[var(--color-border)] divide-y divide-[var(--color-border)] text-sm">
                        <div className="flex justify-between px-4 py-3 text-[var(--color-text-secondary)]">
                            <span>Price per person</span>
                            <span className="font-medium text-[var(--color-text-primary)]">
                                {formatCurrency(pricePerPersonUSD, currency)}
                            </span>
                        </div>
                        <div className="flex justify-between px-4 py-3 text-[var(--color-text-secondary)]">
                            <span>× {totalParticipants} participant{totalParticipants > 1 ? 's' : ''}</span>
                        </div>
                        <div className="flex justify-between px-4 py-3 font-bold text-[var(--color-text-primary)]">
                            <span>Total</span>
                            <div className="text-right">
                                <div className="text-lg text-[var(--color-accent)]">{formatCurrency(totalUSD, currency)}</div>
                                {currency && !currency.isBase && (
                                    <div className="text-xs font-normal text-[var(--color-text-muted)]">≈ ${totalUSD.toFixed(2)} USD</div>
                                )}
                            </div>
                        </div>
                    </div>

                    <p className="text-xs text-[var(--color-text-muted)] bg-amber-50 border border-amber-200 rounded-xl px-4 py-3">
                        💳 <strong>Payment:</strong> No payment is taken now. After the team confirms your booking, you will be contacted with payment instructions.
                    </p>
                </div>

                <DialogFooter>
                    <Button variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
                    <Button onClick={() => mutation.mutate()} isLoading={mutation.isPending}>
                        Confirm Request
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
};

export default TripsList;
