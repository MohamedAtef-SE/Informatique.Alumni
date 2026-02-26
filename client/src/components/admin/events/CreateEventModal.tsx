import { useForm, useFieldArray } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { Button } from '../../ui/Button';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '../../ui/Dialog';
import { Input } from '../../ui/Input';
import { adminService } from '../../../services/adminService';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import type { CreateEventDto } from '../../../types/events';

const timeslotSchema = z.object({
    startTime: z.string().min(1, "Start time is required"),
    endTime: z.string().min(1, "End time is required"),
    capacity: z.number({ message: "Capacity is required" }).min(1, "Min capacity is 1"),
});

const eventSchema = z.object({
    nameEn: z.string().min(3, "English name is required"),
    nameAr: z.string().min(3, "Arabic name is required"),
    code: z.string().min(2, "Code is required"),
    description: z.string().min(10, "Description is required"),
    location: z.string().min(3, "Location is required"),
    lastSubscriptionDate: z.string().refine((val) => !isNaN(Date.parse(val)), "Invalid date"),
    hasFees: z.boolean().default(false),
    feeAmount: z.number().optional(),
    timeslots: z.array(timeslotSchema).min(1, "At least one timeslot is required"),
});

type EventFormValues = z.infer<typeof eventSchema>;

interface CreateEventModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
}

export function CreateEventModal({ open, onOpenChange }: CreateEventModalProps) {
    const queryClient = useQueryClient();
    const { register, handleSubmit, formState: { errors }, reset, watch, control } = useForm<EventFormValues>({
        // @ts-ignore - resolver type mismatch is common with zod/hookform versions
        resolver: zodResolver(eventSchema),
        defaultValues: {
            hasFees: false,
            nameEn: '',
            nameAr: '',
            code: '',
            description: '',
            location: '',
            lastSubscriptionDate: '',
            timeslots: [{ startTime: '', endTime: '', capacity: 50 }]
        }
    });

    const { fields, append, remove } = useFieldArray({ control, name: 'timeslots' });
    const hasFees = watch('hasFees');

    const createMutation = useMutation({
        mutationFn: (data: CreateEventDto) => adminService.createEvent(data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-events'] });
            toast.success('Event created successfully');
            onOpenChange(false);
            reset();
        },
        onError: (error: any) => {
            const backendMessage = error?.response?.data?.error?.message;
            if (backendMessage) {
                toast.error(backendMessage);
            } else {
                toast.error('Failed to create event');
            }
        }
    });

    const onSubmit = (data: EventFormValues) => {
        const dto: CreateEventDto = {
            ...data,
            timeslots: data.timeslots.map(t => ({
                startTime: t.startTime,
                endTime: t.endTime,
                capacity: t.capacity,
            })),
            feeAmount: data.hasFees ? data.feeAmount : 0
        };
        createMutation.mutate(dto);
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
                <DialogHeader>
                    <DialogTitle>Create New Event</DialogTitle>
                    <DialogDescription>Fill in the details to create a new alumni event.</DialogDescription>
                </DialogHeader>

                {/* @ts-ignore - type mismatch between hook-form and zod versions */}
                <form onSubmit={handleSubmit(onSubmit as any)} className="space-y-4">
                    <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-200">English Name</label>
                            <Input {...register('nameEn')} placeholder="e.g. Annual Reunion" />
                            {errors.nameEn && <p className="text-red-500 text-xs">{errors.nameEn.message}</p>}
                        </div>
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-200">Arabic Name</label>
                            <Input {...register('nameAr')} placeholder="اللقاء السنوي" className="text-right" />
                            {errors.nameAr && <p className="text-red-500 text-xs">{errors.nameAr.message}</p>}
                        </div>
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-200">Event Code</label>
                            <Input {...register('code')} placeholder="EVT-2024-001" />
                            {errors.code && <p className="text-red-500 text-xs">{errors.code.message}</p>}
                        </div>
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-200">Subscription Deadline</label>
                            <Input type="datetime-local" {...register('lastSubscriptionDate')} />
                            {errors.lastSubscriptionDate && <p className="text-red-500 text-xs">{errors.lastSubscriptionDate.message}</p>}
                        </div>
                    </div>

                    <div className="space-y-2">
                        <label className="text-sm font-medium text-slate-200">Location</label>
                        <Input {...register('location')} placeholder="Main Hall, Cairo University" />
                        {errors.location && <p className="text-red-500 text-xs">{errors.location.message}</p>}
                    </div>

                    <div className="space-y-2">
                        <label className="text-sm font-medium text-slate-200">Description</label>
                        <textarea
                            className="flex min-h-[80px] w-full rounded-md border border-white/10 bg-slate-950 px-3 py-2 text-sm text-white ring-offset-background placeholder:text-slate-400 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                            {...register('description')}
                            placeholder="Event details..."
                        />
                        {errors.description && <p className="text-red-500 text-xs">{errors.description.message}</p>}
                    </div>

                    {/* Timeslots Section */}
                    <div className="space-y-3">
                        <div className="flex items-center justify-between">
                            <label className="text-sm font-medium text-slate-200">Timeslots</label>
                            <Button
                                type="button"
                                variant="outline"
                                size="sm"
                                onClick={() => append({ startTime: '', endTime: '', capacity: 50 })}
                            >
                                + Add Timeslot
                            </Button>
                        </div>
                        {fields.map((field, index) => (
                            <div key={field.id} className="grid grid-cols-[1fr_1fr_100px_auto] gap-2 items-end p-3 rounded-lg border border-white/10 bg-slate-950/50">
                                <div className="space-y-1">
                                    <label className="text-xs text-slate-400">Start Time</label>
                                    <Input type="datetime-local" {...register(`timeslots.${index}.startTime`)} />
                                    {errors.timeslots?.[index]?.startTime && (
                                        <p className="text-red-500 text-xs">{errors.timeslots[index]?.startTime?.message}</p>
                                    )}
                                </div>
                                <div className="space-y-1">
                                    <label className="text-xs text-slate-400">End Time</label>
                                    <Input type="datetime-local" {...register(`timeslots.${index}.endTime`)} />
                                    {errors.timeslots?.[index]?.endTime && (
                                        <p className="text-red-500 text-xs">{errors.timeslots[index]?.endTime?.message}</p>
                                    )}
                                </div>
                                <div className="space-y-1">
                                    <label className="text-xs text-slate-400">Capacity</label>
                                    <Input type="number" {...register(`timeslots.${index}.capacity`, { valueAsNumber: true })} />
                                    {errors.timeslots?.[index]?.capacity && (
                                        <p className="text-red-500 text-xs">{errors.timeslots[index]?.capacity?.message}</p>
                                    )}
                                </div>
                                {fields.length > 1 && (
                                    <Button
                                        type="button"
                                        variant="ghost"
                                        size="sm"
                                        className="text-red-400 hover:text-red-300 hover:bg-red-950/30 mb-0.5"
                                        onClick={() => remove(index)}
                                    >
                                        ✕
                                    </Button>
                                )}
                            </div>
                        ))}
                        {errors.timeslots?.message && (
                            <p className="text-red-500 text-xs">{errors.timeslots.message}</p>
                        )}
                    </div>

                    <div className="flex items-center gap-2">
                        <input
                            type="checkbox"
                            id="hasFees"
                            {...register('hasFees')}
                            className="h-4 w-4 rounded border-white/10 bg-slate-950 text-accent focus:ring-accent"
                        />
                        <label htmlFor="hasFees" className="text-sm font-medium text-slate-200">This event has attendance fees</label>
                    </div>

                    {hasFees && (
                        <div className="space-y-2 animate-in fade-in slide-in-from-top-2">
                            <label className="text-sm font-medium text-slate-200">Fee Amount (EGP)</label>
                            <Input type="number" {...register('feeAmount', { valueAsNumber: true })} />
                        </div>
                    )}

                    <DialogFooter>
                        <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
                        <Button type="submit" isLoading={createMutation.isPending}>Create Event</Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
