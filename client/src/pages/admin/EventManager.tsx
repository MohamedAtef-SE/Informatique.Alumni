import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { Plus, Edit, Trash2, Globe, Calendar, MapPin } from 'lucide-react';
import type { EventListDto } from '../../types/events';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/Table';
import { Button } from '../../components/ui/Button';
import { toast } from 'sonner';
import { PageHeader } from '../../components/admin/PageHeader';
import { DataTableShell } from '../../components/admin/DataTableShell';
import { StatusBadge } from '../../components/admin/StatusBadge';
import { CreateEventModal } from '../../components/admin/events/CreateEventModal';
import { EditEventModal } from '../../components/admin/events/EditEventModal';
import { ConfirmDialog } from '../../components/ui/ConfirmDialog';

const EventManager = () => {
    const queryClient = useQueryClient();
    const [filter, setFilter] = useState('');
    const [page, setPage] = useState(1);
    const pageSize = 10;
    const [statusFilter, setStatusFilter] = useState<'all' | 'published' | 'draft'>('all');
    const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
    const [editingEventId, setEditingEventId] = useState<string | null>(null);
    const [confirmAction, setConfirmAction] = useState<{
        type: 'publish' | 'delete';
        eventId: string;
        eventName: string;
    } | null>(null);

    const { data, isLoading } = useQuery({
        queryKey: ['admin-events', filter, page, statusFilter],
        queryFn: async () => {
            // Client-side filtering for status if backend doesn't support it directly in search
            // Assuming backend supports basic filtering string
            const result = await adminService.getEvents({
                filter,
                skipCount: (page - 1) * pageSize,
                maxResultCount: pageSize
            });
            return result;
        }
    });

    const publishMutation = useMutation({
        mutationFn: adminService.publishEvent,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-events'] });
            toast.success('Event Published!');
        },
        onError: () => {
            toast.error('Failed to publish event.');
        }
    });

    const deleteMutation = useMutation({
        mutationFn: adminService.deleteEvent,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-events'] });
            toast.success('Event Deleted.');
        },
        onError: () => {
            toast.error('Failed to delete event.');
        }
    });

    const handleDelete = (id: string, name: string) => {
        setConfirmAction({ type: 'delete', eventId: id, eventName: name });
    };

    const handlePublish = (id: string, name: string) => {
        setConfirmAction({ type: 'publish', eventId: id, eventName: name });
    };

    const executeConfirmedAction = () => {
        if (!confirmAction) return;
        if (confirmAction.type === 'delete') {
            deleteMutation.mutate(confirmAction.eventId);
        } else {
            publishMutation.mutate(confirmAction.eventId);
        }
        setConfirmAction(null);
    };

    const filteredItems = data?.items.filter(item => {
        if (statusFilter === 'all') return true;
        if (statusFilter === 'published') return item.isPublished;
        if (statusFilter === 'draft') return !item.isPublished;
        return true;
    });

    return (
        <div className="space-y-8 animate-in fade-in duration-500">
            <PageHeader
                title="Event Manager"
                description="Create and manage events for the alumni community."
                action={
                    <Button onClick={() => setIsCreateModalOpen(true)} className="shadow-neon">
                        <Plus className="w-4 h-4 mr-2" /> New Event
                    </Button>
                }
            />

            {/* Status Tabs */}
            <div className="flex space-x-2 border-b border-white/10 pb-4">
                {[
                    { label: 'All Events', value: 'all' },
                    { label: 'Published', value: 'published' },
                    { label: 'Drafts', value: 'draft' }
                ].map((tab) => (
                    <Button
                        key={tab.label}
                        variant={statusFilter === tab.value ? 'default' : 'ghost'}
                        size="sm"
                        onClick={() => {
                            setStatusFilter(tab.value as any);
                            setPage(1);
                        }}
                    >
                        {tab.label}
                    </Button>
                ))}
            </div>

            <DataTableShell
                searchPlaceholder="Search events..."
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
                            <TableHead className="w-[30%]">Event Name</TableHead>
                            <TableHead>Date</TableHead>
                            <TableHead>Location</TableHead>
                            <TableHead>Status</TableHead>
                            <TableHead className="text-right">Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {isLoading ? (
                            <TableRow>
                                <TableCell colSpan={5} className="h-32 text-center text-slate-400">Loading events...</TableCell>
                            </TableRow>
                        ) : filteredItems?.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={5} className="h-32 text-center text-slate-500">
                                    No events found.
                                </TableCell>
                            </TableRow>
                        ) : (
                            filteredItems?.map((event: EventListDto) => (
                                <TableRow key={event.id}>
                                    <TableCell>
                                        <div className="text-slate-900 dark:text-slate-200 font-medium">{event.nameEn}</div>
                                        <div className="text-xs text-slate-500 mt-0.5">{event.code}</div>
                                    </TableCell>
                                    <TableCell>
                                        <div className="flex items-center gap-2 text-slate-500">
                                            <Calendar className="w-4 h-4 text-slate-400" />
                                            {event.startDate && !event.startDate.startsWith('0001-01-01')
                                                ? new Date(event.startDate).toLocaleDateString()
                                                : (event.lastSubscriptionDate && !event.lastSubscriptionDate.startsWith('0001-01-01') ? new Date(event.lastSubscriptionDate).toLocaleDateString() : 'N/A')}
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <div className="flex items-center gap-2 text-slate-500">
                                            <MapPin className="w-4 h-4 text-slate-400" />
                                            {event.location}
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <StatusBadge variant={event.isPublished ? 'success' : 'warning'}>
                                            {event.isPublished ? 'Published' : 'Draft'}
                                        </StatusBadge>
                                    </TableCell>
                                    <TableCell className="text-right">
                                        <div className="flex justify-end gap-2">
                                            {!event.isPublished && (
                                                <Button
                                                    size="icon"
                                                    variant="ghost"
                                                    className="text-blue-500 hover:text-blue-700 hover:bg-blue-50"
                                                    onClick={() => handlePublish(event.id, event.nameEn)}
                                                    disabled={publishMutation.isPending}
                                                    title="Publish"
                                                >
                                                    <Globe className="w-4 h-4" />
                                                </Button>
                                            )}
                                            <Button
                                                size="icon"
                                                variant="ghost"
                                                className="text-slate-400 hover:text-slate-600 hover:bg-slate-100"
                                                title="Edit"
                                                onClick={() => setEditingEventId(event.id)}
                                            >
                                                <Edit className="w-4 h-4" />
                                            </Button>
                                            <Button
                                                size="icon"
                                                variant="ghost"
                                                className="text-red-500 hover:text-red-700 hover:bg-red-50"
                                                onClick={() => handleDelete(event.id, event.nameEn)}
                                                disabled={deleteMutation.isPending}
                                                title="Delete"
                                            >
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

            <CreateEventModal open={isCreateModalOpen} onOpenChange={setIsCreateModalOpen} />
            <EditEventModal
                open={!!editingEventId}
                onOpenChange={(open) => { if (!open) setEditingEventId(null); }}
                eventId={editingEventId}
            />
            <ConfirmDialog
                open={!!confirmAction}
                onOpenChange={(open) => { if (!open) setConfirmAction(null); }}
                title={confirmAction?.type === 'delete' ? 'Delete Event' : 'Publish Event'}
                description={
                    confirmAction?.type === 'delete'
                        ? `Are you sure you want to delete "${confirmAction?.eventName}"? This action cannot be undone.`
                        : `Are you sure you want to publish "${confirmAction?.eventName}"? It will become visible to all alumni.`
                }
                confirmLabel={confirmAction?.type === 'delete' ? 'Delete' : 'Publish'}
                variant={confirmAction?.type === 'delete' ? 'danger' : 'default'}
                onConfirm={executeConfirmedAction}
                isLoading={deleteMutation.isPending || publishMutation.isPending}
            />
        </div>
    );
};

export default EventManager;
