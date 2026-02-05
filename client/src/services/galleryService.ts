import { api } from './api';
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

export const galleryService = {
    getAlbums: async (input: any) => {
        const response = await api.get<PagedResultDto<AlbumDto>>('/api/app/gallery/albums', { params: input });
        return response.data;
    },
    // Used for public/admin view
    getAlbum: async (id: string) => {
        const response = await api.get<AlbumDto>(`/api/app/gallery/${id}`);
        return response.data;
    },
    // Used for Portal Gallery (includes membership check)
    getPhotos: async (albumId: string) => {
        // Calling GetAlbumDetailsAsync (Standard ABP Route Convention)
        const response = await api.get<AlbumDetailDto>(`/api/app/gallery/${albumId}/album-details`);

        // Map to PhotoDto format expected by UI
        return response.data.mediaItems.map(item => ({
            id: item.id,
            url: item.url,
            albumId: albumId,
            caption: ''
        }));
    }
};
