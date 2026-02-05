import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { galleryService } from '../../services/galleryService';
import { Image, ArrowLeft, X, ChevronLeft, ChevronRight } from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import { Card } from '../../components/ui/Card';

const Gallery = () => {
    const [selectedAlbumId, setSelectedAlbumId] = useState<string | null>(null);
    const { t } = useTranslation();

    return (
        <div className="space-y-8 animate-fade-in">
            <div className="flex items-center gap-4">
                {selectedAlbumId && (
                    <button onClick={() => setSelectedAlbumId(null)} className="p-2 hover:bg-slate-100 rounded-full transition-colors text-[var(--color-text-secondary)] hover:text-[var(--color-accent)]">
                        <ArrowLeft className="w-6 h-6" />
                    </button>
                )}
                <div>
                    <h1 className="text-3xl font-heading font-bold text-[var(--color-text-primary)]">{selectedAlbumId ? t('gallery.album_photos') : t('gallery.title')}</h1>
                    <p className="text-[var(--color-text-secondary)] mt-1">{t('gallery.subtitle')}</p>
                </div>
            </div>

            {selectedAlbumId ? (
                <AlbumView albumId={selectedAlbumId} />
            ) : (
                <AlbumsList onSelect={setSelectedAlbumId} />
            )}
        </div>
    );
};

const AlbumsList = ({ onSelect }: { onSelect: (id: string) => void }) => {
    const { t } = useTranslation();
    const { data, isError } = useQuery({ queryKey: ['albums'], queryFn: () => galleryService.getAlbums({ maxResultCount: 20 }) });

    const items = data?.items ?? [];

    if (isError) return <p className="text-center py-20 text-[var(--color-error)]">{t('gallery.error_loading')}</p>;

    return (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 animate-slide-up">
            {items.map((album) => (
                <div
                    key={album.id}
                    onClick={() => onSelect(album.id)}
                    className="group cursor-pointer"
                >
                    <Card variant="default" className="overflow-hidden hover:shadow-lg hover:border-[var(--color-accent)]/50 transition-all duration-300 border-[var(--color-border)]">
                        <div className="h-56 bg-slate-100 relative overflow-hidden">
                            {album.coverImageUrl ? (
                                <img src={album.coverImageUrl} alt={album.title} className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-700" />
                            ) : (
                                <div className="w-full h-full flex items-center justify-center text-[var(--color-text-muted)] bg-slate-50">
                                    <Image className="w-12 h-12 opacity-50" />
                                </div>
                            )}
                            <div className="absolute inset-0 bg-gradient-to-t from-black/80 via-black/20 to-transparent opacity-80 group-hover:opacity-100 transition-opacity" />
                            <div className="absolute bottom-0 left-0 w-full p-4 pt-12 transform group-hover:translate-y-[-4px] transition-transform">
                                <h3 className="text-xl font-bold text-white mb-1 shadow-sm">{album.title}</h3>
                                <p className="text-sm text-white/80 font-medium">{album.photoCount} {t('gallery.photos_count')}</p>
                            </div>
                        </div>
                    </Card>
                </div>
            ))}
            {items.length === 0 && (
                <div className="col-span-full text-center py-20 text-[var(--color-text-muted)] bg-slate-50 rounded-xl border border-dashed border-[var(--color-border)]">
                    {t('gallery.no_albums')}
                </div>
            )}
        </div>
    );
};

const AlbumView = ({ albumId }: { albumId: string }) => {
    const { t } = useTranslation();
    const { data: photos, isError } = useQuery({ queryKey: ['photos', albumId], queryFn: () => galleryService.getPhotos(albumId) });
    const [lightboxIndex, setLightboxIndex] = useState<number | null>(null);

    const photoList = photos ?? [];

    if (isError) return <p className="text-center py-20 text-[var(--color-error)]">{t('gallery.error_photos')}</p>;

    return (
        <>
            <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4 animate-slide-up">
                {photoList.map((photo, index) => (
                    <motion.div
                        key={photo.id}
                        layoutId={`photo-${photo.id}`}
                        onClick={() => setLightboxIndex(index)}
                        className="aspect-square rounded-xl overflow-hidden bg-slate-100 hover:scale-[1.02] hover:shadow-lg transition-all cursor-pointer relative group border border-[var(--color-border)]"
                    >
                        <img src={photo.url} alt="Gallery" className="w-full h-full object-cover" />
                        <div className="absolute inset-0 bg-black/0 group-hover:bg-black/10 transition-colors" />
                    </motion.div>
                ))}
                {photoList.length === 0 && (
                    <div className="col-span-full text-center py-20 text-[var(--color-text-muted)] bg-slate-50 rounded-xl border border-dashed border-[var(--color-border)]">
                        {t('gallery.no_photos')}
                    </div>
                )}
            </div>

            <AnimatePresence>
                {lightboxIndex !== null && (
                    <Lightbox
                        photos={photoList}
                        initialIndex={lightboxIndex}
                        onClose={() => setLightboxIndex(null)}
                    />
                )}
            </AnimatePresence>
        </>
    );
};

const Lightbox = ({ photos, initialIndex, onClose }: { photos: any[], initialIndex: number, onClose: () => void }) => {
    const [currentIndex, setCurrentIndex] = useState(initialIndex);

    // Sync internal state if initialIndex changes (though mostly for mount)
    useEffect(() => {
        setCurrentIndex(initialIndex);
    }, [initialIndex]);

    const handleNext = (e?: React.MouseEvent) => {
        e?.stopPropagation();
        setCurrentIndex((prev) => (prev + 1) % photos.length);
    };

    const handlePrev = (e?: React.MouseEvent) => {
        e?.stopPropagation();
        setCurrentIndex((prev) => (prev - 1 + photos.length) % photos.length);
    };

    // Keyboard navigation
    useEffect(() => {
        const handleKeyDown = (e: KeyboardEvent) => {
            if (e.key === 'Escape') onClose();
            if (e.key === 'ArrowRight') handleNext();
            if (e.key === 'ArrowLeft') handlePrev();
        };
        window.addEventListener('keydown', handleKeyDown);
        return () => window.removeEventListener('keydown', handleKeyDown);
    }, [onClose]);

    const currentPhoto = photos[currentIndex];

    return (
        <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            onClick={onClose}
            className="fixed inset-0 z-[100] flex items-center justify-center bg-black/95 backdrop-blur-sm p-4"
        >
            <button
                onClick={onClose}
                className="absolute top-4 right-4 p-2 text-white/50 hover:text-white hover:bg-white/10 rounded-full transition-colors z-[110]"
            >
                <X className="w-8 h-8" />
            </button>

            <button
                onClick={handlePrev}
                className="absolute left-4 p-4 text-white/50 hover:text-white hover:bg-white/10 rounded-full transition-colors z-50 hidden md:block"
            >
                <ChevronLeft className="w-10 h-10" />
            </button>

            <motion.div
                key={currentPhoto.id}
                layoutId={`photo-${currentPhoto.id}`}
                initial={{ opacity: 0, scale: 0.9 }}
                animate={{ opacity: 1, scale: 1 }}
                exit={{ opacity: 0, scale: 0.9 }}
                transition={{ type: "spring", damping: 25, stiffness: 300 }}
                className="relative max-w-7xl max-h-[90vh] flex items-center justify-center pointer-events-none w-full h-full"
                onClick={(e) => e.stopPropagation()}
            >
                <img
                    src={currentPhoto.url}
                    alt="Lightbox"
                    className="max-w-full max-h-[85vh] object-contain rounded-md shadow-2xl pointer-events-auto"
                />
            </motion.div>

            <button
                onClick={handleNext}
                className="absolute right-4 p-4 text-white/50 hover:text-white hover:bg-white/10 rounded-full transition-colors z-50 hidden md:block"
            >
                <ChevronRight className="w-10 h-10" />
            </button>

            <div className="absolute bottom-4 left-0 right-0 text-center text-white/50 text-sm font-medium">
                {currentIndex + 1} / {photos.length}
            </div>
        </motion.div>
    );
};

export default Gallery;
