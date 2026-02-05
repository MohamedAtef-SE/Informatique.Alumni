import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { alumniService } from '../../services/alumniService';
import { useAuth } from 'react-oidc-context';
import { Mail, Save, Camera, Linkedin, Facebook } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/Card';
import { useDropzone } from 'react-dropzone';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '../../components/ui/Dialog';
import ImageCropper from '../../components/common/ImageCropper';
import { Button } from '../../components/ui/Button';
import { Input } from '../../components/ui/Input';
import { cn } from '../../utils/cn';
import { toast } from 'sonner';

const Profile = () => {
    const auth = useAuth();
    const queryClient = useQueryClient();
    const [isEditing, setIsEditing] = useState(false);
    const { t } = useTranslation();

    // Form State
    const [formData, setFormData] = useState({
        bio: '',
        jobTitle: '',
        company: '',
        city: '',
        country: '',
        facebookUrl: '',
        linkedinUrl: ''
    });

    const { data: profile } = useQuery({
        queryKey: ['my-profile'],
        queryFn: () => alumniService.getMyProfile(),
    });

    useEffect(() => {
        if (profile) {
            setFormData({
                bio: profile?.bio || '',
                jobTitle: profile?.jobTitle || '',
                company: profile?.company || '',
                city: profile?.city || '',
                country: profile?.country || '',
                facebookUrl: profile?.facebookUrl || '',
                linkedinUrl: profile?.linkedinUrl || ''
            });
        }
    }, [profile]);

    const updateMutation = useMutation({
        mutationFn: (data: any) => alumniService.updateProfile(data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['my-profile'] });
            setIsEditing(false);
            toast.success(t('my_profile.toast.update_success'));
        },
        onError: () => {
            toast.error(t('my_profile.toast.update_error'));
        }
    });

    const handleSave = () => {
        updateMutation.mutate(formData);
    };

    const [selectedFile, setSelectedFile] = useState<File | null>(null);

    const uploadPhotoMutation = useMutation({
        mutationFn: (file: Blob) => alumniService.uploadProfilePhoto(file as File),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['my-profile'] });
            toast.success(t('my_profile.toast.photo_success'));
            setSelectedFile(null);
        },
        onError: () => {
            toast.error(t('my_profile.toast.photo_error'));
        }
    });

    const onDrop = useCallback((acceptedFiles: File[]) => {
        if (acceptedFiles && acceptedFiles.length > 0) {
            setSelectedFile(acceptedFiles[0]);
        }
    }, []);

    const { getRootProps, getInputProps } = useDropzone({
        onDrop,
        accept: {
            'image/jpeg': [],
            'image/png': [],
            'image/webp': []
        },
        maxFiles: 1,
        multiple: false
    });

    const handleCropComplete = async (croppedBlob: Blob) => {
        const file = new File([croppedBlob], "profile_photo.jpg", { type: "image/jpeg" });
        uploadPhotoMutation.mutate(file);
    };

    return (
        <div className="max-w-5xl mx-auto space-y-8 animate-slide-up">
            {/* Context Header */}
            <div className="flex justify-between items-end border-b border-[var(--color-border)] pb-6">
                <div>
                    <h1 className="text-4xl font-heading font-bold text-[var(--color-text-primary)]">
                        {t('my_profile.title')}
                    </h1>
                    <p className="text-[var(--color-text-secondary)] mt-2">{t('my_profile.subtitle')}</p>
                </div>
                {!isEditing ? (
                    <Button onClick={() => setIsEditing(true)}>
                        {t('my_profile.actions.edit')}
                    </Button>
                ) : (
                    <div className="flex gap-3">
                        <Button variant="ghost" onClick={() => setIsEditing(false)}>
                            {t('my_profile.actions.cancel')}
                        </Button>
                        <Button onClick={handleSave} disabled={updateMutation.isPending}>
                            {updateMutation.isPending ? t('my_profile.actions.saving') : <><Save className="w-4 h-4 mr-2" /> {t('my_profile.actions.save')}</>}
                        </Button>
                    </div>
                )}
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-12 gap-8">
                {/* Left Column: Avatar & Quick Info */}
                <div className="lg:col-span-4 space-y-6">
                    <Card variant="default" className="overflow-hidden border-t-4 border-t-[var(--color-accent)]">
                        <CardContent className="pt-8 flex flex-col items-center text-center">
                            <div className="relative group">
                                <div className="absolute -inset-1 bg-gradient-to-r from-[var(--color-accent)] to-emerald-400 rounded-full blur opacity-20 group-hover:opacity-50 transition duration-1000 group-hover:duration-200"></div>

                                <div
                                    {...getRootProps()}
                                    className="w-32 h-32 rounded-full border-4 border-white overflow-hidden relative group cursor-pointer shadow-xl z-10"
                                >
                                    <input {...getInputProps()} />
                                    <img
                                        src={profile?.photoUrl ? alumniService.getPhotoUrl(profile.photoUrl) : `https://ui-avatars.com/api/?name=${auth.user?.profile.name}`}
                                        alt="Profile"
                                        className={cn("w-full h-full object-cover shadow-inner", uploadPhotoMutation.isPending && "opacity-50 blur-sm")}
                                    />
                                    <div className="absolute inset-0 bg-black/40 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity">
                                        {uploadPhotoMutation.isPending ? (
                                            <div className="w-6 h-6 border-2 border-white border-t-transparent rounded-full animate-spin" />
                                        ) : (
                                            <Camera className="w-8 h-8 text-white" />
                                        )}
                                    </div>
                                </div>
                            </div>

                            <Dialog open={!!selectedFile} onOpenChange={() => setSelectedFile(null)}>
                                <DialogContent className="sm:max-w-xl p-0 overflow-hidden bg-white border-[var(--color-border)]">
                                    <DialogHeader className="p-4 border-b border-[var(--color-border)] bg-slate-50">
                                        <DialogTitle>{t('my_profile.crop_photo')}</DialogTitle>
                                    </DialogHeader>
                                    {selectedFile && (
                                        <div className="p-4">
                                            <ImageCropper
                                                imageSrc={URL.createObjectURL(selectedFile)}
                                                onCropComplete={handleCropComplete}
                                                onCancel={() => setSelectedFile(null)}
                                            />
                                        </div>
                                    )}
                                </DialogContent>
                            </Dialog>

                            <h2 className="text-2xl font-bold text-[var(--color-text-primary)] mt-6 font-heading">{auth.user?.profile.name}</h2>
                            <p className="text-[var(--color-accent)] font-semibold mt-1">
                                {profile?.jobTitle || t('profile.default_job_title')}
                            </p>
                            {profile?.company && (
                                <p className="text-[var(--color-text-secondary)] text-sm mt-1">{profile.company}</p>
                            )}

                            <div className="w-full h-px bg-[var(--color-border)] my-8" />

                            <div className="w-full space-y-4 text-left">
                                <div className="flex items-center gap-3 text-[var(--color-text-primary)]">
                                    <div className="w-8 h-8 rounded-xl bg-[var(--color-accent-light)] flex items-center justify-center text-[var(--color-accent)]">
                                        <Mail className="w-4 h-4" />
                                    </div>
                                    <span className="text-sm font-medium truncate">{auth.user?.profile.email}</span>
                                </div>
                            </div>
                        </CardContent>
                    </Card>
                </div>

                {/* Right Column: Detailed Form */}
                <div className="lg:col-span-8 space-y-6">
                    <Card variant="default">
                        <CardHeader>
                            <CardTitle>{t('my_profile.sections.professional')}</CardTitle>
                        </CardHeader>
                        <CardContent className="space-y-6">
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                                <Input
                                    label={t('my_profile.form.job_title')}
                                    value={isEditing ? formData.jobTitle : (profile?.jobTitle || '')}
                                    readOnly={!isEditing}
                                    onChange={e => setFormData({ ...formData, jobTitle: e.target.value })}
                                    className={cn(!isEditing && "border-transparent bg-transparent pl-0 text-[var(--color-text-primary)] font-semibold text-lg")}
                                />
                                <Input
                                    label={t('my_profile.form.company')}
                                    value={isEditing ? formData.company : (profile?.company || '')}
                                    readOnly={!isEditing}
                                    onChange={e => setFormData({ ...formData, company: e.target.value })}
                                    className={cn(!isEditing && "border-transparent bg-transparent pl-0 text-[var(--color-text-primary)] font-semibold text-lg")}
                                />
                            </div>

                            <div className="space-y-2">
                                <label className="text-sm font-bold text-[var(--color-text-secondary)] uppercase tracking-wider">{t('my_profile.form.bio')}</label>
                                {isEditing ? (
                                    <textarea
                                        className="w-full bg-slate-50 border border-[var(--color-border)] rounded-xl px-4 py-3 text-[var(--color-text-primary)] focus:border-[var(--color-accent)] focus:ring-2 focus:ring-[var(--color-accent)]/20 shadow-inner outline-none min-h-[150px] resize-none transition-all"
                                        value={formData.bio}
                                        onChange={e => setFormData({ ...formData, bio: e.target.value })}
                                        placeholder={t('my_profile.form.bio_placeholder')}
                                    />
                                ) : (
                                    <p className="text-[var(--color-text-secondary)] text-base leading-relaxed p-4 bg-slate-50/50 rounded-xl border border-dashed border-[var(--color-border)]">
                                        {profile?.bio || t('my_profile.form.no_bio')}
                                    </p>
                                )}
                            </div>
                        </CardContent>
                    </Card>

                    <Card variant="default">
                        <CardHeader>
                            <CardTitle>{t('my_profile.sections.location_socials')}</CardTitle>
                        </CardHeader>
                        <CardContent className="space-y-6">
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                                <Input
                                    label={t('my_profile.form.city')}
                                    value={isEditing ? formData.city : (profile?.city || '')}
                                    readOnly={!isEditing}
                                    onChange={e => setFormData({ ...formData, city: e.target.value })}
                                    className={cn(!isEditing && "border-transparent bg-transparent pl-0 text-[var(--color-text-primary)] font-medium")}
                                />
                                <Input
                                    label={t('my_profile.form.country')}
                                    value={isEditing ? formData.country : (profile?.country || '')}
                                    readOnly={!isEditing}
                                    onChange={e => setFormData({ ...formData, country: e.target.value })}
                                    className={cn(!isEditing && "border-transparent bg-transparent pl-0 text-[var(--color-text-primary)] font-medium")}
                                />
                            </div>

                            <div className="pt-6 border-t border-[var(--color-border)]">
                                {isEditing ? (
                                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                                        <div className="space-y-2">
                                            <label className="text-sm font-bold text-[var(--color-text-secondary)] flex items-center gap-2">
                                                <Linkedin className="w-4 h-4 text-blue-600" /> {t('my_profile.form.linkedin')}
                                            </label>
                                            <Input
                                                placeholder="https://linkedin.com/in/..."
                                                value={formData.linkedinUrl}
                                                onChange={e => setFormData({ ...formData, linkedinUrl: e.target.value })}
                                            />
                                        </div>
                                        <div className="space-y-2">
                                            <label className="text-sm font-bold text-[var(--color-text-secondary)] flex items-center gap-2">
                                                <Facebook className="w-4 h-4 text-blue-800" /> {t('my_profile.form.facebook')}
                                            </label>
                                            <Input
                                                placeholder="https://facebook.com/..."
                                                value={formData.facebookUrl}
                                                onChange={e => setFormData({ ...formData, facebookUrl: e.target.value })}
                                            />
                                        </div>
                                    </div>
                                ) : (
                                    <div className="flex items-center gap-4">
                                        {profile?.linkedinUrl ? (
                                            <a
                                                href={profile.linkedinUrl}
                                                target="_blank"
                                                rel="noopener noreferrer"
                                                className="w-12 h-12 rounded-xl bg-blue-50 hover:bg-blue-100 flex items-center justify-center text-blue-600 shadow-sm transition-all duration-200 hover:scale-110 border border-blue-100"
                                                title="LinkedIn Profile"
                                            >
                                                <Linkedin className="w-6 h-6" />
                                            </a>
                                        ) : (
                                            <div className="w-12 h-12 rounded-xl bg-slate-50 flex items-center justify-center text-slate-300 border border-[var(--color-border)] border-dashed cursor-not-allowed" title="LinkedIn not connected">
                                                <Linkedin className="w-6 h-6" />
                                            </div>
                                        )}
                                        {profile?.facebookUrl ? (
                                            <a
                                                href={profile.facebookUrl}
                                                target="_blank"
                                                rel="noopener noreferrer"
                                                className="w-12 h-12 rounded-xl bg-blue-100/30 hover:bg-blue-100/50 flex items-center justify-center text-blue-800 shadow-sm transition-all duration-200 hover:scale-110 border border-blue-100"
                                                title="Facebook Profile"
                                            >
                                                <Facebook className="w-6 h-6" />
                                            </a>
                                        ) : (
                                            <div className="w-12 h-12 rounded-xl bg-slate-50 flex items-center justify-center text-slate-300 border border-[var(--color-border)] border-dashed cursor-not-allowed" title="Facebook not connected">
                                                <Facebook className="w-6 h-6" />
                                            </div>
                                        )}
                                    </div>
                                )}
                            </div>
                        </CardContent>
                    </Card>
                </div>
            </div>
        </div>
    );
};

export default Profile;
