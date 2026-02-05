import type { FullAuditedEntityDto, EntityDto, PagedAndSortedResultRequestDto } from './common';

export const SyndicateStatus = {
    Pending: 0,
    Approved: 1,
    Rejected: 2,
    MoreInfoRequired: 3
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
