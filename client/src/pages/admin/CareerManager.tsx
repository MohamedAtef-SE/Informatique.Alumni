import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { Plus, Edit, Trash2, Search, Briefcase } from 'lucide-react';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../components/ui/Table';
import { Button } from '../../components/ui/Button';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/Card';
import { cn } from '../../utils/cn';
import { toast } from 'sonner';

const CareerManager = () => {
    const [filter, setFilter] = useState('');

    const { data } = useQuery({
        queryKey: ['admin-career', filter],
        queryFn: () => adminService.getCareerServices({ filter, maxResultCount: 20 })
    });

    return (
        <div className="space-y-8 animate-fade-in">
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
                <div>
                    <h1 className="text-3xl font-heading font-bold text-white">
                        Career Manager
                    </h1>
                    <p className="text-slate-400 mt-1">Manage career workshops, training sessions, and services.</p>
                </div>
                <Button className="shadow-neon" onClick={() => toast.info("Create wizard coming soon!")}>
                    <Plus className="w-4 h-4 mr-2" /> New Workshop
                </Button>
            </div>

            <Card variant="glass">
                <CardHeader className="flex flex-col md:flex-row gap-4 justify-between items-start md:items-center border-b border-white/5 pb-6">
                    <CardTitle>Services List</CardTitle>
                    <div className="relative group w-full md:w-72">
                        <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-500 group-focus-within:text-accent transition-colors" />
                        <input
                            type="text"
                            placeholder="Search workshops..."
                            className="w-full bg-slate-900/50 border border-white/10 rounded-lg pl-10 pr-4 py-2 text-sm text-white focus:outline-none focus:border-accent focus:ring-1 focus:ring-accent transition-all"
                            value={filter}
                            onChange={(e) => setFilter(e.target.value)}
                        />
                    </div>
                </CardHeader>
                <CardContent className="p-0">
                    <Table>
                        <TableHeader>
                            <TableRow className="hover:bg-transparent border-white/5">
                                <TableHead className="w-[40%]">Title</TableHead>
                                <TableHead>Code</TableHead>
                                <TableHead>Status</TableHead>
                                <TableHead className="text-right">Actions</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {data?.items.length === 0 ? (
                                <TableRow>
                                    <TableCell colSpan={4} className="h-32 text-center text-slate-500">
                                        No services found.
                                    </TableCell>
                                </TableRow>
                            ) : (
                                data?.items.map((item: any) => (
                                    <TableRow key={item.id}>
                                        <TableCell>
                                            <div className="text-white font-medium flex items-center gap-2">
                                                <div className="p-1.5 rounded bg-white/5 text-slate-400">
                                                    <Briefcase className="w-4 h-4" />
                                                </div>
                                                {item.nameEn}
                                            </div>
                                        </TableCell>
                                        <TableCell>
                                            <code className="text-xs font-mono text-slate-400 bg-white/5 px-2 py-1 rounded border border-white/10">
                                                {item.code}
                                            </code>
                                        </TableCell>
                                        <TableCell>
                                            <span className={cn(
                                                "px-2.5 py-1 rounded-full text-xs font-bold uppercase border",
                                                "bg-blue-500/10 text-blue-500 border-blue-500/20"
                                            )}>
                                                Active
                                            </span>
                                        </TableCell>
                                        <TableCell className="text-right">
                                            <div className="flex justify-end gap-2">
                                                <Button size="icon" variant="ghost" className="text-slate-400 hover:text-white" title="Edit">
                                                    <Edit className="w-4 h-4" />
                                                </Button>
                                                <Button
                                                    size="icon"
                                                    variant="ghost"
                                                    className="text-red-400 hover:text-red-300 hover:bg-red-500/20"
                                                    title="Delete"
                                                    onClick={() => {
                                                        if (confirm("Delete this service?")) toast.success("Service deleted (mock)");
                                                    }}
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
                </CardContent>
            </Card>
        </div>
    );
};

export default CareerManager;
