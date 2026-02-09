export interface MedicalPartnerDto {
    id: string;
    name: string;
    description?: string;
    type: number; // Enum: 0=Other, 1=Pharmacy, 2=Hospital, 3=Lab, 4=Clinic
    address: string;
    contactNumber: string;
    website?: string;

    // Premium Fields
    logoUrl?: string;
    city?: string;
    region?: string;
    category?: string;
    email?: string;
    hotlineNumber?: string;

    isActive: boolean;
    offers: MedicalOfferDto[];
    discountRate?: number; // Calculated or from specific offer
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
