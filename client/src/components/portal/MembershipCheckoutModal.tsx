import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { servicesAppService } from '../../services/servicesService';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '../ui/Dialog';
import { Button } from '../ui/Button';
import { CreditCard, Loader2, CheckCircle, ShieldCheck } from 'lucide-react';
import { toast } from 'sonner';

interface MembershipCheckoutModalProps {
    isOpen: boolean;
    onClose: () => void;
}

export const MembershipCheckoutModal: React.FC<MembershipCheckoutModalProps> = ({ isOpen, onClose }) => {
    const { t } = useTranslation();
    const queryClient = useQueryClient();
    const [isSuccess, setIsSuccess] = useState(false);

    const checkoutMutation = useMutation({
        mutationFn: servicesAppService.processMembershipCheckout,
        onSuccess: () => {
            setIsSuccess(true);
            toast.success(t('services.membership.payment_success', 'Membership activated successfully!'));
            // Refresh the user profile & card data globally
            queryClient.invalidateQueries({ queryKey: ['my-card'] });
            queryClient.invalidateQueries({ queryKey: ['current-user'] });
            // Unlock dependent features natively in real-time
            queryClient.invalidateQueries({ queryKey: ['certificate-definitions'] });
            queryClient.invalidateQueries({ queryKey: ['syndicate-status'] });
            queryClient.invalidateQueries({ queryKey: ['membership-history'] });
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
                                    {t('services.membership.checkout_title', 'Membership Activation')}
                                </DialogTitle>
                            </DialogHeader>
                            <p className="text-sm text-slate-500 dark:text-slate-400 mt-2">
                                {t('services.membership.checkout_desc', 'Complete your secure payment to instantly activate your Alumni benefits.')}
                            </p>
                        </div>

                        <div className="p-6 space-y-6">
                            {/* Order Summary */}
                            <div className="bg-white dark:bg-slate-900 rounded-xl border border-slate-200 dark:border-slate-800 overflow-hidden">
                                <div className="p-4 border-b border-slate-100 dark:border-slate-800 flex justify-between items-center">
                                    <span className="font-medium text-slate-700 dark:text-slate-300">Standard Alumni Membership</span>
                                    <span className="font-bold text-slate-900 dark:text-white">100 EGP</span>
                                </div>
                                <div className="p-4 bg-slate-50 dark:bg-slate-800/30 flex justify-between items-center text-sm">
                                    <span className="text-slate-500">Taxes & Fees</span>
                                    <span className="text-slate-700 dark:text-slate-300">0.00 EGP</span>
                                </div>
                                <div className="p-4 border-t border-slate-200 dark:border-slate-800 bg-indigo-50/50 dark:bg-indigo-500/10 flex justify-between items-center">
                                    <span className="font-bold text-indigo-900 dark:text-indigo-100">Total</span>
                                    <span className="font-black text-xl text-indigo-600 dark:text-indigo-400">100 EGP</span>
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

                            <Button 
                                onClick={handleCheckout} 
                                disabled={checkoutMutation.isPending}
                                className="w-full bg-indigo-600 hover:bg-indigo-700 text-white font-bold py-6 text-lg shadow-lg shadow-indigo-500/20 transition-all active:scale-[0.98]"
                            >
                                {checkoutMutation.isPending ? (
                                    <>
                                        <Loader2 className="w-5 h-5 mr-2 animate-spin" />
                                        Processing Payment...
                                    </>
                                ) : (
                                    `Pay 100 EGP`
                                )}
                            </Button>
                        </div>
                    </>
                ) : (
                    /* Success State */
                    <div className="p-10 flex flex-col items-center justify-center text-center space-y-4 bg-emerald-50 dark:bg-emerald-900/10 animate-in fade-in zoom-in duration-300">
                        <div className="w-20 h-20 bg-emerald-100 dark:bg-emerald-500/20 rounded-full flex items-center justify-center mb-2">
                            <CheckCircle className="w-10 h-10 text-emerald-600 dark:text-emerald-400" />
                        </div>
                        <h2 className="text-2xl font-bold text-emerald-800 dark:text-emerald-400">Payment Successful!</h2>
                        <p className="text-emerald-600/80 dark:text-emerald-500/80 mb-6">
                            Your Alumni Membership is now fully active. You have been granted immediate access to all premium features, digital IDs, and benefits.
                        </p>
                        <Button 
                            onClick={handleClose} 
                            className="bg-emerald-600 hover:bg-emerald-700 text-white font-bold px-8"
                        >
                            Enter Portal
                        </Button>
                    </div>
                )}
            </DialogContent>
        </Dialog>
    );
};
