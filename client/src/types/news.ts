import type { FullAuditedEntityDto } from './common';

export interface BlogPostDto extends FullAuditedEntityDto<string> {
    title: string;
    content: string;
    category?: string;
    authorId: string;
    coverImageUrl?: string;
    isPublished: boolean;
}

export interface MagazineIssueDto extends FullAuditedEntityDto<string> {
    title: string;
    description?: string;
    publishDate: string;
    pdfUrl: string;
    thumbnailUrl: string;
}
