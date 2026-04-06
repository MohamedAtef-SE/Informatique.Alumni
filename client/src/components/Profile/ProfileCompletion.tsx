import React from 'react';
import { Card, CardContent } from '../ui/Card';
import { CheckCircle2, Circle, ArrowRight, ShieldCheck } from 'lucide-react';
import { cn } from '../../utils/cn';

interface ProfileCompletionProps {
    profile: any;
}

const ProfileCompletion: React.FC<ProfileCompletionProps> = ({ profile }) => {
    const steps = [
        { key: 'photoUrl', label: 'Add a profile photo', weight: 20 },
        { key: 'bio', label: 'Write a professional bio', weight: 20 },
        { key: 'jobTitle', label: 'Add your current job title', weight: 15 },
        { key: 'company', label: 'Add your current company', weight: 15 },
        { key: 'linkedinUrl', label: 'Link your LinkedIn profile', weight: 15 },
        { key: 'academicHistory', label: 'Complete academic history', weight: 15, isArray: true },
    ];

    const completedSteps = steps.filter(step => {
        if (step.isArray) return profile?.[step.key]?.length > 0;
        return !!profile?.[step.key];
    });

    const score = Math.min(100, completedSteps.reduce((acc, step) => acc + step.weight, 0));
    const nextStep = steps.find(step => {
        if (step.isArray) return !profile?.[step.key] || profile?.[step.key]?.length === 0;
        return !profile?.[step.key];
    });

    return (
        <Card className="border-none shadow-xl bg-gradient-to-br from-slate-900 to-indigo-950 text-white overflow-hidden relative">
            {/* Background Glow */}
            <div className="absolute -right-12 -bottom-12 w-32 h-32 bg-blue-500/20 rounded-full blur-3xl pointer-events-none" />
            
            <CardContent className="p-6">
                <div className="flex items-center gap-3 mb-6">
                    <div className="p-2 bg-white/10 rounded-xl text-blue-400">
                        <ShieldCheck className="w-5 h-5" />
                    </div>
                    <div>
                        <h3 className="text-lg font-bold">Profile Strength</h3>
                        <p className="text-[10px] text-slate-400 font-bold uppercase tracking-widest">Global Ranking</p>
                    </div>
                </div>

                {/* Progress Bar */}
                <div className="mb-6">
                    <div className="flex justify-between items-end mb-2">
                        <span className={cn(
                            "text-xs font-bold uppercase tracking-wider",
                            score < 40 ? "text-amber-400" : score < 80 ? "text-blue-400" : "text-emerald-400"
                        )}>
                            {score < 40 ? 'Beginner' : score < 80 ? 'Intermediate' : 'All-Star'}
                        </span>
                        <span className="text-xl font-black">{score}%</span>
                    </div>
                    <div className="h-2 w-full bg-white/10 rounded-full overflow-hidden">
                        <div 
                            className={cn(
                                "h-full transition-all duration-1000",
                                score < 40 ? "bg-amber-400 shadow-[0_0_10px_rgba(251,191,36,0.5)]" : 
                                score < 80 ? "bg-blue-400 shadow-[0_0_10px_rgba(96,165,250,0.5)]" : 
                                "bg-emerald-400 shadow-[0_0_10px_rgba(52,211,153,0.5)]"
                            )}
                            style={{ width: `${score}%` }} 
                        />
                    </div>
                </div>

                {nextStep && (
                    <div className="p-4 rounded-2xl bg-white/5 border border-white/10 group cursor-pointer hover:bg-white/10 transition-colors">
                        <div className="flex items-start gap-3">
                            <Circle className="w-5 h-5 text-blue-400 mt-0.5 shrink-0" />
                            <div className="flex-1">
                                <p className="text-sm font-bold text-slate-100 mb-1">Boost your visibility</p>
                                <p className="text-xs text-slate-400 leading-relaxed font-medium">{nextStep.label}</p>
                                <div className="mt-3 flex items-center gap-1 text-[10px] font-bold text-blue-400 uppercase tracking-widest transition-transform group-hover:translate-x-1">
                                    Take action <ArrowRight className="w-3 h-3" />
                                </div>
                            </div>
                        </div>
                    </div>
                )}
                
                {score === 100 && (
                    <div className="flex items-center gap-3 p-4 rounded-2xl bg-emerald-500/10 border border-emerald-500/20 text-emerald-400">
                        <CheckCircle2 className="w-5 h-5" />
                        <span className="text-xs font-bold uppercase tracking-wider">Perfect profile</span>
                    </div>
                )}
            </CardContent>
        </Card>
    );
};

export default ProfileCompletion;
