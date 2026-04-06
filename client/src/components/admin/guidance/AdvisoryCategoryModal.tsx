import { useForm } from 'react-hook-form';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { advisoryCategoryService } from '../../../services/advisoryCategoryService';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '../../ui/Dialog';
import { Button } from '../../ui/Button';
import { Input } from '../../ui/Input';
import { Label } from '../../ui/Label';
import { toast } from 'sonner';
import type { AdvisoryCategoryDto, CreateUpdateAdvisoryCategoryDto } from '../../../types/guidance';
import { useEffect } from 'react';

interface AdvisoryCategoryModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    category?: AdvisoryCategoryDto | null;
}

export const AdvisoryCategoryModal = ({ open, onOpenChange, category }: AdvisoryCategoryModalProps) => {
    const queryClient = useQueryClient();
    const isEdit = !!category;

    const { register, handleSubmit, reset, setValue, watch } = useForm<CreateUpdateAdvisoryCategoryDto>({
        defaultValues: {
            nameAr: '',
            nameEn: '',
            isActive: true
        }
    });

    useEffect(() => {
        if (category) {
            reset({
                nameAr: category.nameAr,
                nameEn: category.nameEn,
                isActive: category.isActive
            });
        } else {
            reset({
                nameAr: '',
                nameEn: '',
                isActive: true
            });
        }
    }, [category, reset, open]);

    const mutation = useMutation({
        mutationFn: (data: CreateUpdateAdvisoryCategoryDto) => 
            isEdit ? advisoryCategoryService.update(category!.id, data) : advisoryCategoryService.create(data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-advisory-categories'] });
            toast.success(`Category ${isEdit ? 'updated' : 'created'} successfully`);
            onOpenChange(false);
        },
        onError: () => toast.error(`Failed to ${isEdit ? 'update' : 'create'} category`)
    });

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="sm:max-w-[425px] bg-slate-900 text-slate-100 border-slate-800">
                <DialogHeader>
                    <DialogTitle className="text-white">{isEdit ? 'Edit Category' : 'New Advisory Category'}</DialogTitle>
                </DialogHeader>
                <form onSubmit={handleSubmit((data: CreateUpdateAdvisoryCategoryDto) => mutation.mutate(data))} className="space-y-4 py-4">
                    <div className="space-y-2">
                        <Label htmlFor="nameEn" className="text-slate-400">Name (English)</Label>
                        <Input id="nameEn" {...register('nameEn', { required: true })} className="bg-slate-800 border-slate-700" placeholder="e.g. Career Coaching" />
                    </div>
                    <div className="space-y-2">
                        <Label htmlFor="nameAr" className="text-slate-400">Name (Arabic)</Label>
                        <Input id="nameAr" {...register('nameAr', { required: true })} dir="rtl" className="bg-slate-800 border-slate-700 font-arabic" placeholder="مثلاً: التدريب المهني" />
                    </div>
                    <div className="flex items-center gap-3 bg-slate-800/50 p-3 rounded-lg border border-slate-700/50">
                        <input 
                            type="checkbox"
                            id="isActive" 
                            className="w-4 h-4 rounded border-slate-700 text-indigo-600 focus:ring-indigo-500 bg-slate-900"
                            checked={watch('isActive')} 
                            onChange={(e) => setValue('isActive', e.target.checked)} 
                        />
                        <Label htmlFor="isActive" className="text-sm font-medium cursor-pointer">Active and visible to alumni</Label>
                    </div>
                    <div className="flex justify-end gap-3 pt-4">
                        <Button type="button" variant="ghost" onClick={() => onOpenChange(false)}>Cancel</Button>
                        <Button type="submit" className="bg-indigo-600 hover:bg-indigo-500 shadow-neon" isLoading={mutation.isPending}>
                            {isEdit ? 'Save Changes' : 'Create Category'}
                        </Button>
                    </div>
                </form>
            </DialogContent>
        </Dialog>
    );
};
