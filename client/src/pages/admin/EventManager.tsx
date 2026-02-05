import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { Plus, Edit, Trash2, Globe, Search, Calendar, MapPin } from 'lucide-react';
import type { EventListDto } from '../../types/events';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/Table';
import { Button } from '../../components/ui/Button';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/Card';
import { cn } from '../../utils/cn';
import { toast } from 'sonner';

const EventManager = () => {
    const queryClient = useQueryClient();
    const [filter, setFilter] = useState('');

    const { data } = useQuery({
        queryKey: ['admin-events', filter],
        queryFn: () => adminService.getEvents({ filter, maxResultCount: 50 })
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

    const handleDelete = (id: string) => {
        if (confirm('Are you sure you want to delete this event? This cannot be undone.')) {
            deleteMutation.mutate(id);
        }
    };

    const handlePublish = (id: string) => {
        if (confirm('Are you sure you want to publish this event? It will be visible to all alumni.')) {
            publishMutation.mutate(id);
        }
    };

    return (
        <div className="space-y-8 animate-fade-in">
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
                <div>
                    <h1 className="text-3xl font-heading font-bold text-white">
                        Event Manager
                    </h1>
                    <p className="text-slate-400 mt-1">Create and manage events for the alumni community.</p>
                </div>
                <Button className="shadow-neon">
                    <Plus className="w-4 h-4 mr-2" /> New Event
                </Button>
            </div>

            <Card variant="glass">
                <CardHeader className="flex flex-col md:flex-row gap-4 justify-between items-start md:items-center border-b border-white/5 pb-6">
                    <CardTitle>Events List</CardTitle>
                    <div className="relative group w-full md:w-72">
                        <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-500 group-focus-within:text-accent transition-colors" />
                        <input
                            type="text"
                            placeholder="Search events..."
                            className="w-full bg-slate-900/50 border border-white/10 rounded-lg pl-10 pr-4 py-2 text-sm text-white focus:outline-none focus:border-accent focus:ring-1 focus:ring-accent transition-all"
                            value={filter}
                            onChange={(e) => setFilter(e.target.value)}
                        />
                    </div>
                </CardHeader>
                <CardContent className="p-0">
                    <Table>
                        <TableHeader>
                            <TableRow className="hover:bg-transparent border-white/5">
                                <TableHead className="w-[30%]">Event Name</TableHead>
                                <TableHead>Date</TableHead>
                                <TableHead>Location</TableHead>
                                <TableHead>Status</TableHead>
                                <TableHead className="text-right">Actions</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {data?.items.length === 0 ? (
                                <TableRow>
                                    <TableCell colSpan={5} className="h-32 text-center text-slate-500">
                                        No events found.
                                    </TableCell>
                                </TableRow>
                            ) : (
                                data?.items.map((event: EventListDto) => (
                                    <TableRow key={event.id}>
                                        <TableCell>
                                            <div className="text-white font-medium">{event.nameEn}</div>
                                            <div className="text-xs text-slate-500 mt-0.5">{event.code}</div>
                                        </TableCell>
                                        <TableCell>
                                            <div className="flex items-center gap-2 text-slate-300">
                                                <Calendar className="w-4 h-4 text-slate-500" />
                                                {new Date(event.lastSubscriptionDate).toLocaleDateString()}
                                            </div>
                                        </TableCell>
                                        <TableCell>
                                            <div className="flex items-center gap-2 text-slate-300">
                                                <MapPin className="w-4 h-4 text-slate-500" />
                                                {event.location}
                                            </div>
                                        </TableCell>
                                        <TableCell>
                                            <span className={cn(
                                                "px-2.5 py-1 rounded-full text-xs font-bold border",
                                                event.isPublished
                                                    ? "bg-emerald-500/20 text-emerald-500 border-emerald-500/20"
                                                    : "bg-amber-500/20 text-amber-500 border-amber-500/20"
                                            )}>
                                                {event.isPublished ? 'Published' : 'Draft'}
                                            </span>
                                        </TableCell>
                                        <TableCell className="text-right">
                                            <div className="flex justify-end gap-2">
                                                {!event.isPublished && (
                                                    <Button
                                                        size="icon"
                                                        variant="ghost"
                                                        className="text-blue-400 hover:text-blue-300 hover:bg-blue-500/20"
                                                        onClick={() => handlePublish(event.id)}
                                                        disabled={publishMutation.isPending}
                                                        title="Publish"
                                                    >
                                                        <Globe className="w-4 h-4" />
                                                    </Button>
                                                )}
                                                <Button size="icon" variant="ghost" className="text-slate-400 hover:text-white hover:bg-white/10" title="Edit">
                                                    <Edit className="w-4 h-4" />
                                                </Button>
                                                <Button
                                                    size="icon"
                                                    variant="ghost"
                                                    className="text-red-400 hover:text-red-300 hover:bg-red-500/20"
                                                    onClick={() => handleDelete(event.id)}
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
                </CardContent>
            </Card>
        </div>
    );
};

export default EventManager;
