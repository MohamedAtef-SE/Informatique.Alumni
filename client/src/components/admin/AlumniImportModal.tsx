import React, { useState } from 'react';
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from '../../components/ui/Dialog';
import { Button } from '../../components/ui/Button';
import { UploadCloud, FileSpreadsheet, Loader2, Download, AlertCircle } from 'lucide-react';
import { adminService } from '../../services/adminService';
import { toast } from 'sonner';

interface AlumniImportModalProps {
    isOpen: boolean;
    onClose: () => void;
    onSuccess: () => void;
}

interface ImportResult {
    totalRecords: number;
    successCount: number;
    failureCount: number;
    errorMessages: string[];
}

export const AlumniImportModal: React.FC<AlumniImportModalProps> = ({ isOpen, onClose, onSuccess }) => {
    const [file, setFile] = useState<File | null>(null);
    const [isUploading, setIsUploading] = useState(false);
    const [result, setResult] = useState<ImportResult | null>(null);

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const selected = e.target.files?.[0];
        if (selected) {
            if (!selected.name.endsWith('.xlsx')) {
                toast.error('Only .xlsx files are allowed.');
                return;
            }
            setFile(selected);
            setResult(null); // Reset prev results
        }
    };

    const handleUpload = async () => {
        if (!file) return;

        try {
            setIsUploading(true);
            const res = await adminService.importAlumniExcel(file);
            setResult(res as ImportResult);
            if ((res as ImportResult).failureCount === 0) {
                toast.success('All records imported successfully!');
            } else if ((res as ImportResult).successCount > 0) {
                toast.warning('Import completed with some errors.');
            } else {
                toast.error('Import failed for all records.');
            }
            onSuccess(); // Refresh the table in the background implicitly
        } catch (error: any) {
            toast.error(error?.response?.data?.error?.message || 'Failed to import the file due to a server error.');
        } finally {
            setIsUploading(false);
        }
    };

    const downloadTemplate = async () => {
        try {
            const blob = await adminService.downloadImportTemplate();
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = 'Alumni_Import_Template.xlsx';
            document.body.appendChild(a);
            a.click();
            window.URL.revokeObjectURL(url);
            document.body.removeChild(a);
            toast.success("Template downloaded! Fill down 'DataLookup' selections!");
        } catch (error) {
            toast.error("Failed to download template.");
        }
    };

    const handleClose = () => {
        setFile(null);
        setResult(null);
        onClose();
    };

    return (
        <Dialog open={isOpen} onOpenChange={handleClose}>
            <DialogContent className="sm:max-w-2xl bg-white dark:bg-slate-900 border-slate-200 dark:border-slate-800">
                <DialogHeader>
                    <DialogTitle className="flex items-center gap-2 text-slate-900 dark:text-white">
                        <FileSpreadsheet className="w-5 h-5 text-emerald-500" />
                        Bulk Import Alumni
                    </DialogTitle>
                    <DialogDescription className="text-slate-500 dark:text-slate-400">
                        Upload an Excel file (.xlsx) containing historical alumni records to bootstrap the platform.
                    </DialogDescription>
                </DialogHeader>

                <div className="py-4 space-y-6">
                    {/* Template Section */}
                    <div className="flex items-center justify-between p-4 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50 dark:bg-slate-800/50">
                        <div className="space-y-1">
                            <h4 className="text-sm font-medium text-slate-900 dark:text-slate-200">Need the correct format?</h4>
                            <p className="text-xs text-slate-500 dark:text-slate-400">Includes Data Validation dropdowns for precise mappings!</p>
                        </div>
                        <Button variant="outline" size="sm" onClick={() => downloadTemplate()} className="gap-2">
                            <Download className="w-4 h-4" />
                            Template
                        </Button>
                    </div>

                    {/* Upload Section */}
                    {!result ? (
                        <div className="space-y-4">
                            <div className="relative group cursor-pointer">
                                <input
                                    type="file"
                                    accept=".xlsx"
                                    onChange={handleFileChange}
                                    className="absolute inset-0 w-full h-full opacity-0 cursor-pointer z-10"
                                    disabled={isUploading}
                                />
                                <div className={`flex flex-col items-center justify-center p-8 border-2 border-dashed rounded-xl transition-colors duration-200 ${file ? 'border-emerald-500 bg-emerald-50 dark:bg-emerald-500/10' : 'border-slate-300 dark:border-slate-700 hover:border-indigo-500 hover:bg-slate-50 dark:hover:bg-slate-800/50'}`}>
                                    <UploadCloud className={`w-10 h-10 mb-3 ${file ? 'text-emerald-500' : 'text-slate-400 group-hover:text-indigo-500'}`} />
                                    <p className="text-sm font-medium text-slate-700 dark:text-slate-300">
                                        {file ? file.name : "Drag & drop your Excel file here"}
                                    </p>
                                    <p className="text-xs text-slate-500 mt-1">
                                        {file ? `${(file.size / 1024).toFixed(1)} KB` : "Supports .xlsx up to 10MB"}
                                    </p>
                                </div>
                            </div>

                            <div className="flex justify-end gap-3 pt-4 border-t border-slate-200 dark:border-slate-800">
                                <Button variant="ghost" onClick={handleClose} disabled={isUploading}>Cancel</Button>
                                <Button 
                                    onClick={handleUpload} 
                                    disabled={!file || isUploading}
                                    className="bg-indigo-600 hover:bg-indigo-700 text-white gap-2"
                                >
                                    {isUploading ? <Loader2 className="w-4 h-4 animate-spin" /> : <UploadCloud className="w-4 h-4" />}
                                    {isUploading ? 'Processing...' : 'Upload & Process'}
                                </Button>
                            </div>
                        </div>
                    ) : (
                        /* Results Section */
                        <div className="space-y-6 animate-in fade-in duration-300">
                            <div className="grid grid-cols-3 gap-4">
                                <div className="p-4 rounded-xl border border-slate-200 dark:border-slate-800 text-center">
                                    <div className="text-2xl font-bold text-slate-900 dark:text-white">{result.totalRecords}</div>
                                    <div className="text-xs text-slate-500 font-medium">Total Rows</div>
                                </div>
                                <div className="p-4 rounded-xl border border-emerald-200 dark:border-emerald-500/30 bg-emerald-50 dark:bg-emerald-500/5 text-center">
                                    <div className="text-2xl font-bold text-emerald-600 dark:text-emerald-400">{result.successCount}</div>
                                    <div className="text-xs text-emerald-600/70 dark:text-emerald-400/70 font-medium">Succeeded</div>
                                </div>
                                <div className="p-4 rounded-xl border border-red-200 dark:border-red-500/30 bg-red-50 dark:bg-red-500/5 text-center">
                                    <div className="text-2xl font-bold text-red-600 dark:text-red-400">{result.failureCount}</div>
                                    <div className="text-xs text-red-600/70 dark:text-red-400/70 font-medium">Failed</div>
                                </div>
                            </div>

                            {result.errorMessages.length > 0 && (
                                <div className="space-y-2">
                                    <h4 className="text-sm font-semibold flex items-center gap-2 text-slate-900 dark:text-slate-200">
                                        <AlertCircle className="w-4 h-4 text-amber-500" />
                                        Error Log
                                    </h4>
                                    <div className="max-h-[200px] overflow-y-auto rounded-lg border border-slate-200 dark:border-slate-800 bg-slate-50 dark:bg-slate-900 p-3 space-y-2">
                                        {result.errorMessages.map((err, idx) => (
                                            <div key={idx} className="text-xs text-red-600 dark:text-red-400 break-all font-mono">
                                                {err}
                                            </div>
                                        ))}
                                    </div>
                                </div>
                            )}

                            <div className="flex justify-end pt-4 border-t border-slate-200 dark:border-slate-800">
                                <Button onClick={handleClose} variant="default">Done</Button>
                            </div>
                        </div>
                    )}
                </div>
            </DialogContent>
        </Dialog>
    );
};
