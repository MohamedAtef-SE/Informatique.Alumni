import type { FullAuditedEntityDto } from './common';

export interface AlumniTripDto extends FullAuditedEntityDto<string> {
    title: string;
    description?: string;
    destination: string;
    startDate: string;
    endDate: string;
    maxCapacity: number;
    pricePerPerson: number;
    isActive: boolean;
}

export const TripRequestStatus = {
    Pending: 0,
    Approved: 1,
    Rejected: 2,
    Cancelled: 3
} as const;
export type TripRequestStatus = typeof TripRequestStatus[keyof typeof TripRequestStatus];

export interface TripRequestDto extends FullAuditedEntityDto<string> {
    tripId: string;
    alumniId: string;
    guestCount: number;
    totalAmount: number;
    status: TripRequestStatus;
    totalParticipants: number;
}
