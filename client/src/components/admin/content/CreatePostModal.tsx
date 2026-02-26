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
import type { CreateBlogPostDto } from '../../../types/news';

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

interface CreatePostModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
}

export function CreatePostModal({ open, onOpenChange }: CreatePostModalProps) {
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

    const { data: categories } = useQuery({
        queryKey: ['article-categories-lookup'],
        queryFn: adminService.getCategoryLookup
    });

    const handleTitleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const val = e.target.value;
        setValue('title', val);
        const generatedSlug = val.toLowerCase().replace(/[^a-z0-9\u0600-\u06FF]+/g, '-').replace(/(^-|-$)+/g, '');
        setValue('slug', generatedSlug, { shouldValidate: true });
    };

    const createMutation = useMutation({
        mutationFn: (data: CreateBlogPostDto) => adminService.createPost(data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-posts'] });
            toast.success('Post created successfully');
            onOpenChange(false);
            reset();
        },
        onError: () => {
            toast.error('Failed to create post');
        }
    });

    const onSubmit = (data: PostFormValues) => {
        const dto: CreateBlogPostDto = {
            ...data,
            coverImageUrl: data.coverImageUrl || undefined,
            tags: data.tags || undefined,
            summary: data.summary || undefined,
            categoryId: data.categoryId || undefined
        };
        createMutation.mutate(dto);
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
                <DialogHeader>
                    <DialogTitle>Create New Post</DialogTitle>
                    <DialogDescription>Write and publish a new blog post or news article.</DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                    <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                            <label className="text-sm font-medium text-slate-200">Title</label>
                            <Input
                                {...register('title')}
                                onChange={(e) => {
                                    register('title').onChange(e);
                                    handleTitleChange(e);
                                }}
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
                            id="isPublished"
                            {...register('isPublished')}
                            className="h-4 w-4 rounded border-white/10 bg-slate-950 text-accent focus:ring-accent"
                        />
                        <label htmlFor="isPublished" className="text-sm font-medium text-slate-200">Publish immediately</label>
                    </div>

                    <DialogFooter>
                        <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
                        <Button type="submit" isLoading={createMutation.isPending}>Create Post</Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
