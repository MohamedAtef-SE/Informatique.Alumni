import type { FullAuditedEntityDto } from './common';

export interface BlogPostDto extends FullAuditedEntityDto<string> {
    title: string;
    slug: string;
    summary?: string;
    content: string;
    categoryId?: string;
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

export interface CreateBlogPostDto {
    title: string;
    slug: string;
    summary?: string;
    content: string;
    coverImageUrl?: string;
    isPublished: boolean;
    tags?: string;
    categoryId?: string;
}

export interface UpdateBlogPostDto extends CreateBlogPostDto { }

export interface ArticleCategoryLookupDto {
    id: string;
    nameAr: string;
    nameEn: string;
}
