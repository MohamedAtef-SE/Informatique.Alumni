import { api } from './api';
import type { PagedResultDto } from '../types/common';
import type { AssociationRequestDto, MembershipRequestFilterDto, UpdateStatusDto } from '../types/membership';
import type { EventListDto, CreateEventDto, UpdateEventDto } from '../types/events';
import type { UpdateCertificateStatusDto } from '../types/certificates';
import type { SyndicateStatus } from '../types/syndicates';

export const adminService = {
    // Membership Management
    getRequests: async (input: MembershipRequestFilterDto) => {
        const response = await api.get<PagedResultDto<AssociationRequestDto>>('/api/app/membership', {
            params: input
        });
        return response.data;
    },

    approveRequest: async (id: string) => {
        const response = await api.post<AssociationRequestDto>(`/api/app/membership/${id}/approve`);
        return response.data;
    },

    rejectRequest: async (id: string, reason: string) => {
        await api.post(`/api/app/membership/${id}/reject`, null, {
            params: { reason }
        });
    },

    updateStatus: async (id: string, input: UpdateStatusDto) => {
        await api.put(`/api/app/membership/${id}/status`, input);
    },

    // Event Management
    getEvents: async (input: any) => {
        const response = await api.get<PagedResultDto<EventListDto>>('/api/app/events', { params: input });
        return response.data;
    },

    createEvent: async (input: CreateEventDto) => {
        const response = await api.post('/api/app/events', input);
        return response.data;
    },

    updateEvent: async (id: string, input: UpdateEventDto) => {
        const response = await api.put(`/api/app/events/${id}`, input);
        return response.data;
    },

    deleteEvent: async (id: string) => {
        await api.delete(`/api/app/events/${id}`);
    },

    publishEvent: async (id: string) => {
        await api.post(`/api/app/events/${id}/publish`);
    },

    // Content (CMS) & News
    getPosts: async (input: any) => {
        const response = await api.get('/api/app/blog/posts', { params: input });
        return response.data;
    },

    // Career
    getCareerServices: async (input: any) => {
        const response = await api.get('/api/app/career/services', { params: input });
        return response.data;
    },
    createCareerService: async (input: any) => {
        const response = await api.post('/api/app/career/services', input);
        return response.data;
    },
    deleteCareerService: async (id: string) => {
        await api.delete(`/api/app/career/services/${id}`);
    },

    // Benefits
    getGrants: async (input: any) => {
        const response = await api.get('/api/app/benefits/grants', { params: input });
        return response.data;
    },
    createGrant: async (input: any) => {
        const response = await api.post('/api/app/benefits/grants', input);
        return response.data;
    },
    getDiscounts: async (input: any) => {
        const response = await api.get('/api/app/benefits/discounts', { params: input });
        return response.data;
    },
    createDiscount: async (input: any) => {
        const response = await api.post('/api/app/benefits/discounts', input);
        return response.data;
    },

    // Gallery
    getAlbums: async (input: any) => {
        const response = await api.get('/api/app/gallery/albums', { params: input });
        return response.data;
    },
    createAlbum: async (input: any) => {
        const response = await api.post('/api/app/gallery/albums', input);
        return response.data;
    },

    // Syndicates
    getSyndicateRequests: async (input: any) => {
        const response = await api.get('/api/app/syndicate/list', { params: input });
        return response.data;
    },
    updateSyndicateRequestStatus: async (id: string, status: SyndicateStatus) => {
        const response = await api.put(`/api/app/syndicate/${id}/request-status`, null, { params: { newStatus: status } });
        return response.data;
    },

    // Health
    getMedicalPartners: async (input: any) => {
        const response = await api.get('/api/app/medical-partner', { params: input });
        return response.data;
    },
    getHealthcareOffers: async (input: any) => {
        const response = await api.get('/api/app/healthcare-offer-management', { params: input });
        return response.data;
    },

    // Certificates
    getCertificateRequests: async (input: any) => {
        const response = await api.get('/api/app/certificate-request', { params: input });
        return response.data;
    },
    updateCertificateStatus: async (id: string, input: UpdateCertificateStatusDto) => {
        const response = await api.put(`/api/app/certificate-request/${id}/status`, input);
        return response.data;
    },

    // Organization
    getBranches: async (input: any) => {
        const response = await api.get('/api/app/branch', { params: input });
        return response.data;
    },
    createBranch: async (input: any) => {
        const response = await api.post('/api/app/branch', input);
        return response.data;
    },

    // Trips
    getTrips: async (input: any) => {
        const response = await api.get('/api/app/trip/active-trips', { params: input });
        return response.data;
    },
    createTrip: async (input: any) => {
        const response = await api.post('/api/app/trip', input);
        return response.data;
    },

    // Guidance
    getAdvisingRequests: async (input: any) => {
        const response = await api.get('/api/app/advising', { params: input });
        return response.data;
    },
    updateAdvisingStatus: async (id: string, input: any) => {
        const response = await api.put(`/api/app/advising/${id}/status`, input);
        return response.data;
    },

    // Communication
    sendMessage: async (input: any) => {
        await api.post('/api/app/communication/message', input);
    },
    getRecipientsCount: async (input: any) => {
        const response = await api.post('/api/app/communication/recipients-count', input);
        return response.data;
    }
};
