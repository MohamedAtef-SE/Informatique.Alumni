import { useQuery } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { Plus, Trash2, Image } from 'lucide-react';

const GalleryManager = () => {
    const { data } = useQuery({
        queryKey: ['admin-gallery'],
        queryFn: () => adminService.getAlbums({ maxResultCount: 20 })
    });

    return (
        <div className="space-y-6">
            <div className="flex justify-between items-center">
                <h1 className="text-2xl font-bold text-white">Gallery Manager</h1>
                <button className="btn-primary flex items-center gap-2 px-4 py-2">
                    <Plus className="w-4 h-4" /> New Album
                </button>
            </div>

            <div className="glass-panel overflow-hidden">
                <div className="grid grid-cols-1 md:grid-cols-3 gap-6 p-6">


                    {data?.items.map((album: any) => (
                        <div key={album.id} className="bg-black/20 rounded-xl overflow-hidden border border-white/5 hover:border-[var(--color-accent)]/50 transition-colors group">
                            <div className="aspect-video bg-gray-800 relative">
                                {album.coverImageUrl ? (
                                    <img src={album.coverImageUrl} alt={album.title} className="w-full h-full object-cover" />
                                ) : (
                                    <div className="w-full h-full flex items-center justify-center text-gray-600">
                                        <Image className="w-12 h-12" />
                                    </div>
                                )}
                                <div className="absolute inset-0 bg-black/60 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center gap-4">
                                    <button className="p-2 rounded-full bg-red-600 text-white hover:bg-red-700 transition-colors"><Trash2 className="w-5 h-5" /></button>
                                </div>
                            </div>
                            <div className="p-4">
                                <h3 className="text-white font-bold truncate">{album.title}</h3>
                                <p className="text-xs text-gray-400">{new Date(album.eventDate).toLocaleDateString()} • {album.photoCount} Photos</p>
                            </div>
                        </div>
                    ))}
                </div>
            </div>
        </div>
    );
};

export default GalleryManager;
