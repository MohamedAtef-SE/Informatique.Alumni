import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { Button } from '../ui/Button';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '../ui/Dialog';
import { Input } from '../ui/Input';

const reasonSchema = z.object({
    reason: z.string().min(5, "Reason is required (min 5 characters)")
});

type ReasonFormValues = z.infer<typeof reasonSchema>;

interface ReasonModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    title?: string;
    description?: string;
    onConfirm: (reason: string) => void;
    isLoading?: boolean;
}

export function ReasonModal({
    open,
    onOpenChange,
    title = "Reject Request",
    description = "Please provide a reason for this action.",
    onConfirm,
    isLoading
}: ReasonModalProps) {
    const { register, handleSubmit, formState: { errors }, reset } = useForm<ReasonFormValues>({
        resolver: zodResolver(reasonSchema),
        defaultValues: { reason: '' }
    });

    const onSubmit = (data: ReasonFormValues) => {
        onConfirm(data.reason);
        reset();
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>{title}</DialogTitle>
                    <DialogDescription>{description}</DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                    <div className="space-y-2">
                        <label className="text-sm font-medium text-slate-200">Reason / Note</label>
                        <Input {...register('reason')} placeholder="Type your reason here..." />
                        {errors.reason && <p className="text-red-500 text-xs">{errors.reason.message}</p>}
                    </div>

                    <DialogFooter>
                        <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
                        <Button type="submit" variant="destructive" isLoading={isLoading}>Confirm Reject</Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
