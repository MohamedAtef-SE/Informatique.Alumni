import React, { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { Button } from '../../ui/Button';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '../../ui/Dialog';
import { Input } from '../../ui/Input';
import { adminService } from '../../../services/adminService';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { type UpdateMedicalPartnerDto, MedicalPartnerType, type MedicalPartnerDto } from '../../../types/health';

const partnerSchema = z.object({
    name: z.string().min(2, "Name is required"),
    description: z.string().optional(),
    type: z.coerce.number().min(0, "Type is required"),
    address: z.string().min(5, "Address is required"),
    contactNumber: z.string().min(5, "Contact number is required"),
    website: z.string().url().optional().or(z.literal('')),
    city: z.string().optional(),
    region: z.string().optional(),
    email: z.string().email().optional().or(z.literal('')),
    hotlineNumber: z.string().optional(),
    category: z.string().optional()
});

type PartnerFormValues = z.infer<typeof partnerSchema>;

interface EditMedicalPartnerModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    partner: MedicalPartnerDto | null;
}

export function EditMedicalPartnerModal({ open, onOpenChange, partner }: EditMedicalPartnerModalProps) {
    const queryClient = useQueryClient();
    const { register, handleSubmit, formState: { errors }, reset } = useForm<PartnerFormValues>({
        // @ts-ignore
        resolver: zodResolver(partnerSchema),
    });

    useEffect(() => {
        if (partner && open) {
            reset({
                name: partner.name,
                description: partner.description || '',
                type: partner.type,
                address: partner.address,
                contactNumber: partner.contactNumber,
                website: partner.website || '',
                city: partner.city || '',
                region: partner.region || '',
                email: partner.email || '',
                hotlineNumber: partner.hotlineNumber || '',
                category: partner.category || ''
            });
        }
    }, [partner, open, reset]);

    const updateMutation = useMutation({
        mutationFn: (data: UpdateMedicalPartnerDto) => {
            if (!partner?.id) throw new Error("No partner ID");
            return adminService.updateMedicalPartner(partner.id, data);
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-medical-partners'] });
            toast.success('Medical Partner updated');
            onOpenChange(false);
        },
        onError: () => toast.error('Failed to update partner')
    });

    const onSubmit = (data: PartnerFormValues) => {
        const dto: UpdateMedicalPartnerDto = {
            ...data,
            website: data.website || undefined,
            email: data.email || undefined
        };
        updateMutation.mutate(dto);
    };

    if (!partner) return null;

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
                <DialogHeader>
                    <DialogTitle>Edit Medical Partner</DialogTitle>
                    <DialogDescription>Update details for {partner.name}.</DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit(onSubmit as any)} className="space-y-4">
                    <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-800 dark:text-slate-200">Name</label>
                            <Input {...register('name')} placeholder="e.g. Al-Salam Hospital" />
                            {errors.name && <p className="text-red-500 text-xs">{errors.name.message}</p>}
                        </div>
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-800 dark:text-slate-200">Type</label>
                            <select
                                {...register('type')}
                                className="flex h-10 w-full rounded-md border border-slate-200 dark:border-white/10 bg-white dark:bg-slate-950 px-3 py-2 text-sm text-slate-900 dark:text-white placeholder:text-slate-400 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 disabled:cursor-not-allowed disabled:opacity-50"
                            >
                                <option value={MedicalPartnerType.Hospital}>Hospital</option>
                                <option value={MedicalPartnerType.Clinic}>Clinic</option>
                                <option value={MedicalPartnerType.Pharmacy}>Pharmacy</option>
                                <option value={MedicalPartnerType.Lab}>Lab</option>
                                <option value={MedicalPartnerType.Other}>Other</option>
                            </select>
                        </div>
                    </div>

                    <div className="space-y-2">
                        <label className="text-sm font-medium text-slate-800 dark:text-slate-200">Description</label>
                        <Input {...register('description')} placeholder="Specialties, working hours..." />
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-800 dark:text-slate-200">Address</label>
                            <Input {...register('address')} placeholder="123 Main St" />
                            {errors.address && <p className="text-red-500 text-xs">{errors.address.message}</p>}
                        </div>
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-800 dark:text-slate-200">City</label>
                            <Input {...register('city')} placeholder="Cairo" />
                        </div>
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-800 dark:text-slate-200">Contact Number</label>
                            <Input {...register('contactNumber')} placeholder="+20..." />
                            {errors.contactNumber && <p className="text-red-500 text-xs">{errors.contactNumber.message}</p>}
                        </div>
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-800 dark:text-slate-200">Hotline</label>
                            <Input {...register('hotlineNumber')} placeholder="19xxx" />
                        </div>
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-800 dark:text-slate-200">Email</label>
                            <Input {...register('email')} type="email" placeholder="contact@hospital.com" />
                            {errors.email && <p className="text-red-500 text-xs">{errors.email.message}</p>}
                        </div>
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-800 dark:text-slate-200">Website</label>
                            <Input {...register('website')} placeholder="https://..." />
                            {errors.website && <p className="text-red-500 text-xs">{errors.website.message}</p>}
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
