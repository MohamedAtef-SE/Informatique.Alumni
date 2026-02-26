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
import type { CreateCommercialDiscountDto, CommercialDiscountDto } from '../../../types/benefits';

const discountSchema = z.object({
    providerName: z.string().min(2, "Provider name is required"),
    title: z.string().min(3, "Title is required"),
    description: z.string().min(5, "Description is required"),
    discountPercentage: z.number().min(1).max(100),
    promoCode: z.string().optional(),
    validUntil: z.string().min(1, "Expiration date is required"),
    websiteUrl: z.string().url().optional().or(z.literal(''))
});

type DiscountFormValues = z.infer<typeof discountSchema>;

interface EditDiscountModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    discount: CommercialDiscountDto | null;
}

export function EditDiscountModal({ open, onOpenChange, discount }: EditDiscountModalProps) {
    const queryClient = useQueryClient();
    const { register, handleSubmit, formState: { errors }, reset } = useForm<DiscountFormValues>({
        // @ts-ignore
        resolver: zodResolver(discountSchema),
        defaultValues: {
            providerName: '',
            title: '',
            description: '',
            discountPercentage: 0,
            promoCode: '',
            websiteUrl: ''
        }
    });

    useEffect(() => {
        if (discount && open) {
            reset({
                providerName: discount.providerName,
                title: discount.title,
                description: discount.description,
                discountPercentage: discount.discountPercentage,
                promoCode: discount.promoCode || '',
                validUntil: discount.validUntil ? discount.validUntil.split('T')[0] : '',
                websiteUrl: discount.websiteUrl || ''
            });
        }
    }, [discount, open, reset]);

    const updateMutation = useMutation({
        mutationFn: (data: CreateCommercialDiscountDto) => {
            if (!discount) throw new Error("No discount specified");
            return adminService.updateDiscount(discount.id, data);
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-discounts'] });
            toast.success('Discount updated successfully');
            onOpenChange(false);
        },
        onError: () => {
            toast.error('Failed to update discount');
        }
    });

    const onSubmit = (data: DiscountFormValues) => {
        const dto: CreateCommercialDiscountDto = {
            ...data,
            promoCode: data.promoCode || undefined,
            websiteUrl: data.websiteUrl || undefined
        };
        updateMutation.mutate(dto);
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="max-w-2xl">
                <DialogHeader>
                    <DialogTitle>Edit Discount Offer</DialogTitle>
                    <DialogDescription>Update details for this discount or offer.</DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                    <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-700 dark:text-slate-200">Provider Name</label>
                            <Input {...register('providerName')} placeholder="e.g. Amazon, local store..." />
                            {errors.providerName && <p className="text-red-500 text-xs">{errors.providerName.message}</p>}
                        </div>
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-700 dark:text-slate-200">Offer Title</label>
                            <Input {...register('title')} placeholder="e.g. 20% Off Electronics" />
                            {errors.title && <p className="text-red-500 text-xs">{errors.title.message}</p>}
                        </div>
                    </div>

                    <div className="space-y-2">
                        <label className="text-sm font-medium text-slate-700 dark:text-slate-200">Description</label>
                        <textarea
                            className="flex min-h-[60px] w-full rounded-md border border-slate-200 dark:border-white/10 bg-white dark:bg-slate-950 px-3 py-2 text-sm text-slate-900 dark:text-white ring-offset-background placeholder:text-slate-400 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                            {...register('description')}
                            placeholder="Details about the offer..."
                        />
                        {errors.description && <p className="text-red-500 text-xs">{errors.description.message}</p>}
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-700 dark:text-slate-200">Discount (%)</label>
                            <Input type="number" {...register('discountPercentage', { valueAsNumber: true })} />
                            {errors.discountPercentage && <p className="text-red-500 text-xs">{errors.discountPercentage.message}</p>}
                        </div>
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-700 dark:text-slate-200">Promo Code (Optional)</label>
                            <Input {...register('promoCode')} placeholder="SUMMER2024" />
                        </div>
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-700 dark:text-slate-200">Valid Until</label>
                            <Input type="date" {...register('validUntil')} />
                            {errors.validUntil && <p className="text-red-500 text-xs">{errors.validUntil.message}</p>}
                        </div>
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-700 dark:text-slate-200">Website URL (Optional)</label>
                            <Input {...register('websiteUrl')} placeholder="https://..." />
                            {errors.websiteUrl && <p className="text-red-500 text-xs">{errors.websiteUrl.message}</p>}
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
