import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { servicesAppService } from '../../../services/servicesService';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '../../ui/Dialog';
import { Button } from '../../ui/Button';
import { CreditCard, Loader2, CheckCircle, ShieldCheck, FileText } from 'lucide-react';
import { toast } from 'sonner';

interface CertificatePaymentModalProps {
    isOpen: boolean;
    onClose: () => void;
    certificateId: string;
    certificateName: string;
    amount: number;
}

export const CertificatePaymentModal: React.FC<CertificatePaymentModalProps> = ({ 
    isOpen, 
    onClose,
    certificateId,
    certificateName,
    amount
}) => {
    const { t } = useTranslation();
    const queryClient = useQueryClient();
    const [isSuccess, setIsSuccess] = useState(false);

    const checkoutMutation = useMutation({
        mutationFn: () => servicesAppService.payCertificate(certificateId, amount),
        onSuccess: () => {
            setIsSuccess(true);
            toast.success(t('services.certificates.payment_success', 'Certificate payment processed successfully!'));
            // Refresh the user's certificate requests natively
            queryClient.invalidateQueries({ queryKey: ['my-certificates'] });
        },
        onError: (err: any) => {
            toast.error(err?.response?.data?.error?.message || t('common.error', 'An error occurred during payment.'));
        }
    });

    const handleCheckout = () => {
        checkoutMutation.mutate();
    };

    const handleClose = () => {
        setIsSuccess(false);
        onClose();
    };

    return (
        <Dialog open={isOpen} onOpenChange={handleClose}>
            <DialogContent className="sm:max-w-md bg-white dark:bg-slate-900 border-slate-200 dark:border-slate-800 p-0 overflow-hidden">
                {!isSuccess ? (
                    <>
                        <div className="bg-slate-50 dark:bg-slate-800/50 p-6 border-b border-slate-200 dark:border-slate-800">
                            <DialogHeader>
                                <DialogTitle className="flex items-center gap-2 text-xl text-slate-900 dark:text-white">
                                    <CreditCard className="w-5 h-5 text-indigo-500" />
                                    {t('services.certificates.checkout_title', 'Secure Checkout')}
                                </DialogTitle>
                            </DialogHeader>
                            <p className="text-sm text-slate-500 dark:text-slate-400 mt-2">
                                {t('services.certificates.checkout_desc', 'Complete your secure payment to begin processing your official documents.')}
                            </p>
                        </div>

                        <div className="p-6 space-y-6">
                            {/* Order Summary */}
                            <div className="bg-white dark:bg-slate-900 rounded-xl border border-slate-200 dark:border-slate-800 overflow-hidden">
                                <div className="p-4 border-b border-slate-100 dark:border-slate-800 flex justify-between items-center gap-4">
                                    <span className="font-medium text-slate-700 dark:text-slate-300 flex items-center gap-2 truncate">
                                        <FileText className="w-4 h-4 text-slate-400 shrink-0" />
                                        <span className="truncate">{certificateName || 'Document Request'}</span>
                                    </span>
                                    <span className="font-bold text-slate-900 dark:text-white shrink-0">{amount} EGP</span>
                                </div>
                                <div className="p-4 border-t border-slate-200 dark:border-slate-800 bg-indigo-50/50 dark:bg-indigo-500/10 flex justify-between items-center">
                                    <span className="font-bold text-indigo-900 dark:text-indigo-100">Total Due</span>
                                    <span className="font-black text-xl text-indigo-600 dark:text-indigo-400">{amount} EGP</span>
                                </div>
                            </div>

                            {/* Mock Stripe Elements */}
                            <div className="space-y-3">
                                <label className="text-xs font-semibold uppercase tracking-wider text-slate-500 flex items-center gap-1.5">
                                    <ShieldCheck className="w-4 h-4 text-emerald-500" />
                                    Secure Payment Info
                                </label>
                                <div className="p-3 border border-slate-200 dark:border-slate-700 rounded-lg bg-slate-50 dark:bg-slate-800 flex items-center gap-3">
                                    <CreditCard className="w-5 h-5 text-slate-400" />
                                    <div className="flex-1 font-mono text-slate-700 dark:text-slate-300 tracking-widest text-sm">
                                        •••• •••• •••• 4242
                                    </div>
                                    <div className="text-xs text-slate-400 font-mono">12/28</div>
                                    <div className="text-xs text-slate-400 font-mono border-l border-slate-300 dark:border-slate-600 pl-3">CVC</div>
                                </div>
                                <p className="text-[10px] text-center text-slate-400">
                                    This is a secure, encrypted transaction. Your card details are never stored on our servers.
                                </p>
                            </div>

                            {/* Payment Button Overlay */}
                            <div className="relative">
                                <Button 
                                    onClick={handleCheckout} 
                                    disabled={checkoutMutation.isPending}
                                    className="relative z-10 w-full bg-indigo-600 hover:bg-indigo-700 text-white font-bold py-6 text-lg shadow-lg shadow-indigo-500/20 transition-all active:scale-[0.98]"
                                >
                                    {checkoutMutation.isPending ? (
                                        <>
                                            <Loader2 className="w-5 h-5 mr-2 animate-spin" />
                                            Processing Payment...
                                        </>
                                    ) : (
                                        `Pay ${amount} EGP`
                                    )}
                                </Button>
                                {/* Loading Overlay */}
                                {checkoutMutation.isPending && (
                                    <div className="absolute inset-0 z-20 flex items-center justify-center bg-white/50 dark:bg-slate-900/50 backdrop-blur-[1px] rounded-md cursor-not-allowed" />
                                )}
                            </div>
                        </div>
                    </>
                ) : (
                    /* Success State */
                    <div className="p-10 flex flex-col items-center justify-center text-center space-y-4 bg-emerald-50 dark:bg-emerald-900/10 animate-in fade-in zoom-in duration-300">
                        <div className="w-20 h-20 bg-emerald-100 dark:bg-emerald-500/20 rounded-full flex items-center justify-center mb-2">
                            <CheckCircle className="w-10 h-10 text-emerald-600 dark:text-emerald-400" />
                        </div>
                        <h2 className="text-2xl font-bold text-emerald-800 dark:text-emerald-400">Payment Successful!</h2>
                        <p className="text-emerald-600/80 dark:text-emerald-500/80 mb-6 font-medium">
                            Your payment of {amount} EGP has been received. Your request is now officially being processed by the administration.
                        </p>
                        <Button 
                            onClick={handleClose} 
                            className="bg-emerald-600 hover:bg-emerald-700 text-white font-bold px-8 shadow-md"
                        >
                            Return to My Requests
                        </Button>
                    </div>
                )}
            </DialogContent>
        </Dialog>
    );
};
