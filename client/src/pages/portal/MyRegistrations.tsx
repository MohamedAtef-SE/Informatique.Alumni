import { useQuery } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { eventsService } from '../../services/eventsService';
import { RegistrationStatus, RegistrationType } from '../../types/events';
import { Card, CardContent } from '../../components/ui/Card';
import { Button } from '../../components/ui/Button';
import { Calendar, MapPin, Ticket, Clock, CheckCircle2, AlertCircle, XCircle, QrCode, UserCircle, GraduationCap } from 'lucide-react';
import { cn } from '../../utils/cn';
import { useState } from 'react';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from '../../components/ui/Dialog';

const MyRegistrations = () => {
    const { t } = useTranslation();
    const [selectedTicket, setSelectedTicket] = useState<any>(null);

    const { data: registrations, isLoading } = useQuery({
        queryKey: ['my-registrations'],
        queryFn: () => eventsService.getMyRegistrations()
    });

    const getStatusStyles = (status: RegistrationStatus) => {
        switch (status) {
            case RegistrationStatus.Confirmed:
                return {
                    bg: 'bg-emerald-50',
                    text: 'text-emerald-700',
                    border: 'border-emerald-200',
                    icon: <CheckCircle2 className="w-4 h-4" />,
                    label: t('events.status_confirmed', 'Confirmed')
                };
            case RegistrationStatus.Pending:
                return {
                    bg: 'bg-amber-50',
                    text: 'text-amber-700',
                    border: 'border-amber-200',
                    icon: <Clock className="w-4 h-4" />,
                    label: t('events.status_pending', 'Pending Approval')
                };
            case RegistrationStatus.Cancelled:
            case RegistrationStatus.Attended:
                 return {
                    bg: status === RegistrationStatus.Attended ? 'bg-blue-50' : 'bg-red-50',
                    text: status === RegistrationStatus.Attended ? 'text-blue-700' : 'text-red-700',
                    border: status === RegistrationStatus.Attended ? 'border-blue-200' : 'border-red-200',
                    icon: status === RegistrationStatus.Attended ? <CheckCircle2 className="w-4 h-4" /> : <XCircle className="w-4 h-4" />,
                    label: status === RegistrationStatus.Attended ? t('events.status_attended', 'Attended') : t('events.status_cancelled', 'Cancelled')
                };
            default:
                return {
                    bg: 'bg-slate-50',
                    text: 'text-slate-700',
                    border: 'border-slate-200',
                    icon: <AlertCircle className="w-4 h-4" />,
                    label: t('events.status_unknown', 'Unknown')
                };
        }
    };

    if (isLoading) {
        return (
            <div className="flex items-center justify-center min-h-[400px]">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
            </div>
        );
    }

    return (
        <div className="max-w-6xl mx-auto px-4 py-8 animate-fade-in">
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-8">
                <div>
                    <h1 className="text-3xl font-bold text-slate-900 flex items-center gap-3">
                        <Ticket className="w-8 h-8 text-blue-600" />
                        {t('events.my_tickets_title', 'My Tickets')}
                    </h1>
                    <p className="text-slate-500 mt-2">
                        {t('events.my_tickets_subtitle', 'Keep track of all your event and workshop bookings in one place.')}
                    </p>
                </div>
            </div>

            {!registrations || registrations.length === 0 ? (
                <Card className="border-dashed border-2 border-slate-200 bg-slate-50/50">
                    <CardContent className="flex flex-col items-center justify-center py-20 text-center">
                        <Ticket className="w-16 h-16 text-slate-200 mb-4" />
                        <h3 className="text-xl font-bold text-slate-900">{t('events.no_registrations', 'No registrations found')}</h3>
                        <p className="text-slate-500 max-w-sm mt-2">
                            {t('events.no_registrations_desc', "You haven't registered for any events or workshops yet.")}
                        </p>
                        <Button className="mt-6" variant="default" onClick={() => window.location.href = '/portal/events'}>
                            {t('events.browse_events', 'Browse Events')}
                        </Button>
                    </CardContent>
                </Card>
            ) : (
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                    {registrations.map((reg) => {
                        const status = getStatusStyles(reg.status);
                        const isWorkshop = reg.type === RegistrationType.Workshop;
                        
                        return (
                            <Card key={reg.id} className="overflow-hidden hover:shadow-lg transition-shadow border-slate-200 group">
                                <div className="p-6">
                                    <div className="flex items-start justify-between mb-4">
                                        <div className="flex items-center gap-2">
                                            <div className={cn(
                                                "flex items-center gap-2 px-3 py-1 rounded-full text-xs font-bold uppercase tracking-wider border",
                                                status.bg, status.text, status.border
                                            )}>
                                                {status.icon}
                                                {status.label}
                                            </div>
                                            <div className={cn(
                                                "text-[10px] font-black uppercase tracking-tighter px-2 py-1 rounded-md border",
                                                isWorkshop ? "bg-amber-50 text-amber-600 border-amber-200" : "bg-blue-50 text-blue-600 border-blue-200"
                                            )}>
                                                {isWorkshop ? t('career.workshop', 'Workshop') : t('events.event', 'Event')}
                                            </div>
                                        </div>
                                        <span className="text-xs text-slate-400 font-medium">
                                            {reg.creationTime ? new Date(reg.creationTime).toLocaleDateString() : ''}
                                        </span>
                                    </div>

                                    <h3 className={cn(
                                        "text-xl font-bold text-slate-900 mb-4 transition-colors",
                                        isWorkshop ? "group-hover:text-amber-600" : "group-hover:text-blue-600"
                                    )}>
                                        {reg.eventName || (isWorkshop ? t('career.unnamed_workshop', 'Unnamed Workshop') : t('events.unnamed_event', 'Unnamed Event'))}
                                    </h3>

                                    <div className="space-y-3 mb-6">
                                        <div className="flex items-center gap-3 text-sm text-slate-600">
                                            <Calendar className={cn("w-4 h-4", isWorkshop ? "text-amber-500" : "text-blue-500")} />
                                            <span>{reg.eventDate ? new Date(reg.eventDate as string).toLocaleDateString() : t('events.date_tba', 'Date TBA')}</span>
                                        </div>
                                        <div className="flex items-center gap-3 text-sm text-slate-600">
                                            <MapPin className={cn("w-4 h-4", isWorkshop ? "text-amber-500" : "text-blue-500")} />
                                            <span>{reg.location || t('events.location_tba', 'Location TBA')}</span>
                                        </div>
                                        
                                        {isWorkshop && reg.lecturerName && (
                                            <div className="flex items-center gap-3 text-sm text-slate-600">
                                                <UserCircle className="w-4 h-4 text-amber-500" />
                                                <span>{t('career.lecturer', 'Lecturer')}: {reg.lecturerName}</span>
                                            </div>
                                        )}

                                        {reg.paidAmount > 0 && (
                                            <div className="flex items-center gap-3 text-sm font-bold text-emerald-600 bg-emerald-50 px-3 py-2 rounded-lg w-fit">
                                                <div className="w-2 h-2 rounded-full bg-emerald-500" />
                                                {t('events.paid', 'Paid')}: {reg.paidAmount} EGP
                                            </div>
                                        )}
                                    </div>

                                    <div className="flex items-center gap-3">
                                        {reg.status === RegistrationStatus.Confirmed ? (
                                            <Button 
                                                className={cn(
                                                    "flex-1 gap-2 shadow-sm",
                                                    isWorkshop ? "bg-amber-600 hover:bg-amber-700" : ""
                                                )}
                                                onClick={() => setSelectedTicket(reg)}
                                            >
                                                <QrCode className="w-4 h-4" />
                                                {t('events.view_ticket', 'View Ticket')}
                                            </Button>
                                        ) : (
                                            <Button variant="outline" className="flex-1 opacity-50 cursor-not-allowed" disabled>
                                                {t('events.ticket_pending', 'Ticket Processing...')}
                                            </Button>
                                        )}
                                        <Button 
                                            variant="ghost" 
                                            size="sm"
                                            onClick={() => {
                                                const path = isWorkshop ? `/portal/career/${reg.eventId}` : `/portal/events/${reg.eventId}`;
                                                window.location.href = path;
                                            }}
                                        >
                                            {isWorkshop ? t('career.workshop_details', 'Workshop Info') : t('events.view_details', 'Event Details')}
                                        </Button>
                                    </div>
                                </div>
                            </Card>
                        );
                    })}
                </div>
            )}

            <Dialog open={!!selectedTicket} onOpenChange={(open) => !open && setSelectedTicket(null)}>
                <DialogContent className="sm:max-w-[440px] max-h-[95vh] overflow-y-auto bg-zinc-950 border-zinc-800 text-white p-0 shadow-[0_0_50px_-12px_rgba(59,130,246,0.3)] rounded-[2rem] scrollbar-hide">
                    {/* Ticket Header with Dynamic Accent */}
                    <div className={cn(
                        "relative p-8 pb-4 bg-gradient-to-b from-transparent to-transparent",
                        selectedTicket?.type === RegistrationType.Workshop ? "from-amber-600/10" : "from-blue-600/10"
                    )}>
                        <div className="absolute top-0 left-0 w-full h-full bg-[url('https://www.transparenttextures.com/patterns/carbon-fibre.png')] opacity-5 pointer-events-none"></div>
                        <DialogHeader className="text-center relative z-10">
                            <div className={cn(
                                "w-20 h-20 rounded-2xl flex items-center justify-center mx-auto mb-6 rotate-3 border border-white/10 group transition-all duration-500",
                                selectedTicket?.type === RegistrationType.Workshop 
                                    ? "bg-gradient-to-br from-amber-500 to-orange-600 shadow-[0_0_30px_rgba(245,158,11,0.5)]" 
                                    : "bg-gradient-to-br from-blue-500 to-indigo-600 shadow-[0_0_30px_rgba(59,130,246,0.5)]"
                            )}>
                                {selectedTicket?.type === RegistrationType.Workshop ? (
                                    <GraduationCap className="w-12 h-12 text-white -rotate-3 group-hover:scale-110 transition-transform duration-500" />
                                ) : (
                                    <Ticket className="w-12 h-12 text-white -rotate-3 group-hover:scale-110 transition-transform duration-500" />
                                )}
                            </div>
                            <DialogTitle className="text-3xl font-black tracking-tighter text-white uppercase italic leading-none drop-shadow-lg">
                                {selectedTicket?.type === RegistrationType.Workshop 
                                    ? t('career.workshop_pass', 'Workshop Pass') 
                                    : t('events.admission_ticket', 'Admission Ticket')}
                            </DialogTitle>
                            <DialogDescription className={cn(
                                "font-bold uppercase tracking-[0.2em] text-[10px] mt-2 flex items-center justify-center gap-2",
                                selectedTicket?.type === RegistrationType.Workshop ? "text-amber-400" : "text-blue-400"
                            )}>
                                <span className={cn("h-px w-8", selectedTicket?.type === RegistrationType.Workshop ? "bg-amber-500/30" : "bg-blue-500/30")}></span>
                                {selectedTicket?.eventName}
                                <span className={cn("h-px w-8", selectedTicket?.type === RegistrationType.Workshop ? "bg-amber-500/30" : "bg-blue-500/30")}></span>
                            </DialogDescription>
                        </DialogHeader>

                        {/* QR Code Container */}
                        <div className="mt-8 bg-white p-6 rounded-[2rem] shadow-[inset_0_2px_10px_rgba(0,0,0,0.1),0_20px_40px_rgba(0,0,0,0.4)] flex flex-col items-center relative group overflow-hidden">
                            {/* Watermark Stamp */}
                            <div className="absolute -top-4 -right-4 w-24 h-24 border-4 border-emerald-500/20 rounded-full flex items-center justify-center -rotate-12 pointer-events-none">
                                <span className="text-[8px] font-black text-emerald-500/40 uppercase text-center leading-tight">
                                    Authorized<br/>Entry<br/>{new Date().getFullYear()}
                                </span>
                            </div>

                            <div className="bg-slate-50 p-4 rounded-2xl border border-slate-100 mb-5 group-hover:scale-[1.02] transition-transform duration-500">
                                <img 
                                    src={selectedTicket?.qrCodeUrl} 
                                    alt="Ticket QR Code" 
                                    className="w-52 h-52 mix-blend-multiply"
                                />
                            </div>
                            <div className="text-center">
                                <p className="text-zinc-900 font-mono font-black tracking-[0.3em] text-xl leading-none px-4 py-2 bg-zinc-100 rounded-lg border border-zinc-200 uppercase">
                                    {selectedTicket?.ticketCode?.substring(0, 8)}
                                </p>
                                <div className="flex items-center justify-center gap-2 mt-3">
                                    <div className={cn(
                                        "h-1.5 w-1.5 rounded-full animate-pulse",
                                        selectedTicket?.type === RegistrationType.Workshop ? "bg-amber-500" : "bg-blue-500"
                                    )}></div>
                                    <p className="text-zinc-400 text-[9px] font-black uppercase tracking-[0.2em]">
                                        {t('events.unique_pass_id', 'Unique Pass Authentication')}
                                    </p>
                                </div>
                            </div>
                        </div>
                    </div>

                    {/* Perforated Divider */}
                    <div className="relative h-12 flex items-center overflow-hidden">
                        <div className="absolute left-[-16px] w-8 h-8 rounded-full bg-zinc-950 shadow-inner"></div>
                        <div className="absolute right-[-16px] w-8 h-8 rounded-full bg-zinc-950 shadow-inner"></div>
                        <div className="w-full flex justify-between px-4 gap-2">
                             {[...Array(20)].map((_, i) => (
                                <div key={i} className="h-1 w-2 bg-zinc-800 rounded-full opacity-50"></div>
                             ))}
                        </div>
                    </div>

                    {/* Ticket Stub Area */}
                    <div className="px-8 pb-10 pt-2 bg-zinc-900/50">
                        <div className="grid grid-cols-2 gap-8 mb-6">
                            <div className="space-y-1">
                                <p className={cn(
                                    "text-[9px] font-black uppercase tracking-widest opacity-70",
                                    selectedTicket?.type === RegistrationType.Workshop ? "text-amber-500" : "text-blue-500"
                                )}>
                                    {t('events.attendee', 'Member Attendee')}
                                </p>
                                <p className="font-bold text-base text-zinc-100 truncate flex items-center gap-2">
                                    <UserCircle className="w-4 h-4 text-zinc-400" />
                                    Mohamed Atef
                                </p>
                            </div>
                            <div className="space-y-1">
                                <p className={cn(
                                    "text-[9px] font-black uppercase tracking-widest opacity-70",
                                    selectedTicket?.type === RegistrationType.Workshop ? "text-amber-500" : "text-blue-500"
                                )}>
                                    {t('events.tier', 'Access Tier')}
                                </p>
                                <p className="font-bold text-base text-zinc-100 flex items-center gap-2">
                                    <div className="w-2 h-2 rounded-full bg-amber-400 shadow-[0_0_8px_rgba(251,191,36,0.5)]"></div>
                                    Alumnus Pass
                                </p>
                            </div>
                        </div>
                        
                        {selectedTicket?.type === RegistrationType.Workshop && (
                            <div className="grid grid-cols-2 gap-8 mb-6 border-t border-white/5 pt-6">
                                <div className="space-y-1">
                                    <p className="text-[9px] font-black text-amber-500 uppercase tracking-widest opacity-70">
                                        {t('career.lecturer', 'Lecturer')}
                                    </p>
                                    <p className="font-bold text-sm text-zinc-300 truncate">
                                        {selectedTicket.lecturerName || 'TBA'}
                                    </p>
                                </div>
                                <div className="space-y-1">
                                    <p className="text-[9px] font-black text-amber-500 uppercase tracking-widest opacity-70">
                                        {t('career.room', 'Venue')}
                                    </p>
                                    <p className="font-bold text-sm text-zinc-300 truncate">
                                        {selectedTicket.room || 'TBA'}
                                    </p>
                                </div>
                            </div>
                        )}

                        <div className="mt-8 flex gap-3">
                            <Button 
                                className={cn(
                                    "flex-1 font-black uppercase tracking-tighter py-6 rounded-xl shadow-lg transition-all hover:-translate-y-0.5 active:translate-y-0",
                                    selectedTicket?.type === RegistrationType.Workshop 
                                        ? "bg-amber-500 text-zinc-950 hover:bg-amber-400 shadow-amber-500/20" 
                                        : "bg-white text-zinc-950 hover:bg-zinc-200 shadow-white/20"
                                )}
                                onClick={() => window.print()}
                            >
                                {t('events.print_ticket', 'Print Pass')}
                            </Button>
                            <Button 
                                variant="outline"
                                className="border-zinc-700 text-zinc-400 hover:bg-zinc-800 hover:text-white"
                                onClick={() => setSelectedTicket(null)}
                            >
                                {t('common.close', 'Dismiss')}
                            </Button>
                        </div>
                        
                        <p className="text-center text-[8px] text-zinc-600 mt-6 font-medium uppercase tracking-widest">
                            {t('events.ticket_footer', 'Non-transferable • Presentation required for entry')}
                        </p>
                    </div>
                </DialogContent>
            </Dialog>
        </div>
    );
};

export default MyRegistrations;
