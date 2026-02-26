import type { FullAuditedEntityDto, EntityDto, PagedAndSortedResultRequestDto } from './common';

export const SyndicateStatus = {
    Draft: -1,
    Pending: 0,
    Reviewing: 1,
    SentToSyndicate: 2,
    CardReady: 3,
    Rejected: 4,
    Received: 5
} as const;
export type SyndicateStatus = typeof SyndicateStatus[keyof typeof SyndicateStatus];

export const PaymentStatus = {
    Pending: 0,
    Paid: 1,
    Failed: 2
} as const;
export type PaymentStatus = typeof PaymentStatus[keyof typeof PaymentStatus];

export interface SyndicateDto extends EntityDto<string> {
    name: string;
    description: string;
    requirements: string;
}

export interface SyndicateSubscriptionDto extends FullAuditedEntityDto<string> {
    alumniId: string;
    syndicateId: string;
    status: SyndicateStatus;
    feeAmount: number;
    paymentStatus: PaymentStatus;
    adminNotes?: string;
    documents: SyndicateDocumentDto[];
}

export interface SyndicateDocumentDto extends EntityDto<string> {
    requirementName: string;
    fileBlobName: string;
}

export interface SyndicateSubscriptionAdmin extends FullAuditedEntityDto<string> {
    alumniId: string;
    alumniName: string;
    alumniNationalId: string;
    alumniMobile: string;
    syndicateId: string;
    syndicateName: string;
    status: SyndicateStatus;
    feeAmount: number;
    paymentStatus: PaymentStatus;
    deliveryMethod: number;
    adminNotes?: string;
    documents: SyndicateDocumentDto[];
}

export interface SyndicateRequestFilterDto extends PagedAndSortedResultRequestDto {
    fromDate?: string;
    toDate?: string;
    branchId?: string;
    status?: SyndicateStatus;
    paymentStatus?: PaymentStatus;
    filter?: string;
}

export interface CreateUpdateSyndicateDto {
    name: string;
    description: string;
    requirements: string;
}
