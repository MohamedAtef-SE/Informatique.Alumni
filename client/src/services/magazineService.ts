import { api } from './api';
import type { PagedResultDto } from '../types/common';

export interface MagazineListDto {
    id: string;
    title: string;
    issueDate: string;
    downloadUrl: string;
    creationTime: string;
}

export const magazineService = {
    getList: async (input: any) => {
        const response = await api.get<PagedResultDto<MagazineListDto>>('/api/app/magazine', { params: input });
        return response.data;
    },

    download: async (id: string) => {
        const response = await api.post(`/api/app/magazine/${id}/download`, {}, {
            responseType: 'blob'
        });
        return response.data;
    }
};
