import type { PagedAndSortedResultRequestDto } from './common';

export const AdvisingRequestStatus = {
    Pending: 0,
    Approved: 1,
    Rejected: 2,
    Completed: 3,
    Cancelled: 4
} as const;

export type AdvisingRequestStatus = typeof AdvisingRequestStatus[keyof typeof AdvisingRequestStatus];

export interface GuidanceAdminDto {
    id: string;
    alumniId: string;
    alumniName: string;
    alumniEmail: string;
    advisorId: string;
    advisorName: string;
    advisorEmail: string;
    subject: string;
    notes: string;
    startTime: string;
    endTime: string;
    status: AdvisingRequestStatus;
    meetingLink?: string;
}

export interface AdvisoryCategoryDto {
    id: string;
    nameAr: string;
    nameEn: string;
    isActive: boolean;
}

export interface CreateUpdateAdvisoryCategoryDto {
    nameAr: string;
    nameEn: string;
    isActive: boolean;
}

export interface AdvisoryCategoryFilterDto extends PagedAndSortedResultRequestDto {
    filter?: string;
    isActive?: boolean;
}
