import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '../../ui/Dialog';
import { Button } from '../../ui/Button';
import { CertificateRequestStatus, CertificateLanguage, type CertificateRequestDto, type CertificateRequestItemDto } from '../../../types/certificates';
import { FileBadge, User, Mail, Hash, Truck, Package, CreditCard, Check, Loader2, Calendar, FileText, Download, Info } from 'lucide-react';
import { StatusBadge } from '../StatusBadge';

interface ReviewCertificateModalProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    request: CertificateRequestDto | null;
    onApprove: (id: string) => void;
    onReject: (id: string) => void;
    isApproving?: boolean;
    isRejecting?: boolean;
}

export const ReviewCertificateModal = ({
    open,
    onOpenChange,
    request,
    onApprove,
    onReject,
    isApproving = false,
    isRejecting = false
}: ReviewCertificateModalProps) => {
    if (!request) return null;

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="max-w-3xl max-h-[90vh] overflow-y-auto">
                <DialogHeader className="border-b border-slate-200 pb-4">
                    <div className="flex items-center justify-between">
                        <div>
                            <DialogTitle className="text-xl font-bold flex items-center gap-2">
                                <FileText className="w-5 h-5 text-primary" />
                                Review Certificate Request
                            </DialogTitle>
                            <DialogDescription className="mt-1 flex items-center gap-2">
                                Request <span className="font-mono bg-slate-100 text-slate-600 px-1 py-0.5 rounded text-xs select-all">{request.id}</span>
                            </DialogDescription>
                        </div>
                        <StatusBadge variant={
                            request.status === CertificateRequestStatus.Delivered ? 'success' :
                                request.status === CertificateRequestStatus.Rejected ? 'destructive' :
                                    request.status === CertificateRequestStatus.ReadyForPickup ? 'info' :
                                        request.status === CertificateRequestStatus.Processing ? 'warning' : 'default'
                        }>
                            {Object.keys(CertificateRequestStatus).find(k => CertificateRequestStatus[k as keyof typeof CertificateRequestStatus] === request.status) || 'Unknown'}
                        </StatusBadge>
                    </div>
                </DialogHeader>

                <div className="py-4 space-y-6">
                    {/* Requester Profile */}
                    <div className="bg-slate-50 border border-slate-200 rounded-xl p-5">
                        <h4 className="text-sm font-semibold text-slate-900 mb-3 flex items-center gap-1.5">
                            <User className="w-4 h-4 text-primary" />
                            Requester Information
                        </h4>
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <div>
                                <p className="text-xs text-slate-500 mb-1">Full Name</p>
                                <p className="text-sm font-medium text-slate-900">{request.alumniName || 'Unknown Alumni'}</p>
                            </div>
                            <div>
                                <p className="text-xs text-slate-500 mb-1">Student ID</p>
                                <p className="text-sm font-medium text-slate-900 flex items-center gap-1">
                                    <Hash className="w-3.5 h-3.5 text-slate-400" />
                                    {request.studentId || 'N/A'}
                                </p>
                            </div>
                            <div>
                                <p className="text-xs text-slate-500 mb-1">Academic Status</p>
                                <p className="text-sm font-medium text-slate-900">
                                    {request.collegeName || 'N/A'}
                                    {request.graduationYear ? ` (Class of ${request.graduationYear})` : ''}
                                </p>
                            </div>
                            <div>
                                <p className="text-xs text-slate-500 mb-1">Contact Details</p>
                                <div className="flex flex-col gap-1">
                                    <p className="text-sm font-medium text-slate-900 flex items-center gap-1.5">
                                        <Mail className="w-3.5 h-3.5 text-slate-400" />
                                        {request.alumniEmail || 'No Email'}
                                    </p>
                                    <p className="text-sm font-medium text-slate-900 flex items-center gap-1.5">
                                        <Hash className="w-3.5 h-3.5 text-slate-400" />
                                        {request.mobileNumber ? `+${request.mobileNumber.replace('+', '')}` : 'No Mobile'}
                                    </p>
                                </div>
                            </div>
                        </div>
                    </div>

                    {/* Request Items */}
                    <div className="bg-slate-50 border border-slate-200 rounded-xl p-5">
                        <h4 className="text-sm font-semibold text-slate-900 mb-4 flex items-center gap-1.5">
                            <FileBadge className="w-4 h-4 text-emerald-600" />
                            Requested Documents ({request.items?.length || 0})
                        </h4>
                        <div className="space-y-4">
                            {request.items?.map((item: CertificateRequestItemDto) => (
                                <div key={item.id} className="flex flex-col p-4 bg-white border border-slate-200 rounded-lg gap-4">
                                    <div className="flex flex-col sm:flex-row sm:items-start justify-between gap-3">
                                        <div>
                                            <p className="font-semibold text-slate-900">{item.certificateDefinitionName}</p>
                                            <div className="flex items-center gap-2 mt-1">
                                                {item.qualificationName && (
                                                    <span className="text-xs text-slate-600 font-medium">{item.qualificationName}</span>
                                                )}
                                            </div>
                                        </div>
                                        <div className="flex flex-wrap items-center gap-3">
                                            {item.attachmentUrl && (
                                                <Button size="sm" variant="outline" className="h-7 text-xs bg-slate-50 border-slate-200 hover:bg-slate-100" asChild>
                                                    <a href={item.attachmentUrl} target="_blank" rel="noopener noreferrer">
                                                        <Download className="w-3 h-3 mr-1" /> View Proof
                                                    </a>
                                                </Button>
                                            )}
                                            <span className="px-2 py-1 rounded bg-indigo-50 text-indigo-700 font-medium text-xs border border-indigo-100">
                                                {item.language === CertificateLanguage.Arabic ? 'Arabic' : 'English'}
                                            </span>
                                            <div className="flex flex-col items-end min-w-[60px]">
                                                <span className="text-xs text-slate-500">Item Fee</span>
                                                <span className="font-semibold text-slate-900">EGP {item.fee}</span>
                                            </div>
                                        </div>
                                    </div>

                                    {/* Additional Requirements Block */}
                                    {item.requiredDocuments && (
                                        <div className="mt-1 p-3 bg-blue-50/50 border border-blue-100 rounded text-sm relative">
                                            <div className="flex items-center gap-1.5 text-blue-800 font-semibold mb-2 text-xs uppercase tracking-wide">
                                                <Info className="w-3.5 h-3.5" />
                                                Requirement Rules
                                            </div>
                                            <div
                                                className="prose prose-sm max-w-none text-slate-700 prose-p:my-1 prose-ul:my-1 prose-li:my-0.5"
                                                dangerouslySetInnerHTML={{ __html: item.requiredDocuments }}
                                            />
                                        </div>
                                    )}
                                </div>
                            ))}
                            {request.userNotes && (
                                <div className="mt-3 p-3 bg-amber-50 border border-amber-200 rounded-lg">
                                    <p className="text-xs font-semibold text-amber-800 mb-1">User Notes :</p>
                                    <p className="text-sm text-amber-900">{request.userNotes}</p>
                                </div>
                            )}
                        </div>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                        {/* Financials */}
                        <div className="bg-slate-50 border border-slate-200 rounded-xl p-5">
                            <h4 className="text-sm font-semibold text-slate-900 mb-3 flex items-center gap-1.5">
                                <CreditCard className="w-4 h-4 text-blue-600" />
                                Financial Summary
                            </h4>
                            <div className="space-y-3 text-sm">
                                <div className="flex justify-between items-center text-slate-600">
                                    <span>Total Value</span>
                                    <span className="font-medium">EGP {request.totalFees}</span>
                                </div>
                                <div className="flex justify-between items-center text-emerald-600">
                                    <span>Wallet Applied</span>
                                    <span className="font-medium">- EGP {request.usedWalletAmount ?? 0}</span>
                                </div>
                                <div className="flex justify-between items-center text-green-600 border-b border-slate-200 pb-3">
                                    <span>Paid Online</span>
                                    <span className="font-medium">- EGP {request.paidGatewayAmount ?? 0}</span>
                                </div>
                                <div className="flex justify-between items-center pt-1">
                                    <span className="font-semibold text-slate-900">Remaining Balance</span>
                                    {request.remainingAmount > 0 ? (
                                        <span className="font-bold text-red-600">EGP {request.remainingAmount}</span>
                                    ) : (
                                        <span className="font-bold text-emerald-600 flex items-center gap-1">
                                            <Check className="w-4 h-4" /> Paid in Full
                                        </span>
                                    )}
                                </div>
                            </div>
                        </div>

                        {/* Delivery */}
                        <div className="bg-slate-50 border border-slate-200 rounded-xl p-5">
                            <h4 className="text-sm font-semibold text-slate-900 mb-3 flex items-center gap-1.5">
                                {request.deliveryMethod === 2 ? <Truck className="w-4 h-4 text-purple-600" /> : <Package className="w-4 h-4 text-purple-600" />}
                                Delivery Details
                            </h4>
                            <div className="bg-white border border-slate-200 p-3 rounded-lg">
                                <div className="flex items-center gap-2 mb-2">
                                    <span className="px-2 py-0.5 rounded bg-slate-100 text-slate-700 text-xs font-semibold">
                                        {request.deliveryMethod === 2 ? 'Courier Delivery' : 'Office Pickup'}
                                    </span>
                                </div>
                                {request.deliveryMethod === 2 ? (
                                    <>
                                        <p className="text-xs text-slate-500 mb-1">Shipping Address</p>
                                        <p className="text-sm font-medium text-slate-900">{request.deliveryAddress || 'Address not provided'}</p>
                                    </>
                                ) : (
                                    <>
                                        <p className="text-xs text-slate-500 mb-1">Pickup Branch</p>
                                        <p className="text-sm font-medium text-slate-900">{request.targetBranchName || 'Not selected'}</p>
                                    </>
                                )}
                            </div>
                            <div className="mt-4 text-xs text-slate-500 flex items-center gap-2">
                                <Calendar className="w-4 h-4" />
                                Requested on {new Date(request.creationTime || Date.now()).toLocaleDateString('en-US', {
                                    year: 'numeric', month: 'long', day: 'numeric', hour: '2-digit', minute: '2-digit'
                                })}
                            </div>
                        </div>
                    </div>
                </div>

                <DialogFooter className="border-t border-slate-200 pt-4 px-1 gap-2 sm:space-x-2">
                    <Button variant="outline" onClick={() => onOpenChange(false)}>
                        Close
                    </Button>
                    {request.status === CertificateRequestStatus.PendingPayment && (
                        <>
                            <Button
                                variant="destructive"
                                onClick={() => onReject(request.id)}
                                disabled={isRejecting || isApproving}
                                className="sm:mr-auto"
                            >
                                Reject Request
                            </Button>
                            <Button
                                variant="default"
                                onClick={() => onApprove(request.id)}
                                disabled={isRejecting || isApproving || request.remainingAmount > 0}
                                title={request.remainingAmount > 0 ? 'Cannot process unpaid requests' : ''}
                            >
                                {isApproving ? <Loader2 className="w-4 h-4 mr-2 animate-spin" /> : null}
                                Approve & Start Processing
                            </Button>
                        </>
                    )}
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
};
