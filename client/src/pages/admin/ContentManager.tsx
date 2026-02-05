import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { Plus, Edit, Trash2, Search, FileText } from 'lucide-react';
import type { BlogPostDto } from '../../types/news';

const ContentManager = () => {
    const [filter, setFilter] = useState('');

    const { data } = useQuery({
        queryKey: ['admin-posts', filter],
        queryFn: () => adminService.getPosts({ filter, maxResultCount: 20 })
    });

    return (
        <div className="space-y-6">
            <div className="flex justify-between items-center">
                <h1 className="text-2xl font-bold text-white">Content Manager</h1>
                <div className="flex gap-2">
                    <div className="relative">
                        <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-500" />
                        <input
                            type="text"
                            placeholder="Search posts..."
                            className="pl-9 pr-4 py-2 rounded-lg bg-[var(--glass-border)] text-white focus:outline-none focus:border-[var(--color-accent)] border border-transparent"
                            value={filter}
                            onChange={(e) => setFilter(e.target.value)}
                        />
                    </div>
                    <button className="btn-primary flex items-center gap-2 px-4 py-2">
                        <Plus className="w-4 h-4" /> New Post
                    </button>
                </div>
            </div>

            <div className="glass-panel overflow-hidden">
                <table className="w-full text-left text-sm">
                    <thead className="bg-white/5 text-gray-400 uppercase tracking-wider text-xs">
                        <tr>
                            <th className="px-6 py-4">Title</th>
                            <th className="px-6 py-4">Category</th>
                            <th className="px-6 py-4">Status</th>
                            <th className="px-6 py-4">Created At</th>
                            <th className="px-6 py-4 text-right">Actions</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-white/5">


                        {data?.items.map((post: BlogPostDto) => (
                            <tr key={post.id} className="hover:bg-white/5 transition-colors">
                                <td className="px-6 py-4">
                                    <div className="text-white font-medium flex items-center gap-2">
                                        <FileText className="w-4 h-4 text-gray-500" />
                                        {post.title}
                                    </div>
                                </td>
                                <td className="px-6 py-4 text-gray-300">{post.category || 'Uncategorized'}</td>
                                <td className="px-6 py-4">
                                    <span className={`px-2 py-1 rounded text-xs font-bold uppercase ${post.isPublished ? 'bg-emerald-500/20 text-emerald-500' : 'bg-gray-500/20 text-gray-400'}`}>
                                        {post.isPublished ? 'Published' : 'Draft'}
                                    </span>
                                </td>
                                <td className="px-6 py-4 text-gray-400">{post.creationTime ? new Date(post.creationTime).toLocaleDateString() : 'N/A'}</td>
                                <td className="px-6 py-4 text-right flex justify-end gap-2">
                                    <button className="p-1 rounded bg-white/10 text-gray-300 hover:bg-white/20 hover:text-white transition-colors" title="Edit">
                                        <Edit className="w-4 h-4" />
                                    </button>
                                    <button className="p-1 rounded bg-red-500/20 text-red-400 hover:bg-red-500 hover:text-white transition-colors" title="Delete">
                                        <Trash2 className="w-4 h-4" />
                                    </button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
};

export default ContentManager;
