import { useState } from 'react';
import { useFieldArray, useForm, type Control } from 'react-hook-form';
import { useTranslation } from 'react-i18next';
import { Plus, Trash, Pencil, GraduationCap, Award, Code, Globe, Link as LinkIcon, Star } from 'lucide-react';
import { Button } from '../ui/Button';
import { Card, CardHeader, CardTitle, CardContent } from '../ui/Card';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '../ui/Dialog';
import { Input } from '../ui/Input';
import { zodResolver } from '@hookform/resolvers/zod';
import { 
    cvEducationSchema, type EducationFormData, 
    cvSkillSchema, type SkillFormData,
    cvLanguageSchema, type LanguageFormData,
    cvProjectSchema, type ProjectFormData,
    cvSocialLinkSchema, type SocialLinkFormData,
    cvCertificationSchema, type CertificationFormData,
    type CvFormData
} from '../../schemas/cvSchema';

interface SectionProps {
    control: Control<CvFormData>;
}

// --- Education Section ---
export const EducationSection = ({ control }: SectionProps) => {
    const { t } = useTranslation();
    const { fields, append, remove, update } = useFieldArray({ control, name: "educations" });
    const [isDialogOpen, setIsDialogOpen] = useState(false);
    const [editingIndex, setEditingIndex] = useState<number | null>(null);
    const { register, handleSubmit, reset, formState: { errors } } = useForm<EducationFormData>({ resolver: zodResolver(cvEducationSchema) });

    const openAdd = () => { setEditingIndex(null); reset({ institution: '', degree: '', startDate: '', endDate: '' }); setIsDialogOpen(true); };
    const openEdit = (index: number, data: EducationFormData) => { setEditingIndex(index); reset(data); setIsDialogOpen(true); };
    const onSave = (data: EducationFormData) => { if (editingIndex !== null) update(editingIndex, data); else append(data); setIsDialogOpen(false); };

    return (
        <Card>
            <CardHeader className="flex flex-row items-center justify-between">
                <CardTitle className="flex items-center gap-2"><GraduationCap className="w-5 h-5 text-indigo-500" /> {t('career.cv.education', 'Education')}</CardTitle>
                <Button size="sm" variant="ghost" onClick={openAdd}><Plus className="w-4 h-4 mr-1" /> {t('common.add')}</Button>
            </CardHeader>
            <CardContent className="space-y-3">
                {fields.map((field: any, index) => (
                    <div key={field.id} className="p-3 border rounded-lg bg-slate-50/50 flex justify-between items-center group relative">
                        <div>
                            <div className="font-bold text-slate-900">{field.degree}</div>
                            <div className="text-sm text-slate-500">{field.institution}</div>
                            <div className="text-xs text-slate-400 font-mono mt-1">{field.startDate} - {field.endDate || t('common.present')}</div>
                        </div>
                        <div className="flex gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                            <Button size="icon" variant="ghost" className="h-8 w-8 text-blue-500" onClick={() => openEdit(index, field)}><Pencil className="w-3.5 h-3.5" /></Button>
                            <Button size="icon" variant="ghost" className="h-8 w-8 text-red-500" onClick={() => remove(index)}><Trash className="w-3.5 h-3.5" /></Button>
                        </div>
                    </div>
                ))}
            </CardContent>
            <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
                <DialogContent>
                    <DialogHeader><DialogTitle>{editingIndex !== null ? t('common.edit') : t('common.add')} {t('career.cv.education')}</DialogTitle></DialogHeader>
                    <div className="space-y-4 py-2">
                        <Input label={t('career.cv.institution')} error={errors.institution?.message} {...register('institution')} />
                        <Input label={t('career.cv.degree')} error={errors.degree?.message} {...register('degree')} />
                        <div className="grid grid-cols-2 gap-4">
                            <Input type="date" label={t('career.cv.start_date')} error={errors.startDate?.message} {...register('startDate')} />
                            <Input type="date" label={t('career.cv.end_date')} error={errors.endDate?.message} {...register('endDate')} />
                        </div>
                    </div>
                    <DialogFooter>
                        <Button variant="ghost" onClick={() => setIsDialogOpen(false)}>{t('common.cancel')}</Button>
                        <Button onClick={handleSubmit(onSave)}>{t('common.save')}</Button>
                    </DialogFooter>
                </DialogContent>
            </Dialog>
        </Card>
    );
};

// --- Skills Section ---
export const SkillsSection = ({ control }: SectionProps) => {
    const { t } = useTranslation();
    const { fields, append, remove } = useFieldArray({ control, name: "skills" });
    const [isDialogOpen, setIsDialogOpen] = useState(false);
    const { register, handleSubmit, reset, formState: { errors } } = useForm<SkillFormData>({ resolver: zodResolver(cvSkillSchema) });

    const onAdd = (data: SkillFormData) => { append(data); setIsDialogOpen(false); reset(); };

    return (
        <Card>
            <CardHeader className="flex flex-row items-center justify-between">
                <CardTitle className="flex items-center gap-2"><Star className="w-5 h-5 text-amber-500" /> {t('career.cv.skills', 'Professional Skills')}</CardTitle>
                <Button size="sm" variant="ghost" onClick={() => setIsDialogOpen(true)}><Plus className="w-4 h-4 mr-1" /> {t('common.add')}</Button>
            </CardHeader>
            <CardContent className="flex flex-wrap gap-2">
                {fields.map((field: any, index) => (
                    <div key={field.id} className="inline-flex items-center gap-1.5 px-3 py-1.5 bg-slate-100 dark:bg-slate-800 rounded-full text-sm font-medium group">
                        <span>{field.name}</span>
                        {field.proficiencyLevel && <span className="text-[10px] text-slate-400 capitalize">({field.proficiencyLevel})</span>}
                        <button onClick={() => remove(index)} className="ml-1 text-slate-400 hover:text-red-500 transition-colors">
                            <Plus className="w-3.5 h-3.5 rotate-45" />
                        </button>
                    </div>
                ))}
            </CardContent>
            <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
                <DialogContent>
                    <DialogHeader><DialogTitle>{t('common.add')} {t('career.cv.skill')}</DialogTitle></DialogHeader>
                    <div className="space-y-4 py-2">
                        <Input label={t('career.cv.skill_name')} error={errors.name?.message} {...register('name')} />
                        <div className="space-y-1">
                            <label className="text-sm font-medium">Proficiency Level</label>
                            <select {...register('proficiencyLevel')} className="w-full h-10 px-3 bg-white border rounded-md text-sm outline-none focus:ring-2 focus:ring-accent/20">
                                <option value="Beginner">Beginner</option>
                                <option value="Intermediate">Intermediate</option>
                                <option value="Advanced">Advanced</option>
                                <option value="Expert">Expert</option>
                                <option value="Master">Master</option>
                            </select>
                        </div>
                    </div>
                    <DialogFooter>
                        <Button variant="ghost" onClick={() => setIsDialogOpen(false)}>{t('common.cancel')}</Button>
                        <Button onClick={handleSubmit(onAdd)}>{t('common.add')}</Button>
                    </DialogFooter>
                </DialogContent>
            </Dialog>
        </Card>
    );
};

// --- Languages Section ---
export const LanguagesSection = ({ control }: SectionProps) => {
    const { t } = useTranslation();
    const { fields, append, remove } = useFieldArray({ control, name: "languages" });
    const [isDialogOpen, setIsDialogOpen] = useState(false);
    const { register, handleSubmit, reset, formState: { errors } } = useForm<LanguageFormData>({ resolver: zodResolver(cvLanguageSchema) });

    const onAdd = (data: LanguageFormData) => { append(data); setIsDialogOpen(false); reset(); };

    return (
        <Card>
            <CardHeader className="flex flex-row items-center justify-between">
                <CardTitle className="flex items-center gap-2"><Globe className="w-5 h-5 text-emerald-500" /> {t('career.cv.languages', 'Languages')}</CardTitle>
                <Button size="sm" variant="ghost" onClick={() => setIsDialogOpen(true)}><Plus className="w-4 h-4 mr-1" /> {t('common.add')}</Button>
            </CardHeader>
            <CardContent className="space-y-2">
                {fields.map((field: any, index) => (
                    <div key={field.id} className="flex justify-between items-center p-2 px-3 bg-slate-50/50 rounded-lg">
                        <div>
                            <span className="font-bold text-slate-800">{field.name}</span>
                            <span className="ml-2 text-xs text-slate-500 italic opacity-75">{field.fluencyLevel}</span>
                        </div>
                        <Button size="icon" variant="ghost" className="h-7 w-7 text-red-400" onClick={() => remove(index)}><Trash className="w-3.5 h-3.5" /></Button>
                    </div>
                ))}
            </CardContent>
            <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
                <DialogContent>
                    <DialogHeader><DialogTitle>{t('common.add')} {t('career.cv.language')}</DialogTitle></DialogHeader>
                    <div className="space-y-4 py-2">
                        <Input label={t('career.cv.language_name')} error={errors.name?.message} {...register('name')} />
                        <div className="space-y-1">
                            <label className="text-sm font-medium">Fluency Level</label>
                            <select {...register('fluencyLevel')} className="w-full h-10 px-3 bg-white border rounded-md text-sm outline-none focus:ring-2 focus:ring-accent/20">
                                <option value="Elementary">Elementary</option>
                                <option value="Limited Working">Limited Working</option>
                                <option value="Professional Working">Professional Working</option>
                                <option value="Full Professional">Full Professional</option>
                                <option value="Native / Bilingual">Native / Bilingual</option>
                            </select>
                        </div>
                    </div>
                    <DialogFooter>
                        <Button variant="ghost" onClick={() => setIsDialogOpen(false)}>{t('common.cancel')}</Button>
                        <Button onClick={handleSubmit(onAdd)}>{t('common.add')}</Button>
                    </DialogFooter>
                </DialogContent>
            </Dialog>
        </Card>
    );
};

// --- Social Links ---
export const SocialLinksSection = ({ control }: SectionProps) => {
    const { t } = useTranslation();
    const { fields, append, remove } = useFieldArray({ control, name: "socialLinks" });
    const [isDialogOpen, setIsDialogOpen] = useState(false);
    const { register, handleSubmit, reset, formState: { errors } } = useForm<SocialLinkFormData>({ resolver: zodResolver(cvSocialLinkSchema) });

    const onAdd = (data: SocialLinkFormData) => { append(data); setIsDialogOpen(false); reset(); };

    return (
        <Card>
            <CardHeader className="flex flex-row items-center justify-between">
                <CardTitle className="flex items-center gap-2"><LinkIcon className="w-5 h-5 text-blue-500" /> {t('career.cv.social_links', 'Social Links')}</CardTitle>
                <Button size="sm" variant="ghost" onClick={() => setIsDialogOpen(true)}><Plus className="w-4 h-4 mr-1" /> {t('common.add')}</Button>
            </CardHeader>
            <CardContent className="space-y-2">
                {fields.map((field: any, index) => (
                    <div key={field.id} className="flex justify-between items-center p-2 px-3 bg-slate-50/50 rounded-lg group">
                        <div className="flex items-center gap-2">
                            <div className="text-xs font-bold uppercase tracking-wider text-slate-400 min-w-[70px]">{field.platform}</div>
                            <span className="text-sm text-blue-600 dark:text-blue-400 font-medium truncate max-w-[200px]">{field.url}</span>
                        </div>
                        <Button size="icon" variant="ghost" className="h-7 w-7 text-red-400" onClick={() => remove(index)}><Trash className="w-3.5 h-3.5" /></Button>
                    </div>
                ))}
            </CardContent>
            <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
                <DialogContent>
                    <DialogHeader><DialogTitle>{t('common.add')} Social Link</DialogTitle></DialogHeader>
                    <div className="space-y-4 py-2">
                        <Input label="Platform (LinkedIn, GitHub, etc.)" error={errors.platform?.message} {...register('platform')} />
                        <Input label="Profile URL" placeholder="https://..." error={errors.url?.message} {...register('url')} />
                    </div>
                    <DialogFooter>
                        <Button variant="ghost" onClick={() => setIsDialogOpen(false)}>{t('common.cancel')}</Button>
                        <Button onClick={handleSubmit(onAdd)}>{t('common.add')}</Button>
                    </DialogFooter>
                </DialogContent>
            </Dialog>
        </Card>
    );
};

// --- Projects Section ---
export const ProjectsSection = ({ control }: SectionProps) => {
    const { t } = useTranslation();
    const { fields, append, remove, update } = useFieldArray({ control, name: "projects" });
    const [isDialogOpen, setIsDialogOpen] = useState(false);
    const [editingIndex, setEditingIndex] = useState<number | null>(null);
    const { register, handleSubmit, reset, formState: { errors } } = useForm<ProjectFormData>({ resolver: zodResolver(cvProjectSchema) });

    const openAdd = () => { setEditingIndex(null); reset({ name: '', description: '', link: '' }); setIsDialogOpen(true); };
    const openEdit = (index: number, data: ProjectFormData) => { setEditingIndex(index); reset(data); setIsDialogOpen(true); };
    const onSave = (data: ProjectFormData) => { if (editingIndex !== null) update(editingIndex, data); else append(data); setIsDialogOpen(false); };

    return (
        <Card>
            <CardHeader className="flex flex-row items-center justify-between">
                <CardTitle className="flex items-center gap-2"><Code className="w-5 h-5 text-blue-600" /> {t('career.cv.projects', 'Projects')}</CardTitle>
                <Button size="sm" variant="ghost" onClick={openAdd}><Plus className="w-4 h-4 mr-1" /> {t('common.add')}</Button>
            </CardHeader>
            <CardContent className="space-y-3">
                {fields.map((field: any, index) => (
                    <div key={field.id} className="p-3 border rounded-lg bg-slate-50/50 group relative">
                        <div className="absolute top-2 right-2 flex gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                            <Button size="icon" variant="ghost" className="h-7 w-7 text-blue-500" onClick={() => openEdit(index, field)}><Pencil className="w-3.5 h-3.5" /></Button>
                            <Button size="icon" variant="ghost" className="h-7 w-7 text-red-500" onClick={() => remove(index)}><Trash className="w-3.5 h-3.5" /></Button>
                        </div>
                        <div className="font-bold text-slate-900">{field.name}</div>
                        {field.description && <p className="text-xs text-slate-500 mt-1 line-clamp-2">{field.description}</p>}
                        {field.link && <a href={field.link} target="_blank" rel="noreferrer" className="text-[10px] text-blue-500 hover:underline mt-1 inline-flex items-center gap-1"><LinkIcon className="w-3 h-3" /> External Link</a>}
                    </div>
                ))}
            </CardContent>
            <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
                <DialogContent>
                    <DialogHeader><DialogTitle>{editingIndex !== null ? t('common.edit') : t('common.add')} Project</DialogTitle></DialogHeader>
                    <div className="space-y-4 py-2">
                        <Input label="Project Name" error={errors.name?.message} {...register('name')} />
                        <div className="space-y-1">
                            <label className="text-sm font-medium">Description</label>
                            <textarea {...register('description')} className="w-full border rounded-md p-2 text-sm" rows={2} />
                        </div>
                        <Input label="Project Link (URL)" error={errors.link?.message} {...register('link')} />
                    </div>
                    <DialogFooter>
                        <Button variant="ghost" onClick={() => setIsDialogOpen(false)}>{t('common.cancel')}</Button>
                        <Button onClick={handleSubmit(onSave)}>{t('common.save')}</Button>
                    </DialogFooter>
                </DialogContent>
            </Dialog>
        </Card>
    );
};

// --- Certifications Section ---
export const CertificationsSection = ({ control }: SectionProps) => {
    const { t } = useTranslation();
    const { fields, append, remove, update } = useFieldArray({ control, name: "certifications" });
    const [isDialogOpen, setIsDialogOpen] = useState(false);
    const [editingIndex, setEditingIndex] = useState<number | null>(null);
    const { register, handleSubmit, reset, formState: { errors } } = useForm<CertificationFormData>({ resolver: zodResolver(cvCertificationSchema) });

    const openAdd = () => { setEditingIndex(null); reset({ name: '', issuer: '', date: '' }); setIsDialogOpen(true); };
    const openEdit = (index: number, data: CertificationFormData) => { setEditingIndex(index); reset(data); setIsDialogOpen(true); };
    const onSave = (data: CertificationFormData) => { if (editingIndex !== null) update(editingIndex, data); else append(data); setIsDialogOpen(false); };

    return (
        <Card>
            <CardHeader className="flex flex-row items-center justify-between">
                <CardTitle className="flex items-center gap-2"><Award className="w-5 h-5 text-amber-600" /> {t('career.cv.certifications', 'Certifications')}</CardTitle>
                <Button size="sm" variant="ghost" onClick={openAdd}><Plus className="w-4 h-4 mr-1" /> {t('common.add')}</Button>
            </CardHeader>
            <CardContent className="space-y-3">
                {fields.map((field: any, index) => (
                    <div key={field.id} className="flex justify-between items-center p-3 border rounded-lg bg-slate-50/50 group relative">
                        <div>
                            <div className="font-bold text-slate-900">{field.name}</div>
                            <div className="text-xs text-slate-500">{field.issuer} {field.date && `• ${field.date}`}</div>
                        </div>
                        <div className="flex gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                            <Button size="icon" variant="ghost" className="h-7 w-7 text-blue-500" onClick={() => openEdit(index, field)}><Pencil className="w-3.5 h-3.5" /></Button>
                            <Button size="icon" variant="ghost" className="h-7 w-7 text-red-500" onClick={() => remove(index)}><Trash className="w-3.5 h-3.5" /></Button>
                        </div>
                    </div>
                ))}
            </CardContent>
            <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
                <DialogContent>
                    <DialogHeader><DialogTitle>{editingIndex !== null ? t('common.edit') : t('common.add')} Certification</DialogTitle></DialogHeader>
                    <div className="space-y-4 py-2">
                        <Input label="Certification Name" error={errors.name?.message} {...register('name')} />
                        <Input label="Issuing Organization" error={errors.issuer?.message} {...register('issuer')} />
                        <Input type="date" label="Date Obtained" error={errors.date?.message} {...register('date')} />
                    </div>
                    <DialogFooter>
                        <Button variant="ghost" onClick={() => setIsDialogOpen(false)}>{t('common.cancel')}</Button>
                        <Button onClick={handleSubmit(onSave)}>{t('common.save')}</Button>
                    </DialogFooter>
                </DialogContent>
            </Dialog>
        </Card>
    );
};
