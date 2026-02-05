import { useState } from 'react';
import { useFieldArray, useForm, type Control } from 'react-hook-form';
import { useTranslation } from 'react-i18next';
import { Plus, Trash, Pencil, Briefcase } from 'lucide-react';
import { Button } from '../ui/Button';
import { Card, CardHeader, CardTitle, CardContent } from '../ui/Card';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '../ui/Dialog';
import { Input } from '../ui/Input';
import type { ExperienceFormData } from '../../schemas/cvSchema';
import { cvExperienceSchema } from '../../schemas/cvSchema';
import { zodResolver } from '@hookform/resolvers/zod';

interface ExperienceSectionProps {
    control: Control<any>;
}

export const ExperienceSection = ({ control }: ExperienceSectionProps) => {
    const { t } = useTranslation();
    const { fields, append, remove, update } = useFieldArray({
        control,
        name: "experiences"
    });

    const [isDialogOpen, setIsDialogOpen] = useState(false);
    const [editingIndex, setEditingIndex] = useState<number | null>(null);

    // Local form for the Dialog
    const { register, handleSubmit, reset, formState: { errors } } = useForm<ExperienceFormData>({
        resolver: zodResolver(cvExperienceSchema)
    });

    const openAddDialog = () => {
        setEditingIndex(null);
        reset({ company: '', position: '', startDate: '', endDate: '', description: '' });
        setIsDialogOpen(true);
    };

    const openEditDialog = (index: number, data: ExperienceFormData) => {
        setEditingIndex(index);
        reset(data);
        setIsDialogOpen(true);
    };

    const onSave = (data: ExperienceFormData) => {
        if (editingIndex !== null) {
            update(editingIndex, data);
        } else {
            append(data);
        }
        setIsDialogOpen(false);
    };

    return (
        <Card>
            <CardHeader className="flex flex-row items-center justify-between">
                <CardTitle className="flex items-center gap-2">
                    <Briefcase className="w-5 h-5 text-[var(--color-accent)]" />
                    {t('career.cv.experience', 'Experience')}
                </CardTitle>
                <Button size="sm" variant="ghost" onClick={openAddDialog}>
                    <Plus className="w-4 h-4 mr-1" /> {t('common.add')}
                </Button>
            </CardHeader>
            <CardContent className="space-y-4">
                {fields.length === 0 && (
                    <p className="text-center text-gray-500 py-4 italic">{t('career.cv.no_experience')}</p>
                )}
                {fields.map((field: any, index) => (
                    <div key={field.id} className="p-4 border rounded-lg bg-slate-50 relative group">
                        <div className="absolute top-2 right-2 flex gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
                            <button onClick={() => openEditDialog(index, field as ExperienceFormData)} className="text-blue-500 hover:text-blue-700">
                                <Pencil className="w-4 h-4" />
                            </button>
                            <button onClick={() => remove(index)} className="text-red-500 hover:text-red-700">
                                <Trash className="w-4 h-4" />
                            </button>
                        </div>
                        <h4 className="font-bold text-lg">{field.company}</h4>
                        <p className="text-sm font-medium text-[var(--color-accent)]">{field.position}</p>
                        <p className="text-xs text-gray-500">
                            {field.startDate} - {field.endDate || t('common.present')}
                        </p>
                        {field.description && <p className="text-sm mt-2 text-gray-700">{field.description}</p>}
                    </div>
                ))}
            </CardContent>

            <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
                <DialogContent>
                    <DialogHeader>
                        <DialogTitle>{editingIndex !== null ? t('common.edit') : t('common.add')} {t('career.cv.experience')}</DialogTitle>
                    </DialogHeader>
                    <div className="space-y-4 py-4">
                        <Input label={t('career.cv.company')} error={errors.company?.message} {...register('company')} />
                        <Input label={t('career.cv.position')} error={errors.position?.message} {...register('position')} />
                        <div className="grid grid-cols-2 gap-4">
                            <Input type="date" label={t('career.cv.start_date')} error={errors.startDate?.message} {...register('startDate')} />
                            <Input type="date" label={t('career.cv.end_date')} error={errors.endDate?.message} {...register('endDate')} />
                        </div>
                        <div className="space-y-1">
                            <label className="text-sm font-medium">{t('career.cv.description')}</label>
                            <textarea
                                className="w-full border rounded-md p-2 text-sm"
                                rows={3}
                                {...register('description')}
                            />
                        </div>
                    </div>
                    <DialogFooter>
                        <Button variant="ghost" onClick={() => setIsDialogOpen(false)}>{t('common.cancel')}</Button>
                        <Button onClick={handleSubmit(onSave)}>{t('common.save')}</Button>
                    </DialogFooter>
                </DialogContent>
            </Dialog>
        </Card>
    );
};
