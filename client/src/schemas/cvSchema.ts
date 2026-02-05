import { z } from 'zod';

export const cvExperienceSchema = z.object({
    id: z.string().optional(),
    company: z.string().min(1, "Company name is required"),
    position: z.string().min(1, "Job title is required"),
    startDate: z.string().min(1, "Start date is required"),
    endDate: z.string().optional().or(z.literal('')),
    description: z.string().optional(),
});

export const cvEducationSchema = z.object({
    id: z.string().optional(),
    institution: z.string().min(1, "Institution is required"),
    degree: z.string().min(1, "Degree is required"),
    startDate: z.string().min(1, "Start date is required"),
    endDate: z.string().optional().or(z.literal('')),
});

export const cvSchema = z.object({
    id: z.string().optional(),
    summary: z.string().optional(),
    isLookingForJob: z.boolean().default(false),
    experiences: z.array(cvExperienceSchema).default([]),
    educations: z.array(cvEducationSchema).default([]),
});

export type CvFormData = z.infer<typeof cvSchema>;
export type ExperienceFormData = z.infer<typeof cvExperienceSchema>;
export type EducationFormData = z.infer<typeof cvEducationSchema>;
