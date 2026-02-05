import type { AlumniListDto } from '../../types/alumni';
import { Link } from 'react-router-dom';
import { MapPin, Award } from 'lucide-react';
import { alumniService } from '../../services/alumniService';
import { Card, CardContent } from '../ui/Card';
import { Button } from '../ui/Button';
import { cn } from '../../utils/cn';

interface AlumniCardProps {
    alumni: AlumniListDto;
}

const AlumniCard = ({ alumni }: AlumniCardProps) => {
    return (
        <Card variant="default" className="group overflow-hidden hover:border-[var(--color-accent)]/30 hover:shadow-xl transition-all duration-500 h-full border-[var(--color-border)]">
            <CardContent className="p-6 flex flex-col items-center text-center h-full">
                {/* Photo with Glow Effect */}
                <div className="relative mb-5 group-hover:scale-105 transition-transform duration-500">
                    <div className={cn(
                        "absolute -inset-2 bg-gradient-to-r rounded-full blur-md opacity-10 group-hover:opacity-30 transition-opacity duration-500",
                        alumni.isVip ? "from-accent via-emerald-400 to-emerald-600" : "from-blue-500 via-indigo-500 to-purple-500"
                    )}></div>
                    <div className="w-24 h-24 rounded-full overflow-hidden border-2 border-white relative z-10 bg-slate-50 shadow-lg">
                        <img
                            src={alumniService.getPhotoUrl(alumni.photoUrl) || `https://ui-avatars.com/api/?name=${alumni.name}&background=f1f5f9&color=2D96D7`}
                            alt={alumni.name}
                            className="w-full h-full object-cover"
                        />
                    </div>
                    {alumni.isVip && (
                        <div className="absolute -bottom-1 left-1.2 -translate-x-1/2 bg-[var(--color-accent)] text-white text-[10px] uppercase font-bold px-2 py-0.5 rounded-full shadow-lg z-20 border border-white whitespace-nowrap tracking-wider">
                            VIP
                        </div>
                    )}
                </div>

                {/* Info */}
                <h3 className="text-lg font-bold text-[var(--color-text-primary)] mb-1 group-hover:text-[var(--color-accent)] transition-colors font-heading">
                    {alumni.name}
                </h3>
                <p className="text-sm text-[var(--color-text-secondary)] mb-4 line-clamp-1 h-5">
                    {alumni.major}
                </p>

                {/* Divider */}
                <div className="w-full h-px bg-[var(--color-border)] my-4" />

                {/* Meta Details */}
                <div className="w-full space-y-3 flex-1">
                    <div className="flex items-center justify-between text-xs text-[var(--color-text-secondary)]">
                        <span className="flex items-center gap-1.5 font-medium"><Award className="w-3.5 h-3.5 text-[var(--color-accent)]" /> Class</span>
                        <span className="text-[var(--color-text-primary)] font-bold">{alumni.graduationYear}</span>
                    </div>
                    <div className="flex items-center justify-between text-xs text-[var(--color-text-secondary)]">
                        <span className="flex items-center gap-1.5 font-medium"><MapPin className="w-3.5 h-3.5 text-blue-500" /> College</span>
                        <span className="text-[var(--color-text-primary)] font-bold truncate max-w-[120px]" title={alumni.college}>{alumni.college}</span>
                    </div>
                </div>

                <Link to={`/portal/directory/${alumni.id}`} className="w-full mt-6">
                    <Button variant="outline" size="sm" className="w-full text-xs hover:bg-[var(--color-accent)] hover:text-white transition-all">
                        View Profile
                    </Button>
                </Link>
            </CardContent>
        </Card>
    );
};

export default AlumniCard;
