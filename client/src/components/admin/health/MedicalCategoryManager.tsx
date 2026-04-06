import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../../services/adminService';
import { Plus, Trash2, Edit2, Check, X, Pill, Building2, Beaker, Stethoscope } from 'lucide-react';
import { DataTableShell } from '../DataTableShell';
import { Button } from '../../../components/ui/Button';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../../components/ui/Table';
import { Input } from '../../../components/ui/Input';
import { toast } from 'sonner';
import { MedicalPartnerType, type MedicalCategoryDto } from '../../../types/health';

export const MedicalCategoryManager = () => {
    const queryClient = useQueryClient();
    const [page, setPage] = useState(1);
    const pageSize = 10;
    const [filter, setFilter] = useState('');
    const [isAdding, setIsAdding] = useState(false);
    const [editingId, setEditingId] = useState<string | null>(null);

    // Form states
    const [formData, setFormData] = useState({
        nameEn: '',
        nameAr: '',
        baseType: MedicalPartnerType.Other as number,
        isActive: true
    });

    const { data, isLoading } = useQuery({
        queryKey: ['admin-medical-categories', page, filter],
        queryFn: () => adminService.getMedicalCategories({
            skipCount: (page - 1) * pageSize,
            maxResultCount: pageSize,
            filterText: filter
        })
    });

    const createMutation = useMutation({
        mutationFn: adminService.createMedicalCategory,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-medical-categories'] });
            toast.success('Category created');
            setIsAdding(false);
            resetForm();
        }
    });

    const updateMutation = useMutation({
        mutationFn: ({ id, input }: { id: string, input: any }) => adminService.updateMedicalCategory(id, input),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-medical-categories'] });
            toast.success('Category updated');
            setEditingId(null);
        }
    });

    const deleteMutation = useMutation({
        mutationFn: adminService.deleteMedicalCategory,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-medical-categories'] });
            toast.success('Category deleted');
        }
    });

    const resetForm = () => {
        setFormData({ nameEn: '', nameAr: '', baseType: MedicalPartnerType.Other, isActive: true });
    };

    const handleSave = () => {
        if (!formData.nameEn || !formData.nameAr) {
            toast.error('Names are required');
            return;
        }

        if (editingId) {
            updateMutation.mutate({ id: editingId, input: formData });
        } else {
            createMutation.mutate(formData);
        }
    };

    const startEdit = (cat: MedicalCategoryDto) => {
        setEditingId(cat.id);
        setFormData({
            nameEn: cat.nameEn,
            nameAr: cat.nameAr,
            baseType: cat.baseType,
            isActive: cat.isActive
        });
    };

    const getIcon = (type: number) => {
        switch (type) {
            case MedicalPartnerType.Pharmacy: return <Pill className="w-4 h-4 text-pink-500" />;
            case MedicalPartnerType.Hospital: return <Building2 className="w-4 h-4 text-blue-500" />;
            case MedicalPartnerType.Lab: return <Beaker className="w-4 h-4 text-purple-500" />;
            case MedicalPartnerType.Clinic: return <Stethoscope className="w-4 h-4 text-emerald-500" />;
            default: return <Stethoscope className="w-4 h-4 text-slate-400" />;
        }
    };

    const items = data?.items || [];
    const totalCount = data?.totalCount || 0;

    return (
        <div className="space-y-4">
            <div className="flex justify-between items-center">
                <h3 className="text-lg font-semibold text-slate-900 dark:text-white">Categories</h3>
                <Button size="sm" onClick={() => { setIsAdding(true); resetForm(); }} disabled={isAdding}>
                    <Plus className="w-4 h-4 mr-2" /> Add Category
                </Button>
            </div>

            <DataTableShell
                searchPlaceholder="Search categories..."
                onSearch={setFilter}
                pagination={{
                    currentPage: page,
                    totalPages: Math.ceil(totalCount / pageSize) || 1,
                    onPageChange: setPage
                }}
            >
                <Table>
                    <TableHeader>
                        <TableRow className="hover:bg-transparent border-slate-200 dark:border-white/5">
                            <TableHead>Icon/Base</TableHead>
                            <TableHead>English Name</TableHead>
                            <TableHead>Arabic Name</TableHead>
                            <TableHead>Status</TableHead>
                            <TableHead className="text-right">Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {isAdding && (
                            <TableRow className="bg-accent/5 animate-in slide-in-from-top-2">
                                <TableCell>
                                    <select 
                                        className="bg-transparent border border-white/10 rounded px-2 py-1 text-xs"
                                        value={formData.baseType}
                                        onChange={e => setFormData({...formData, baseType: Number(e.target.value)})}
                                    >
                                        {Object.entries(MedicalPartnerType).map(([key, val]) => (
                                            <option key={val} value={val}>{key}</option>
                                        ))}
                                    </select>
                                </TableCell>
                                <TableCell>
                                    <Input 
                                        placeholder="Category Name (EN)" 
                                        value={formData.nameEn} 
                                        onChange={e => setFormData({...formData, nameEn: e.target.value})}
                                        className="h-8 text-xs"
                                    />
                                </TableCell>
                                <TableCell>
                                    <Input 
                                        placeholder="Category Name (AR)" 
                                        value={formData.nameAr} 
                                        onChange={e => setFormData({...formData, nameAr: e.target.value})}
                                        dir="rtl"
                                        className="h-8 text-xs"
                                    />
                                </TableCell>
                                <TableCell>Active</TableCell>
                                <TableCell className="text-right">
                                    <div className="flex justify-end gap-1">
                                        <Button size="icon" variant="ghost" className="h-8 w-8 text-green-500" onClick={handleSave}>
                                            <Check className="w-4 h-4" />
                                        </Button>
                                        <Button size="icon" variant="ghost" className="h-8 w-8 text-slate-500" onClick={() => setIsAdding(false)}>
                                            <X className="w-4 h-4" />
                                        </Button>
                                    </div>
                                </TableCell>
                            </TableRow>
                        )}

                        {isLoading ? (
                            <TableRow>
                                <TableCell colSpan={5} className="py-16 text-center">
                                    <div className="flex flex-col items-center justify-center gap-3">
                                        <div className="relative flex h-10 w-10 items-center justify-center">
                                            <div className="absolute h-full w-full rounded-full border-2 border-[var(--color-accent)] animate-ping opacity-20" />
                                            <div className="h-5 w-5 rotate-45 rounded-sm bg-[var(--color-accent)] animate-pulse shadow-[0_0_15px_rgba(45,150,215,0.4)]" />
                                        </div>
                                    </div>
                                </TableCell>
                            </TableRow>
                        ) : items.length === 0 && !isAdding ? (
                            <TableRow><TableCell colSpan={5} className="text-center py-8">No categories found.</TableCell></TableRow>
                        ) : (
                            items.map((cat: MedicalCategoryDto) => (
                                <TableRow key={cat.id}>
                                    <TableCell>
                                        <div className="flex items-center gap-2">
                                            {getIcon(cat.baseType)}
                                            <span className="text-xs uppercase text-slate-500">
                                                {Object.keys(MedicalPartnerType).find(k => MedicalPartnerType[k as keyof typeof MedicalPartnerType] === cat.baseType)}
                                            </span>
                                        </div>
                                    </TableCell>
                                    <TableCell>
                                        {editingId === cat.id ? (
                                            <Input 
                                                value={formData.nameEn} 
                                                onChange={e => setFormData({...formData, nameEn: e.target.value})}
                                                className="h-8 text-xs"
                                            />
                                        ) : cat.nameEn}
                                    </TableCell>
                                    <TableCell dir="rtl">
                                        {editingId === cat.id ? (
                                            <Input 
                                                value={formData.nameAr} 
                                                onChange={e => setFormData({...formData, nameAr: e.target.value})}
                                                className="h-8 text-xs"
                                            />
                                        ) : cat.nameAr}
                                    </TableCell>
                                    <TableCell>
                                        <span className={cat.isActive ? "text-green-500" : "text-slate-500 text-xs"}>
                                            {cat.isActive ? 'Active' : 'Inactive'}
                                        </span>
                                    </TableCell>
                                    <TableCell className="text-right">
                                        <div className="flex justify-end gap-1">
                                            {editingId === cat.id ? (
                                                <>
                                                    <Button size="icon" variant="ghost" className="h-8 w-8 text-green-500" onClick={handleSave}>
                                                        <Check className="w-4 h-4" />
                                                    </Button>
                                                    <Button size="icon" variant="ghost" className="h-8 w-8 text-slate-500" onClick={() => setEditingId(null)}>
                                                        <X className="w-4 h-4" />
                                                    </Button>
                                                </>
                                            ) : (
                                                <>
                                                    <Button size="icon" variant="ghost" className="h-8 w-8" onClick={() => startEdit(cat)}>
                                                        <Edit2 className="w-4 h-4" />
                                                    </Button>
                                                    <Button size="icon" variant="ghost" className="h-8 w-8 text-red-500" onClick={() => deleteMutation.mutate(cat.id)}>
                                                        <Trash2 className="w-4 h-4" />
                                                    </Button>
                                                </>
                                            )}
                                        </div>
                                    </TableCell>
                                </TableRow>
                            ))
                        )}
                    </TableBody>
                </Table>
            </DataTableShell>
        </div>
    );
};
