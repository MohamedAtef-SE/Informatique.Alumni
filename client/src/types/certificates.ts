import type { FullAuditedEntityDto, EntityDto, PagedAndSortedResultRequestDto } from './common';
import type { DeliveryMethod } from './membership';

export const CertificateRequestStatus = {
    Draft: 1,
    PendingPayment: 2,
    Processing: 3,
    ReadyForPickup: 4,
    OutForDelivery: 5,
    Delivered: 6,
    Rejected: 7
} as const;
export type CertificateRequestStatus = typeof CertificateRequestStatus[keyof typeof CertificateRequestStatus];

export const CertificateLanguage = {
    Arabic: 0,
    English: 1
} as const;
export type CertificateLanguage = typeof CertificateLanguage[keyof typeof CertificateLanguage];

export interface CertificateRequestDto extends FullAuditedEntityDto<string> {
    // Requester Information
    alumniId: string;
    alumniName?: string;
    alumniEmail?: string;
    mobileNumber?: string;
    studentId?: string;
    collegeName?: string;
    graduationYear?: number;
    items: CertificateRequestItemDto[];
    status: CertificateRequestStatus;
    totalItemFees: number;
    deliveryFee: number;
    totalFees: number;
    usedWalletAmount: number;
    paidGatewayAmount: number;
    remainingAmount: number;
    deliveryMethod: DeliveryMethod;
    targetBranchName?: string;
    deliveryAddress?: string;
    adminNotes?: string;
    userNotes?: string;
}

export interface CertificateRequestItemDto extends EntityDto<string> {
    certificateDefinitionName: string;
    qualificationName?: string;
    language: CertificateLanguage;
    fee: number;

    // Proof mapping
    attachmentUrl?: string;
    requiredDocuments?: string;
}

export interface CertificateRequestFilterDto extends PagedAndSortedResultRequestDto {
    branchId?: string;
    status?: CertificateRequestStatus;
    filter?: string;
}

export interface UpdateCertificateStatusDto {
    newStatus: CertificateRequestStatus;
    note?: string;
}

// --- New Types for Availability Check ---

export interface CertificateDefinitionDto extends EntityDto<string> {
    nameAr: string;
    nameEn: string;
    fee: number;
    degreeType: number;
    description?: string;
    requiredDocuments?: string;
    isActive: boolean;
}

export interface CertificateAvailabilityDto {
    isEligible: boolean;
    ineligibilityReason?: string;
    items: CertificateDefinitionDto[];
}
