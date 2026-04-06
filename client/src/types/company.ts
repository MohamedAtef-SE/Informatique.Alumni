import type { EntityDto, PagedAndSortedResultRequestDto } from './common';

export interface CompanyDto extends EntityDto<string> {
    nameAr: string;
    nameEn: string;
    logoBlobName: string;
    websiteUrl?: string;
    industry?: string;
    description?: string;
    email?: string;
    phoneNumber?: string;
    isActive: boolean;
}

export interface CreateUpdateCompanyDto {
    nameAr: string;
    nameEn: string;
    websiteUrl?: string;
    industry?: string;
    description?: string;
    email?: string;
    phoneNumber?: string;
    isActive?: boolean;
    logo?: File; // For multipart form-data upload
}

export interface CompanyFilterDto extends PagedAndSortedResultRequestDto {
    filter?: string;
    isActive?: boolean;
}
