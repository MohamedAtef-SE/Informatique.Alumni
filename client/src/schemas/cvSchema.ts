import { z } from 'zod';

export const cvExperienceSchema = z.object({
    id: z.string().nullish(),
    company: z.string().min(1, "Company name is required"),
    position: z.string().min(1, "Job title is required"),
    startDate: z.string().min(1, "Start date is required"),
    endDate: z.string().nullish().or(z.literal('')),
    description: z.string().nullish(),
});

export const cvEducationSchema = z.object({
    id: z.string().nullish(),
    institution: z.string().min(1, "Institution is required"),
    degree: z.string().min(1, "Degree is required"),
    startDate: z.string().min(1, "Start date is required"),
    endDate: z.string().nullish().or(z.literal('')),
});

export const cvSkillSchema = z.object({
    id: z.string().nullish(),
    name: z.string().min(1, "Skill name is required"),
    proficiencyLevel: z.string().nullish(),
});

export const cvLanguageSchema = z.object({
    id: z.string().nullish(),
    name: z.string().min(1, "Language name is required"),
    fluencyLevel: z.string().nullish(),
});

export const cvCertificationSchema = z.object({
    id: z.string().nullish(),
    name: z.string().min(1, "Certification name is required"),
    issuer: z.string().min(1, "Issuer is required"),
    date: z.string().nullish().or(z.literal('')),
});

export const cvProjectSchema = z.object({
    id: z.string().nullish(),
    name: z.string().min(1, "Project name is required"),
    description: z.string().nullish(),
    link: z.string().url("Must be a valid URL").nullish().or(z.literal('')),
});

export const cvSocialLinkSchema = z.object({
    id: z.string().nullish(),
    platform: z.string().min(1, "Platform is required"),
    url: z.string().url("Must be a valid URL"),
});

export const cvSchema = z.object({
    id: z.string().nullish(),
    summary: z.string().nullish(),
    isLookingForJob: z.boolean().default(false),
    experiences: z.array(cvExperienceSchema).default([]),
    educations: z.array(cvEducationSchema).default([]),
    skills: z.array(cvSkillSchema).default([]),
    languages: z.array(cvLanguageSchema).default([]),
    certifications: z.array(cvCertificationSchema).default([]),
    projects: z.array(cvProjectSchema).default([]),
    socialLinks: z.array(cvSocialLinkSchema).default([]),
});

export type CvFormData = z.infer<typeof cvSchema>;
export type ExperienceFormData = z.infer<typeof cvExperienceSchema>;
export type EducationFormData = z.infer<typeof cvEducationSchema>;
export type SkillFormData = z.infer<typeof cvSkillSchema>;
export type LanguageFormData = z.infer<typeof cvLanguageSchema>;
export type CertificationFormData = z.infer<typeof cvCertificationSchema>;
export type ProjectFormData = z.infer<typeof cvProjectSchema>;
export type SocialLinkFormData = z.infer<typeof cvSocialLinkSchema>;
