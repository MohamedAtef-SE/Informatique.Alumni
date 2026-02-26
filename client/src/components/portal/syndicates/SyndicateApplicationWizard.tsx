
import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { servicesAppService } from '../../../services/servicesService'; // Adjust path
import { Button } from '../../ui/Button'; // Adjust path
import { toast } from 'sonner';
import { Loader2, CheckCircle2, Upload, CreditCard, ChevronRight } from 'lucide-react';
import clsx from 'clsx';
import { Card } from '../../ui/Card'; // Adjust path

interface Syndicate {
    id: string;
    name: string;
    description: string;
    requirements: string; // Comma separated?
    fee: number;
}

interface ApplicationWizardProps {
    onClose: () => void;
    onSuccess: () => void;
    existingSubscription?: any;
}

export default function SyndicateApplicationWizard({ onClose, onSuccess, existingSubscription }: ApplicationWizardProps) {
    const { t } = useTranslation();
    const queryClient = useQueryClient();

    // Determine initial step based on status
    // Draft (-1) -> Step 1 (Resume)
    // Pending (0) -> Step 2 (Uploads)
    // Reviewing (1) -> Step 2 (Uploads allowed)
    const initialStep = existingSubscription && (existingSubscription.status === 0 || existingSubscription.status === 1) ? 2 : 1;

    const [step, setStep] = useState(initialStep);
    const [selectedSyndicateId, setSelectedSyndicateId] = useState<string>(existingSubscription?.syndicateId || '');
    const [deliveryMethod, setDeliveryMethod] = useState(1); // 1: Office Pickup
    const [subscriptionId, setSubscriptionId] = useState<string | null>(existingSubscription?.id || null);
    // Actually UploadDocumentAsync requires file content as bytes? Or separate Blob upload?
    // Backend "UploadDocumentAsync" takes "FileContent" (byte[]).
    // This means we should upload directly to that endpoint.
    // BUT frontend usually uploads to a temp blob or sends base64.
    // servicesService.uploadSyndicateDocument takes input.
    // I'll implement file reading to base64/bytes.

    const { data: syndicates, isLoading: isLoadingSyndicates } = useQuery({
        queryKey: ['syndicates'],
        queryFn: servicesAppService.getSyndicates
    });

    const selectedSyndicate = syndicates?.find((s: Syndicate) => s.id === selectedSyndicateId);

    // Mutations
    const applyMutation = useMutation({
        mutationFn: servicesAppService.applySyndicate,
        onSuccess: (data) => {
            setSubscriptionId(data.id);
            setStep(2); // Move to Upload
        },
        onError: (err: any) => {
            toast.error(err.response?.data?.error?.message || t('services.syndicates.application_fail'));
        }
    });

    const uploadMutation = useMutation({
        mutationFn: async (file: File) => {
            if (!subscriptionId) return;
            // Convert to byte array / base64 strings?
            // The DTO expects byte[].
            // Let's assume the generated proxy or manual call handles standard JSON.
            // We need to read file as ArrayBuffer or Base64.
            // If API expects byte[], usually Base64 string in JSON works for ABP.

            return new Promise((resolve, reject) => {
                const reader = new FileReader();
                reader.readAsDataURL(file); // Base64
                reader.onload = async () => {
                    const base64 = (reader.result as string).split(',')[1];
                    try {
                        await servicesAppService.uploadSyndicateDocument(subscriptionId, {
                            requirementName: "Document", // Generic for now? Or ask user?
                            fileName: file.name,
                            fileContent: base64 // Check if backend maps this correctly.
                            // If backend expects byte[], ABP JSON deserializer handles Base64 string.
                        });
                        resolve(true);
                    } catch (e) {
                        reject(e);
                    }
                };
                reader.onerror = error => reject(error);
            });
        },
        onSuccess: () => {
            toast.success(t('services.syndicates.upload_success'));
            queryClient.invalidateQueries({ queryKey: ['syndicate-status'] });
        },
        onError: (err: any) => toast.error(err?.response?.data?.error?.message || t('services.syndicates.upload_fail'))
    });

    const payMutation = useMutation({
        mutationFn: () => servicesAppService.paySyndicate(subscriptionId!),
        onSuccess: () => {
            toast.success(t('services.syndicates.payment_success'));
            queryClient.invalidateQueries({ queryKey: ['syndicate-status'] });
            onSuccess();
        },
        onError: (err: any) => toast.error(err.response?.data?.error?.message || t('services.syndicates.payment_fail'))
    });

    // Step 1: Selection
    const handleCreateApplication = () => {
        if (!selectedSyndicateId) return;
        applyMutation.mutate({
            syndicateId: selectedSyndicateId,
            deliveryMethod: deliveryMethod
        });
    };

    // Step 2: Uploads
    const handleFileUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files && e.target.files[0]) {
            uploadMutation.mutate(e.target.files[0]);
        }
    };

    // Render Steps
    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4 animate-fade-in">
            <Card className="w-full max-w-2xl max-h-[90vh] overflow-y-auto bg-white border-none shadow-2xl">
                <div className="p-6 border-b border-gray-100 flex justify-between items-center sticky top-0 bg-white z-10">
                    <h2 className="text-xl font-bold text-gray-900">{t('services.syndicates.wizard_title', 'Syndicate Application')}</h2>
                    <button onClick={onClose} className="text-gray-400 hover:text-gray-600">×</button>
                </div>

                <div className="p-6 space-y-8">
                    {/* Stepper */}
                    <div className="flex justify-between relative">
                        <div className="absolute top-1/2 left-0 w-full h-0.5 bg-gray-200 -z-10"></div>
                        {[1, 2, 3].map((s) => (
                            <div key={s} className={clsx(
                                "w-10 h-10 rounded-full flex items-center justify-center font-bold text-sm transition-colors",
                                step >= s ? "bg-blue-600 text-white" : "bg-gray-100 text-gray-400"
                            )}>
                                {step > s ? <CheckCircle2 className="w-5 h-5" /> : s}
                            </div>
                        ))}
                    </div>

                    {/* Step 1 Content */}
                    {step === 1 && (
                        <div className="space-y-6 animate-slide-in-right">
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-2">{t('services.syndicates.select_syndicate')}</label>
                                {isLoadingSyndicates ? <Loader2 className="animate-spin" /> : (
                                    <div className="grid gap-3">
                                        {syndicates?.map((syn: Syndicate) => (
                                            <div
                                                key={syn.id}
                                                onClick={() => setSelectedSyndicateId(syn.id)}
                                                className={clsx(
                                                    "p-4 rounded-xl border-2 cursor-pointer transition-all hover:bg-blue-50",
                                                    selectedSyndicateId === syn.id ? "border-blue-600 bg-blue-50 ring-1 ring-blue-600" : "border-gray-200"
                                                )}
                                            >
                                                <div className="flex justify-between items-center">
                                                    <span className="font-bold text-gray-900">{syn.name}</span>
                                                    <span className="text-blue-600 font-bold">{syn.fee} EGP</span>
                                                </div>
                                                <p className="text-xs text-gray-500 mt-1">{syn.description}</p>
                                            </div>
                                        ))}
                                    </div>
                                )}
                            </div>

                            {selectedSyndicate && (
                                <div className="bg-slate-50 p-4 rounded-lg border border-slate-200 text-sm text-slate-700">
                                    <strong>{t('services.syndicates.requirements')}:</strong> {selectedSyndicate.requirements || "None"}
                                </div>
                            )}

                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-2">{t('services.certificates.delivery_label')}</label>
                                <div className="flex gap-4">
                                    <label className="flex items-center gap-2 cursor-pointer border p-3 rounded-lg flex-1 hover:bg-gray-50">
                                        <input type="radio" name="delivery" checked={deliveryMethod === 1} onChange={() => setDeliveryMethod(1)} className="w-4 h-4 text-blue-600" />
                                        {t('services.certificates.pickup')}
                                    </label>
                                    <label className="flex items-center gap-2 cursor-pointer border p-3 rounded-lg flex-1 hover:bg-gray-50">
                                        <input type="radio" name="delivery" checked={deliveryMethod === 2} onChange={() => setDeliveryMethod(2)} className="w-4 h-4 text-blue-600" />
                                        {t('services.certificates.home_delivery')}
                                    </label>
                                </div>
                            </div>

                            <Button
                                onClick={handleCreateApplication}
                                disabled={!selectedSyndicateId || applyMutation.isPending}
                                className="w-full py-3 text-lg"
                            >
                                {applyMutation.isPending ? <Loader2 className="animate-spin mr-2" /> : null}
                                {t('services.syndicates.create_application')}
                            </Button>
                        </div>
                    )}

                    {/* Step 2 Content: Uploads */}
                    {step === 2 && (
                        <div className="space-y-6 animate-slide-in-right">
                            <div className="text-center">
                                <Upload className="w-12 h-12 mx-auto text-blue-600 mb-2" />
                                <h3 className="text-lg font-bold">{t('services.syndicates.upload_docs')}</h3>
                                <p className="text-sm text-gray-500">{t('services.syndicates.upload_instruction')}</p>
                            </div>

                            <div className="bg-amber-50 border border-amber-200 p-4 rounded-lg text-sm text-amber-800">
                                <strong>Requirement:</strong> {selectedSyndicate?.requirements}
                            </div>

                            <div className="border-2 border-dashed border-gray-300 rounded-xl p-8 text-center hover:bg-gray-50 transition-colors relative cursor-pointer">
                                <input type="file" onChange={handleFileUpload} className="absolute inset-0 w-full h-full opacity-0 cursor-pointer" />
                                <p className="text-gray-600 font-medium">{t('services.syndicates.click_upload')}</p>
                                <p className="text-xs text-gray-400 mt-1">{t('services.syndicates.supported_formats')}</p>
                            </div>

                            {existingSubscription?.documents && existingSubscription.documents.length > 0 && (
                                <div className="space-y-2">
                                    <h4 className="font-semibold text-sm text-gray-700">{t('services.syndicates.uploaded_documents')}</h4>
                                    <div className="space-y-2">
                                        {existingSubscription.documents.map((doc: any) => (
                                            <div key={doc.id} className="flex justify-between items-center p-3 bg-gray-50 rounded-lg border border-gray-200">
                                                <div className="flex items-center gap-2">
                                                    <CheckCircle2 className="w-4 h-4 text-green-600" />
                                                    <span className="text-sm font-medium text-gray-700">{doc.requirementName}</span>
                                                </div>
                                                <a
                                                    href={servicesAppService.getSyndicateDocument(existingSubscription.id, doc.id)}
                                                    target="_blank"
                                                    rel="noopener noreferrer"
                                                    className="text-blue-600 hover:text-blue-800 text-sm font-medium"
                                                >
                                                    {t('common.view', 'View')}
                                                </a>
                                            </div>
                                        ))}
                                    </div>
                                </div>
                            )}

                            {uploadMutation.isPending && <div className="text-center text-blue-600 text-sm"><Loader2 className="inline w-4 h-4 animate-spin" /> {t('common.loading')}</div>}

                            <div className="flex gap-3">
                                <Button variant="outline" onClick={() => setStep(1)} disabled>{t('common.back')}</Button>
                                {existingSubscription?.paymentStatus === 1 ? (
                                    <div className="flex-1 flex items-center justify-center text-green-600 font-bold bg-green-50 rounded-lg">
                                        <CheckCircle2 className="w-5 h-5 mr-2" />
                                        {t('services.syndicates.payment_success', 'Payment Completed')}
                                    </div>
                                ) : (
                                    <Button onClick={() => setStep(3)} className="flex-1">
                                        {t('services.syndicates.continue_payment')} <ChevronRight className="w-4 h-4 ml-1" />
                                    </Button>
                                )}
                            </div>
                        </div>
                    )}

                    {/* Step 3 Content: Payment */}
                    {step === 3 && (
                        <div className="space-y-6 animate-slide-in-right">
                            <div className="text-center">
                                <CreditCard className="w-12 h-12 mx-auto text-blue-600 mb-2" />
                                <h3 className="text-lg font-bold">{t('services.syndicates.payment')}</h3>
                            </div>

                            <div className="bg-slate-50 border border-slate-200 p-6 rounded-xl space-y-4">
                                <div className="flex justify-between items-center">
                                    <span className="text-gray-600">{t('services.syndicates.total_fee')}</span>
                                    <span className="text-2xl font-bold text-gray-900">{selectedSyndicate?.fee} EGP</span>
                                </div>
                                <div className="text-sm text-gray-500 bg-blue-50 p-3 rounded-lg border border-blue-100">
                                    {t('services.syndicates.payment_note', 'This is a secure payment simulation. No actual charge will be made.')}
                                </div>
                            </div>

                            {existingSubscription?.paymentStatus === 1 ? (
                                <div className="text-center p-4 bg-green-50 rounded-xl border border-green-200 text-green-700 font-bold">
                                    <CheckCircle2 className="w-8 h-8 mx-auto mb-2" />
                                    {t('services.syndicates.payment_success')}
                                </div>
                            ) : (
                                <Button
                                    onClick={() => payMutation.mutate()}
                                    disabled={payMutation.isPending}
                                    className="w-full py-4 text-lg bg-green-600 hover:bg-green-700 text-white"
                                >
                                    {payMutation.isPending ? <Loader2 className="animate-spin mr-2" /> : null}
                                    {t('services.syndicates.pay_now')}
                                </Button>
                            )}
                        </div>
                    )}
                </div>
            </Card>
        </div>
    );
}
