import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { alumniService } from '../../services/alumniService';
import { Wallet, CreditCard, ArrowUpRight, History, ShieldCheck, Info, Loader2 } from 'lucide-react';
import { cn } from '../../utils/cn';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/Card';
import { Button } from '../../components/ui/Button';
import { Input } from '../../components/ui/Input';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogFooter } from '../../components/ui/Dialog';
import { toast } from 'sonner';
import { useTranslation } from 'react-i18next';

const MyWallet = () => {
    const { t } = useTranslation();
    const queryClient = useQueryClient();
    const [isTopUpOpen, setIsTopUpOpen] = useState(false);
    const [amount, setAmount] = useState<string>('');
    const [step, setStep] = useState<'input' | 'processing' | 'success'>('input');

    const { data: profile, isLoading } = useQuery({
        queryKey: ['my-profile'],
        queryFn: () => alumniService.getMyProfile()
    });

    const { data: activity, isLoading: isActivityLoading } = useQuery({
        queryKey: ['wallet-activity'],
        queryFn: () => alumniService.getWalletActivity()
    });

    const topUpMutation = useMutation({
        mutationFn: (amt: number) => alumniService.topUpWallet(amt),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['my-profile'] });
            queryClient.invalidateQueries({ queryKey: ['wallet-activity'] });
            setStep('success');
            toast.success(t('wallet.topup_success', 'Wallet topped up successfully!'));
            setTimeout(() => {
                setIsTopUpOpen(false);
                setStep('input');
                setAmount('');
            }, 3000);
        },
        onError: () => {
            setStep('input');
            toast.error(t('wallet.topup_error', 'Failed to process payment. Please try again.'));
        }
    });

    const handleTopUp = () => {
        const numAmount = parseFloat(amount);
        if (isNaN(numAmount) || numAmount <= 0) {
            toast.error(t('wallet.invalid_amount', 'Please enter a valid amount.'));
            return;
        }

        setStep('processing');
        // Simulate network delay for realism
        setTimeout(() => {
            topUpMutation.mutate(numAmount);
        }, 2000);
    };

    if (isLoading) {
        return (
            <div className="flex flex-col items-center justify-center py-20 gap-4">
                <Loader2 className="w-10 h-10 text-blue-600 animate-spin" />
                <p className="text-slate-500 font-medium">{t('common.loading')}</p>
            </div>
        );
    }

    return (
        <div className="max-w-4xl mx-auto space-y-8 animate-in fade-in duration-500">
            {/* Header Section */}
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-6">
                <div>
                    <h1 className="text-3xl font-bold text-slate-900 flex items-center gap-3">
                        <div className="p-2 bg-blue-50 rounded-xl">
                            <Wallet className="w-8 h-8 text-blue-600" />
                        </div>
                        {t('wallet.title', 'My Digital Wallet')}
                    </h1>
                    <p className="text-slate-500 mt-2 font-medium">
                        {t('wallet.description', 'Manage your funds and pay for association services securely.')}
                    </p>
                </div>
                <Button 
                    size="lg" 
                    className="bg-blue-600 hover:bg-blue-700 shadow-lg shadow-blue-500/20"
                    onClick={() => setIsTopUpOpen(true)}
                >
                    <ArrowUpRight className="w-5 h-5 mr-2" />
                    {t('wallet.topup_btn', 'Add Funds')}
                </Button>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
                {/* Balance Card */}
                <Card className="lg:col-span-2 overflow-hidden border-none shadow-2xl bg-gradient-to-br from-slate-900 via-slate-800 to-blue-900 text-white">
                    <CardContent className="p-10 relative">
                        <div className="absolute top-0 right-0 p-8 opacity-10">
                            <Wallet className="w-32 h-32" />
                        </div>
                        
                        <div className="space-y-1">
                            <span className="text-blue-300 text-sm font-bold uppercase tracking-widest">
                                {t('wallet.current_balance', 'Available Balance')}
                            </span>
                            <div className="flex items-baseline gap-2">
                                <span className="text-6xl font-black">{profile?.walletBalance?.toLocaleString() || '0.00'}</span>
                                <span className="text-2xl font-bold text-blue-200">{t('common.currency', 'EGP')}</span>
                            </div>
                        </div>

                        <div className="mt-12 flex flex-wrap gap-4">
                            <div className="bg-white/10 backdrop-blur-md px-4 py-3 rounded-xl border border-white/10 flex items-center gap-3">
                                <ShieldCheck className="w-5 h-5 text-emerald-400" />
                                <span className="text-sm font-medium">{t('wallet.secure_status', 'Secured Account')}</span>
                            </div>
                            <div className="bg-white/10 backdrop-blur-md px-4 py-3 rounded-xl border border-white/10 flex items-center gap-3">
                                <CreditCard className="w-5 h-5 text-blue-300" />
                                <span className="text-sm font-medium">•••• 4242</span>
                            </div>
                        </div>
                    </CardContent>
                </Card>

                {/* Quick Info */}
                <Card className="border-slate-200 shadow-sm">
                    <CardHeader>
                        <CardTitle className="text-lg flex items-center gap-2">
                            <Info className="w-5 h-5 text-blue-500" />
                            {t('wallet.why_wallet_title', 'Why use the wallet?')}
                        </CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-4">
                        <div className="flex gap-3">
                            <div className="w-8 h-8 rounded-full bg-blue-50 flex items-center justify-center flex-shrink-0">
                                <span className="text-blue-600 font-bold text-xs">1</span>
                            </div>
                            <p className="text-sm text-slate-600 leading-relaxed">
                                {t('wallet.reason_1', 'Instant payment for events and certificates.')}
                            </p>
                        </div>
                        <div className="flex gap-3">
                            <div className="w-8 h-8 rounded-full bg-blue-50 flex items-center justify-center flex-shrink-0">
                                <span className="text-blue-600 font-bold text-xs">2</span>
                            </div>
                            <p className="text-sm text-slate-600 leading-relaxed">
                                {t('wallet.reason_2', 'Keep track of all association-related expenses in one place.')}
                            </p>
                        </div>
                        <div className="flex gap-3">
                            <div className="w-8 h-8 rounded-full bg-blue-50 flex items-center justify-center flex-shrink-0">
                                <span className="text-blue-600 font-bold text-xs">3</span>
                            </div>
                            <p className="text-sm text-slate-600 leading-relaxed">
                                {t('wallet.reason_3', 'Refunds are processed much faster to your digital wallet.')}
                            </p>
                        </div>
                    </CardContent>
                </Card>
            </div>

            {/* Recent Activity */}
            <Card className="border-slate-200">
                <CardHeader>
                    <CardTitle className="text-xl flex items-center gap-2">
                        <History className="w-6 h-6 text-slate-400" />
                        {t('wallet.recent_activity', 'Recent Activity')}
                    </CardTitle>
                </CardHeader>
                <CardContent className="pb-6">
                    {isActivityLoading ? (
                        <div className="flex justify-center py-10">
                            <Loader2 className="w-6 h-6 text-blue-600 animate-spin" />
                        </div>
                    ) : activity && activity.length > 0 ? (
                        <div className="divide-y divide-slate-100">
                            {activity.map((item) => (
                                <div key={item.id} className="py-4 flex items-center justify-between group hover:bg-slate-50/50 transition-colors px-2 rounded-lg">
                                    <div className="flex items-center gap-4">
                                        <div className={cn(
                                            "w-10 h-10 rounded-full flex items-center justify-center",
                                            item.type === 'Deposit' ? "bg-emerald-50 text-emerald-600" : "bg-slate-100 text-slate-600"
                                        )}>
                                            {item.type === 'Deposit' ? <ArrowUpRight className="w-5 h-5 rotate-45" /> : <CreditCard className="w-5 h-5" />}
                                        </div>
                                        <div>
                                            <p className="font-bold text-slate-800">{item.description}</p>
                                            <p className="text-xs text-slate-400 font-medium tracking-wide uppercase">
                                                {new Date(item.transactionDate).toLocaleDateString()} • {new Date(item.transactionDate).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                                            </p>
                                        </div>
                                    </div>
                                    <div className="text-right">
                                        <p className={cn(
                                            "text-lg font-black",
                                            item.type === 'Deposit' ? "text-emerald-600" : "text-slate-900"
                                        )}>
                                            {item.type === 'Deposit' ? '+' : '-'}{item.amount.toLocaleString()} <span className="text-xs font-bold opacity-50">EGP</span>
                                        </p>
                                        <p className="text-[10px] text-slate-300 font-bold uppercase tracking-widest">{item.type}</p>
                                    </div>
                                </div>
                            ))}
                        </div>
                    ) : (
                        <div className="flex flex-col items-center justify-center py-10 text-slate-400 space-y-3">
                            <div className="w-16 h-16 rounded-full bg-slate-50 flex items-center justify-center">
                                <History className="w-8 h-8 opacity-20" />
                            </div>
                            <p>{t('wallet.no_activity', 'No recent transactions to display.')}</p>
                        </div>
                    )}
                </CardContent>
            </Card>

            {/* Top Up Modal */}
            <Dialog open={isTopUpOpen} onOpenChange={setIsTopUpOpen}>
                <DialogContent className="max-w-md">
                    <DialogHeader>
                        <DialogTitle className="text-2xl font-bold text-slate-800">
                            {t('wallet.topup_modal_title', 'Add Funds to Wallet')}
                        </DialogTitle>
                        <DialogDescription>
                            {t('wallet.topup_modal_desc', 'Enter the amount you wish to add. You will be redirected to our secure mock payment provider.')}
                        </DialogDescription>
                    </DialogHeader>

                    {step === 'input' && (
                        <div className="space-y-6 pt-4">
                            <div className="grid grid-cols-3 gap-2">
                                {[100, 500, 1000].map(val => (
                                    <Button 
                                        key={val} 
                                        variant="outline" 
                                        className="h-12 border-slate-200 hover:border-blue-500 hover:bg-blue-50"
                                        onClick={() => setAmount(val.toString())}
                                    >
                                        +{val} EGP
                                    </Button>
                                ))}
                            </div>
                            <Input
                                label={t('wallet.amount_label', 'Custom Amount (EGP)')}
                                type="number"
                                placeholder="0.00"
                                value={amount}
                                onChange={(e) => setAmount(e.target.value)}
                                className="text-xl font-bold"
                                autoFocus
                            />
                            <DialogFooter className="pt-4">
                                <Button className="w-full" size="lg" onClick={handleTopUp}>
                                    {t('wallet.proceed_to_pay', 'Proceed to Payment')}
                                </Button>
                            </DialogFooter>
                        </div>
                    )}

                    {step === 'processing' && (
                        <div className="py-12 flex flex-col items-center justify-center space-y-6 text-center animate-in zoom-in duration-300">
                            <div className="relative">
                                <div className="w-20 h-20 border-4 border-blue-600/20 border-t-blue-600 rounded-full animate-spin"></div>
                                <ShieldCheck className="w-8 h-8 text-blue-600 absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2" />
                            </div>
                            <div>
                                <h3 className="text-xl font-bold text-slate-800">{t('wallet.processing_title', 'Securing Transaction...')}</h3>
                                <p className="text-slate-500 mt-1">{t('wallet.processing_desc', 'Connecting to payment gateway. Please do not refresh.')}</p>
                            </div>
                        </div>
                    )}

                    {step === 'success' && (
                        <div className="py-12 flex flex-col items-center justify-center space-y-6 text-center animate-in scale-in duration-500">
                            <div className="w-20 h-20 bg-emerald-100 rounded-full flex items-center justify-center shadow-lg shadow-emerald-500/20">
                                <ShieldCheck className="w-10 h-10 text-emerald-600" />
                            </div>
                            <div>
                                <h3 className="text-3xl font-black text-slate-800">{t('wallet.success_title', 'Success!')}</h3>
                                <p className="text-slate-600 mt-2 text-lg">
                                    {t('wallet.success_desc', 'Payment processed. {{amount}} EGP added to your balance.', { amount })}
                                </p>
                            </div>
                        </div>
                    )}
                </DialogContent>
            </Dialog>
        </div>
    );
};

export default MyWallet;
