import type { FullAuditedEntityDto } from './common';

export const MedicalPartnerType = {
    Hospital: 0,
    Clinic: 1,
    Pharmacy: 2,
    Lab: 3
} as const;
export type MedicalPartnerType = typeof MedicalPartnerType[keyof typeof MedicalPartnerType];

export interface MedicalPartnerDto extends FullAuditedEntityDto<string> {
    name: string;
    description?: string;
    type: MedicalPartnerType;
    address: string;
    contactNumber: string;
    website?: string;
    isActive: boolean;
    offers: MedicalOfferDto[];
}

export interface MedicalOfferDto extends FullAuditedEntityDto<string> {
    medicalPartnerId: string;
    title: string;
    description: string;
    discountCode?: string;
    isActive: boolean;
}

export interface CreateUpdateMedicalPartnerDto {
    name: string;
    description?: string;
    type: MedicalPartnerType;
    address: string;
    contactNumber: string;
    website?: string;
}

export interface CreateUpdateMedicalOfferDto {
    title: string;
    description: string;
    discountCode?: string;
}
