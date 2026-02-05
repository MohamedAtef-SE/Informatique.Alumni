import type { FullAuditedEntityDto } from './common';

export interface CurriculumVitaeDto extends FullAuditedEntityDto<string> {
    summary?: string;
    isLookingForJob: boolean;
    experiences: CvExperienceDto[];
    educations: CvEducationDto[];
    skills: CvSkillDto[];
    languages: CvLanguageDto[];
    certifications: CvCertificationDto[];
    projects: CvProjectDto[];
    // Extend as needed for other lists
}

export interface CvExperienceDto {
    id: string;
    company: string;
    position: string;
    startDate: string;
    endDate?: string;
    description?: string;
}

export interface CvEducationDto {
    id: string;
    institution: string;
    degree: string;
    startDate: string;
    endDate?: string;
}

export interface CvSkillDto {
    id: string;
    name: string;
    proficiencyLevel?: string;
}

export interface CvLanguageDto {
    id: string;
    name: string;
    fluencyLevel?: string;
}

export interface CvCertificationDto {
    id: string;
    name: string;
    issuer: string;
    date?: string;
}

export interface CvProjectDto {
    id: string;
    name: string;
    description?: string;
    link?: string;
}
