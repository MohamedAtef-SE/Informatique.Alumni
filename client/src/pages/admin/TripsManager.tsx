import { useState, useEffect } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import {
    Plus, MapPin, Calendar, Users, Trash2, RefreshCw,
    ChevronRight, CheckCircle, XCircle, Power, PowerOff, Eye, Clock, DollarSign
} from 'lucide-react';
import { PageHeader } from '../../components/admin/PageHeader';
import { DataTableShell } from '../../components/admin/DataTableShell';
import { Button } from '../../components/ui/Button';
import { CreateTripModal } from '../../components/admin/trips/CreateTripModal';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/Table';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '../../components/ui/Dialog';
import { toast } from 'sonner';
import type { TripAdminDto, TripRequestAdminDto } from '../../types/trips';
import type { CurrencyDto } from '../../types/lookups';
import { StatusBadge } from '../../components/admin/StatusBadge';
import { useCurrencyStore, formatCurrency } from '../../stores/useCurrencyStore';
import { ConfirmDialog } from '../../components/ui/ConfirmDialog';

const TripType: Record<number, string> = {
    0: 'Internal',
    1: 'External',
    2: 'International',
};

const RequestStatusLabel: Record<number, string> = {
    0: 'Pending',
    1: 'Approved',
    2: 'Rejected',
    3: 'Cancelled',
};

const TripsManager = () => {
    const queryClient = useQueryClient();
    const [page, setPage] = useState(1);
    const pageSize = 10;
    const [filter, setFilter] = useState('');
    const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
    const [selectedTrip, setSelectedTrip] = useState<TripAdminDto | null>(null);
    const [confirmAction, setConfirmAction] = useState<{
        type: 'delete' | 'activate' | 'deactivate';
        trip: TripAdminDto;
    } | null>(null);
    const { selectedCurrency, setSelectedCurrency } = useCurrencyStore();

    // Load currencies from backend lookup table
    const { data: currencies = [], isLoading: currenciesLoading } = useQuery<CurrencyDto[]>({
        queryKey: ['currencies'],
        queryFn: adminService.getCurrencies,
        staleTime: 5 * 60 * 1000,
    });

    useEffect(() => {
        if (!currencies.length) return;
        if (!selectedCurrency) {
            setSelectedCurrency(currencies.find(c => c.isBase) ?? currencies[0]);
        } else {
            const full = currencies.find(c => c.code === selectedCurrency.code);
            if (full && full.exchangeRateFromUSD !== selectedCurrency.exchangeRateFromUSD) {
                setSelectedCurrency(full);
            }
        }
    }, [currencies]);

    // Load trips via TripAdminAppService
    const { data, isLoading } = useQuery({
        queryKey: ['admin-trips', page, filter],
        queryFn: () => adminService.getTrips({
            skipCount: (page - 1) * pageSize,
            maxResultCount: pageSize,
            filter
        })
    });

    const activateMutation = useMutation({
        mutationFn: adminService.activateTrip,
        onSuccess: () => { queryClient.invalidateQueries({ queryKey: ['admin-trips'] }); toast.success('Trip activated'); },
        onError: () => toast.error('Failed to activate trip')
    });

    const deactivateMutation = useMutation({
        mutationFn: adminService.deactivateTrip,
        onSuccess: () => { queryClient.invalidateQueries({ queryKey: ['admin-trips'] }); toast.success('Trip deactivated'); },
        onError: () => toast.error('Failed to deactivate trip')
    });

    const deleteMutation = useMutation({
        mutationFn: adminService.deleteTrip,
        onSuccess: () => { queryClient.invalidateQueries({ queryKey: ['admin-trips'] }); toast.success('Trip deleted'); },
        onError: () => toast.error('Failed to delete trip')
    });

    const executeConfirmedAction = () => {
        if (!confirmAction) return;
        if (confirmAction.type === 'delete') deleteMutation.mutate(confirmAction.trip.id);
        else if (confirmAction.type === 'activate') activateMutation.mutate(confirmAction.trip.id);
        else if (confirmAction.type === 'deactivate') deactivateMutation.mutate(confirmAction.trip.id);
        setConfirmAction(null);
    };

    return (
        <div className="space-y-8 animate-in fade-in duration-500">
            <PageHeader
                title="Trips Manager"
                description="Manage alumni trips and booking requests."
                action={
                    <div className="flex items-center gap-2">
                        {/* Currency selector */}
                        <div className="relative">
                            <select
                                value={selectedCurrency?.code ?? 'USD'}
                                onChange={(e) => {
                                    const chosen = currencies.find(c => c.code === e.target.value);
                                    if (chosen) setSelectedCurrency(chosen);
                                }}
                                disabled={currenciesLoading}
                                className="h-9 rounded-md border border-slate-300 bg-white text-slate-700 text-sm pl-3 pr-8 focus:outline-none focus:ring-1 focus:ring-indigo-500 cursor-pointer appearance-none"
                            >
                                {currencies.map(c => (
                                    <option key={c.code} value={c.code}>{c.flagEmoji} {c.code}</option>
                                ))}
                            </select>
                            {currenciesLoading && (
                                <RefreshCw className="absolute right-2 top-2.5 w-4 h-4 text-slate-400 animate-spin" />
                            )}
                        </div>
                        <Button onClick={() => setIsCreateModalOpen(true)} className="shadow-neon">
                            <Plus className="w-4 h-4 mr-2" /> New Trip
                        </Button>
                    </div>
                }
            />

            {selectedCurrency && selectedCurrency.exchangeRateFromUSD && !selectedCurrency.isBase && (
                <p className="text-xs text-slate-500 -mt-4">
                    1 USD = {selectedCurrency.symbol}{selectedCurrency.exchangeRateFromUSD.toLocaleString()} {selectedCurrency.code}
                    &nbsp;·&nbsp;Rates synced daily from ECB
                </p>
            )}

            <DataTableShell
                searchPlaceholder="Search trips..."
                onSearch={setFilter}
                pagination={{
                    currentPage: page,
                    totalPages: Math.ceil((data?.totalCount || 0) / pageSize),
                    onPageChange: setPage
                }}
            >
                <Table>
                    <TableHeader>
                        <TableRow>
                            <TableHead className="w-[22%]">Trip Name</TableHead>
                            <TableHead>Type</TableHead>
                            <TableHead>Location</TableHead>
                            <TableHead>Dates</TableHead>
                            <TableHead>Price</TableHead>
                            <TableHead>Capacity</TableHead>
                            <TableHead>Requests</TableHead>
                            <TableHead>Status</TableHead>
                            <TableHead className="text-right">Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {isLoading ? (
                            <TableRow>
                                <TableCell colSpan={9} className="h-32 text-center text-slate-400">Loading trips...</TableCell>
                            </TableRow>
                        ) : !data?.items?.length ? (
                            <TableRow>
                                <TableCell colSpan={9} className="h-32 text-center text-slate-500">
                                    No trips found. Create one to get started.
                                </TableCell>
                            </TableRow>
                        ) : (
                            data.items.map((trip: TripAdminDto) => (
                                <TableRow key={trip.id}>
                                    <TableCell>
                                        <div className="text-slate-900 dark:text-slate-200 font-medium">{trip.nameEn}</div>
                                        <div className="text-xs text-slate-500 mt-0.5">{trip.nameAr}</div>
                                    </TableCell>
                                    <TableCell>
                                        <span className="text-xs text-slate-600 bg-slate-100 px-2 py-1 rounded-full whitespace-nowrap">
                                            {TripType[trip.tripType] ?? `Type ${trip.tripType}`}
                                        </span>
                                    </TableCell>
                                    <TableCell>
                                        <div className="flex items-center gap-1.5 text-slate-500">
                                            <MapPin className="w-3.5 h-3.5 text-slate-400" />
                                            {trip.location || '—'}
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <div className="flex items-center gap-1.5 text-slate-500 text-sm">
                                            <Calendar className="w-3.5 h-3.5 text-slate-400" />
                                            {new Date(trip.startDate).toLocaleDateString()}
                                        </div>
                                        <div className="text-xs text-slate-400 pl-5">→ {new Date(trip.endDate).toLocaleDateString()}</div>
                                    </TableCell>
                                    <TableCell>
                                        <div className="flex items-center gap-1 text-slate-600 font-medium text-sm">
                                            <DollarSign className="w-3.5 h-3.5 text-emerald-500" />
                                            {formatCurrency(trip.pricePerPerson, selectedCurrency)}
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <div className="flex items-center gap-1.5 text-slate-500 text-sm">
                                            <Users className="w-3.5 h-3.5 text-slate-400" />
                                            {trip.maxCapacity ?? '∞'}
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <button
                                            onClick={() => setSelectedTrip(trip)}
                                            className="flex items-center gap-1.5 text-sm text-indigo-600 hover:text-indigo-800 transition-colors"
                                        >
                                            <Users className="w-3.5 h-3.5" />
                                            {trip.requestCount}
                                            <ChevronRight className="w-3 h-3" />
                                        </button>
                                    </TableCell>
                                    <TableCell>
                                        <StatusBadge variant={trip.isActive ? 'success' : 'warning'}>
                                            {trip.isActive ? 'Active' : 'Draft'}
                                        </StatusBadge>
                                    </TableCell>
                                    <TableCell className="text-right">
                                        <div className="flex justify-end gap-1">
                                            {trip.isActive ? (
                                                <Button size="icon" variant="ghost"
                                                    className="text-amber-500 hover:text-amber-700 hover:bg-amber-50"
                                                    onClick={() => setConfirmAction({ type: 'deactivate', trip })}
                                                    title="Deactivate">
                                                    <PowerOff className="w-4 h-4" />
                                                </Button>
                                            ) : (
                                                <Button size="icon" variant="ghost"
                                                    className="text-emerald-500 hover:text-emerald-700 hover:bg-emerald-50"
                                                    onClick={() => setConfirmAction({ type: 'activate', trip })}
                                                    title="Activate">
                                                    <Power className="w-4 h-4" />
                                                </Button>
                                            )}
                                            <Button size="icon" variant="ghost"
                                                className="text-blue-500 hover:text-blue-700 hover:bg-blue-50"
                                                onClick={() => setSelectedTrip(trip)}
                                                title="View Requests">
                                                <Eye className="w-4 h-4" />
                                            </Button>
                                            <Button size="icon" variant="ghost"
                                                className="text-red-500 hover:text-red-700 hover:bg-red-50"
                                                onClick={() => setConfirmAction({ type: 'delete', trip })}
                                                title="Delete">
                                                <Trash2 className="w-4 h-4" />
                                            </Button>
                                        </div>
                                    </TableCell>
                                </TableRow>
                            ))
                        )}
                    </TableBody>
                </Table>
            </DataTableShell>

            <CreateTripModal open={isCreateModalOpen} onOpenChange={setIsCreateModalOpen} />

            {selectedTrip && (
                <TripRequestsPanel
                    trip={selectedTrip}
                    currency={selectedCurrency}
                    open={!!selectedTrip}
                    onOpenChange={(open) => { if (!open) setSelectedTrip(null); }}
                />
            )}

            <ConfirmDialog
                open={!!confirmAction}
                onOpenChange={(open) => { if (!open) setConfirmAction(null); }}
                title={
                    confirmAction?.type === 'delete' ? 'Delete Trip'
                        : confirmAction?.type === 'activate' ? 'Activate Trip'
                            : 'Deactivate Trip'
                }
                description={
                    confirmAction?.type === 'delete'
                        ? `Are you sure you want to delete "${confirmAction?.trip.nameEn}"? This action cannot be undone.`
                        : confirmAction?.type === 'activate'
                            ? `Are you sure you want to activate "${confirmAction?.trip.nameEn}"? It will be visible to alumni.`
                            : `Are you sure you want to deactivate "${confirmAction?.trip.nameEn}"? Alumni will no longer see it.`
                }
                confirmLabel={confirmAction?.type === 'delete' ? 'Delete' : confirmAction?.type === 'activate' ? 'Activate' : 'Deactivate'}
                variant={confirmAction?.type === 'delete' ? 'danger' : 'default'}
                onConfirm={executeConfirmedAction}
                isLoading={deleteMutation.isPending || activateMutation.isPending || deactivateMutation.isPending}
            />
        </div>
    );
};

// ── Trip Requests Panel ────────────────────────────────────────────────────────

interface TripRequestsPanelProps {
    trip: TripAdminDto;
    currency: CurrencyDto | null;
    open: boolean;
    onOpenChange: (open: boolean) => void;
}

const TripRequestsPanel = ({ trip, currency, open, onOpenChange }: TripRequestsPanelProps) => {
    const queryClient = useQueryClient();

    const { data, isLoading } = useQuery({
        queryKey: ['trip-requests', trip.id],
        queryFn: () => adminService.getTripRequests(trip.id, { skipCount: 0, maxResultCount: 50 }),
        enabled: open
    });

    const approveMutation = useMutation({
        mutationFn: adminService.approveTripRequest,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['trip-requests', trip.id] });
            queryClient.invalidateQueries({ queryKey: ['admin-trips'] });
            toast.success('Request approved');
        },
        onError: () => toast.error('Failed to approve')
    });

    const rejectMutation = useMutation({
        mutationFn: adminService.rejectTripRequest,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['trip-requests', trip.id] });
            queryClient.invalidateQueries({ queryKey: ['admin-trips'] });
            toast.success('Request rejected');
        },
        onError: () => toast.error('Failed to reject')
    });

    const statusVariant = (status: number) => {
        if (status === 1) return 'success';
        if (status === 2) return 'destructive';
        if (status === 3) return 'default';
        return 'warning';
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="max-w-3xl max-h-[85vh] overflow-y-auto">
                <DialogHeader>
                    <DialogTitle className="flex items-center gap-2">
                        <Users className="w-5 h-5 text-indigo-500" />
                        Booking Requests — {trip.nameEn}
                    </DialogTitle>
                </DialogHeader>

                {isLoading ? (
                    <p className="text-slate-400 text-sm py-8 text-center">Loading requests...</p>
                ) : !data?.items?.length ? (
                    <p className="text-slate-500 text-sm py-8 text-center">No booking requests yet for this trip.</p>
                ) : (
                    <Table>
                        <TableHeader>
                            <TableRow>
                                <TableHead>Alumni Info</TableHead>
                                <TableHead>Guests</TableHead>
                                <TableHead>Total Amount</TableHead>
                                <TableHead>Submitted</TableHead>
                                <TableHead>Status</TableHead>
                                <TableHead className="text-right">Action</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {data.items.map((req: TripRequestAdminDto) => (
                                <TableRow key={req.id}>
                                    <TableCell>
                                        <div className="flex flex-col">
                                            <span className="font-semibold text-slate-800 text-sm">{req.alumniName}</span>
                                            <span className="text-xs text-slate-500">{req.alumniEmail}</span>
                                            <span className="text-xs text-slate-400 mt-0.5">{req.phoneNumber}</span>
                                        </div>
                                    </TableCell>
                                    <TableCell className="text-slate-600 text-sm">
                                        +{req.guestCount} guests
                                    </TableCell>
                                    <TableCell className="text-emerald-600 font-medium text-sm">
                                        {formatCurrency(req.totalAmount, currency)}
                                    </TableCell>
                                    <TableCell className="text-xs text-slate-500">
                                        <div className="flex items-center gap-1">
                                            <Clock className="w-3 h-3" />
                                            {new Date(req.creationTime).toLocaleDateString()}
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <StatusBadge variant={statusVariant(req.status) as 'success' | 'destructive' | 'default' | 'warning'}>
                                            {RequestStatusLabel[req.status] ?? 'Unknown'}
                                        </StatusBadge>
                                    </TableCell>
                                    <TableCell className="text-right">
                                        {req.status === 0 && (
                                            <div className="flex items-center justify-end gap-1">
                                                <Button
                                                    size="sm"
                                                    className="bg-emerald-600 hover:bg-emerald-500 text-white h-7 text-xs"
                                                    onClick={() => approveMutation.mutate(req.id)}
                                                    isLoading={approveMutation.isPending}
                                                >
                                                    <CheckCircle className="w-3.5 h-3.5 mr-1" /> Approve
                                                </Button>
                                                <Button
                                                    size="sm"
                                                    variant="ghost"
                                                    className="text-red-500 hover:text-red-700 hover:bg-red-50 h-7 text-xs"
                                                    onClick={() => rejectMutation.mutate(req.id)}
                                                    isLoading={rejectMutation.isPending}
                                                >
                                                    <XCircle className="w-3.5 h-3.5 mr-1" /> Reject
                                                </Button>
                                            </div>
                                        )}
                                    </TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                )}
            </DialogContent>
        </Dialog>
    );
};

export default TripsManager;
