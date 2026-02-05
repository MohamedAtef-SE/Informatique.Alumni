import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from './common';

export const AdvisingRequestStatus = {
    Pending: 0,
    Approved: 1,
    Rejected: 2,
    Cancelled: 3,
    Completed: 4
} as const;
export type AdvisingRequestStatus = typeof AdvisingRequestStatus[keyof typeof AdvisingRequestStatus];

export interface AdvisingRequestDto extends FullAuditedEntityDto<string> {
    alumniId: string;
    advisorId: string;
    startTime: string; // DateTime
    endTime: string;
    subject: string;
    description?: string;
    status: AdvisingRequestStatus;
}

export interface AdvisingRequestFilterDto extends PagedAndSortedResultRequestDto {
    fromDate?: string;
    toDate?: string;
    status?: AdvisingRequestStatus;
    branchId?: string;
}

export interface UpdateAdvisingStatusDto {
    status: AdvisingRequestStatus;
    notes?: string;
}
