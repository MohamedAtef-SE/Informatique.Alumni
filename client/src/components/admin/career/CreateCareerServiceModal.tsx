import { useForm, useFieldArray } from 'react-hook-form';

import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { Button } from '../../ui/Button';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '../../ui/Dialog';
import { Input } from '../../ui/Input';
import { adminService } from '../../../services/adminService';
import { useMutation, useQueryClient, useQuery } from '@tanstack/react-query';
import { toast } from 'sonner';
import type { CreateCareerServiceDto } from '../../../types/career';
import { Plus, Trash2 } from 'lucide-react';
import { useTranslation } from 'react-i18next';

const timeslotSchema = z.object({
    date: z.string().min(1, "Date is required"),
    startTime: z.string().min(1, "Start time is required"),
    endTime: z.string().min(1, "End time is required"),
    lecturerName: z.string().min(2, "Lecturer name is required"),
    room: z.string().min(1, "Room is required"),
    address: z.string().min(1, "Address is required"),
    capacity: z.number().min(1, "Capacity must be at least 1")
});

const serviceSchema = z.object({
    nameEn: z.string().min(3, "English name is required"),
    nameAr: z.string().min(3, "Arabic name is required"),
    code: z.string().min(2, "Code is required"),
    description: z.string().min(10, "Description must be at least 10 characters"),
    mapUrl: z.string().url("Invalid URL").optional().or(z.literal('')),
    hasFees: z.boolean().default(false),
    feeAmount: z.number().default(0),
    lastSubscriptionDate: z.string().min(1, "Date is required"),
    serviceTypeId: z.string().min(1, "Service Type is required"),
    branchId: z.string().min(1, "Branch is required"),
    timeslots: z.array(timeslotSchema).min(1, "At least one timeslot is required")
});

type ServiceFormValues = z.infer<typeof serviceSchema>;

interface CreateCareerServiceModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
}

export function CreateCareerServiceModal({ open, onOpenChange }: CreateCareerServiceModalProps) {
    const queryClient = useQueryClient();
    const { i18n } = useTranslation();

    const { data: lookups, isLoading: isLoadingLookups } = useQuery({
        queryKey: ['career-lookups'],
        queryFn: adminService.getCareerLookups,
        enabled: open
    });

    const { register, control, handleSubmit, formState: { errors }, reset, watch } = useForm<ServiceFormValues>({
        // @ts-ignore
        resolver: zodResolver(serviceSchema),
        defaultValues: {
            hasFees: false,
            feeAmount: 0,
            timeslots: [{
                date: '',
                startTime: '',
                endTime: '',
                lecturerName: '',
                room: '',
                address: '',
                capacity: 30
            }]
        }
    });

    const { fields, append, remove } = useFieldArray({
        control,
        name: "timeslots"
    });

    const hasFees = watch('hasFees');

    const createMutation = useMutation({
        mutationFn: (data: CreateCareerServiceDto) => adminService.createCareerService(data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-career'] });
            toast.success('Service created successfully');
            onOpenChange(false);
            reset();
        },
        onError: (error: any) => {
            const errorMsg = error.response?.data?.error?.message || 'Failed to create service';
            toast.error(errorMsg);
        }
    });

    const onSubmit = (data: ServiceFormValues) => {
        const dto: CreateCareerServiceDto = {
            ...data,
            mapUrl: data.mapUrl || undefined,
            feeAmount: data.hasFees ? data.feeAmount : 0
        };
        createMutation.mutate(dto);
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
                <DialogHeader>
                    <DialogTitle>Create Career Service</DialogTitle>
                    <DialogDescription>Add a new workshop, training, or career service.</DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit(onSubmit as any)} className="space-y-6">
                    <div className="space-y-4">
                        <h3 className="text-lg font-medium text-white border-b border-white/10 pb-2">Basic Information</h3>
                        <div className="grid grid-cols-2 gap-4">
                            <div className="space-y-2">
                                <label className="text-sm font-medium text-slate-700 dark:text-slate-200">English Name</label>
                                <Input {...register('nameEn')} placeholder="e.g. CV Writing Workshop" />
                                {errors.nameEn && <p className="text-red-500 text-xs">{errors.nameEn.message}</p>}
                            </div>
                            <div className="space-y-2">
                                <label className="text-sm font-medium text-slate-700 dark:text-slate-200">Arabic Name</label>
                                <Input {...register('nameAr')} placeholder="ورشة كتابة السيرة الذاتية" className="text-right" />
                                {errors.nameAr && <p className="text-red-500 text-xs">{errors.nameAr.message}</p>}
                            </div>
                        </div>

                        <div className="grid grid-cols-2 gap-4">
                            <div className="space-y-2">
                                <label className="text-sm font-medium text-slate-700 dark:text-slate-200">Code</label>
                                <Input {...register('code')} placeholder="WS-2024-001" />
                                {errors.code && <p className="text-red-500 text-xs">{errors.code.message}</p>}
                            </div>
                            <div className="space-y-2">
                                <label className="text-sm font-medium text-slate-700 dark:text-slate-200">Subscription Deadline</label>
                                <Input type="datetime-local" {...register('lastSubscriptionDate')} />
                                {errors.lastSubscriptionDate && <p className="text-red-500 text-xs">{errors.lastSubscriptionDate.message}</p>}
                            </div>
                        </div>

                        <div className="grid grid-cols-2 gap-4">
                            <div className="space-y-2">
                                <label className="text-sm font-medium text-slate-700 dark:text-slate-200">Service Type</label>
                                <select
                                    {...register('serviceTypeId')}
                                    className="flex h-10 w-full rounded-md border border-slate-200 dark:border-white/10 bg-white dark:bg-slate-950 px-3 py-2 text-sm text-slate-900 dark:text-white ring-offset-background disabled:cursor-not-allowed disabled:opacity-50"
                                    disabled={isLoadingLookups}
                                >
                                    <option value="">Select a Service Type</option>
                                    {lookups?.serviceTypes.map(st => (
                                        <option key={st.id} value={st.id}>
                                            {i18n.language.startsWith('ar') ? st.nameAr : st.nameEn}
                                        </option>
                                    ))}
                                </select>
                                {errors.serviceTypeId && <p className="text-red-500 text-xs">{errors.serviceTypeId.message}</p>}
                            </div>
                            <div className="space-y-2">
                                <label className="text-sm font-medium text-slate-700 dark:text-slate-200">Branch Location</label>
                                <select
                                    {...register('branchId')}
                                    className="flex h-10 w-full rounded-md border border-slate-200 dark:border-white/10 bg-white dark:bg-slate-950 px-3 py-2 text-sm text-slate-900 dark:text-white ring-offset-background disabled:cursor-not-allowed disabled:opacity-50"
                                    disabled={isLoadingLookups}
                                >
                                    <option value="">Select a Branch</option>
                                    {lookups?.branches.map(b => (
                                        <option key={b.id} value={b.id}>
                                            {i18n.language.startsWith('ar') ? b.nameAr : b.nameEn}
                                        </option>
                                    ))}
                                </select>
                                {errors.branchId && <p className="text-red-500 text-xs">{errors.branchId.message}</p>}
                            </div>
                        </div>

                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-700 dark:text-slate-200">Description</label>
                            <textarea
                                className="flex min-h-[80px] w-full rounded-md border border-slate-200 dark:border-white/10 bg-white dark:bg-slate-950 px-3 py-2 text-sm text-slate-900 dark:text-white ring-offset-background placeholder:text-slate-400 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                                {...register('description')}
                                placeholder="Service details..."
                            />
                            {errors.description && <p className="text-red-500 text-xs">{errors.description.message}</p>}
                        </div>

                        <div className="flex items-center gap-2">
                            <input
                                type="checkbox"
                                id="hasFeesCareer"
                                {...register('hasFees')}
                                className="h-4 w-4 rounded border-white/10 bg-slate-950 text-accent focus:ring-accent"
                            />
                            <label htmlFor="hasFeesCareer" className="text-sm font-medium text-slate-700 dark:text-slate-200">Requires Fees</label>
                        </div>

                        {hasFees && (
                            <div className="space-y-2 animate-in fade-in slide-in-from-top-2">
                                <label className="text-sm font-medium text-slate-700 dark:text-slate-200">Fee Amount (EGP)</label>
                                <Input type="number" {...register('feeAmount', { valueAsNumber: true })} />
                            </div>
                        )}
                    </div>

                    <div className="space-y-4">
                        <div className="flex justify-between items-center border-b border-white/10 pb-2">
                            <h3 className="text-lg font-medium text-white">Sessions / Timeslots</h3>
                            <Button type="button" size="sm" variant="outline" onClick={() => append({ date: '', startTime: '', endTime: '', lecturerName: '', room: '', address: '', capacity: 30 })}>
                                <Plus className="w-4 h-4 mr-2" /> Add Session
                            </Button>
                        </div>

                        {fields.map((field, index) => (
                            <div key={field.id} className="p-4 rounded-lg bg-white/5 space-y-4 relative">
                                <div className="absolute right-2 top-2">
                                    <Button type="button" size="icon" variant="ghost" className="text-red-400 hover:bg-red-500/10" onClick={() => remove(index)}>
                                        <Trash2 className="w-4 h-4" />
                                    </Button>
                                </div>
                                <h4 className="text-sm font-bold text-slate-300">Session {index + 1}</h4>
                                <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
                                    <div className="space-y-2">
                                        <label className="text-xs font-medium text-slate-400">Date</label>
                                        <Input type="date" {...register(`timeslots.${index}.date`)} />
                                        {errors.timeslots?.[index]?.date && <p className="text-red-500 text-xs">{errors.timeslots[index].date?.message}</p>}
                                    </div>
                                    <div className="space-y-2">
                                        <label className="text-xs font-medium text-slate-400">Start Time</label>
                                        <Input type="time" {...register(`timeslots.${index}.startTime`)} />
                                        {errors.timeslots?.[index]?.startTime && <p className="text-red-500 text-xs">{errors.timeslots[index].startTime?.message}</p>}
                                    </div>
                                    <div className="space-y-2">
                                        <label className="text-xs font-medium text-slate-400">End Time</label>
                                        <Input type="time" {...register(`timeslots.${index}.endTime`)} />
                                    </div>
                                    <div className="space-y-2">
                                        <label className="text-xs font-medium text-slate-400">Lecturer</label>
                                        <Input {...register(`timeslots.${index}.lecturerName`)} placeholder="Dr. Name" />
                                    </div>
                                    <div className="space-y-2">
                                        <label className="text-xs font-medium text-slate-400">Room/Place</label>
                                        <Input {...register(`timeslots.${index}.room`)} placeholder="Room 101" />
                                    </div>
                                    <div className="space-y-2">
                                        <label className="text-xs font-medium text-slate-400">Full Address</label>
                                        <Input {...register(`timeslots.${index}.address`)} placeholder="Building A..." />
                                    </div>
                                    <div className="space-y-2">
                                        <label className="text-xs font-medium text-slate-400">Capacity</label>
                                        <Input type="number" {...register(`timeslots.${index}.capacity`, { valueAsNumber: true })} />
                                    </div>
                                </div>
                            </div>
                        ))}
                        {errors.timeslots && <p className="text-red-500 text-xs">{errors.timeslots.message}</p>}
                    </div>

                    <DialogFooter>
                        <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
                        <Button type="submit" isLoading={createMutation.isPending}>Create Service</Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
