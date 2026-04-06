import React from 'react';
import { Card, CardContent } from '../ui/Card';
import { Star, Plus, ExternalLink, GraduationCap, Briefcase, Award } from 'lucide-react';
import { Button } from '../ui/Button';

const ProfileFeatured: React.FC = () => {
    // Mock data for featured items (since it's not in the DTO yet)
    // In a real impl, this would come from profile.featuredItems
    const featuredItems = [
        {
            id: 1,
            title: 'Outstanding Alumni Award 2024',
            category: 'Achievement',
            icon: <Award className="w-5 h-5" />,
            image: 'https://images.unsplash.com/photo-1579546678181-0022d9b9ef9c?q=80&w=2070&auto=format&fit=crop',
            description: 'Recognized for significant contributions to the field of software engineering and mentorship.'
        },
        {
            id: 2,
            title: 'Modern Architecture Patterns',
            category: 'Project',
            icon: <Briefcase className="w-5 h-5" />,
            image: 'https://images.unsplash.com/photo-1460925895917-afdab827c52f?q=80&w=2015&auto=format&fit=crop',
            description: 'A deep dive into microservices and event-driven architecture styles.'
        }
    ];

    return (
        <Card className="overflow-hidden border-slate-200 border shadow-sm">
            <CardContent className="p-8">
                <div className="flex items-center justify-between mb-8">
                    <h3 className="text-2xl font-heading font-black text-slate-900 tracking-tight flex items-center gap-3">
                        <div className="p-2 bg-amber-50 rounded-xl text-amber-500">
                            <Star className="w-6 h-6 fill-current" />
                        </div>
                        Featured
                    </h3>
                    <Button variant="outline" size="sm" className="rounded-xl border-slate-200 text-slate-600 gap-2 font-bold uppercase tracking-widest text-[10px]">
                        <Plus className="w-4 h-4" /> Manage
                    </Button>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    {featuredItems.map((item) => (
                        <div key={item.id} className="group relative rounded-3xl overflow-hidden border border-slate-100 shadow-sm hover:shadow-xl transition-all duration-500 bg-white">
                            <div className="h-40 overflow-hidden relative">
                                <img src={item.image} alt={item.title} className="w-full h-full object-cover group-hover:scale-110 transition-transform duration-700" />
                                <div className="absolute inset-0 bg-gradient-to-t from-slate-950/80 via-transparent to-transparent opacity-60 group-hover:opacity-80 transition-opacity" />
                                <div className="absolute top-4 left-4 p-2 bg-white/20 backdrop-blur-md rounded-xl text-white border border-white/20">
                                    {item.icon}
                                </div>
                            </div>

                            <div className="p-6">
                                <div className="flex items-center gap-2 text-[10px] font-black uppercase tracking-[0.2em] text-blue-600 mb-2">
                                    <span className="w-2 h-2 rounded-full bg-blue-600" />
                                    {item.category}
                                </div>
                                <h4 className="text-lg font-bold text-slate-900 mb-2 group-hover:text-blue-600 transition-colors">
                                    {item.title}
                                </h4>
                                <p className="text-sm text-slate-500 line-clamp-2 leading-relaxed">
                                    {item.description}
                                </p>
                                <div className="mt-4 flex items-center gap-1 text-[10px] font-bold text-slate-900 uppercase tracking-widest opacity-0 group-hover:opacity-100 transition-all group-hover:translate-x-1">
                                    View detail <ExternalLink className="w-3 h-3" />
                                </div>
                            </div>
                        </div>
                    ))}
                    
                    {/* Add Placeholder for new item */}
                    <div className="flex flex-col items-center justify-center p-8 rounded-3xl bg-slate-50 border border-dashed border-slate-200 group cursor-pointer hover:bg-slate-100/50 transition-colors border-2 h-[340px]">
                        <div className="w-16 h-16 rounded-full bg-white shadow-sm flex items-center justify-center text-slate-300 group-hover:scale-110 group-hover:text-blue-500 transition-all">
                            <Plus className="w-8 h-8" />
                        </div>
                        <p className="mt-4 font-bold text-slate-400 group-hover:text-slate-600">Feature an achievement</p>
                    </div>
                </div>
            </CardContent>
        </Card>
    );
};

export default ProfileFeatured;
