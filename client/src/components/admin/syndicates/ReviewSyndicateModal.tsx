import { useState } from 'react';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from '../../ui/Dialog';
import { Button } from '../../ui/Button';
import type { SyndicateSubscriptionAdmin } from '../../../types/syndicates';
import { SyndicateStatus } from '../../../types/syndicates';
import { Eye, User, CreditCard, Building2, MapPin, CheckCircle, XCircle } from 'lucide-react';
import { toast } from 'sonner';

interface ReviewSyndicateModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    subscription: SyndicateSubscriptionAdmin | null;
    onApprove: (id: string) => void;
    onReject: (id: string) => void;
    isApproving: boolean;
    isRejecting: boolean;
}

export function ReviewSyndicateModal({
    open,
    onOpenChange,
    subscription,
    onApprove,
    onReject,
    isApproving,
    isRejecting
}: ReviewSyndicateModalProps) {
    const [previewDoc, setPreviewDoc] = useState<{ id: string, name: string, url: string } | null>(null);

    if (!subscription) return null;

    const handleViewDocument = (docId: string, filename: string) => {
        try {
            // Get token from oidc storage
            const oidcStorageKey = `oidc.user:https://localhost:44386:Alumni_App`;
            const oidcStorage = localStorage.getItem(oidcStorageKey);
            let tokenParam = '';

            if (oidcStorage) {
                const user = JSON.parse(oidcStorage);
                if (user?.access_token) {
                    tokenParam = `?access_token=${user.access_token}`;
                }
            }

            // Construct direct backend URL (avoids blob memory issues and missing extensions)
            const viewUrl = `https://localhost:44386/api/app/syndicate-admin/document/${subscription.id}/${docId}${tokenParam}`;

            setPreviewDoc({ id: docId, name: filename, url: viewUrl });

        } catch (error) {
            console.error('View error:', error);
            toast.error('Failed to open document preview');
        }
    };

    const extractOriginalName = (blobName: string) => {
        if (!blobName?.startsWith('syndicate_doc_')) return blobName || 'Document';
        // Format: syndicate(0)_doc(1)_{subId}(2)_{guid}(3)_{originalName}(4+)
        const parts = blobName.split('_');
        if (parts.length >= 5) {
            return parts.slice(4).join('_');
        }
        return blobName;
    };

    return (
        <>
            <Dialog open={open} onOpenChange={onOpenChange}>
                <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
                    <DialogHeader>
                        <DialogTitle>Review Syndicate Application</DialogTitle>
                        <DialogDescription>
                            Verify the alumni identity and attached documents before approving.
                        </DialogDescription>
                    </DialogHeader>

                    <div className="space-y-6 py-4">
                        {/* Alumni Details Card */}
                        <div className="bg-slate-50 dark:bg-white/5 rounded-lg border border-slate-200 dark:border-white/10 p-5">
                            <h3 className="text-sm font-semibold text-slate-800 dark:text-slate-200 mb-4 flex items-center gap-2">
                                <User className="w-4 h-4 text-blue-500" />
                                Applicant Information
                            </h3>
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <span className="text-xs text-slate-500 block mb-1">Full Name</span>
                                    <span className="font-medium text-slate-900 dark:text-white">{subscription.alumniName}</span>
                                </div>
                                <div>
                                    <span className="text-xs text-slate-500 block mb-1">National ID</span>
                                    <span className="font-medium text-slate-900 dark:text-white font-mono">{subscription.alumniNationalId}</span>
                                </div>
                                <div>
                                    <span className="text-xs text-slate-500 block mb-1">Mobile</span>
                                    <span className="font-medium text-slate-900 dark:text-white">{subscription.alumniMobile}</span>
                                </div>
                                <div>
                                    <span className="text-xs text-slate-500 block mb-1">Syndicate</span>
                                    <span className="font-medium text-slate-900 dark:text-white flex items-center gap-1">
                                        <Building2 className="w-3.5 h-3.5 text-slate-400" />
                                        {subscription.syndicateName}
                                    </span>
                                </div>
                            </div>
                        </div>

                        {/* Meta Info */}
                        <div className="grid grid-cols-2 gap-4">
                            <div className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-white/10 p-4">
                                <h3 className="text-xs font-semibold text-slate-500 uppercase tracking-wider mb-3 flex items-center gap-1.5">
                                    <CreditCard className="w-4 h-4" />
                                    Payment Details
                                </h3>
                                <div className="space-y-2">
                                    <div className="flex justify-between">
                                        <span className="text-sm text-slate-600 dark:text-slate-400">Total Fee</span>
                                        <span className="font-medium text-slate-900 dark:text-white">${subscription.feeAmount.toFixed(2)}</span>
                                    </div>
                                    <div className="flex justify-between">
                                        <span className="text-sm text-slate-600 dark:text-slate-400">Status</span>
                                        <span className={`font-medium ${subscription.paymentStatus === 1 ? 'text-green-600' : 'text-amber-600'}`}>
                                            {subscription.paymentStatus === 1 ? 'Paid' : subscription.paymentStatus === 2 ? 'Failed' : 'Pending'}
                                        </span>
                                    </div>
                                </div>
                            </div>

                            <div className="bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-white/10 p-4">
                                <h3 className="text-xs font-semibold text-slate-500 uppercase tracking-wider mb-3 flex items-center gap-1.5">
                                    <MapPin className="w-4 h-4" />
                                    Delivery Method
                                </h3>
                                <div className="flex items-center gap-2">
                                    <span className="font-medium text-slate-900 dark:text-white">
                                        {subscription.deliveryMethod === 0 ? 'Office Pickup' : 'Courier Delivery'}
                                    </span>
                                </div>
                            </div>
                        </div>

                        {/* Documents List */}
                        <div>
                            <h3 className="text-sm font-semibold text-slate-800 dark:text-slate-200 mb-3 border-b border-slate-200 dark:border-white/10 pb-2">
                                Uploaded Documents
                            </h3>
                            {subscription.documents.length === 0 ? (
                                <div className="text-sm text-slate-500 italic p-4 bg-slate-50 dark:bg-white/5 rounded-lg border border-dashed border-slate-200">
                                    No documents were uploaded by this applicant.
                                </div>
                            ) : (
                                <ul className="space-y-2">
                                    {subscription.documents.map((doc) => {
                                        const originalName = extractOriginalName(doc.fileBlobName);
                                        return (
                                            <li key={doc.id} className="flex items-center justify-between p-3 bg-white dark:bg-slate-900 border border-slate-200 dark:border-white/10 rounded-lg hover:bg-slate-50 dark:hover:bg-white/5 transition-colors">
                                                <div className="flex items-center gap-3">
                                                    <div className="w-8 h-8 rounded-full bg-blue-50 dark:bg-blue-500/10 flex items-center justify-center text-blue-600">
                                                        <Eye className="w-4 h-4" />
                                                    </div>
                                                    <div>
                                                        <p className="text-sm font-medium text-slate-900 dark:text-white">{doc.requirementName}</p>
                                                        <p className="text-xs text-slate-500 font-mono mt-0.5" title={originalName}>
                                                            {originalName.length > 35 ? originalName.substring(0, 32) + '...' : originalName}
                                                        </p>
                                                    </div>
                                                </div>
                                                <Button
                                                    size="sm"
                                                    variant="outline"
                                                    onClick={() => handleViewDocument(doc.id, originalName)}
                                                >
                                                    View
                                                </Button>
                                            </li>
                                        );
                                    })}
                                </ul>
                            )}
                        </div>
                    </div>

                    {subscription.status === SyndicateStatus.Pending && (
                        <div className="flex justify-end gap-3 pt-4 border-t border-slate-200 dark:border-white/10">
                            <Button
                                variant="outline"
                                className="text-red-600 hover:text-red-700 hover:bg-red-50"
                                onClick={() => onReject(subscription.id)}
                                disabled={isApproving || isRejecting}
                            >
                                <XCircle className="w-4 h-4 mr-2" />
                                Reject Application
                            </Button>
                            <Button
                                onClick={() => onApprove(subscription.id)}
                                isLoading={isApproving}
                                disabled={isApproving || isRejecting}
                            >
                                <CheckCircle className="w-4 h-4 mr-2" />
                                Approve &amp; Start Processing
                            </Button>
                        </div>
                    )}
                </DialogContent>
            </Dialog>

            {/* Document Preview Gallery Dialog */}
            <Dialog open={!!previewDoc} onOpenChange={(open) => !open && setPreviewDoc(null)}>
                <DialogContent className="max-w-4xl max-h-[90vh] flex flex-col p-0 overflow-hidden bg-slate-900 border-none">
                    <DialogHeader className="p-4 bg-slate-900/80 absolute top-0 left-0 right-0 z-10">
                        <DialogTitle className="text-white drop-shadow-md">{previewDoc?.name}</DialogTitle>
                    </DialogHeader>
                    <div className="flex-1 overflow-auto flex items-center justify-center p-4 pt-16 bg-slate-950/90 min-h-[50vh]">
                        {previewDoc?.name.toLowerCase().endsWith('.pdf') ? (
                            <iframe
                                src={previewDoc.url}
                                className="w-full h-[75vh] bg-white rounded-md border-0"
                                title={previewDoc.name}
                            />
                        ) : (
                            previewDoc?.url && (
                                <img
                                    src={previewDoc.url}
                                    alt={previewDoc.name}
                                    className="max-w-full max-h-[75vh] object-contain rounded-md shadow-2xl"
                                />
                            )
                        )}
                    </div>
                </DialogContent>
            </Dialog>
        </>
    );
}
