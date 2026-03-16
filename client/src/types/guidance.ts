import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from './common';

export const AdvisingRequestStatus = {
    Pending: 1,  // Was 0
    Approved: 2, // Was 1
    Rejected: 3, // Was 2
    Completed: 4,
    Cancelled: 5
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

export interface GuidanceAdminDto extends FullAuditedEntityDto<string> {
    alumniId: string;
    alumniName: string;
    alumniEmail: string;
    advisorId: string;
    advisorName: string;
    advisorEmail: string;
    startTime: string;
    endTime: string;
    subject: string;
    notes: string;
    meetingLink?: string;
    status: AdvisingRequestStatus;
}
