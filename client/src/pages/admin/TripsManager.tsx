import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { Plus, Plane, MapPin, Calendar, Users, DollarSign, Trash2 } from 'lucide-react';
import { PageHeader } from '../../components/admin/PageHeader';
import { DataTableShell } from '../../components/admin/DataTableShell';
import { Button } from '../../components/ui/Button';
import { CreateTripModal } from '../../components/admin/trips/CreateTripModal';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/Table';
import { toast } from 'sonner';
import type { AlumniTripDto } from '../../types/trips';
import { StatusBadge } from '../../components/admin/StatusBadge';

const TripsManager = () => {
    const queryClient = useQueryClient();
    const [page, setPage] = useState(1);
    const pageSize = 10;
    const [filter, setFilter] = useState('');
    const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);

    const { data, isLoading } = useQuery({
        queryKey: ['admin-trips', page, filter],
        queryFn: () => adminService.getTrips({
            skipCount: (page - 1) * pageSize,
            maxResultCount: pageSize,
            filter
        })
    });

    const deleteMutation = useMutation({
        mutationFn: adminService.deleteTrip,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-trips'] });
            toast.success('Trip deleted successfully');
        },
        onError: () => toast.error('Failed to delete trip')
    });

    const handleDelete = (id: string) => {
        if (confirm('Are you sure you want to delete this trip?')) {
            deleteMutation.mutate(id);
        }
    };

    return (
        <div className="space-y-8 animate-in fade-in duration-500">
            <PageHeader
                title="Trips Manager"
                description="Manage alumni trips, events, and travel packages."
                action={
                    <Button className="shadow-neon" onClick={() => setIsCreateModalOpen(true)}>
                        <Plus className="w-4 h-4 mr-2" /> New Trip
                    </Button>
                }
            />

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
                        <TableRow className="hover:bg-transparent border-white/5">
                            <TableHead>Title/Dest</TableHead>
                            <TableHead>Dates</TableHead>
                            <TableHead>Capacity</TableHead>
                            <TableHead>Price</TableHead>
                            <TableHead>Status</TableHead>
                            <TableHead className="text-right">Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {isLoading ? (
                            <TableRow>
                                <TableCell colSpan={6} className="h-32 text-center text-slate-400">Loading trips...</TableCell>
                            </TableRow>
                        ) : data?.items.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={6} className="h-32 text-center text-slate-500">
                                    No trips found.
                                </TableCell>
                            </TableRow>
                        ) : (
                            data?.items.map((trip: AlumniTripDto) => (
                                <TableRow key={trip.id} className="group">
                                    <TableCell>
                                        <div className="flex flex-col">
                                            <div className="font-medium text-white flex items-center gap-2">
                                                <Plane className="w-4 h-4 text-sky-400" />
                                                {trip.title}
                                            </div>
                                            <div className="text-xs text-slate-400 flex items-center gap-1 mt-1">
                                                <MapPin className="w-3 h-3" />
                                                {trip.destination}
                                            </div>
                                        </div>
                                    </TableCell>
                                    <TableCell className="text-slate-300 text-xs">
                                        <div className="flex flex-col gap-1">
                                            <div className="flex items-center gap-1">
                                                <Calendar className="w-3 h-3 text-slate-500" />
                                                {new Date(trip.startDate).toLocaleDateString()}
                                            </div>
                                            <div className="pl-4 text-slate-500">
                                                To {new Date(trip.endDate).toLocaleDateString()}
                                            </div>
                                        </div>
                                    </TableCell>
                                    <TableCell className="text-slate-300 text-sm">
                                        <div className="flex items-center gap-1">
                                            <Users className="w-4 h-4 text-slate-500" />
                                            {trip.maxCapacity}
                                        </div>
                                    </TableCell>
                                    <TableCell className="text-emerald-400 font-mono text-sm">
                                        <div className="flex items-center gap-1">
                                            <DollarSign className="w-3 h-3" />
                                            {trip.pricePerPerson}
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <StatusBadge variant={trip.isActive ? 'success' : 'default'}>
                                            {trip.isActive ? 'Active' : 'Draft'}
                                        </StatusBadge>
                                    </TableCell>
                                    <TableCell className="text-right">
                                        <Button
                                            size="icon"
                                            variant="ghost"
                                            className="text-red-400 hover:text-red-300 hover:bg-red-500/20 opacity-0 group-hover:opacity-100 transition-opacity"
                                            onClick={() => handleDelete(trip.id)}
                                        >
                                            <Trash2 className="w-4 h-4" />
                                        </Button>
                                    </TableCell>
                                </TableRow>
                            ))
                        )}
                    </TableBody>
                </Table>
            </DataTableShell>

            <CreateTripModal open={isCreateModalOpen} onOpenChange={setIsCreateModalOpen} />
        </div>
    );
};

export default TripsManager;
