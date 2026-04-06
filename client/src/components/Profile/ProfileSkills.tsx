import React from 'react';
import { Card, CardContent } from '../ui/Card';
import { Lightbulb, Plus, Trophy, Brain } from 'lucide-react';
import { Button } from '../ui/Button';
import { Badge } from '../ui/Badge';

const ProfileSkills: React.FC = () => {
    // Mock data for skills
    const skillCategories = [
        {
            name: 'Industry Knowledge',
            skills: ['Software Engineering', 'Project Management', 'Agile Methodology', 'Business Analysis']
        },
        {
            name: 'Tools & Technologies',
            skills: ['React', 'TypeScript', 'Tailwind CSS', 'Node.js', 'PostgreSQL']
        },
        {
            name: 'Soft Skills',
            skills: ['Leadership', 'Public Speaking', 'Mentoring']
        }
    ];

    return (
        <Card className="overflow-hidden border-slate-200 border shadow-sm mt-8">
            <CardContent className="p-8">
                <div className="flex items-center justify-between mb-8">
                    <h3 className="text-2xl font-heading font-black text-slate-900 tracking-tight flex items-center gap-3">
                        <div className="p-2 bg-blue-50 rounded-xl text-blue-600">
                            <Brain className="w-6 h-6" />
                        </div>
                        Skills & Expertise
                    </h3>
                    <Button variant="outline" size="sm" className="rounded-xl border-slate-200 text-slate-600 gap-2 font-bold uppercase tracking-widest text-[10px]">
                        <Plus className="w-4 h-4" /> Add Skill
                    </Button>
                </div>

                <div className="space-y-8">
                    {skillCategories.map((category, idx) => (
                        <div key={idx} className="space-y-4">
                            <h4 className="text-xs font-black uppercase tracking-[0.2em] text-slate-400 flex items-center gap-2">
                                <span className="w-1 h-1 bg-slate-400 rounded-full" />
                                {category.name}
                            </h4>
                            <div className="flex flex-wrap gap-3">
                                {category.skills.map((skill, sIdx) => (
                                    <div key={sIdx} className="group cursor-pointer">
                                        <Badge 
                                            variant="secondary" 
                                            className="h-10 px-6 rounded-2xl bg-slate-50 border border-slate-100 text-slate-700 hover:bg-blue-600 hover:text-white hover:border-blue-600 transition-all duration-300 font-bold"
                                        >
                                            {skill}
                                        </Badge>
                                    </div>
                                ))}
                            </div>
                        </div>
                    ))}
                </div>

                <div className="mt-10 pt-8 border-t border-slate-100 flex items-center justify-between text-blue-600 hover:text-blue-700 cursor-pointer group">
                    <span className="text-xs font-bold uppercase tracking-widest">Show all 24 skills</span>
                    <Trophy className="w-4 h-4 group-hover:scale-110 transition-transform" />
                </div>
            </CardContent>
        </Card>
    );
};

export default ProfileSkills;
