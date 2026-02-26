import type { FullAuditedEntityDto } from './common';

export interface AcademicGrantDto extends FullAuditedEntityDto<string> {
    nameAr: string;
    nameEn: string;
    type: string;
    percentage: number;
}

export interface CommercialDiscountDto extends FullAuditedEntityDto<string> {
    categoryId: string;
    providerName: string;
    title: string;
    description: string;
    discountPercentage: number;
    promoCode?: string;
    validUntil: string;
    websiteUrl?: string;
}

export interface CreateAcademicGrantDto {
    nameAr: string;
    nameEn: string;
    type: string; // 'Partial', 'Full'
    percentage: number;
}

export interface CreateCommercialDiscountDto {
    providerName: string;
    title: string;
    description: string;
    discountPercentage: number;
    promoCode?: string;
    validUntil: string;
    websiteUrl?: string;
}
