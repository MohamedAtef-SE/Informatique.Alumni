import { api } from './api';
import type { PagedResultDto } from '../types/common';
import type { GetEventsInput, EventListDto, EventDetailDto, AlumniEventRegistrationDto } from '../types/events';

export const eventsService = {
    getList: async (input: GetEventsInput) => {
        const response = await api.get<PagedResultDto<EventListDto>>('/api/app/events', {
            params: input
        });
        return response.data;
    },

    get: async (id: string) => {
        const response = await api.get<EventDetailDto>(`/api/app/events/${id}`);
        return response.data;
    },

    register: async (eventId: string) => {
        const response = await api.post<AlumniEventRegistrationDto>(`/api/app/events/${eventId}/register`);
        return response.data;
    },

    getMyRegistrations: async () => {
        const response = await api.get<AlumniEventRegistrationDto[]>('/api/app/events/my-registrations');
        return response.data;
    },

    getAgendaPdf: async (eventId: string) => {
        const response = await api.get(`/api/app/events/${eventId}/agenda-pdf`, {
            responseType: 'blob'
        });
        return response.data;
    },

    // Trips
    getTrips: async (input: any) => {
        const response = await api.get('/api/app/trip/active-trips', { params: input });
        return response.data;
    }
};
