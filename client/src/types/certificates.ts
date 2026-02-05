import type { FullAuditedEntityDto, EntityDto, PagedAndSortedResultRequestDto } from './common';
import type { DeliveryMethod } from './membership';

export const CertificateRequestStatus = {
    Pending: 0,
    Processing: 1,
    ReadyForPickup: 2,
    Delivered: 3,
    Rejected: 4
} as const;
export type CertificateRequestStatus = typeof CertificateRequestStatus[keyof typeof CertificateRequestStatus];

export const CertificateLanguage = {
    Arabic: 0,
    English: 1
} as const;
export type CertificateLanguage = typeof CertificateLanguage[keyof typeof CertificateLanguage];

export interface CertificateRequestDto extends FullAuditedEntityDto<string> {
    alumniId: string;
    items: CertificateRequestItemDto[];
    status: CertificateRequestStatus;
    totalFees: number;
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
