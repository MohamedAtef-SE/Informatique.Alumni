import { useEffect } from 'react';
import { useForm, useWatch } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { Button } from '../../ui/Button';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '../../ui/Dialog';
import { Input } from '../../ui/Input';
import { adminService } from '../../../services/adminService';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import type { BranchDto, CreateUpdateBranchDto } from '../../../types/organization';
import { MapPicker } from '../../ui/MapPicker';

const branchSchema = z.object({
    name: z.string().min(2, "Name is required"),
    code: z.string().min(2, "Code is required"),
    address: z.string().optional(),
    presidentId: z.string().optional(),
    email: z.string().email("Invalid email format").optional().or(z.literal('')),
    phoneNumber: z.string().optional(),
    linkedInPage: z.string().url("Must be a valid URL").optional().or(z.literal('')),
    facebookPage: z.string().url("Must be a valid URL").optional().or(z.literal('')),
    whatsAppGroup: z.string().url("Must be a valid URL").optional().or(z.literal('')),
    latitude: z.union([z.string(), z.number()]).optional(),
    longitude: z.union([z.string(), z.number()]).optional()
});

type BranchFormValues = z.infer<typeof branchSchema>;

interface BranchModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    branch?: BranchDto | null; // If provided, it's an Update. If undefined/null, it's a Create.
}

export function BranchModal({ open, onOpenChange, branch }: BranchModalProps) {
    const queryClient = useQueryClient();
    const isEditing = !!branch;

    const { register, handleSubmit, formState: { errors }, reset, setValue, control } = useForm<BranchFormValues>({
        resolver: zodResolver(branchSchema),
        defaultValues: {
            name: '',
            code: '',
            address: '',
            presidentId: '',
            email: '',
            phoneNumber: '',
            linkedInPage: '',
            facebookPage: '',
            whatsAppGroup: '',
            latitude: undefined,
            longitude: undefined
        }
    });

    useEffect(() => {
        if (open) {
            if (isEditing && branch) {
                reset({
                    name: branch.name,
                    code: branch.code,
                    address: branch.address || '',
                    presidentId: branch.presidentId || '',
                    email: branch.email || '',
                    phoneNumber: branch.phoneNumber || '',
                    linkedInPage: branch.linkedInPage || '',
                    facebookPage: branch.facebookPage || '',
                    whatsAppGroup: branch.whatsAppGroup || '',
                    latitude: branch.latitude,
                    longitude: branch.longitude
                });
            } else {
                reset({
                    name: '',
                    code: '',
                    address: '',
                    presidentId: '',
                    email: '',
                    phoneNumber: '',
                    linkedInPage: '',
                    facebookPage: '',
                    whatsAppGroup: '',
                    latitude: undefined,
                    longitude: undefined
                });
            }
        }
    }, [branch, open, reset, isEditing]);

    const saveMutation = useMutation({
        mutationFn: (data: CreateUpdateBranchDto) => {
            if (isEditing && branch) {
                return adminService.updateBranch(branch.id, data);
            } else {
                return adminService.createBranch(data);
            }
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-branches'] });
            toast.success(`Branch ${isEditing ? 'updated' : 'created'} successfully`);
            onOpenChange(false);
            reset();
        },
        onError: () => toast.error(`Failed to ${isEditing ? 'update' : 'create'} branch`)
    });

    // Sync lat/lng with the interactive MapPicker
    const watchedLat = useWatch({ control, name: 'latitude' });
    const watchedLng = useWatch({ control, name: 'longitude' });

    const handleLocationChange = (lat: number, lng: number) => {
        setValue('latitude', lat, { shouldDirty: true });
        setValue('longitude', lng, { shouldDirty: true });
    };

    const onSubmit = (data: BranchFormValues) => {
        const payload: CreateUpdateBranchDto = {
            ...data,
            presidentId: data.presidentId || undefined,
            email: data.email || undefined,
            phoneNumber: data.phoneNumber || undefined,
            linkedInPage: data.linkedInPage || undefined,
            facebookPage: data.facebookPage || undefined,
            whatsAppGroup: data.whatsAppGroup || undefined,
            latitude: data.latitude ? Number(data.latitude) : undefined,
            longitude: data.longitude ? Number(data.longitude) : undefined
        };
        saveMutation.mutate(payload);
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
                <DialogHeader>
                    <DialogTitle>{isEditing ? 'Update Branch' : 'Create New Branch'}</DialogTitle>
                    <DialogDescription>
                        {isEditing ? 'Modify the details of this organization branch.' : 'Add a new organization branch location.'}
                    </DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                    <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-200">Branch Name <span className="text-red-500">*</span></label>
                            <Input {...register('name')} placeholder="e.g. Main Branch" />
                            {errors.name && <p className="text-red-500 text-xs">{errors.name.message}</p>}
                        </div>

                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-200">Branch Code <span className="text-red-500">*</span></label>
                            <Input {...register('code')} placeholder="e.g. BR-01" />
                            {errors.code && <p className="text-red-500 text-xs">{errors.code.message}</p>}
                        </div>
                    </div>

                    <div className="space-y-2">
                        <label className="text-sm font-medium text-slate-200">Physical Address</label>
                        <Input {...register('address')} placeholder="123 Street..." />
                    </div>

                    <div className="pt-4 border-t border-slate-800">
                        <h4 className="text-sm font-semibold text-slate-300 mb-4">Contact & Location</h4>
                        <div className="grid grid-cols-2 gap-4">
                            <div className="space-y-2">
                                <label className="text-sm font-medium text-slate-200">Official Email</label>
                                <Input {...register('email')} type="email" placeholder="branch@example.com" />
                                {errors.email && <p className="text-red-500 text-xs">{errors.email.message}</p>}
                            </div>
                            <div className="space-y-2">
                                <label className="text-sm font-medium text-slate-200">Phone Number</label>
                                <Input {...register('phoneNumber')} placeholder="+1 234 567 890" />
                            </div>
                        </div>

                        {/* Interactive Map Location Picker */}
                        <div className="space-y-2 mt-4">
                            <label className="text-sm font-medium text-slate-200 flex items-center gap-2">
                                Branch Location
                                <span className="text-[10px] font-normal text-slate-500 uppercase tracking-wider">click map to pin</span>
                            </label>
                            <MapPicker
                                latitude={watchedLat ? Number(watchedLat) : undefined}
                                longitude={watchedLng ? Number(watchedLng) : undefined}
                                onLocationChange={handleLocationChange}
                            />
                        </div>
                    </div>

                    <div className="pt-4 border-t border-slate-800">
                        <h4 className="text-sm font-semibold text-slate-300 mb-4">Social Media Links</h4>
                        <div className="space-y-4">
                            <div className="space-y-2">
                                <label className="text-sm font-medium text-slate-200">LinkedIn Page</label>
                                <Input {...register('linkedInPage')} placeholder="https://linkedin.com/..." />
                                {errors.linkedInPage && <p className="text-red-500 text-xs">{errors.linkedInPage.message}</p>}
                            </div>
                            <div className="space-y-2">
                                <label className="text-sm font-medium text-slate-200">Facebook Page</label>
                                <Input {...register('facebookPage')} placeholder="https://facebook.com/..." />
                                {errors.facebookPage && <p className="text-red-500 text-xs">{errors.facebookPage.message}</p>}
                            </div>
                            <div className="space-y-2">
                                <label className="text-sm font-medium text-slate-200">WhatsApp Group</label>
                                <Input {...register('whatsAppGroup')} placeholder="https://chat.whatsapp.com/..." />
                                {errors.whatsAppGroup && <p className="text-red-500 text-xs">{errors.whatsAppGroup.message}</p>}
                            </div>
                        </div>
                    </div>

                    <DialogFooter>
                        <Button type="button" variant="outline" onClick={() => {
                            onOpenChange(false);
                            // Ensure we reset state when cancelling out of Edit Mode
                            reset();
                        }}>Cancel</Button>
                        <Button type="submit" isLoading={saveMutation.isPending}>{isEditing ? 'Save Changes' : 'Create Branch'}</Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog >
    );
}
