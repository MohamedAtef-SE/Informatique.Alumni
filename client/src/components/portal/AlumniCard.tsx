import type { AlumniListDto } from '../../types/alumni';
import { Link } from 'react-router-dom';
import { Award, GraduationCap } from 'lucide-react';
import { alumniService } from '../../services/alumniService';
import { Card, CardContent } from '../ui/Card';
import { Button } from '../ui/Button';
import { cn } from '../../utils/cn';

interface AlumniCardProps {
    alumni: AlumniListDto;
}

const AlumniCard = ({ alumni }: AlumniCardProps) => {
    return (
        <Card variant="glass" className="group overflow-hidden hover:border-accent/40 transition-all duration-500 h-full flex flex-col border-[var(--color-border)] shadow-xl hover:shadow-2xl bg-white">
            <CardContent className="p-0 flex flex-col h-full">
                {/* Visual Header / Avatar Area */}
                <div className="relative pt-12 pb-8 flex flex-col items-center">
                    {/* Background Decorative Element */}
                    <div className="absolute top-0 inset-x-0 h-24 bg-gradient-to-b from-accent/10 to-transparent group-hover:from-accent/20 transition-all duration-500" />
                    
                    {/* Photo with Glow Effect */}
                    <div className="relative group-hover:scale-105 transition-transform duration-500 z-10">
                        <div className={cn(
                            "absolute -inset-3 bg-gradient-to-tr rounded-full blur-xl opacity-20 group-hover:opacity-40 transition-opacity duration-500",
                            alumni.isVip ? "from-amber-400 via-orange-400 to-yellow-600" : "from-accent via-blue-500 to-indigo-600"
                        )}></div>
                        <div className="w-28 h-28 rounded-full overflow-hidden border-4 border-white dark:border-slate-900 relative shadow-2xl bg-slate-50">
                            <img
                                src={alumniService.getPhotoUrl(alumni.photoUrl) || `https://ui-avatars.com/api/?name=${alumni.name}&background=f1f5f9&color=2D96D7&size=128`}
                                alt={alumni.name}
                                className="w-full h-full object-cover"
                            />
                        </div>
                        {alumni.isVip && (
                            <div className="absolute top-1 right-1 bg-amber-400 text-white rounded-full p-1 shadow-lg border border-white z-20">
                                <Award className="w-4 h-4" />
                            </div>
                        )}
                    </div>

                    <div className="mt-4 text-center px-4">
                        <h3 className="text-xl font-bold text-[var(--color-text-primary)] mb-1 group-hover:text-accent transition-colors font-heading tracking-tight">
                            {alumni.name}
                        </h3>
                        <div className="flex items-center justify-center gap-2 text-accent/80 font-medium text-sm">
                            <GraduationCap className="w-4 h-4" />
                            <span className="truncate max-w-[200px]">{alumni.major || 'Graduate'}</span>
                        </div>
                    </div>
                </div>

                {/* Info Grid */}
                <div className="px-6 pb-6 flex-1 flex flex-col justify-between">
                    <div className="grid grid-cols-2 gap-3 py-4 border-y border-[var(--color-border)] opacity-80">
                        <div className="flex flex-col gap-1">
                            <span className="text-[10px] uppercase tracking-widest text-[var(--color-text-secondary)] font-bold">Class of</span>
                            <span className="text-sm font-bold text-[var(--color-text-primary)]">{alumni.graduationYear || '—'}</span>
                        </div>
                        <div className="flex flex-col gap-1 border-l border-[var(--color-border)] pl-4">
                            <span className="text-[10px] uppercase tracking-widest text-[var(--color-text-secondary)] font-bold">College</span>
                            <span className="text-sm font-bold text-[var(--color-text-primary)] truncate" title={alumni.college}>{alumni.college || '—'}</span>
                        </div>
                    </div>

                    <Link to={`/portal/directory/${alumni.userId}`} className="w-full mt-6">
                        <Button variant="outline" className="w-full group/btn relative overflow-hidden h-11 bg-[var(--color-secondary)] hover:bg-accent text-[var(--color-accent)] group-hover:text-white border-[var(--color-border)] hover:border-accent transition-all duration-300">
                            <span className="relative z-10 font-bold uppercase tracking-wider text-xs">View Profile</span>
                            <div className="absolute inset-0 bg-gradient-to-r from-accent to-blue-600 opacity-0 group-hover:opacity-100 transition-opacity duration-300" />
                        </Button>
                    </Link>
                </div>
            </CardContent>
        </Card>
    );
};

export default AlumniCard;
