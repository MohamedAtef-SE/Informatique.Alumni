import { api } from './api';
import type { PagedResultDto } from '../types/common';
import type { CareerServiceDto, AlumniCareerSubscriptionDto, CareerPaymentMethod } from '../types/career';

export const careerService = {
    getServices: async (input: any) => {
        const response = await api.get<PagedResultDto<CareerServiceDto>>('/api/app/career/services', {
            params: input
        });
        return response.data;
    },

    getService: async (id: string) => {
        const response = await api.get<CareerServiceDto>(`/api/app/career/${id}`);
        return response.data;
    },

    // Jobs
    getJobs: async (input: any) => {
        const response = await api.get('/api/app/job/jobs', { params: input });
        return response.data;
    },
    applyJob: async (jobId: string) => {
        const response = await api.post(`/api/app/job/apply/${jobId}`);
        return response.data;
    },

    // CV
    getMyCv: async () => {
        const response = await api.get<any>('/api/app/cv/my-cv');
        return response.data;
    },
    updateCv: async (input: any) => {
        const response = await api.put<any>('/api/app/cv/my-cv', input);
        return response.data;
    },
    downloadCv: async (cvId: string) => {
        const response = await api.get(`/api/app/cv/${cvId}/pdf`, { responseType: 'blob' });
        return response.data;
    },

    // Guidance
    getAdvisors: async () => {
        const response = await api.get<any[]>('/api/app/advising/available-advisors');
        return response.data;
    },
    getBookedSessions: async () => {
        const response = await api.get('/api/app/advising/my-requests', { params: { maxResultCount: 10 } }); // Call separate endpoint
        return response.data;
    },
    bookSession: async (input: any) => {
        // ABP CreateRequestAsync -> POST /api/app/advising/request (drops "Create" prefix)
        const response = await api.post('/api/app/advising/request', input);
        return response.data;
    },

    subscribe: async (serviceId: string, timeslotId: string, paymentMethod: CareerPaymentMethod) => {
        const response = await api.post<AlumniCareerSubscriptionDto>(`/api/app/career/subscribe`, null, {
            params: { serviceId, timeslotId, paymentMethod }
        });
        return response.data;
    },

    cancelSubscription: async (serviceId: string) => {
        await api.delete(`/api/app/career/subscription/${serviceId}`);
    }
};
