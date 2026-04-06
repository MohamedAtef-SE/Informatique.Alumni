import { MapPin, Edit2, GraduationCap, ShieldCheck, Building2 } from 'lucide-react';
import { alumniService } from '../../services/alumniService';
import { useTranslation } from 'react-i18next';
import { Badge } from '../ui/Badge';
import { Button } from '../ui/Button';
import { Card } from '../ui/Card';

interface ProfileHeaderProps {
    profile: any;
    onEdit: () => void;
    isOwnProfile?: boolean;
}

const ProfileHeader = ({ profile, onEdit, isOwnProfile = false }: ProfileHeaderProps) => {
    const { t } = useTranslation();

    const displayName = profile?.name || profile?.nameEn || profile?.nameAr || profile?.userName || 'Alumni Member';

    const latestEducation = profile?.academicHistory?.[0] || profile?.educations?.[0];
    const currentCompany = profile?.experiences?.[0]?.companyName || profile?.company;

    return (
        <Card className="overflow-hidden border-slate-200 border shadow-sm bg-white dark:bg-slate-900 mb-6 rounded-xl">
            {/* Professional Banner - Contained within Card */}
            <div className="h-48 md:h-64 w-full overflow-hidden relative group/banner">
                {/* Dynamic Cover Background with Gradient Overlay */}
                <div className="absolute inset-0 bg-gradient-to-br from-blue-900 via-indigo-950 to-slate-950 z-10 opacity-70" />
                
                <img
                    src="https://images.unsplash.com/photo-1497366216548-37526070297c?q=80&w=2069&auto=format&fit=crop"
                    alt="Cover"
                    className="w-full h-full object-cover"
                />

                <div className="absolute top-0 right-0 w-96 h-96 bg-blue-500/10 rounded-full blur-[120px] -mr-48 -mt-48 animate-pulse" />
                
                {isOwnProfile && (
                    <Button variant="glass" size="icon" className="absolute top-4 right-4 z-20 border-white/20 text-white hover:bg-white/10 rounded-full opacity-0 group-hover/banner:opacity-100 transition-opacity backdrop-blur-md">
                        <Edit2 className="w-4 h-4" />
                    </Button>
                )}
            </div>

            {/* Profile Content Area */}
            <div className="px-6 md:px-10 pb-8 pt-0 relative">
                <div className="flex flex-col md:flex-row items-center md:items-start gap-6 relative z-20">
                    
                    {/* Avatar Overlapping Banner */}
                    <div className="relative group/avatar -mt-16 md:-mt-24 self-center md:self-start">
                        <div className="absolute -inset-1 bg-gradient-to-tr from-blue-600 to-indigo-400 rounded-full blur opacity-20 group-hover/avatar:opacity-40 transition-opacity" />
                        <div className="w-32 h-32 md:w-44 md:h-44 rounded-full border-[5px] border-white dark:border-slate-900 overflow-hidden shadow-xl relative bg-white transition-transform group-hover/avatar:scale-[1.02] duration-500">
                            <img
                                src={profile?.photoUrl ? alumniService.getPhotoUrl(profile.photoUrl) : `https://ui-avatars.com/api/?name=${encodeURIComponent(displayName)}&size=256&background=f1f5f9&color=2D96D7`}
                                alt={displayName}
                                className="w-full h-full object-cover"
                            />
                            {isOwnProfile && (
                                <div className="absolute inset-0 bg-black/40 flex items-center justify-center opacity-0 group-hover/avatar:opacity-100 transition-opacity cursor-pointer backdrop-blur-[2px]" onClick={onEdit}>
                                    <Edit2 className="w-6 h-6 text-white" />
                                </div>
                            )}
                        </div>
                    </div>

                    {/* Information Cluster */}
                    <div className="flex-1 flex flex-col md:flex-row justify-between w-full pt-4">
                        
                        {/* Identity Pillar (Left) */}
                        <div className="space-y-1 text-center md:text-left flex-1">
                            <div className="flex flex-col md:flex-row md:items-center gap-2">
                                <h1 className="text-3xl font-heading font-black text-slate-900 dark:text-white tracking-tight">
                                    {displayName}
                                </h1>
                                <div className="flex justify-center md:justify-start gap-2">
                                    {profile?.status === 1 && (
                                        <Badge variant="outline" className="bg-emerald-50 text-emerald-600 border-emerald-100 font-bold uppercase tracking-widest text-[9px] py-0.5">
                                            <ShieldCheck className="w-3 h-3 mr-1" /> Verified Alumni
                                        </Badge>
                                    )}
                                    {profile?.isVip && (
                                        <Badge className="bg-amber-400 text-slate-950 border-amber-300 font-black uppercase tracking-widest text-[9px] py-0.5 shadow-sm">
                                            PREMIUM
                                        </Badge>
                                    )}
                                </div>
                            </div>

                            <p className="text-lg md:text-xl text-slate-600 dark:text-slate-300 font-medium tracking-tight">
                                {profile?.jobTitle || t('profile.default_job_title')}
                            </p>

                            <div className="flex flex-wrap justify-center md:justify-start gap-x-6 gap-y-2 text-slate-500 font-semibold text-sm pt-1">
                                {(profile?.city || profile?.country) && (
                                    <span className="flex items-center gap-1.5 focus-within:text-blue-600 transition-colors">
                                        <MapPin className="w-4 h-4 text-slate-400" /> {profile.city}, {profile.country}
                                    </span>
                                )}
                            </div>
                        </div>

                        {/* Top Education/Company (Right Pillar) */}
                        <div className="hidden md:flex flex-col gap-3 pt-1 max-w-[280px]">
                            {currentCompany && (
                                <div className="flex items-center gap-2 group cursor-pointer">
                                    <div className="w-8 h-8 rounded bg-slate-50 border border-slate-100 flex items-center justify-center text-slate-400 group-hover:bg-blue-50 transition-colors">
                                        <Building2 className="w-4 h-4" />
                                    </div>
                                    <span className="text-xs font-bold text-slate-800 dark:text-slate-200 line-clamp-2 leading-tight group-hover:text-blue-600 transition-colors">
                                        {currentCompany}
                                    </span>
                                </div>
                            )}
                            {latestEducation && (
                                <div className="flex items-center gap-2 group cursor-pointer">
                                    <div className="w-8 h-8 rounded bg-slate-50 border border-slate-100 flex items-center justify-center text-slate-400 group-hover:bg-blue-50 transition-colors">
                                        <GraduationCap className="w-4 h-4" />
                                    </div>
                                    <div className="flex flex-col">
                                        <span className="text-xs font-bold text-slate-800 dark:text-slate-200 line-clamp-1 leading-tight group-hover:text-blue-600 transition-colors">
                                            {latestEducation.college}
                                        </span>
                                        <span className="text-[10px] text-slate-500 font-medium truncate max-w-[200px]">
                                            {latestEducation.major} • Class {latestEducation.graduationYear}
                                        </span>
                                    </div>
                                </div>
                            )}
                        </div>
                    </div>
                </div>

                {/* Main Action Buttons */}
                <div className="flex flex-wrap gap-2 mt-8 justify-center md:justify-start">
                    {isOwnProfile ? (
                        <Button onClick={onEdit} className="h-10 px-8 rounded-full font-bold uppercase tracking-wider text-[10px] bg-blue-600 hover:bg-blue-700 text-white shadow-md shadow-blue-500/10">
                            <Edit2 className="w-3.5 h-3.5 mr-2" /> Edit Profile
                        </Button>
                    ) : (
                        <Button className="h-10 px-10 rounded-full font-bold uppercase tracking-wider text-[10px] bg-blue-600 hover:bg-blue-700 text-white shadow-md shadow-blue-500/20">
                            Message
                        </Button>
                    )}
                </div>
            </div>
        </Card>
    );
};

export default ProfileHeader;
