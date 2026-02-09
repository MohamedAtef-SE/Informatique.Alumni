import { useState, cloneElement, type ReactElement } from 'react';
import { useQuery } from '@tanstack/react-query';
import { servicesAppService } from '../../services/servicesService';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '../ui/Dialog';
import { Button } from '../ui/Button';
import { CreditCard, Lock } from 'lucide-react';
import { useTranslation } from 'react-i18next';

interface MembershipGuardProps {
    children: ReactElement<any>;
    onActive?: () => void;
}

export const MembershipGuard = ({ children, onActive }: MembershipGuardProps) => {
    const { t } = useTranslation();
    const [showModal, setShowModal] = useState(false);

    // Check membership status (reusing existing endpoint)
    const { data: card } = useQuery({
        queryKey: ['my-card'],
        queryFn: servicesAppService.getCard,
        retry: false,
        staleTime: 5 * 60 * 1000 // Cache for 5 minutes
    });

    const handleClick = (e: any) => {
        // Stop immediate propagation to prevent parent handlers
        e.preventDefault();
        e.stopPropagation();

        if (card?.isActive) {
            // Member is active - proceed
            if (onActive) {
                onActive();
            } else if (children.props && typeof children.props.onClick === 'function') {
                children.props.onClick(e);
            }
        } else {
            // Member is NOT active - show friendly message
            setShowModal(true);
        }
    };

    return (
        <>
            {cloneElement(children, { onClick: handleClick } as any)}

            <Dialog open={showModal} onOpenChange={setShowModal}>
                <DialogContent className="sm:max-w-[425px]">
                    <DialogHeader>
                        <DialogTitle className="flex items-center gap-2 text-amber-600">
                            <Lock className="w-5 h-5" />
                            {t('membership.required_title', 'Active Membership Required')}
                        </DialogTitle>
                    </DialogHeader>

                    <div className="py-4 text-center space-y-4">
                        <div className="bg-amber-50 w-16 h-16 rounded-full flex items-center justify-center mx-auto">
                            <CreditCard className="w-8 h-8 text-amber-600" />
                        </div>

                        <p className="text-slate-600">
                            {t('membership.required_desc', 'This feature is exclusively available to active alumni members. Please renew your membership or apply for a new card to access this benefit.')}
                        </p>

                        <div className="bg-slate-50 p-3 rounded-lg text-xs text-slate-500 border border-slate-100">
                            {t('membership.benefits_teaser', 'Active members get access to Jobs, Guidance Sessions, Exclusive Discounts, Syndicate Services, and more!')}
                        </div>
                    </div>

                    <div className="flex gap-3 mt-4">
                        <Button variant="outline" onClick={() => setShowModal(false)} className="flex-1">
                            {t('common.close', 'Close')}
                        </Button>
                        <Button
                            className="flex-1 bg-amber-600 hover:bg-amber-700 text-white"
                            onClick={() => {
                                setShowModal(false);
                                // Navigate to Membership tab
                                const membershipTab = document.getElementById('tab-membership');
                                if (membershipTab) membershipTab.click();
                                // Or use router navigation if tabs allow deep linking, but current impl uses local state
                                // Assuming ServicesLayout handles tab switching via state, we might need a better way if this component is deep.
                                // For now, simple Close is fine, or context awareness.
                                // Quick hack: Dispatch a custom event or use an injected navigator.
                                // Since we are inside ServicesLayout mostly, we can just close.
                                // To actally switch tab, we'd need access to setActiveTab from parent.
                                // For now, "Apply/Renew" button can just close and maybe show a toast "Go to Membership Tab".
                                document.dispatchEvent(new CustomEvent('switch-tab', { detail: 'membership' }));
                            }}
                        >
                            {t('membership.apply_now', 'Apply / Renew')}
                        </Button>
                    </div>
                </DialogContent>
            </Dialog>
        </>
    );
};
