import { useParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { alumniService } from '../../services/alumniService';
import { MapPin, Mail, Calendar, Briefcase, GraduationCap, Globe } from 'lucide-react';
import { Card, CardContent } from '../../components/ui/Card';
import { Button } from '../../components/ui/Button';

const AlumniProfile = () => {
    const { id } = useParams<{ id: string }>();
    const { t } = useTranslation();

    const { data: profile } = useQuery({
        queryKey: ['alumni', id],
        queryFn: () => alumniService.getProfile(id!),
        enabled: !!id
    });


    if (!profile) return <div className="text-center py-20 text-[var(--color-text-muted)]">{t('profile.not_found')}</div>;

    return (
        <div className="space-y-6 max-w-5xl mx-auto animate-fade-in">
            {/* Header Card */}
            <Card variant="default" className="overflow-hidden border-t-4 border-t-[var(--color-accent)]">
                <div className="absolute top-0 left-0 w-full h-32 bg-gradient-to-r from-[var(--color-accent-light)]/50 to-slate-100 opacity-50"></div>

                <CardContent className="relative flex flex-col md:flex-row items-center md:items-end gap-6 pt-10 pb-8 px-8">
                    <div className="w-32 h-32 rounded-full border-4 border-white overflow-hidden shadow-xl bg-white z-10">
                        <img
                            src={alumniService.getPhotoUrl(profile.photoUrl) || `https://ui-avatars.com/api/?name=${profile.name}`}
                            alt={profile.name}
                            className="w-full h-full object-cover"
                        />
                    </div>

                    <div className="flex-1 text-center md:text-left mb-2 z-10">
                        <h1 className="text-3xl font-heading font-bold text-[var(--color-text-primary)] flex items-center justify-center md:justify-start gap-3">
                            {profile.name}
                            {profile.isVip && <span className="bg-[var(--color-accent)] text-white text-xs px-2 py-1 rounded-full font-bold shadow-sm">{t('profile.vip')}</span>}
                        </h1>
                        <p className="text-lg text-[var(--color-accent)] font-medium">{profile.jobTitle || t('profile.default_job_title')}</p>

                        <div className="flex flex-wrap justify-center md:justify-start gap-4 mt-4 text-sm text-[var(--color-text-secondary)]">
                            {profile.city && <span className="flex items-center gap-1"><MapPin className="w-4 h-4 text-slate-400" /> {profile.city}, {profile.country}</span>}
                            <span className="flex items-center gap-1"><Calendar className="w-4 h-4 text-slate-400" /> {t('profile.class_of', { year: profile.educations?.[0]?.graduationYear })}</span>
                        </div>
                    </div>

                    <div className="flex gap-3 z-10">
                        <Button className="flex items-center gap-2 shadow-lg shadow-blue-500/20">
                            <Mail className="w-4 h-4" /> {t('profile.message_btn')}
                        </Button>
                    </div>
                </CardContent>
            </Card>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                {/* Left Column: Info */}
                <div className="space-y-6">
                    <Card variant="default">
                        <CardContent className="p-6">
                            <h3 className="text-lg font-bold text-[var(--color-text-primary)] mb-4 border-b border-[var(--color-border)] pb-2 flex items-center gap-2">
                                <Briefcase className="w-5 h-5 text-[var(--color-accent)]" /> {t('profile.about')}
                            </h3>
                            <p className="text-[var(--color-text-secondary)] text-sm leading-relaxed">
                                {profile.bio || t('profile.no_bio')}
                            </p>
                        </CardContent>
                    </Card>

                    <Card variant="default">
                        <CardContent className="p-6">
                            <h3 className="text-lg font-bold text-[var(--color-text-primary)] mb-4 border-b border-[var(--color-border)] pb-2 flex items-center gap-2">
                                <Globe className="w-5 h-5 text-[var(--color-accent)]" /> {t('profile.contact')}
                            </h3>
                            <div className="space-y-3 text-sm">
                                <div className="flex items-center gap-3 text-[var(--color-text-secondary)]">
                                    <Mail className="w-4 h-4 text-slate-400" />
                                    <span>{profile.id ? t('profile.protected_email') : t('profile.email_hidden')}</span>
                                </div>
                                {/* Placeholder for real contact logic */}
                            </div>
                        </CardContent>
                    </Card>
                </div>

                {/* Right Column: Experience & Education */}
                <div className="lg:col-span-2 space-y-6">
                    {/* Education */}
                    <Card variant="default">
                        <CardContent className="p-6">
                            <h3 className="text-lg font-bold text-[var(--color-text-primary)] mb-6 flex items-center gap-2">
                                <GraduationCap className="w-5 h-5 text-[var(--color-accent)]" /> {t('profile.education')}
                            </h3>

                            <div className="space-y-6 relative before:absolute before:left-2 before:top-2 before:bottom-2 before:w-0.5 before:bg-[var(--color-border)] rtl:before:right-2 rtl:before:left-auto">
                                {profile.educations?.map((edu, idx) => (
                                    <div key={idx} className="relative pl-8 rtl:pl-0 rtl:pr-8">
                                        <div className="absolute left-0 top-1 w-4 h-4 rounded-full bg-white border-2 border-[var(--color-accent)] shadow-sm rtl:right-0 rtl:left-auto"></div>
                                        <h4 className="text-base font-semibold text-[var(--color-text-primary)]">{edu.institutionName}</h4>
                                        <p className="text-[var(--color-accent)] font-medium">{edu.degree} in {edu.major}</p>
                                        <p className="text-xs text-[var(--color-text-muted)] mt-1">
                                            {t('profile.class_of', { year: edu.graduationYear })} • {edu.graduationSemester === 1 ? t('profile.semester.fall') : t('profile.semester.spring')}
                                        </p>
                                    </div>
                                ))}
                            </div>
                        </CardContent>
                    </Card>

                    {/* Experience (Placeholder as backend DTO structure is strict) */}
                </div>
            </div>
        </div>
    );
};

export default AlumniProfile;
