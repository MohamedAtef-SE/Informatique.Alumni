import { useParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { alumniService } from '../../services/alumniService';
import { 
    Briefcase, GraduationCap, 
    ShieldCheck, Mail, Globe, 
    Building2, Calendar
} from 'lucide-react';
import { Card, CardContent, Button } from '../../components/ui';
import LoadingLayer from '../../components/common/LoadingLayer';
import ProfileHeader from '../../components/Profile/ProfileHeader';

const AlumniProfile = () => {
    const { id } = useParams<{ id: string }>();
    const { t } = useTranslation();

    const { data: profile, isLoading, isError } = useQuery({
        queryKey: ['alumni', id],
        queryFn: () => alumniService.getProfile(id!),
        enabled: !!id,
        retry: false
    });

    if (isLoading) return <LoadingLayer />;
    if (isError || !profile) return (
        <div className="flex flex-col items-center justify-center py-32 space-y-4">
            <div className="p-4 bg-slate-100 rounded-full text-slate-400">
                <ShieldCheck className="w-12 h-12" />
            </div>
            <h2 className="text-2xl font-bold text-slate-900">{t('profile.not_found')}</h2>
            <p className="text-slate-500 text-center max-w-sm">
                The alumni profile you are looking for does not exist or the ID is incorrect.
            </p>
            <Button variant="outline" onClick={() => window.history.back()}>Back to Directory</Button>
        </div>
    );

    return (
        <div className="min-h-screen bg-[#f3f2ef] pb-20 font-sans">
            <div className="max-w-6xl mx-auto px-4 md:px-12 pt-8">
                {/* Standard Profile Header (Shared component) */}
                <ProfileHeader
                    profile={profile}
                    onEdit={() => {}}
                    isOwnProfile={false}
                />

                <div className="grid grid-cols-1 lg:grid-cols-12 gap-8">
                    
                    {/* Main Content Column */}
                    <div className="lg:col-span-8 space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-1000">
                        
                        {/* About Section */}
                        <Card className="border-slate-200 border shadow-sm">
                            <CardContent className="p-8">
                                <h3 className="text-2xl font-heading font-black text-slate-900 tracking-tight mb-6">
                                    About
                                </h3>
                                <p className="text-slate-600 leading-relaxed whitespace-pre-wrap text-[15px]">
                                    {profile.bio || "This alumnus focused on their journey and hasn't added a bio yet."}
                                </p>
                            </CardContent>
                        </Card>

                        {/* Professional Experience Section */}
                        <Card className="border-slate-200 border shadow-sm overflow-hidden">
                            <CardContent className="p-8">
                                <h3 className="text-2xl font-heading font-black text-slate-900 tracking-tight mb-8">
                                    Experience
                                </h3>

                                {profile.experiences && profile.experiences.length > 0 ? (
                                    <div className="space-y-10">
                                        {profile.experiences.map((exp: any, idx: number) => (
                                            <div key={idx} className="flex gap-4 group">
                                                <div className="p-3 bg-slate-50 rounded-xl border border-slate-100 text-slate-400 group-hover:bg-blue-50 group-hover:text-blue-600 transition-all self-start">
                                                    <Briefcase className="w-6 h-6" />
                                                </div>
                                                <div className="flex-1 space-y-1 pb-8 border-b last:border-0 border-slate-100">
                                                    <h4 className="text-lg font-bold text-slate-900 leading-snug">{exp.jobTitle}</h4>
                                                    <p className="text-blue-600 font-bold text-sm tracking-wide uppercase">{exp.companyName}</p>
                                                    <div className="flex items-center gap-2 text-slate-400 text-xs font-bold uppercase tracking-widest mt-1">
                                                        <Calendar className="w-3 h-3" />
                                                        {new Date(exp.startDate).toLocaleDateString('en', { month: 'short', year: 'numeric' })}
                                                        {' - '}
                                                        {exp.endDate ? new Date(exp.endDate).toLocaleDateString('en', { month: 'short', year: 'numeric' }) : 'Present'}
                                                    </div>
                                                    {exp.description && (
                                                        <p className="mt-4 text-slate-500 text-sm leading-relaxed max-w-2xl">{exp.description}</p>
                                                    )}
                                                </div>
                                            </div>
                                        ))}
                                    </div>
                                ) : (
                                    <div className="text-center py-12 rounded-3xl border-2 border-dashed border-slate-100 bg-slate-50/50">
                                        <p className="text-slate-400 font-medium">No professional experience listed.</p>
                                    </div>
                                )}
                            </CardContent>
                        </Card>

                        {/* Academic Foundation */}
                        <Card className="border-slate-200 border shadow-sm">
                            <CardContent className="p-8">
                                <h3 className="text-2xl font-heading font-black text-slate-900 tracking-tight mb-8">
                                    Education
                                </h3>

                                {profile.educations && profile.educations.length > 0 ? (
                                    <div className="space-y-10">
                                        {profile.educations.map((edu: any, idx: number) => (
                                            <div key={idx} className="flex gap-4 group">
                                                <div className="p-3 bg-blue-50 rounded-xl border border-blue-100 text-blue-600 self-start">
                                                    <Building2 className="w-6 h-6" />
                                                </div>
                                                <div className="flex-1 space-y-1 pb-8 border-b last:border-0 border-slate-100">
                                                    <h4 className="text-lg font-bold text-slate-900 leading-snug">{edu.institutionName || 'University of Information'}</h4>
                                                    <p className="text-slate-600 font-bold text-sm">{edu.degree}{edu.major ? ` in ${edu.major}` : ''}</p>
                                                    <div className="flex items-center gap-2 text-slate-400 text-xs font-bold uppercase tracking-widest mt-1">
                                                        <GraduationCap className="w-3.5 h-3.5 text-blue-400" />
                                                        Class of {edu.graduationYear}
                                                    </div>
                                                </div>
                                            </div>
                                        ))}
                                    </div>
                                ) : (
                                    <p className="text-slate-400 italic">No academic history available.</p>
                                )}
                            </CardContent>
                        </Card>
                    </div>

                    {/* Sidebar Column */}
                    <div className="lg:col-span-4 space-y-8">
                        {/* Contact Card */}
                        <Card className="border-slate-200 border shadow-sm">
                            <CardContent className="p-8">
                                <h4 className="font-black text-slate-900 mb-6 uppercase tracking-widest text-xs">Contact Information</h4>
                                <div className="space-y-6">
                                    {profile.emails?.map((email: any) => (
                                        <div key={email.id} className="flex items-center gap-4">
                                            <div className="w-10 h-10 rounded-xl bg-slate-50 flex items-center justify-center text-slate-400">
                                                <Mail className="w-4 h-4" />
                                            </div>
                                            <div className="flex flex-col">
                                                <span className="text-[10px] font-bold text-slate-400 uppercase tracking-widest leading-none mb-1">Email</span>
                                                <span className="text-slate-700 font-bold text-sm">{email.email}</span>
                                            </div>
                                        </div>
                                    ))}
                                    {profile.mobiles?.map((mobile: any) => (
                                        <div key={mobile.id} className="flex items-center gap-4">
                                            <div className="w-10 h-10 rounded-xl bg-slate-50 flex items-center justify-center text-slate-400">
                                                <Globe className="w-4 h-4" />
                                            </div>
                                            <div className="flex flex-col">
                                                <span className="text-[10px] font-bold text-slate-400 uppercase tracking-widest leading-none mb-1">Mobile</span>
                                                <span className="text-slate-700 font-bold text-sm">{mobile.mobileNumber}</span>
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            </CardContent>
                        </Card>

                        {/* Public Link */}
                        <Card className="border-slate-200 border shadow-sm bg-gradient-to-br from-white to-slate-50">
                            <CardContent className="p-8">
                                <h4 className="font-black text-slate-900 mb-4 uppercase tracking-widest text-xs">Profile Link</h4>
                                <div className="p-4 rounded-xl bg-white border border-slate-200 flex flex-col gap-2">
                                    <span className="text-[11px] font-medium text-slate-400 break-all select-all">
                                        {window.location.origin}/portal/directory/{id}
                                    </span>
                                    <Button variant="ghost" size="sm" className="text-blue-600 font-bold uppercase tracking-widest text-[10px] self-end h-8">
                                        Copy Link
                                    </Button>
                                </div>
                            </CardContent>
                        </Card>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default AlumniProfile;
