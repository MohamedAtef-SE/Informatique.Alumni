import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../../services/adminService';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '../../../components/ui/Dialog';
import { Button } from '../../../components/ui/Button';
import { Input } from '../../../components/ui/Input';
import { Label } from '../../../components/ui/Label';
import { Textarea } from '../../../components/ui/Textarea';
import { toast } from 'sonner';
import type { CertificateDefinitionDto } from '../../../types/certificates';
import { Loader2, Plus, X } from 'lucide-react';

interface CertificateDefinitionModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    definition?: CertificateDefinitionDto | null;
}

export const CertificateDefinitionModal = ({ open, onOpenChange, definition }: CertificateDefinitionModalProps) => {
    const queryClient = useQueryClient();
    const isEdit = !!definition;

    const { register, handleSubmit, reset, watch, setValue, formState: { errors } } = useForm<any>({
        defaultValues: {
            nameAr: '',
            nameEn: '',
            fee: 0,
            degreeType: 1,
            description: '',
            requiredDocuments: []
        }
    });

    const [newDocName, setNewDocName] = useState('');
    const requiredDocs = watch('requiredDocuments') || [];

    useEffect(() => {
        if (definition) {
            reset({
                nameAr: definition.nameAr,
                nameEn: definition.nameEn,
                fee: definition.fee,
                degreeType: definition.degreeType,
                description: definition.description || '',
                requiredDocuments: definition.requiredDocuments ? definition.requiredDocuments.split(',').filter(s => !!s.trim()).map(s => s.trim()) : []
            });
        } else {
            reset({
                nameAr: '',
                nameEn: '',
                fee: 0,
                degreeType: 1,
                description: '',
                requiredDocuments: []
            });
        }
    }, [definition, reset, open]);

    const mutation = useMutation({
        mutationFn: (data: any) => {
            const payload = {
                ...data,
                requiredDocuments: Array.isArray(data.requiredDocuments) 
                    ? data.requiredDocuments.join(', ') 
                    : data.requiredDocuments
            };

            if (isEdit && definition) {
                return adminService.updateCertificateDefinition(definition.id, { 
                    ...payload, 
                    isActive: definition.isActive 
                });
            }
            return adminService.createCertificateDefinition(payload);
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-certificate-definitions'] });
            toast.success(`Certificate type ${isEdit ? 'updated' : 'created'} successfully`);
            onOpenChange(false);
        },
        // ... handled in component
    });

    const onSubmit = (data: any) => {
        mutation.mutate(data);
    };

    const addDocument = () => {
        if (newDocName.trim()) {
            setValue('requiredDocuments', [...requiredDocs, newDocName.trim()]);
            setNewDocName('');
        }
    };

    const handleKeyPress = (e: React.KeyboardEvent) => {
        if (e.key === 'Enter') {
            e.preventDefault();
            addDocument();
        }
    };

    const removeDocument = (index: number) => {
        const updated = [...requiredDocs];
        updated.splice(index, 1);
        setValue('requiredDocuments', updated);
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="sm:max-w-[500px]">
                <DialogHeader>
                    <DialogTitle>{isEdit ? 'Edit Certificate Type' : 'Create New Certificate Type'}</DialogTitle>
                </DialogHeader>

                <form onSubmit={handleSubmit(onSubmit)} className="space-y-4 py-4">
                    <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                            <Label htmlFor="nameEn">Name (English)</Label>
                            <Input
                                id="nameEn"
                                {...register('nameEn', { required: 'English name is required' })}
                                placeholder="e.g. Graduation Certificate"
                            />
                            {errors.nameEn && <p className="text-xs text-red-500">{errors.nameEn.message as string}</p>}
                        </div>
                        <div className="space-y-2 text-right">
                            <Label htmlFor="nameAr">الاسم (بالعربية)</Label>
                            <Input
                                id="nameAr"
                                className="text-right font-arabic"
                                {...register('nameAr', { required: 'Arabic name is required' })}
                                placeholder="مثلاً شهادة تخرج"
                            />
                            {errors.nameAr && <p className="text-xs text-red-500">{errors.nameAr.message as string}</p>}
                        </div>
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                            <Label htmlFor="fee">Fee (EGP)</Label>
                            <Input
                                id="fee"
                                type="number"
                                {...register('fee', { required: 'Fee is required', min: 0, valueAsNumber: true })}
                            />
                            {errors.fee && <p className="text-xs text-red-500">{errors.fee.message as string}</p>}
                        </div>
                        <div className="space-y-2">
                            <Label htmlFor="degreeType">Degree Level</Label>
                            <select
                                id="degreeType"
                                className="w-full h-10 px-3 py-2 rounded-md border border-slate-200 dark:border-white/10 bg-white dark:bg-white/5 text-sm transition-all focus:outline-none focus:ring-2 focus:ring-[var(--color-accent)]/20"
                                {...register('degreeType', { valueAsNumber: true })}
                            >
                                <option value={1}>Undergraduate</option>
                                <option value={2}>Postgraduate</option>
                            </select>
                        </div>
                    </div>

                    <div className="space-y-2">
                        <Label htmlFor="description">Description (Internal Info)</Label>
                        <Textarea
                            id="description"
                            {...register('description')}
                            placeholder="Brief internal details..."
                            rows={2}
                        />
                    </div>

                    <div className="space-y-3">
                        <Label>Required Documents (e.g. Identity, Receipt)</Label>
                        <div className="flex gap-2">
                            <Input 
                                value={newDocName}
                                onChange={(e) => setNewDocName(e.target.value)}
                                onKeyDown={handleKeyPress}
                                placeholder="Type doc name..."
                                className="h-9"
                            />
                            <Button 
                                type="button" 
                                size="sm" 
                                onClick={addDocument}
                                disabled={!newDocName.trim()}
                            >
                                <Plus className="w-4 h-4" />
                            </Button>
                        </div>
                        
                        <div className="flex flex-wrap gap-2 p-3 bg-slate-50 dark:bg-white/5 rounded-lg border border-slate-200 dark:border-white/10 min-h-[50px] transition-all">
                            {requiredDocs.length === 0 ? (
                                <span className="text-xs text-slate-400 italic">No documents required yet.</span>
                            ) : (
                                requiredDocs.map((doc: string, idx: number) => (
                                    <span key={idx} className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-full bg-white dark:bg-slate-800 border border-slate-200 dark:border-white/10 text-xs font-semibold text-slate-700 dark:text-slate-200 shadow-sm transition-all animate-in zoom-in-95">
                                        {doc}
                                        <button type="button" onClick={() => removeDocument(idx)} className="text-slate-400 hover:text-red-500 transition-colors">
                                            <X className="w-3.5 h-3.5" />
                                        </button>
                                    </span>
                                ))
                            )}
                        </div>
                    </div>

                    <DialogFooter className="pt-4">
                        <Button type="button" variant="ghost" onClick={() => onOpenChange(false)}>Cancel</Button>
                        <Button type="submit" disabled={mutation.isPending} className="min-w-[120px] shadow-neon">
                            {mutation.isPending ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : null}
                            {isEdit ? 'Save Changes' : 'Create Type'}
                        </Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
};
