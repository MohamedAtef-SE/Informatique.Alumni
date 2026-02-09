import type { EntityDto, PagedAndSortedResultRequestDto } from './common';

export const MembershipRequestStatus = {
    Pending: 0,
    Approved: 1,
    Rejected: 2,
    Paid: 3,
    CardPrinted: 4,
    CardDelivered: 5
} as const;
export type MembershipRequestStatus = typeof MembershipRequestStatus[keyof typeof MembershipRequestStatus];

export const DeliveryMethod = {
    Pickup: 0,
    Courier: 1
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
