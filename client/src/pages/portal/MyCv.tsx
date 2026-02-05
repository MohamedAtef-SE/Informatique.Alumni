import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { zodResolver } from '@hookform/resolvers/zod';
import { careerService } from '../../services/careerService';
import { cvSchema, type CvFormData } from '../../schemas/cvSchema';
import { Button } from '../../components/ui/Button';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/Card';
import { ExperienceSection } from '../../components/cv/ExperienceSection';

const MyCv = () => {
    const { t } = useTranslation();
    const queryClient = useQueryClient();

    const { data: cv, isLoading } = useQuery({
        queryKey: ['my-cv'],
        queryFn: careerService.getMyCv,
        retry: false
    });

    const { control, register, handleSubmit, reset, formState: { isDirty, errors } } = useForm<CvFormData>({
        resolver: zodResolver(cvSchema),
        defaultValues: {
            summary: '',
            isLookingForJob: false,
            experiences: [],
            educations: []
        }
    });

    useEffect(() => {
        if (cv) {
            reset({
                id: cv.id,
                summary: cv.summary || '',
                isLookingForJob: cv.isLookingForJob || false,
                experiences: cv.experiences || [],
                educations: cv.educations || [] // Future: Implement EducationSection
            });
        }
    }, [cv, reset]);

    const updateMutation = useMutation({
        mutationFn: careerService.updateCv,
        onSuccess: (data) => {
            queryClient.setQueryData(['my-cv'], data);
            toast.success(t('career.cv.saved', 'CV Saved Successfully'));
            reset(data); // Reset dirty state with new data
        },
        onError: () => toast.error(t('career.cv.error', 'Failed to save CV'))
    });

    const onSubmit = (data: CvFormData) => {
        // Ensure strictly typed payload matching what backend expects
        updateMutation.mutate(data);
    };

    if (isLoading) return <div className="p-10 text-center">{t('common.loading')}</div>;

    return (
        <div className="space-y-6 animate-slide-up max-w-4xl mx-auto pb-20">
            {/* Header & Main Info */}
            <Card className="border-t-4 border-t-[var(--color-accent)]">
                <CardHeader className="flex flex-row justify-between items-center">
                    <div>
                        <CardTitle className="text-2xl">{t('career.cv.title', 'My Curriculum Vitae')}</CardTitle>
                        <p className="text-[var(--color-text-secondary)] text-sm mt-1">
                            {t('career.cv.subtitle', 'Keep your professional profile up to date to apply for jobs.')}
                        </p>
                    </div>
                    <Button
                        onClick={handleSubmit(onSubmit)}
                        disabled={updateMutation.isPending || !isDirty}
                        className="shadow-lg"
                    >
                        {updateMutation.isPending ? t('common.saving') : t('common.save')}
                    </Button>
                </CardHeader>
                <CardContent className="space-y-6">
                    {/* Summary */}
                    <div className="space-y-2">
                        <label className="text-sm font-bold text-[var(--color-text-primary)]">
                            {t('career.cv.summary_label', 'Professional Summary')}
                        </label>
                        <textarea
                            className="w-full bg-white border border-[var(--color-border)] rounded-lg px-3 py-2 min-h-[120px] text-[var(--color-text-primary)] focus:ring-1 focus:ring-[var(--color-accent)] outline-none"
                            placeholder={t('career.cv.summary_placeholder', 'Briefly describe your professional background...')}
                            {...register('summary')}
                        />
                        {errors.summary && <p className="text-red-500 text-xs">{errors.summary.message}</p>}
                    </div>

                    {/* Open To Work */}
                    <div className="flex items-center gap-3 p-4 bg-blue-50/50 rounded-lg border border-blue-100">
                        <input
                            type="checkbox"
                            id="lookingForJob"
                            {...register('isLookingForJob')}
                            className="w-5 h-5 rounded border-gray-300 text-[var(--color-accent)] focus:ring-[var(--color-accent)]"
                        />
                        <div>
                            <label htmlFor="lookingForJob" className="font-semibold text-[var(--color-text-primary)] block cursor-pointer">
                                {t('career.cv.opentowork', 'Open to Opportunities')}
                            </label>
                            <p className="text-xs text-[var(--color-text-secondary)]">
                                {t('career.cv.opentowork_desc', 'Recruiters can see that you are looking for a job.')}
                            </p>
                        </div>
                    </div>
                </CardContent>
            </Card>

            {/* Experience Section */}
            <ExperienceSection control={control} />

            {/* Placeholder for Education */}
            {/* <EducationSection control={control} /> */}

            <div className="text-center text-xs text-gray-400 py-4">
                {t('career.cv.auto_save_note', 'Remember to save your changes.')}
            </div>
        </div>
    );
};

export default MyCv;
