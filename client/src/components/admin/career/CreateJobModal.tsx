import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../../services/adminService';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '../../../components/ui/Dialog';
import { Button } from '../../../components/ui/Button';
import { Input } from '../../../components/ui/Input';
import { Label } from '../../../components/ui/Label';
import { toast } from 'sonner';
import { Loader2 } from 'lucide-react';

const jobSchema = z.object({
    title: z.string().min(3, 'Title must be at least 3 characters'),
    description: z.string().min(10, 'Description must be at least 10 characters'),
    requirements: z.string().optional(),
    companyId: z.string().uuid('Please select a company'),
    closingDate: z.string().optional().refine((val) => !val || new Date(val) > new Date(), {
        message: 'Closing date must be in the future',
    }),
});

type JobFormValues = z.infer<typeof jobSchema>;

interface CreateJobModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
}

export const CreateJobModal = ({ open, onOpenChange }: CreateJobModalProps) => {
    const queryClient = useQueryClient();
    
    // Fetch companies for the dropdown
    const { data: companies, isLoading: isLoadingCompanies } = useQuery({
        queryKey: ['admin-companies-lookup'],
        queryFn: () => adminService.getCompanies({}),
        enabled: open
    });

    const { register, handleSubmit, reset, formState: { errors } } = useForm<JobFormValues>({
        resolver: zodResolver(jobSchema),
        defaultValues: {
            isActive: true
        } as any
    });

    const mutation = useMutation({
        mutationFn: adminService.createJob,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-jobs'] });
            toast.success('Job listing created successfully.');
            reset();
            onOpenChange(false);
        },
        onError: (error: any) => {
            toast.error(error.response?.data?.error?.message || 'Failed to create job.');
        }
    });

    const onSubmit = (data: JobFormValues) => {
        mutation.mutate(data);
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="sm:max-w-[600px] bg-white dark:bg-slate-900 border-white/10">
                <DialogHeader>
                    <DialogTitle className="text-2xl font-bold bg-gradient-to-r from-accent to-blue-400 bg-clip-text text-transparent">
                        Post New Job
                    </DialogTitle>
                </DialogHeader>

                <form onSubmit={handleSubmit(onSubmit)} className="space-y-6 py-4">
                    <div className="grid gap-4">
                        <div className="space-y-2">
                            <Label htmlFor="title">Job Title</Label>
                            <Input
                                id="title"
                                placeholder="e.g. Senior Software Engineer"
                                {...register('title')}
                                className={errors.title ? 'border-red-500' : ''}
                            />
                            {errors.title && <p className="text-xs text-red-500">{errors.title.message}</p>}
                        </div>

                        <div className="space-y-2">
                            <Label htmlFor="companyId">Company</Label>
                            <select
                                id="companyId"
                                {...register('companyId')}
                                className="w-full flex h-10 rounded-md border border-slate-200 bg-white px-3 py-2 text-sm ring-offset-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-950 focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 dark:border-slate-800 dark:bg-slate-950 dark:ring-offset-slate-950 dark:focus-visible:ring-slate-300"
                            >
                                <option value="">Select a company...</option>
                                {companies?.items.map((c) => (
                                    <option key={c.id} value={c.id}>
                                        {c.nameEn} ({c.nameAr})
                                    </option>
                                ))}
                            </select>
                            {errors.companyId && <p className="text-xs text-red-500">{errors.companyId.message}</p>}
                            {isLoadingCompanies && <p className="text-xs text-slate-500 flex items-center gap-2 mt-1"><Loader2 className="w-3 h-3 animate-spin"/> Loading companies...</p>}
                        </div>

                        <div className="space-y-2">
                            <Label htmlFor="description">Job Description</Label>
                            <textarea
                                id="description"
                                rows={4}
                                className="w-full flex rounded-md border border-slate-200 bg-white px-3 py-2 text-sm ring-offset-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-950 focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 dark:border-slate-800 dark:bg-slate-950 dark:ring-offset-slate-950 dark:focus-visible:ring-slate-300 min-h-[100px]"
                                placeholder="Describe the role, responsibilities, and team..."
                                {...register('description')}
                            />
                            {errors.description && <p className="text-xs text-red-500">{errors.description.message}</p>}
                        </div>

                        <div className="space-y-2">
                            <Label htmlFor="requirements">Requirements (Optional)</Label>
                            <textarea
                                id="requirements"
                                rows={3}
                                className="w-full flex rounded-md border border-slate-200 bg-white px-3 py-2 text-sm ring-offset-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-950 focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 dark:border-slate-800 dark:bg-slate-950 dark:ring-offset-slate-950 dark:focus-visible:ring-slate-300"
                                placeholder="List skills, experience, and qualifications..."
                                {...register('requirements')}
                            />
                        </div>

                        <div className="space-y-2">
                            <Label htmlFor="closingDate">Closing Date (Optional)</Label>
                            <Input
                                id="closingDate"
                                type="date"
                                {...register('closingDate')}
                            />
                        </div>
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
                            className="bg-accent hover:bg-accent/90 text-white min-w-[100px]"
                            disabled={mutation.isPending}
                        >
                            {mutation.isPending ? (
                                <>
                                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                                    Posting...
                                </>
                            ) : (
                                'Post Job'
                            )}
                        </Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
};
