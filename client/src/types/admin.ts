import type { EntityDto, PagedAndSortedResultRequestDto } from './common';

export const AdvisoryStatus = {
    None: 0,
    Requested: 1,
    Approved: 2,
    Rejected: 3,
    Inactive: 4
} as const;
export type AdvisoryStatus = typeof AdvisoryStatus[keyof typeof AdvisoryStatus];

export interface DistributionItem {
    label: string;
    count: number;
}

export interface MonthlyMetric {
    month: string;
    count: number;
}

export interface ActivityItem {
    time: string;
    description: string;
    type: string;
}

export interface DashboardStatsDto {
    totalAlumni: number;
    pendingAlumni: number;
    activeAlumni: number;
    rejectedAlumni: number;
    bannedAlumni: number;
    activeJobs: number;
    upcomingEvents: number;
    pendingGuidanceRequests: number;
    pendingSyndicateRequests: number;
    totalRevenue: number;
    alumniByCollege: DistributionItem[];
    topEmployers: DistributionItem[];
    topLocations: DistributionItem[];
    monthlyRegistrations: MonthlyMetric[];
    recentActivities: ActivityItem[];
}

export interface AlumniAdminListDto extends EntityDto<string> {
    userId: string;
    fullName: string;
    email: string;
    mobileNumber: string;
    status: number;
    isVip: boolean;
    isAdvisor: boolean;
    isNotable: boolean;
    idCardStatus: number;
    advisoryStatus: AdvisoryStatus;
    creationTime: string;
}

export interface AlumniAdminDto extends AlumniAdminListDto {
    userName: string;
    jobTitle: string;
    bio?: string;
    nationalId: string;
    photoUrl?: string;
    address?: string;
    city?: string;
    country?: string;
    company?: string;
    facebookUrl?: string;
    linkedinUrl?: string;
    rejectionReason?: string;
    
    // Advisory
    advisoryStatus: AdvisoryStatus;
    advisoryBio?: string;
    advisoryExperienceYears: number;
    advisoryRejectionReason?: string;
    expertiseNames?: string[];
    walletBalance: number;
    viewCount: number;
    showInDirectory: boolean;
    lastModificationTime?: string;
}

export interface AlumniAdminGetListInput extends PagedAndSortedResultRequestDto {
    statusFilter?: number;
}

export interface JobAdminDto extends EntityDto<string> {
    title: string;
    description: string;
    requirements?: string;
    companyId: string;
    isActive: boolean;
    closingDate?: string;
    creationTime: string;
    applicationCount: number;
}

export interface JobAdminGetListInput extends PagedAndSortedResultRequestDto {
    isActive?: boolean;
    filter?: string;
}

export interface JobApplicationAdminDto extends EntityDto<string> {
    jobId: string;
    alumniId: string;
    alumniName: string;
    cvSnapshotBlobName: string;
    creationTime: string;
}

export interface AlumniCvDto {
    alumniId: string;
    fullName: string;
    summary?: string;
    experiences: CvExperienceDto[];
    educations: CvEducationDto[];
    skills: CvSkillDto[];
    languages: CvLanguageDto[];
    certifications: CvCertificationDto[];
    projects: CvProjectDto[];
    socialLinks: CvSocialLinkDto[];
}

export interface CvExperienceDto {
    company: string;
    position: string;
    startDate: string;
    endDate?: string;
    description?: string;
}

export interface CvEducationDto {
    institution: string;
    degree: string;
    startDate: string;
    endDate?: string;
}

export interface CvSkillDto {
    name: string;
    proficiencyLevel?: string;
}

export interface CvLanguageDto {
    name: string;
    fluencyLevel?: string;
}

export interface CvCertificationDto {
    name: string;
    issuer: string;
    date?: string;
}

export interface CvProjectDto {
    name: string;
    description?: string;
    link?: string;
}

export interface CvSocialLinkDto {
    platform: string;
    url: string;
}
