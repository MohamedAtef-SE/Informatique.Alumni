import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { Button } from '../../ui/Button';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '../../ui/Dialog';
import { Input } from '../../ui/Input';
import { adminService } from '../../../services/adminService';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import type { CreateUpdateCareerServiceTypeDto } from '../../../types/career';

const typeSchema = z.object({
    nameEn: z.string().min(3, "English name is required"),
    nameAr: z.string().min(3, "Arabic name is required"),
    isActive: z.boolean().default(true),
});

type TypeFormValues = z.infer<typeof typeSchema>;

interface CreateCareerServiceTypeModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
}

export function CreateCareerServiceTypeModal({ open, onOpenChange }: CreateCareerServiceTypeModalProps) {
    const queryClient = useQueryClient();

    const { register, handleSubmit, formState: { errors }, reset } = useForm<TypeFormValues>({
        resolver: zodResolver(typeSchema),
        defaultValues: {
            isActive: true,
        }
    });

    const createMutation = useMutation({
        mutationFn: (data: CreateUpdateCareerServiceTypeDto) => adminService.createCareerServiceType(data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-career-types'] });
            toast.success('Career Category created successfully');
            onOpenChange(false);
            reset();
        },
        onError: (error: any) => {
            const errorMsg = error.response?.data?.error?.message || 'Failed to create category';
            toast.error(errorMsg);
        }
    });

    const onSubmit = (data: TypeFormValues) => {
        createMutation.mutate(data);
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="max-w-md">
                <DialogHeader>
                    <DialogTitle>Create Career Category</DialogTitle>
                    <DialogDescription>Add a new service type for organizing career services.</DialogDescription>
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
                                id="isActiveType"
                                {...register('isActive')}
                                className="h-4 w-4 rounded border-white/10 bg-slate-950 text-accent focus:ring-accent"
                            />
                            <label htmlFor="isActiveType" className="text-sm font-medium text-slate-700 dark:text-slate-200">Active</label>
                        </div>
                    </div>

                    <DialogFooter>
                        <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
                        <Button type="submit" isLoading={createMutation.isPending}>Create Category</Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
