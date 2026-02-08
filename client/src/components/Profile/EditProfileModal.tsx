import { useEffect, useState, useRef } from 'react';
import { useForm, useFieldArray } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '../ui/Dialog';
import { Button } from '../ui/Button';
import { Input } from '../ui/Input';
import { useTranslation } from 'react-i18next';
import { Loader2, Plus, Trash2 } from 'lucide-react';
import { alumniService } from '../../services/alumniService';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';

// Validation Schema
const profileSchema = z.object({
    bio: z.string().optional(),
    jobTitle: z.string().min(2, "Job title is required"),
    company: z.string().min(2, "Company is required"),
    city: z.string().optional(),
    country: z.string().optional(),
    facebookUrl: z.string().url("Invalid URL").optional().or(z.literal('')),
    linkedinUrl: z.string().url("Invalid URL").optional().or(z.literal('')),
    // Dynamic Fields
    emails: z.array(z.object({ id: z.string().optional(), email: z.string().email(), isPrimary: z.boolean() })).optional(),
    mobiles: z.array(z.object({ id: z.string().optional(), mobileNumber: z.string(), isPrimary: z.boolean() })).optional(),
    phones: z.array(z.object({ id: z.string().optional(), phoneNumber: z.string(), label: z.string().optional() })).optional(),
});

type ProfileFormValues = z.infer<typeof profileSchema>;

interface EditProfileModalProps {
    isOpen: boolean;
    onClose: () => void;
    profile: any;
}

const EditProfileModal = ({ isOpen, onClose, profile }: EditProfileModalProps) => {
    const { t } = useTranslation();
    const queryClient = useQueryClient();

    // File Upload State
    const [selectedFile, setSelectedFile] = useState<File | null>(null);
    const [previewUrl, setPreviewUrl] = useState<string | null>(null);
    const fileInputRef = useRef<HTMLInputElement>(null);

    const { register, control, handleSubmit, reset, formState: { errors } } = useForm<ProfileFormValues>({
        resolver: zodResolver(profileSchema),
        defaultValues: {
            bio: '',
            jobTitle: '',
            company: '',
            city: '',
            country: '',
            facebookUrl: '',
            linkedinUrl: '',
            emails: [],
            mobiles: [],
            phones: []
        }
    });

    const { fields: emailFields, append: appendEmail, remove: removeEmail } = useFieldArray({
        control,
        name: "emails"
    });

    const { fields: mobileFields, append: appendMobile, remove: removeMobile } = useFieldArray({
        control,
        name: "mobiles"
    });

    // Reset form when profile changes
    useEffect(() => {
        if (profile) {
            reset({
                bio: profile.bio || '',
                jobTitle: profile.jobTitle || '',
                company: profile.company || '',
                city: profile.city || '',
                country: profile.country || '',
                facebookUrl: profile.facebookUrl || '',
                linkedinUrl: profile.linkedinUrl || '',
                emails: profile.emails || [],
                mobiles: profile.mobiles || [],
                phones: profile.phones || []
            });
            // Reset file state
            setSelectedFile(null);
            setPreviewUrl(null);
        }
    }, [profile, reset]);

    const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        const file = event.target.files?.[0];
        if (file) {
            setSelectedFile(file);
            // Create local preview
            const objectUrl = URL.createObjectURL(file);
            setPreviewUrl(objectUrl);
        }
    };

    const handleAvatarClick = () => {
        fileInputRef.current?.click();
    };

    const updateMutation = useMutation({
        mutationFn: async (data: ProfileFormValues) => {
            // 1. Upload Photo if selected
            if (selectedFile) {
                await alumniService.uploadProfilePhoto(selectedFile);
            }

            // 2. Update Profile Data
            // Map form values to DTO strictly
            // Ensure new items have no ID (or empty) so backend treats them as new.
            // Existing items will have their ID preserved.
            const dto: any = {
                ...data,
                emails: data.emails?.map(e => ({ ...e, id: e.id || undefined })) || [],
                mobiles: data.mobiles?.map(m => ({ ...m, id: m.id || undefined })) || [],
                phones: data.phones?.map(p => ({ ...p, id: p.id || undefined })) || []
            };
            return alumniService.updateProfile(dto);
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['my-profile'] });
            toast.success(t('my_profile.toast.update_success'));
            onClose();
        },
        onError: () => {
            toast.error(t('my_profile.toast.update_error'));
        }
    });

    const onSubmit = (data: ProfileFormValues) => {
        updateMutation.mutate(data);
    };

    return (
        <Dialog open={isOpen} onOpenChange={onClose}>
            <DialogContent className="sm:max-w-2xl max-h-[90vh] overflow-y-auto bg-white text-slate-900 border-slate-200">
                <DialogHeader>
                    <DialogTitle className="text-slate-900">{t('my_profile.actions.edit')}</DialogTitle>
                </DialogHeader>

                <form onSubmit={handleSubmit(onSubmit)} className="space-y-6 mt-4">
                    {/* Header: Avatar & Basic Info */}
                    <div className="flex flex-col items-center sm:flex-row sm:items-start gap-6 pb-6 border-b border-slate-200">
                        {/* Avatar Upload */}
                        <div className="relative group cursor-pointer" onClick={handleAvatarClick}>
                            <div className="w-24 h-24 rounded-full overflow-hidden border-4 border-slate-100 shadow-sm bg-slate-100">
                                <img
                                    src={previewUrl || alumniService.getPhotoUrl(profile?.photoUrl) || `https://ui-avatars.com/api/?name=${profile?.name || 'User'}&background=0D8ABC&color=fff`}
                                    alt="Profile"
                                    className="w-full h-full object-cover"
                                />
                            </div>
                            <div className="absolute inset-0 bg-black/40 rounded-full flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity">
                                <span className="text-white text-xs font-medium">Change</span>
                            </div>
                            <input
                                type="file"
                                ref={fileInputRef}
                                className="hidden"
                                accept="image/png, image/jpeg, image/jpg, image/webp"
                                onChange={handleFileChange}
                            />
                        </div>

                        <div className="flex-1 space-y-4 w-full">
                            <h3 className="text-sm font-bold text-slate-900 uppercase tracking-wider">Professional Info</h3>
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <div className="space-y-2">
                                    <label className="text-sm font-medium text-slate-700">Job Title</label>
                                    <Input
                                        {...register("jobTitle")}
                                        error={errors.jobTitle?.message}
                                        className="bg-white border-slate-300 text-slate-900 focus:bg-white"
                                    />
                                </div>
                                <div className="space-y-2">
                                    <label className="text-sm font-medium text-slate-700">Company</label>
                                    <Input
                                        {...register("company")}
                                        error={errors.company?.message}
                                        className="bg-white border-slate-300 text-slate-900 focus:bg-white"
                                    />
                                </div>
                            </div>
                        </div>
                    </div>

                    {/* Bio */}
                    <div className="space-y-2">
                        <label className="text-sm font-medium text-slate-700">Bio</label>
                        <textarea
                            {...register("bio")}
                            className="w-full min-h-[100px] rounded-md border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-blue-500"
                        />
                    </div>

                    {/* Location */}
                    <div className="space-y-4">
                        <h3 className="text-sm font-semibold text-slate-900 border-b border-slate-200 pb-2">Location</h3>
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <div className="space-y-2">
                                <label className="text-sm font-medium text-slate-700">City</label>
                                <Input
                                    {...register("city")}
                                    className="bg-white border-slate-300 text-slate-900 focus:bg-white"
                                />
                            </div>
                            <div className="space-y-2">
                                <label className="text-sm font-medium text-slate-700">Country</label>
                                <Input
                                    {...register("country")}
                                    className="bg-white border-slate-300 text-slate-900 focus:bg-white"
                                />
                            </div>
                        </div>
                    </div>

                    {/* Contacts (Emails) */}
                    <div className="space-y-4">
                        <div className="flex justify-between items-center border-b border-slate-200 pb-2">
                            <h3 className="text-sm font-semibold text-slate-900">Emails</h3>
                            <Button type="button" variant="ghost" size="sm" onClick={() => appendEmail({ email: '', isPrimary: false })}>
                                <Plus className="w-4 h-4 mr-1 text-slate-600" /> <span className="text-slate-600">Add</span>
                            </Button>
                        </div>
                        <div className="space-y-3">
                            {emailFields.map((field, index) => (
                                <div key={field.id} className="flex gap-3 items-start">
                                    <div className="flex-1">
                                        <Input
                                            {...register(`emails.${index}.email`)}
                                            placeholder="Email Address"
                                            error={errors.emails?.[index]?.email?.message}
                                            className="bg-white border-slate-300 text-slate-900 focus:bg-white placeholder:text-slate-400"
                                        />
                                    </div>
                                    <div className="flex items-center pt-2">
                                        <input type="checkbox" {...register(`emails.${index}.isPrimary`)} className="mr-2" />
                                        <span className="text-sm text-slate-600">Primary</span>
                                    </div>
                                    <Button type="button" variant="ghost" size="icon" onClick={() => removeEmail(index)}>
                                        <Trash2 className="w-4 h-4 text-red-500" />
                                    </Button>
                                </div>
                            ))}
                        </div>
                    </div>

                    {/* Contacts (Mobiles) */}
                    <div className="space-y-4">
                        <div className="flex justify-between items-center border-b border-slate-200 pb-2">
                            <h3 className="text-sm font-semibold text-slate-900">Mobiles</h3>
                            <Button type="button" variant="ghost" size="sm" onClick={() => appendMobile({ mobileNumber: '', isPrimary: false })}>
                                <Plus className="w-4 h-4 mr-1 text-slate-600" /> <span className="text-slate-600">Add</span>
                            </Button>
                        </div>
                        <div className="space-y-3">
                            {mobileFields.map((field, index) => (
                                <div key={field.id} className="flex gap-3 items-start">
                                    <div className="flex-1">
                                        <Input
                                            {...register(`mobiles.${index}.mobileNumber`)}
                                            placeholder="Mobile Number"
                                            error={errors.mobiles?.[index]?.mobileNumber?.message}
                                            className="bg-white border-slate-300 text-slate-900 focus:bg-white placeholder:text-slate-400"
                                        />
                                    </div>
                                    <div className="flex items-center pt-2">
                                        <input type="checkbox" {...register(`mobiles.${index}.isPrimary`)} className="mr-2" />
                                        <span className="text-sm text-slate-600">Primary</span>
                                    </div>
                                    <Button type="button" variant="ghost" size="icon" onClick={() => removeMobile(index)}>
                                        <Trash2 className="w-4 h-4 text-red-500" />
                                    </Button>
                                </div>
                            ))}
                        </div>
                    </div>

                    {/* Social Links */}
                    <div className="space-y-4">
                        <h3 className="text-sm font-semibold text-slate-900 border-b border-slate-200 pb-2">Social Links</h3>
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <div className="space-y-2">
                                <label className="text-sm font-medium text-slate-700">LinkedIn URL</label>
                                <Input
                                    {...register("linkedinUrl")}
                                    error={errors.linkedinUrl?.message}
                                    className="bg-white border-slate-300 text-slate-900 focus:bg-white"
                                />
                            </div>
                            <div className="space-y-2">
                                <label className="text-sm font-medium text-slate-700">Facebook URL</label>
                                <Input
                                    {...register("facebookUrl")}
                                    error={errors.facebookUrl?.message}
                                    className="bg-white border-slate-300 text-slate-900 focus:bg-white"
                                />
                            </div>
                        </div>
                    </div>

                    <div className="flex justify-end gap-3 pt-4 border-t border-slate-200">
                        <Button type="button" variant="ghost" onClick={onClose} className="text-slate-600 hover:text-slate-900 hover:bg-slate-100">
                            Cancel
                        </Button>
                        <Button type="submit" disabled={updateMutation.isPending}>
                            {updateMutation.isPending && <Loader2 className="w-4 h-4 mr-2 animate-spin" />}
                            Save Changes
                        </Button>
                    </div>
                </form>
            </DialogContent>
        </Dialog>
    );
};

export default EditProfileModal;
