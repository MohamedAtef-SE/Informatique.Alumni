import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { Button } from '../../ui/Button';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '../../ui/Dialog';
import { Input } from '../../ui/Input';
import { adminService } from '../../../services/adminService';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import type { CareerServiceTypeDto, CreateUpdateCareerServiceTypeDto } from '../../../types/career';

const typeSchema = z.object({
    nameEn: z.string().min(3, "English name is required"),
    nameAr: z.string().min(3, "Arabic name is required"),
    isActive: z.boolean().default(true),
});

type TypeFormValues = z.infer<typeof typeSchema>;

interface EditCareerServiceTypeModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    careerType: CareerServiceTypeDto | null;
}

export function EditCareerServiceTypeModal({ open, onOpenChange, careerType }: EditCareerServiceTypeModalProps) {
    const queryClient = useQueryClient();

    const { register, handleSubmit, formState: { errors }, reset } = useForm<TypeFormValues>({
        resolver: zodResolver(typeSchema),
    });

    useEffect(() => {
        if (careerType && open) {
            reset({
                nameEn: careerType.nameEn,
                nameAr: careerType.nameAr,
                isActive: careerType.isActive,
            });
        }
    }, [careerType, open, reset]);

    const updateMutation = useMutation({
        mutationFn: (data: CreateUpdateCareerServiceTypeDto) => 
            adminService.updateCareerServiceType(careerType!.id, data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-career-types'] });
            toast.success('Career Category updated successfully');
            onOpenChange(false);
        },
        onError: (error: any) => {
            const errorMsg = error.response?.data?.error?.message || 'Failed to update category';
            toast.error(errorMsg);
        }
    });

    const onSubmit = (data: TypeFormValues) => {
        updateMutation.mutate(data);
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="max-w-md">
                <DialogHeader>
                    <DialogTitle>Update Career Category</DialogTitle>
                    <DialogDescription>Modify the service type details.</DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
                    <div className="space-y-4">
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-700 dark:text-slate-200">English Name</label>
                            <Input {...register('nameEn')} placeholder="e.g. Workshop" />
                            {errors.nameEn && <p className="text-red-500 text-xs">{errors.nameEn.message}</p>}
                        </div>
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-700 dark:text-slate-200 text-right block">الاسم بالعربية</label>
                            <Input {...register('nameAr')} placeholder="مثلاً: ورشة عمل" className="text-right" dir="rtl" />
                            {errors.nameAr && <p className="text-red-500 text-xs text-right">{errors.nameAr.message}</p>}
                        </div>
                        <div className="flex items-center gap-2">
                            <input
                                type="checkbox"
                                id="isActiveEditType"
                                {...register('isActive')}
                                className="h-4 w-4 rounded border-white/10 bg-slate-950 text-accent focus:ring-accent"
                            />
                            <label htmlFor="isActiveEditType" className="text-sm font-medium text-slate-700 dark:text-slate-200">Active Status</label>
                        </div>
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
