export interface MedicalPartnerDto {
    id: string;
    name: string;
    description?: string;
    type: number; // Legacy
    address: string;
    contactNumber: string;
    website?: string;
    logoUrl?: string;
    city?: string;
    region?: string;
    category?: string; // Legacy
    email?: string;
    hotlineNumber?: string;
    medicalCategoryId?: string;
    medicalCategoryName?: string;
    medicalCategoryBaseType?: number;
    isVerified: boolean;

    isActive: boolean;
    offers: MedicalOfferDto[];
    discountRate?: number;
}

export interface MedicalCategoryDto {
    id: string;
    nameAr: string;
    nameEn: string;
    baseType: number;
    isActive: boolean;
}

export interface CreateUpdateMedicalCategoryDto {
    nameAr: string;
    nameEn: string;
    baseType: number;
    isActive: boolean;
}

export interface MedicalOfferDto {
    id: string;
    medicalPartnerId: string;
    title: string;
    description: string;
    discountCode?: string;
    isActive: boolean;
}

export interface GetMedicalPartnersInput {
    filterText?: string;
    category?: string;
    city?: string;
    region?: string;
    hasActiveOffers?: boolean;
    type?: number;
    skipCount?: number;
    maxResultCount?: number;
    sorting?: string;
}

export const MedicalPartnerType = {
    Other: 0,
    Pharmacy: 1,
    Hospital: 2,
    Lab: 3,
    Clinic: 4
} as const;

export type MedicalPartnerType = typeof MedicalPartnerType[keyof typeof MedicalPartnerType];

export interface HealthStatsDto {
    medicalPartnersCount: number;
    averageSavings: string;
    verifiedQuality: string;
    activeOffersCount: number;
}
export interface CreateMedicalPartnerDto {
    name: string;
    description?: string;
    medicalCategoryId?: string;
    address: string;
    contactNumber: string;
    website?: string;
    city?: string;
    region?: string;
    email?: string;
    hotlineNumber?: string;
    category?: string; // Legacy
    isVerified?: boolean;
}

export interface UpdateMedicalPartnerDto extends CreateMedicalPartnerDto { }
