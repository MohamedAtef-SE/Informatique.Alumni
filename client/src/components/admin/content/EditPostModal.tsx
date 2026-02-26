import { useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { Button } from '../../ui/Button';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '../../ui/Dialog';
import { Input } from '../../ui/Input';
import { adminService } from '../../../services/adminService';
import { useMutation, useQueryClient, useQuery } from '@tanstack/react-query';
import { toast } from 'sonner';
import type { BlogPostDto, UpdateBlogPostDto } from '../../../types/news';

const postSchema = z.object({
    title: z.string().min(3, "Title is required"),
    slug: z.string().min(3, "Slug is required"),
    summary: z.string().optional(),
    content: z.string().min(10, "Content is required"),
    coverImageUrl: z.string().url("Invalid URL").optional().or(z.literal('')),
    tags: z.string().optional(),
    categoryId: z.string().optional(),
    isPublished: z.boolean().default(false),
});

type PostFormValues = z.infer<typeof postSchema>;

interface EditPostModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    post: BlogPostDto | null;
}

export function EditPostModal({ open, onOpenChange, post }: EditPostModalProps) {
    const { t, i18n } = useTranslation();
    const queryClient = useQueryClient();
    const { register, handleSubmit, formState: { errors }, reset, setValue } = useForm<PostFormValues>({
        // @ts-ignore
        resolver: zodResolver(postSchema),
        defaultValues: {
            title: '',
            slug: '',
            content: '',
            isPublished: false
        }
    });

    useEffect(() => {
        if (post && open) {
            setValue('title', post.title);
            setValue('slug', post.slug || '');
            setValue('summary', post.summary || '');
            setValue('content', post.content);
            setValue('coverImageUrl', post.coverImageUrl || '');
            setValue('tags', post.tags || '');
            setValue('categoryId', post.categoryId || '');
            setValue('isPublished', post.isPublished);
        }
    }, [post, open, setValue]);

    const { data: categories } = useQuery({
        queryKey: ['article-categories-lookup'],
        queryFn: adminService.getCategoryLookup
    });

    const updateMutation = useMutation({
        mutationFn: (data: UpdateBlogPostDto) => adminService.updatePost(post!.id, data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-posts'] });
            toast.success('Post updated successfully');
            onOpenChange(false);
            reset();
        },
        onError: () => {
            toast.error('Failed to update post');
        }
    });

    const onSubmit = (data: PostFormValues) => {
        if (!post) return;

        const dto: UpdateBlogPostDto = {
            ...data,
            coverImageUrl: data.coverImageUrl || undefined,
            tags: data.tags || undefined,
            summary: data.summary || undefined,
            categoryId: data.categoryId || undefined
        };
        updateMutation.mutate(dto);
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
                <DialogHeader>
                    <DialogTitle>Edit Post</DialogTitle>
                    <DialogDescription>Update the content of your blog post.</DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                    <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-200">Title</label>
                            <Input
                                {...register('title')}
                                placeholder="Article Title"
                            />
                            {errors.title && <p className="text-red-500 text-xs">{errors.title.message}</p>}
                        </div>
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-200">Category</label>
                            <select
                                {...register('categoryId')}
                                className="flex h-10 w-full rounded-md border border-white/10 bg-slate-950 px-3 py-2 text-sm text-white ring-offset-background placeholder:text-slate-400 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                            >
                                <option value="">Select a category</option>
                                {categories?.items?.map(c => (
                                    <option key={c.id} value={c.id}>
                                        {i18n.language.startsWith('ar') ? c.nameAr : c.nameEn}
                                    </option>
                                ))}
                            </select>
                        </div>
                    </div>

                    <div className="space-y-2">
                        <label className="text-sm font-medium text-slate-200">Slug (URL)</label>
                        <Input {...register('slug')} placeholder="article-url-slug" />
                        {errors.slug && <p className="text-red-500 text-xs">{errors.slug.message}</p>}
                    </div>

                    <div className="space-y-2">
                        <label className="text-sm font-medium text-slate-200">Short Description</label>
                        <Input {...register('summary')} placeholder="Brief summary" />
                    </div>

                    <div className="space-y-2">
                        <label className="text-sm font-medium text-slate-200">Content</label>
                        <textarea
                            className="flex min-h-[200px] w-full rounded-md border border-white/10 bg-slate-950 px-3 py-2 text-sm text-white ring-offset-background placeholder:text-slate-400 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                            {...register('content')}
                            placeholder="Write your content here..."
                        />
                        {errors.content && <p className="text-red-500 text-xs">{errors.content.message}</p>}
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-200">Cover Image URL</label>
                            <Input {...register('coverImageUrl')} placeholder="https://..." />
                            {errors.coverImageUrl && <p className="text-red-500 text-xs">{errors.coverImageUrl.message}</p>}
                        </div>
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-200">Tags</label>
                            <Input {...register('tags')} placeholder="Tech, Event, News" />
                        </div>
                    </div>

                    <div className="flex items-center gap-2">
                        <input
                            type="checkbox"
                            id="isPublished-edit"
                            {...register('isPublished')}
                            className="h-4 w-4 rounded border-white/10 bg-slate-950 text-accent focus:ring-accent"
                        />
                        <label htmlFor="isPublished-edit" className="text-sm font-medium text-slate-200">Publish immediately</label>
                    </div>

                    <DialogFooter>
                        <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
                        <Button type="submit" isLoading={updateMutation.isPending}>Save Changes</Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
