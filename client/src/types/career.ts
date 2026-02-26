import type { FullAuditedEntityDto, EntityDto } from './common';

export interface CareerServiceDto extends FullAuditedEntityDto<string> {
    nameAr: string;
    nameEn: string;
    code: string;
    description: string;
    mapUrl?: string;
    hasFees: boolean;
    feeAmount: number;
    lastSubscriptionDate: string;
    subscribedCount: number;
    serviceTypeId?: string;
    branchId?: string;
    serviceType?: { id: string; nameEn: string; nameAr: string };
    branch?: { id: string; name: string; nameEn?: string; nameAr?: string };
    timeslots: CareerServiceTimeslotDto[];
}

export interface CareerServiceTimeslotDto extends EntityDto<string> {
    date: string;
    startTime: string; // TimeSpan string "00:00:00"
    endTime: string;
    lecturerName: string;
    room: string;
    address: string;
    capacity: number;
    currentCount: number;
}

export interface CreateCareerServiceDto {
    nameAr: string;
    nameEn: string;
    code: string;
    description: string;
    mapUrl?: string;
    hasFees: boolean;
    feeAmount: number;
    lastSubscriptionDate: string;
    serviceTypeId: string;
    branchId: string;
    timeslots: CreateCareerServiceTimeslotDto[];
}

export interface CreateCareerServiceTimeslotDto {
    date: string;
    startTime: string;
    endTime: string;
    lecturerName: string;
    room: string;
    address: string;
    capacity: number;
}

export const CareerPaymentMethod = {
    Cash: 0,
    Wallet: 1,
    Online: 2
} as const;

export type CareerPaymentMethod = typeof CareerPaymentMethod[keyof typeof CareerPaymentMethod];

export interface AlumniCareerSubscriptionDto {
    id: string;
    careerServiceId: string;
    alumniId: string;
    timeslotId: string;
    paymentMethod: CareerPaymentMethod;
    paymentStatus: string;
    amountPaid: number;
    registrationDate: string;
}

export interface CareerLookupItemDto {
    id: string;
    nameAr: string;
    nameEn: string;
}

export interface CareerLookupsDto {
    serviceTypes: CareerLookupItemDto[];
    branches: CareerLookupItemDto[];
}
