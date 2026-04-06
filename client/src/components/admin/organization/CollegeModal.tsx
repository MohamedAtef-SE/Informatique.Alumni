import { useState, useEffect } from 'react';
import { useMutation, useQueryClient, useQuery } from '@tanstack/react-query';
import { adminService } from '../../../services/adminService';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from '../../ui/Dialog';
import { Button } from '../../ui/Button';
import { Input } from '../../ui/Input';
import { toast } from 'sonner';
import type { CollegeDto, CreateUpdateCollegeDto } from '../../../types/organization';

interface CollegeModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    college: CollegeDto | null;
}

export const CollegeModal = ({ open, onOpenChange, college }: CollegeModalProps) => {
    const queryClient = useQueryClient();
    const [formData, setFormData] = useState<CreateUpdateCollegeDto>({
        name: '',
        branchId: null,
        externalId: null
    });

    useEffect(() => {
        if (college && open) {
            setFormData({
                name: college.name,
                branchId: college.branchId,
                externalId: college.externalId,
            });
        } else if (open) {
            setFormData({
                name: '',
                branchId: null,
                externalId: null
            });
        }
    }, [college, open]);

    const { data: branchesData, isLoading: branchesLoading } = useQuery({
        queryKey: ['admin-branches-lookup'],
        queryFn: () => adminService.getBranches({ skipCount: 0, maxResultCount: 1000 }),
        enabled: open
    });

    const createMutation = useMutation({
        mutationFn: adminService.createCollege,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-colleges'] });
            toast.success('College created successfully');
            onOpenChange(false);
        },
        onError: (err: any) => {
            toast.error(err.response?.data?.error?.message || 'Failed to create college');
        }
    });

    const updateMutation = useMutation({
        mutationFn: ({ id, data }: { id: string, data: CreateUpdateCollegeDto }) => 
            adminService.updateCollege(id, data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-colleges'] });
            toast.success('College updated successfully');
            onOpenChange(false);
        },
        onError: (err: any) => {
            toast.error(err.response?.data?.error?.message || 'Failed to update college');
        }
    });

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        // Validation
        if (!formData.name.trim()) {
            toast.error('College name is required');
            return;
        }

        if (college) {
            updateMutation.mutate({ id: college.id, data: formData });
        } else {
            createMutation.mutate(formData);
        }
    };

    const isPending = createMutation.isPending || updateMutation.isPending;

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>{college ? 'Edit College' : 'New College'}</DialogTitle>
                    <DialogDescription>{college ? 'Update college details' : 'Create a new college in the system'}</DialogDescription>
                </DialogHeader>
            <form onSubmit={handleSubmit} className="space-y-4">
                <div className="space-y-2">
                    <label className="text-sm font-medium text-slate-300">College Name <span className="text-red-500">*</span></label>
                    <Input
                        placeholder="e.g. Engineering, Mass Communication"
                        value={formData.name}
                        onChange={(e: any) => setFormData(prev => ({ ...prev, name: e.target.value }))}
                        required
                    />
                </div>

                <div className="space-y-2">
                    <label className="text-sm font-medium text-slate-300">Linked Branch</label>
                    <select
                        className="flex h-10 w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                        value={formData.branchId || ''}
                        onChange={(e: any) => setFormData(prev => ({ ...prev, branchId: e.target.value || null }))}
                        disabled={branchesLoading}
                    >
                        <option value="">-- No linked branch --</option>
                        {branchesData?.items.map(branch => (
                            <option key={branch.id} value={branch.id}>{branch.name}</option>
                        ))}
                    </select>
                </div>

                <div className="space-y-2">
                    <label className="text-sm font-medium text-slate-300">External ID / SIS Mapping Code</label>
                    <Input
                        placeholder="Optional"
                        value={formData.externalId || ''}
                        onChange={(e: any) => setFormData(prev => ({ ...prev, externalId: e.target.value || null }))}
                    />
                </div>

                <div className="flex justify-end gap-3 mt-6">
                    <Button type="button" variant="ghost" onClick={() => onOpenChange(false)}>
                        Cancel
                    </Button>
                    <Button type="submit" className="shadow-neon" disabled={isPending}>
                        {isPending ? 'Saving...' : 'Save College'}
                    </Button>
                </div>
            </form>
            </DialogContent>
        </Dialog>
    );
};
