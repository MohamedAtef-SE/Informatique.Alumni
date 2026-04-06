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
import { Loader2, Upload } from 'lucide-react';
import { useState } from 'react';

const companySchema = z.object({
    nameAr: z.string().min(2, 'Arabic Name is required'),
    nameEn: z.string().min(2, 'English Name is required'),
    industry: z.string().min(2, 'Industry is required'),
    websiteUrl: z.string().url('Invalid URL').optional().or(z.literal('')),
    email: z.string().email('Invalid email').optional().or(z.literal('')),
    phoneNumber: z.string().optional(),
    description: z.string().optional(),
});

type CompanyFormValues = z.infer<typeof companySchema>;

interface CreateCompanyModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
}

export const CreateCompanyModal = ({ open, onOpenChange }: CreateCompanyModalProps) => {
    const queryClient = useQueryClient();
    const [logoFile, setLogoFile] = useState<File | null>(null);
    const [logoPreview, setLogoPreview] = useState<string | null>(null);

    const { register, handleSubmit, reset, formState: { errors } } = useForm<CompanyFormValues>({
        resolver: zodResolver(companySchema),
    });

    const mutation = useMutation({
        mutationFn: (data: CompanyFormValues) => 
            adminService.createCompany({ ...data, logo: logoFile || undefined }),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-companies'] });
            queryClient.invalidateQueries({ queryKey: ['admin-companies-lookup'] });
            toast.success('Company created successfully.');
            handleClose();
        },
        onError: (err: any) => {
            toast.error(err.response?.data?.error?.message || 'Failed to create company.');
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

    const handleClose = () => {
        reset();
        setLogoFile(null);
        setLogoPreview(null);
        onOpenChange(false);
    };

    const onSubmit = (data: CompanyFormValues) => {
        if (!logoFile) {
            toast.error('Please upload a company logo.');
            return;
        }
        mutation.mutate(data);
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="sm:max-w-[700px] bg-white dark:bg-slate-900 border-white/10 max-h-[90vh] overflow-y-auto">
                <DialogHeader>
                    <DialogTitle className="text-2xl font-bold bg-gradient-to-r from-accent to-blue-400 bg-clip-text text-transparent">
                        Register New Company
                    </DialogTitle>
                </DialogHeader>

                <form onSubmit={handleSubmit(onSubmit)} className="space-y-6 py-4">
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                        {/* Basic Info */}
                        <div className="space-y-4">
                            <div className="space-y-2">
                                <Label htmlFor="nameEn">Company Name (English)</Label>
                                <Input
                                    id="nameEn"
                                    placeholder="e.g. Tech Solutions Inc."
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
                                    placeholder="اسم الشركة"
                                    {...register('nameAr')}
                                    className={errors.nameAr ? 'border-red-500' : ''}
                                />
                                {errors.nameAr && <p className="text-xs text-red-500">{errors.nameAr.message}</p>}
                            </div>

                            <div className="space-y-2">
                                <Label htmlFor="industry">Industry</Label>
                                <Input
                                    id="industry"
                                    placeholder="e.g. Technology, Finance, Education"
                                    {...register('industry')}
                                    className={errors.industry ? 'border-red-500' : ''}
                                />
                                {errors.industry && <p className="text-xs text-red-500">{errors.industry.message}</p>}
                            </div>
                        </div>

                        {/* Contact & Media */}
                        <div className="space-y-4">
                            <div className="space-y-2">
                                <Label>Company Logo</Label>
                                <div className="flex flex-col items-center gap-4 p-4 border-2 border-dashed border-slate-200 dark:border-slate-800 rounded-lg bg-slate-50 dark:bg-slate-950/50">
                                    {logoPreview ? (
                                        <div className="relative w-24 h-24 group cursor-pointer" onClick={() => document.getElementById('logo-upload')?.click()}>
                                            <img src={logoPreview} alt="Preview" className="w-full h-full object-contain rounded-md shadow-sm" />
                                            <div className="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center rounded-md">
                                                <Upload className="w-6 h-6 text-white" />
                                            </div>
                                        </div>
                                    ) : (
                                        <label 
                                            htmlFor="logo-upload" 
                                            className="flex flex-col items-center gap-2 cursor-pointer text-slate-500 hover:text-accent transition-colors"
                                        >
                                            <div className="p-3 bg-white dark:bg-slate-900 rounded-full shadow-sm">
                                                <Upload className="w-6 h-6" />
                                            </div>
                                            <span className="text-sm font-medium">Click to upload logo</span>
                                        </label>
                                    )}
                                    <input 
                                        id="logo-upload" 
                                        type="file" 
                                        accept="image/*" 
                                        className="hidden" 
                                        onChange={handleLogoChange}
                                    />
                                    <p className="text-[10px] text-slate-400">PNG, JPG or JPEG. Max 2MB.</p>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div className="space-y-2">
                            <Label htmlFor="websiteUrl">Website URL</Label>
                            <Input
                                id="websiteUrl"
                                type="url"
                                placeholder="https://example.com"
                                {...register('websiteUrl')}
                            />
                        </div>

                        <div className="space-y-2">
                            <Label htmlFor="email">Public Email</Label>
                            <Input
                                id="email"
                                type="email"
                                placeholder="contact@company.com"
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
                            placeholder="Briefly describe the company's business and values..."
                            {...register('description')}
                        />
                    </div>

                    <DialogFooter>
                        <Button
                            type="button"
                            variant="ghost"
                            onClick={handleClose}
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
                                    Saving...
                                </>
                            ) : (
                                'Save Company'
                            )}
                        </Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
};
