import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../../services/adminService';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '../../../components/ui/Dialog';
import { Button } from '../../../components/ui/Button';
import { Input } from '../../../components/ui/Input';
import { Label } from '../../../components/ui/Label';
import { toast } from 'sonner';
import { Loader2, Upload, ExternalLink } from 'lucide-react';
import { useState, useEffect } from 'react';
import type { CompanyDto } from '../../../types/company';

const companySchema = z.object({
    nameAr: z.string().min(2, 'Arabic Name is required'),
    nameEn: z.string().min(2, 'English Name is required'),
    industry: z.string().min(2, 'Industry is required'),
    websiteUrl: z.string().url('Invalid URL').optional().or(z.literal('')),
    email: z.string().email('Invalid email').optional().or(z.literal('')),
    phoneNumber: z.string().optional(),
    description: z.string().optional(),
    isActive: z.boolean(),
});

type CompanyFormValues = z.infer<typeof companySchema>;

interface EditCompanyModalProps {
    company: CompanyDto | null;
    open: boolean;
    onOpenChange: (open: boolean) => void;
}

export const EditCompanyModal = ({ company, open, onOpenChange }: EditCompanyModalProps) => {
    const queryClient = useQueryClient();
    const [logoFile, setLogoFile] = useState<File | null>(null);
    const [logoPreview, setLogoPreview] = useState<string | null>(null);

    const { register, handleSubmit, setValue, watch, formState: { errors } } = useForm<CompanyFormValues>({
        resolver: zodResolver(companySchema),
    });

    useEffect(() => {
        if (company) {
            setValue('nameAr', company.nameAr);
            setValue('nameEn', company.nameEn);
            setValue('industry', company.industry || '');
            setValue('websiteUrl', company.websiteUrl || '');
            setValue('email', company.email || '');
            setValue('phoneNumber', company.phoneNumber || '');
            setValue('description', company.description || '');
            setValue('isActive', company.isActive);
            setLogoPreview(null);
            setLogoFile(null);
        }
    }, [company, setValue]);

    const mutation = useMutation({
        mutationFn: (data: CompanyFormValues) => 
            adminService.updateCompany(company!.id, { ...data, logo: logoFile || undefined }),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-companies'] });
            queryClient.invalidateQueries({ queryKey: ['admin-companies-lookup'] });
            toast.success('Company updated successfully.');
            onOpenChange(false);
        },
        onError: (err: any) => {
            toast.error(err.response?.data?.error?.message || 'Failed to update company.');
        }
    });

    const handleLogoChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (file) {
            setLogoFile(file);
            const reader = new FileReader();
            reader.onloadend = () => {
                setLogoPreview(reader.result as string);
            };
            reader.readAsDataURL(file);
        }
    };

    const onSubmit = (data: CompanyFormValues) => {
        mutation.mutate(data);
    };

    if (!company) return null;

    const isActive = watch('isActive');

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="sm:max-w-[700px] bg-white dark:bg-slate-900 border-white/10 max-h-[90vh] overflow-y-auto">
                <DialogHeader>
                    <DialogTitle className="text-2xl font-bold bg-gradient-to-r from-accent to-blue-400 bg-clip-text text-transparent flex items-center gap-3">
                        Edit Company: {company.nameEn}
                    </DialogTitle>
                </DialogHeader>

                <form onSubmit={handleSubmit(onSubmit)} className="space-y-6 py-4">
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div className="space-y-4">
                            <div className="space-y-2">
                                <Label htmlFor="nameEn">Company Name (English)</Label>
                                <Input
                                    id="nameEn"
                                    {...register('nameEn')}
                                    className={errors.nameEn ? 'border-red-500' : ''}
                                />
                                {errors.nameEn && <p className="text-xs text-red-500">{errors.nameEn.message}</p>}
                            </div>

                            <div className="space-y-2">
                                <Label htmlFor="nameAr">Company Name (Arabic)</Label>
                                <Input
                                    id="nameAr"
                                    dir="rtl"
                                    {...register('nameAr')}
                                    className={errors.nameAr ? 'border-red-500' : ''}
                                />
                                {errors.nameAr && <p className="text-xs text-red-500">{errors.nameAr.message}</p>}
                            </div>

                            <div className="space-y-2">
                                <Label htmlFor="industry">Industry</Label>
                                <Input
                                    id="industry"
                                    {...register('industry')}
                                    className={errors.industry ? 'border-red-500' : ''}
                                />
                                {errors.industry && <p className="text-xs text-red-500">{errors.industry.message}</p>}
                            </div>
                        </div>

                        <div className="space-y-4">
                            <Label>Company Logo</Label>
                            <div className="flex flex-col items-center gap-4 p-4 border-2 border-dashed border-slate-200 dark:border-slate-800 rounded-lg bg-slate-50 dark:bg-slate-950/50">
                                {logoPreview || company.logoBlobName ? (
                                    <div className="relative w-24 h-24 group cursor-pointer" onClick={() => document.getElementById('edit-logo-upload')?.click()}>
                                        <img 
                                            src={logoPreview || `${import.meta.env.VITE_API_BASE_URL}/api/app/file/download/${company.logoBlobName}`} 
                                            alt="Preview" 
                                            className="w-full h-full object-contain rounded-md shadow-sm" 
                                        />
                                        <div className="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center rounded-md">
                                            <Upload className="w-6 h-6 text-white" />
                                        </div>
                                    </div>
                                ) : (
                                    <label htmlFor="edit-logo-upload" className="flex flex-col items-center gap-2 cursor-pointer text-slate-500 hover:text-accent">
                                        <Upload className="w-6 h-6" />
                                        <span className="text-sm">Click to change logo</span>
                                    </label>
                                )}
                                <input 
                                    id="edit-logo-upload" 
                                    type="file" 
                                    accept="image/*" 
                                    className="hidden" 
                                    onChange={handleLogoChange}
                                />
                                <p className="text-[10px] text-slate-400">Updating logo is optional.</p>
                            </div>

                            <div className="flex items-center justify-between p-4 rounded-lg bg-slate-50 dark:bg-slate-950/20 border border-slate-200 dark:border-slate-800">
                                <div className="space-y-0.5">
                                    <Label>Active Status</Label>
                                    <p className="text-xs text-slate-500">Enable or disable company visibility</p>
                                </div>
                                <input
                                    type="checkbox"
                                    className="h-5 w-5 rounded border-slate-300 text-accent focus:ring-accent"
                                    checked={isActive}
                                    onChange={(e) => setValue('isActive', e.target.checked)}
                                />
                            </div>
                        </div>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div className="space-y-2">
                            <Label htmlFor="websiteUrl">Website URL</Label>
                            <div className="relative">
                                <Input
                                    id="websiteUrl"
                                    type="url"
                                    {...register('websiteUrl')}
                                />
                                {watch('websiteUrl') && (
                                    <a 
                                        href={watch('websiteUrl')} 
                                        target="_blank" 
                                        rel="noreferrer"
                                        className="absolute right-3 top-2.5 text-slate-400 hover:text-accent"
                                    >
                                        <ExternalLink className="w-4 h-4" />
                                    </a>
                                )}
                            </div>
                        </div>

                        <div className="space-y-2">
                            <Label htmlFor="email">Public Email</Label>
                            <Input
                                id="email"
                                type="email"
                                {...register('email')}
                            />
                        </div>
                    </div>

                    <div className="space-y-2">
                        <Label htmlFor="description">About the Company</Label>
                        <textarea
                            id="description"
                            rows={3}
                            className="w-full flex rounded-md border border-slate-200 bg-white px-3 py-2 text-sm ring-offset-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-950 focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 dark:border-slate-800 dark:bg-slate-950 dark:ring-offset-slate-950 dark:focus-visible:ring-slate-300"
                            {...register('description')}
                        />
                    </div>

                    <DialogFooter>
                        <Button
                            type="button"
                            variant="ghost"
                            onClick={() => onOpenChange(false)}
                            disabled={mutation.isPending}
                        >
                            Cancel
                        </Button>
                        <Button
                            type="submit"
                            className="bg-accent hover:bg-accent/90 text-white min-w-[120px]"
                            disabled={mutation.isPending}
                        >
                            {mutation.isPending ? (
                                <>
                                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                                    Updating...
                                </>
                            ) : (
                                'Update Company'
                            )}
                        </Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
};
