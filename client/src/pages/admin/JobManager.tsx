import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { Plus, Eye, CheckCircle2, XCircle, Briefcase, Users, Calendar, GraduationCap, Award, FileText, Globe, ExternalLink, Folder, Link2 } from 'lucide-react';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/Table';
import { Button } from '../../components/ui/Button';
import { toast } from 'sonner';
import { PageHeader } from '../../components/admin/PageHeader';
import { DataTableShell } from '../../components/admin/DataTableShell';
import { StatusBadge } from '../../components/admin/StatusBadge';
import { CreateJobModal } from '../../components/admin/career';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter, DialogDescription } from '../../components/ui/Dialog';
import { Card, CardHeader, CardTitle, CardContent } from '../../components/ui/Card';
import { Badge } from '../../components/ui/Badge';
import type { JobAdminDto, JobApplicationAdminDto } from '../../types/admin';

// --- Alumni Profile Modal ---
const AlumniProfileModal = ({ alumniId, fullName, isOpen, onClose }: { alumniId: string, fullName: string, isOpen: boolean, onClose: () => void }) => {
    const { data: cv, isLoading } = useQuery({
        queryKey: ['alumni-cv', alumniId],
        queryFn: () => adminService.getAlumniCv(alumniId),
        enabled: isOpen && !!alumniId
    });

    return (
        <Dialog open={isOpen} onOpenChange={(open) => !open && onClose()}>
            <DialogContent className="max-w-4xl max-h-[90vh] overflow-hidden flex flex-col p-0 gap-0 border-none shadow-2xl">
                <DialogHeader className="p-6 bg-slate-900 text-white shrink-0">
                    <div className="flex items-center gap-4">
                        <div className="w-16 h-16 rounded-full bg-accent/20 flex items-center justify-center text-accent text-2xl font-bold border-2 border-accent/30">
                            {fullName.charAt(0)}
                        </div>
                        <div>
                            <DialogTitle className="text-2xl font-bold tracking-tight text-accent transition-colors">{fullName}</DialogTitle>
                            <DialogDescription className="text-slate-400 mt-1 flex items-center gap-2">
                                <FileText className="w-4 h-4" />
                                Professional Curriculum Vitae
                            </DialogDescription>
                        </div>
                    </div>
                </DialogHeader>

                <div className="flex-1 overflow-auto bg-slate-50 dark:bg-slate-950/50 p-6 space-y-6">
                    {isLoading ? (
                        <div className="flex flex-col items-center justify-center py-20 gap-4">
                            <div className="w-10 h-10 border-4 border-accent border-t-transparent rounded-full animate-spin"></div>
                            <p className="text-slate-500 font-medium font-bold">Extracting professional profile...</p>
                        </div>
                    ) : cv ? (
                        <>
                            {/* Summary Section */}
                            {cv.summary && (
                                <Card className="border-none shadow-sm overflow-hidden">
                                    <CardHeader className="bg-white dark:bg-slate-900 pb-3">
                                        <CardTitle className="text-xs font-semibold flex items-center gap-2 text-slate-500 uppercase tracking-widest font-bold">
                                            <FileText className="w-4 h-4" /> Professional Summary
                                        </CardTitle>
                                    </CardHeader>
                                    <CardContent className="bg-white dark:bg-slate-900 pt-0 text-slate-700 dark:text-slate-300 leading-relaxed italic">
                                        "{cv.summary}"
                                    </CardContent>
                                </Card>
                            )}

                            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                                {/* Experience & Education Main Column */}
                                <div className="md:col-span-2 space-y-6">
                                    {/* Work Experience */}
                                    <section className="space-y-4">
                                        <h3 className="text-lg font-bold flex items-center gap-2 px-1">
                                            <Briefcase className="w-5 h-5 text-accent" /> Work Experience
                                        </h3>
                                        <div className="space-y-4">
                                            {cv.experiences.length > 0 ? cv.experiences.map((exp, i) => (
                                                <div key={i} className="relative pl-6 pb-2 before:absolute before:left-0 before:top-2 before:bottom-0 before:w-px before:bg-slate-200 dark:before:bg-slate-800">
                                                    <div className="absolute left-[-4px] top-2 w-2 h-2 rounded-full bg-accent border-2 border-white dark:border-slate-900" />
                                                    <div className="font-bold text-slate-900 dark:text-white">{exp.position}</div>
                                                    <div className="text-accent text-sm font-semibold">{exp.company}</div>
                                                    <div className="text-xs text-slate-400 mt-1 flex items-center gap-1.5">
                                                        <Calendar className="w-3 h-3" />
                                                        {new Date(exp.startDate).toLocaleDateString()} - {exp.endDate ? new Date(exp.endDate).toLocaleDateString() : 'Present'}
                                                    </div>
                                                    {exp.description && <p className="text-sm text-slate-600 dark:text-slate-400 mt-2 leading-relaxed">{exp.description}</p>}
                                                </div>
                                            )) : <p className="text-slate-400 italic px-1">No experience details listed.</p>}
                                        </div>
                                    </section>

                                    {/* Education */}
                                    <section className="space-y-4">
                                        <h3 className="text-lg font-bold flex items-center gap-2 px-1">
                                            <GraduationCap className="w-5 h-5 text-indigo-500" /> Education
                                        </h3>
                                        <div className="space-y-4">
                                            {cv.educations.length > 0 ? cv.educations.map((edu, i) => (
                                                <div key={i} className="bg-white dark:bg-slate-900 p-4 rounded-xl shadow-sm border border-slate-100 dark:border-slate-800 flex justify-between items-start">
                                                    <div>
                                                        <div className="font-bold text-slate-900 dark:text-white uppercase tracking-tight">{edu.degree}</div>
                                                        <div className="text-slate-500 text-sm mt-1">{edu.institution}</div>
                                                    </div>
                                                    <Badge variant="secondary" className="font-mono text-[10px]">
                                                        {new Date(edu.startDate).getFullYear()} - {edu.endDate ? new Date(edu.endDate).getFullYear() : ''}
                                                    </Badge>
                                                </div>
                                            )) : <p className="text-slate-400 italic px-1">No education details listed.</p>}
                                        </div>
                                    </section>

                                    {/* Projects */}
                                    {cv.projects && cv.projects.length > 0 && (
                                        <section className="space-y-4">
                                            <h3 className="text-lg font-bold flex items-center gap-2 px-1 text-blue-600">
                                                <Folder className="w-5 h-5" /> Key Projects
                                            </h3>
                                            <div className="grid grid-cols-1 gap-4">
                                                {cv.projects.map((project, i) => (
                                                    <div key={i} className="bg-white dark:bg-slate-900 p-4 rounded-xl border border-slate-100 dark:border-slate-800 shadow-sm transition-all hover:shadow-md">
                                                        <div className="flex justify-between items-start">
                                                            <div className="font-bold text-slate-900 dark:text-white uppercase tracking-tight">{project.name}</div>
                                                            {project.link && (
                                                                <a href={project.link} target="_blank" rel="noopener noreferrer" className="text-accent hover:text-accent/80 transition-colors">
                                                                    <ExternalLink className="w-4 h-4" />
                                                                </a>
                                                            )}
                                                        </div>
                                                        {project.description && <p className="text-sm text-slate-500 mt-2 line-clamp-2">{project.description}</p>}
                                                    </div>
                                                ))}
                                            </div>
                                        </section>
                                    )}
                                </div>
                                
                                {/* Sidebar Column */}
                                <div className="space-y-8">
                                    {/* Skills */}
                                    <section className="space-y-4">
                                        <h3 className="text-lg font-bold flex items-center gap-2 px-1">
                                            <Award className="w-5 h-5 text-amber-500" /> Professional Skills
                                        </h3>
                                        <div className="bg-white dark:bg-slate-900 p-5 rounded-2xl shadow-sm border border-slate-100 dark:border-slate-800 flex flex-wrap gap-2">
                                            {cv.skills.length > 0 ? cv.skills.map((skill, i) => (
                                                <div key={i} className="group relative">
                                                    <Badge variant="outline" className="px-3 py-1 bg-slate-50 dark:bg-slate-800/50 hover:bg-accent/10 hover:text-accent hover:border-accent/30 transition-all cursor-default font-bold">
                                                        {skill.name}
                                                    </Badge>
                                                    {skill.proficiencyLevel && (
                                                        <div className="absolute bottom-full left-1/2 -translate-x-1/2 mb-2 px-2 py-1 bg-slate-900 text-[10px] text-white rounded opacity-0 group-hover:opacity-100 transition-opacity whitespace-nowrap z-10">
                                                            {skill.proficiencyLevel}
                                                        </div>
                                                    )}
                                                </div>
                                            )) : <p className="text-slate-400 italic px-1">No skills listed.</p>}
                                        </div>
                                    </section>

                                    {/* Languages */}
                                    {cv.languages && cv.languages.length > 0 && (
                                        <section className="space-y-4">
                                            <h3 className="text-lg font-bold flex items-center gap-2 px-1">
                                                <Globe className="w-5 h-5 text-indigo-500" /> Languages
                                            </h3>
                                            <div className="space-y-2">
                                                {cv.languages.map((lang, i) => (
                                                    <div key={i} className="flex justify-between items-center bg-white dark:bg-slate-900 px-4 py-2.5 rounded-xl border border-slate-100 dark:border-slate-800 shadow-sm">
                                                        <span className="font-bold text-slate-800 dark:text-slate-200">{lang.name}</span>
                                                        <span className="text-xs font-mono text-accent bg-accent/5 px-2 py-0.5 rounded uppercase">{lang.fluencyLevel || 'Native'}</span>
                                                    </div>
                                                ))}
                                            </div>
                                        </section>
                                    )}

                                    {/* Certifications */}
                                    {cv.certifications && cv.certifications.length > 0 && (
                                        <section className="space-y-4">
                                            <h3 className="text-lg font-bold flex items-center gap-2 px-1">
                                                <Award className="w-5 h-5 text-emerald-500" /> Certifications
                                            </h3>
                                            <div className="space-y-3">
                                                {cv.certifications.map((cert, i) => (
                                                    <div key={i} className="bg-white dark:bg-slate-900 p-3.5 rounded-xl border border-slate-100 dark:border-slate-800 shadow-sm">
                                                        <div className="font-bold text-slate-900 dark:text-white leading-tight">{cert.name}</div>
                                                        <div className="text-[11px] text-slate-500 mt-1 flex justify-between items-center">
                                                            <span>{cert.issuer}</span>
                                                            {cert.date && <span>{new Date(cert.date).getFullYear()}</span>}
                                                        </div>
                                                    </div>
                                                ))}
                                            </div>
                                        </section>
                                    )}

                                    {/* Social Links */}
                                    {cv.socialLinks && cv.socialLinks.length > 0 && (
                                        <section className="space-y-4">
                                            <h3 className="text-lg font-bold flex items-center gap-2 px-1">
                                                <Link2 className="w-5 h-5 text-slate-500" /> Professional Links
                                            </h3>
                                            <div className="grid grid-cols-2 gap-2">
                                                {cv.socialLinks.map((link, i) => (
                                                    <a key={i} href={link.url} target="_blank" rel="noopener noreferrer" className="flex items-center gap-2 px-3 py-2 bg-white dark:bg-slate-900 rounded-xl border border-slate-100 dark:border-slate-800 hover:border-accent hover:bg-accent/5 transition-all text-xs font-bold text-slate-700 dark:text-slate-300">
                                                        <ExternalLink className="w-3.5 h-3.5" />
                                                        <span className="truncate capitalize">{link.platform}</span>
                                                    </a>
                                                ))}
                                            </div>
                                        </section>
                                    )}
                                </div>
                            </div>
                        </>
                    ) : (
                        <div className="text-center py-20 text-slate-500">Could not retrieve CV details.</div>
                    )}
                </div>

                <DialogFooter className="p-6 bg-white dark:bg-slate-900 border-t border-slate-100 dark:border-slate-800 shrink-0">
                    <Button variant="outline" onClick={onClose} className="px-8 font-bold">Close Profile</Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
};

const JobManager = () => {
    const queryClient = useQueryClient();
    const [filter, setFilter] = useState('');
    const [page, setPage] = useState(1);
    const pageSize = 10;
    const [statusFilter, setStatusFilter] = useState<'all' | 'active' | 'closed'>('all');
    const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
    const [viewingApplications, setViewingApplications] = useState<JobAdminDto | null>(null);

    const { data, isLoading } = useQuery({
        queryKey: ['admin-jobs', filter, page, statusFilter],
        queryFn: async () => {
            const result = await adminService.getAdminJobs({
                filter,
                isActive: statusFilter === 'all' ? undefined : statusFilter === 'active',
                skipCount: (page - 1) * pageSize,
                maxResultCount: pageSize
            });
            return result;
        }
    });

    const approveMutation = useMutation({
        mutationFn: adminService.approveJob,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-jobs'] });
            toast.success('Job reopened successfully.');
        }
    });

    const rejectMutation = useMutation({
        mutationFn: adminService.rejectJob,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-jobs'] });
            toast.success('Job closed successfully.');
        }
    });

    return (
        <div className="space-y-8 animate-in fade-in duration-500">
            <PageHeader
                title="Job Board Manager"
                description="Manage job listings and view alumni applications."
                action={
                    <Button className="shadow-neon" onClick={() => setIsCreateModalOpen(true)}>
                        <Plus className="w-4 h-4 mr-2" /> Post New Job
                    </Button>
                }
            />

            {/* Status Tabs */}
            <div className="flex space-x-2 border-b border-white/10 pb-4">
                {[
                    { label: 'All Listings', value: 'all' },
                    { label: 'Active', value: 'active' },
                    { label: 'Closed', value: 'closed' }
                ].map((tab) => (
                    <Button
                        key={tab.label}
                        variant={statusFilter === tab.value ? 'default' : 'ghost'}
                        size="sm"
                        onClick={() => {
                            setStatusFilter(tab.value as any);
                            setPage(1);
                        }}
                    >
                        {tab.label}
                    </Button>
                ))}
            </div>

            <DataTableShell
                searchPlaceholder="Search jobs..."
                onSearch={setFilter}
                pagination={{
                    currentPage: page,
                    totalPages: Math.ceil((data?.totalCount || 0) / pageSize),
                    onPageChange: setPage
                }}
            >
                <Table>
                    <TableHeader>
                        <TableRow className="hover:bg-transparent border-white/5">
                            <TableHead className="w-[35%]">Job Title</TableHead>
                            <TableHead>Posted Date</TableHead>
                            <TableHead>Closing Date</TableHead>
                            <TableHead>Applications</TableHead>
                            <TableHead>Status</TableHead>
                            <TableHead className="text-right">Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {isLoading ? (
                            <TableRow>
                                <TableCell colSpan={6} className="h-32 text-center text-slate-400">Loading jobs...</TableCell>
                            </TableRow>
                        ) : data?.items.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={6} className="h-32 text-center text-slate-500">
                                    No job listings found.
                                </TableCell>
                            </TableRow>
                        ) : (
                            data?.items.map((job: JobAdminDto) => (
                                <TableRow key={job.id}>
                                    <TableCell>
                                        <div className="text-slate-900 dark:text-white font-medium flex items-center gap-2">
                                            <div className="p-1.5 rounded bg-slate-100 dark:bg-white/5 text-slate-500 dark:text-slate-400">
                                                <Briefcase className="w-4 h-4" />
                                            </div>
                                            <span>{job.title}</span>
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <div className="text-sm text-slate-500">
                                            {new Date(job.creationTime).toLocaleDateString()}
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <div className="text-sm text-slate-500">
                                            {job.closingDate ? new Date(job.closingDate).toLocaleDateString() : 'No Limit'}
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <div className="flex items-center gap-1.5 text-slate-700 dark:text-slate-300">
                                            <Users className="w-4 h-4 text-slate-400" />
                                            <span className="font-mono font-bold">{job.applicationCount}</span>
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <StatusBadge variant={job.isActive ? 'success' : 'secondary'}>
                                            {job.isActive ? 'Active' : 'Closed'}
                                        </StatusBadge>
                                    </TableCell>
                                    <TableCell className="text-right">
                                        <div className="flex justify-end gap-2">
                                            <Button
                                                size="sm"
                                                variant="outline"
                                                className="h-8"
                                                onClick={() => setViewingApplications(job)}
                                            >
                                                <Eye className="w-3.5 h-3.5 mr-1.5" /> Applications
                                            </Button>
                                            {job.isActive ? (
                                                <Button
                                                    size="icon"
                                                    variant="ghost"
                                                    className="text-red-400 hover:text-red-300 hover:bg-red-500/20 h-8 w-8"
                                                    title="Close Job"
                                                    onClick={() => rejectMutation.mutate(job.id)}
                                                    disabled={rejectMutation.isPending}
                                                >
                                                    <XCircle className="w-4 h-4" />
                                                </Button>
                                            ) : (
                                                <Button
                                                    size="icon"
                                                    variant="ghost"
                                                    className="text-emerald-400 hover:text-emerald-300 hover:bg-emerald-500/20 h-8 w-8"
                                                    title="Reopen Job"
                                                    onClick={() => approveMutation.mutate(job.id)}
                                                    disabled={approveMutation.isPending}
                                                >
                                                    <CheckCircle2 className="w-4 h-4" />
                                                </Button>
                                            )}
                                        </div>
                                    </TableCell>
                                </TableRow>
                            ))
                        )}
                    </TableBody>
                </Table>
            </DataTableShell>

            <CreateJobModal open={isCreateModalOpen} onOpenChange={setIsCreateModalOpen} />

            <JobApplicationsModal
                job={viewingApplications}
                onClose={() => setViewingApplications(null)}
            />
        </div>
    );
};

const JobApplicationsModal = ({ job, onClose }: { job: JobAdminDto | null, onClose: () => void }) => {
    const [page] = useState(1);
    const [selectedAlumni, setSelectedAlumni] = useState<{ id: string, name: string } | null>(null);
    const pageSize = 10;

    const { data, isLoading } = useQuery({
        queryKey: ['job-applications', job?.id, page],
        queryFn: () => adminService.getJobApplications(job!.id, {
            skipCount: (page - 1) * pageSize,
            maxResultCount: pageSize
        }),
        enabled: !!job
    });

    return (
        <>
            <Dialog open={!!job} onOpenChange={(open) => !open && onClose()}>
                <DialogContent className="max-w-3xl max-h-[80vh] overflow-hidden flex flex-col">
                    <DialogHeader>
                        <DialogTitle className="flex items-center gap-2">
                            <Users className="w-5 h-5 text-accent" />
                            Applications: {job?.title}
                        </DialogTitle>
                    </DialogHeader>

                    <div className="flex-1 overflow-auto py-4">
                        <Table>
                            <TableHeader>
                                <TableRow>
                                    <TableHead>Alumni</TableHead>
                                    <TableHead>Applied Date</TableHead>
                                    <TableHead className="text-right">Action</TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {isLoading ? (
                                    <TableRow>
                                        <TableCell colSpan={3} className="text-center py-8 text-slate-400">Loading applications...</TableCell>
                                    </TableRow>
                                ) : data?.items.length === 0 ? (
                                    <TableRow>
                                        <TableCell colSpan={3} className="text-center py-8 text-slate-500">No applications yet.</TableCell>
                                    </TableRow>
                                ) : (
                                    data?.items.map((app: JobApplicationAdminDto) => (
                                        <TableRow key={app.id}>
                                            <TableCell>
                                                <div className="font-medium text-slate-900 dark:text-white uppercase">
                                                    {app.alumniName}
                                                </div>
                                                <div className="text-[10px] text-slate-500 font-mono opacity-50">{app.alumniId}</div>
                                            </TableCell>
                                            <TableCell>
                                                <div className="flex items-center gap-1.5 text-sm text-slate-500">
                                                    <Calendar className="w-3.5 h-3.5" />
                                                    {new Date(app.creationTime).toLocaleDateString()}
                                                </div>
                                            </TableCell>
                                            <TableCell className="text-right">
                                                <Button 
                                                    size="sm" 
                                                    variant="ghost" 
                                                    className="text-accent hover:bg-accent/10 font-bold"
                                                    onClick={() => setSelectedAlumni({ id: app.alumniId, name: app.alumniName })}
                                                >
                                                    <Eye className="w-4 h-4 mr-2" /> View CV
                                                </Button>
                                            </TableCell>
                                        </TableRow>
                                    ))
                                )}
                            </TableBody>
                        </Table>
                    </div>
                </DialogContent>
            </Dialog>

            {/* Live Profile View */}
            {selectedAlumni && (
                <AlumniProfileModal 
                    isOpen={!!selectedAlumni}
                    onClose={() => setSelectedAlumni(null)}
                    alumniId={selectedAlumni.id}
                    fullName={selectedAlumni.name}
                />
            )}
        </>
    );
};

export default JobManager;
