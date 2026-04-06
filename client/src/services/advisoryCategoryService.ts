import { api } from './api';
import type { PagedResultDto } from '../types/common';
import type { 
    AdvisoryCategoryDto, 
    CreateUpdateAdvisoryCategoryDto, 
    AdvisoryCategoryFilterDto 
} from '../types/guidance';

export const advisoryCategoryService = {
    getList: async (input: AdvisoryCategoryFilterDto) => {
        const response = await api.get<PagedResultDto<AdvisoryCategoryDto>>('/api/app/advisory-category', {
            params: {
                SkipCount: input.skipCount,
                MaxResultCount: input.maxResultCount,
                Sorting: input.sorting,
                Filter: input.filter,
                IsActive: input.isActive
            }
        });
        return response.data;
    },

    getActiveList: async () => {
        const response = await api.get<AdvisoryCategoryDto[]>('/api/app/advisory-category/active-list');
        return response.data;
    },

    get: async (id: string) => {
        const response = await api.get<AdvisoryCategoryDto>(`/api/app/advisory-category/${id}`);
        return response.data;
    },

    create: async (input: CreateUpdateAdvisoryCategoryDto) => {
        const response = await api.post<AdvisoryCategoryDto>('/api/app/advisory-category', input);
        return response.data;
    },

    update: async (id: string, input: CreateUpdateAdvisoryCategoryDto) => {
        const response = await api.put<AdvisoryCategoryDto>(`/api/app/advisory-category/${id}`, input);
        return response.data;
    },

    delete: async (id: string) => {
        await api.delete(`/api/app/advisory-category/${id}`);
    }
};
