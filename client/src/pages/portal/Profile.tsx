import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { alumniService } from '../../services/alumniService';
import { careerService } from '../../services/careerService';
import { 
    Calendar, GraduationCap, 
    Briefcase, ShieldCheck, Edit2, Plus, 
    Sparkles, Building2, BookOpen
} from 'lucide-react';
import ProfileHeader from '../../components/Profile/ProfileHeader';
import ProfileDashboard from '../../components/Profile/ProfileDashboard';
import ProfileCompletion from '../../components/Profile/ProfileCompletion';
import ProfileFeatured from '../../components/Profile/ProfileFeatured';
import ProfileSkills from '../../components/Profile/ProfileSkills';
import EditProfileModal from '../../components/Profile/EditProfileModal';
import { Card, CardContent } from '../../components/ui/Card';
import { BecomeAdvisorModal } from '../../components/Profile/BecomeAdvisorModal';
import { AdvisoryStatus } from '../../types/admin';
import { Button } from '../../components/ui/Button';
import LoadingLayer from '../../components/common/LoadingLayer';
import { cn } from '../../utils/cn';

const Profile = () => {
    const [isEditModalOpen, setIsEditModalOpen] = useState(false);
    const [isAdvisorModalOpen, setIsAdvisorModalOpen] = useState(false);

    const { data: profile, isLoading } = useQuery({
        queryKey: ['my-profile'],
        queryFn: () => alumniService.getMyProfile(),
    });

    const { data: cvData } = useQuery({
        queryKey: ['my-cv'],
        queryFn: () => careerService.getMyCv(),
    });

    if (isLoading) return <LoadingLayer />;
    if (!profile) return null;

    return (
        <div className="min-h-screen bg-[#f3f2ef] pb-20 font-sans">
            <div className="max-w-6xl mx-auto px-4 md:px-12 pt-8">
                {/* Standard Profile Header (Shared component) */}
                <ProfileHeader
                    profile={profile}
                    onEdit={() => setIsEditModalOpen(true)}
                    isOwnProfile={true}
                />

                <div className="grid grid-cols-1 lg:grid-cols-12 gap-8">
                    
                    {/* Main Content Column (LinkedIn style) */}
                    <div className="lg:col-span-8 space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-1000">
                        
                        {/* Analytics Dashboard (Private) */}
                        <ProfileDashboard viewCount={profile.viewCount || 0} />

                        {/* About Section */}
                        <Card className="border-slate-200 border shadow-sm">
                            <CardContent className="p-8">
                                <div className="flex items-center justify-between mb-6">
                                    <h3 className="text-2xl font-heading font-black text-slate-900 tracking-tight flex items-center gap-3">
                                        About
                                    </h3>
                                    <Button variant="ghost" size="icon" className="rounded-full text-slate-400 hover:text-blue-600 hover:bg-blue-50">
                                        <Edit2 className="w-5 h-5" />
                                    </Button>
                                </div>
                                <p className="text-slate-600 leading-relaxed whitespace-pre-wrap text-[15px]">
                                    {profile.bio || "You haven't added a bio yet. Summarize your professional background and aspirations to attract more opportunities."}
                                </p>
                            </CardContent>
                        </Card>

                        {/* Featured Items */}
                        <ProfileFeatured />

                        {/* Experience Section */}
                        <Card className="border-slate-200 border shadow-sm overflow-hidden">
                            <CardContent className="p-8">
                                <div className="flex items-center justify-between mb-8">
                                    <h3 className="text-2xl font-heading font-black text-slate-900 tracking-tight flex items-center gap-3">
                                        Experience
                                    </h3>
                                    <div className="flex gap-2">
                                        <Button variant="ghost" size="icon" className="rounded-full text-slate-400 hover:text-blue-600 hover:bg-blue-50">
                                            <Plus className="w-6 h-6" />
                                        </Button>
                                        <Button variant="ghost" size="icon" className="rounded-full text-slate-400 hover:text-blue-600 hover:bg-blue-50">
                                            <Edit2 className="w-5 h-5" />
                                        </Button>
                                    </div>
                                </div>

                                {cvData?.experiences && cvData.experiences.length > 0 ? (
                                    <div className="space-y-10">
                                        {cvData.experiences.map((exp: any) => (
                                            <div key={exp.id} className="flex gap-4 group">
                                                <div className="p-3 bg-slate-50 rounded-xl border border-slate-100 text-slate-400 group-hover:bg-blue-50 group-hover:text-blue-600 transition-all self-start">
                                                    <Briefcase className="w-6 h-6" />
                                                </div>
                                                <div className="flex-1 space-y-1 pb-8 border-b last:border-0 border-slate-100">
                                                    <h4 className="text-lg font-bold text-slate-900 leading-snug">{exp.position}</h4>
                                                    <p className="text-blue-600 font-bold text-sm tracking-wide uppercase">{exp.company}</p>
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
                                        <p className="text-slate-400 font-medium">No experience shown on your profile yet.</p>
                                        <Button variant="ghost" className="mt-4 text-blue-600 gap-2 font-bold uppercase tracking-widest text-xs">
                                            <Plus className="w-4 h-4" /> Add Experience
                                        </Button>
                                    </div>
                                )}
                            </CardContent>
                        </Card>

                        {/* Education Section */}
                        <Card className="border-slate-200 border shadow-sm">
                            <CardContent className="p-8">
                                <div className="flex items-center justify-between mb-8">
                                    <h3 className="text-2xl font-heading font-black text-slate-900 tracking-tight flex items-center gap-3">
                                        Education
                                    </h3>
                                    <Button variant="ghost" size="icon" className="rounded-full text-slate-400 hover:text-blue-600 hover:bg-blue-50">
                                        <Edit2 className="w-5 h-5" />
                                    </Button>
                                </div>

                                {profile.academicHistory && profile.academicHistory.length > 0 ? (
                                    <div className="space-y-10">
                                        {profile.academicHistory.map((edu: any) => (
                                            <div key={edu.id || Math.random()} className="flex gap-4 group">
                                                <div className="p-3 bg-blue-50 rounded-xl border border-blue-100 text-blue-600 self-start">
                                                    <Building2 className="w-6 h-6" />
                                                </div>
                                                <div className="flex-1 space-y-1 pb-8 border-b last:border-0 border-slate-100">
                                                    <h4 className="text-lg font-bold text-slate-900 leading-snug">{edu.college}</h4>
                                                    <p className="text-slate-600 font-bold text-sm">{edu.degreeName}{edu.major ? ` in ${edu.major}` : ''}</p>
                                                    <div className="flex items-center gap-2 text-slate-400 text-xs font-bold uppercase tracking-widest mt-1">
                                                        <GraduationCap className="w-3 h-3 text-blue-400" />
                                                        Class of {edu.graduationYear}
                                                    </div>
                                                </div>
                                            </div>
                                        ))}
                                    </div>
                                ) : (
                                    <p className="text-slate-400 italic">No academic foundation listed.</p>
                                )}
                            </CardContent>
                        </Card>

                        {/* Skills Section */}
                        <ProfileSkills />
                    </div>

                    {/* Sidebar Column */}
                    <div className="lg:col-span-4 space-y-8">
                        
                        {/* Profile Strength Meter */}
                        <ProfileCompletion profile={profile} />

                        {/* Advisory Status Card */}
                        <Card className={cn(
                            "overflow-hidden border-none shadow-xl transition-all duration-500",
                            profile.advisoryStatus === AdvisoryStatus.Approved ? "bg-gradient-to-br from-indigo-700 to-slate-900 text-white" : "bg-white border border-slate-200"
                        )}>
                            <CardContent className="p-8">
                                <div className="flex items-center gap-4 mb-6">
                                    <div className={cn(
                                        "p-3 rounded-2xl shadow-inner",
                                        profile.advisoryStatus === AdvisoryStatus.Approved ? "bg-white/10" : "bg-indigo-50 text-indigo-600"
                                    )}>
                                        <ShieldCheck className="w-6 h-6" />
                                    </div>
                                    <div>
                                        <h4 className="font-black text-lg">Mentorship</h4>
                                        <p className={cn(
                                            "text-xs font-black uppercase tracking-[0.2em] opacity-60",
                                            profile.advisoryStatus === AdvisoryStatus.Approved ? "text-indigo-100" : "text-slate-400"
                                        )}>
                                            Elite Advisors
                                        </p>
                                    </div>
                                </div>

                                {profile.advisoryStatus === AdvisoryStatus.None && (
                                    <div className="space-y-6">
                                        <p className="text-sm text-slate-600 leading-relaxed font-medium">
                                            Become a certified advisor and help shape the next generation of graduates.
                                        </p>
                                        <Button 
                                            className="w-full h-12 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl shadow-lg shadow-indigo-600/20 font-bold uppercase tracking-widest text-[10px]"
                                            onClick={() => setIsAdvisorModalOpen(true)}
                                        >
                                            Apply for Advisory
                                        </Button>
                                    </div>
                                )}
                            </CardContent>
                        </Card>
                    </div>
                </div>
            </div>

            <EditProfileModal
                isOpen={isEditModalOpen}
                onClose={() => setIsEditModalOpen(false)}
                profile={profile}
            />

            <BecomeAdvisorModal
                isOpen={isAdvisorModalOpen}
                onClose={() => setIsAdvisorModalOpen(false)}
            />
        </div>
    );
};

export default Profile;
