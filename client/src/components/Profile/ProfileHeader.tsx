import { Button } from '../ui/Button';
import { MapPin, Briefcase, Edit2 } from 'lucide-react';
import { alumniService } from '../../services/alumniService';
import { useTranslation } from 'react-i18next';

interface ProfileHeaderProps {
    profile: any; // Type strictly in real impl
    onEdit: () => void;
    isOwnProfile?: boolean;
}

const ProfileHeader = ({ profile, onEdit, isOwnProfile = false }: ProfileHeaderProps) => {
    const { t, i18n } = useTranslation();

    const displayName = i18n.language === 'ar'
        ? (profile?.nameAr || profile?.nameEn || profile?.name)
        : (profile?.nameEn || profile?.nameAr || profile?.name);

    return (
        <div className="relative mb-32 md:mb-40">
            {/* Cover Image */}
            <div className="h-48 md:h-64 bg-gradient-to-r from-blue-900 to-slate-800 rounded-b-3xl shadow-md overflow-hidden relative">
                <div className="absolute inset-0 bg-pattern opacity-10"></div> {/* Optional pattern */}
            </div>

            {/* Profile Info Card Container */}
            <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 absolute top-32 md:top-40 left-0 right-0">
                <div className="bg-white rounded-2xl shadow-lg p-6 md:p-8 flex flex-col md:flex-row items-start md:items-end gap-6 animate-fade-in-up">

                    {/* Avatar */}
                    <div className="relative -mt-16 md:-mt-20 shrink-0">
                        <div className="w-32 h-32 md:w-40 md:h-40 rounded-full border-4 border-white shadow-xl overflow-hidden bg-white">
                            <img
                                src={profile?.photoUrl ? alumniService.getPhotoUrl(profile.photoUrl) : `https://ui-avatars.com/api/?name=${displayName}`}
                                alt={displayName}
                                className="w-full h-full object-cover"
                            />
                        </div>
                    </div>

                    {/* Identity */}
                    <div className="flex-1 text-center md:text-left pt-2 md:pt-0">
                        <h1 className="text-3xl font-heading font-bold text-slate-900">
                            {displayName}
                        </h1>
                        <p className="text-lg text-blue-600 font-medium mt-1">
                            {profile?.jobTitle || t('profile.default_job_title')}
                        </p>
                        {profile?.company && (
                            <p className="text-slate-600 flex items-center justify-center md:justify-start gap-2 mt-1 text-sm">
                                <Briefcase className="w-4 h-4" /> {profile.company}
                            </p>
                        )}
                        <div className="flex flex-wrap justify-center md:justify-start gap-4 mt-3 text-sm text-slate-500">
                            {(profile?.city || profile?.country) && (
                                <span className="flex items-center gap-1">
                                    <MapPin className="w-4 h-4" />
                                    {[profile.city, profile.country].filter(Boolean).join(', ')}
                                </span>
                            )}
                        </div>
                    </div>

                    {/* Actions */}
                    {isOwnProfile && (
                        <div className="w-full md:w-auto mt-4 md:mt-0">
                            <Button onClick={onEdit} className="w-full md:w-auto flex items-center gap-2 shadow-sm">
                                <Edit2 className="w-4 h-4" /> {t('my_profile.actions.edit')}
                            </Button>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};

export default ProfileHeader;
