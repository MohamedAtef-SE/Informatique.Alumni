import { api, BASE_URL } from './api';
import type { PagedResultDto } from '../types/common';

export interface AlbumDto {
    id: string;
    title: string;
    description?: string;
    coverImageUrl?: string;
    photoCount: number;
    creationTime: string;
}

export interface AlbumDetailDto {
    id: string;
    title: string;
    description: string;
    mediaItems: MediaItemDto[];
}

export interface MediaItemDto {
    id: string;
    url: string;
    type: string;
}

const fixUrl = (url?: string) => {
    if (!url) return '';
    if (url.startsWith('http')) return url;
    // Prepend BASE_URL to relative paths from backend
    return `${BASE_URL}${url.startsWith('/') ? '' : '/'}${url}`;
};

export const galleryService = {
    getAlbums: async (input: any) => {
        const response = await api.get<PagedResultDto<AlbumDto>>('/api/app/gallery/albums', { params: input });
        return {
            ...response.data,
            items: response.data.items.map(item => ({
                ...item,
                coverImageUrl: fixUrl(item.coverImageUrl)
            }))
        };
    },
    // Used for public/admin view
    getAlbum: async (id: string) => {
        const response = await api.get<AlbumDto>(`/api/app/gallery/${id}`);
        return {
            ...response.data,
            coverImageUrl: fixUrl(response.data.coverImageUrl)
        };
    },
    // Used for Portal Gallery (includes membership check)
    getPhotos: async (albumId: string): Promise<AlbumDetailDto> => {
        // Calling GetAlbumDetailsAsync (Standard ABP Route Convention)
        const response = await api.get<AlbumDetailDto>(`/api/app/gallery/${albumId}/album-details`);

        return {
            ...response.data,
            mediaItems: response.data.mediaItems.map(item => ({
                ...item,
                url: fixUrl(item.url)
            }))
        };
    }
};
