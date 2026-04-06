import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminService } from '../../../services/adminService';
import { Button } from '../../../components/ui/Button';
import { 
    Table, 
    TableBody, 
    TableCell, 
    TableHead, 
    TableHeader, 
    TableRow 
} from '../../../components/ui/Table';
import { 
    Sheet, 
    SheetContent, 
    SheetHeader, 
    SheetTitle, 
    SheetDescription 
} from '../../../components/ui/Sheet';
import { 
    FileText, 
    Plus, 
    Trash2, 
    Edit2, 
    Loader2, 
    AlertCircle 
} from 'lucide-react';
import { CertificateDefinitionModal } from './CertificateDefinitionModal';
import { ConfirmDialog } from '../../../components/ui/ConfirmDialog';
import { toast } from 'sonner';
import type { CertificateDefinitionDto } from '../../../types/certificates';

interface CertificateTypeManagementDrawerProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
}

export const CertificateTypeManagementDrawer = ({ open, onOpenChange }: CertificateTypeManagementDrawerProps) => {
    const queryClient = useQueryClient();
    const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
    const [definitionToEdit, setDefinitionToEdit] = useState<CertificateDefinitionDto | null>(null);
    const [definitionToDelete, setDefinitionToDelete] = useState<string | null>(null);

    const { data: definitions, isLoading } = useQuery({
        queryKey: ['admin-certificate-definitions'],
        queryFn: () => adminService.getCertificateDefinitions({ skipCount: 0, maxResultCount: 1000 }),
        enabled: open
    });

    const deleteMutation = useMutation({
        mutationFn: adminService.deleteCertificateDefinition,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-certificate-definitions'] });
            toast.success('Certificate type deleted successfully');
            setDefinitionToDelete(null);
        },
        onError: (error: any) => {
            const message = error.response?.data?.error?.message || 'Failed to delete certificate type';
            toast.error(message);
            setDefinitionToDelete(null);
        }
    });

    return (
        <>
            <Sheet open={open} onOpenChange={onOpenChange}>
                <SheetContent side="right" className="w-[100%] sm:max-w-2xl overflow-y-auto">
                    <SheetHeader className="mb-6">
                        <SheetTitle className="flex items-center gap-2">
                            <FileText className="w-5 h-5 text-primary" />
                            Certificate Type Management
                        </SheetTitle>
                        <SheetDescription>
                            Define available certificate types, their fees, and required documents.
                        </SheetDescription>
                    </SheetHeader>

                    <div className="space-y-6">
                        <div className="flex justify-end">
                            <Button 
                                className="shadow-neon group px-6 h-11 transition-all duration-300 hover:scale-[1.02]" 
                                onClick={() => setIsCreateModalOpen(true)}
                            >
                                <Plus className="w-5 h-5 mr-2.5 opacity-90 group-hover:scale-110 transition-transform" />
                                <span className="tracking-wide">Add Certificate Type</span>
                            </Button>
                        </div>

                        <div className="rounded-lg border border-slate-200 overflow-hidden bg-white">
                            <Table>
                                <TableHeader className="bg-slate-50">
                                    <TableRow>
                                        <TableHead>Type Name</TableHead>
                                        <TableHead>Fee (EGP)</TableHead>
                                        <TableHead>Level</TableHead>
                                        <TableHead className="text-right">Actions</TableHead>
                                    </TableRow>
                                </TableHeader>
                                <TableBody>
                                    {isLoading ? (
                                        <TableRow>
                                            <TableCell colSpan={4} className="h-40 text-center">
                                                <Loader2 className="w-6 h-6 animate-spin mx-auto text-primary" />
                                                <p className="mt-2 text-sm text-slate-500">Loading definitions...</p>
                                            </TableCell>
                                        </TableRow>
                                    ) : definitions?.items.length === 0 ? (
                                        <TableRow>
                                            <TableCell colSpan={4} className="h-40 text-center text-slate-500">
                                                <AlertCircle className="w-8 h-8 mx-auto mb-2 opacity-20" />
                                                No certificate types defined yet.
                                            </TableCell>
                                        </TableRow>
                                    ) : (
                                        definitions?.items.map((def: CertificateDefinitionDto) => (
                                            <TableRow key={def.id} className="group hover:bg-slate-50 transition-colors">
                                                <TableCell>
                                                    <div className="flex flex-col">
                                                        <span className="font-semibold text-slate-900">{def.nameEn}</span>
                                                        <span className="text-xs text-slate-500 font-arabic text-right mt-0.5">{def.nameAr}</span>
                                                    </div>
                                                </TableCell>
                                                <TableCell className="font-mono font-medium text-slate-700">
                                                    {def.fee.toLocaleString()}
                                                </TableCell>
                                                <TableCell>
                                                    <span className={`text-[10px] uppercase font-bold px-2 py-0.5 rounded-full ${
                                                        def.degreeType === 1 ? 'bg-blue-50 text-blue-600 border border-blue-100' : 'bg-purple-50 text-purple-600 border border-purple-100'
                                                    }`}>
                                                        {def.degreeType === 1 ? 'Undergrad' : 'Postgrad'}
                                                    </span>
                                                </TableCell>
                                                <TableCell className="text-right">
                                                    <div className="flex justify-end gap-1">
                                                        <Button 
                                                            size="icon" 
                                                            variant="ghost" 
                                                            className="h-8 w-8 text-blue-500 hover:text-blue-600 hover:bg-blue-50"
                                                            onClick={() => setDefinitionToEdit(def)}
                                                        >
                                                            <Edit2 className="w-3.5 h-3.5" />
                                                        </Button>
                                                        <Button 
                                                            size="icon" 
                                                            variant="ghost" 
                                                            className="h-8 w-8 text-red-500 hover:text-red-600 hover:bg-red-50"
                                                            onClick={() => setDefinitionToDelete(def.id)}
                                                        >
                                                            <Trash2 className="w-3.5 h-3.5" />
                                                        </Button>
                                                    </div>
                                                </TableCell>
                                            </TableRow>
                                        ))
                                    )}
                                </TableBody>
                            </Table>
                        </div>
                    </div>
                </SheetContent>
            </Sheet>

            <CertificateDefinitionModal 
                open={isCreateModalOpen || !!definitionToEdit}
                onOpenChange={(open) => {
                    if (!open) {
                        setIsCreateModalOpen(false);
                        setDefinitionToEdit(null);
                    }
                }}
                definition={definitionToEdit}
            />

            <ConfirmDialog 
                open={!!definitionToDelete}
                onOpenChange={(open) => !open && setDefinitionToDelete(null)}
                title="Delete Certificate Type?"
                description="This will prevent users from requesting this type. Existing requests will not be affected."
                confirmLabel="Delete Type"
                variant="danger"
                onConfirm={() => definitionToDelete && deleteMutation.mutate(definitionToDelete)}
                isLoading={deleteMutation.isPending}
            />
        </>
    );
};
