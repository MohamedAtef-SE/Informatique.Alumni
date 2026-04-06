import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { alumniService } from '../../services/alumniService';
import { advisoryCategoryService } from '../../services/advisoryCategoryService';
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '../ui/Dialog';
import { Button } from '../ui/Button';
import { toast } from 'sonner';
import { ShieldCheck, GraduationCap, Briefcase, ChevronRight, Check } from 'lucide-react';

interface BecomeAdvisorModalProps {
    isOpen: boolean;
    onClose: () => void;
}

export const BecomeAdvisorModal = ({ isOpen, onClose }: BecomeAdvisorModalProps) => {
    const queryClient = useQueryClient();
    const [step, setStep] = useState(1);
    
    // Form State
    const [bio, setBio] = useState('');
    const [experienceYears, setExperienceYears] = useState<number>(5);
    const [selectedExpertise, setSelectedExpertise] = useState<string[]>([]);

    // Fetch Expertise Types (Professional Categories)
    const { data: expertiseTypes, isLoading: isExpertiseLoading } = useQuery({
        queryKey: ['expertise-types'],
        queryFn: advisoryCategoryService.getActiveList,
        enabled: isOpen
    });

    // Fetch My Profile for Status Check
    const { data: profile, isLoading: isProfileLoading } = useQuery({
        queryKey: ['my-profile'],
        queryFn: alumniService.getMyProfile,
        enabled: isOpen
    });

    const applyMutation = useMutation({
        mutationFn: alumniService.applyAsAdvisor,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['my-profile'] });
            toast.success('Application submitted! We will review your profile and get back to you.');
            onClose();
            setStep(1);
            setBio('');
            setSelectedExpertise([]);
        },
        onError: (error: any) => {
            toast.error(error.response?.data?.error?.message || 'Failed to submit application');
        }
    });

    const toggleExpertise = (id: string) => {
        setSelectedExpertise(prev => 
            prev.includes(id) ? prev.filter(x => x !== id) : [...prev, id]
        );
    };

    const handleNext = () => {
        if (step === 1 && (!bio.trim() || bio.length < 50)) {
            toast.error('Please provide a detailed bio (at least 50 characters)');
            return;
        }
        setStep(step + 1);
    };

    const handleSubmit = () => {
        if (selectedExpertise.length === 0) {
            toast.error('Please select at least one area of expertise');
            return;
        }
        applyMutation.mutate({
            bio,
            experienceYears,
            expertiseIds: selectedExpertise
        });
    };

    return (
        <Dialog open={isOpen} onOpenChange={onClose}>
            <DialogContent className="sm:max-w-2xl bg-slate-900 border-slate-800 text-slate-100 p-0 overflow-hidden">
                <div className="flex h-full min-h-[500px]">
                    {/* Left Sidebar - Progress */}
                    <div className="w-1/3 bg-slate-800/10 border-r border-slate-800 p-8 hidden sm:block">
                        <div className="flex flex-col gap-8">
                            <div className="flex items-center gap-3">
                                <div className="w-10 h-10 rounded-xl bg-indigo-500/20 flex items-center justify-center border border-indigo-500/30">
                                    <ShieldCheck className="w-6 h-6 text-indigo-400" />
                                </div>
                                <span className="font-bold text-lg tracking-tight">Become an Advisor</span>
                            </div>

                            <div className="space-y-6">
                                {[
                                    { s: 1, label: 'Professional Bio', icon: <UserIcon className="w-4 h-4" /> },
                                    { s: 2, label: 'Experience', icon: <Briefcase className="w-4 h-4" /> },
                                    { s: 3, label: 'Expertise Areas', icon: <GraduationCap className="w-4 h-4" /> }
                                ].map((item) => (
                                    <div key={item.s} className={`flex items-center gap-3 transition-all ${step >= item.s ? 'text-indigo-400' : 'text-slate-600'}`}>
                                        <div className={`w-8 h-8 rounded-full border flex items-center justify-center text-xs font-bold ${
                                            step === item.s ? 'border-indigo-500 bg-indigo-500/10 scale-110' : 
                                            step > item.s ? 'border-emerald-500 bg-emerald-500/10' : 'border-slate-700'
                                        }`}>
                                            {step > item.s ? <Check className="w-4 h-4 text-emerald-500" /> : item.s}
                                        </div>
                                        <span className={`text-sm font-medium ${step === item.s ? 'font-bold' : ''}`}>{item.label}</span>
                                    </div>
                                ))}
                            </div>
                        </div>
                    </div>

                    {/* Main Form Content */}
                    <div className="flex-1 flex flex-col p-8 bg-slate-900/50">
                        {isProfileLoading ? (
                             <div className="flex-1 flex items-center justify-center">
                                <div className="w-8 h-8 border-4 border-indigo-500 border-t-transparent rounded-full animate-spin" />
                             </div>
                        ) : profile?.status !== 1 ? (
                            <div className="flex-1 flex flex-col items-center justify-center text-center space-y-6 max-w-md mx-auto animate-in fade-in zoom-in-95">
                                <div className="w-20 h-20 rounded-full bg-amber-500/10 flex items-center justify-center border border-amber-500/20 shadow-[0_0_30px_rgba(245,158,11,0.1)]">
                                    <ShieldCheck className="w-10 h-10 text-amber-500" />
                                </div>
                                <div className="space-y-2">
                                    <h3 className="text-xl font-bold text-slate-50">Verification Required</h3>
                                    <p className="text-slate-400 text-sm leading-relaxed">
                                        The Alumni Advisory program is only available to **Active Members**. Your profile is currently pending verification or inactive.
                                    </p>
                                </div>
                                <div className="p-4 bg-slate-800/50 border border-slate-700/50 rounded-xl w-full text-left">
                                    <p className="text-xs text-slate-500 font-medium uppercase tracking-wider mb-2">How to get access:</p>
                                    <ul className="text-xs text-slate-400 space-y-2 list-disc pl-4">
                                        <li>Ensure your profile information is complete.</li>
                                        <li>Wait for administrative approval.</li>
                                        <li>Contact support if your verification is delayed.</li>
                                    </ul>
                                </div>
                                <Button 
                                    className="bg-slate-800 hover:bg-slate-700 text-slate-300 w-full" 
                                    onClick={onClose}
                                >
                                    Understood
                                </Button>
                            </div>
                        ) : (
                            <div className="flex-1 space-y-6">
                            {step === 1 && (
                                <div className="space-y-4 animate-in fade-in slide-in-from-right-4">
                                    <DialogHeader className="p-0 space-y-2">
                                        <DialogTitle className="text-2xl font-bold text-slate-50">Share your vision</DialogTitle>
                                        <DialogDescription className="text-slate-400">
                                            Write a professional bio specifically for mentorship. Explain how you can help current students and fellow alumni.
                                        </DialogDescription>
                                    </DialogHeader>
                                    <div className="space-y-2 pt-2">
                                        <label className="text-xs font-bold text-slate-500 uppercase tracking-widest">Advisory Bio</label>
                                        <textarea 
                                            placeholder="Example: Senior Software Engineer with 8 years of experience in FinTech. I can help with architectural decisions, career growth strategies, and interview preparation..."
                                            className="w-full min-h-[220px] bg-slate-800/50 border border-slate-700 focus:border-indigo-500 text-slate-100 placeholder:text-slate-600 rounded-xl p-4 outline-none resize-none shadow-inner transition-colors"
                                            value={bio}
                                            onChange={(e: any) => setBio(e.target.value)}
                                        />
                                        <div className="flex justify-between items-center text-[10px]">
                                            <span className={bio.length < 50 ? 'text-amber-500' : 'text-emerald-500'}>
                                                {bio.length < 50 ? `Minimum 50 characters required (${bio.length}/50)` : 'Characters: ' + bio.length}
                                            </span>
                                        </div>
                                    </div>
                                </div>
                            )}

                            {step === 2 && (
                                <div className="space-y-4 animate-in fade-in slide-in-from-right-4">
                                    <DialogHeader className="p-0 space-y-2">
                                        <DialogTitle className="text-2xl font-bold text-slate-50">Your Track Record</DialogTitle>
                                        <DialogDescription className="text-slate-400">
                                            How many years of professional experience do you have?
                                        </DialogDescription>
                                    </DialogHeader>
                                    <div className="space-y-6 pt-8">
                                        <div className="bg-indigo-500/5 border border-indigo-500/20 p-6 rounded-2xl text-center">
                                            <div className="text-5xl font-black text-indigo-400 mb-2">{experienceYears}</div>
                                            <div className="text-sm font-semibold text-slate-400 uppercase tracking-widest">Years of Experience</div>
                                        </div>
                                        <input 
                                            type="range" 
                                            min="1" 
                                            max="40" 
                                            value={experienceYears}
                                            onChange={(e: any) => setExperienceYears(parseInt(e.target.value))}
                                            className="w-full h-2 bg-slate-800 rounded-lg appearance-none cursor-pointer accent-indigo-500"
                                        />
                                        <div className="flex justify-between px-1 text-[10px] text-slate-600 font-bold">
                                            <span>1 YEAR</span>
                                            <span>40+ YEARS</span>
                                        </div>
                                    </div>
                                </div>
                            )}

                            {step === 3 && (
                                <div className="space-y-4 animate-in fade-in slide-in-from-right-4">
                                    <DialogHeader className="p-0 space-y-2">
                                        <DialogTitle className="text-2xl font-bold text-slate-50">Expertise Areas</DialogTitle>
                                        <DialogDescription className="text-slate-400">
                                            Select the industries or domains where you can provide high-value advice.
                                        </DialogDescription>
                                    </DialogHeader>
                                    <div className="pt-2">
                                        {isExpertiseLoading ? (
                                            <div className="grid grid-cols-2 gap-2">
                                                {[1,2,3,4].map(i => <div key={i} className="h-10 bg-slate-800/50 rounded-lg animate-pulse" />)}
                                            </div>
                                        ) : (
                                            <div className="grid grid-cols-1 sm:grid-cols-2 gap-2 max-h-[300px] overflow-y-auto pr-2 custom-scrollbar">
                                                {expertiseTypes?.map((expertise: any) => (
                                                    <button
                                                        key={expertise.id}
                                                        onClick={() => toggleExpertise(expertise.id)}
                                                        className={`flex items-center gap-3 p-3 rounded-xl border transition-all text-sm group ${
                                                            selectedExpertise.includes(expertise.id)
                                                                ? 'bg-indigo-500/20 border-indigo-500 text-indigo-100 shadow-[0_0_15px_rgba(99,102,241,0.1)]'
                                                                : 'bg-slate-800/30 border-slate-700/50 text-slate-400 hover:border-slate-600 hover:bg-slate-800/50'
                                                        }`}
                                                    >
                                                        <div className={`w-5 h-5 rounded-md flex items-center justify-center transition-colors ${
                                                            selectedExpertise.includes(expertise.id) ? 'bg-indigo-500 text-white' : 'bg-slate-700/50 group-hover:bg-slate-700'
                                                        }`}>
                                                            {selectedExpertise.includes(expertise.id) && <Check className="w-3 h-3" />}
                                                        </div>
                                                        <span className="font-medium">{expertise.nameEn}</span>
                                                    </button>
                                                ))}
                                            </div>
                                        )}
                                        <div className="mt-4 text-[10px] text-slate-500 font-bold uppercase">
                                            {selectedExpertise.length} areas selected
                                        </div>
                                    </div>
                                </div>
                            )}
                        </div>
                    )}

                    {profile?.status === 1 && (
                            <DialogFooter className="mt-8 pt-6 border-t border-slate-800 flex justify-between items-center">
                                {step > 1 ? (
                                    <Button variant="ghost" className="text-slate-400" onClick={() => setStep(step - 1)}>Back</Button>
                                ) : (
                                    <div />
                                )}
                                
                                <div className="flex gap-3">
                                    <Button variant="ghost" className="text-slate-500" onClick={onClose}>Cancel</Button>
                                    {step < 3 ? (
                                        <Button className="bg-indigo-600 hover:bg-indigo-500 px-8 group" onClick={handleNext}>
                                            Next
                                            <ChevronRight className="w-4 h-4 ml-2 group-hover:translate-x-1 transition-transform" />
                                        </Button>
                                    ) : (
                                        <Button 
                                            className="bg-emerald-600 hover:bg-emerald-500 px-8" 
                                            onClick={handleSubmit}
                                            isLoading={applyMutation.isPending}
                                        >
                                            Submit Application
                                        </Button>
                                    )}
                                </div>
                            </DialogFooter>
                        )}
                    </div>
                </div>
            </DialogContent>
        </Dialog>
    );
};

// Internal icon component with specific name to avoid lucide-react confusion
const UserIcon = ({ className }: { className?: string }) => (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
        <path d="M19 21v-2a4 4 0 0 0-4-4H9a4 4 0 0 0-4 4v2" />
        <circle cx="12" cy="7" r="4" />
    </svg>
);
