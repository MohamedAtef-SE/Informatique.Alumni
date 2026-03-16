import type { EntityDto } from './common';

export interface BranchDto extends EntityDto<string> {
    name: string;
    code: string;
    address?: string;
    presidentId?: string;
    email?: string;
    phoneNumber?: string;
    linkedInPage?: string;
    facebookPage?: string;
    whatsAppGroup?: string;
    latitude?: number;
    longitude?: number;
}

export interface CreateUpdateBranchDto {
    name: string;
    code: string;
    address?: string;
    presidentId?: string;
    email?: string;
    phoneNumber?: string;
    linkedInPage?: string;
    facebookPage?: string;
    whatsAppGroup?: string;
    latitude?: number;
    longitude?: number;
}
