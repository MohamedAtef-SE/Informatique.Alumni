import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { Button } from '../../ui/Button';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '../../ui/Dialog';
import { Input } from '../../ui/Input';
import { adminService } from '../../../services/adminService';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import type { CreateGalleryAlbumDto } from '../../../types/gallery';

const albumSchema = z.object({
    title: z.string().min(3, "Album name must be at least 3 characters"),
    description: z.string().optional(),
});

type AlbumFormValues = z.infer<typeof albumSchema>;

interface CreateAlbumModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
}

export function CreateAlbumModal({ open, onOpenChange }: CreateAlbumModalProps) {
    const queryClient = useQueryClient();

    const { register, handleSubmit, formState: { errors }, reset } = useForm<AlbumFormValues>({
        // @ts-ignore
        resolver: zodResolver(albumSchema),
        defaultValues: {
            title: '',
            description: '',
        }
    });

    const createMutation = useMutation({
        mutationFn: (data: CreateGalleryAlbumDto) => adminService.createAlbum(data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-gallery'] });
            toast.success('Album created successfully');
            onOpenChange(false);
            reset();
        },
        onError: () => {
            toast.error('Failed to create album');
        }
    });

    const onSubmit = (data: AlbumFormValues) => {
        const dto: CreateGalleryAlbumDto = {
            title: data.title,
            description: data.description,
            eventDate: new Date().toISOString()
        };
        createMutation.mutate(dto);
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>Create New Album</DialogTitle>
                    <DialogDescription>Create a new photo album. You can add photos after creation.</DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                    <div className="space-y-2">
                        <label className="text-sm font-medium text-slate-700 dark:text-slate-200">Album Name</label>
                        <Input {...register('title')} placeholder="e.g. Annual Reunion 2024" />
                        {errors.title && <p className="text-red-500 text-xs">{errors.title.message}</p>}
                    </div>

                    <div className="space-y-2">
                        <label className="text-sm font-medium text-slate-700 dark:text-slate-200">Description (Optional)</label>
                        <textarea
                            className="flex min-h-[80px] w-full rounded-md border border-slate-200 dark:border-white/10 bg-white dark:bg-slate-950 px-3 py-2 text-sm text-slate-900 dark:text-white ring-offset-background placeholder:text-slate-400 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                            {...register('description')}
                            placeholder="Brief description of the album..."
                        />
                    </div>

                    <DialogFooter>
                        <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
                        <Button type="submit" isLoading={createMutation.isPending}>Create Album</Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
