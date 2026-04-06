import { useState, useEffect } from 'react';
import { useMutation, useQueryClient, useQuery } from '@tanstack/react-query';
import { adminService } from '../../../services/adminService';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from '../../ui/Dialog';
import { Button } from '../../ui/Button';
import { Input } from '../../ui/Input';
import { toast } from 'sonner';
import type { MajorDto, CreateUpdateMajorDto } from '../../../types/organization';

interface MajorModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    major: MajorDto | null;
}

export const MajorModal = ({ open, onOpenChange, major }: MajorModalProps) => {
    const queryClient = useQueryClient();
    const [formData, setFormData] = useState<CreateUpdateMajorDto>({
        name: '',
        collegeId: ''
    });

    useEffect(() => {
        if (major && open) {
            setFormData({
                name: major.name,
                collegeId: major.collegeId,
            });
        } else if (open) {
            setFormData({
                name: '',
                collegeId: ''
            });
        }
    }, [major, open]);

    const { data: collegesData, isLoading: collegesLoading } = useQuery({
        queryKey: ['admin-colleges-lookup'],
        queryFn: () => adminService.getColleges({ skipCount: 0, maxResultCount: 1000 }),
        enabled: open
    });

    const createMutation = useMutation({
        mutationFn: adminService.createMajor,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-majors'] });
            toast.success('Major created successfully');
            onOpenChange(false);
        },
        onError: (err: any) => {
            toast.error(err.response?.data?.error?.message || 'Failed to create major');
        }
    });

    const updateMutation = useMutation({
        mutationFn: ({ id, data }: { id: string, data: CreateUpdateMajorDto }) => 
            adminService.updateMajor(id, data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-majors'] });
            toast.success('Major updated successfully');
            onOpenChange(false);
        },
        onError: (err: any) => {
            toast.error(err.response?.data?.error?.message || 'Failed to update major');
        }
    });

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        
        if (!formData.name.trim()) {
            toast.error('Major name is required');
            return;
        }
        
        if (!formData.collegeId) {
            toast.error('Selecting a linked college is required');
            return;
        }

        if (major) {
            updateMutation.mutate({ id: major.id, data: formData });
        } else {
            createMutation.mutate(formData);
        }
    };

    const isPending = createMutation.isPending || updateMutation.isPending;

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>{major ? 'Edit Major' : 'New Major'}</DialogTitle>
                    <DialogDescription>{major ? 'Update major details' : 'Create a new major linked to a college'}</DialogDescription>
                </DialogHeader>
            <form onSubmit={handleSubmit} className="space-y-4">
                <div className="space-y-2">
                    <label className="text-sm font-medium text-slate-300">Major Name <span className="text-red-500">*</span></label>
                    <Input
                        placeholder="e.g. Computer Science, Accounting"
                        value={formData.name}
                        onChange={(e: any) => setFormData(prev => ({ ...prev, name: e.target.value }))}
                        required
                    />
                </div>

                <div className="space-y-2">
                    <label className="text-sm font-medium text-slate-300">Linked College <span className="text-red-500">*</span></label>
                    <select
                        className="flex h-10 w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                        value={formData.collegeId}
                        onChange={(e: any) => setFormData(prev => ({ ...prev, collegeId: e.target.value }))}
                        disabled={collegesLoading}
                        required
                    >
                        <option value="">-- Select a mapping college --</option>
                        {collegesData?.items.map(college => (
                            <option key={college.id} value={college.id}>{college.name}</option>
                        ))}
                    </select>
                </div>

                <div className="flex justify-end gap-3 mt-6">
                    <Button type="button" variant="ghost" onClick={() => onOpenChange(false)}>
                        Cancel
                    </Button>
                    <Button type="submit" className="shadow-neon" disabled={isPending}>
                        {isPending ? 'Saving...' : 'Save Major'}
                    </Button>
                </div>
            </form>
            </DialogContent>
        </Dialog>
    );
};
