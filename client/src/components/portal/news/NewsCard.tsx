import type { BlogPostDto } from '../../../types/news';
import { Card, CardContent } from '../../ui/Card';
import { Calendar, ArrowRight, Eye, Hash } from 'lucide-react';
import { useTranslation } from 'react-i18next';


interface NewsCardProps {
    post: BlogPostDto;
    onClick: () => void;
}

const NewsCard = ({ post, onClick }: NewsCardProps) => {
    const { t, i18n } = useTranslation();

    return (
        <Card
            variant="default"
            onClick={onClick}
            className="cursor-pointer group hover:border-[var(--color-accent)]/30 border-[var(--color-border)] hover:shadow-xl transition-all h-full flex flex-col overflow-hidden"
        >
            <div className="relative h-48 overflow-hidden bg-slate-100">
                <img
                    src={post.coverImageUrl || `https://images.unsplash.com/photo-1504711434969-e33886168f5c?q=80&w=2070&auto=format&fit=crop`}
                    alt={post.title}
                    className="w-full h-full object-cover transition-transform duration-500 group-hover:scale-110"
                />
                <div className="absolute top-4 left-4">
                    <span className="px-3 py-1 bg-white/90 backdrop-blur-md text-[var(--color-accent)] text-xs font-bold uppercase tracking-wider rounded-lg shadow-sm">
                        {post.category || 'News'}
                    </span>
                </div>
            </div>

            <CardContent className="p-6 flex-1 flex flex-col">
                <div className="flex items-center gap-2 text-xs text-[var(--color-text-muted)] mb-3">
                    <Calendar className="w-3 h-3" />
                    <span>{new Date(post.creationTime || Date.now()).toLocaleDateString(i18n.language)}</span>
                    {post.viewCount > 0 && (
                        <>
                            <span>•</span>
                            <span className="flex items-center gap-1"><Eye className="w-3 h-3" /> {post.viewCount}</span>
                        </>
                    )}
                </div>

                <h3 className="text-xl font-bold text-[var(--color-text-primary)] mb-3 group-hover:text-[var(--color-accent)] transition-colors line-clamp-2">
                    {post.title}
                </h3>

                <p className="text-sm text-[var(--color-text-secondary)] line-clamp-3 mb-4 flex-1 leading-relaxed">
                    {post.summary || post.content.replace(/<[^>]*>?/gm, '')}
                </p>

                {post.tags && (
                    <div className="flex flex-wrap gap-2 mb-4">
                        {post.tags.split(',').slice(0, 3).map(tag => (
                            <span key={tag} className="text-[10px] bg-slate-100 text-[var(--color-text-secondary)] px-2 py-0.5 rounded flex items-center gap-1">
                                <Hash className="w-2 h-2" /> {tag.trim()}
                            </span>
                        ))}
                    </div>
                )}

                <div className="pt-4 border-t border-[var(--color-border)] flex items-center justify-between text-sm font-medium text-[var(--color-accent)] opacity-0 group-hover:opacity-100 transition-opacity transform translate-y-2 group-hover:translate-y-0">
                    {t('common.read_more')}
                    <ArrowRight className="w-4 h-4 ml-1 group-hover:translate-x-1 transition-transform" />
                </div>
            </CardContent>
        </Card>
    );
};

export default NewsCard;
