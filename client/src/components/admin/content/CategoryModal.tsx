import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { Button } from '../../ui/Button';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '../../ui/Dialog';
import { Input } from '../../ui/Input';
import { adminService } from '../../../services/adminService';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import type { ArticleCategoryDto, CreateUpdateArticleCategoryDto } from '../../../types/news';
import { useEffect } from 'react';

type CategoryFormValues = {
    nameEn: string;
    nameAr: string;
    isActive: boolean;
};

interface CategoryModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    category?: ArticleCategoryDto | null;
}

export function CategoryModal({ open, onOpenChange, category }: CategoryModalProps) {
    const isEditing = !!category;
    const { register, handleSubmit, formState: { errors }, reset } = useForm<CategoryFormValues>({
        resolver: zodResolver(z.object({
            nameEn: z.string().min(2, "English name is required"),
            nameAr: z.string().min(2, "Arabic name is required"),
            isActive: z.boolean(),
        })),
        defaultValues: {
            nameEn: '',
            nameAr: '',
            isActive: true
        }
    });
    const queryClient = useQueryClient();

    useEffect(() => {
        if (open) {
            if (category) {
                reset({
                    nameEn: category.nameEn,
                    nameAr: category.nameAr,
                    isActive: category.isActive
                });
            } else {
                reset({
                    nameEn: '',
                    nameAr: '',
                    isActive: true
                });
            }
        }
    }, [open, category, reset]);

    const mutation = useMutation({
        mutationFn: (data: CreateUpdateArticleCategoryDto) => 
            isEditing 
                ? adminService.updateContentCategory(category!.id, data)
                : adminService.createContentCategory(data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-content-categories'] });
            queryClient.invalidateQueries({ queryKey: ['article-categories-lookup'] });
            toast.success(isEditing ? 'Category updated successfully' : 'Category created successfully');
            onOpenChange(false);
            reset();
        },
        onError: () => {
            toast.error('Failed to save category');
        }
    });

    const onSubmit = (data: CategoryFormValues) => {
        mutation.mutate(data);
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="sm:max-w-[425px]">
                <DialogHeader>
                    <DialogTitle>{isEditing ? 'Edit Category' : 'Create Category'}</DialogTitle>
                    <DialogDescription>
                        {isEditing ? 'Update the details of the CMS category.' : 'Add a new category for your blog posts and articles.'}
                    </DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                    <div className="space-y-2">
                        <label className="text-sm font-medium text-slate-200">Name (English)</label>
                        <Input {...register('nameEn')} placeholder="Technology" />
                        {errors.nameEn && <p className="text-red-500 text-xs">{errors.nameEn.message}</p>}
                    </div>

                    <div className="space-y-2">
                        <label className="text-sm font-medium text-slate-200">Name (Arabic)</label>
                        <Input {...register('nameAr')} placeholder="تكنولوجيا" />
                        {errors.nameAr && <p className="text-red-500 text-xs">{errors.nameAr.message}</p>}
                    </div>

                    <div className="flex items-center gap-2">
                        <input
                            type="checkbox"
                            id="isActive"
                            {...register('isActive')}
                            className="h-4 w-4 rounded border-white/10 bg-slate-950 text-accent focus:ring-accent"
                        />
                        <label htmlFor="isActive" className="text-sm font-medium text-slate-200">Is Active</label>
                    </div>

                    <DialogFooter>
                        <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
                        <Button type="submit" isLoading={mutation.isPending}>
                            {isEditing ? 'Update' : 'Create'}
                        </Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
