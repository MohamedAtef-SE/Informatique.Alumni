import type { EntityDto, PagedAndSortedResultRequestDto } from './common';

export const MembershipRequestStatus = {
    Pending: 1,
    Paid: 2,
    Approved: 3,
    Rejected: 4,
    InProgress: 5,
    ReadyForPickup: 6,
    OutForDelivery: 7,
    Delivered: 8
} as const;
export type MembershipRequestStatus = typeof MembershipRequestStatus[keyof typeof MembershipRequestStatus];

export const DeliveryMethod = {
    OfficePickup: 1,
    HomeDelivery: 2
} as const;
export type DeliveryMethod = typeof DeliveryMethod[keyof typeof DeliveryMethod];

export interface SubscriptionFeeDto extends EntityDto<string> {
    name: string;
    amount: number;
    year: number;
    seasonStartDate: string;
    seasonEndDate: string;
    isActive: boolean;
}

export interface EligibilityCheckDto {
    checkName: string;
    status: 'Pass' | 'Fail' | 'Warning';
    message: string;
    icon: string;
}

export interface AssociationRequestDto extends EntityDto<string> {
    alumniId: string;
    subscriptionFeeId: string;
    subscriptionFeeName: string;
    status: MembershipRequestStatus;
    requestDate: string;
    approvalDate?: string;
    rejectionReason?: string;
    remainingAmount: number;
    deliveryFee: number;
    deliveryMethod: DeliveryMethod;

    // Alumni Identity
    alumniName: string;
    alumniNationalId: string;
    alumniPhotoUrl?: string;
    collegeName?: string;
    graduationYear?: number;

    // Eligibility
    eligibilityChecks: EligibilityCheckDto[];
    eligibilitySummary: 'AllClear' | 'NeedsReview' | 'CannotApprove';
}

export interface MembershipRequestFilterDto extends PagedAndSortedResultRequestDto {
    branchId?: string;
    gradYear?: number;
    gradSemester?: number;
    collegeId?: string;
    majorId?: string;
    deliveryMethod?: DeliveryMethod;
    status?: MembershipRequestStatus;
    minDate?: string;
    maxDate?: string;
    filter?: string;
}

export interface CardPrintDto {
    requestId: string;
    alumniName: string;
    alumniNationalId: string;
    alumniPhotoUrl: string;
    degree: string;
    collegeName: string;
    majorName: string;
    gradYear: number;
    isActive: boolean;
}

export interface UpdateStatusDto {
    status: MembershipRequestStatus;
    note?: string;
}
