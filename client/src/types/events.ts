import type { FullAuditedEntityDto, EntityDto, PagedAndSortedResultRequestDto } from './common';

export interface GetEventsInput extends PagedAndSortedResultRequestDto {
    filter?: string;
    startDate?: string;
    endDate?: string;
    isParticipated?: boolean;
}

export interface EventListDto extends FullAuditedEntityDto<string> {
    nameAr: string;
    nameEn: string;
    code: string;
    description: string;
    location: string;
    address?: string;
    googleMapUrl?: string;
    hasFees: boolean;
    feeAmount?: number;
    lastSubscriptionDate: string; // ISO Date
    isPublished: boolean;
    branchId?: string;
    branchName?: string;
    activityTypeName?: string;
    coverImageUrl?: string;
    startDate?: string;
    endDate?: string;
}

export interface EventDetailDto extends EventListDto {
    timeslots: EventTimeslotDto[];
    participatingCompanies: EventParticipatingCompanyDto[];
    agenda: EventAgendaItemDto[];
    myRegistration?: AlumniEventRegistrationDto;
}

export interface EventTimeslotDto extends EntityDto<string> {
    startTime: string;
    endTime: string;
    capacity: number;
}

export interface CreateEventDto {
    nameAr: string;
    nameEn: string;
    code: string;
    description: string;
    location: string;
    address?: string;
    googleMapUrl?: string;
    hasFees: boolean;
    feeAmount?: number;
    lastSubscriptionDate: string;
    branchId?: string;
    timeslots: CreateTimeslotDto[];
}

export interface UpdateEventDto extends CreateEventDto { }

export interface CreateTimeslotDto {
    startTime: string;
    endTime: string;
    capacity: number;
}

export interface CompanyDto extends EntityDto<string> {
    nameAr: string;
    nameEn: string;
    logoBlobName: string;
    websiteUrl?: string;
}

export interface ParticipationTypeDto extends FullAuditedEntityDto<string> {
    nameAr: string;
    nameEn: string;
}

export interface EventParticipatingCompanyDto extends EntityDto<string> {
    companyId: string;
    participationTypeId: string;
    company?: CompanyDto;
    participationType?: ParticipationTypeDto;
}

export interface EventAgendaItemDto {
    id: string;
    eventId: string;
    date: string;
    startTime: string; // TimeSpan
    endTime: string;
    activityName: string;
    place?: string;
    description?: string;
}

export const RegistrationStatus = {
    Pending: 0,
    Confirmed: 1,
    Cancelled: 2,
    Attended: 3
} as const;
export type RegistrationStatus = typeof RegistrationStatus[keyof typeof RegistrationStatus];

export interface AlumniEventRegistrationDto extends FullAuditedEntityDto<string> {
    alumniId: string;
    eventId: string;
    ticketCode: string;
    status: RegistrationStatus;
    qrCodeUrl: string;
    eventName?: string;
    eventDate?: string;
    location?: string;
}
