import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { Button } from '../../ui/Button';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '../../ui/Dialog';
import { Input } from '../../ui/Input';
import { adminService } from '../../../services/adminService';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import type { CreateTripDto } from '../../../types/trips';

const tripSchema = z.object({
    title: z.string().min(3, "Title is required"),
    description: z.string().optional(),
    destination: z.string().min(2, "Destination is required"),
    startDate: z.string().min(1, "Start date is required"),
    endDate: z.string().min(1, "End date is required"),
    maxCapacity: z.coerce.number().min(1, "Capacity must be at least 1"),
    pricePerPerson: z.coerce.number().min(0, "Price must be positive"),
    isActive: z.boolean().default(true)
});

type TripFormValues = z.infer<typeof tripSchema>;

interface CreateTripModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
}

export function CreateTripModal({ open, onOpenChange }: CreateTripModalProps) {
    const queryClient = useQueryClient();
    const { register, handleSubmit, formState: { errors }, reset } = useForm<TripFormValues>({
        // @ts-ignore
        resolver: zodResolver(tripSchema),
        defaultValues: {
            title: '',
            description: '',
            destination: '',
            maxCapacity: 20,
            pricePerPerson: 0,
            isActive: true
        }
    });

    const createMutation = useMutation({
        mutationFn: (data: CreateTripDto) => adminService.createTrip(data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-trips'] });
            toast.success('Trip created successfully');
            onOpenChange(false);
            reset();
        },
        onError: () => toast.error('Failed to create trip')
    });

    const onSubmit = (data: TripFormValues) => {
        createMutation.mutate(data);
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
                <DialogHeader>
                    <DialogTitle>Create New Trip</DialogTitle>
                    <DialogDescription>Add a new travel or event trip for alumni.</DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit(onSubmit as any)} className="space-y-4">
                    <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-200">Trip Title</label>
                            <Input {...register('title')} placeholder="e.g. Summer Beach Camp" />
                            {errors.title && <p className="text-red-500 text-xs">{errors.title.message}</p>}
                        </div>
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-200">Destination</label>
                            <Input {...register('destination')} placeholder="e.g. Dahab" />
                            {errors.destination && <p className="text-red-500 text-xs">{errors.destination.message}</p>}
                        </div>
                    </div>

                    <div className="space-y-2">
                        <label className="text-sm font-medium text-slate-200">Description</label>
                        <Input {...register('description')} placeholder="Details about the trip..." />
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-200">Start Date</label>
                            <Input {...register('startDate')} type="datetime-local" />
                            {errors.startDate && <p className="text-red-500 text-xs">{errors.startDate.message}</p>}
                        </div>
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-200">End Date</label>
                            <Input {...register('endDate')} type="datetime-local" />
                            {errors.endDate && <p className="text-red-500 text-xs">{errors.endDate.message}</p>}
                        </div>
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-200">Max Capacity</label>
                            <Input {...register('maxCapacity')} type="number" />
                            {errors.maxCapacity && <p className="text-red-500 text-xs">{errors.maxCapacity.message}</p>}
                        </div>
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-200">Price Per Person</label>
                            <Input {...register('pricePerPerson')} type="number" />
                            {errors.pricePerPerson && <p className="text-red-500 text-xs">{errors.pricePerPerson.message}</p>}
                        </div>
                    </div>

                    <div className="flex items-center gap-2">
                        <input type="checkbox" {...register('isActive')} id="isActive" className="rounded border-gray-600 bg-slate-950 text-indigo-500" />
                        <label htmlFor="isActive" className="text-sm font-medium text-slate-200">Active / Published</label>
                    </div>

                    <DialogFooter>
                        <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
                        <Button type="submit" isLoading={createMutation.isPending}>Create Trip</Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
