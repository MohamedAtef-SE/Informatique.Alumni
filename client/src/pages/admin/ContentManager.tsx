import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { Plus, Edit, Trash2, FileText, CheckCircle, XCircle } from 'lucide-react';
import type { BlogPostDto } from '../../types/news';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/Table';
import { Button } from '../../components/ui/Button';
import { toast } from 'sonner';
import { PageHeader } from '../../components/admin/PageHeader';
import { DataTableShell } from '../../components/admin/DataTableShell';
import { StatusBadge } from '../../components/admin/StatusBadge';
import { CreatePostModal } from '../../components/admin/content/CreatePostModal';
import { EditPostModal } from '../../components/admin/content/EditPostModal';
import { ConfirmDialog } from '../../components/ui/ConfirmDialog';

const ContentManager = () => {
    const queryClient = useQueryClient();
    const [filter, setFilter] = useState('');
    const [page, setPage] = useState(1);
    const pageSize = 10;
    const [statusFilter, setStatusFilter] = useState<'all' | 'published' | 'draft'>('all');

    // Modal States
    const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
    const [isEditModalOpen, setIsEditModalOpen] = useState(false);
    const [editingPost, setEditingPost] = useState<BlogPostDto | null>(null);

    // Delete Confirmation State
    const [isDeleteConfirmOpen, setIsDeleteConfirmOpen] = useState(false);
    const [postToDelete, setPostToDelete] = useState<BlogPostDto | null>(null);

    const { data, isLoading } = useQuery({
        queryKey: ['admin-posts', filter, page, statusFilter],
        queryFn: async () => {
            const result = await adminService.getPosts({
                filter,
                skipCount: (page - 1) * pageSize,
                maxResultCount: pageSize
            });
            return result;
        }
    });

    const deleteMutation = useMutation({
        mutationFn: adminService.deletePost,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-posts'] });
            toast.success('Post Deleted.');
            setIsDeleteConfirmOpen(false);
            setPostToDelete(null);
        },
        onError: () => {
            toast.error('Failed to delete post.');
        }
    });

    const publishMutation = useMutation({
        mutationFn: adminService.publishPost,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-posts'] });
            toast.success('Post Published.');
        },
        onError: () => {
            toast.error('Failed to publish post.');
        }
    });

    const unpublishMutation = useMutation({
        mutationFn: adminService.unpublishPost,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-posts'] });
            toast.success('Post Unpublished.');
        },
        onError: () => {
            toast.error('Failed to unpublish post.');
        }
    });

    const handleDeleteClick = (post: BlogPostDto) => {
        setPostToDelete(post);
        setIsDeleteConfirmOpen(true);
    };

    const confirmDelete = () => {
        if (postToDelete) {
            deleteMutation.mutate(postToDelete.id);
        }
    };

    const handleEditClick = (post: BlogPostDto) => {
        setEditingPost(post);
        setIsEditModalOpen(true);
    };

    const filteredItems = data?.items.filter(item => {
        if (statusFilter === 'all') return true;
        if (statusFilter === 'published') return item.isPublished;
        if (statusFilter === 'draft') return !item.isPublished;
        return true;
    });

    return (
        <div className="space-y-8 animate-in fade-in duration-500">
            <PageHeader
                title="Content Manager"
                description="Manage blog posts, news, and articles."
                action={
                    <Button onClick={() => setIsCreateModalOpen(true)} className="shadow-neon">
                        <Plus className="w-4 h-4 mr-2" /> New Post
                    </Button>
                }
            />

            {/* Status Tabs */}
            <div className="flex space-x-2 border-b border-white/10 pb-4">
                {[
                    { label: 'All Posts', value: 'all' },
                    { label: 'Published', value: 'published' },
                    { label: 'Drafts', value: 'draft' }
                ].map((tab) => (
                    <Button
                        key={tab.label}
                        variant={statusFilter === tab.value ? 'default' : 'ghost'}
                        size="sm"
                        onClick={() => {
                            setStatusFilter(tab.value as any);
                            setPage(1);
                        }}
                    >
                        {tab.label}
                    </Button>
                ))}
            </div>

            <DataTableShell
                searchPlaceholder="Search posts..."
                onSearch={setFilter}
                pagination={{
                    currentPage: page,
                    totalPages: Math.ceil((data?.totalCount || 0) / pageSize),
                    onPageChange: setPage
                }}
            >
                <Table>
                    <TableHeader>
                        <TableRow className="hover:bg-transparent border-white/5">
                            <TableHead className="w-[40%]">Title</TableHead>
                            <TableHead>Category</TableHead>
                            <TableHead>Status</TableHead>
                            <TableHead>Created At</TableHead>
                            <TableHead className="text-right">Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {isLoading ? (
                            <TableRow>
                                <TableCell colSpan={5} className="h-32 text-center text-slate-400">Loading posts...</TableCell>
                            </TableRow>
                        ) : filteredItems?.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={5} className="h-32 text-center text-slate-500">
                                    No posts found.
                                </TableCell>
                            </TableRow>
                        ) : (
                            filteredItems?.map((post: BlogPostDto) => (
                                <TableRow key={post.id}>
                                    <TableCell>
                                        <div className="flex items-center gap-3">
                                            <div className="w-8 h-8 rounded bg-slate-800 flex items-center justify-center text-slate-500">
                                                <FileText className="w-4 h-4" />
                                            </div>
                                            <div>
                                                <div className="text-slate-900 dark:text-slate-200 font-medium">{post.title}</div>
                                                <div className="text-xs text-slate-500 mt-0.5">By {post.authorName || 'System Admin'}</div>
                                            </div>
                                        </div>
                                    </TableCell>
                                    <TableCell className="text-slate-600">{post.category || 'Uncategorized'}</TableCell>
                                    <TableCell>
                                        <StatusBadge variant={post.isPublished ? 'success' : 'secondary'}>
                                            {post.isPublished ? 'Published' : 'Draft'}
                                        </StatusBadge>
                                    </TableCell>
                                    <TableCell className="text-slate-500">
                                        {post.creationTime ? new Date(post.creationTime).toLocaleDateString() : 'N/A'}
                                    </TableCell>
                                    <TableCell className="text-right">
                                        <div className="flex justify-end gap-2">
                                            {post.isPublished ? (
                                                <Button
                                                    size="icon"
                                                    variant="ghost"
                                                    className="text-orange-400 hover:text-orange-300 hover:bg-orange-500/20"
                                                    onClick={() => unpublishMutation.mutate(post.id)}
                                                    disabled={unpublishMutation.isPending}
                                                    title="Unpublish"
                                                >
                                                    <XCircle className="w-4 h-4" />
                                                </Button>
                                            ) : (
                                                <Button
                                                    size="icon"
                                                    variant="ghost"
                                                    className="text-green-400 hover:text-green-300 hover:bg-green-500/20"
                                                    onClick={() => publishMutation.mutate(post.id)}
                                                    disabled={publishMutation.isPending}
                                                    title="Publish"
                                                >
                                                    <CheckCircle className="w-4 h-4" />
                                                </Button>
                                            )}
                                            <Button
                                                size="icon"
                                                variant="ghost"
                                                className="text-slate-400 hover:text-white hover:bg-white/10"
                                                onClick={() => handleEditClick(post)}
                                                title="Edit"
                                            >
                                                <Edit className="w-4 h-4" />
                                            </Button>
                                            <Button
                                                size="icon"
                                                variant="ghost"
                                                className="text-red-400 hover:text-red-300 hover:bg-red-500/20"
                                                onClick={() => handleDeleteClick(post)}
                                                disabled={deleteMutation.isPending}
                                                title="Delete"
                                            >
                                                <Trash2 className="w-4 h-4" />
                                            </Button>
                                        </div>
                                    </TableCell>
                                </TableRow>
                            ))
                        )}
                    </TableBody>
                </Table>
            </DataTableShell>

            <CreatePostModal open={isCreateModalOpen} onOpenChange={setIsCreateModalOpen} />

            <EditPostModal
                open={isEditModalOpen}
                onOpenChange={setIsEditModalOpen}
                post={editingPost}
            />

            <ConfirmDialog
                open={isDeleteConfirmOpen}
                onOpenChange={setIsDeleteConfirmOpen}
                title="Delete Post"
                description={`Are you sure you want to delete "${postToDelete?.title}"? This action cannot be undone.`}
                confirmLabel="Delete"
                variant="danger"
                onConfirm={confirmDelete}
                isLoading={deleteMutation.isPending}
            />
        </div>
    );
};

export default ContentManager;
