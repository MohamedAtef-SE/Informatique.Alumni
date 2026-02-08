import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery } from '@tanstack/react-query';
import { alumniService } from '../../services/alumniService';
import { careerService } from '../../services/careerService';
import { Mail, Phone, MapPin, Globe, Calendar, GraduationCap, Briefcase } from 'lucide-react';
import ProfileHeader from '../../components/Profile/ProfileHeader';
import ProfileTabs from '../../components/Profile/ProfileTabs';
import EditProfileModal from '../../components/Profile/EditProfileModal';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/Card';

const Profile = () => {
    const { t } = useTranslation();
    const [activeTab, setActiveTab] = useState('about');
    const [isEditModalOpen, setIsEditModalOpen] = useState(false);

    const { data: profile, isLoading } = useQuery({
        queryKey: ['my-profile'],
        queryFn: () => alumniService.getMyProfile(),
    });

    const { data: cvData } = useQuery({
        queryKey: ['my-cv'],
        queryFn: () => careerService.getMyCv(),
    });

    if (isLoading) {
        return <div className="flex justify-center py-20"><div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div></div>;
    }

    if (!profile) return null;

    return (
        <div className="min-h-screen bg-slate-50 pb-20">
            {/* Header */}
            <ProfileHeader
                profile={profile}
                onEdit={() => setIsEditModalOpen(true)}
                isOwnProfile={true}
            />

            <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8">
                {/* Tabs */}
                <ProfileTabs activeTab={activeTab} onTabChange={setActiveTab} />

                {/* Tab Content */}
                <div className="animate-fade-in">
                    {activeTab === 'about' && (
                        <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
                            <div className="md:col-span-2 space-y-6">
                                {/* Bio */}
                                <Card>
                                    <CardHeader>
                                        <CardTitle>{t('profile.about')}</CardTitle>
                                    </CardHeader>
                                    <CardContent>
                                        <p className="text-slate-600 leading-relaxed whitespace-pre-wrap">
                                            {profile.bio || t('profile.no_bio')}
                                        </p>
                                    </CardContent>
                                </Card>

                                {/* Professional Info */}
                                <Card>
                                    <CardHeader>
                                        <CardTitle>{t('my_profile.sections.professional')}</CardTitle>
                                    </CardHeader>
                                    <CardContent className="space-y-4">
                                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                            <div>
                                                <label className="text-sm font-medium text-slate-500">{t('my_profile.form.job_title')}</label>
                                                <p className="text-slate-900 font-medium">{profile.jobTitle || '-'}</p>
                                            </div>
                                            <div>
                                                <label className="text-sm font-medium text-slate-500">{t('my_profile.form.company')}</label>
                                                <p className="text-slate-900 font-medium">{profile.company || '-'}</p>
                                            </div>
                                        </div>
                                    </CardContent>
                                </Card>
                            </div>

                            <div className="space-y-6">
                                {/* Contact Info */}
                                <Card>
                                    <CardHeader>
                                        <CardTitle>{t('profile.contact')}</CardTitle>
                                    </CardHeader>
                                    <CardContent className="space-y-4">
                                        {/* Emails */}
                                        <div className="space-y-2">
                                            <h4 className="text-sm font-medium text-slate-900 flex items-center gap-2">
                                                <Mail className="w-4 h-4 text-slate-400" /> {t('my_profile.form.emails')}
                                            </h4>
                                            {profile.emails && profile.emails.length > 0 ? (
                                                <ul className="space-y-1">
                                                    {profile.emails.map((email: any) => (
                                                        <li key={email.id} className="text-sm text-slate-600 flex justify-between">
                                                            <span>{email.email}</span>
                                                            {email.isPrimary && <span className="text-xs bg-blue-100 text-blue-700 px-2 py-0.5 rounded-full">Primary</span>}
                                                        </li>
                                                    ))}
                                                </ul>
                                            ) : (
                                                <p className="text-sm text-slate-400 italic">No emails listed</p>
                                            )}
                                        </div>

                                        {/* Mobiles */}
                                        <div className="space-y-2 border-t pt-3">
                                            <h4 className="text-sm font-medium text-slate-900 flex items-center gap-2">
                                                <Phone className="w-4 h-4 text-slate-400" /> {t('my_profile.form.mobiles')}
                                            </h4>
                                            {profile.mobiles && profile.mobiles.length > 0 ? (
                                                <ul className="space-y-1">
                                                    {profile.mobiles.map((mobile: any) => (
                                                        <li key={mobile.id} className="text-sm text-slate-600 flex justify-between">
                                                            <span>{mobile.mobileNumber}</span>
                                                            {mobile.isPrimary && <span className="text-xs bg-blue-100 text-blue-700 px-2 py-0.5 rounded-full">Primary</span>}
                                                        </li>
                                                    ))}
                                                </ul>
                                            ) : (
                                                <p className="text-sm text-slate-400 italic">No mobile numbers</p>
                                            )}
                                        </div>

                                        {/* Socials */}
                                        <div className="space-y-2 border-t pt-3">
                                            <h4 className="text-sm font-medium text-slate-900 flex items-center gap-2">
                                                <Globe className="w-4 h-4 text-slate-400" /> Socials
                                            </h4>
                                            <div className="flex gap-3">
                                                {profile.linkedinUrl && (
                                                    <a href={profile.linkedinUrl} target="_blank" rel="noreferrer" className="text-blue-600 hover:text-blue-700 text-sm font-medium">LinkedIn</a>
                                                )}
                                                {profile.facebookUrl && (
                                                    <a href={profile.facebookUrl} target="_blank" rel="noreferrer" className="text-blue-800 hover:text-blue-900 text-sm font-medium">Facebook</a>
                                                )}
                                                {!profile.linkedinUrl && !profile.facebookUrl && <span className="text-sm text-slate-400 italic">None linked</span>}
                                            </div>
                                        </div>
                                    </CardContent>
                                </Card>

                                {/* Location */}
                                <Card>
                                    <CardContent className="pt-6">
                                        <div className="flex items-start gap-3">
                                            <MapPin className="w-5 h-5 text-slate-400 mt-0.5" />
                                            <div>
                                                <h4 className="text-sm font-medium text-slate-900">Location</h4>
                                                <p className="text-sm text-slate-600 mt-1">
                                                    {[profile.address, profile.city, profile.country].filter(Boolean).join(', ') || 'Not specified'}
                                                </p>
                                            </div>
                                        </div>
                                    </CardContent>
                                </Card>
                            </div>
                        </div>
                    )}

                    {activeTab === 'education' && (
                        <div className="space-y-6">
                            <Card>
                                <CardHeader>
                                    <CardTitle>Academic History</CardTitle>
                                </CardHeader>
                                <CardContent>
                                    {profile.academicHistory && profile.academicHistory.length > 0 ? (
                                        <div className="space-y-8 relative before:absolute before:left-2 before:top-2 before:bottom-2 before:w-0.5 before:bg-slate-200">
                                            {profile.academicHistory.map((edu: any, idx: number) => (
                                                <div key={idx} className="relative pl-8">
                                                    <div className="absolute left-0 top-1.5 w-4 h-4 rounded-full bg-white border-4 border-blue-600"></div>
                                                    <h4 className="text-lg font-bold text-slate-900">{edu.degreeName}{edu.major ? ` in ${edu.major}` : ''}</h4>
                                                    <p className="text-blue-600 font-medium">{edu.college}</p>
                                                    <div className="flex items-center gap-4 mt-2 text-sm text-slate-500">
                                                        <span className="flex items-center gap-1">
                                                            <Calendar className="w-4 h-4" /> Class of {edu.graduationYear}
                                                        </span>
                                                        {edu.cumulativeGPA && (
                                                            <span className="flex items-center gap-1">
                                                                <GraduationCap className="w-4 h-4" /> GPA: {edu.cumulativeGPA}
                                                            </span>
                                                        )}
                                                    </div>
                                                </div>
                                            ))}
                                        </div>
                                    ) : (
                                        <p className="text-slate-500 italic">No academic history found.</p>
                                    )}
                                </CardContent>
                            </Card>
                        </div>
                    )}

                    {activeTab === 'experience' && (
                        <div className="space-y-6">
                            <Card>
                                <CardHeader>
                                    <CardTitle className="flex items-center gap-2">
                                        <Briefcase className="w-5 h-5" /> Work Experience
                                    </CardTitle>
                                </CardHeader>
                                <CardContent>
                                    {cvData?.experiences && cvData.experiences.length > 0 ? (
                                        <div className="space-y-8 relative before:absolute before:left-2 before:top-2 before:bottom-2 before:w-0.5 before:bg-slate-200">
                                            {cvData.experiences.map((exp: any) => (
                                                <div key={exp.id} className="relative pl-8">
                                                    <div className="absolute left-0 top-1.5 w-4 h-4 rounded-full bg-white border-4 border-green-600"></div>
                                                    <h4 className="text-lg font-bold text-slate-900">{exp.position}</h4>
                                                    <p className="text-green-600 font-medium">{exp.company}</p>
                                                    <div className="flex items-center gap-4 mt-2 text-sm text-slate-500">
                                                        <span className="flex items-center gap-1">
                                                            <Calendar className="w-4 h-4" />
                                                            {new Date(exp.startDate).toLocaleDateString('en', { month: 'short', year: 'numeric' })}
                                                            {' - '}
                                                            {exp.endDate ? new Date(exp.endDate).toLocaleDateString('en', { month: 'short', year: 'numeric' }) : 'Present'}
                                                        </span>
                                                    </div>
                                                    {exp.description && (
                                                        <p className="mt-2 text-slate-600 text-sm">{exp.description}</p>
                                                    )}
                                                </div>
                                            ))}
                                        </div>
                                    ) : (
                                        <div className="text-center py-8 text-slate-500">
                                            <Briefcase className="w-12 h-12 mx-auto mb-4 text-slate-300" />
                                            <p>No work experience added yet.</p>
                                            <p className="mt-2 text-sm">Add your experience in the <a href="/portal/my-cv" className="text-blue-600 hover:underline">My CV</a> section.</p>
                                        </div>
                                    )}
                                </CardContent>
                            </Card>
                        </div>
                    )}
                </div>
            </div>

            <EditProfileModal
                isOpen={isEditModalOpen}
                onClose={() => setIsEditModalOpen(false)}
                profile={profile}
            />
        </div>
    );
};

export default Profile;
