import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { Button } from '../../ui/Button';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '../../ui/Dialog';
import { Input } from '../../ui/Input';
import { adminService } from '../../../services/adminService';
import { useEffect } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import type { CreateAcademicGrantDto, AcademicGrantDto } from '../../../types/benefits';

const grantSchema = z.object({
    nameEn: z.string().min(3, "English name is required"),
    nameAr: z.string().min(3, "Arabic name is required"),
    type: z.string().min(1, "Type is required"),
    percentage: z.number().min(1, "Percentage must be at least 1").max(100, "Percentage cannot exceed 100")
});

type GrantFormValues = z.infer<typeof grantSchema>;

interface EditGrantModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    grant: AcademicGrantDto | null;
}

export function EditGrantModal({ open, onOpenChange, grant }: EditGrantModalProps) {
    const queryClient = useQueryClient();
    const { register, handleSubmit, formState: { errors }, reset } = useForm<GrantFormValues>({
        // @ts-ignore
        resolver: zodResolver(grantSchema),
        defaultValues: {
            nameEn: '',
            nameAr: '',
            type: 'Partial',
            percentage: 0
        }
    });

    useEffect(() => {
        if (grant && open) {
            reset({
                nameEn: grant.nameEn,
                nameAr: grant.nameAr,
                type: grant.type,
                percentage: grant.percentage
            });
        }
    }, [grant, open, reset]);

    const updateMutation = useMutation({
        mutationFn: (data: CreateAcademicGrantDto) => {
            if (!grant) throw new Error("No grant specified");
            return adminService.updateGrant(grant.id, data);
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-grants'] });
            toast.success('Grant updated successfully');
            onOpenChange(false);
        },
        onError: () => {
            toast.error('Failed to update grant');
        }
    });

    const onSubmit = (data: GrantFormValues) => {
        updateMutation.mutate(data);
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>Edit Academic Grant</DialogTitle>
                    <DialogDescription>Update details for this scholarship.</DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                    <div className="space-y-2">
                        <label className="text-sm font-medium text-slate-700 dark:text-slate-200">English Name</label>
                        <Input {...register('nameEn')} placeholder="e.g. Merit Scholarship" />
                        {errors.nameEn && <p className="text-red-500 text-xs">{errors.nameEn.message}</p>}
                    </div>

                    <div className="space-y-2">
                        <label className="text-sm font-medium text-slate-700 dark:text-slate-200">Arabic Name</label>
                        <Input {...register('nameAr')} placeholder="منحة التفوق..." className="text-right" />
                        {errors.nameAr && <p className="text-red-500 text-xs">{errors.nameAr.message}</p>}
                    </div>

                    <div className="space-y-2">
                        <label className="text-sm font-medium text-slate-700 dark:text-slate-200">Type</label>
                        <select
                            {...register('type')}
                            className="flex h-10 w-full rounded-md border border-slate-200 dark:border-white/10 bg-white dark:bg-slate-950 px-3 py-2 text-sm text-slate-900 dark:text-white ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-slate-400 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                        >
                            <option value="Partial">Partial Coverage</option>
                            <option value="Full">Full Coverage</option>
                        </select>
                        {errors.type && <p className="text-red-500 text-xs">{errors.type.message}</p>}
                    </div>

                    <div className="space-y-2">
                        <label className="text-sm font-medium text-slate-700 dark:text-slate-200">Percentage Coverage (%)</label>
                        <Input type="number" {...register('percentage', { valueAsNumber: true })} />
                        {errors.percentage && <p className="text-red-500 text-xs">{errors.percentage.message}</p>}
                    </div>

                    <DialogFooter>
                        <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
                        <Button type="submit" isLoading={updateMutation.isPending}>Save Changes</Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
