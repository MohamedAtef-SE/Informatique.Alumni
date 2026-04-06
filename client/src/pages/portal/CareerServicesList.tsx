import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { useState } from 'react';
import { careerService } from '../../services/careerService';
import { Users, ArrowRight, Briefcase, Calendar, Clock, FileText, MapPin } from 'lucide-react';
import { Link } from 'react-router-dom';
import { cn } from '../../utils/cn';
import { Card, CardContent, CardHeader, CardTitle, CardFooter } from '../../components/ui/Card';
import { Button } from '../../components/ui/Button';
import { Input } from '../../components/ui/Input';
import { Label } from '../../components/ui/Label';
import { toast } from 'sonner';
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogFooter,
} from '../../components/ui/Dialog';
import FeedbackModal from '../../components/common/FeedbackModal';
import MyCv from './MyCv';
import { MembershipGuard } from '../../components/common/MembershipGuard';

const CareerServicesList = () => {
    const [activeTab, setActiveTab] = useState<'services' | 'jobs' | 'guidance' | 'cv'>('services');
    const { t } = useTranslation();

    return (
        <div className="space-y-8 animate-fade-in">
            <div>
                <h1 className="text-3xl font-heading font-bold text-[var(--color-text-primary)]">
                    {t('career.title')}
                </h1>
                <p className="text-[var(--color-text-secondary)] mt-2">{t('career.subtitle')}</p>
            </div>

            {/* Tabs */}
            <div className="flex gap-2 border-b border-[var(--color-border)] pb-1">
                <TabButton
                    active={activeTab === 'services'}
                    onClick={() => setActiveTab('services')}
                    icon={<Briefcase className="w-4 h-4" />}
                    label={t('career.tabs.workshops')}
                />
                <TabButton
                    active={activeTab === 'jobs'}
                    onClick={() => setActiveTab('jobs')}
                    icon={<Briefcase className="w-4 h-4" />}
                    label={t('career.tabs.jobs')}
                />
                <TabButton
                    active={activeTab === 'guidance'}
                    onClick={() => setActiveTab('guidance')}
                    icon={<Users className="w-4 h-4" />}
                    label={t('career.tabs.guidance')}
                />
                <TabButton
                    active={activeTab === 'cv'}
                    onClick={() => setActiveTab('cv')}
                    icon={<FileText className="w-4 h-4" />}
                    label={t('career.tabs.cv', 'My CV')}
                />
            </div>

            <div className="min-h-[400px]">
                {activeTab === 'services' && <ServicesTab />}
                {activeTab === 'jobs' && <JobsTab />}
                {activeTab === 'guidance' && <GuidanceTab />}
                {activeTab === 'cv' && <MyCv />}
            </div>
        </div>
    );
};

const TabButton = ({ active, onClick, icon, label }: { active: boolean, onClick: () => void, icon: React.ReactNode, label: string }) => (
    <button
        onClick={onClick}
        className={cn(
            "px-6 py-3 flex items-center gap-2 border-b-2 transition-all duration-300 font-medium text-sm",
            active
                ? "border-[var(--color-accent)] text-[var(--color-accent)] bg-[var(--color-accent-light)]/20 rounded-t-lg"
                : "border-transparent text-[var(--color-text-secondary)] hover:text-[var(--color-text-primary)] hover:bg-slate-100 rounded-t-lg"
        )}
    >
        {icon}
        {label}
    </button>
);

const ServicesTab = () => {
    const { t } = useTranslation();
    const { data } = useQuery({
        queryKey: ['career-services'],
        queryFn: () => careerService.getServices({ maxResultCount: 20 })
    });

    return (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 animate-slide-up">
            {data?.items.map((service) => (
                <Card key={service.id} variant="default" className="group hover:border-[var(--color-accent)]/50 transition-colors h-full flex flex-col border-[var(--color-border)]">
                    <CardHeader>
                        <div className="flex justify-between items-start mb-2">
                            <span className={cn(
                                "px-2.5 py-1 rounded-full text-xs font-bold uppercase border",
                                service.hasFees
                                    ? "bg-amber-100 text-amber-700 border-amber-200"
                                    : "bg-emerald-100 text-emerald-700 border-emerald-200"
                            )}>
                                {service.hasFees ? `${service.feeAmount} ${t('common.currency')}` : t('career.free')}
                            </span>
                            <span className="text-xs font-mono text-[var(--color-text-muted)] border border-[var(--color-border)] px-2 py-1 rounded">
                                {service.code}
                            </span>
                        </div>
                        <CardTitle className="group-hover:text-[var(--color-accent)] transition-colors text-[var(--color-text-primary)]">{service.nameEn}</CardTitle>
                    </CardHeader>
                    <CardContent className="flex-1">
                        <p className="text-[var(--color-text-secondary)] text-sm line-clamp-3 leading-relaxed">
                            {service.description}
                        </p>
                    </CardContent>
                    <CardFooter>
                        <Link to={`/portal/career/${service.id}`} className="w-full">
                            <Button variant="outline" className="w-full group-hover:bg-[var(--color-accent)] group-hover:text-white transition-all">
                                {t('career.view_details')} <ArrowRight className="w-4 h-4 ml-2 rtl:rotate-180" />
                            </Button>
                        </Link>
                    </CardFooter>
                </Card>
            ))}
            {data?.items.length === 0 && (
                <div className="col-span-full py-20 text-center text-[var(--color-text-muted)] bg-slate-50 rounded-xl border border-dashed border-[var(--color-border)]">
                    {t('career.no_services')}
                </div>
            )}
        </div>
    );
};

const JobsTab = () => {
    const { t } = useTranslation();
    const { data } = useQuery({ queryKey: ['jobs'], queryFn: () => careerService.getJobs({ maxResultCount: 20 }) });

    // Application State
    const [selectedJob, setSelectedJob] = useState<any>(null);
    const [confirmOpen, setConfirmOpen] = useState(false);
    const [feedback, setFeedback] = useState<{ open: boolean, variant: 'success' | 'warning' | 'info', title: string, message: string }>({
        open: false, variant: 'success', title: '', message: ''
    });

    const applyMutation = useMutation({
        mutationFn: (jobId: string) => careerService.applyJob(jobId),
        onSuccess: () => {
            setConfirmOpen(false);
            setFeedback({
                open: true,
                variant: 'success',
                title: t('career.jobs.success_title', 'Application Sent!'),
                message: t('career.jobs.success_message', 'Your application has been successfully submitted. Good luck!')
            });
            // Optionally mark job as applied in local state or refetch
            // queryClient.invalidateQueries({ queryKey: ['jobs'] });
        },
        onError: (err: any) => {
            setConfirmOpen(false);
            console.error(err);
            const errorMsg = err.response?.data?.error?.message || err.message;
            const isCvError = errorMsg.includes("CV") || err.response?.data?.error?.code === "Career:NoCvFound";

            setFeedback({
                open: true,
                variant: isCvError ? 'warning' : 'info',
                title: t('career.jobs.error_title', 'Application Failed'),
                message: isCvError
                    ? t('career.jobs.cv_error', 'We could not find a valid CV in your profile. Please create or approve your CV first.')
                    : errorMsg
            });
        }
    });

    const handleApplyClick = (job: any) => {
        setSelectedJob(job);
        setConfirmOpen(true);
    };

    const confirmApply = () => {
        if (selectedJob) {
            applyMutation.mutate(selectedJob.id);
        }
    };

    return (
        <div className="space-y-6 animate-slide-up">

            {data?.items.map((job: any) => (
                <Card key={job.id} variant="default" className="hover:bg-slate-50 transition-colors border-[var(--color-border)]">
                    <CardContent className="pt-6 flex flex-col md:flex-row gap-6">
                        <div className="flex-1">
                            <h3 className="text-xl font-bold text-[var(--color-text-primary)] mb-1 font-heading">{job.title}</h3>
                            <p className="text-[var(--color-accent)] mb-3 font-medium flex items-center gap-2">
                                <Briefcase className="w-4 h-4" /> {job.companyName}
                                <span className="text-[var(--color-text-muted)]">•</span>
                                <span className="text-[var(--color-text-secondary)]">{job.location}</span>
                            </p>
                            <p className="text-sm text-[var(--color-text-secondary)] line-clamp-2 mb-4 leading-relaxed">{job.description}</p>
                            <div className="flex flex-wrap gap-2">
                                {job.tags?.map((tag: string) => (
                                    <span key={tag} className="px-2 py-1 rounded-md bg-slate-100 border border-slate-200 text-xs text-[var(--color-text-secondary)]">
                                        {tag}
                                    </span>
                                ))}
                            </div>
                        </div>
                        <div className="flex items-center justify-end md:w-48">
                            <MembershipGuard>
                                <Button
                                    className="w-full md:w-auto shadow-md shadow-blue-500/10"
                                    onClick={() => handleApplyClick(job)}
                                    disabled={applyMutation.isPending && selectedJob?.id === job.id}
                                >
                                    {t('career.jobs.apply')}
                                </Button>
                            </MembershipGuard>
                        </div>
                    </CardContent>
                </Card>
            ))}
            {data?.items.length === 0 && <p className="text-center py-20 text-[var(--color-text-muted)] flex items-center justify-center border border-dashed border-[var(--color-border)] rounded-xl bg-slate-50">{t('career.jobs.empty')}</p>}

            {/* Confirmation Dialog */}
            <Dialog open={confirmOpen} onOpenChange={setConfirmOpen}>
                <DialogContent>
                    <DialogHeader>
                        <DialogTitle>{t('career.jobs.confirm_title', 'Apply for this Job?')}</DialogTitle>
                    </DialogHeader>
                    <div className="py-4">
                        <p className="text-[var(--color-text-secondary)]">
                            {t('career.jobs.confirm_message', 'We will submit your current CV to')} <span className="font-bold">{selectedJob?.companyName}</span>.
                        </p>
                        <p className="text-xs text-[var(--color-text-muted)] mt-2">
                            {t('career.jobs.confirm_note', 'Make sure your CV is up to date.')}
                        </p>
                    </div>
                    <DialogFooter className="gap-2 sm:gap-0">
                        <Button variant="ghost" onClick={() => setConfirmOpen(false)}>
                            {t('common.cancel')}
                        </Button>
                        <Button onClick={confirmApply} disabled={applyMutation.isPending}>
                            {applyMutation.isPending ? t('common.processing') : t('career.jobs.confirm_btn', 'Yes, Apply Now')}
                        </Button>
                    </DialogFooter>
                </DialogContent>
            </Dialog>

            {/* Feedback Modal */}
            <FeedbackModal
                isOpen={feedback.open}
                variant={feedback.variant}
                title={feedback.title}
                message={feedback.message}
                onClose={() => setFeedback(prev => ({ ...prev, open: false }))}
            />
        </div>
    );
};

const GuidanceTab = () => {
    const { t } = useTranslation();
    const queryClient = useQueryClient();
    const { data: sessions } = useQuery({ queryKey: ['my-sessions'], queryFn: careerService.getBookedSessions });
    const { data: advisors } = useQuery({ queryKey: ['advisors'], queryFn: careerService.getAdvisors });
    
    const [topic, setTopic] = useState('');
    const [selectedAdvisorId, setSelectedAdvisorId] = useState('');
    const [selectedDate, setSelectedDate] = useState('');
    const [selectedTime, setSelectedTime] = useState('');

    const selectedAdvisor = advisors?.find((a: any) => a.id === selectedAdvisorId);

    // Fetch Branch Rules for the selected advisor
    const { data: rule, isLoading: isLoadingRule } = useQuery({
        queryKey: ['guidance-rule', selectedAdvisor?.branchId],
        queryFn: () => careerService.getGuidanceRule(selectedAdvisor.branchId),
        enabled: !!selectedAdvisor?.branchId
    });

    const bookMutation = useMutation({
        mutationFn: careerService.bookSession,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['my-sessions'] });
            toast.success(t('career.guidance.success', 'Booking Request Sent!'));
            setTopic('');
            setSelectedDate('');
            setSelectedTime('');
        },
        onError: (err: any) => {
            const msg = err.response?.data?.error?.message || err.message || t('career.guidance.error');
            toast.error(msg);
        }
    });

    // Helper to check if a day is valid
    const isDayValid = (dateStr: string) => {
        if (!dateStr || !rule) return true;
        const day = new Date(dateStr).getDay();
        // C# DayOfWeek: Sun=0, Mon=1... matches JS getDay()
        return rule.weekDays.includes(day);
    };

    // Helper to generate time slots
    const generateSlots = () => {
        if (!rule) return [];
        const slots = [];
        const [startH, startM] = rule.startTime.split(':').map(Number);
        const [endH, endM] = rule.endTime.split(':').map(Number);
        const duration = rule.sessionDurationMinutes;

        let current = new Date();
        current.setHours(startH, startM, 0, 0);
        const end = new Date();
        end.setHours(endH, endM, 0, 0);

        while (current < end) {
            slots.push(current.toTimeString().substring(0, 5));
            current.setMinutes(current.getMinutes() + duration);
        }
        return slots;
    };

    const slots = generateSlots();
    const isDateBlocked = selectedDate && !isDayValid(selectedDate);

    return (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8 animate-slide-up">
            {/* My Sessions List */}
            <div className="lg:col-span-2 space-y-6">
                <h2 className="text-xl font-bold text-[var(--color-text-primary)] flex items-center gap-2">
                    <Calendar className="w-5 h-5 text-[var(--color-accent)]" /> {t('career.guidance.my_sessions')}
                </h2>
                {sessions?.items.map((session: any) => (
                    <Card key={session.id} variant="default" className="hover:border-[var(--color-accent)]/30 transition-shadow hover:shadow-md border-[var(--color-border)]">
                        <CardContent className="p-4 flex justify-between items-center">
                            <div className="space-y-3">
                                <h4 className="font-bold text-[var(--color-text-primary)] text-lg">{session.subject}</h4>
                                <div className="grid grid-cols-1 sm:grid-cols-2 gap-x-6 gap-y-1">
                                    <p className="text-sm text-[var(--color-text-secondary)] flex items-center gap-2">
                                        <Users className="w-3.5 h-3.5 text-[var(--color-accent)]" />
                                        <span className="font-medium">{session.advisorName || 'Advisor'}</span>
                                        {session.advisorJobTitle && <span className="text-xs text-[var(--color-text-muted)]">({session.advisorJobTitle})</span>}
                                    </p>
                                    <p className="text-sm text-[var(--color-text-muted)] flex items-center gap-2">
                                        <Clock className="w-3.5 h-3.5" />
                                        {new Date(session.startTime).toLocaleString()}
                                    </p>
                                    <p className="text-sm text-[var(--color-text-muted)] flex items-center gap-2">
                                        <MapPin className="w-3.5 h-3.5" />
                                        {session.location || 'Online'}
                                    </p>
                                </div>
                            </div>
                            <div className={cn(
                                "px-3 py-1 rounded-full text-xs font-bold uppercase border",
                                session.status === 0 ? "bg-amber-100 text-amber-700 border-amber-200" :
                                session.status === 1 ? "bg-emerald-100 text-emerald-700 border-emerald-200" :
                                session.status === 2 ? "bg-red-100 text-red-700 border-red-200" :
                                session.status === 3 ? "bg-blue-100 text-blue-700 border-blue-200" :
                                "bg-slate-100 text-slate-700 border-slate-200"
                            )}>
                                {session.status === 0 ? t('career.guidance.status.pending', 'Pending') : 
                                 session.status === 1 ? t('career.guidance.status.approved', 'Approved') : 
                                 session.status === 2 ? t('career.guidance.status.rejected', 'Rejected') : 
                                 session.status === 3 ? t('career.guidance.status.completed', 'Completed') :
                                 t('career.guidance.status.canceled', 'Canceled')}
                            </div>
                        </CardContent>
                    </Card>
                ))}
                {(!sessions?.items || sessions.items.length === 0) && (
                    <div className="text-center py-10 text-[var(--color-text-muted)] bg-slate-50 rounded-xl border border-dashed border-[var(--color-border)]">
                        {t('career.guidance.no_sessions')}
                    </div>
                )}
            </div>

            {/* Booking Side Panel */}
            <div className="h-fit sticky top-24">
                <Card variant="default" className="shadow-xl border-slate-100">
                    <CardHeader className="bg-slate-50/50 border-b border-slate-100">
                        <CardTitle className="text-lg">Book a Session</CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-6 pt-6">
                        {/* Advisor Selection */}
                        <div className="space-y-2">
                            <label className="text-xs font-semibold text-[var(--color-text-secondary)] uppercase tracking-wider">{t('career.guidance.advisor_label')}</label>
                            <div className="relative group">
                                <select
                                    className="w-full bg-white border border-[var(--color-border)] rounded-xl px-4 py-3 text-[var(--color-text-primary)] text-sm focus:border-[var(--color-accent)] focus:ring-1 focus:ring-[var(--color-accent)] outline-none appearance-none transition-all group-hover:border-slate-300"
                                    value={selectedAdvisorId}
                                    onChange={(e) => setSelectedAdvisorId(e.target.value)}
                                >
                                    <option value="">{t('career.guidance.advisor_placeholder')}</option>
                                    {advisors?.map((advisor: any) => (
                                        <option key={advisor.id} value={advisor.id}>
                                            {advisor.name} ({advisor.jobTitle || 'Advisor'})
                                        </option>
                                    ))}
                                </select>
                                <div className="absolute right-4 top-1/2 -translate-y-1/2 pointer-events-none text-slate-400">
                                    <ArrowRight className="w-4 h-4 rotate-90" />
                                </div>
                            </div>
                        </div>

                        {/* Date Picker */}
                        <div className="space-y-2">
                            <Label className="text-xs font-semibold text-[var(--color-text-secondary)] uppercase tracking-wider">Select Date</Label>
                            <Input
                                type="date"
                                className="bg-white border-[var(--color-border)] text-[var(--color-text-primary)] rounded-xl h-11"
                                value={selectedDate}
                                min={new Date().toISOString().split('T')[0]}
                                onChange={(e) => {
                                    setSelectedDate(e.target.value);
                                    setSelectedTime('');
                                }}
                                disabled={!selectedAdvisorId}
                            />
                            {isDateBlocked && (
                                <p className="text-[11px] text-red-500 font-medium flex items-center gap-1 animate-in fade-in slide-in-from-top-1">
                                    Our office is closed on this day. Please try another date.
                                </p>
                            )}
                        </div>

                        {/* Interactive Slot Picker */}
                        {selectedDate && !isDateBlocked && (
                            <div className="space-y-3 animate-in zoom-in-95 duration-200">
                                <Label className="text-xs font-semibold text-[var(--color-text-secondary)] uppercase tracking-wider">Available Times</Label>
                                <div className="grid grid-cols-3 gap-2">
                                    {slots.map(slot => (
                                        <button
                                            key={slot}
                                            type="button"
                                            onClick={() => setSelectedTime(slot)}
                                            className={cn(
                                                "py-2.5 px-2 rounded-lg text-xs font-bold transition-all border",
                                                selectedTime === slot
                                                    ? "bg-[var(--color-accent)] border-[var(--color-accent)] text-white shadow-lg shadow-[var(--color-accent)]/20 scale-105"
                                                    : "bg-white border-slate-100 text-slate-600 hover:border-slate-300"
                                            )}
                                        >
                                            {slot}
                                        </button>
                                    ))}
                                </div>
                            </div>
                        )}

                        {/* Topic */}
                        <div className="space-y-2">
                            <Label className="text-xs font-semibold text-[var(--color-text-secondary)] uppercase tracking-wider">Discussion Topic</Label>
                            <Input
                                placeholder="What would you like to discuss?"
                                value={topic}
                                onChange={(e) => setTopic(e.target.value)}
                                className="bg-white border-[var(--color-border)] text-[var(--color-text-primary)] rounded-xl"
                                disabled={!selectedTime}
                            />
                        </div>

                        <MembershipGuard>
                            <Button
                                onClick={() => {
                                    if (!selectedAdvisorId) return toast.error(t('career.guidance.select_advisor_error'));
                                    if (!selectedDate || !selectedTime) return toast.error("Please select both a date and a time slot");

                                    bookMutation.mutate({
                                        advisorId: selectedAdvisorId,
                                        date: new Date(selectedDate).toISOString(),
                                        startTime: selectedTime,
                                        subject: topic || "Career Counseling"
                                    });
                                }}
                                disabled={bookMutation.isPending || !selectedTime}
                                className="w-full h-11 rounded-xl shadow-lg shadow-blue-500/20 font-bold transition-all disabled:opacity-50"
                            >
                                {bookMutation.isPending ? t('career.guidance.requesting') : t('career.guidance.request_btn')}
                            </Button>
                        </MembershipGuard>
                    </CardContent>
                </Card>
            </div>
        </div>
    );
};

export default CareerServicesList;
