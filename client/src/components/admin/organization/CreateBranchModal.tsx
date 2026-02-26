import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { Button } from '../../ui/Button';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '../../ui/Dialog';
import { Input } from '../../ui/Input';
import { adminService } from '../../../services/adminService';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import type { CreateUpdateBranchDto } from '../../../types/organization';

const branchSchema = z.object({
    name: z.string().min(2, "Name is required"),
    code: z.string().min(2, "Code is required"),
    address: z.string().optional()
});

type BranchFormValues = z.infer<typeof branchSchema>;

interface CreateBranchModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
}

export function CreateBranchModal({ open, onOpenChange }: CreateBranchModalProps) {
    const queryClient = useQueryClient();
    const { register, handleSubmit, formState: { errors }, reset } = useForm<BranchFormValues>({
        resolver: zodResolver(branchSchema),
        defaultValues: {
            name: '',
            code: '',
            address: ''
        }
    });

    const createMutation = useMutation({
        mutationFn: (data: CreateUpdateBranchDto) => adminService.createBranch(data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-branches'] });
            toast.success('Branch created successfully');
            onOpenChange(false);
            reset();
        },
        onError: () => toast.error('Failed to create branch')
    });

    const onSubmit = (data: BranchFormValues) => {
        createMutation.mutate(data);
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>Create New Branch</DialogTitle>
                    <DialogDescription>Add a new organization branch location.</DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                    <div className="space-y-2">
                        <label className="text-sm font-medium text-slate-200">Branch Name</label>
                        <Input {...register('name')} placeholder="e.g. Main Branch" />
                        {errors.name && <p className="text-red-500 text-xs">{errors.name.message}</p>}
                    </div>

                    <div className="space-y-2">
                        <label className="text-sm font-medium text-slate-200">Branch Code</label>
                        <Input {...register('code')} placeholder="e.g. BR-01" />
                        {errors.code && <p className="text-red-500 text-xs">{errors.code.message}</p>}
                    </div>

                    <div className="space-y-2">
                        <label className="text-sm font-medium text-slate-200">Address (Optional)</label>
                        <Input {...register('address')} placeholder="123 Street..." />
                    </div>

                    <DialogFooter>
                        <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
                        <Button type="submit" isLoading={createMutation.isPending}>Create Branch</Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
