import type { BlogPostDto } from '../../../types/news';
import { Button } from '../../ui/Button';
import { ArrowRight, Calendar, User } from 'lucide-react';
import { useTranslation } from 'react-i18next';

interface FeaturedNewsProps {
    post: BlogPostDto;
    onClick: () => void;
}

const FeaturedNews = ({ post, onClick }: FeaturedNewsProps) => {
    const { t, i18n } = useTranslation();

    return (
        <div className="relative rounded-2xl overflow-hidden shadow-2xl group cursor-pointer mb-12 h-[500px]" onClick={onClick}>
            {/* Background Image */}
            <div className="absolute inset-0">
                <img
                    src={post.coverImageUrl || "https://images.unsplash.com/photo-1523050854058-8df90110c9f1?q=80&w=2070&auto=format&fit=crop"}
                    alt={post.title}
                    className="w-full h-full object-cover transition-transform duration-700 group-hover:scale-105"
                />
                <div className="absolute inset-0 bg-gradient-to-t from-black via-black/50 to-transparent opacity-90"></div>
            </div>

            {/* Content */}
            <div className="absolute bottom-0 left-0 right-0 p-8 md:p-12 text-white">
                <div className="max-w-4xl animate-slide-up">
                    <div className="flex items-center gap-3 mb-4">
                        <span className="px-3 py-1 bg-[var(--color-accent)] text-white text-xs font-bold uppercase tracking-wider rounded-full shadow-lg shadow-blue-500/20">
                            {post.category || 'News'}
                        </span>
                        {post.isFeatured && (
                            <span className="px-2 py-0.5 border border-white/30 text-white/80 text-[10px] font-medium rounded uppercase tracking-widest backdrop-blur-sm">
                                Featured
                            </span>
                        )}
                    </div>

                    <h2 className="text-3xl md:text-5xl font-heading font-bold mb-4 leading-tight group-hover:text-blue-200 transition-colors">
                        {post.title}
                    </h2>

                    {post.summary && (
                        <p className="text-lg md:text-xl text-gray-200 mb-6 line-clamp-2 max-w-2xl font-light">
                            {post.summary}
                        </p>
                    )}

                    <div className="flex flex-wrap items-center gap-6 text-sm text-gray-300 mb-8 border-t border-white/10 pt-6 mt-6">
                        <div className="flex items-center gap-2">
                            <div className="w-8 h-8 rounded-full bg-white/10 flex items-center justify-center">
                                <User className="w-4 h-4" />
                            </div>
                            <span className="font-medium text-white">{post.authorName || 'Alumni Team'}</span>
                        </div>
                        <div className="flex items-center gap-2">
                            <Calendar className="w-4 h-4" />
                            <span>{new Date(post.creationTime || Date.now()).toLocaleDateString(i18n.language, { year: 'numeric', month: 'long', day: 'numeric' })}</span>
                        </div>
                        {post.viewCount > 0 && (
                            <div className="flex items-center gap-2">
                                <span className="w-1 h-1 bg-gray-500 rounded-full"></span>
                                <span>{post.viewCount} Views</span>
                            </div>
                        )}
                    </div>

                    <Button
                        variant="default"
                        className="py-6 px-8 text-lg group-hover:translate-x-2 transition-transform shadow-none bg-white text-black hover:bg-white/90 border-transparent"
                    >
                        {t('common.read_more')} <ArrowRight className="w-5 h-5 ml-2" />
                    </Button>
                </div>
            </div>
        </div>
    );
};

export default FeaturedNews;
