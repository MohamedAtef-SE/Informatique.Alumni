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

export interface CreateTripDto {
    title: string;
    description?: string;
    destination: string;
    startDate: string;
    endDate: string;
    maxCapacity: number;
    pricePerPerson: number;
    isActive: boolean;
}

export interface UpdateTripDto extends CreateTripDto {
    id: string;
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

// ── Admin-facing types (TripAdminAppService) ─────────────────────────────────

export interface TripAdminDto {
    id: string;
    nameAr: string;
    nameEn: string;
    tripType: number;
    startDate: string;
    endDate: string;
    location: string;
    pricePerPerson: number;
    maxCapacity: number | null;
    isActive: boolean;
    requestCount: number;
    creationTime: string;
}

export interface TripRequestAdminDto {
    id: string;
    tripId: string;
    alumniId: string;
    alumniName: string;
    alumniEmail: string;
    phoneNumber: string;
    guestCount: number;
    totalAmount: number;
    status: TripRequestStatus;
    creationTime: string;
}

export interface CreateTripInput {
    title: string;
    description?: string;
    destination: string;
    tripType: number;
    startDate: string;
    endDate: string;
    maxCapacity: number;
    pricePerPerson: number;
    isActive: boolean;
    type?: number;
}
