import { api, BASE_URL } from './api';
import type { PagedResultDto } from '../types/common';
import type { BlogPostDto } from '../types/news';
import type { AcademicGrantDto, CommercialDiscountDto } from '../types/benefits';
import type { AssociationRequestDto, CardPrintDto } from '../types/membership';
import type { CertificateAvailabilityDto } from '../types/certificates';

export const servicesAppService = {
    // News
    getPosts: async (input?: {
        category?: string;
        keyword?: string;
        minDate?: string;
        maxDate?: string;
        isFeatured?: boolean;
        tag?: string;
        skipCount?: number;
        maxResultCount?: number;
        sorting?: string;
    }) => {
        const response = await api.get<PagedResultDto<BlogPostDto>>('/api/app/blog', { params: input });
        return response.data;
    },
    getPost: async (id: string) => {
        const response = await api.get<BlogPostDto>(`/api/app/blog/${id}`);
        return response.data;
    },

    // Benefits
    getGrants: async () => {
        const response = await api.get<PagedResultDto<AcademicGrantDto>>('/api/app/benefits/grants');
        return response.data;
    },

    getDiscounts: async () => {
        const response = await api.get<PagedResultDto<CommercialDiscountDto>>('/api/app/benefits/discounts');
        return response.data;
    },

    // Membership
    getMembershipStatus: async () => {
        // Assuming a customized endpoint or just listing requests for now
        const response = await api.get<PagedResultDto<AssociationRequestDto>>('/api/app/membership');
        return response.data;
    },

    getCard: async () => {
        const response = await api.get<CardPrintDto>('/api/app/membership/my-card');
        return response.data;
    },

    // Certificates
    getCertificates: async () => {
        const response = await api.get('/api/app/certificate-request/my-requests');
        return response.data;
    },
    getCertificateDefinitions: async () => {
        const response = await api.get<CertificateAvailabilityDto>('/api/app/certificate-definition/available');
        return response.data;
    },
    getBranches: async () => {
        const response = await api.get('/api/app/branch/academic-structure');
        return response.data;
    },
    requestCertificate: async (input: any) => {
        const response = await api.post('/api/app/certificate-request', input);
        return response.data;
    },

    // Syndicates
    // Syndicates
    getSyndicateStatus: async () => {
        const response = await api.get<any[]>('/api/app/syndicate/my-applications');
        return response.data;
    },
    applySyndicate: async (input: any) => {
        const response = await api.post('/api/app/syndicate/apply', input);
        return response.data;
    },
    paySyndicate: async (id: string) => {
        const response = await api.post(`/api/app/syndicate-payments/${id}`);
        return response.data;
    },
    uploadSyndicateDocument: async (id: string, input: any) => {
        const response = await api.post(`/api/app/syndicate-uploads/${id}`, input);
        return response.data;
    },
    getSyndicateDocument: (subscriptionId: string, documentId: string) => {
        return `${BASE_URL}/api/app/syndicate-uploads/${subscriptionId}/${documentId}`;
    },
    getSyndicates: async () => {
        const response = await api.get('/api/app/syndicate/syndicates');
        return response.data;
    },

    // Health
    getMedicalPartners: async (input: any) => {
        const response = await api.get('/api/app/medical-partner', { params: input });
        return response.data;
    },
    getHealthcareOffers: async (input: any) => {
        const response = await api.get('/api/app/healthcare-offer-management/active', { params: input });
        return response.data;
    },
    getHealthStats: async () => {
        const response = await api.get('/api/app/medical-partner/stats'); // Standard REST convention for GetStatsAsync mapping
        return response.data;
    }
};
