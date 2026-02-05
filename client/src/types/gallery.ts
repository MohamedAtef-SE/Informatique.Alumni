import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from './common';

export interface GalleryAlbumDto extends FullAuditedEntityDto<string> {
    title: string;
    description?: string;
    eventDate: string;
    coverImageUrl?: string;
    photoCount: number;
}

export interface CreateGalleryAlbumDto {
    title: string;
    description?: string;
    eventDate: string;
    coverImage?: File; // For frontend use if uploading directly, or byte[]
}

export interface GalleryFilterDto extends PagedAndSortedResultRequestDto {
    year?: number;
    filter?: string;
}
