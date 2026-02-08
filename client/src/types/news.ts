import type { FullAuditedEntityDto } from './common';

export interface BlogPostDto extends FullAuditedEntityDto<string> {
    title: string;
    summary?: string;
    content: string;
    category?: string;
    authorId: string;
    authorName?: string;
    coverImageUrl?: string;
    tags?: string;
    isPublished: boolean;
    isFeatured: boolean;
    viewCount: number;
}

export interface MagazineIssueDto extends FullAuditedEntityDto<string> {
    title: string;
    description?: string;
    publishDate: string;
    pdfUrl: string;
    thumbnailUrl: string;
}
