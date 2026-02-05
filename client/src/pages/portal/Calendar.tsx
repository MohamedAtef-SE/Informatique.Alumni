import { useQuery } from '@tanstack/react-query';
import { calendarService } from '../../services/calendarService';
import { Download, AlertCircle, Calendar as CalendarIcon } from 'lucide-react';
import clsx from 'clsx';
import { Card, CardContent } from '../../components/ui/Card';
import { Button } from '../../components/ui/Button';

const Calendar = () => {
    const { data: events } = useQuery({
        queryKey: ['academic-calendar'],
        queryFn: calendarService.getCalendar
    });

    const handleDownload = async () => {
        try {
            const blob = await calendarService.downloadExcel();
            const url = window.URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = url;
            link.download = `Academic_Calendar.xlsx`;
            document.body.appendChild(link);
            link.click();
            link.remove();
        } catch (error) {
            console.error("Failed to download calendar", error);
            alert("Failed to download calendar.");
        }
    };

    // Group by Term
    const grouped = events?.reduce((acc, event) => {
        const term = String(event.semester) || 'General'; // Handle number or string
        if (!acc[term]) acc[term] = [];
        acc[term].push(event);
        return acc;
    }, {} as Record<string, typeof events>) || {};

    return (
        <div className="space-y-6 animate-fade-in">
            <div className="flex justify-between items-center">
                <div>
                    <h1 className="text-3xl font-heading font-bold text-[var(--color-text-primary)]">Academic Calendar</h1>
                    <p className="text-[var(--color-text-secondary)] mt-1">Key dates, holidays, and academic schedule.</p>
                </div>
                <Button variant="outline" onClick={handleDownload} className="flex items-center gap-2 shadow-sm">
                    <Download className="w-4 h-4" /> Export to Excel
                </Button>
            </div>

            <div className="space-y-8 animate-slide-up">
                {Object.entries(grouped).map(([term, termEvents]) => (
                    <Card key={term} variant="default" className="border-[var(--color-border)] shadow-sm">
                        <CardContent className="p-6">
                            <h2 className="text-xl font-bold text-[var(--color-accent)] mb-4 flex items-center gap-2">
                                <CalendarIcon className="w-5 h-5" /> {term}
                            </h2>
                            <div className="space-y-4">
                                {termEvents?.map((event, idx) => (
                                    <div key={idx} className={clsx("flex items-start gap-4 p-4 rounded-xl border transition-all hover:shadow-md", "bg-slate-50 border-[var(--color-border)] hover:border-[var(--color-accent)]/30 hover:bg-white")}>
                                        <div className="flex-shrink-0 w-16 text-center bg-white rounded-lg p-2 border border-[var(--color-border)] shadow-sm">
                                            <div className="text-xs text-[var(--color-text-muted)] uppercase font-bold">{new Date(event.startDate).toLocaleString('default', { month: 'short' })}</div>
                                            <div className="text-2xl font-bold text-[var(--color-text-primary)]">{new Date(event.startDate).getDate()}</div>
                                        </div>
                                        <div>
                                            <div className="flex items-center gap-2">
                                                <h3 className="text-lg font-bold text-[var(--color-text-primary)]">{event.eventName}</h3>
                                            </div>
                                            {event.description && <p className="text-[var(--color-text-secondary)] text-sm mt-1 leading-relaxed">{event.description}</p>}
                                            {event.endDate && <p className="text-[var(--color-text-muted)] text-xs mt-2 font-medium bg-slate-100 w-fit px-2 py-1 rounded">Ends: {new Date(event.endDate).toLocaleDateString()}</p>}
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </CardContent>
                    </Card>
                ))}

                {(!events || events.length === 0) && (
                    <div className="text-center py-20 text-[var(--color-text-muted)] bg-slate-50 rounded-xl border border-dashed border-[var(--color-border)]">
                        <AlertCircle className="w-12 h-12 mx-auto mb-4 opacity-50" />
                        No academic events found.
                    </div>
                )}
            </div>
        </div>
    );
};

export default Calendar;
