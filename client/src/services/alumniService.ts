import { api } from './api';
import type { PagedResultDto } from '../types/common';
import type { AlumniListDto, AlumniSearchFilterDto, AlumniProfileDetailDto, AlumniMyProfileDto, UpdateMyProfileDto, WalletActivityDto } from '../types/alumni';
import type { AlumniTripDto } from '../types/trips';
import type { CurrencyDto } from '../types/lookups';

export const alumniService = {
    getList: async (filter: AlumniSearchFilterDto) => {
        const response = await api.get<PagedResultDto<AlumniListDto>>('/api/app/alumni-search', {
            params: filter,
            paramsSerializer: { indexes: null }
        });
        return response.data;
    },

    getProfile: async (id: string) => {
        const response = await api.get<AlumniProfileDetailDto>(`/api/app/alumni-search/${id}`);
        return response.data;
    },

    getMyProfile: async () => {
        const response = await api.get<AlumniMyProfileDto>('/api/app/alumni-profile/my-profile');
        return response.data;
    },

    updateProfile: async (input: UpdateMyProfileDto) => {
        const response = await api.put<AlumniMyProfileDto>('/api/app/alumni-profile/my-profile', input);
        return response.data;
    },

    uploadProfilePhoto: async (file: File) => {
        const formData = new FormData();
        formData.append('file', file, file.name);
        const response = await api.post<string>('/api/app/alumni-profile/upload-photo', formData, {
            headers: { 'Content-Type': 'multipart/form-data' }
        });
        return response.data;
    },

    applyAsAdvisor: async (input: { bio: string; experienceYears: number; expertiseIds: string[] }) => {
        await api.post('/api/app/alumni-profile/apply-as-advisor', input);
    },

    topUpWallet: async (amount: number) => {
        const response = await api.post<AlumniMyProfileDto>(`/api/app/alumni-profile/top-up-wallet?amount=${amount}`);
        return response.data;
    },

    getWalletActivity: async () => {
        const response = await api.get<WalletActivityDto[]>('/api/app/alumni-profile/wallet-activity');
        return response.data;
    },

    // ── Trips ─────────────────────────────────────────────────────────────────

    /** Returns active, upcoming trips available for booking */
    getActiveTrips: async (params?: { skipCount?: number; maxResultCount?: number }) => {
        const response = await api.get<PagedResultDto<AlumniTripDto>>('/api/app/trip/active-trips', { params });
        return response.data;
    },

    /** Submit a booking request. guestCount = extra guests beyond the alumni themselves */
    requestTrip: async (tripId: string, guestCount: number) => {
        await api.post(`/api/app/trip/request-trip/${tripId}`, { guestCount });
    },


    // ── Currencies (AllowAnonymous) ────────────────────────────────────────────

    getCurrencies: async (): Promise<CurrencyDto[]> => {
        const response = await api.get<CurrencyDto[]>('/api/app/lookup/currencies');
        return response.data;
    },

    // Helper to resolve photo URL
    getPhotoUrl: (path?: string) => {
        if (!path) return undefined;
        if (path.startsWith('http') || path.startsWith('https')) return path;
        
        // Ensure path starts with a slash if not already present
        const resolvedPath = path.startsWith('/') ? path : `/${path}`;
        const baseUrl = import.meta.env.VITE_API_URL || 'https://localhost:44386';
        
        // Serve through the secure controller
        return `${baseUrl}/api/profile-photo${resolvedPath}`;
    }
};
