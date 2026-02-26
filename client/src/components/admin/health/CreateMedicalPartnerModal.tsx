import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { Button } from '../../ui/Button';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '../../ui/Dialog';
import { Input } from '../../ui/Input';
import { adminService } from '../../../services/adminService';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { type CreateMedicalPartnerDto, MedicalPartnerType } from '../../../types/health';

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

interface CreateMedicalPartnerModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
}

export function CreateMedicalPartnerModal({ open, onOpenChange }: CreateMedicalPartnerModalProps) {
    const queryClient = useQueryClient();
    const { register, handleSubmit, formState: { errors }, reset } = useForm<PartnerFormValues>({
        // @ts-ignore
        resolver: zodResolver(partnerSchema),
        defaultValues: {
            type: MedicalPartnerType.Clinic,
            name: '',
            address: '',
            contactNumber: ''
        }
    });

    const createMutation = useMutation({
        mutationFn: (data: CreateMedicalPartnerDto) => adminService.createMedicalPartner(data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-medical-partners'] });
            toast.success('Medical Partner created');
            onOpenChange(false);
            reset();
        },
        onError: () => toast.error('Failed to create partner')
    });

    const onSubmit = (data: PartnerFormValues) => {
        const dto: CreateMedicalPartnerDto = {
            ...data,
            website: data.website || undefined,
            email: data.email || undefined
        };
        createMutation.mutate(dto);
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
                <DialogHeader>
                    <DialogTitle>Add Medical Partner</DialogTitle>
                    <DialogDescription>Add a new hospital, clinic, or pharmacy.</DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit(onSubmit as any)} className="space-y-4">
                    <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-200">Name</label>
                            <Input {...register('name')} placeholder="e.g. Al-Salam Hospital" />
                            {errors.name && <p className="text-red-500 text-xs">{errors.name.message}</p>}
                        </div>
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-200">Type</label>
                            <select
                                {...register('type')}
                                className="flex h-10 w-full rounded-md border border-white/10 bg-slate-950 px-3 py-2 text-sm text-white placeholder:text-slate-400 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-accent disabled:cursor-not-allowed disabled:opacity-50"
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
                        <label className="text-sm font-medium text-slate-200">Description</label>
                        <Input {...register('description')} placeholder="Specialties, working hours..." />
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-200">Address</label>
                            <Input {...register('address')} placeholder="123 Main St" />
                            {errors.address && <p className="text-red-500 text-xs">{errors.address.message}</p>}
                        </div>
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-200">City</label>
                            <Input {...register('city')} placeholder="Cairo" />
                        </div>
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-200">Contact Number</label>
                            <Input {...register('contactNumber')} placeholder="+20..." />
                            {errors.contactNumber && <p className="text-red-500 text-xs">{errors.contactNumber.message}</p>}
                        </div>
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-200">Hotline</label>
                            <Input {...register('hotlineNumber')} placeholder="19xxx" />
                        </div>
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-200">Email</label>
                            <Input {...register('email')} type="email" placeholder="contact@hospital.com" />
                            {errors.email && <p className="text-red-500 text-xs">{errors.email.message}</p>}
                        </div>
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-200">Website</label>
                            <Input {...register('website')} placeholder="https://..." />
                            {errors.website && <p className="text-red-500 text-xs">{errors.website.message}</p>}
                        </div>
                    </div>

                    <DialogFooter>
                        <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
                        <Button type="submit" isLoading={createMutation.isPending}>Create Partner</Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
