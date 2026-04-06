import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../../services/adminService';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '../../ui/Dialog';
import { Button } from '../../ui/Button';
import { Input } from '../../ui/Input';
import { Label } from '../../ui/Label';
import { toast } from 'sonner';
import { FileUp, Loader2 } from 'lucide-react';

interface CreateMagazineModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
}

export const CreateMagazineModal = ({ open, onOpenChange }: CreateMagazineModalProps) => {
    const queryClient = useQueryClient();
    const [title, setTitle] = useState('');
    const [publishDate, setPublishDate] = useState(new Date().toISOString().split('T')[0]);
    const [file, setFile] = useState<File | null>(null);

    const createMutation = useMutation({
        mutationFn: adminService.createMagazine,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-magazines'] });
            toast.success('Magazine issue uploaded successfully!');
            onOpenChange(false);
            resetForm();
        },
        onError: (error: any) => {
            console.error('Upload error:', error);
            toast.error(error.response?.data?.error?.message || 'Failed to upload magazine issue.');
        }
    });

    const resetForm = () => {
        setTitle('');
        setPublishDate(new Date().toISOString().split('T')[0]);
        setFile(null);
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        if (!title || !file || !publishDate) {
            toast.error('Please fill all required fields and select a PDF file.');
            return;
        }

        if (file.type !== 'application/pdf') {
            toast.error('Please select a valid PDF file.');
            return;
        }

        createMutation.mutate({
            title,
            publishDate: new Date(publishDate).toISOString(),
            file
        });
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="sm:max-w-[500px]">
                <DialogHeader>
                    <DialogTitle>Upload New Magazine Issue</DialogTitle>
                    <DialogDescription>
                        Add a new PDF issue to the Alumni Magazine section.
                    </DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit} className="space-y-6 pt-4">
                    <div className="space-y-2">
                        <Label htmlFor="title">Issue Title</Label>
                        <Input
                            id="title"
                            placeholder="e.g., Spring 2024 - Issue #12"
                            value={title}
                            onChange={(e) => setTitle(e.target.value)}
                            required
                        />
                    </div>

                    <div className="space-y-2">
                        <Label htmlFor="publishDate">Publish Date</Label>
                        <Input
                            id="publishDate"
                            type="date"
                            value={publishDate}
                            onChange={(e) => setPublishDate(e.target.value)}
                            required
                        />
                    </div>

                    <div className="space-y-2">
                        <Label>Magazine PDF File</Label>
                        <div className="relative">
                            <input
                                type="file"
                                id="pdf-upload"
                                className="hidden"
                                accept="application/pdf"
                                onChange={(e) => setFile(e.target.files?.[0] || null)}
                            />
                            <label
                                htmlFor="pdf-upload"
                                className="flex flex-col items-center justify-center w-full h-32 border-2 border-dashed border-white/10 rounded-xl hover:border-accent/50 hover:bg-white/5 transition-all cursor-pointer group"
                            >
                                <FileUp className={file ? "w-8 h-8 text-green-500 mb-2" : "w-8 h-8 text-slate-500 mb-2 group-hover:text-accent"} />
                                <span className="text-sm text-slate-400">
                                    {file ? file.name : "Click to select PDF file"}
                                </span>
                                <span className="text-xs text-slate-500 mt-1">Maximum size: 50MB</span>
                            </label>
                        </div>
                    </div>

                    <DialogFooter className="pt-4 border-t border-white/5">
                        <Button
                            type="button"
                            variant="ghost"
                            onClick={() => onOpenChange(false)}
                            disabled={createMutation.isPending}
                        >
                            Cancel
                        </Button>
                        <Button
                            type="submit"
                            className="shadow-neon px-8"
                            disabled={createMutation.isPending}
                        >
                            {createMutation.isPending ? (
                                <>
                                    <Loader2 className="w-4 h-4 mr-2 animate-spin text-white" />
                                    <span>...</span>
                                </>
                            ) : (
                                'Upload Issue'
                            )}
                        </Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
};
