import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { Plus, Building2, Globe, Mail, Phone, Edit, Trash2, ToggleLeft, ToggleRight } from 'lucide-react';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/Table';
import { Button } from '../../components/ui/Button';
import { toast } from 'sonner';
import { PageHeader } from '../../components/admin/PageHeader';
import { DataTableShell } from '../../components/admin/DataTableShell';
import { StatusBadge } from '../../components/admin/StatusBadge';
import { CreateCompanyModal, EditCompanyModal } from '../../components/admin/company';
import type { CompanyDto } from '../../types/company';

const CompanyManager = () => {
    const queryClient = useQueryClient();
    const [filter, setFilter] = useState('');
    const [page, setPage] = useState(1);
    const pageSize = 10;
    const [statusFilter, setStatusFilter] = useState<'all' | 'active' | 'inactive'>('all');

    const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
    const [editingCompany, setEditingCompany] = useState<CompanyDto | null>(null);

    const { data, isLoading } = useQuery({
        queryKey: ['admin-companies', filter, page, statusFilter],
        queryFn: () => adminService.getCompanies({
            filter,
            isActive: statusFilter === 'all' ? undefined : statusFilter === 'active',
            skipCount: (page - 1) * pageSize,
            maxResultCount: pageSize,
            sorting: 'nameEn'
        })
    });

    const deleteMutation = useMutation({
        mutationFn: adminService.deleteCompany,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-companies'] });
            toast.success('Company deleted successfully.');
        },
        onError: (error: any) => {
            toast.error(error.response?.data?.error?.message || 'Failed to delete company. It might be linked to events or jobs.');
        }
    });

    const toggleStatusMutation = useMutation({
        mutationFn: ({ id, isActive, currentData }: { id: string, isActive: boolean, currentData: any }) =>
            adminService.updateCompany(id, { ...currentData, isActive }),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-companies'] });
            toast.success('Status updated successfully.');
        }
    });

    const handleDelete = (id: string) => {
        if (window.confirm('Are you sure you want to delete this company? This action cannot be undone.')) {
            deleteMutation.mutate(id);
        }
    };

    return (
        <div className="space-y-8 animate-in fade-in duration-500">
            <PageHeader
                title="Company Partners"
                description="Manage corporate partners for the Job Board and Events."
                action={
                    <Button className="shadow-neon" onClick={() => setIsCreateModalOpen(true)}>
                        <Plus className="w-4 h-4 mr-2" /> Register Company
                    </Button>
                }
            />

            <div className="flex space-x-2 border-b border-white/10 pb-4">
                {[
                    { label: 'All Partners', value: 'all' },
                    { label: 'Active', value: 'active' },
                    { label: 'Inactive', value: 'inactive' }
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
                searchPlaceholder="Search by name, industry..."
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
                            <TableHead>Company</TableHead>
                            <TableHead>Industry</TableHead>
                            <TableHead>Contact Info</TableHead>
                            <TableHead>Website</TableHead>
                            <TableHead>Status</TableHead>
                            <TableHead className="text-right px-6">Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {isLoading ? (
                            <TableRow>
                                <TableCell colSpan={6} className="h-48 text-center text-slate-400">
                                    <div className="flex flex-col items-center gap-2">
                                        <Building2 className="w-8 h-8 animate-pulse text-slate-600" />
                                        <span>Loading companies...</span>
                                    </div>
                                </TableCell>
                            </TableRow>
                        ) : data?.items.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={6} className="h-40 text-center text-slate-500">
                                    No companies matching your criteria.
                                </TableCell>
                            </TableRow>
                        ) : (
                            data?.items.map((company: CompanyDto) => (
                                <TableRow key={company.id} className="group hover:bg-slate-50/50 dark:hover:bg-white/5 transition-colors">
                                    <TableCell>
                                        <div className="flex items-center gap-3">
                                            <div className="w-12 h-12 rounded-lg bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 flex items-center justify-center overflow-hidden flex-shrink-0 shadow-sm">
                                                {company.logoBlobName ? (
                                                    <img
                                                        src={`${import.meta.env.VITE_API_BASE_URL}/api/app/file/download/${company.logoBlobName}`}
                                                        alt={company.nameEn}
                                                        className="w-full h-full object-contain p-1"
                                                    />
                                                ) : (
                                                    <Building2 className="w-6 h-6 text-slate-400" />
                                                )}
                                            </div>
                                            <div className="flex flex-col">
                                                <span className="font-semibold text-slate-900 dark:text-white leading-tight">{company.nameEn}</span>
                                                <span className="text-xs text-slate-500 font-arabic" dir="rtl">{company.nameAr}</span>
                                            </div>
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <div className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-50 text-blue-700 dark:bg-blue-900/30 dark:text-blue-300 border border-blue-100 dark:border-blue-800">
                                            {company.industry || 'General'}
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        <div className="space-y-1">
                                            {company.email && (
                                                <div className="flex items-center gap-1.5 text-xs text-slate-500">
                                                    <Mail className="w-3.5 h-3.5" />
                                                    {company.email}
                                                </div>
                                            )}
                                            {company.phoneNumber && (
                                                <div className="flex items-center gap-1.5 text-xs text-slate-500">
                                                    <Phone className="w-3.5 h-3.5" />
                                                    {company.phoneNumber}
                                                </div>
                                            )}
                                            {!company.email && !company.phoneNumber && (
                                                <span className="text-xs text-slate-400 italic">No contact info</span>
                                            )}
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        {company.websiteUrl ? (
                                            <a
                                                href={company.websiteUrl}
                                                target="_blank"
                                                rel="noreferrer"
                                                className="inline-flex items-center gap-1.5 text-xs font-medium text-accent hover:underline"
                                            >
                                                <Globe className="w-3.5 h-3.5" />
                                                Visit Site
                                            </a>
                                        ) : (
                                            <span className="text-xs text-slate-400">-</span>
                                        )}
                                    </TableCell>
                                    <TableCell>
                                        <StatusBadge variant={company.isActive ? 'success' : 'secondary'}>
                                            {company.isActive ? 'Active' : 'Inactive'}
                                        </StatusBadge>
                                    </TableCell>
                                    <TableCell className="text-right px-6">
                                        <div className="flex justify-end gap-2">
                                            <Button
                                                size="icon"
                                                variant="ghost"
                                                className="h-8 w-8 text-blue-500 hover:bg-blue-50 dark:hover:bg-blue-500/10"
                                                onClick={() => setEditingCompany(company)}
                                                title="Edit Details"
                                            >
                                                <Edit className="w-4 h-4" />
                                            </Button>
                                            <Button
                                                size="icon"
                                                variant="ghost"
                                                className={`h-8 w-8 ${company.isActive ? 'text-slate-400' : 'text-emerald-500'} hover:bg-slate-50 dark:hover:bg-white/5`}
                                                onClick={() => toggleStatusMutation.mutate({
                                                    id: company.id,
                                                    isActive: !company.isActive,
                                                    currentData: company
                                                })}
                                                title={company.isActive ? 'Deactivate' : 'Activate'}
                                            >
                                                {company.isActive ? <ToggleLeft className="w-4 h-4" /> : <ToggleRight className="w-4 h-4" />}
                                            </Button>
                                            <Button
                                                size="icon"
                                                variant="ghost"
                                                className="h-8 w-8 text-red-500 hover:bg-red-50 dark:hover:bg-red-500/10"
                                                onClick={() => handleDelete(company.id)}
                                                title="Delete Partner"
                                            >
                                                <Trash2 className="w-4 h-4" />
                                            </Button>
                                        </div>
                                    </TableCell>
                                </TableRow>
                            ))
                        )}
                    </TableBody>
                </Table>
            </DataTableShell>

            <CreateCompanyModal
                open={isCreateModalOpen}
                onOpenChange={setIsCreateModalOpen}
            />

            <EditCompanyModal
                company={editingCompany}
                open={!!editingCompany}
                onOpenChange={(open) => !open && setEditingCompany(null)}
            />
        </div>
    );
};

export default CompanyManager;
