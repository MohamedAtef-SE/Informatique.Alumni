import { api } from './api';
import type { PagedResultDto } from '../types/common';
import type { AlumniListDto, AlumniSearchFilterDto, AlumniProfileDetailDto, AlumniMyProfileDto, UpdateMyProfileDto } from '../types/alumni';

export const alumniService = {
    getList: async (filter: AlumniSearchFilterDto) => {
        const response = await api.get<PagedResultDto<AlumniListDto>>('/api/app/alumni-search', {
            params: filter,
            paramsSerializer: {
                indexes: null // array indexes format (e.g. ids[0]=1&ids[1]=2)
            }
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
        // Use 'file' as the field name to match IFormFile parameter
        formData.append('file', file, file.name);

        // Use ABP convention route: /api/app/alumni-profile/upload-photo
        const response = await api.post<string>('/api/app/alumni-profile/upload-photo', formData, {
            headers: {
                'Content-Type': 'multipart/form-data'
            }
        });

        return response.data;
    },

    // Helper to resolve photo URL
    getPhotoUrl: (path?: string) => {
        if (!path) return undefined;
        if (path.startsWith('http') || path.startsWith('https')) return path;

        // Use the same BASE_URL fallback as the api module
        const baseUrl = import.meta.env.VITE_API_URL || 'https://localhost:44386';
        return `${baseUrl}${path}`;
    }
};
