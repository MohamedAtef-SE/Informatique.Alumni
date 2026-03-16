import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { Button } from '../ui/Button';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '../ui/Dialog';
import { Input } from '../ui/Input';

const approveSchema = z.object({
    meetingLink: z.string().optional().or(z.literal(''))
});

type ApproveFormValues = z.infer<typeof approveSchema>;

interface ApproveModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    title?: string;
    description?: string;
    onConfirm: (meetingLink: string) => void;
    isLoading?: boolean;
}

export function ApproveModal({
    open,
    onOpenChange,
    title = "Approve Session",
    description = "Please provide the Meeting URL (e.g. Google Meet or Zoom) for this session.",
    onConfirm,
    isLoading
}: ApproveModalProps) {
    const { register, handleSubmit, formState: { errors }, reset } = useForm<ApproveFormValues>({
        resolver: zodResolver(approveSchema),
        defaultValues: { meetingLink: '' }
    });

    const onSubmit = (data: ApproveFormValues) => {
        onConfirm(data.meetingLink || '');
        reset();
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle className="text-slate-900">{title}</DialogTitle>
                    <DialogDescription className="text-slate-600">{description}</DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                    <div className="space-y-2">
                        <label className="text-sm font-medium text-slate-700">Meeting URL</label>
                        <Input
                            {...register('meetingLink')}
                            placeholder="https://meet.google.com/..."
                            className="bg-white border-slate-200 text-slate-900"
                        />
                        {errors.meetingLink && <p className="text-red-500 text-xs">{errors.meetingLink.message}</p>}
                    </div>

                    <DialogFooter>
                        <Button type="button" variant="outline" onClick={() => onOpenChange(false)} className="text-slate-700 border-slate-300">Cancel</Button>
                        <Button type="submit" variant="default" isLoading={isLoading} className="bg-emerald-600 hover:bg-emerald-700 text-white">Confirm Approve</Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
