import { useState, useRef } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { BASE_URL } from '../../services/api';
import { Plus, Trash2, Image, Calendar, Eye, Upload } from 'lucide-react';
import { PageHeader } from '../../components/admin/PageHeader';
import { Button } from '../../components/ui/Button';
import { CreateAlbumModal } from '../../components/admin/gallery/CreateAlbumModal';
import { ConfirmDialog } from '../../components/ui/ConfirmDialog';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from '../../components/ui/Dialog';
import { toast } from 'sonner';
import type { GalleryAlbumDto } from '../../types/gallery';

const resolveImageUrl = (url: string) => url.startsWith('http') ? url : `${BASE_URL}${url}`;

const GalleryManager = () => {
    const queryClient = useQueryClient();
    const [page, setPage] = useState(1);
    const pageSize = 9;
    const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);

    // Delete album confirm
    const [isDeleteConfirmOpen, setIsDeleteConfirmOpen] = useState(false);
    const [albumToDelete, setAlbumToDelete] = useState<{ id: string; title: string } | null>(null);

    // Album detail/preview
    const [selectedAlbumId, setSelectedAlbumId] = useState<string | null>(null);
    const [isPreviewOpen, setIsPreviewOpen] = useState(false);

    // Photo delete confirm
    const [isPhotoDeleteOpen, setIsPhotoDeleteOpen] = useState(false);
    const [photoToDelete, setPhotoToDelete] = useState<{ id: string } | null>(null);

    // File input ref
    const fileInputRef = useRef<HTMLInputElement>(null);

    const { data, isLoading } = useQuery({
        queryKey: ['admin-gallery', page],
        queryFn: () => adminService.getAlbums({ skipCount: (page - 1) * pageSize, maxResultCount: pageSize })
    });

    const albumDetailQuery = useQuery({
        queryKey: ['admin-album-detail', selectedAlbumId],
        queryFn: () => adminService.getAlbumDetails(selectedAlbumId!),
        enabled: !!selectedAlbumId && isPreviewOpen
    });

    const deleteMutation = useMutation({
        mutationFn: adminService.deleteAlbum,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-gallery'] });
            toast.success('Album deleted.');
            setIsDeleteConfirmOpen(false);
            setAlbumToDelete(null);
        },
        onError: () => { toast.error('Failed to delete album.'); setIsDeleteConfirmOpen(false); }
    });

    const uploadPhotosMutation = useMutation({
        mutationFn: (files: File[]) => adminService.uploadAlbumImages(selectedAlbumId!, files),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-album-detail', selectedAlbumId] });
            queryClient.invalidateQueries({ queryKey: ['admin-gallery'] });
            toast.success('Photos uploaded successfully!');
        },
        onError: () => toast.error('Failed to upload photos.')
    });

    const deletePhotoMutation = useMutation({
        mutationFn: (mediaItemId: string) => adminService.deleteMediaItem(selectedAlbumId!, mediaItemId),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-album-detail', selectedAlbumId] });
            queryClient.invalidateQueries({ queryKey: ['admin-gallery'] });
            toast.success('Photo removed.');
            setIsPhotoDeleteOpen(false);
            setPhotoToDelete(null);
        },
        onError: () => { toast.error('Failed to remove photo.'); setIsPhotoDeleteOpen(false); }
    });

    const handleDeleteClick = (e: React.MouseEvent, id: string, title: string) => {
        e.stopPropagation();
        setAlbumToDelete({ id, title });
        setIsDeleteConfirmOpen(true);
    };

    const confirmDelete = () => { if (albumToDelete) deleteMutation.mutate(albumToDelete.id); };

    const handleAlbumClick = (albumId: string) => {
        setSelectedAlbumId(albumId);
        setIsPreviewOpen(true);
    };

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const files = e.target.files;
        if (files && files.length > 0) {
            uploadPhotosMutation.mutate(Array.from(files));
            e.target.value = '';
        }
    };

    const totalPages = Math.ceil((data?.totalCount || 0) / pageSize);

    return (
        <div className="space-y-8 animate-in fade-in duration-500">
            <PageHeader
                title="Gallery Manager"
                description="Manage photo albums and event galleries."
                action={
                    <Button className="shadow-neon" onClick={() => setIsCreateModalOpen(true)}>
                        <Plus className="w-4 h-4 mr-2" /> New Album
                    </Button>
                }
            />

            {isLoading ? (
                <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                    {[1, 2, 3].map(i => (
                        <div key={i} className="aspect-video bg-white/5 rounded-xl animate-pulse" />
                    ))}
                </div>
            ) : data?.items.length === 0 ? (
                <div className="text-center py-20 text-slate-500 bg-white/5 rounded-xl border border-white/5 dashed">
                    <Image className="w-12 h-12 mx-auto mb-4 opacity-50" />
                    <p>No albums found. Create your first one!</p>
                </div>
            ) : (
                <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                    {data?.items.map((album: GalleryAlbumDto) => (
                        <div
                            key={album.id}
                            className="bg-slate-950/50 rounded-xl overflow-hidden border border-white/5 hover:border-primary/50 transition-all group hover:-translate-y-1 hover:shadow-lg hover:shadow-primary/10 cursor-pointer"
                            onClick={() => handleAlbumClick(album.id)}
                        >
                            <div className="aspect-video bg-slate-900 relative group-hover:scale-[1.02] transition-transform duration-500">
                                {album.coverImageUrl ? (
                                    <img src={resolveImageUrl(album.coverImageUrl!)} alt={album.title} className="w-full h-full object-cover" />
                                ) : (
                                    <div className="w-full h-full flex items-center justify-center text-slate-700">
                                        <Image className="w-12 h-12" />
                                    </div>
                                )}
                                <div className="absolute inset-0 bg-black/60 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center gap-4">
                                    <Button
                                        size="icon"
                                        className="rounded-full w-10 h-10 bg-white/20 backdrop-blur hover:bg-white/30 text-white border-0"
                                        onClick={(e) => { e.stopPropagation(); handleAlbumClick(album.id); }}
                                        title="View Album"
                                    >
                                        <Eye className="w-5 h-5" />
                                    </Button>
                                    <Button
                                        size="icon"
                                        variant="destructive"
                                        onClick={(e) => handleDeleteClick(e, album.id, album.title)}
                                        className="rounded-full w-10 h-10"
                                        title="Delete Album"
                                    >
                                        <Trash2 className="w-5 h-5" />
                                    </Button>
                                </div>
                                <div className="absolute bottom-2 right-2 px-2 py-1 bg-black/70 backdrop-blur rounded text-xs text-white font-mono">
                                    {album.photoCount} Photos
                                </div>
                            </div>
                            <div className="p-4">
                                <h3 className="text-white font-bold truncate text-lg group-hover:text-primary transition-colors">{album.title}</h3>
                                <div className="flex items-center gap-2 text-xs text-slate-400 mt-1">
                                    <Calendar className="w-3 h-3" />
                                    {album.creationTime ? new Date(album.creationTime).toLocaleDateString() : 'Recently created'}
                                </div>
                                {album.description && (
                                    <p className="text-sm text-slate-400 mt-2 line-clamp-2">{album.description}</p>
                                )}
                            </div>
                        </div>
                    ))}
                </div>
            )}

            {totalPages > 1 && (
                <div className="flex justify-center gap-2 mt-8">
                    <Button variant="ghost" disabled={page === 1} onClick={() => setPage(p => Math.max(1, p - 1))}>
                        Previous
                    </Button>
                    <div className="flex items-center px-4 text-sm text-slate-400">Page {page} of {totalPages}</div>
                    <Button variant="ghost" disabled={page === totalPages} onClick={() => setPage(p => Math.min(totalPages, p + 1))}>
                        Next
                    </Button>
                </div>
            )}

            <CreateAlbumModal open={isCreateModalOpen} onOpenChange={setIsCreateModalOpen} />

            {/* Delete Album Confirmation */}
            <ConfirmDialog
                open={isDeleteConfirmOpen}
                onOpenChange={setIsDeleteConfirmOpen}
                title="Delete Album"
                description={`Are you sure you want to delete "${albumToDelete?.title}"? All photos will be permanently removed.`}
                onConfirm={confirmDelete}
                variant="danger"
                confirmLabel="Delete"
                isLoading={deleteMutation.isPending}
            />

            {/* Delete Photo Confirmation */}
            <ConfirmDialog
                open={isPhotoDeleteOpen}
                onOpenChange={setIsPhotoDeleteOpen}
                title="Remove Photo"
                description="Are you sure you want to remove this photo from the album?"
                onConfirm={() => { if (photoToDelete) deletePhotoMutation.mutate(photoToDelete.id); }}
                variant="danger"
                confirmLabel="Remove"
                isLoading={deletePhotoMutation.isPending}
            />

            {/* Hidden file input */}
            <input
                ref={fileInputRef}
                type="file"
                accept="image/*"
                multiple
                className="hidden"
                onChange={handleFileChange}
            />

            {/* Album Preview Dialog */}
            <Dialog open={isPreviewOpen} onOpenChange={setIsPreviewOpen}>
                <DialogContent className="max-w-4xl max-h-[85vh] overflow-y-auto">
                    <DialogHeader>
                        <DialogTitle>{albumDetailQuery.data?.title || 'Album Details'}</DialogTitle>
                        <DialogDescription>{albumDetailQuery.data?.description}</DialogDescription>
                    </DialogHeader>

                    <div className="flex justify-end">
                        <Button
                            size="sm"
                            variant="outline"
                            onClick={() => fileInputRef.current?.click()}
                            disabled={uploadPhotosMutation.isPending}
                        >
                            <Upload className="w-4 h-4 mr-2" />
                            {uploadPhotosMutation.isPending ? 'Uploading...' : 'Add Photos'}
                        </Button>
                    </div>

                    {albumDetailQuery.isLoading ? (
                        <div className="grid grid-cols-2 md:grid-cols-3 gap-3 py-4">
                            {[1, 2, 3, 4, 5, 6].map(i => (
                                <div key={i} className="aspect-square bg-slate-100 dark:bg-white/5 rounded-lg animate-pulse" />
                            ))}
                        </div>
                    ) : albumDetailQuery.data?.mediaItems?.length === 0 ? (
                        <div className="text-center py-12 text-slate-500">
                            <Image className="w-10 h-10 mx-auto mb-3 opacity-50" />
                            <p className="mb-4">No photos in this album yet.</p>
                            <Button
                                variant="outline"
                                onClick={() => fileInputRef.current?.click()}
                                disabled={uploadPhotosMutation.isPending}
                            >
                                <Upload className="w-4 h-4 mr-2" /> Upload Photos
                            </Button>
                        </div>
                    ) : (
                        <div className="grid grid-cols-2 md:grid-cols-3 gap-3 py-4">
                            {albumDetailQuery.data?.mediaItems?.map((item: any) => (
                                <div key={item.id} className="aspect-square rounded-lg overflow-hidden bg-slate-100 dark:bg-white/5 relative group/photo">
                                    <img
                                        src={resolveImageUrl(item.url)}
                                        alt=""
                                        className="w-full h-full object-cover hover:scale-105 transition-transform duration-300"
                                    />
                                    <div className="absolute inset-0 bg-black/50 opacity-0 group-hover/photo:opacity-100 transition-opacity flex items-center justify-center">
                                        <Button
                                            size="icon"
                                            variant="destructive"
                                            className="rounded-full w-9 h-9"
                                            onClick={() => {
                                                setPhotoToDelete({ id: item.id });
                                                setIsPhotoDeleteOpen(true);
                                            }}
                                            title="Remove photo"
                                        >
                                            <Trash2 className="w-4 h-4" />
                                        </Button>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </DialogContent>
            </Dialog>
        </div>
    );
};

export default GalleryManager;
