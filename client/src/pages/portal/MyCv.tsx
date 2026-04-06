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

import { EducationSection, SkillsSection, LanguagesSection, SocialLinksSection, ProjectsSection, CertificationsSection } from '../../components/cv/CvSections';

const MyCv = () => {
    const { t } = useTranslation();
    const queryClient = useQueryClient();

    const { data: cv, isLoading } = useQuery({
        queryKey: ['my-cv'],
        queryFn: careerService.getMyCv,
        retry: false
    });

    const { control, register, handleSubmit, reset, formState: { isDirty, errors } } = useForm<CvFormData>({
        resolver: zodResolver(cvSchema) as any,
        defaultValues: {
            summary: '',
            isLookingForJob: false,
            experiences: [],
            educations: [],
            skills: [],
            languages: [],
            certifications: [],
            projects: [],
            socialLinks: []
        }
    });

    useEffect(() => {
        if (cv) {
            reset({
                id: cv.id,
                summary: cv.summary || '',
                isLookingForJob: cv.isLookingForJob || false,
                experiences: cv.experiences || [],
                educations: cv.educations || [],
                skills: cv.skills || [],
                languages: cv.languages || [],
                certifications: cv.certifications || [],
                projects: cv.projects || [],
                socialLinks: cv.socialLinks || []
            });
        }
    }, [cv, reset]);

    const updateMutation = useMutation({
        mutationFn: (data: CvFormData) => careerService.updateCv(data),
        onSuccess: (data) => {
            queryClient.setQueryData(['my-cv'], data);
            toast.success(t('career.cv.saved', 'CV Saved Successfully'));
            reset(data); 
        },
        onError: () => toast.error(t('career.cv.error', 'Failed to save CV'))
    });

    const onSubmit = (data: CvFormData) => {
        console.log("Submitting CV Data:", data);
        updateMutation.mutate(data);
    };

    // Validation Summary Utility
    const validationErrors = Object.entries(errors).flatMap(([key, value]) => {
        if (Array.isArray(value)) {
            return value.map((err, index) => ({
                section: key,
                index,
                message: Object.values(err as any).map((e: any) => e.message).join(', ')
            }));
        }
        return [{ section: key, message: (value as any).message }];
    });

    if (isLoading) return <div className="p-10 text-center">{t('common.loading')}</div>;

    return (
        <div className="space-y-6 animate-slide-up max-w-4xl mx-auto pb-20 px-4 sm:px-6">
            {/* Validation Error Banner */}
            {validationErrors.length > 0 && (
                <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800/30 rounded-2xl p-4 flex gap-4 animate-in fade-in slide-in-from-top-4 duration-300">
                    <div className="bg-red-100 dark:bg-red-900/40 p-2 rounded-xl h-fit">
                        <svg className="w-5 h-5 text-red-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                        </svg>
                    </div>
                    <div className="flex-1">
                        <h3 className="font-bold text-red-800 dark:text-red-400 text-sm">
                            {t('common.validation_error', 'Some fields require attention')}
                        </h3>
                        <ul className="mt-2 space-y-1">
                            {validationErrors.map((error, i) => (
                                <li key={i} className="text-xs text-red-700 dark:text-red-300 flex items-center gap-2">
                                    <span className="w-1.5 h-1.5 rounded-full bg-red-400" />
                                    <span className="font-semibold capitalize">{error.section.replace(/([A-Z])/g, ' $1')}:</span> {error.message}
                                </li>
                            ))}
                        </ul>
                    </div>
                </div>
            )}
            {/* Header & Main Info */}
            <Card className="border-t-4 border-t-[var(--color-accent)] shadow-blue-50/50">
                <CardHeader className="flex flex-row justify-between items-center">
                    <div>
                        <CardTitle className="text-2xl font-bold tracking-tight">{t('career.cv.title', 'My Curriculum Vitae')}</CardTitle>
                        <p className="text-[var(--color-text-secondary)] text-sm mt-1">
                            {t('career.cv.subtitle', 'Keep your professional profile up to date to apply for jobs.')}
                        </p>
                    </div>
                    <Button
                        onClick={handleSubmit(onSubmit as any)}
                        disabled={updateMutation.isPending || !isDirty}
                        className="shadow-md font-bold px-8 transition-all hover:scale-105 active:scale-95"
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
                            className="w-full bg-white dark:bg-slate-900 border border-[var(--color-border)] rounded-xl px-4 py-3 min-h-[140px] text-[var(--color-text-primary)] focus:ring-2 focus:ring-[var(--color-accent)]/20 focus:border-[var(--color-accent)] outline-none transition-all shadow-inner"
                            placeholder={t('career.cv.summary_placeholder', 'Briefly describe your professional background...')}
                            {...register('summary')}
                        />
                        {errors.summary && <p className="text-red-500 text-xs font-medium ml-1">{errors.summary.message}</p>}
                    </div>

                    {/* Open To Work */}
                    <div className="flex items-center gap-4 p-5 bg-blue-50/30 dark:bg-blue-900/10 rounded-2xl border border-blue-100/50 dark:border-blue-800/30">
                        <input
                            type="checkbox"
                            id="lookingForJob"
                            {...register('isLookingForJob')}
                            className="w-6 h-6 rounded-md border-slate-300 text-[var(--color-accent)] focus:ring-[var(--color-accent)] transition-all cursor-pointer"
                        />
                        <div>
                            <label htmlFor="lookingForJob" className="font-bold text-[var(--color-text-primary)] block cursor-pointer transition-colors hover:text-accent">
                                {t('career.cv.opentowork', 'Open to Opportunities')}
                            </label>
                            <p className="text-xs text-[var(--color-text-secondary)] mt-0.5">
                                {t('career.cv.opentowork_desc', 'Recruiters can see that you are looking for a job.')}
                            </p>
                        </div>
                    </div>
                </CardContent>
            </Card>

            {/* Grid Layout for CV Sections */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="space-y-6">
                    <ExperienceSection control={control} />
                    <EducationSection control={control} />
                    <ProjectsSection control={control} />
                </div>
                <div className="space-y-6">
                    <SkillsSection control={control} />
                    <CertificationsSection control={control} />
                    <LanguagesSection control={control} />
                    <SocialLinksSection control={control} />
                </div>
            </div>

            <div className="text-center text-[10px] text-gray-400 font-mono py-8 uppercase tracking-widest opacity-50">
                {t('career.cv.auto_save_note', 'Remember to save your changes.')}
            </div>
        </div>
    );
};

export default MyCv;
