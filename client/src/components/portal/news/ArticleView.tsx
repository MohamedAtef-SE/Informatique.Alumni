import { useQuery } from '@tanstack/react-query';
import { servicesAppService } from '../../../services/servicesService';
import { X, Calendar, User, Tag, Share2, Printer, ChevronLeft } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { Button } from '../../ui/Button';

interface ArticleViewProps {
    id: string;
    onClose: () => void;
}

const ArticleView = ({ id, onClose }: ArticleViewProps) => {
    const { i18n } = useTranslation();
    const { data: post, isLoading } = useQuery({
        queryKey: ['news', id],
        queryFn: () => servicesAppService.getPost(id)
    });

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/80 backdrop-blur-md p-0 md:p-4 animate-fade-in">
            <div className="bg-white w-full max-w-5xl h-full md:h-[90vh] md:rounded-2xl shadow-2xl overflow-hidden flex flex-col relative animate-slide-up">

                {/* Header / Config Bar */}
                <div className="h-16 border-b border-[var(--color-border)] flex items-center justify-between px-6 bg-white z-20 shrink-0">
                    <Button variant="ghost" onClick={onClose} className="md:hidden">
                        <ChevronLeft className="w-5 h-5 mr-1" />
                        Back
                    </Button>

                    <div className="hidden md:flex items-center gap-2 text-sm text-[var(--color-text-muted)]">
                        Reading Mode
                    </div>

                    <div className="flex items-center gap-2">
                        <Button variant="ghost" size="sm" onClick={() => window.print()} title="Print Article">
                            <Printer className="w-4 h-4" />
                        </Button>
                        <Button variant="ghost" size="sm" title="Share">
                            <Share2 className="w-4 h-4" />
                        </Button>
                        <div className="w-px h-6 bg-slate-200 mx-2 hidden md:block"></div>
                        <Button variant="ghost" size="icon" onClick={onClose} className="rounded-full hover:bg-slate-100">
                            <X className="w-6 h-6 text-[var(--color-text-secondary)]" />
                        </Button>
                    </div>
                </div>

                {/* Content Area */}
                <div className="flex-1 overflow-y-auto custom-scrollbar bg-slate-50">
                    {isLoading ? (
                        <div className="flex items-center justify-center h-full">
                            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-[var(--color-accent)]"></div>
                        </div>
                    ) : post ? (
                        <div className="max-w-3xl mx-auto bg-white min-h-full shadow-sm py-12 px-6 md:px-16">

                            {/* Article Header */}
                            <header className="mb-12 text-center">
                                <span className="inline-block px-3 py-1 bg-[var(--color-accent-light)]/20 text-[var(--color-accent)] text-xs font-bold uppercase tracking-widest rounded-full mb-6">
                                    {post.category}
                                </span>

                                <h1 className="text-3xl md:text-5xl font-heading font-bold text-[var(--color-text-primary)] mb-6 leading-tight">
                                    {post.title}
                                </h1>

                                <div className="flex flex-wrap items-center justify-center gap-6 text-sm text-[var(--color-text-muted)]">
                                    <div className="flex items-center gap-2">
                                        <div className="w-8 h-8 rounded-full bg-slate-100 flex items-center justify-center">
                                            <User className="w-4 h-4 text-slate-500" />
                                        </div>
                                        <span className="font-medium text-[var(--color-text-primary)]">{post.authorName || 'Alumni Team'}</span>
                                    </div>
                                    <div className="flex items-center gap-2">
                                        <Calendar className="w-4 h-4" />
                                        <span>{new Date(post.creationTime || Date.now()).toLocaleDateString(i18n.language, { dateStyle: 'long' })}</span>
                                    </div>
                                </div>
                            </header>

                            {/* Cover Image */}
                            {post.coverImageUrl && (
                                <div className="mb-12 rounded-xl overflow-hidden shadow-lg">
                                    <img
                                        src={post.coverImageUrl}
                                        alt={post.title}
                                        className="w-full h-auto object-cover max-h-[500px]"
                                    />
                                    {post.summary && (
                                        <div className="bg-slate-50 p-4 border-l-4 border-[var(--color-accent)] text-sm text-[var(--color-text-secondary)] italic">
                                            {post.summary}
                                        </div>
                                    )}
                                </div>
                            )}

                            {/* Main Content */}
                            <article className="prose prose-lg prose-slate max-w-none prose-headings:font-heading prose-a:text-[var(--color-accent)] prose-img:rounded-xl">
                                <div dangerouslySetInnerHTML={{ __html: post.content }} />
                            </article>

                            {/* Tags Footer */}
                            {post.tags && (
                                <div className="mt-16 pt-8 border-t border-[var(--color-border)]">
                                    <div className="flex items-center gap-3">
                                        <Tag className="w-4 h-4 text-[var(--color-text-muted)]" />
                                        <div className="flex flex-wrap gap-2">
                                            {post.tags.split(',').map(tag => (
                                                <span key={tag} className="px-3 py-1 bg-slate-100 hover:bg-slate-200 text-slate-600 text-sm rounded-full transition-colors cursor-pointer">
                                                    #{tag.trim()}
                                                </span>
                                            ))}
                                        </div>
                                    </div>
                                </div>
                            )}

                        </div>
                    ) : (
                        <div className="flex flex-col items-center justify-center h-full text-[var(--color-error)]">
                            <p className="text-lg font-bold">Failed to load article</p>
                            <Button variant="outline" onClick={onClose} className="mt-4">Close</Button>
                        </div>
                    )}
                </div>
            </div>

            {/* Backdrop click to close */}
            <div className="absolute inset-0 -z-10" onClick={onClose}></div>
        </div>
    );
};

export default ArticleView;
