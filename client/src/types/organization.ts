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

export interface CollegeDto extends EntityDto<string> {
    name: string;
    branchId?: string | null;
    externalId?: string | null;
}

export interface CreateUpdateCollegeDto {
    name: string;
    branchId?: string | null;
    externalId?: string | null;
}

export interface MajorDto extends EntityDto<string> {
    name: string;
    collegeId: string;
}

export interface CreateUpdateMajorDto {
    name: string;
    collegeId: string;
}
