import type { PagedAndSortedResultRequestDto } from './common';

export const VipFilterOption = {
    All: 0,
    VipOnly: 1,
    NonVipOnly: 2
} as const;

export type VipFilterOption = typeof VipFilterOption[keyof typeof VipFilterOption];

export interface AlumniSearchFilterDto extends PagedAndSortedResultRequestDto {
    graduationYears?: number[];
    graduationSemesters?: number[];
    branchId?: string; // Guid
    collegeId?: string;
    majorId?: string;
    minorId?: string;
    gpaMin?: number;
    gpaMax?: number;
    gender?: string;
    nationalityId?: string;
    isVip?: VipFilterOption;
}

export interface AlumniListDto {
    id: string;
    userId: string;
    name: string;
    alumniId: string;
    gender?: string;
    nationality?: string;
    graduationYear: number;
    graduationSemester: number;
    college: string;
    major: string;
    minor?: string;
    gpa: number;
    primaryEmail?: string;
    primaryMobile?: string;
    isVip: boolean;
    photoUrl?: string;
}

export interface AlumniEducationDto {
    id: string;
    institutionName: string;
    degree: string;
    graduationYear: number;
    graduationSemester: number;
    college?: string;
    major?: string;
    minor?: string;
    gpa?: number;
}

export interface ContactEmailDto {
    id: string;
    email: string;
    isPrimary: boolean;
}

export interface ContactMobileDto {
    id: string;
    mobileNumber: string;
    isPrimary: boolean;
}

export interface ContactPhoneDto {
    id: string;
    phoneNumber: string;
    label?: string;
}

export interface QualificationHistoryDto {
    qualificationId: string;
    degreeName: string;
    major: string;
    college: string;
    graduationYear: number;
    cumulativeGPA: number;
}

// This matches the C# AlumniMyProfileDto
export interface AlumniMyProfileDto {
    // Read-Only Data
    alumniId: string;
    nameAr: string;
    nameEn: string;
    nationalId: string;
    nationality?: string;
    gender?: string;
    birthDate?: string;
    openingBalance: number;
    viewCount: number;
    academicHistory: QualificationHistoryDto[];

    // Editable Data
    photoUrl?: string;
    address?: string;
    city?: string;
    country?: string;
    bio?: string;
    jobTitle?: string;
    company?: string;
    facebookUrl?: string;
    linkedinUrl?: string;

    // Contact Collections
    emails: ContactEmailDto[];
    mobiles: ContactMobileDto[];
    phones: ContactPhoneDto[];
}

export interface AlumniProfileDetailDto {
    id: string;
    userId: string;
    name: string;
    alumniId: string;
    nationalId: string;
    birthdate?: string;
    gender?: string;
    nationality?: string;
    educations: AlumniEducationDto[];
    address?: string;
    city?: string;
    country?: string;
    jobTitle?: string;
    bio?: string;
    walletBalance: number;
    openingBalance: number;
    isVip: boolean;
    showInDirectory: boolean;
    viewCount: number;
    photoUrl?: string;
    company?: string;
    phoneNumber?: string;
    facebookUrl?: string;
    linkedinUrl?: string;
}

export interface UpdateAlumniProfileDto {
    bio?: string;
    jobTitle?: string;
    company?: string;
    city?: string;
    country?: string;
    facebookUrl?: string;
    linkedinUrl?: string;
}

export interface UpdateMyProfileDto {
    address?: string;
    city?: string;
    country?: string;
    profilePhoto?: File;
    bio?: string;
    jobTitle?: string;
    company?: string;
    facebookUrl?: string;
    linkedinUrl?: string;
    emails: ContactEmailDto[];
    mobiles: ContactMobileDto[];
    phones: ContactPhoneDto[];
}
